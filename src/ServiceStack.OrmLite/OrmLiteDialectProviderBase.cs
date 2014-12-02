//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2013 Service Stack LLC. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;
using ServiceStack.Text;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{

    public abstract class OrmLiteDialectProviderBase<TDialect>
        : IOrmLiteDialectProvider
        where TDialect : IOrmLiteDialectProvider
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(IOrmLiteDialectProvider));

        protected OrmLiteDialectProviderBase()
        {
            UpdateStringColumnDefinitions();
            StringSerializer = new JsvStringSerializer();
        }

        #region ADO.NET supported types
        /* ADO.NET UNDERSTOOD DATA TYPES:
			COUNTER	DbType.Int64
			AUTOINCREMENT	DbType.Int64
			IDENTITY	DbType.Int64
			LONG	DbType.Int64
			TINYINT	DbType.Byte
			INTEGER	DbType.Int64
			INT	DbType.Int32
			VARCHAR	DbType.String
			NVARCHAR	DbType.String
			CHAR	DbType.String
			NCHAR	DbType.String
			TEXT	DbType.String
			NTEXT	DbType.String
			STRING	DbType.String
			DOUBLE	DbType.Double
			FLOAT	DbType.Double
			REAL	DbType.Single
			BIT	DbType.Boolean
			YESNO	DbType.Boolean
			LOGICAL	DbType.Boolean
			BOOL	DbType.Boolean
			NUMERIC	DbType.Decimal
			DECIMAL	DbType.Decimal
			MONEY	DbType.Decimal
			CURRENCY	DbType.Decimal
			TIME	DbType.DateTime
			DATE	DbType.DateTime
			TIMESTAMP	DbType.DateTime
			DATETIME	DbType.DateTime
			BLOB	DbType.Binary
			BINARY	DbType.Binary
			VARBINARY	DbType.Binary
			IMAGE	DbType.Binary
			GENERAL	DbType.Binary
			OLEOBJECT	DbType.Binary
			GUID	DbType.Guid
			UNIQUEIDENTIFIER	DbType.Guid
			MEMO	DbType.String
			NOTE	DbType.String
			LONGTEXT	DbType.String
			LONGCHAR	DbType.String
			SMALLINT	DbType.Int16
			BIGINT	DbType.Int64
			LONGVARCHAR	DbType.String
			SMALLDATE	DbType.DateTime
			SMALLDATETIME	DbType.DateTime
		 */
        #endregion

        public IOrmLiteExecFilter ExecFilter { get; set; }

        public string StringLengthNonUnicodeColumnDefinitionFormat = "VARCHAR({0})";
        public string StringLengthUnicodeColumnDefinitionFormat = "NVARCHAR({0})";

        //Set by Constructor and UpdateStringColumnDefinitions()
        public string StringColumnDefinition;
        public string StringLengthColumnDefinitionFormat;

        private string maxStringColumnDefinition;
        public string MaxStringColumnDefinition
        {
            get { return maxStringColumnDefinition ?? StringColumnDefinition; }
            set { maxStringColumnDefinition = value; }
        }

        public string AutoIncrementDefinition = "AUTOINCREMENT"; //SqlServer express limit
        public string IntColumnDefinition = "INTEGER";
        public string LongColumnDefinition = "BIGINT";
        public string GuidColumnDefinition = "GUID";
        public string BoolColumnDefinition = "BOOL";
        public string RealColumnDefinition = "DOUBLE";
        public string DecimalColumnDefinition = "DECIMAL";
        public string BlobColumnDefinition = "BLOB";
        public string DateTimeColumnDefinition = "DATETIME";
        public string TimeColumnDefinition = "BIGINT";
        public string DateTimeOffsetColumnDefinition = "DATETIMEOFFSET";

        private int defaultDecimalPrecision = 18;
        private int defaultDecimalScale = 12;

        public int DefaultDecimalPrecision
        {
            get { return defaultDecimalPrecision; }
            set { defaultDecimalPrecision = value; }
        }

        public int DefaultDecimalScale
        {
            get { return defaultDecimalScale; }
            set { defaultDecimalScale = value; }
        }

        private int defaultStringLength = 8000; //SqlServer express limit
        public int DefaultStringLength
        {
            get
            {
                return defaultStringLength;
            }
            set
            {
                defaultStringLength = value;
                UpdateStringColumnDefinitions();
            }
        }

        private string paramString = "@";
        public string ParamString
        {
            get { return paramString; }
            set { paramString = value; }
        }

        protected bool useUnicode;
        public virtual bool UseUnicode
        {
            get
            {
                return useUnicode;
            }
            set
            {
                useUnicode = value;
                UpdateStringColumnDefinitions();
            }
        }

        private INamingStrategy namingStrategy = new OrmLiteNamingStrategyBase();
        public INamingStrategy NamingStrategy
        {
            get
            {
                return namingStrategy;
            }
            set
            {
                namingStrategy = value;
            }
        }

        public IStringSerializer StringSerializer { get; set; }

        public virtual void UpdateStringColumnDefinitions()
        {
            this.StringLengthColumnDefinitionFormat = useUnicode
                ? StringLengthUnicodeColumnDefinitionFormat
                : StringLengthNonUnicodeColumnDefinitionFormat;

            this.StringColumnDefinition = string.Format(
                this.StringLengthColumnDefinitionFormat, DefaultStringLength);
        }

        protected DbTypes<TDialect> DbTypeMap = new DbTypes<TDialect>();
        protected void InitColumnTypeMap()
        {
            DbTypeMap.Set<string>(DbType.String, StringColumnDefinition);
            DbTypeMap.Set<char>(DbType.StringFixedLength, StringColumnDefinition);
            DbTypeMap.Set<char?>(DbType.StringFixedLength, StringColumnDefinition);
            DbTypeMap.Set<char[]>(DbType.String, StringColumnDefinition);
            DbTypeMap.Set<bool>(DbType.Boolean, BoolColumnDefinition);
            DbTypeMap.Set<bool?>(DbType.Boolean, BoolColumnDefinition);
            DbTypeMap.Set<Guid>(DbType.Guid, GuidColumnDefinition);
            DbTypeMap.Set<Guid?>(DbType.Guid, GuidColumnDefinition);
            DbTypeMap.Set<DateTime>(DbType.DateTime, DateTimeColumnDefinition);
            DbTypeMap.Set<DateTime?>(DbType.DateTime, DateTimeColumnDefinition);

            DbTypeMap.Set<TimeSpan>(DbType.Int64, TimeColumnDefinition); //using ticks
            DbTypeMap.Set<TimeSpan?>(DbType.Int64, TimeColumnDefinition);
            DbTypeMap.Set<DateTimeOffset>(DbType.DateTimeOffset, DateTimeOffsetColumnDefinition);
            DbTypeMap.Set<DateTimeOffset?>(DbType.DateTimeOffset, DateTimeOffsetColumnDefinition);

            DbTypeMap.Set<byte>(DbType.Byte, IntColumnDefinition);
            DbTypeMap.Set<byte?>(DbType.Byte, IntColumnDefinition);
            DbTypeMap.Set<sbyte>(DbType.SByte, IntColumnDefinition);
            DbTypeMap.Set<sbyte?>(DbType.SByte, IntColumnDefinition);
            DbTypeMap.Set<short>(DbType.Int16, IntColumnDefinition);
            DbTypeMap.Set<short?>(DbType.Int16, IntColumnDefinition);
            DbTypeMap.Set<ushort>(DbType.UInt16, IntColumnDefinition);
            DbTypeMap.Set<ushort?>(DbType.UInt16, IntColumnDefinition);
            DbTypeMap.Set<int>(DbType.Int32, IntColumnDefinition);
            DbTypeMap.Set<int?>(DbType.Int32, IntColumnDefinition);
            DbTypeMap.Set<uint>(DbType.UInt32, IntColumnDefinition);
            DbTypeMap.Set<uint?>(DbType.UInt32, IntColumnDefinition);

            DbTypeMap.Set<long>(DbType.Int64, LongColumnDefinition);
            DbTypeMap.Set<long?>(DbType.Int64, LongColumnDefinition);
            DbTypeMap.Set<ulong>(DbType.UInt64, LongColumnDefinition);
            DbTypeMap.Set<ulong?>(DbType.UInt64, LongColumnDefinition);

            DbTypeMap.Set<float>(DbType.Single, RealColumnDefinition);
            DbTypeMap.Set<float?>(DbType.Single, RealColumnDefinition);
            DbTypeMap.Set<double>(DbType.Double, RealColumnDefinition);
            DbTypeMap.Set<double?>(DbType.Double, RealColumnDefinition);

            DbTypeMap.Set<decimal>(DbType.Decimal, DecimalColumnDefinition);
            DbTypeMap.Set<decimal?>(DbType.Decimal, DecimalColumnDefinition);

            DbTypeMap.Set<byte[]>(DbType.Binary, BlobColumnDefinition);

            DbTypeMap.Set<object>(DbType.String, StringColumnDefinition);

            OnAfterInitColumnTypeMap();
        }

        public virtual void OnAfterInitColumnTypeMap()
        {
        }

        public string DefaultValueFormat = " DEFAULT ({0})";

        public virtual bool ShouldQuoteValue(Type fieldType)
        {
            string fieldDefinition;
            if (!DbTypeMap.ColumnTypeMap.TryGetValue(fieldType, out fieldDefinition))
            {
                fieldDefinition = this.GetUndefinedColumnDefinition(fieldType, null);
            }

            return fieldDefinition != IntColumnDefinition
                   && fieldDefinition != LongColumnDefinition
                   && fieldDefinition != RealColumnDefinition
                   && fieldDefinition != DecimalColumnDefinition
                   && fieldDefinition != BoolColumnDefinition;
        }

        protected const int NotFound = -1;

        /// <summary>
        /// Populates row fields during re-hydration of results.
        /// </summary>
        public virtual void SetDbValue(FieldDefinition fieldDef, IDataReader reader, int colIndex, object instance)
        {
            if (HandledDbNullValue(fieldDef, reader, colIndex, instance)) return;

            var convertedValue = ConvertDbValue(reader.GetValue(colIndex), fieldDef.FieldType);
            try
            {
                fieldDef.SetValueFn(instance, convertedValue);
            }
            catch (NullReferenceException ignore) { }
        }

        public static bool HandledDbNullValue(FieldDefinition fieldDef, IDataReader dataReader, int colIndex, object instance)
        {
            if (fieldDef == null || fieldDef.SetValueFn == null || colIndex == NotFound) return true;
            if (dataReader.IsDBNull(colIndex))
            {
                if (fieldDef.IsNullable)
                {
                    fieldDef.SetValueFn(instance, null);
                }
                else
                {
                    fieldDef.SetValueFn(instance, fieldDef.FieldType.GetDefaultValue());
                }
                return true;
            }
            return false;
        }

        public abstract IDbConnection CreateConnection(string filePath, Dictionary<string, string> options);

        public virtual string GetQuotedValue(string paramValue)
        {
            return "'" + paramValue.Replace("'", "''") + "'";
        }

        public virtual string GetQuotedTableName(ModelDefinition modelDef)
        {
            return GetQuotedTableName(modelDef.ModelName);
        }

        public virtual string GetQuotedTableName(string tableName)
        {
            return string.Format("\"{0}\"", namingStrategy.GetTableName(tableName));
        }

        public virtual string GetQuotedColumnName(string columnName)
        {
            return string.Format("\"{0}\"", namingStrategy.GetColumnName(columnName));
        }

        public virtual string GetQuotedName(string name)
        {
            return string.Format("\"{0}\"", name);
        }

        public virtual string SanitizeFieldNameForParamName(string fieldName)
        {
            return (fieldName ?? "").Replace(" ", "");
        }

        protected virtual string GetUndefinedColumnDefinition(Type fieldType, int? fieldLength)
        {
            return fieldLength.HasValue
                ? string.Format(StringLengthColumnDefinitionFormat, fieldLength.GetValueOrDefault(DefaultStringLength))
                : MaxStringColumnDefinition;
        }

        public virtual string GetColumnDefinition(string fieldName, Type fieldType,
            bool isPrimaryKey, bool autoIncrement, bool isNullable, bool isRowVersion,
            int? fieldLength, int? scale, string defaultValue, string customFieldDefinition)
        {
            string fieldDefinition;

            if (customFieldDefinition != null)
            {
                fieldDefinition = customFieldDefinition;
            }
            else if (fieldType == typeof(string))
            {
                fieldDefinition = fieldLength == StringLengthAttribute.MaxText
                    ? MaxStringColumnDefinition
                    : string.Format(StringLengthColumnDefinitionFormat, fieldLength.GetValueOrDefault(DefaultStringLength));
            }
            else
            {
                if (!DbTypeMap.ColumnTypeMap.TryGetValue(fieldType, out fieldDefinition))
                {
                    fieldDefinition = this.GetUndefinedColumnDefinition(fieldType, fieldLength);
                }
            }

            var sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldName), fieldDefinition);

            if (isPrimaryKey)
            {
                sql.Append(" PRIMARY KEY");
                if (autoIncrement)
                {
                    sql.Append(" ").Append(AutoIncrementDefinition);
                }
            }
            else
            {
                if (isNullable)
                {
                    sql.Append(" NULL");
                }
                else
                {
                    sql.Append(" NOT NULL");
                }
            }

            if (!string.IsNullOrEmpty(defaultValue))
            {
                sql.AppendFormat(DefaultValueFormat, defaultValue);
            }

            return sql.ToString();
        }

        public virtual string SelectIdentitySql { get; set; }

        public virtual long GetLastInsertId(IDbCommand dbCmd)
        {
            if (SelectIdentitySql == null)
                throw new NotImplementedException("Returning last inserted identity is not implemented on this DB Provider.");

            dbCmd.CommandText = SelectIdentitySql;
            return dbCmd.ExecLongScalar();
        }

        public virtual long InsertAndGetLastInsertId<T>(IDbCommand dbCmd)
        {
            if (SelectIdentitySql == null)
                throw new NotImplementedException("Returning last inserted identity is not implemented on this DB Provider.");

            dbCmd.CommandText += "; " + SelectIdentitySql;

            return dbCmd.ExecLongScalar();
        }

        // Fmt
        public virtual string ToSelectStatement(Type tableType, string sqlFilter, params object[] filterParams)
        {
            const string SelectStatement = "SELECT";
            var isFullSelectStatement =
                !string.IsNullOrEmpty(sqlFilter)
                && sqlFilter.TrimStart().StartsWith(SelectStatement, StringComparison.OrdinalIgnoreCase);

            if (isFullSelectStatement)
                return sqlFilter.SqlFmt(filterParams);

            var modelDef = tableType.GetModelDefinition();
            var sql = new StringBuilder();
            sql.AppendFormat("SELECT {0} FROM {1}",
                GetColumnNames(modelDef),
                GetQuotedTableName(modelDef));

            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFmt(filterParams);
                if (!sqlFilter.StartsWith("ORDER ", StringComparison.OrdinalIgnoreCase)
                    && !sqlFilter.StartsWith("LIMIT ", StringComparison.OrdinalIgnoreCase))
                {
                    sql.Append(" WHERE ");
                }

                sql.Append(sqlFilter);
            }

            return sql.ToString();
        }

        public virtual string ToSelectStatement(ModelDefinition modelDef, 
            string selectExpression, 
            string bodyExpression,
            string orderByExpression = null, 
            int? offset = null, 
            int? rows = null)
        {

            var sb = new StringBuilder(selectExpression);
            sb.Append(bodyExpression);
            if (orderByExpression != null)
            {
                sb.Append(orderByExpression);
            }

            if (offset != null || rows != null)
            {
                sb.Append("\nLIMIT ");
                if (offset == null)
                {
                    sb.Append(rows);
                }
                else
                {
                    sb.Append(rows.GetValueOrDefault(int.MaxValue)).Append(" OFFSET ").Append(offset);
                }
            }

            return sb.ToString();
        }

        public virtual string GetRowVersionColumnName(FieldDefinition field)
        {
            return GetQuotedColumnName(field.FieldName);
        }

        public virtual string GetColumnNames(ModelDefinition modelDef)
        {
            var sqlColumns = new StringBuilder();
            foreach (var field in modelDef.FieldDefinitions)
            {
                if (sqlColumns.Length > 0)
                    sqlColumns.Append(", ");

                sqlColumns.Append(field.GetQuotedName(this));
            }

            return sqlColumns.ToString();
        }

        /// Fmt
        public virtual string ToInsertRowStatement(IDbCommand command, object objWithProperties, ICollection<string> insertFields = null)
        {
            if (insertFields == null)
                insertFields = new List<string>();

            var sbColumnNames = new StringBuilder();
            var sbColumnValues = new StringBuilder();
            var modelDef = objWithProperties.GetType().GetModelDefinition();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.ShouldSkipInsert())
                    continue;

                //insertFields contains Property "Name" of fields to insert ( that's how expressions work )
                if (insertFields.Count > 0 && !insertFields.Contains(fieldDef.Name)) 
                    continue;

                if (sbColumnNames.Length > 0) sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(GetQuotedColumnName(fieldDef.FieldName));
                    sbColumnValues.Append(fieldDef.GetQuotedValue(objWithProperties, this));
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToInsertRowStatement(): " + ex.Message, ex);
                    throw;
                }
            }

            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                    GetQuotedTableName(modelDef), sbColumnNames, sbColumnValues);

            return sql;
        }

        public virtual void PrepareParameterizedInsertStatement<T>(IDbCommand cmd, ICollection<string> insertFields = null)
        {
            var sbColumnNames = new StringBuilder();
            var sbColumnValues = new StringBuilder();
            var modelDef = typeof(T).GetModelDefinition();

            cmd.Parameters.Clear();
            cmd.CommandTimeout = OrmLiteConfig.CommandTimeout;

            foreach (var fieldDef in modelDef.FieldDefinitionsArray)
            {
                if (fieldDef.ShouldSkipInsert())
                    continue;

                //insertFields contains Property "Name" of fields to insert ( that's how expressions work )
                if (insertFields != null && !insertFields.Contains(fieldDef.Name)) 
                    continue;

                if (sbColumnNames.Length > 0)
                    sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0)
                    sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(GetQuotedColumnName(fieldDef.FieldName));
                    sbColumnValues.Append(this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName)));

                    AddParameter(cmd, fieldDef);
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in PrepareParameterizedInsertStatement(): " + ex.Message, ex);
                    throw;
                }
            }

            cmd.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                            GetQuotedTableName(modelDef), sbColumnNames, sbColumnValues);
        }

        public virtual bool PrepareParameterizedUpdateStatement<T>(IDbCommand cmd, ICollection<string> updateFields = null)
        {
            var sqlFilter = new StringBuilder();
            var sql = new StringBuilder();
            var modelDef = typeof(T).GetModelDefinition();
            var hadRowVesion = false;
            var updateAllFields = updateFields == null || updateFields.Count == 0;

            cmd.Parameters.Clear();
            cmd.CommandTimeout = OrmLiteConfig.CommandTimeout;

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.ShouldSkipUpdate()) 
                    continue;

                try
                {
                    if ((fieldDef.IsPrimaryKey || fieldDef.IsRowVersion) && updateAllFields)
                    {
                        if (sqlFilter.Length > 0) 
                            sqlFilter.Append(" AND ");

                        AppendFieldCondition(sqlFilter, fieldDef, cmd);

                        if (fieldDef.IsRowVersion)
                            hadRowVesion = true;

                        continue;
                    }

                    if (!updateAllFields && !updateFields.Contains(fieldDef.Name))
                        continue;

                    if (sql.Length > 0)
                        sql.Append(", ");

                    AppendFieldCondition(sql, fieldDef, cmd);
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in PrepareParameterizedUpdateStatement(): " + ex.Message, ex);
                }
            }

            if (sql.Length > 0)
            {
                cmd.CommandText = string.Format("UPDATE {0} SET {1} {2}",
                    GetQuotedTableName(modelDef), sql, (sqlFilter.Length > 0 ? "WHERE " + sqlFilter : ""));
            }

            return hadRowVesion;
        }

        public virtual void AppendFieldCondition(StringBuilder sqlFilter, FieldDefinition fieldDef, IDbCommand cmd)
        {
            sqlFilter
                .Append(GetQuotedColumnName(fieldDef.FieldName))
                .Append("=")
                .Append(this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName)));

            AddParameter(cmd, fieldDef);
        }

        public virtual void AppendFieldConditionFmt(StringBuilder sqlFilter, FieldDefinition fieldDef, object objWithProperties)
        {
            sqlFilter.AppendFormat("{0}={1}", 
                GetQuotedColumnName(fieldDef.FieldName),
                fieldDef.GetQuotedValue(objWithProperties, this));
        }

        public virtual bool PrepareParameterizedDeleteStatement<T>(IDbCommand cmd, ICollection<string> deleteFields = null)
        {
            var sqlFilter = new StringBuilder();
            var modelDef = typeof(T).GetModelDefinition();
            var hadRowVesion = false;
            var hasSpecificFilter = deleteFields != null && deleteFields.Count > 0;

            cmd.Parameters.Clear();
            cmd.CommandTimeout = OrmLiteConfig.CommandTimeout;

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.ShouldSkipDelete()) 
                    continue;

                if (!fieldDef.IsRowVersion && (hasSpecificFilter && !deleteFields.Contains(fieldDef.Name)))
                    continue;

                if (fieldDef.IsRowVersion)
                    hadRowVesion = true;

                try
                {
                    if (sqlFilter.Length > 0)
                        sqlFilter.Append(" AND ");

                    AppendFieldCondition(sqlFilter, fieldDef, cmd);
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in PrepareParameterizedDeleteStatement(): " + ex.Message, ex);
                }
            }

            cmd.CommandText = string.Format("DELETE FROM {0} WHERE {1}",
                GetQuotedTableName(modelDef), sqlFilter);

            return hadRowVesion;
        }

        protected void AddParameter(IDbCommand cmd, FieldDefinition fieldDef)
        {
            var p = cmd.CreateParameter();
            SetParameter(fieldDef, p);
            cmd.Parameters.Add(p);
        }

        public virtual void SetParameter(FieldDefinition fieldDef, IDbDataParameter p)
        {
            p.ParameterName = this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName));

            DbType dbType;
            var sqlDbType = DbTypeMap.ColumnDbTypeMap.TryGetValue(fieldDef.ColumnType, out dbType) 
                ? dbType 
                : DbType.String;

            p.DbType = sqlDbType;
        }

        public virtual void SetParameterValues<T>(IDbCommand dbCmd, object obj)
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

                SetParameterValue<T>(fieldDef, p, obj);
            }
        }

        public Dictionary<string, FieldDefinition> GetFieldDefinitionMap(ModelDefinition modelDef)
        {
            return modelDef.GetFieldDefinitionMap(SanitizeFieldNameForParamName);
        }

        public virtual void SetParameterValue<T>(FieldDefinition fieldDef, IDataParameter p, object obj)
        {
            var value = GetValueOrDbNull<T>(fieldDef, obj);
            p.Value = value;
        }

        protected virtual object GetValue<T>(FieldDefinition fieldDef, object obj)
        {
            var value = obj is T
               ? fieldDef.GetValue(obj)
               : GetAnonValue<T>(fieldDef, obj);

            return GetFieldValue(fieldDef, value);
        }

        public object GetFieldValue(FieldDefinition fieldDef, object value)
        {
            if (value != null)
            {
                if (fieldDef.IsRefType)
                {
                    //Let ADO.NET providers handle byte[]
                    if (fieldDef.FieldType == typeof (byte[]))
                    {
                        return value;
                    }
                    return StringSerializer.SerializeToString(value);
                }
                if (fieldDef.FieldType.IsEnum)
                {
                    var enumValue = StringSerializer.SerializeToString(value);
                    if (enumValue == null)
                        return null;

                    enumValue = enumValue.Trim('"');
                    long intEnum;
                    if (Int64.TryParse(enumValue, out intEnum))
                        return intEnum;
                    
                    return enumValue;
                }
                if (fieldDef.FieldType == typeof (TimeSpan))
                {
                    var timespan = (TimeSpan) value;
                    return timespan.Ticks;
                }
            }

            return value;
        }

        protected virtual object GetValueOrDbNull<T>(FieldDefinition fieldDef, object obj)
        {
            var value = GetValue<T>(fieldDef, obj);
            return value ?? DBNull.Value;
        }

        protected virtual object GetQuotedValueOrDbNull<T>(FieldDefinition fieldDef, object obj)
        {
            var value = obj is T
                ? fieldDef.GetValue(obj)
                : GetAnonValue<T>(fieldDef, obj);

            if (value == null)
                return DBNull.Value;

            var unquotedVal = GetQuotedValue(value, fieldDef.FieldType)
                .TrimStart('\'').TrimEnd('\''); ;

            if (string.IsNullOrEmpty(unquotedVal))
                return DBNull.Value;

            return unquotedVal;
        }

        static readonly ConcurrentDictionary<string, PropertyGetterDelegate> anonValueFnMap =
            new ConcurrentDictionary<string, PropertyGetterDelegate>();

        protected virtual object GetAnonValue<T>(FieldDefinition fieldDef, object obj)
        {
            var anonType = obj.GetType();
            var key = anonType.Name + "." + fieldDef.Name;

            var factoryFn = (Func<string, PropertyGetterDelegate>)(_ =>
                anonType.GetProperty(fieldDef.Name).GetPropertyGetterFn());

            var getterFn = anonValueFnMap.GetOrAdd(key, factoryFn);

            return getterFn(obj);
        }

        public virtual string ToUpdateRowStatement(object objWithProperties, ICollection<string> updateFields = null)
        {
            var sqlFilter = new StringBuilder();
            var sql = new StringBuilder();
            var modelDef = objWithProperties.GetType().GetModelDefinition();
            var updateAllFields = updateFields == null || updateFields.Count == 0;

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.ShouldSkipUpdate())
                    continue;

                try
                {
                    if (fieldDef.IsPrimaryKey && updateAllFields)
                    {
                        if (sqlFilter.Length > 0) 
                            sqlFilter.Append(" AND ");

                        AppendFieldConditionFmt(sqlFilter, fieldDef, objWithProperties);

                        continue;
                    }

                    if (!updateAllFields && !updateFields.Contains(fieldDef.Name) || fieldDef.AutoIncrement) continue;
                    if (sql.Length > 0)
                        sql.Append(", ");

                    AppendFieldConditionFmt(sql, fieldDef, objWithProperties);
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToUpdateRowStatement(): " + ex.Message, ex);
                }
            }

            var updateSql = string.Format("UPDATE {0} SET {1}{2}",
                GetQuotedTableName(modelDef), sql, (sqlFilter.Length > 0 ? " WHERE " + sqlFilter : ""));

            if (sql.Length == 0)
                throw new Exception("No valid update properties provided (e.g. p => p.FirstName): " + updateSql);

            return updateSql;
        }

        public virtual string ToDeleteRowStatement(object objWithProperties)
        {
            var sqlFilter = new StringBuilder();
            var modelDef = objWithProperties.GetType().GetModelDefinition();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.ShouldSkipDelete())
                    continue;

                try
                {
                    if (fieldDef.IsPrimaryKey)
                    {
                        if (sqlFilter.Length > 0) 
                            sqlFilter.Append(" AND ");

                        AppendFieldConditionFmt(sqlFilter, fieldDef, objWithProperties);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToDeleteRowStatement(): " + ex.Message, ex);
                }
            }

            var deleteSql = string.Format("DELETE FROM {0} WHERE {1}",
                GetQuotedTableName(modelDef), sqlFilter);

            return deleteSql;
        }

        public virtual string ToDeleteStatement(Type tableType, string sqlFilter, params object[] filterParams)
        {
            var sql = new StringBuilder();
            const string deleteStatement = "DELETE ";

            var isFullDeleteStatement =
                !string.IsNullOrEmpty(sqlFilter)
                && sqlFilter.Length > deleteStatement.Length
                && sqlFilter.Substring(0, deleteStatement.Length).ToUpper().Equals(deleteStatement);

            if (isFullDeleteStatement) 
                return sqlFilter.SqlFmt(filterParams);

            var modelDef = tableType.GetModelDefinition();
            sql.AppendFormat("DELETE FROM {0}", GetQuotedTableName(modelDef));
            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFmt(filterParams);
                sql.Append(" WHERE ");
                sql.Append(sqlFilter);
            }

            return sql.ToString();
        }

        public virtual string ToCreateTableStatement(Type tableType)
        {
            var sbColumns = new StringBuilder();
            var sbConstraints = new StringBuilder();

            var modelDef = tableType.GetModelDefinition();
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                var columnDefinition = GetColumnDefinition(
                    fieldDef.FieldName,
                    fieldDef.ColumnType,
                    fieldDef.IsPrimaryKey,
                    fieldDef.AutoIncrement,
                    fieldDef.IsNullable,
                    fieldDef.IsRowVersion,
                    fieldDef.FieldLength,
                    fieldDef.Scale,
                    fieldDef.DefaultValue,
                    fieldDef.CustomFieldDefinition);

                if (columnDefinition == null)
                    continue;

                if (sbColumns.Length != 0) 
                    sbColumns.Append(", \n  ");

                sbColumns.Append(columnDefinition);

                if (fieldDef.ForeignKey == null) continue;

                var refModelDef = fieldDef.ForeignKey.ReferenceType.GetModelDefinition();
                sbConstraints.AppendFormat(
                    ", \n\n  CONSTRAINT {0} FOREIGN KEY ({1}) REFERENCES {2} ({3})",
                    GetQuotedName(fieldDef.ForeignKey.GetForeignKeyName(modelDef, refModelDef, NamingStrategy, fieldDef)),
                    GetQuotedColumnName(fieldDef.FieldName),
                    GetQuotedTableName(refModelDef),
                    GetQuotedColumnName(refModelDef.PrimaryKey.FieldName));

                sbConstraints.Append(GetForeignKeyOnDeleteClause(fieldDef.ForeignKey));
                sbConstraints.Append(GetForeignKeyOnUpdateClause(fieldDef.ForeignKey));
            }
            var sql = new StringBuilder(string.Format(
                "CREATE TABLE {0} \n(\n  {1}{2} \n); \n", GetQuotedTableName(modelDef), sbColumns, sbConstraints));

            return sql.ToString();
        }

        public virtual string ToPostCreateTableStatement(ModelDefinition modelDef)
        {
            return null;
        }

        public virtual string ToPostDropTableStatement(ModelDefinition modelDef)
        {
            return null;
        }

        public virtual string GetForeignKeyOnDeleteClause(ForeignKeyConstraint foreignKey)
        {
            return !string.IsNullOrEmpty(foreignKey.OnDelete) ? " ON DELETE " + foreignKey.OnDelete : "";
        }

        public virtual string GetForeignKeyOnUpdateClause(ForeignKeyConstraint foreignKey)
        {
            return !string.IsNullOrEmpty(foreignKey.OnUpdate) ? " ON UPDATE " + foreignKey.OnUpdate : "";
        }

        public virtual List<string> ToCreateIndexStatements(Type tableType)
        {
            var sqlIndexes = new List<string>();

            var modelDef = tableType.GetModelDefinition();
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (!fieldDef.IsIndexed) continue;

                var indexName = GetIndexName(fieldDef.IsUnique, modelDef.ModelName.SafeVarName(), fieldDef.FieldName);

                sqlIndexes.Add(
                    ToCreateIndexStatement(fieldDef.IsUnique, indexName, modelDef, fieldDef.FieldName, isCombined: false, fieldDef: fieldDef));
            }

            foreach (var compositeIndex in modelDef.CompositeIndexes)
            {
                var indexName = GetCompositeIndexName(compositeIndex, modelDef);
                var indexNames = string.Join(" ASC, ",
                    compositeIndex.FieldNames.ConvertAll(GetQuotedName).ToArray());

                sqlIndexes.Add(
                    ToCreateIndexStatement(compositeIndex.Unique, indexName, modelDef, indexNames, true));
            }

            return sqlIndexes;
        }

        public virtual DbType GetColumnDbType(Type valueType)
        {
            if (valueType.IsEnum)
                return DbTypeMap.ColumnDbTypeMap[typeof(string)];

            return DbTypeMap.ColumnDbTypeMap[valueType];
        }

        public virtual string GetColumnTypeDefinition(Type fieldType)
        {
            string fieldDefinition;
            DbTypeMap.ColumnTypeMap.TryGetValue(fieldType, out fieldDefinition);
            return fieldDefinition ?? GetUndefinedColumnDefinition(fieldType, null);
        }

        public virtual bool DoesTableExist(IDbConnection db, string tableName)
        {
            return db.Exec(dbCmd => DoesTableExist(dbCmd, tableName));
        }

        public virtual bool DoesTableExist(IDbCommand dbCmd, string tableName)
        {
            return false;
        }

        public virtual bool DoesSequenceExist(IDbCommand dbCmd, string sequenceName)
        {
            return true;
        }

        protected virtual string GetIndexName(bool isUnique, string modelName, string fieldName)
        {
            return string.Format("{0}idx_{1}_{2}", isUnique ? "u" : "", modelName, fieldName).ToLower();
        }

        protected virtual string GetCompositeIndexName(CompositeIndexAttribute compositeIndex, ModelDefinition modelDef)
        {
            return compositeIndex.Name ?? GetIndexName(compositeIndex.Unique, modelDef.ModelName.SafeVarName(),
                                                       string.Join("_", compositeIndex.FieldNames.ToArray()));
        }

        protected virtual string GetCompositeIndexNameWithSchema(CompositeIndexAttribute compositeIndex, ModelDefinition modelDef)
        {
            return compositeIndex.Name ?? GetIndexName(compositeIndex.Unique,
                    (modelDef.IsInSchema ?
                        modelDef.Schema + "_" + GetQuotedTableName(modelDef) :
                        GetQuotedTableName(modelDef)).SafeVarName(),
                    string.Join("_", compositeIndex.FieldNames.ToArray()));
        }

        protected virtual string ToCreateIndexStatement(bool isUnique, string indexName, ModelDefinition modelDef, string fieldName,
            bool isCombined = false, FieldDefinition fieldDef = null)
        {
            return string.Format("CREATE {0}{1}{2} INDEX {3} ON {4} ({5} ASC); \n",
                                 isUnique ? "UNIQUE" : "",
                                 fieldDef != null && fieldDef.IsClustered ? " CLUSTERED" : "",
                                 fieldDef != null && fieldDef.IsNonClustered ? " NONCLUSTERED" : "",
                                 indexName,
                                 GetQuotedTableName(modelDef),
                                 (isCombined) ? fieldName : GetQuotedColumnName(fieldName));
        }

        public virtual List<string> ToCreateSequenceStatements(Type tableType)
        {
            return new List<string>();
        }

        public virtual string ToCreateSequenceStatement(Type tableType, string sequenceName)
        {
            return "";
        }

        public virtual List<string> SequenceList(Type tableType)
        {
            return new List<string>();
        }

        // TODO : make abstract  ??
        public virtual string ToExistStatement(Type fromTableType,
            object objWithProperties,
            string sqlFilter,
            params object[] filterParams)
        {
            throw new NotImplementedException();
        }

        // TODO : make abstract  ??
        public virtual string ToSelectFromProcedureStatement(
            object fromObjWithProperties,
            Type outputModelType,
            string sqlFilter,
            params object[] filterParams)
        {
            throw new NotImplementedException();
        }

        // TODO : make abstract  ??
        public virtual string ToExecuteProcedureStatement(object objWithProperties)
        {
            return null;
        }

        protected static ModelDefinition GetModel(Type modelType)
        {
            return modelType.GetModelDefinition();
        }

        public virtual SqlExpression<T> SqlExpression<T>()
        {
            throw new NotImplementedException();
        }

        public IDbCommand CreateParameterizedDeleteStatement(IDbConnection connection, object objWithProperties)
        {
            throw new NotImplementedException();
        }

        public virtual string GetDropForeignKeyConstraints(ModelDefinition modelDef)
        {
            return null;
        }

        public static ModelDefinition GetModelDefinition(Type modelType)
        {
            return modelType.GetModelDefinition();
        }

        public virtual string ToAddColumnStatement(Type modelType, FieldDefinition fieldDef)
        {

            var column = GetColumnDefinition(fieldDef.FieldName,
                                             fieldDef.ColumnType,
                                             fieldDef.IsPrimaryKey,
                                             fieldDef.AutoIncrement,
                                             fieldDef.IsNullable,
                                             fieldDef.IsRowVersion,
                                             fieldDef.FieldLength,
                                             fieldDef.Scale,
                                             fieldDef.DefaultValue,
                                             fieldDef.CustomFieldDefinition);
            return string.Format("ALTER TABLE {0} ADD COLUMN {1};",
                                 GetQuotedTableName(modelType.GetModelDefinition().ModelName),
                                 column);
        }


        public virtual string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef)
        {
            var column = GetColumnDefinition(fieldDef.FieldName,
                                             fieldDef.ColumnType,
                                             fieldDef.IsPrimaryKey,
                                             fieldDef.AutoIncrement,
                                             fieldDef.IsNullable,
                                             fieldDef.IsRowVersion,
                                             fieldDef.FieldLength,
                                             fieldDef.Scale,
                                             fieldDef.DefaultValue,
                                             fieldDef.CustomFieldDefinition);
            return string.Format("ALTER TABLE {0} MODIFY COLUMN {1};",
                                 GetQuotedTableName(modelType.GetModelDefinition().ModelName),
                                 column);
        }

        public virtual string ToChangeColumnNameStatement(Type modelType,
                                                          FieldDefinition fieldDef,
                                                          string oldColumnName)
        {
            var column = GetColumnDefinition(fieldDef.FieldName,
                                             fieldDef.ColumnType,
                                             fieldDef.IsPrimaryKey,
                                             fieldDef.AutoIncrement,
                                             fieldDef.IsNullable,
                                             fieldDef.IsRowVersion,
                                             fieldDef.FieldLength,
                                             fieldDef.Scale,
                                             fieldDef.DefaultValue,
                                             fieldDef.CustomFieldDefinition);
            return string.Format("ALTER TABLE {0} CHANGE COLUMN {1} {2};",
                                 GetQuotedTableName(modelType.GetModelDefinition().ModelName),
                                 GetQuotedColumnName(oldColumnName),
                                 column);
        }

        public virtual string ToAddForeignKeyStatement<T, TForeign>(Expression<Func<T, object>> field,
                                                                    Expression<Func<TForeign, object>> foreignField,
                                                                    OnFkOption onUpdate,
                                                                    OnFkOption onDelete,
                                                                    string foreignKeyName = null)
        {
            var sourceMD = ModelDefinition<T>.Definition;
            var fieldName = sourceMD.GetFieldDefinition(field).FieldName;

            var referenceMD = ModelDefinition<TForeign>.Definition;
            var referenceFieldName = referenceMD.GetFieldDefinition(foreignField).FieldName;

            string name = GetQuotedName(foreignKeyName.IsNullOrEmpty() ?
                                        "fk_" + sourceMD.ModelName + "_" + fieldName + "_" + referenceFieldName :
                                        foreignKeyName);

            return string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6};",
                                 GetQuotedTableName(sourceMD.ModelName),
                                 name,
                                 GetQuotedColumnName(fieldName),
                                 GetQuotedTableName(referenceMD.ModelName),
                                 GetQuotedColumnName(referenceFieldName),
                                 GetForeignKeyOnDeleteClause(new ForeignKeyConstraint(typeof(T), onDelete: FkOptionToString(onDelete))),
                                 GetForeignKeyOnUpdateClause(new ForeignKeyConstraint(typeof(T), onUpdate: FkOptionToString(onUpdate))));
        }

        public virtual string ToCreateIndexStatement<T>(Expression<Func<T, object>> field,
                                                        string indexName = null, bool unique = false)
        {

            var sourceMD = ModelDefinition<T>.Definition;
            var fieldName = sourceMD.GetFieldDefinition(field).FieldName;

            string name = GetQuotedName(indexName.IsNullOrEmpty() ?
                                       (unique ? "uidx" : "idx") + "_" + sourceMD.ModelName + "_" + fieldName :
                                       indexName);

            string command = string.Format("CREATE{0}INDEX {1} ON {2}({3});",
                                           unique ? " UNIQUE " : " ",
                                           name,
                                           GetQuotedTableName(sourceMD.ModelName),
                                           GetQuotedColumnName(fieldName));
            return command;
        }


        protected virtual string FkOptionToString(OnFkOption option)
        {
            switch (option)
            {
                case OnFkOption.Cascade: return "CASCADE";
                case OnFkOption.NoAction: return "NO ACTION";
                case OnFkOption.SetNull: return "SET NULL";
                case OnFkOption.SetDefault: return "SET DEFAULT";
                case OnFkOption.Restrict:
                default: return "RESTRICT";
            }
        }


        public static ulong ConvertToULong(byte[] bytes)
        {
            Array.Reverse(bytes); //Correct Endianness
            var ulongValue = BitConverter.ToUInt64(bytes, 0);
            return ulongValue;
        }

        public virtual object ConvertDbValue(object value, Type type)
        {
            if (value == null || value is DBNull) return null;

            var strValue = value as string;
            if (strValue != null && OrmLiteConfig.StringFilter != null)
            {
                value = OrmLiteConfig.StringFilter(strValue);
            }

            if (value.GetType() == type)
            {
                return value;
            }

            if (type == typeof(DateTimeOffset))
            {
                if (strValue != null)
                {
                    var moment = DateTimeOffset.Parse(strValue, null, DateTimeStyles.RoundtripKind);
                    return moment;
                }
                if (value is DateTime)
                {
                    return new DateTimeOffset((DateTime)value);
                }
            }

            if (!type.IsEnum)
            {
                var typeCode = type.GetUnderlyingTypeCode();
                switch (typeCode)
                {
                    case TypeCode.Int16:
                        return value is short ? value : Convert.ToInt16(value);
                    case TypeCode.UInt16:
                        return value is ushort ? value : Convert.ToUInt16(value);
                    case TypeCode.Int32:
                        return value is int ? value : Convert.ToInt32(value);
                    case TypeCode.UInt32:
                        return value is uint ? value : Convert.ToUInt32(value);
                    case TypeCode.Int64:
                        return value is long ? value : Convert.ToInt64(value);
                    case TypeCode.UInt64:
                        if (value is ulong)
                            return value;
                        var byteValue = value as byte[];
                        if (byteValue != null)
                            return ConvertToULong(byteValue);
                        return Convert.ToUInt64(value);
                    case TypeCode.Single:
                        return value is float ? value : Convert.ToSingle(value);
                    case TypeCode.Double:
                        return value is double ? value : Convert.ToDouble(value);
                    case TypeCode.Decimal:
                        return value is decimal ? value : Convert.ToDecimal(value);
                }

                if (type == typeof(TimeSpan))
                {
                    var ticks = (long)value;
                    return TimeSpan.FromTicks(ticks);
                }
            }

            try
            {
                var convertedValue = StringSerializer.DeserializeFromString(value.ToString(), type);
                return convertedValue;
            }
            catch (Exception)
            {
                Log.ErrorFormat("Error ConvertDbValue trying to convert {0} into {1}", value, type.Name);
                throw;
            }
        }

        public virtual string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

            if (fieldType.IsRefType())
            {
                return GetQuotedValue(StringSerializer.SerializeToString(value));
            }

            if (fieldType.IsEnum)
            {
                var isEnumFlags = fieldType.IsEnumFlags();
                long enumValue;
                if (!isEnumFlags && Int64.TryParse(value.ToString(), out enumValue))
                {
                    value = Enum.ToObject(fieldType, enumValue).ToString();
                }

                var enumString = StringSerializer.SerializeToString(value);

                return !isEnumFlags
                    ? GetQuotedValue(enumString.Trim('"'))
                    : enumString;
            }

            var typeCode = fieldType.GetTypeCode();
            switch (typeCode)
            {
                case TypeCode.Single:
                    return ((float)value).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Double:
                    return ((double)value).ToString(CultureInfo.InvariantCulture);
                case TypeCode.Decimal:
                    return ((decimal)value).ToString(CultureInfo.InvariantCulture);

                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    if (fieldType.IsNumericType())
                    {
                        if (value is TimeSpan)
                            return ((TimeSpan)value).Ticks.ToString(CultureInfo.InvariantCulture);

                        return Convert.ChangeType(value, fieldType).ToString();                        
                    }
                    break;
            }

            if (fieldType == typeof(TimeSpan))
                return ((TimeSpan)value).Ticks.ToString(CultureInfo.InvariantCulture);
 
            return ShouldQuoteValue(fieldType)
                    ? GetQuotedValue(value.ToString())
                    : value.ToString();
        }

        public virtual string EscapeWildcards(string value)
        {
            if (value == null)
                return null;

            return value
                .Replace("^", @"^^")
                .Replace(@"\", @"^\")
                .Replace("_", @"^_")
                .Replace("%", @"^%");
        }

        public virtual string GetLoadChildrenSubSelect<From>(ModelDefinition modelDef, SqlExpression<From> expr)
        {
            expr.Select(this.GetQuotedColumnName(modelDef, modelDef.PrimaryKey));

            var subSql = expr.ToSelectStatement();

            return subSql;
        }

        //Async API's, should be overrided by Dialect Providers to use .ConfigureAwait(false)
        //Default impl below uses TaskAwaiter shim in async.cs

        public virtual Task OpenAsync(IDbConnection db, CancellationToken token = default(CancellationToken))
        {
            db.Open();
            return TaskResult.Finished;
        }

        public virtual Task<IDataReader> ExecuteReaderAsync(IDbCommand cmd, CancellationToken token = default(CancellationToken))
        {
            return cmd.ExecuteReader().InTask();
        }

        public virtual Task<int> ExecuteNonQueryAsync(IDbCommand cmd, CancellationToken token = default(CancellationToken))
        {
            return cmd.ExecuteNonQuery().InTask();
        }

        public virtual Task<object> ExecuteScalarAsync(IDbCommand cmd, CancellationToken token = default(CancellationToken))
        {
            return cmd.ExecuteScalar().InTask();
        }

        public virtual Task<bool> ReadAsync(IDataReader reader, CancellationToken token = default(CancellationToken))
        {
            return reader.Read().InTask();
        }

#if NET45
        public virtual async Task<List<T>> ReaderEach<T>(IDataReader reader, Func<T> fn, CancellationToken token = default(CancellationToken))
        {
            try
            {
                var to = new List<T>();
                while (await ReadAsync(reader, token))
                {
                    var row = fn();
                    to.Add(row);
                }
                return to;
            }
            finally
            {
                reader.Dispose();
            }
        }

        public virtual async Task<Return> ReaderEach<Return>(IDataReader reader, Action fn, Return source, CancellationToken token = default(CancellationToken))
        {
            try
            {
                while (await ReadAsync(reader, token))
                {
                    fn();
                }
                return source;
            }
            finally
            {
                reader.Dispose();
            }
        }

        public virtual async Task<T> ReaderRead<T>(IDataReader reader, Func<T> fn, CancellationToken token = default(CancellationToken))
        {
            try
            {
                if (await ReadAsync(reader, token))
                    return fn();

                return default(T);
            }
            finally 
            {
                reader.Dispose();
            }
        }

        public virtual Task<long> InsertAndGetLastInsertIdAsync<T>(IDbCommand dbCmd, CancellationToken token)
        {
            if (SelectIdentitySql == null)
                return new NotImplementedException("Returning last inserted identity is not implemented on this DB Provider.")
                    .InTask<long>();

            dbCmd.CommandText += "; " + SelectIdentitySql;

            return dbCmd.ExecLongScalarAsync(null, token);
        }
#else
        public Task<List<T>> ReaderEach<T>(IDataReader reader, Func<T> fn, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException(OrmLiteUtilExtensions.AsyncRequiresNet45Error);
        }

        public Task<Return> ReaderEach<Return>(IDataReader reader, Action fn, Return source, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException(OrmLiteUtilExtensions.AsyncRequiresNet45Error);
        }

        public Task<T> ReaderRead<T>(IDataReader reader, Func<T> fn, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException(OrmLiteUtilExtensions.AsyncRequiresNet45Error);
        }

        public Task<long> InsertAndGetLastInsertIdAsync<T>(IDbCommand dbCmd, CancellationToken token)
        {
            throw new NotImplementedException(OrmLiteUtilExtensions.AsyncRequiresNet45Error);
        }
#endif

    }
}
