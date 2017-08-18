//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2013 ServiceStack, Inc. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;
using ServiceStack.OrmLite.Converters;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public abstract class OrmLiteDialectProviderBase<TDialect>
        : IOrmLiteDialectProvider
        where TDialect : IOrmLiteDialectProvider
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(IOrmLiteDialectProvider));

        protected OrmLiteDialectProviderBase()
        {
            Variables = new Dictionary<string, string>();
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

        protected void InitColumnTypeMap()
        {
            EnumConverter = new EnumConverter();
            RowVersionConverter = new RowVersionConverter();
            ReferenceTypeConverter = new ReferenceTypeConverter();
            ValueTypeConverter = new ValueTypeConverter();

            RegisterConverter<string>(new StringConverter());
            RegisterConverter<char>(new CharConverter());
            RegisterConverter<char[]>(new CharArrayConverter());
            RegisterConverter<byte[]>(new ByteArrayConverter());

            RegisterConverter<byte>(new ByteConverter());
            RegisterConverter<sbyte>(new SByteConverter());
            RegisterConverter<short>(new Int16Converter());
            RegisterConverter<ushort>(new UInt16Converter());
            RegisterConverter<int>(new Int32Converter());
            RegisterConverter<uint>(new UInt32Converter());
            RegisterConverter<long>(new Int64Converter());
            RegisterConverter<ulong>(new UInt64Converter());

            RegisterConverter<ulong>(new UInt64Converter());

            RegisterConverter<float>(new FloatConverter());
            RegisterConverter<double>(new DoubleConverter());
            RegisterConverter<decimal>(new DecimalConverter());

            RegisterConverter<Guid>(new GuidConverter());
            RegisterConverter<TimeSpan>(new TimeSpanAsIntConverter());
            RegisterConverter<DateTime>(new DateTimeConverter());
            RegisterConverter<DateTimeOffset>(new DateTimeOffsetConverter());
        }

        public string GetColumnTypeDefinition(Type columnType, int? fieldLength, int? scale)
        {
            var converter = GetConverter(columnType);
            if (converter != null)
            {
                var customPrecisionConverter = converter as IHasColumnDefinitionPrecision;
                if (customPrecisionConverter != null)
                    return customPrecisionConverter.GetColumnDefinition(fieldLength, scale);

                var customLengthConverter = converter as IHasColumnDefinitionLength;
                if (customLengthConverter != null)
                    return customLengthConverter.GetColumnDefinition(fieldLength);

                if (string.IsNullOrEmpty(converter.ColumnDefinition))
                    throw new ArgumentException($"{converter.GetType().Name} requires a ColumnDefinition");

                return converter.ColumnDefinition;
            }

            var stringConverter = columnType.IsRefType()
                ? ReferenceTypeConverter
                : columnType.IsEnum()
                    ? EnumConverter
                    : (IHasColumnDefinitionLength)ValueTypeConverter;

            return stringConverter.GetColumnDefinition(fieldLength);
        }

        [Obsolete("Use GetConverter().DbType")]
        public virtual DbType GetColumnDbType(Type columnType)
        {
            var converter = GetConverterBestMatch(columnType);
            return converter.DbType;
        }

        public virtual void InitDbParam(IDbDataParameter dbParam, Type columnType)
        {
            var converter = GetConverterBestMatch(columnType);
            converter.InitDbParam(dbParam, columnType);
        }

        public abstract IDbDataParameter CreateParam();

        public Dictionary<string, string> Variables { get; set; }

        public IOrmLiteExecFilter ExecFilter { get; set; }

        public Dictionary<Type, IOrmLiteConverter> Converters = new Dictionary<Type, IOrmLiteConverter>();

        public string AutoIncrementDefinition = "AUTOINCREMENT"; //SqlServer express limit

        public DecimalConverter DecimalConverter => (DecimalConverter)Converters[typeof(decimal)];

        public StringConverter StringConverter => (StringConverter)Converters[typeof(string)];

        [Obsolete("Use GetStringConverter().UseUnicode")]
        public virtual bool UseUnicode
        {
            get => StringConverter.UseUnicode;
            set => StringConverter.UseUnicode = true;
        }

        [Obsolete("Use GetStringConverter().StringLength")]
        public int DefaultStringLength
        {
            get => StringConverter.StringLength;
            set => StringConverter.StringLength = value;
        }

        public string ParamString { get; set; } = "@";

        public INamingStrategy NamingStrategy { get; set; } = new OrmLiteNamingStrategyBase();

        public IStringSerializer StringSerializer { get; set; }

        private Func<string, string> paramNameFilter;
        public Func<string, string> ParamNameFilter
        {
            get => paramNameFilter ?? OrmLiteConfig.ParamNameFilter;
            set => paramNameFilter = value;
        }

        public string DefaultValueFormat = " DEFAULT ({0})";

        private EnumConverter enumConverter;
        public EnumConverter EnumConverter
        {
            get => enumConverter;
            set
            {
                value.DialectProvider = this;
                enumConverter = value;
            }
        }

        private RowVersionConverter rowVersionConverter;
        public RowVersionConverter RowVersionConverter
        {
            get => rowVersionConverter;
            set
            {
                value.DialectProvider = this;
                rowVersionConverter = value;
            }
        }

        private ReferenceTypeConverter referenceTypeConverter;
        public ReferenceTypeConverter ReferenceTypeConverter
        {
            get => referenceTypeConverter;
            set
            {
                value.DialectProvider = this;
                referenceTypeConverter = value;
            }
        }

        private ValueTypeConverter valueTypeConverter;
        public ValueTypeConverter ValueTypeConverter
        {
            get => valueTypeConverter;
            set
            {
                value.DialectProvider = this;
                valueTypeConverter = value;
            }
        }

        public void RegisterConverter<T>(IOrmLiteConverter converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            converter.DialectProvider = this;
            Converters[typeof(T)] = converter;
        }

        public IOrmLiteConverter GetConverter(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            return Converters.TryGetValue(type, out IOrmLiteConverter converter)
                ? converter
                : null;
        }

        public virtual bool ShouldQuoteValue(Type fieldType)
        {
            var converter = GetConverter(fieldType);
            return converter == null || converter is NativeValueOrmLiteConverter;
        }

        public virtual ulong FromDbRowVersion(object value)
        {
            return RowVersionConverter.FromDbRowVersion(value);
        }

        public IOrmLiteConverter GetConverterBestMatch(Type type)
        {
            var converter = GetConverter(type);
            if (converter != null)
                return converter;

            if (type.IsEnum())
                return EnumConverter;

            return type.IsRefType()
                ? (IOrmLiteConverter)ReferenceTypeConverter
                : ValueTypeConverter;
        }

        public virtual IOrmLiteConverter GetConverterBestMatch(FieldDefinition fieldDef)
        {
            var fieldType = Nullable.GetUnderlyingType(fieldDef.FieldType) ?? fieldDef.FieldType;

            if (fieldDef.IsRowVersion)
                return RowVersionConverter;

            IOrmLiteConverter converter;

            if (Converters.TryGetValue(fieldType, out converter))
                return converter;

            if (fieldType.IsEnum())
                return EnumConverter;

            return fieldType.IsRefType()
                ? (IOrmLiteConverter)ReferenceTypeConverter
                : ValueTypeConverter;
        }

        public virtual object ToDbValue(object value, Type type)
        {
            if (value == null || value is DBNull)
                return null;

            var converter = GetConverterBestMatch(type);
            try
            {
                return converter.ToDbValue(type, value);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in {converter.GetType().Name}.ToDbValue() value '{value.GetType().Name}' and Type '{type.Name}'", ex);
                throw;
            }
        }

        public virtual object FromDbValue(object value, Type type)
        {
            if (value == null || value is DBNull)
                return null;

            var converter = GetConverterBestMatch(type);
            try
            {
                return converter.FromDbValue(type, value);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in {converter.GetType().Name}.FromDbValue() value '{value.GetType().Name}' and Type '{type.Name}'", ex);
                throw;
            }
        }

        public object GetValue(IDataReader reader, int columnIndex, Type type)
        {
            IOrmLiteConverter converter;
            if (Converters.TryGetValue(type, out converter))
                return converter.GetValue(reader, columnIndex, null);

            return reader.GetValue(columnIndex);
        }

        public virtual int GetValues(IDataReader reader, object[] values)
        {
            return reader.GetValues(values);
        }

        public abstract IDbConnection CreateConnection(string filePath, Dictionary<string, string> options);

        public virtual string GetQuotedValue(string paramValue)
        {
            return "'" + paramValue.Replace("'", "''") + "'";
        }

        public virtual string GetSchemaName(string schema)
        {
            return NamingStrategy.GetSchemaName(schema);
        }

        public virtual string GetTableName(ModelDefinition modelDef)
        {
            return GetTableName(modelDef.ModelName, modelDef.Schema);
        }

        public virtual string GetTableName(string table, string schema = null)
        {
            return schema != null
                ? $"{NamingStrategy.GetSchemaName(schema)}.{NamingStrategy.GetTableName(table)}"
                : NamingStrategy.GetTableName(table);
        }

        public virtual string GetQuotedTableName(ModelDefinition modelDef)
        {
            return GetQuotedTableName(modelDef.ModelName, modelDef.Schema);
        }

        public virtual string GetQuotedTableName(string tableName, string schema = null)
        {
            if (schema == null)
                return GetQuotedName(NamingStrategy.GetTableName(tableName));

            var escapedSchema = NamingStrategy.GetSchemaName(schema)
                .Replace(".", "\".\"");

            return GetQuotedName(escapedSchema)
                + "."
                + GetQuotedName(NamingStrategy.GetTableName(tableName));
        }

        public virtual string GetQuotedColumnName(string columnName)
        {
            return GetQuotedName(NamingStrategy.GetColumnName(columnName));
        }

        public virtual string GetQuotedName(string name)
        {
            return $"\"{name}\"";
        }

        public virtual string SanitizeFieldNameForParamName(string fieldName)
        {
            return OrmLiteConfig.SanitizeFieldNameForParamNameFn(fieldName);
        }

        public virtual string GetColumnDefinition(FieldDefinition fieldDef)
        {
            var fieldDefinition = fieldDef.CustomFieldDefinition ?? 
                GetColumnTypeDefinition(fieldDef.ColumnType, fieldDef.FieldLength, fieldDef.Scale);

            var sql = StringBuilderCache.Allocate();
            sql.Append($"{GetQuotedColumnName(fieldDef.FieldName)} {fieldDefinition}");

            if (fieldDef.IsPrimaryKey)
            {
                sql.Append(" PRIMARY KEY");
                if (fieldDef.AutoIncrement)
                {
                    sql.Append(" ").Append(AutoIncrementDefinition);
                }
            }
            else
            {
                sql.Append(fieldDef.IsNullable ? " NULL" : " NOT NULL");
            }

            var defaultValue = GetDefaultValue(fieldDef);
            if (!string.IsNullOrEmpty(defaultValue))
            {
                sql.AppendFormat(DefaultValueFormat, defaultValue);
            }

            return StringBuilderCache.ReturnAndFree(sql);
        }

        [Obsolete("Use GetColumnDefinition(fieldDef)")]
        public string GetColumnDefinition(string fieldName, Type fieldType,
            bool isPrimaryKey, bool autoIncrement, bool isNullable, bool isRowVersion,
            int? fieldLength, int? scale, string defaultValue, string customFieldDefinition)
        {
            return GetColumnDefinition(new FieldDefinition
            {
                Name = fieldName,
                FieldType = fieldType,
                IsPrimaryKey = isPrimaryKey,
                AutoIncrement = autoIncrement,
                IsNullable = isNullable,
                IsRowVersion = isRowVersion,
                FieldLength = fieldLength,
                Scale = scale,
                DefaultValue = defaultValue,
                CustomFieldDefinition = customFieldDefinition,
            });
        }

        public virtual string SelectIdentitySql { get; set; }

        public virtual long GetLastInsertId(IDbCommand dbCmd)
        {
            if (SelectIdentitySql == null)
                throw new NotImplementedException("Returning last inserted identity is not implemented on this DB Provider.");

            dbCmd.CommandText = SelectIdentitySql;
            return dbCmd.ExecLongScalar();
        }

        [Obsolete("Use GetLastInsertIdSqlSuffix()")]
        public virtual long InsertAndGetLastInsertId<T>(IDbCommand dbCmd)
        {
            dbCmd.CommandText += GetLastInsertIdSqlSuffix<T>();
            return dbCmd.ExecLongScalar();
        }

        public virtual string GetLastInsertIdSqlSuffix<T>()
        {
            if (SelectIdentitySql == null)
                throw new NotImplementedException("Returning last inserted identity is not implemented on this DB Provider.");

            return "; " + SelectIdentitySql;
        }

        // Fmt
        public virtual string ToSelectStatement(Type tableType, string sqlFilter, params object[] filterParams)
        {
            const string SelectStatement = "SELECT";
            var isFullSelectStatement =
                !string.IsNullOrEmpty(sqlFilter)
                && sqlFilter.TrimStart().StartsWith(SelectStatement, StringComparison.OrdinalIgnoreCase);

            if (isFullSelectStatement)
                return sqlFilter.SqlFmt(this, filterParams);

            var modelDef = tableType.GetModelDefinition();
            var sql = StringBuilderCache.Allocate();
            sql.Append($"SELECT {GetColumnNames(modelDef)} FROM {GetQuotedTableName(modelDef)}");

            if (string.IsNullOrEmpty(sqlFilter))
                return StringBuilderCache.ReturnAndFree(sql);

            sqlFilter = sqlFilter.SqlFmt(this, filterParams);
            if (!sqlFilter.StartsWith("ORDER ", StringComparison.OrdinalIgnoreCase)
                && !sqlFilter.StartsWith("LIMIT ", StringComparison.OrdinalIgnoreCase))
            {
                sql.Append(" WHERE ");
            }

            sql.Append(sqlFilter);

            return StringBuilderCache.ReturnAndFree(sql);
        }

        public virtual string ToSelectStatement(ModelDefinition modelDef,
            string selectExpression,
            string bodyExpression,
            string orderByExpression = null,
            int? offset = null,
            int? rows = null)
        {

            var sb = StringBuilderCache.Allocate();
            sb.Append(selectExpression);
            sb.Append(bodyExpression);
            if (orderByExpression != null)
            {
                sb.Append(orderByExpression);
            }

            if (offset != null || rows != null)
            {
                sb.Append("\n");
                sb.Append(SqlLimit(offset, rows));
            }

            return StringBuilderCache.ReturnAndFree(sb);
        }

        public virtual SelectItem GetRowVersionColumnName(FieldDefinition field, string tablePrefix = null)
        {
            return new SelectItemColumn(this, field.FieldName, null, tablePrefix);
        }

        public virtual string GetColumnNames(ModelDefinition modelDef)
        {
            return GetColumnNames(modelDef, false).ToSelectString();
        }

        public virtual SelectItem[] GetColumnNames(ModelDefinition modelDef, bool tableQualified)
        {
            var tablePrefix = tableQualified ? GetQuotedTableName(modelDef) : "";

            var sqlColumns = new SelectItem[modelDef.FieldDefinitions.Count];
            for (var i = 0; i < sqlColumns.Length; ++i)
            {
                var field = modelDef.FieldDefinitions[i];

                if (field.CustomSelect != null)
                {
                    sqlColumns[i] = new SelectItemExpression(this, field.CustomSelect, field.FieldName);
                }
                else if (field.IsRowVersion)
                {
                    sqlColumns[i] = GetRowVersionColumnName(field, tablePrefix);
                }
                else
                {
                    sqlColumns[i] = new SelectItemColumn(this, field.FieldName, null, tablePrefix);
                }
            }

            return sqlColumns;
        }

        public virtual string ToInsertRowStatement(IDbCommand cmd, object objWithProperties, ICollection<string> insertFields = null)
        {
            if (insertFields == null)
                insertFields = new List<string>();

            var sbColumnNames = StringBuilderCache.Allocate();
            var sbColumnValues = StringBuilderCacheAlt.Allocate();
            var modelDef = objWithProperties.GetType().GetModelDefinition();

            foreach (var fieldDef in modelDef.FieldDefinitionsArray)
            {
                if (fieldDef.ShouldSkipInsert())
                    continue;

                if (insertFields.Count > 0 && !insertFields.Contains(fieldDef.Name, StringComparer.OrdinalIgnoreCase))
                    continue;

                if (sbColumnNames.Length > 0)
                    sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0)
                    sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(GetQuotedColumnName(fieldDef.FieldName));
                    sbColumnValues.Append(this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName)));

                    var p = AddParameter(cmd, fieldDef);
                    p.Value = fieldDef.GetValue(objWithProperties) ?? DBNull.Value;
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToInsertRowStatement(): " + ex.Message, ex);
                    throw;
                }
            }

            var sql = $"INSERT INTO {GetQuotedTableName(modelDef)} ({StringBuilderCache.ReturnAndFree(sbColumnNames)}) " +
                      $"VALUES ({StringBuilderCacheAlt.ReturnAndFree(sbColumnValues)})";

            return sql;
        }

        public virtual string ToInsertStatement<T>(IDbCommand dbCmd, T item, ICollection<string> insertFields = null)
        {
            dbCmd.Parameters.Clear();
            var dialectProvider = dbCmd.GetDialectProvider();
            dialectProvider.PrepareParameterizedInsertStatement<T>(dbCmd);

            if (string.IsNullOrEmpty(dbCmd.CommandText))
                return null;

            dialectProvider.SetParameterValues<T>(dbCmd, item);

            return MergeParamsIntoSql(dbCmd.CommandText, ToArray(dbCmd.Parameters));
        }

        public virtual void PrepareParameterizedInsertStatement<T>(IDbCommand cmd, ICollection<string> insertFields = null)
        {
            var sbColumnNames = StringBuilderCache.Allocate();
            var sbColumnValues = StringBuilderCacheAlt.Allocate();
            var modelDef = typeof(T).GetModelDefinition();

            cmd.Parameters.Clear();

            foreach (var fieldDef in modelDef.FieldDefinitionsArray)
            {
                if (fieldDef.ShouldSkipInsert())
                    continue;

                //insertFields contains Property "Name" of fields to insert ( that's how expressions work )
                if (insertFields != null && !insertFields.Contains(fieldDef.Name, StringComparer.OrdinalIgnoreCase))
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

            cmd.CommandText = $"INSERT INTO {GetQuotedTableName(modelDef)} ({StringBuilderCache.ReturnAndFree(sbColumnNames)}) " +
                              $"VALUES ({StringBuilderCacheAlt.ReturnAndFree(sbColumnValues)})";
        }

        public virtual void PrepareInsertRowStatement<T>(IDbCommand dbCmd, Dictionary<string, object> args)
        {
            var sbColumnNames = StringBuilderCache.Allocate();
            var sbColumnValues = StringBuilderCacheAlt.Allocate();
            var modelDef = typeof(T).GetModelDefinition();

            dbCmd.Parameters.Clear();

            foreach (var entry in args)
            {
                var fieldDef = modelDef.GetFieldDefinition(entry.Key);
                if (fieldDef.ShouldSkipInsert())
                    continue;

                var value = entry.Value;

                if (sbColumnNames.Length > 0)
                    sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0)
                    sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(GetQuotedColumnName(fieldDef.FieldName));
                    sbColumnValues.Append(this.AddParam(dbCmd, value, fieldDef).ParameterName);
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in PrepareInsertRowStatement(): " + ex.Message, ex);
                    throw;
                }
            }

            dbCmd.CommandText = $"INSERT INTO {GetQuotedTableName(modelDef)} ({StringBuilderCache.ReturnAndFree(sbColumnNames)}) " +
                                $"VALUES ({StringBuilderCacheAlt.ReturnAndFree(sbColumnValues)})";
        }

        public virtual string ToUpdateStatement<T>(IDbCommand dbCmd, T item, ICollection<string> updateFields = null)
        {
            dbCmd.Parameters.Clear();
            var dialectProvider = dbCmd.GetDialectProvider();
            dialectProvider.PrepareParameterizedUpdateStatement<T>(dbCmd);

            if (string.IsNullOrEmpty(dbCmd.CommandText))
                return null;

            dialectProvider.SetParameterValues<T>(dbCmd, item);

            return MergeParamsIntoSql(dbCmd.CommandText, ToArray(dbCmd.Parameters));
        }

        IDbDataParameter[] ToArray(IDataParameterCollection dbParams)
        {
            var to = new IDbDataParameter[dbParams.Count];
            for (int i = 0; i < dbParams.Count; i++)
            {
                to[i] = (IDbDataParameter)dbParams[i];
            }
            return to;
        }

        public virtual string MergeParamsIntoSql(string sql, IEnumerable<IDbDataParameter> dbParams)
        {
            foreach (var dbParam in dbParams)
            {
                var quotedValue = dbParam.Value != null
                    ? GetQuotedValue(dbParam.Value, dbParam.Value.GetType())
                    : "null";

                var pattern = dbParam.ParameterName + @"(,|\s|\)|$)";
                var replacement = quotedValue.Replace("$", "$$") + "$1";
                sql = Regex.Replace(sql, pattern, replacement);
            }
            return sql;
        }

        public virtual bool PrepareParameterizedUpdateStatement<T>(IDbCommand cmd, ICollection<string> updateFields = null)
        {
            var sql = StringBuilderCache.Allocate();
            var sqlFilter = StringBuilderCacheAlt.Allocate();
            var modelDef = typeof(T).GetModelDefinition();
            var hadRowVesion = false;
            var updateAllFields = updateFields == null || updateFields.Count == 0;

            cmd.Parameters.Clear();

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

                    if (!updateAllFields && !updateFields.Contains(fieldDef.Name, StringComparer.OrdinalIgnoreCase))
                        continue;

                    if (sql.Length > 0)
                        sql.Append(", ");

                    sql
                        .Append(GetQuotedColumnName(fieldDef.FieldName))
                        .Append("=")
                        .Append(this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName)));

                    AddParameter(cmd, fieldDef);
                }
                catch (Exception ex)
                {
                    OrmLiteUtils.HandleException(ex, "ERROR in PrepareParameterizedUpdateStatement(): " + ex.Message);
                }
            }

            if (sql.Length > 0)
            {
                var strFilter = StringBuilderCacheAlt.ReturnAndFree(sqlFilter);
                cmd.CommandText = $"UPDATE {GetQuotedTableName(modelDef)} " +
                                  $"SET {StringBuilderCache.ReturnAndFree(sql)} {(strFilter.Length > 0 ? "WHERE " + strFilter : "")}";
            }

            return hadRowVesion;
        }

        public virtual void AppendNullFieldCondition(StringBuilder sqlFilter, FieldDefinition fieldDef)
        {
            sqlFilter
                .Append(GetQuotedColumnName(fieldDef.FieldName))
                .Append(" IS NULL");
        }

        public virtual void AppendFieldCondition(StringBuilder sqlFilter, FieldDefinition fieldDef, IDbCommand cmd)
        {
            sqlFilter
                .Append(GetQuotedColumnName(fieldDef.FieldName))
                .Append("=")
                .Append(this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName)));

            AddParameter(cmd, fieldDef);
        }

        public virtual bool PrepareParameterizedDeleteStatement<T>(IDbCommand cmd, IDictionary<string, object> deleteFields)
        {
            if (deleteFields == null || deleteFields.Count == 0)
                throw new ArgumentException("DELETE's must have at least 1 criteria");

            var sqlFilter = StringBuilderCache.Allocate();
            var modelDef = typeof(T).GetModelDefinition();
            var hadRowVesion = false;

            cmd.Parameters.Clear();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.ShouldSkipDelete())
                    continue;

                object fieldValue;

                if (!deleteFields.TryGetValue(fieldDef.Name, out fieldValue))
                    continue;

                if (fieldDef.IsRowVersion)
                    hadRowVesion = true;

                try
                {
                    if (sqlFilter.Length > 0)
                        sqlFilter.Append(" AND ");

                    if (fieldValue != null)
                    {
                        AppendFieldCondition(sqlFilter, fieldDef, cmd);
                    }
                    else
                    {
                        AppendNullFieldCondition(sqlFilter, fieldDef);
                    }
                }
                catch (Exception ex)
                {
                    OrmLiteUtils.HandleException(ex, "ERROR in PrepareParameterizedDeleteStatement(): " + ex.Message);
                }
            }

            cmd.CommandText = $"DELETE FROM {GetQuotedTableName(modelDef)} WHERE {StringBuilderCache.ReturnAndFree(sqlFilter)}";

            return hadRowVesion;
        }

        public virtual void PrepareStoredProcedureStatement<T>(IDbCommand cmd, T obj)
        {
            cmd.CommandText = ToExecuteProcedureStatement(obj);
            cmd.CommandType = CommandType.StoredProcedure;
        }

        protected IDbDataParameter AddParameter(IDbCommand cmd, FieldDefinition fieldDef)
        {
            var p = cmd.CreateParameter();
            SetParameter(fieldDef, p);
            cmd.Parameters.Add(p);
            return p;
        }

        public virtual void SetParameter(FieldDefinition fieldDef, IDbDataParameter p)
        {
            p.ParameterName = this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName));
            InitDbParam(p, fieldDef.ColumnType);
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
                {
                    if (ParamNameFilter != null)
                    {
                        fieldDef = modelDef.GetFieldDefinition(name => 
                            string.Equals(ParamNameFilter(name), fieldName, StringComparison.OrdinalIgnoreCase));
                    }

                    if (fieldDef == null)
                        throw new ArgumentException($"Field Definition '{fieldName}' was not found");
                }

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
               : GetAnonValue(fieldDef, obj);

            return GetFieldValue(fieldDef, value);
        }

        public object GetFieldValue(FieldDefinition fieldDef, object value)
        {
            if (value == null)
                return null;

            var converter = GetConverterBestMatch(fieldDef);
            try
            {
                return converter.ToDbValue(fieldDef.FieldType, value);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in {converter.GetType().Name}.ToDbValue() for field '{fieldDef.Name}' of Type '{fieldDef.FieldType}' with value '{value.GetType().Name}'", ex);
                throw;
            }
        }

        public object GetFieldValue(Type fieldType, object value)
        {
            if (value == null)
                return null;

            var converter = GetConverterBestMatch(fieldType);
            try
            {
                return converter.ToDbValue(fieldType, value);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in {converter.GetType().Name}.ToDbValue() for field of Type '{fieldType}' with value '{value.GetType().Name}'", ex);
                throw;
            }
        }

        protected virtual object GetValueOrDbNull<T>(FieldDefinition fieldDef, object obj)
        {
            var value = GetValue<T>(fieldDef, obj);
            if (value == null)
                return DBNull.Value;

            return value;
        }

        protected virtual object GetQuotedValueOrDbNull<T>(FieldDefinition fieldDef, object obj)
        {
            var value = obj is T
                ? fieldDef.GetValue(obj)
                : GetAnonValue(fieldDef, obj);

            if (value == null)
                return DBNull.Value;

            var unquotedVal = GetQuotedValue(value, fieldDef.FieldType)
                .TrimStart('\'').TrimEnd('\''); ;

            if (string.IsNullOrEmpty(unquotedVal))
                return DBNull.Value;

            return unquotedVal;
        }

        static readonly ConcurrentDictionary<string, GetMemberDelegate> anonValueFnMap =
            new ConcurrentDictionary<string, GetMemberDelegate>();

        protected virtual object GetAnonValue(FieldDefinition fieldDef, object obj)
        {
            var anonType = obj.GetType();
            var key = anonType.Name + "." + fieldDef.Name;

            var factoryFn = (Func<string, GetMemberDelegate>)(_ =>
                anonType.GetProperty(fieldDef.Name).CreateGetter());

            var getterFn = anonValueFnMap.GetOrAdd(key, factoryFn);

            return getterFn(obj);
        }

        public virtual void PrepareUpdateRowStatement(IDbCommand dbCmd, object objWithProperties, ICollection<string> updateFields = null)
        {
            var sql = StringBuilderCache.Allocate();
            var sqlFilter = StringBuilderCacheAlt.Allocate();
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

                        sqlFilter
                            .Append(GetQuotedColumnName(fieldDef.FieldName))
                            .Append("=")
                            .Append(this.AddParam(dbCmd, fieldDef.GetValue(objWithProperties), fieldDef).ParameterName);

                        continue;
                    }

                    if (!updateAllFields && !updateFields.Contains(fieldDef.Name, StringComparer.OrdinalIgnoreCase) || fieldDef.AutoIncrement)
                        continue;

                    if (sql.Length > 0)
                        sql.Append(", ");

                    sql
                        .Append(GetQuotedColumnName(fieldDef.FieldName))
                        .Append("=")
                        .Append(this.AddParam(dbCmd, fieldDef.GetValue(objWithProperties), fieldDef).ParameterName);
                }
                catch (Exception ex)
                {
                    OrmLiteUtils.HandleException(ex, "ERROR in ToUpdateRowStatement(): " + ex.Message);
                }
            }

            var strFilter = StringBuilderCacheAlt.ReturnAndFree(sqlFilter);
            dbCmd.CommandText = $"UPDATE {GetQuotedTableName(modelDef)} " +
                                $"SET {StringBuilderCache.ReturnAndFree(sql)}{(strFilter.Length > 0 ? " WHERE " + strFilter : "")}";

            if (sql.Length == 0)
                throw new Exception("No valid update properties provided (e.g. p => p.FirstName): " + dbCmd.CommandText);
        }

        public virtual void PrepareUpdateRowStatement<T>(IDbCommand dbCmd, Dictionary<string, object> args, string sqlFilter)
        {
            var sql = StringBuilderCache.Allocate();
            var modelDef = typeof(T).GetModelDefinition();

            foreach (var entry in args)
            {
                var fieldDef = modelDef.GetFieldDefinition(entry.Key);
                if (fieldDef.ShouldSkipUpdate() || fieldDef.AutoIncrement)
                    continue;

                var value = entry.Value;

                try
                {
                    if (sql.Length > 0)
                        sql.Append(", ");

                    sql
                        .Append(GetQuotedColumnName(fieldDef.FieldName))
                        .Append("=")
                        .Append(this.AddParam(dbCmd, value, fieldDef).ParameterName);
                }
                catch (Exception ex)
                {
                    OrmLiteUtils.HandleException(ex, "ERROR in PrepareUpdateRowStatement(cmd,args): " + ex.Message);
                }
            }

            dbCmd.CommandText = $"UPDATE {GetQuotedTableName(modelDef)} " +
                                $"SET {StringBuilderCache.ReturnAndFree(sql)}{(string.IsNullOrEmpty(sqlFilter) ? "" : " ")}{sqlFilter}";

            if (sql.Length == 0)
                throw new Exception("No valid update properties provided (e.g. () => new Person { Age = 27 }): " + dbCmd.CommandText);
        }

        public virtual void PrepareUpdateRowAddStatement<T>(IDbCommand dbCmd, Dictionary<string, object> args, string sqlFilter)
        {
            var sql = StringBuilderCache.Allocate();
            var modelDef = typeof(T).GetModelDefinition();

            foreach (var entry in args)
            {
                var fieldDef = modelDef.GetFieldDefinition(entry.Key);
                if (fieldDef.ShouldSkipUpdate() || fieldDef.AutoIncrement || fieldDef.IsPrimaryKey ||
                    fieldDef.IsRowVersion || fieldDef.Name == OrmLiteConfig.IdField)
                    continue;

                var value = entry.Value;

                try
                {
                    if (sql.Length > 0)
                        sql.Append(", ");

                    var quotedFieldName = GetQuotedColumnName(fieldDef.FieldName);

                    if (fieldDef.FieldType.IsNumericType())
                    {
                        sql
                            .Append(quotedFieldName)
                            .Append("=")
                            .Append(quotedFieldName)
                            .Append("+")
                            .Append(this.AddParam(dbCmd, value, fieldDef).ParameterName);
                    }
                    else
                    {
                        sql
                            .Append(quotedFieldName)
                            .Append("=")
                            .Append(this.AddParam(dbCmd, value, fieldDef).ParameterName);
                    }
                }
                catch (Exception ex)
                {
                    OrmLiteUtils.HandleException(ex, "ERROR in PrepareUpdateRowAddStatement(): " + ex.Message);
                }
            }

            dbCmd.CommandText = $"UPDATE {GetQuotedTableName(modelDef)} " +
                                $"SET {StringBuilderCache.ReturnAndFree(sql)}{(string.IsNullOrEmpty(sqlFilter) ? "" : " ")}{sqlFilter}";

            if (sql.Length == 0)
                throw new Exception("No valid update properties provided (e.g. () => new Person { Age = 27 }): " + dbCmd.CommandText);
        }

        public virtual string ToDeleteStatement(Type tableType, string sqlFilter, params object[] filterParams)
        {
            var sql = StringBuilderCache.Allocate();
            const string deleteStatement = "DELETE ";

            var isFullDeleteStatement =
                !string.IsNullOrEmpty(sqlFilter)
                && sqlFilter.Length > deleteStatement.Length
                && sqlFilter.Substring(0, deleteStatement.Length).ToUpper().Equals(deleteStatement);

            if (isFullDeleteStatement)
                return sqlFilter.SqlFmt(this, filterParams);

            var modelDef = tableType.GetModelDefinition();
            sql.Append($"DELETE FROM {GetQuotedTableName(modelDef)}");

            if (string.IsNullOrEmpty(sqlFilter))
                return StringBuilderCache.ReturnAndFree(sql);

            sqlFilter = sqlFilter.SqlFmt(this, filterParams);
            sql.Append(" WHERE ");
            sql.Append(sqlFilter);

            return StringBuilderCache.ReturnAndFree(sql);
        }

        public string GetDefaultValue(Type tableType, string fieldName)
        {
            var modelDef = tableType.GetModelDefinition();
            var fieldDef = modelDef.GetFieldDefinition(fieldName);
            return GetDefaultValue(fieldDef);
        }

        public virtual string GetDefaultValue(FieldDefinition fieldDef)
        {
            var defaultValue = fieldDef.DefaultValue;
            if (string.IsNullOrEmpty(defaultValue))
                return null;

            if (!defaultValue.StartsWith("{"))
                return defaultValue;

            string variable;
            return Variables.TryGetValue(defaultValue, out variable)
                ? variable
                : null;
        }

        public virtual string ToCreateTableStatement(Type tableType)
        {
            var sbColumns = StringBuilderCache.Allocate();
            var sbConstraints = StringBuilderCacheAlt.Allocate();

            var modelDef = tableType.GetModelDefinition();
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.CustomSelect != null)
                    continue;

                var columnDefinition = GetColumnDefinition(fieldDef);

                if (columnDefinition == null)
                    continue;

                if (sbColumns.Length != 0)
                    sbColumns.Append(", \n  ");

                sbColumns.Append(columnDefinition);

                var sqlConstraint = GetCheckConstraint(fieldDef);
                if (sqlConstraint != null)
                {
                    sbConstraints.Append(",\n" + sqlConstraint);
                }

                if (fieldDef.ForeignKey == null || OrmLiteConfig.SkipForeignKeys)
                    continue;

                var refModelDef = fieldDef.ForeignKey.ReferenceType.GetModelDefinition();
                sbConstraints.Append(
                    $", \n\n  CONSTRAINT {GetQuotedName(fieldDef.ForeignKey.GetForeignKeyName(modelDef, refModelDef, NamingStrategy, fieldDef))} " +
                    $"FOREIGN KEY ({GetQuotedColumnName(fieldDef.FieldName)}) " +
                    $"REFERENCES {GetQuotedTableName(refModelDef)} ({GetQuotedColumnName(refModelDef.PrimaryKey.FieldName)})");

                sbConstraints.Append(GetForeignKeyOnDeleteClause(fieldDef.ForeignKey));
                sbConstraints.Append(GetForeignKeyOnUpdateClause(fieldDef.ForeignKey));
            }
            var sql = $"CREATE TABLE {GetQuotedTableName(modelDef)} " +
                      $"\n(\n  {StringBuilderCache.ReturnAndFree(sbColumns)}{StringBuilderCacheAlt.ReturnAndFree(sbConstraints)} \n); \n";

            return sql;
        }

        public virtual string GetCheckConstraint(FieldDefinition fieldDef)
        {
            if (fieldDef.CheckConstraint == null)
                return null;

            return $"CONSTRAINT CHK_{fieldDef.FieldName} CHECK ({fieldDef.CheckConstraint})";
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

                var sb = StringBuilderCache.Allocate();
                foreach (var fieldName in compositeIndex.FieldNames)
                {
                    if (sb.Length > 0)
                        sb.Append(", ");

                    var parts = fieldName.SplitOnLast(' ');
                    if (parts.Length == 2 && (parts[1].ToLower().StartsWith("desc") || parts[1].ToLower().StartsWith("asc")))
                    {
                        sb.Append(GetQuotedColumnName(parts[0]))
                          .Append(' ')
                          .Append(parts[1]);
                    }
                    else
                    {
                        sb.Append(GetQuotedColumnName(fieldName));
                    }
                }

                sqlIndexes.Add(
                    ToCreateIndexStatement(compositeIndex.Unique, indexName, modelDef,
                    StringBuilderCache.ReturnAndFree(sb),
                    isCombined: true));
            }

            return sqlIndexes;
        }

        public virtual bool DoesTableExist(IDbConnection db, string tableName, string schema = null)
        {
            return db.Exec(dbCmd => DoesTableExist(dbCmd, tableName, schema));
        }

        public virtual bool DoesTableExist(IDbCommand dbCmd, string tableName, string schema = null)
        {
            throw new NotImplementedException();
        }

        public virtual bool DoesColumnExist(IDbConnection db, string columnName, string tableName, string schema = null)
        {
            throw new NotImplementedException();
        }

        public virtual bool DoesSequenceExist(IDbCommand dbCmd, string sequenceName)
        {
            throw new NotImplementedException();
        }

        protected virtual string GetIndexName(bool isUnique, string modelName, string fieldName)
        {
            return $"{(isUnique ? "u" : "")}idx_{modelName}_{fieldName}".ToLower();
        }

        protected virtual string GetCompositeIndexName(CompositeIndexAttribute compositeIndex, ModelDefinition modelDef)
        {
            return compositeIndex.Name ?? GetIndexName(compositeIndex.Unique, modelDef.ModelName.SafeVarName(),
                string.Join("_", compositeIndex.FieldNames.Map(x => x.LeftPart(' ')).ToArray()));
        }

        protected virtual string GetCompositeIndexNameWithSchema(CompositeIndexAttribute compositeIndex, ModelDefinition modelDef)
        {
            return compositeIndex.Name ?? GetIndexName(compositeIndex.Unique,
                    (modelDef.IsInSchema
                        ? modelDef.Schema + "_" + GetQuotedTableName(modelDef)
                        : GetQuotedTableName(modelDef)).SafeVarName(),
                    string.Join("_", compositeIndex.FieldNames.ToArray()));
        }

        protected virtual string ToCreateIndexStatement(bool isUnique, string indexName, ModelDefinition modelDef, string fieldName,
            bool isCombined = false, FieldDefinition fieldDef = null)
        {
            return $"CREATE {(isUnique ? "UNIQUE" : "")}" +
                   (fieldDef?.IsClustered == true ? " CLUSTERED" : "") +
                   (fieldDef?.IsNonClustered == true ? " NONCLUSTERED" : "") +
                   $" INDEX {indexName} ON {GetQuotedTableName(modelDef)} " +
                   $"({(isCombined ? fieldName : GetQuotedColumnName(fieldName))}); \n";
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

        public virtual string ToAddColumnStatement(Type modelType, FieldDefinition fieldDef)
        {
            var column = GetColumnDefinition(fieldDef);
            return $"ALTER TABLE {GetQuotedTableName(modelType.GetModelDefinition().ModelName)} ADD COLUMN {column};";
        }

        public virtual string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef)
        {
            var column = GetColumnDefinition(fieldDef);
            return $"ALTER TABLE {GetQuotedTableName(modelType.GetModelDefinition().ModelName)} MODIFY COLUMN {column};";
        }

        public virtual string ToChangeColumnNameStatement(Type modelType, FieldDefinition fieldDef, string oldColumnName)
        {
            var column = GetColumnDefinition(fieldDef);
            return $"ALTER TABLE {GetQuotedTableName(modelType.GetModelDefinition().ModelName)} CHANGE COLUMN {GetQuotedColumnName(oldColumnName)} {column};";
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

            return $"ALTER TABLE {GetQuotedTableName(sourceMD.ModelName)} " +
                   $"ADD CONSTRAINT {name} FOREIGN KEY ({GetQuotedColumnName(fieldName)}) " +
                   $"REFERENCES {GetQuotedTableName(referenceMD.ModelName)} " +
                   $"({GetQuotedColumnName(referenceFieldName)})" +
                   $"{GetForeignKeyOnDeleteClause(new ForeignKeyConstraint(typeof(T), onDelete: FkOptionToString(onDelete)))}" +
                   $"{GetForeignKeyOnUpdateClause(new ForeignKeyConstraint(typeof(T), onUpdate: FkOptionToString(onUpdate)))};";
        }

        public virtual string ToCreateIndexStatement<T>(Expression<Func<T, object>> field, string indexName = null, bool unique = false)
        {
            var sourceDef = ModelDefinition<T>.Definition;
            var fieldName = sourceDef.GetFieldDefinition(field).FieldName;

            string name = GetQuotedName(indexName.IsNullOrEmpty() ?
                (unique ? "uidx" : "idx") + "_" + sourceDef.ModelName + "_" + fieldName :
                indexName);

            string command = $"CREATE {(unique ? "UNIQUE" : "")} " +
                             $"INDEX {name} ON {GetQuotedTableName(sourceDef.ModelName)}" +
                             $"({GetQuotedColumnName(fieldName)});";
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

        public virtual string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

            var converter = value.GetType().IsEnum()
                ? EnumConverter
                : GetConverterBestMatch(fieldType);
            try
            {
                return converter.ToQuotedString(fieldType, value);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in {converter.GetType().Name}.ToQuotedString() value '{converter.GetType().Name}' and Type '{value.GetType().Name}'", ex);
                throw;
            }
        }

        public virtual object GetParamValue(object value, Type fieldType)
        {
            return ToDbValue(value, fieldType);
        }

        public virtual string EscapeWildcards(string value)
        {
            return value?.Replace("^", @"^^")
                .Replace(@"\", @"^\")
                .Replace("_", @"^_")
                .Replace("%", @"^%");
        }

        public virtual string GetLoadChildrenSubSelect<From>(SqlExpression<From> expr)
        {
            var modelDef = expr.ModelDef;
            expr.UnsafeSelect(this.GetQuotedColumnName(modelDef, modelDef.PrimaryKey));

            var subSql = expr.ToSelectStatement();

            return subSql;
        }

        public virtual string ToRowCountStatement(string innerSql)
        {
            return $"SELECT COUNT(*) FROM ({innerSql}) AS COUNT";
        }

        public virtual void DropColumn(IDbConnection db, Type modelType, string columnName)
        {
            var provider = db.GetDialectProvider();
            var command = ToDropColumnStatement(modelType, columnName, provider);

            db.ExecuteSql(command);
        }

        protected virtual string ToDropColumnStatement(Type modelType, string columnName, IOrmLiteDialectProvider provider)
        {
            return $"ALTER TABLE {provider.GetQuotedTableName(modelType.GetModelDefinition().ModelName)} " +
                   $"DROP COLUMN {provider.GetQuotedColumnName(columnName)};";
        }

        public virtual string SqlConcat(IEnumerable<object> args) => $"CONCAT({string.Join(", ", args)})";

        public virtual string SqlCurrency(string fieldOrValue) => SqlCurrency(fieldOrValue, "$");

        public virtual string SqlCurrency(string fieldOrValue, string currencySymbol) => SqlConcat(new List<string> { currencySymbol, fieldOrValue });

        public virtual string SqlBool(bool value) => value ? "true" : "false";

        public virtual string SqlLimit(int? offset = null, int? rows = null) => rows == null && offset == null
            ? "" 
            : offset == null
                ? "LIMIT " + rows
                : "LIMIT " + rows.GetValueOrDefault(int.MaxValue) + " OFFSET " + offset;

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

#if ASYNC
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
            throw new NotImplementedException(OrmLiteUtils.AsyncRequiresNet45Error);
        }

        public Task<Return> ReaderEach<Return>(IDataReader reader, Action fn, Return source, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException(OrmLiteUtils.AsyncRequiresNet45Error);
        }

        public Task<T> ReaderRead<T>(IDataReader reader, Func<T> fn, CancellationToken token = new CancellationToken())
        {
            throw new NotImplementedException(OrmLiteUtils.AsyncRequiresNet45Error);
        }

        public Task<long> InsertAndGetLastInsertIdAsync<T>(IDbCommand dbCmd, CancellationToken token)
        {
            throw new NotImplementedException(OrmLiteUtils.AsyncRequiresNet45Error);
        }
#endif
    }
}
