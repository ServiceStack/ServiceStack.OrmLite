using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ServiceStack.OrmLite.Converters;
using ServiceStack.OrmLite.Oracle.Converters;

namespace ServiceStack.OrmLite.Oracle
{
    public class OracleOrmLiteDialectProvider : OrmLiteDialectProviderBase<OracleOrmLiteDialectProvider>
    {
        public const string OdpProvider = "Oracle.DataAccess.Client";
        public const string MicrosoftProvider = "System.Data.OracleClient";

        protected readonly List<string> ReservedNames = new List<string>
        {
            "ACCESS", "DEFAULT", "INTEGER", "ONLINE", "START", "ADD", "DELETE", "INTERSECT", "OPTION", "SUCCESSFUL", "ALL", "DESC",
            "INTO", "OR", "SYNONYM", "ALTER", "DISTINCT", "IS", "ORDER", "SYSDATE", "AND", "DROP", "LEVEL", "PCTFREE", "TABLE", "ANY",
            "ELSE", "LIKE", "PRIOR", "THEN", "AS", "EXCLUSIVE", "LOCK", "PRIVILEGES", "TO", "ASC", "EXISTS", "LONG", "PUBLIC", "TRIGGER",
            "AUDIT", "FILE", "MAXEXTENTS", "RAW", "UID", "BETWEEN", "FLOAT", "MINUS", "RENAME", "UNION", "BY", "FOR", "MLSLABEL", "RESOURCE",
            "UNIQUE", "CHAR", "FROM", "MODE", "REVOKE", "UPDATE", "CHECK", "GRANT", "MODIFY", "ROW", "USER", "CLUSTER", "GROUP", "NOAUDIT",
            "ROWID", "VALIDATE", "COLUMN", "HAVING", "NOCOMPRESS", "ROWNUM", "VALUES", "COMMENT", "IDENTIFIED", "NOT", "ROWS", "VARCHAR",
            "COMPRESS", "IMMEDIATE", "NOWAIT", "SELECT", "VARCHAR2", "CONNECT", "IN", "NULL", "SESSION", "VIEW", "CREATE", "INCREMENT",
            "NUMBER", "SET", "WHENEVER", "CURRENT", "INDEX", "OF", "SHARE", "WHERE", "DATE", "INITIAL", "OFFLINE", "SIZE", "WITH", "DECIMAL",
            "INSERT", "ON", "SMALLINT", "PASSWORD", "ACTIVE", "LEFT", "DOUBLE", "STRING", "DATETIME", "TYPE", "TIMESTAMP",
            "BYTE", "SHORT", "INT", "SUBTYPE"
        };

        protected readonly List<string> ReservedParameterNames = new List<string>
        {
            "ACCESS", "DEFAULT", "INTEGER", "ONLINE", "START", "ADD", "DELETE", "INTERSECT", "OPTION", "SUCCESSFUL", "ALL", "DESC",
            "INTO", "OR", "SYNONYM", "ALTER", "DISTINCT", "IS", "ORDER", "SYSDATE", "AND", "DROP", "LEVEL", "PCTFREE", "TABLE", "ANY",
            "ELSE", "LIKE", "PRIOR", "THEN", "AS", "EXCLUSIVE", "LOCK", "PRIVILEGES", "TO", "ASC", "EXISTS", "LONG", "PUBLIC", "TRIGGER",
            "AUDIT", "FILE", "MAXEXTENTS", "RAW", "UID", "BETWEEN", "FLOAT", "MINUS", "RENAME", "UNION", "BY", "FOR", "MLSLABEL", "RESOURCE",
            "UNIQUE", "CHAR", "FROM", "MODE", "REVOKE", "UPDATE", "CHECK", "GRANT", "MODIFY", "ROW", "USER", "CLUSTER", "GROUP", "NOAUDIT",
            "ROWID", "VALIDATE", "COLUMN", "HAVING", "NOCOMPRESS", "ROWNUM", "VALUES", "COMMENT", "IDENTIFIED", "NOT", "ROWS", "VARCHAR",
            "COMPRESS", "IMMEDIATE", "NOWAIT", "SELECT", "VARCHAR2", "CONNECT", "IN", "NULL", "SESSION", "VIEW", "CREATE", "INCREMENT",
            "NUMBER", "SET", "WHENEVER", "CURRENT", "INDEX", "OF", "SHARE", "WHERE", "DATE", "INITIAL", "OFFLINE", "SIZE", "WITH", "DECIMAL",
            "INSERT", "ON", "SMALLINT",
            "BYTE", "SHORT", "INT", "SUBTYPE"
        };

        protected const int MaxNameLength = 30;
        protected const int MaxStringColumnLength = 4000;

        private static OracleOrmLiteDialectProvider _instance;
        public static OracleOrmLiteDialectProvider Instance
        {
            // Constructing extras if we happen to hit this concurrently on separate threads is harmless enough
            get { return _instance ?? (_instance = new OracleOrmLiteDialectProvider()); }
        }

        //protected bool CompactGuid;

        //internal const string StringGuidDefinition = "VARCHAR2(37)";
        //internal const string CompactGuidDefinition = "RAW(16)";

        private readonly DbProviderFactory _factory;
        private readonly OracleTimestampConverter _timestampConverter;

        public OracleOrmLiteDialectProvider()
            : this(false, false)
        {
        }

        public OracleOrmLiteDialectProvider(bool compactGuid, bool quoteNames, string clientProvider = OdpProvider)
        {
            ClientProvider = clientProvider;
            //CompactGuid = compactGuid;
            QuoteNames = quoteNames;
            //BoolColumnDefinition = "NUMBER(1)";
            //GuidColumnDefinition = CompactGuid ? CompactGuidDefinition : StringGuidDefinition;
            //LongColumnDefinition = "NUMERIC(18)";
            AutoIncrementDefinition = string.Empty;
            //DateTimeColumnDefinition = "TIMESTAMP";
            //DateTimeOffsetColumnDefinition = "TIMESTAMP WITH TIME ZONE";
            //TimeColumnDefinition = LongColumnDefinition;
            //RealColumnDefinition = "FLOAT";
            // Order-dependency here: must set following values before setting DefaultStringLength
            //StringLengthNonUnicodeColumnDefinitionFormat = "VARCHAR2({0})";
            //StringLengthUnicodeColumnDefinitionFormat = "NVARCHAR2({0})";
            //MaxStringColumnDefinition = StringLengthNonUnicodeColumnDefinitionFormat.Fmt(MaxStringColumnLength);
            //DefaultStringLength = 128;

            ParamString = ":";
//            OrmLiteConfig.UseParameterizeSqlExpressions = true;

            NamingStrategy = new OracleNamingStrategy(MaxNameLength);
            ExecFilter = new OracleExecFilter();

            _factory = DbProviderFactories.GetFactory(ClientProvider);
            _timestampConverter = new OracleTimestampConverter(_factory.GetType());

            InitColumnTypeMap();

            //Special Converters if you need to override default behavior
            base.EnumConverter = new OracleEnumConverter();

            if (compactGuid)
                RegisterConverter<Guid>(new OracleCompactGuidConverter());
            else
                RegisterConverter<Guid>(new OracleGuidConverter());

            RegisterConverter<TimeSpan>(new OracleTimeSpanAsIntConverter());
            RegisterConverter<string>(new OracleStringConverter());
            RegisterConverter<char[]>(new OracleCharArrayConverter());
            RegisterConverter<byte[]>(new OracleByteArrayConverter());

            RegisterConverter<long>(new OracleInt64Converter());
            RegisterConverter<sbyte>(new OracleSByteConverter());
            RegisterConverter<ushort>(new OracleUInt16Converter());
            RegisterConverter<uint>(new OracleUInt32Converter());
            RegisterConverter<ulong>(new OracleUInt64Converter());

            RegisterConverter<float>(new OracleFloatConverter());
            RegisterConverter<double>(new OracleDoubleConverter());
            RegisterConverter<decimal>(new OracleDecimalConverter());

            RegisterConverter<DateTime>(new OracleDateTimeConverter());
            RegisterConverter<DateTimeOffset>(new OracleDateTimeOffsetConverter(_timestampConverter));
            RegisterConverter<bool>(new OracleBoolConverter());

            this.Variables = new Dictionary<string, string>
            {
                { OrmLiteVariables.SystemUtc, "sys_extract_utc(systimestamp)" },
            };
        }

        //public override void OnAfterInitColumnTypeMap()
        //{
        //    base.OnAfterInitColumnTypeMap();

        //    DbTypeMap.Set<bool>(DbType.Int16, BoolColumnDefinition);
        //    DbTypeMap.Set<bool?>(DbType.Int16, BoolColumnDefinition);

        //    DbTypeMap.Set<sbyte>(DbType.Int16, IntColumnDefinition);
        //    DbTypeMap.Set<sbyte?>(DbType.Int16, IntColumnDefinition);
        //    DbTypeMap.Set<ushort>(DbType.Int32, IntColumnDefinition);
        //    DbTypeMap.Set<ushort?>(DbType.Int32, IntColumnDefinition);
        //    DbTypeMap.Set<uint>(DbType.Int64, LongColumnDefinition);
        //    DbTypeMap.Set<uint?>(DbType.Int64, LongColumnDefinition);
        //    DbTypeMap.Set<ulong>(DbType.Int64, LongColumnDefinition);
        //    DbTypeMap.Set<ulong?>(DbType.Int64, LongColumnDefinition);

        //    if (CompactGuid)
        //    {
        //        DbTypeMap.Set<Guid>(DbType.Binary, GuidColumnDefinition);
        //        DbTypeMap.Set<Guid?>(DbType.Binary, GuidColumnDefinition);
        //    }
        //    else
        //    {
        //        DbTypeMap.Set<Guid>(DbType.String, GuidColumnDefinition);
        //        DbTypeMap.Set<Guid?>(DbType.String, GuidColumnDefinition);
        //    }

        //    DbTypeMap.Set<DateTimeOffset>(DbType.String, DateTimeOffsetColumnDefinition);
        //    DbTypeMap.Set<DateTimeOffset?>(DbType.String, DateTimeOffsetColumnDefinition);
        //}

        protected string ClientProvider = OdpProvider;
        public static string RowVersionTriggerFormat = "{0}RowVersionUpdateTrigger";

        public override string ToPostCreateTableStatement(ModelDefinition modelDef)
        {
            if (modelDef.RowVersion != null)
            {
                var triggerName = NamingStrategy.ApplyNameRestrictions(
                    RowVersionTriggerFormat.Fmt(modelDef.ModelName));
                var triggerBody = ":NEW.{0} := :OLD.{0}+1;".Fmt(
                    modelDef.RowVersion.FieldName.SqlColumn(this));

                var sql = "CREATE TRIGGER {0} BEFORE UPDATE ON {1} FOR EACH ROW BEGIN {2} END;".Fmt(
                    Quote(triggerName), NamingStrategy.GetTableName(modelDef), triggerBody);

                return sql;
            }

            return null;
        }


        public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
        {
            if (options != null)
            {
                connectionString = options.Aggregate(connectionString, (current, option) => current + (option.Key + "=" + option.Value + ";"));
            }

            var connection = _factory.CreateConnection();
            if (connection != null) connection.ConnectionString = connectionString;
            return new OracleConnection(connection);
        }

        public override long GetLastInsertId(IDbCommand dbCmd)
        {
            throw new NotSupportedException();
        }

        public override long InsertAndGetLastInsertId<T>(IDbCommand dbCmd)
        {
            dbCmd.ExecuteScalar();

            var modelDef = GetModel(typeof(T));

            var primaryKey = modelDef.PrimaryKey;
            if (primaryKey == null)
                return 0;

            var identityParameter = (DbParameter)dbCmd.Parameters[this.GetParam(SanitizeFieldNameForParamName(primaryKey.FieldName))];
            if (identityParameter == null)
                return 0;

            return Convert.ToInt64(identityParameter.Value);
        }

        //Moved to converters.FromDbValue()
        //public override void SetDbValue(FieldDefinition fieldDef, IDataReader reader, int colIndex, object instance)
        //{
        //    if (OrmLiteUtils.HandledDbNullValue(fieldDef, reader, colIndex, instance)) return;

        //    var value = reader.GetValue(colIndex);
        //    var convertedValue = ConvertDbValue(value, fieldDef.FieldType);

        //    try
        //    {
        //        fieldDef.SetValueFn(instance, convertedValue);
        //    }
        //    catch (NullReferenceException ex)
        //    {
        //        Log.Warn("Unexpected NullReferenceException", ex);
        //    }
        //}

        //Moved to converters.FromDbValue()
        //public override object ConvertDbValue(object value, Type type)
        //{
        //    if (value == null || value is DBNull) return null;

        //    if (type == typeof (DateTimeOffset))
        //    {
        //        return Convert.ChangeType(value, type);
        //    }

        //    if (type == typeof(bool))
        //        return Convert.ToBoolean(value);

        //    if (type == typeof(Guid))
        //    {
        //        if (CompactGuid)
        //        {
        //            var raw = (byte[])value;
        //            return new Guid(raw);
        //        }
        //        return new Guid(value.ToString());
        //    }

        //    if (type == typeof(byte[]))
        //        return value;

        //    if (type == typeof(TimeSpan))
        //    {
        //        var ticks = long.Parse(value.ToString());
        //        return TimeSpan.FromTicks(ticks);
        //    }

        //    return base.ConvertDbValue(value, type);
        //}

        //Moved to Converters.ToQuotedString()
        //public override string GetQuotedValue(object value, Type fieldType)
        //{
        //    if (value == null) return "NULL";

        //    if (fieldType == typeof(Guid))
        //    {
        //        var guid = (Guid)value;
        //        return CompactGuid ? string.Format("CAST('{0}' AS {1})", BitConverter.ToString(guid.ToByteArray()).Replace("-", ""), CompactGuidDefinition)
        //                           : string.Format("CAST('{0}' AS {1})", guid, StringGuidDefinition);
        //    }

        //    if (fieldType == typeof(DateTime) || fieldType == typeof(DateTime?))
        //    {
        //        return GetQuotedDateTimeValue((DateTime)value);
        //    }

        //    if (fieldType == typeof(DateTimeOffset) || fieldType == typeof(DateTimeOffset?))
        //    {
        //        return GetQuotedDateTimeOffsetValue((DateTimeOffset)value);
        //    }

        //    if ((value is TimeSpan) && (fieldType == typeof(Int64) || fieldType == typeof(Int64?)))
        //    {
        //        var longValue = ((TimeSpan)value).Ticks;
        //        return base.GetQuotedValue(longValue, fieldType);
        //    }

        //    if (fieldType == typeof(bool?) || fieldType == typeof(bool))
        //    {
        //        var boolValue = (bool)value;
        //        return base.GetQuotedValue(boolValue ? "1" : "0", typeof(string));
        //    }

        //    if (fieldType == typeof(decimal?) || fieldType == typeof(decimal) ||
        //        fieldType == typeof(double?) || fieldType == typeof(double) ||
        //        fieldType == typeof(float?) || fieldType == typeof(float))
        //    {
        //        var s = base.GetQuotedValue(value, fieldType);
        //        if (s.Length > 20) s = s.Substring(0, 20);
        //        return "'" + s + "'"; // when quoted exception is more clear!
        //    }

        //    //Moved to OracleEnumConverter.ToQuotedString()
        //    if (fieldType.IsEnum)
        //    {
        //        if (value is int && !fieldType.IsEnumFlags())
        //        {
        //            value = fieldType.GetEnumName(value);
        //        }

        //        var enumValue = StringSerializer.SerializeToString(value);
        //        // Oracle stores empty strings in varchar columns as null so match that behavior here
        //        if (enumValue == null)
        //            return null;
        //        enumValue = GetQuotedValue(enumValue.Trim('"'));
        //        return enumValue == "''"
        //            ? "null"
        //            : enumValue;
        //    }

        //    if (fieldType == typeof(byte[]))
        //    {
        //        return "hextoraw('" + BitConverter.ToString((byte[])value).Replace("-", "") + "')";
        //    }

        //    return base.GetQuotedValue(value, fieldType);
        //}

        //Moved to Converters.ToDbParamValue()
        //public override object GetParamValue(object value, Type fieldType)
        //{
        //    if (value == null) return "NULL";

        //    if (fieldType == typeof(Guid))
        //    {
        //        var guid = (Guid)value;

        //        if (CompactGuid)
        //            return guid.ToByteArray();

        //        return guid.ToString();
        //    }

        //    if (fieldType == typeof(DateTimeOffset) || fieldType == typeof(DateTimeOffset?))
        //    {
        //        return GetQuotedDateTimeOffsetValue((DateTimeOffset)value);
        //    }

        //    if ((value is TimeSpan) && (fieldType == typeof(Int64) || fieldType == typeof(Int64?)))
        //    {
        //        var longValue = ((TimeSpan)value).Ticks;
        //        return base.GetQuotedValue(longValue, fieldType);
        //    }

        //    if (fieldType == typeof(TimeSpan))
        //        return ((TimeSpan)value).Ticks;

        //    if (fieldType == typeof(bool?) || fieldType == typeof(bool))
        //    {
        //        var boolValue = (bool)value;
        //        return boolValue ? 1 : 0;
        //    }

        //    //Moved to OracleEnumConverter.ToDbParamValue()
        //    if (fieldType.IsEnum)
        //    {
        //        if (value is int && !fieldType.IsEnumFlags())
        //        {
        //            value = fieldType.GetEnumName(value);
        //        }

        //        var enumValue = StringSerializer.SerializeToString(value);
        //        // Oracle stores empty strings in varchar columns as null so match that behavior here
        //        if (enumValue == null)
        //            return null;
        //        enumValue = enumValue.Trim('"');
        //        return enumValue == ""
        //            ? "null"
        //            : enumValue;
        //    }

        //    if (fieldType == typeof(byte[]))
        //    {
        //        return "hextoraw('" + BitConverter.ToString((byte[])value).Replace("-", "") + "')";
        //    }

        //    //override default by setting base.ReferenceTypeConverter
        //    if (fieldType.IsRefType())
        //    {
        //        return StringSerializer.SerializeToString(value);
        //    }

        //    switch (fieldType.GetTypeCode())
        //    {
        //        case TypeCode.UInt16:
        //        case TypeCode.UInt32:
        //        case TypeCode.UInt64:
        //            return Convert.ToDecimal(value);
        //    }

        //    return value;
        //}

        public override object ToDbValue(object value, Type type)
        {
            if (value == null || value is DBNull)
                return null;

            if (type.IsEnum)
                return EnumConverter.ToDbValue(type, value);

            if (type.IsRefType())
                return ReferenceTypeConverter.ToDbValue(type, value);

            IOrmLiteConverter converter = null;
            try
            {
                if (Converters.TryGetValue(type, out converter))
                {
                    if (type == typeof(DateTimeOffset))
                    {
                        return converter.ToQuotedString(type, value);
                    }

                    return converter.ToDbValue(type, value);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in {0}.ToDbValue() value '{1}' and Type '{2}'"
                    .Fmt(converter.GetType().Name, value != null ? value.GetType().Name : "undefined", type.Name), ex);
                throw;
            }

            return base.ToDbValue(value, type);
        }

        const string IsoDateFormat = "yyyy-MM-dd";
        const string IsoTimeFormat = "HH:mm:ss";
        const string IsoMillisecondFormat = "fffffff";
        const string IsoTimeZoneFormat = "zzz";
        const string OracleDateFormat = "YYYY-MM-DD";
        const string OracleTimeFormat = "HH24:MI:SS";
        const string OracleMillisecondFormat = "FF9";
        const string OracleTimeZoneFormat = "TZH:TZM";

        internal string GetQuotedDateTimeOffsetValue(DateTimeOffset dateValue)
        {
            var iso8601Format = string.Format("{0} {1}", GetIsoDateTimeFormat(dateValue.TimeOfDay), IsoTimeZoneFormat);
            var oracleFormat = string.Format("{0} {1}", GetOracleDateTimeFormat(dateValue.TimeOfDay), OracleTimeZoneFormat);
            return string.Format("TO_TIMESTAMP_TZ({0}, {1})", base.GetQuotedValue(dateValue.ToString(iso8601Format), typeof(string)), base.GetQuotedValue(oracleFormat, typeof(string)));
        }

        internal string GetQuotedDateTimeValue(DateTime dateValue)
        {
            var iso8601Format = GetIsoDateTimeFormat(dateValue.TimeOfDay);
            var oracleFormat = GetOracleDateTimeFormat(dateValue.TimeOfDay);
            return string.Format("TO_TIMESTAMP({0}, {1})", base.GetQuotedValue(dateValue.ToString(iso8601Format), typeof(string)), base.GetQuotedValue(oracleFormat, typeof(string)));
        }

        private string GetIsoDateTimeFormat(TimeSpan timeOfDay)
        {
            return GetTimeFormat(timeOfDay, IsoDateFormat, IsoTimeFormat, IsoMillisecondFormat);
        }

        private string GetOracleDateTimeFormat(TimeSpan timeOfDay)
        {
            return GetTimeFormat(timeOfDay, OracleDateFormat, OracleTimeFormat, OracleMillisecondFormat);
        }

        private string GetTimeFormat(TimeSpan timeOfDay, string dateFormat, string timeFormat, string millisecondFormat)
        {
            var isStartOfDay = timeOfDay.Ticks == 0;
            if (isStartOfDay) return dateFormat;
            var hasFractionalSeconds = (timeOfDay.Milliseconds != 0) || ((timeOfDay.Ticks % TimeSpan.TicksPerMillisecond) != 0);
            return hasFractionalSeconds 
                ? string.Format("{0} {1}.{2}", dateFormat, timeFormat, millisecondFormat) 
                : string.Format("{0} {1}", dateFormat, timeFormat);
        }

        public override string ToSelectStatement(Type tableType, string sqlFilter, params object[] filterParams)
        {
            var sql = new StringBuilder();
            const string selectStatement = "SELECT ";
            var modelDef = GetModel(tableType);
            var isFullSelectStatement = false;
            if (!string.IsNullOrEmpty(sqlFilter))
            {
                var cleanFilter = sqlFilter.Trim().Replace('\r', ' ').Replace('\n', ' ').ToUpperInvariant();
                isFullSelectStatement = cleanFilter.Length > selectStatement.Length
                    && cleanFilter.Substring(0, selectStatement.Length).Equals(selectStatement);
            }

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
                if (fieldDef.IsComputed || fieldDef.IsRowVersion) continue;

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
            var fieldMap = GetFieldDefinitionMap(modelDef);

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
                        fieldDef.Sequence = Sequence(NamingStrategy.GetTableName(modelDef), fieldDef.FieldName, fieldDef.Sequence);
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
                    else if (pi.PropertyType == typeof(Int16) || pi.PropertyType == typeof(Int16?))
                        pi.SetProperty(obj, Convert.ToInt16(result));
                    else if (pi.PropertyType == typeof(Int32) || pi.PropertyType == typeof(Int32?))
                        pi.SetProperty(obj, Convert.ToInt32(result));
                    else if (pi.PropertyType == typeof(Guid) || pi.PropertyType == typeof(Guid?))
                        pi.SetProperty(obj, result);
                    else
                        pi.SetProperty(obj, Convert.ToInt64(result));
                }

                SetParameterValue<T>(fieldDef, p, obj);
            }
        }

        // TODO: do we need this?
        public override void SetParameterValue<T>(FieldDefinition fieldDef, IDataParameter p, object obj)
        {
            p.Value = GetValueOrDbNull<T>(fieldDef, obj);
        }

        //Moved to Converters.ToDbValue()
        //protected override object GetValue<T>(FieldDefinition fieldDef, object obj)
        //{
        //    var value = base.GetValue<T>(fieldDef, obj);

        //    if (value != null)
        //    {
        //        if (fieldDef.FieldType == typeof(Guid))
        //        {
        //            var guid = (Guid)value;
        //            if (CompactGuid) return guid.ToByteArray();
        //            return guid.ToString();
        //        }

        //        if (fieldDef.FieldType == typeof(DateTimeOffset))
        //        {
        //            var timestamp = (DateTimeOffset)value;
        //            return _timestampConverter.ConvertToOracleTimeStampTz(timestamp);
        //        }
        //    }
        //    return value;
        //}

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
                        fieldDef.Sequence = Sequence(NamingStrategy.GetTableName(modelDef), fieldDef.FieldName, fieldDef.Sequence);
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
                    fieldDef.AutoIncrement,
                    fieldDef.IsNullable,
                    fieldDef.IsRowVersion,
                    fieldDef.FieldLength,
                    fieldDef.Scale,
                    GetDefaultValue(fieldDef),
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

        public override string GetLoadChildrenSubSelect<From>(SqlExpression<From> expr)
        {
            if (!expr.OrderByExpression.IsNullOrEmpty() && expr.Rows == null)
            {
                var modelDef = expr.ModelDef;
                expr.Select(this.GetQuotedColumnName(modelDef, modelDef.PrimaryKey))
                    .ClearLimits()
                    .OrderBy(""); //Invalid in Sub Selects

                var subSql = expr.ToSelectStatement();

                return subSql;
            }

            return base.GetLoadChildrenSubSelect(expr);
        }

        public override string ToCreateSequenceStatement(Type tableType, string sequenceName)
        {
            var result = "";
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.AutoIncrement || !fieldDef.Sequence.IsNullOrEmpty())
                {
                    var seqName = Sequence(NamingStrategy.GetTableName(modelDef), fieldDef.FieldName, fieldDef.Sequence);
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
                    var seqName = Sequence(NamingStrategy.GetTableName(modelDef), fieldDef.FieldName, fieldDef.Sequence);

                    if (gens.IndexOf(seqName) == -1)
                        gens.Add(seqName);
                }
            }
            return gens;
        }

        public override string GetColumnDefinition(string fieldName, Type fieldType,
            bool isPrimaryKey, bool autoIncrement, bool isNullable, bool isRowVersion,
            int? fieldLength, int? scale, string defaultValue, string customFieldDefinition)
        {
            var fieldDefinition = customFieldDefinition ?? GetColumnTypeDefinition(fieldType, fieldLength, scale);

            var sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldName), fieldDefinition);

            if (isRowVersion)
            {
                sql.AppendFormat(DefaultValueFormat, 1L);
            }
            else if (!string.IsNullOrEmpty(defaultValue))
            {
                sql.AppendFormat(DefaultValueFormat, defaultValue);
            }

            sql.Append(isNullable ? " NULL" : " NOT NULL");

            var definition = sql.ToString();

            return definition;
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
                    ToCreateIndexStatement(fieldDef.IsUnique, indexName, modelDef, fieldDef.FieldName));
            }

            foreach (var compositeIndex in modelDef.CompositeIndexes)
            {
                var indexName = GetCompositeIndexNameWithSchema(compositeIndex, modelDef);
                indexName = NamingStrategy.ApplyNameRestrictions(indexName);
                var indexNames = string.Join(",", compositeIndex.FieldNames.ToArray());

                sqlIndexes.Add(
                    ToCreateIndexStatement(compositeIndex.Unique, indexName, modelDef, indexNames, isCombined:true));
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
                (isCombined) ? fieldName : GetQuotedColumnName(fieldName));
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
                return long.TryParse(value.ToString(), out nv) 
                    ? nv 
                    : 0;
            }

            using (var dbCmd = connection.CreateCommand())
            {
                dbCmd.Transaction = transaction;
                dbCmd.CommandText = string.Format("SELECT {0}.NEXTVAL FROM dual", Quote(sequence));
                var result = dbCmd.LongScalar();
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
            return Quote(NamingStrategy.GetTableName(modelDef));
        }

        public override string GetQuotedTableName(string tableName, string schema=null)
        {
            return schema == null 
                ? Quote(NamingStrategy.GetTableName(tableName))
                : Quote(NamingStrategy.GetSchemaName(schema))
                  + "."
                  + Quote(NamingStrategy.GetTableName(tableName));
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
            return name.TrimStart('_');
        }

        public virtual string Sequence(string modelName, string fieldName, string sequence)
        {
            //TODO used to return Quote(sequence)
            if (!sequence.IsNullOrEmpty()) return sequence;
            var seqName = NamingStrategy.GetSequenceName(modelName, fieldName);
            return seqName;
        }

        public override SqlExpression<T> SqlExpression<T>()
        {
            return new OracleSqlExpression<T>(this);
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName, string schema=null)
        {
            if (!WillQuote(tableName)) tableName = tableName.ToUpper();

            var sql = "SELECT count(*) FROM USER_TABLES WHERE TABLE_NAME = {0}".SqlFmt(tableName);

            if (schema != null)
                sql += " AND OWNER = {0}".SqlFmt(schema);

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


        public override string ToSelectStatement(ModelDefinition modelDef,
            string selectExpression,
            string bodyExpression,
            string orderByExpression = null,
            int? offset = null,
            int? rows = null)
        {
            var sbInner = new StringBuilder(selectExpression);
            if (!bodyExpression.StartsWith(" ") && !bodyExpression.StartsWith("\n")
                && !selectExpression.EndsWith(" ") && !selectExpression.EndsWith("\n"))
            {
                sbInner.Append(" ");
            }
            sbInner.Append(bodyExpression);

            if (!rows.HasValue && !offset.HasValue)
                return sbInner + " " + orderByExpression;

            if (!offset.HasValue)
                offset = 0;

            if (string.IsNullOrEmpty(orderByExpression) && rows.HasValue)
            {
                var primaryKey = modelDef.FieldDefinitions.FirstOrDefault(x => x.IsPrimaryKey);
                if (primaryKey == null)
                {
                    if (rows.Value == 1 && offset.Value == 0)
                    {
                        // Probably used Single<> extension method on a table with a composite key so let it through.
                        // Lack of an orderby expression will mean it returns a random matching row, but that is OK.
                        orderByExpression = "";
                    }
                    else
                        throw new ApplicationException("Malformed model, no PrimaryKey defined");
                }
                else
                {
                    orderByExpression = string.Format("ORDER BY {0}",
                        this.GetQuotedColumnName(modelDef, primaryKey.FieldName));
                }
            }
            sbInner.Append(" " + orderByExpression);

            var sql = sbInner.ToString();

            //TODO paging doesn't work with ORACLE because we are returning RNUM so we need to figure out a way to return just the desired columns
            var sb = new StringBuilder();
            sb.AppendLine("SELECT * FROM (");
            sb.AppendLine("SELECT \"_ss_ormlite_1_\".*, ROWNUM RNUM FROM (");
            sb.Append(sql);
            sb.AppendLine(") \"_ss_ormlite_1_\"");
            if (rows.HasValue)
                sb.AppendFormat("WHERE ROWNUM <= {0} + {1}) \"_ss_ormlite_2_\" ", offset.Value, rows.Value);
            else
                sb.Append(") \"_ss_ormlite_2_\" ");
            sb.AppendFormat("WHERE \"_ss_ormlite_2_\".RNUM > {0}", offset.Value);

            return sb.ToString();
        }

        public override string ToRowCountStatement(string innerSql)
        {
            return "SELECT COUNT(*) FROM ({0})".Fmt(innerSql);
        }
    }
}
