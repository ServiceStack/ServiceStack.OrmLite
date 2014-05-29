using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ServiceStack.OrmLite.Oracle
{
    public class OracleOrmLiteDialectProvider : OrmLiteDialectProviderBase<OracleOrmLiteDialectProvider>
    {
        public const string OdpProvider = "Oracle.DataAccess.Client";
        public const string MicrosoftProvider = "System.Data.OracleClient";

        protected readonly List<string> ReservedNames = new List<string>
        {
			"USER", "ORDER", "PASSWORD", "ACTIVE", "LEFT", "DOUBLE", "FLOAT", "DECIMAL", "STRING", "DATE",
            "DATETIME", "TYPE","TIMESTAMP", "COMMENT", "LONG"
		};

        protected readonly List<string> ReservedParameterNames = new List<string>
        {
            "COMMENT", "DATE", "DECIMAL", "FLOAT", "ORDER", "USER", "LONG"
        };

        protected const int MaxNameLength = 30;
        protected const int MaxStringColumnLength = 4000;

        private static OracleOrmLiteDialectProvider _instance;
        public static OracleOrmLiteDialectProvider Instance
        {
            // Constructing extras if we happen to hit this concurrently on separate threads is harmless enough
            get { return _instance ?? (_instance = new OracleOrmLiteDialectProvider()); }
        }

        internal long LastInsertId { get; set; }
        protected bool CompactGuid;

        internal const string StringGuidDefinition = "VARCHAR2(37)";
        internal const string CompactGuidDefinition = "RAW(16)";

        public OracleOrmLiteDialectProvider()
            : this(false, false)
        {
        }

        public OracleOrmLiteDialectProvider(bool compactGuid, bool quoteNames, string clientProvider = OdpProvider)
        {
            ClientProvider = clientProvider;
            CompactGuid = compactGuid;
            QuoteNames = quoteNames;
            BoolColumnDefinition = "NUMBER(1)";
            GuidColumnDefinition = CompactGuid ? CompactGuidDefinition : StringGuidDefinition;
            LongColumnDefinition = "NUMERIC(18)";
            AutoIncrementDefinition = string.Empty;
            DateTimeColumnDefinition = "TIMESTAMP";
            DateTimeOffsetColumnDefinition = "TIMESTAMP WITH TIME ZONE";
            TimeColumnDefinition = LongColumnDefinition;
            RealColumnDefinition = "FLOAT";
            // Order-dependency here: must set following values before setting DefaultStringLength
            StringLengthNonUnicodeColumnDefinitionFormat = "VARCHAR2({0})";
            StringLengthUnicodeColumnDefinitionFormat = "NVARCHAR2({0})";
            MaxStringColumnDefinition = StringLengthNonUnicodeColumnDefinitionFormat.Fmt(MaxStringColumnLength);
            DefaultStringLength = 128;

            InitColumnTypeMap();
            ParamString = ":";

            NamingStrategy = new OracleNamingStrategy(MaxNameLength);
            ExecFilter = new OracleExecFilter();
        }

        public override void OnAfterInitColumnTypeMap()
        {
            base.OnAfterInitColumnTypeMap();

            DbTypeMap.Set<bool>(DbType.Int16, BoolColumnDefinition);
            DbTypeMap.Set<bool?>(DbType.Int16, BoolColumnDefinition);

            DbTypeMap.Set<sbyte>(DbType.Int16, IntColumnDefinition);
            DbTypeMap.Set<sbyte?>(DbType.Int16, IntColumnDefinition);
            DbTypeMap.Set<ushort>(DbType.Int32, IntColumnDefinition);
            DbTypeMap.Set<ushort?>(DbType.Int32, IntColumnDefinition);
            DbTypeMap.Set<uint>(DbType.Int64, LongColumnDefinition);
            DbTypeMap.Set<uint?>(DbType.Int64, LongColumnDefinition);
            DbTypeMap.Set<ulong>(DbType.Int64, LongColumnDefinition);
            DbTypeMap.Set<ulong?>(DbType.Int64, LongColumnDefinition);

            if (CompactGuid)
            {
                DbTypeMap.Set<Guid>(DbType.Binary, GuidColumnDefinition);
                DbTypeMap.Set<Guid?>(DbType.Binary, GuidColumnDefinition);
            }
            else
            {
                DbTypeMap.Set<Guid>(DbType.String, GuidColumnDefinition);
                DbTypeMap.Set<Guid?>(DbType.String, GuidColumnDefinition);
            }

            DbTypeMap.Set<DateTimeOffset>(DbType.String, DateTimeOffsetColumnDefinition);
            DbTypeMap.Set<DateTimeOffset?>(DbType.String, DateTimeOffsetColumnDefinition);
        }

        protected string ClientProvider = OdpProvider;

        public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
        {
            if (options != null)
            {
                connectionString = options.Aggregate(connectionString, (current, option) => current + (option.Key + "=" + option.Value + ";"));
            }

            var factory = DbProviderFactories.GetFactory(ClientProvider);
            InitializeOracleTimestampSetting(factory);
            IDbConnection connection = factory.CreateConnection();
            if (connection != null) connection.ConnectionString = connectionString;
            return connection;
        }

        public override long GetLastInsertId(IDbCommand dbCmd)
        {
            return LastInsertId;
        }

        public override long InsertAndGetLastInsertId<T>(IDbCommand dbCmd)
        {
            dbCmd.ExecuteScalar();
            return GetLastInsertId(dbCmd);
        }

        public override void SetDbValue(FieldDefinition fieldDef, IDataReader reader, int colIndex, object instance)
        {
            if (HandledDbNullValue(fieldDef, reader, colIndex, instance)) return;

            object convertedValue;
            if (fieldDef.FieldType == typeof(DateTimeOffset))
            {
                SetOracleTimestampTzFormat();
                convertedValue = ConvertTimestampTzToDateTimeOffset(reader, colIndex);
            }
            else
            {
                var value = reader.GetValue(colIndex);
                convertedValue = ConvertDbValue(value, fieldDef.FieldType);
            }
            try
            {
                fieldDef.SetValueFn(instance, convertedValue);
            }
            catch (NullReferenceException ex)
            {
                Log.Warn("Unexpected NullReferenceException", ex);
            }
        }

        public override object ConvertDbValue(object value, Type type)
        {
            if (value == null || value is DBNull) return null;

            if (type == typeof(bool))
            {
                var intVal = int.Parse(value.ToString());
                return intVal != 0;
            }

            if (type == typeof(Double))
                return double.Parse(value.ToString());

            if (type == typeof(float))
                return float.Parse(value.ToString());

            if (type == typeof(Guid))
            {
                if (CompactGuid)
                {
                    var raw = (byte[])value;
                    return new Guid(raw);
                }
                return new Guid(value.ToString());
            }

            if (type == typeof(byte[]))
                return value;

            if (type == typeof(TimeSpan))
            {
                var ticks = long.Parse(value.ToString());
                return TimeSpan.FromTicks(ticks);
            }

            return base.ConvertDbValue(value, type);
        }

        public override string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

            if (fieldType == typeof(Guid))
            {
                var guid = (Guid)value;
                return CompactGuid ? "'" + BitConverter.ToString(guid.ToByteArray()).Replace("-", "") + "'"
                                   : string.Format("CAST('{0}' AS {1})", guid, StringGuidDefinition);
            }

            if (fieldType == typeof(DateTime) || fieldType == typeof(DateTime?))
            {
                var dateValue = (DateTime)value;
                string iso8601Format = "yyyy-MM-dd";
                string oracleFormat = "YYYY-MM-DD";
                if (dateValue.ToString("yyyy-MM-dd HH:mm:ss.fff").EndsWith("00:00:00.000") == false)
                {
                    iso8601Format = "yyyy-MM-dd HH:mm:ss.fff";
                    oracleFormat = "YYYY-MM-DD HH24:MI:SS.FF3";
                }
                return "TO_TIMESTAMP(" + base.GetQuotedValue(dateValue.ToString(iso8601Format), typeof(string)) + ", " + base.GetQuotedValue(oracleFormat, typeof(string)) + ")";
            }

            if ((value is TimeSpan) && (fieldType == typeof(Int64) || fieldType == typeof(Int64?)))
            {
                var longValue = ((TimeSpan)value).Ticks;
                return base.GetQuotedValue(longValue, fieldType);
            }

            if (fieldType == typeof(bool?) || fieldType == typeof(bool))
            {
                var boolValue = (bool)value;
                return base.GetQuotedValue(boolValue ? "1" : "0", typeof(string));
            }

            if (fieldType == typeof(decimal?) || fieldType == typeof(decimal) ||
                fieldType == typeof(double?) || fieldType == typeof(double) ||
                fieldType == typeof(float?) || fieldType == typeof(float))
            {
                var s = base.GetQuotedValue(value, fieldType);
                if (s.Length > 20) s = s.Substring(0, 20);
                return "'" + s + "'"; // when quoted exception is more clear!
            }

            return base.GetQuotedValue(value, fieldType);
        }

        public override string ToSelectStatement(Type tableType, string sqlFilter, params object[] filterParams)
        {
            var sql = new StringBuilder();
            const string selectStatement = "SELECT ";
            var modelDef = GetModel(tableType);
            var isFullSelectStatement =
                !string.IsNullOrEmpty(sqlFilter)
                && sqlFilter.Trim().Length > selectStatement.Length
                && sqlFilter.Trim().Substring(0, selectStatement.Length).ToUpper().Equals(selectStatement);

            if (isFullSelectStatement)
            {
                if (Regex.Matches(sqlFilter.Trim().ToUpperInvariant(), @"(\b|\n)FROM(\b|\n)").Count < 1)
                    sqlFilter += " FROM DUAL";
                return sqlFilter.SqlFmt(filterParams);
            }

            sql.AppendFormat("SELECT {0} FROM {1}",
                             GetColumnNames(modelDef),
                             GetQuotedTableName(modelDef));
            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFmt(filterParams);
                if (!sqlFilter.StartsWith("\nORDER ", StringComparison.OrdinalIgnoreCase)
                    && !sqlFilter.StartsWith("\nROWS ", StringComparison.OrdinalIgnoreCase)) // ROWS <m> [TO <n>])
                {
                    sql.Append("\nWHERE ");
                }
                sql.Append(sqlFilter);
            }
            return sql.ToString();
        }

        public override void PrepareParameterizedInsertStatement<T>(IDbCommand dbCommand, ICollection<string> insertFields = null)
        {
            if (insertFields == null)
                insertFields = new List<string>();

            var sbColumnNames = new StringBuilder();
            var sbColumnValues = new StringBuilder();
            var modelDef = GetModel(typeof(T));

            dbCommand.Parameters.Clear();
            dbCommand.CommandTimeout = OrmLiteConfig.CommandTimeout;
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.IsComputed) continue;

                //insertFields contains Property "Name" of fields to insert (that's how expressions work)
                if (insertFields.Count > 0 && !insertFields.Contains(fieldDef.Name)) continue;

                if (sbColumnNames.Length > 0) sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(GetQuotedColumnName(fieldDef.FieldName));
                    sbColumnValues.Append(this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName)));

                    AddParameter(dbCommand, fieldDef);
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in CreateParameterizedInsertStatement(): " + ex.Message, ex);
                    throw;
                }
            }

            dbCommand.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                GetQuotedTableName(modelDef), sbColumnNames, sbColumnValues);
        }

        public override void SetParameterValues<T>(IDbCommand dbCmd, object obj)
        {
            var modelDef = GetModel(typeof(T));
            var fieldMap = modelDef.GetFieldDefinitionMap(SanitizeFieldNameForParamName);

            foreach (IDataParameter p in dbCmd.Parameters)
            {
                FieldDefinition fieldDef;
                var fieldName = this.ToFieldName(p.ParameterName);
                fieldMap.TryGetValue(fieldName, out fieldDef);

                if (fieldDef == null)
                    throw new ArgumentException("Field Definition '{0}' was not found".Fmt(fieldName));

                if (fieldDef.AutoIncrement || !string.IsNullOrEmpty(fieldDef.Sequence))
                {
                    if (fieldDef.AutoIncrement && string.IsNullOrEmpty(fieldDef.Sequence))
                    {
                        fieldDef.Sequence = Sequence(GetTableName(modelDef), fieldDef.FieldName, fieldDef.Sequence);
                    }

                    var pi = typeof(T).GetProperty(fieldDef.Name,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                    //TODO fix this hack! Because of the way we handle sequences we have to know whether
                    // this is an insert or update/delete. If we did sequences with triggers this would
                    // not be a problem.
                    var sql = dbCmd.CommandText.TrimStart().ToUpperInvariant();
                    bool isInsert = sql.StartsWith("INSERT", StringComparison.InvariantCulture);

                    var result = GetNextValue(dbCmd.Connection, dbCmd.Transaction, fieldDef.Sequence,
                                              pi.GetValue(obj, new object[] { }), isInsert);
                    if (pi.PropertyType == typeof(String))
                        pi.SetProperty(obj, result.ToString());
                    else if (pi.PropertyType == typeof(Int16))
                        pi.SetProperty(obj, Convert.ToInt16(result));
                    else if (pi.PropertyType == typeof(Int32))
                        pi.SetProperty(obj, Convert.ToInt32(result));
                    else if (pi.PropertyType == typeof(Guid))
                        pi.SetProperty(obj, result);
                    else
                        pi.SetProperty(obj, Convert.ToInt64(result));
                }

                SetParameterValue<T>(fieldDef, p, obj);
            }
        }

        public override void SetParameterValue<T>(FieldDefinition fieldDef, IDataParameter p, object obj)
        {
            var value = GetValueOrDbNull<T>(fieldDef, obj);

            if (fieldDef.ColumnType == typeof(DateTimeOffset) || fieldDef.ColumnType == typeof(DateTimeOffset?))
            {
                SetOracleParameterTypeTimestampTz(p);
            }
            p.Value = value;
        }

        protected override object GetValue<T>(FieldDefinition fieldDef, object obj)
        {
            var value = base.GetValue<T>(fieldDef, obj);

            if (value != null)
            {
                if (fieldDef.FieldType == typeof(Guid))
                {
                    var guid = (Guid)value;
                    if (CompactGuid) return guid.ToByteArray();
                    return guid.ToString();
                }
                if (fieldDef.FieldType == typeof(DateTimeOffset))
                {
                    SetOracleTimestampTzFormat();
                    var timestamp = (DateTimeOffset)value;
                    return timestamp.ToString(DateTimeOffsetOutputFormat, CultureInfo.InvariantCulture);
                }
            }
            return value;
        }

        private string DateTimeOffsetOutputFormat { get; set; }
        private string DateTimeOffsetInputFormat { get; set; }
        private string TimestampTzFormat { get; set; }
        private readonly Dictionary<int, bool> _threadFormatSet = new Dictionary<int, bool>();
        private const BindingFlags InvokeStaticPublic = BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod;
        private Assembly OracleAssembly { get; set; }
        private object ClientInfo { get; set; }
        private MethodInfo SetThreadInfo { get; set; }
        private MethodInfo SetOracleDbType { get; set; }
        private object TimestampTz { get; set; }
        private MethodInfo GetOracleValue { get; set; }

        private void InitializeOracleTimestampSetting(DbProviderFactory factory)
        {
            OracleAssembly = factory.GetType().Assembly;
            var globalizationType = OracleAssembly.GetType("Oracle.DataAccess.Client.OracleGlobalization");
            if (globalizationType != null)
            {
                DateTimeOffsetInputFormat = DateTimeOffsetOutputFormat = "yyyy-MM-dd HH:mm:ss.ffffff zzz";
                TimestampTzFormat = "YYYY-MM-DD HH24:MI:SS.FF6 TZH:TZM";

                ClientInfo = globalizationType.InvokeMember("GetClientInfo", InvokeStaticPublic, null, null, null);
                const BindingFlags setProperty = BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance;
                globalizationType.InvokeMember("TimeStampTZFormat", setProperty, null, ClientInfo, new object[] { TimestampTzFormat });
                SetThreadInfo = globalizationType.GetMethod("SetThreadInfo", BindingFlags.Public | BindingFlags.Static);

                var parameterType = OracleAssembly.GetType("Oracle.DataAccess.Client.OracleParameter");
                var oracleDbTypeProperty = parameterType.GetProperty("OracleDbType", BindingFlags.Public | BindingFlags.Instance);
                SetOracleDbType = oracleDbTypeProperty.GetSetMethod();

                var oracleDbType = OracleAssembly.GetType("Oracle.DataAccess.Client.OracleDbType");
                TimestampTz = Enum.Parse(oracleDbType, "TimeStampTZ");

                var readerType = OracleAssembly.GetType("Oracle.DataAccess.Client.OracleDataReader");
                GetOracleValue = readerType.GetMethod("GetOracleValue", BindingFlags.Public | BindingFlags.Instance);
            }
            else
            {
                //TODO This is Microsoft provider support and it does not handle the offsets correctly,
                // but I don't know how to make it work.

                DateTimeOffsetOutputFormat = "dd-MMM-yy hh:mm:ss.fff tt";
                DateTimeOffsetInputFormat = "dd-MMM-yy hh:mm:ss tt";
                TimestampTzFormat = "DD-MON-RR HH.MI.SSXFF AM";

                //                var parameterType = OracleAssembly.GetType("System.Data.OracleClient.OracleParameter");
                //                var oracleTypeProperty = parameterType.GetProperty("OracleType", BindingFlags.Public | BindingFlags.Instance);
                //                SetOracleDbType = oracleTypeProperty.GetSetMethod();

                var oracleDbType = OracleAssembly.GetType("System.Data.OracleClient.OracleType");
                TimestampTz = Enum.Parse(oracleDbType, "TimestampWithTZ");

                //                var readerType = OracleAssembly.GetType("System.Data.OracleClient.OracleDataReader");
                //                GetOracleValue = readerType.GetMethod("GetOracleValue", BindingFlags.Public | BindingFlags.Instance);
            }
        }

        private void SetOracleTimestampTzFormat()
        {
            if (ClientInfo == null) return;

            var threadId = Thread.CurrentThread.ManagedThreadId;
            if (_threadFormatSet.ContainsKey(threadId)) return;

            SetThreadInfo.Invoke(null, new[] { ClientInfo });
            _threadFormatSet[threadId] = true;
        }

        private void SetOracleParameterTypeTimestampTz(IDataParameter p)
        {
            if (SetOracleDbType != null) SetOracleDbType.Invoke(p, new[] { TimestampTz });
        }

        private DateTimeOffset ConvertTimestampTzToDateTimeOffset(IDataReader dataReader, int colIndex)
        {
            if (GetOracleValue != null)
            {
                var value = GetOracleValue.Invoke(dataReader, new object[] { colIndex }).ToString();
                return DateTimeOffset.ParseExact(value, DateTimeOffsetInputFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                var value = dataReader.GetValue(colIndex);
                return new DateTimeOffset((DateTime)value);
            }
        }

        public override string ToInsertRowStatement(IDbCommand dbCommand, object objWithProperties, ICollection<string> insertFields = null)
        {
            if (insertFields == null)
                insertFields = new List<string>();

            var sbColumnNames = new StringBuilder();
            var sbColumnValues = new StringBuilder();

            var tableType = objWithProperties.GetType();
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.IsComputed) continue;
                if (insertFields.Count > 0 && !insertFields.Contains(fieldDef.Name)) continue;

                if ((fieldDef.AutoIncrement || !string.IsNullOrEmpty(fieldDef.Sequence))
                    && dbCommand != null)
                {
                    if (fieldDef.AutoIncrement && string.IsNullOrEmpty(fieldDef.Sequence))
                    {
                        fieldDef.Sequence = Sequence(GetTableName(modelDef), fieldDef.FieldName, fieldDef.Sequence);
                    }

                    var pi = tableType.GetProperty(fieldDef.Name,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                    var result = GetNextValue(dbCommand.Connection, dbCommand.Transaction, fieldDef.Sequence,
                                              pi.GetValue(objWithProperties, new object[] { }), isInsert: true);
                    if (pi.PropertyType == typeof(String))
                        pi.SetProperty(objWithProperties, result.ToString());
                    else if (pi.PropertyType == typeof(Int16))
                        pi.SetProperty(objWithProperties, Convert.ToInt16(result));
                    else if (pi.PropertyType == typeof(Int32))
                        pi.SetProperty(objWithProperties, Convert.ToInt32(result));
                    else if (pi.PropertyType == typeof(Guid))
                        pi.SetProperty(objWithProperties, result);
                    else
                        pi.SetProperty(objWithProperties, Convert.ToInt64(result));
                }

                if (sbColumnNames.Length > 0) sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(string.Format("{0}", GetQuotedColumnName(fieldDef.FieldName)));
                    if (!string.IsNullOrEmpty(fieldDef.Sequence) && dbCommand == null)
                        sbColumnValues.Append(string.Format(":{0}", fieldDef.Name));
                    else
                        sbColumnValues.Append(fieldDef.GetQuotedValue(objWithProperties));
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Error in ToInsertRowStatement on column {0}: {1}", fieldDef.FieldName, ex);
                    throw;
                }
            }

            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2}) ",
                                    GetQuotedTableName(modelDef), sbColumnNames, sbColumnValues);

            return sql;
        }


        public override string ToUpdateRowStatement(object objWithProperties, ICollection<string> updateFields = null)
        {
            var sqlFilter = new StringBuilder();
            var sql = new StringBuilder();
            var tableType = objWithProperties.GetType();
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.IsComputed) continue;

                var updateFieldsEmptyOrNull = updateFields == null || updateFields.Count == 0;
                if ((fieldDef.IsPrimaryKey || fieldDef.Name == OrmLiteConfig.IdField)
                    && updateFieldsEmptyOrNull)
                {
                    if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");

                    sqlFilter.AppendFormat("{0} = {1}",
                        GetQuotedColumnName(fieldDef.FieldName),
                        fieldDef.GetQuotedValue(objWithProperties));

                    continue;
                }
                if (!updateFieldsEmptyOrNull && !updateFields.Contains(fieldDef.Name)) continue;
                if (sql.Length > 0) sql.Append(",");
                sql.AppendFormat("{0}={1}",
                    GetQuotedColumnName(fieldDef.FieldName),
                    fieldDef.GetQuotedValue(objWithProperties));
            }

            var updateSql = string.Format("UPDATE {0} \nSET {1} {2}",
                GetQuotedTableName(modelDef), sql, (sqlFilter.Length > 0 ? "\nWHERE " + sqlFilter : ""));

            return updateSql;
        }


        public override string ToDeleteRowStatement(object objWithProperties)
        {
            var tableType = objWithProperties.GetType();
            var modelDef = GetModel(tableType);

            var sqlFilter = new StringBuilder();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.IsPrimaryKey || fieldDef.Name == OrmLiteConfig.IdField)
                {
                    if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");
                    sqlFilter.AppendFormat("{0} = {1}",
                        GetQuotedColumnName(fieldDef.FieldName),
                        fieldDef.GetQuotedValue(objWithProperties));
                }
            }

            var deleteSql = string.Format("DELETE FROM {0} WHERE {1}",
                GetQuotedTableName(modelDef), sqlFilter);

            return deleteSql;
        }

        public override string ToCreateTableStatement(Type tableType)
        {
            var sbColumns = new StringBuilder();
            var sbConstraints = new StringBuilder();
            var sbPk = new StringBuilder();

            var modelDef = GetModel(tableType);
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.IsPrimaryKey)
                {
                    sbPk.AppendFormat(sbPk.Length != 0 ? ",{0}" : "{0}", GetQuotedColumnName(fieldDef.FieldName));
                }

                if (sbColumns.Length != 0) sbColumns.Append(", \n  ");

                var columnDefinition = GetColumnDefinition(
                    fieldDef.FieldName,
                    fieldDef.ColumnType,
                    fieldDef.IsPrimaryKey,
                    fieldDef.IsRowVersion,
                    fieldDef.AutoIncrement,
                    fieldDef.IsNullable,
                    fieldDef.FieldLength,
                    fieldDef.Scale,
                    fieldDef.DefaultValue,
                    fieldDef.CustomFieldDefinition);

                sbColumns.Append(columnDefinition);

                if (fieldDef.ForeignKey == null) continue;

                var refModelDef = GetModel(fieldDef.ForeignKey.ReferenceType);
                sbConstraints.AppendFormat(
                    ", \n\n  CONSTRAINT {0} FOREIGN KEY ({1}) REFERENCES {2} ({3})",
                    GetQuotedName(fieldDef.ForeignKey.GetForeignKeyName(modelDef, refModelDef, NamingStrategy, fieldDef)),
                    GetQuotedColumnName(fieldDef.FieldName),
                    GetQuotedTableName(refModelDef),
                    GetQuotedColumnName(refModelDef.PrimaryKey.FieldName));

                sbConstraints.Append(GetForeignKeyOnDeleteClause(fieldDef.ForeignKey));
            }

            if (sbPk.Length != 0) sbColumns.AppendFormat(", \n  PRIMARY KEY({0})", sbPk);

            var sql = new StringBuilder(string.Format(
                "CREATE TABLE {0} \n(\n  {1}{2} \n) \n", GetQuotedTableName(modelDef), sbColumns, sbConstraints));

            return sql.ToString();
        }

        public override string GetForeignKeyOnDeleteClause(ForeignKeyConstraint foreignKey)
        {
            if (string.IsNullOrEmpty(foreignKey.OnDelete)) return string.Empty;
            var onDelete = foreignKey.OnDelete.ToUpperInvariant();
            return (onDelete == "SET NULL" || onDelete == "CASCADE") ? " ON DELETE " + onDelete : string.Empty;
        }

        public override string ToCreateSequenceStatement(Type tableType, string sequenceName)
        {
            var result = "";
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.AutoIncrement || !fieldDef.Sequence.IsNullOrEmpty())
                {
                    var seqName = Sequence(GetTableName(modelDef), fieldDef.FieldName, fieldDef.Sequence);
                    if (seqName.EqualsIgnoreCase(sequenceName))
                    {
                        result = "CREATE SEQUENCE " + GetQuotedName(seqName);
                        break;
                    }
                }
            }
            return result;
        }

        public override List<string> ToCreateSequenceStatements(Type tableType)
        {
            return SequenceList(tableType).Select(seq => "CREATE SEQUENCE " + GetQuotedName(seq)).ToList();
        }

        public override List<string> SequenceList(Type tableType)
        {
            var gens = new List<string>();
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.AutoIncrement || !fieldDef.Sequence.IsNullOrEmpty())
                {
                    var seqName = Sequence(GetTableName(modelDef), fieldDef.FieldName, fieldDef.Sequence);

                    if (gens.IndexOf(seqName) == -1)
                        gens.Add(seqName);
                }
            }
            return gens;
        }

        public override string GetColumnDefinition(string fieldName, Type fieldType,
			bool isPrimaryKey, bool autoIncrement, bool isRowVersion, bool isNullable, 
            int? fieldLength, int? scale, string defaultValue, string customFieldDefinition)
        {
            string fieldDefinition;

            if (isRowVersion)
            {
                isNullable = false;
                defaultValue = null;
                fieldDefinition = GetRowVersionColumnDefinition(fieldType);
            }
            else if (customFieldDefinition != null)
            {
                fieldDefinition = customFieldDefinition;
            }
            else if (fieldType == typeof(string))
            {
                var length = fieldLength.GetValueOrDefault(DefaultStringLength);
                length = Math.Min(length, MaxStringColumnLength);
                fieldDefinition = string.Format(StringLengthColumnDefinitionFormat, length);
            }
            else if (fieldType == typeof(Decimal))
            {
                fieldDefinition = string.Format("{0} ({1},{2})", DecimalColumnDefinition,
                    fieldLength.GetValueOrDefault(DefaultDecimalPrecision),
                    scale.GetValueOrDefault(DefaultDecimalScale));
            }
            else
            {
                fieldDefinition = GetColumnTypeDefinition(fieldType);
            }

            var sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldName), fieldDefinition);

            if (!string.IsNullOrEmpty(defaultValue))
            {
                sql.AppendFormat(DefaultValueFormat, defaultValue);
            }

            sql.Append(isNullable ? " NULL" : " NOT NULL");

            return sql.ToString();
        }


        public override List<string> ToCreateTriggerStatements(Type tableType)
        {
            var triggers = new List<string>();
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (!fieldDef.IsRowVersion) continue;

                const string rowVersionTriggerName = "RowVersionIncrementTrigger";
                string triggerAction = string.Format(":NEW.{0} := :OLD.{0}+1;", GetQuotedColumnName(fieldDef.FieldName));
                var rowVersionUpdateTrigger = string.Format("CREATE TRIGGER {0} BEFORE UPDATE ON {1} FOR EACH ROW BEGIN {2} END;\n",
                                                      rowVersionTriggerName,
                                                      GetQuotedTableName(modelDef),
                                                      triggerAction);
                triggers.Add(rowVersionUpdateTrigger);
            }

            return triggers;
        }

        public override List<string> ToCreateIndexStatements(Type tableType)
        {
            var sqlIndexes = new List<string>();

            var modelDef = GetModel(tableType);
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (!fieldDef.IsIndexed) continue;

                var indexName = GetIndexName(
                    fieldDef.IsUnique,
                    (modelDef.IsInSchema
                        ? modelDef.Schema + "_" + modelDef.ModelName
                        : modelDef.ModelName).SafeVarName(),
                    fieldDef.FieldName);
                indexName = NamingStrategy.ApplyNameRestrictions(indexName);

                sqlIndexes.Add(
                    ToCreateIndexStatement(fieldDef.IsUnique, indexName, modelDef, fieldDef.FieldName, false));
            }

            foreach (var compositeIndex in modelDef.CompositeIndexes)
            {
                var indexName = GetCompositeIndexNameWithSchema(compositeIndex, modelDef);
                indexName = NamingStrategy.ApplyNameRestrictions(indexName);
                var indexNames = string.Join(",", compositeIndex.FieldNames.ToArray());

                sqlIndexes.Add(
                    ToCreateIndexStatement(compositeIndex.Unique, indexName, modelDef, indexNames, false));
            }

            return sqlIndexes;
        }

        protected override string ToCreateIndexStatement(bool isUnique, string indexName, ModelDefinition modelDef, string fieldName,
            bool isCombined = false, FieldDefinition fieldDef = null)
        {
            return string.Format("CREATE {0} INDEX {1} ON {2} ({3} ) \n",
                isUnique ? "UNIQUE" : "",
                indexName,
                GetQuotedTableName(modelDef),
                GetQuotedColumnName(fieldName));
        }


        public override string ToExistStatement(Type fromTableType,
            object objWithProperties,
            string sqlFilter,
            params object[] filterParams)
        {

            var fromModelDef = GetModel(fromTableType);
            var sql = new StringBuilder();
            sql.AppendFormat("SELECT 1 FROM {0}", GetQuotedTableName(fromModelDef));

            var filter = new StringBuilder();

            if (objWithProperties != null)
            {
                var tableType = objWithProperties.GetType();

                if (fromTableType != tableType)
                {
                    var i = 0;
                    var modelDef = GetModel(tableType);

                    var fpk = modelDef.FieldDefinitions.Where(def => def.IsPrimaryKey).ToList();

                    foreach (var fieldDef in fromModelDef.FieldDefinitions)
                    {
                        if (fieldDef.IsComputed) continue;
                        if (fieldDef.ForeignKey != null
                            && GetModel(fieldDef.ForeignKey.ReferenceType).ModelName == modelDef.ModelName)
                        {
                            if (filter.Length > 0) filter.Append(" AND ");
                            filter.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName),
                                fpk[i].GetQuotedValue(objWithProperties));
                            i++;
                        }
                    }
                }
                else
                {
                    var modelDef = GetModel(tableType);
                    foreach (var fieldDef in modelDef.FieldDefinitions)
                    {
                        if (fieldDef.IsComputed) continue;
                        if (fieldDef.IsPrimaryKey)
                        {
                            if (filter.Length > 0) filter.Append(" AND ");
                            filter.AppendFormat("{0} = {1}",
                                GetQuotedColumnName(fieldDef.FieldName),
                                fieldDef.GetQuotedValue(objWithProperties));
                        }
                    }
                }

                if (filter.Length > 0) sql.AppendFormat("\nWHERE {0} ", filter);
            }

            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFmt(filterParams);
                if (!sqlFilter.StartsWith("\nORDER ", StringComparison.OrdinalIgnoreCase)
                    && !sqlFilter.StartsWith("\nROWS ", StringComparison.OrdinalIgnoreCase)) // ROWS <m> [TO <n>])
                {
                    sql.Append(filter.Length > 0 ? " AND  " : "\nWHERE ");
                }
                sql.Append(sqlFilter);
            }

            var sb = new StringBuilder("select 1  from dual where");
            sb.AppendFormat(" exists ({0})", sql);
            return sb.ToString();
        }

        public override string ToSelectFromProcedureStatement(
            object fromObjWithProperties,
            Type outputModelType,
            string sqlFilter,
            params object[] filterParams)
        {

            var sbColumnValues = new StringBuilder();

            Type fromTableType = fromObjWithProperties.GetType();

            var modelDef = GetModel(fromTableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

                sbColumnValues.Append(fieldDef.GetQuotedValue(fromObjWithProperties));
            }

            var sql = new StringBuilder();
            sql.AppendFormat("SELECT {0} FROM  {1} {2}{3}{4}  \n",
                GetColumnNames(GetModel(outputModelType)),
                GetQuotedTableName(modelDef),
                sbColumnValues.Length > 0 ? "(" : "",
                sbColumnValues,
                sbColumnValues.Length > 0 ? ")" : "");

            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFmt(filterParams);
                if (!sqlFilter.StartsWith("\nORDER ", StringComparison.OrdinalIgnoreCase)
                    && !sqlFilter.StartsWith("\nROWS ", StringComparison.OrdinalIgnoreCase)) // ROWS <m> [TO <n>]
                {
                    sql.Append("\nWHERE ");
                }
                sql.Append(sqlFilter);
            }

            return sql.ToString();
        }

        public override string ToExecuteProcedureStatement(object objWithProperties)
        {
            var sbColumnValues = new StringBuilder();

            var tableType = objWithProperties.GetType();
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (sbColumnValues.Length > 0) sbColumnValues.Append(",");
                sbColumnValues.Append(fieldDef.GetQuotedValue(objWithProperties));
            }

            var sql = string.Format("EXECUTE PROCEDURE {0} {1}{2}{3};",
                GetQuotedTableName(modelDef),
                sbColumnValues.Length > 0 ? "(" : "",
                sbColumnValues,
                sbColumnValues.Length > 0 ? ")" : "");

            return sql;
        }

        private object GetNextValue(IDbConnection connection, IDbTransaction transaction, string sequence, object value, bool isInsert)
        {
            if (!isInsert)
            {
                long nv;
                Object retObj;
                if (long.TryParse(value.ToString(), out nv))
                {
                    LastInsertId = nv;
                    retObj = LastInsertId;
                }
                else
                {
                    LastInsertId = 0;
                    retObj = value;
                }
                return retObj;
            }

            using (var dbCmd = connection.CreateCommand())
            {
                dbCmd.Transaction = transaction;
                dbCmd.CommandText = string.Format("SELECT {0}.NEXTVAL FROM dual", Quote(sequence));
                var result = dbCmd.LongScalar();
                LastInsertId = result;
                return result;
            }
        }

        public bool QuoteNames { get; set; }

        private bool WillQuote(string name)
        {
            return QuoteNames || ReservedNames.Contains(name.ToUpper())
                              || name.Contains(" ");
        }

        private string Quote(string name)
        {
            return WillQuote(name) ? string.Format("\"{0}\"", name) : name;
        }

        public override string GetQuotedName(string fieldName)
        {
            return Quote(fieldName);
        }

        public override string GetQuotedTableName(ModelDefinition modelDef)
        {
            return Quote(GetTableName(modelDef));
        }

        public override string GetQuotedTableName(string tableName)
        {
            return Quote(NamingStrategy.GetTableName(tableName));
        }

        private string GetTableName(ModelDefinition modelDef)
        {
            return modelDef.IsInSchema
                       ? NamingStrategy.ApplyNameRestrictions(modelDef.Schema
                            + "_" + NamingStrategy.GetTableName(modelDef.ModelName))
                       : NamingStrategy.GetTableName(modelDef.ModelName);
        }

        public override string GetQuotedColumnName(string fieldName)
        {
            return Quote(NamingStrategy.GetColumnName(fieldName));
        }

        public override string SanitizeFieldNameForParamName(string fieldName)
        {
            var name = (fieldName ?? "").Replace(" ", "");
            if (ReservedParameterNames.Contains(name.ToUpper()))
            {
                name = "P_" + name;
            }
            if (name.Length > MaxNameLength)
            {
                name = name.Substring(0, MaxNameLength);
            }
            return name;
        }

        private string Sequence(string modelName, string fieldName, string sequence)
        {
            //TODO used to return Quote(sequence)
            if (!sequence.IsNullOrEmpty()) return sequence;
            var seqName = NamingStrategy.ApplyNameRestrictions(modelName + "_" + fieldName + "_GEN");
            return seqName;
        }

        public override SqlExpression<T> SqlExpression<T>()
        {
            return new OracleSqlExpression<T>();
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName)
        {
            if (!WillQuote(tableName)) tableName = tableName.ToUpper();

            var sql = "SELECT count(*) FROM USER_TABLES WHERE TABLE_NAME = {0}".SqlFmt(tableName);

            dbCmd.CommandText = sql;
            var result = dbCmd.LongScalar();

            return result > 0;
        }

        public override bool DoesSequenceExist(IDbCommand dbCmd, string sequenceName)
        {
            if (!WillQuote(sequenceName)) sequenceName = sequenceName.ToUpper();

            var sql = "SELECT count(*) FROM USER_SEQUENCES WHERE SEQUENCE_NAME = {0}".SqlFmt(sequenceName);
            dbCmd.CommandText = sql;
            var result = dbCmd.LongScalar();
            return result > 0;
        }

        public override string ToAddForeignKeyStatement<T, TForeign>(Expression<Func<T, object>> field,
                                                                    Expression<Func<TForeign, object>> foreignField,
                                                                    OnFkOption onUpdate,
                                                                    OnFkOption onDelete,
                                                                    string foreignKeyName = null)
        {
            var sourceMd = ModelDefinition<T>.Definition;
            var fieldName = sourceMd.GetFieldDefinition(field).FieldName;

            var referenceMd = ModelDefinition<TForeign>.Definition;
            var referenceFieldName = referenceMd.GetFieldDefinition(foreignField).FieldName;

            var name = GetQuotedName(foreignKeyName.IsNullOrEmpty()
                                     ? "fk_" + sourceMd.ModelName + "_" + fieldName + "_" + referenceFieldName
                                     : foreignKeyName);

            return string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}",
                                 GetQuotedTableName(sourceMd.ModelName),
                                 name,
                                 GetQuotedColumnName(fieldName),
                                 GetQuotedTableName(referenceMd.ModelName),
                                 GetQuotedColumnName(referenceFieldName),
                                 GetForeignKeyOnDeleteClause(new ForeignKeyConstraint(typeof(T), FkOptionToString(onDelete))));
        }

        public override string EscapeWildcards(string value)
        {
            if (value == null)
                return null;

            return value
                .Replace("^", @"^^")
                .Replace("_", @"^_")
                .Replace("%", @"^%");
        }
    }
}
