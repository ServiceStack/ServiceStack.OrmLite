using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServerOrmLiteDialectProvider : OrmLiteDialectProviderBase<SqlServerOrmLiteDialectProvider>
    {
        public static SqlServerOrmLiteDialectProvider Instance = new SqlServerOrmLiteDialectProvider();

        private static readonly DateTime timeSpanOffset = new DateTime(1900, 01, 01);

        public SqlServerOrmLiteDialectProvider()
        {
            base.AutoIncrementDefinition = "IDENTITY(1,1)";
            StringColumnDefinition = UseUnicode ? "NVARCHAR(4000)" : "VARCHAR(8000)";
            base.MaxStringColumnDefinition = "VARCHAR(MAX)";
            base.GuidColumnDefinition = "UniqueIdentifier";
            base.RealColumnDefinition = "FLOAT";
            base.BoolColumnDefinition = "BIT";
            base.TimeColumnDefinition = "TIME"; //SQLSERVER 2008+
            base.BlobColumnDefinition = "VARBINARY(MAX)";
            base.SelectIdentitySql = "SELECT SCOPE_IDENTITY()";
            base.DecimalColumnDefinition = "DECIMAL(38,6)";
            base.DefaultDecimalPrecision = 38;
            base.DefaultDecimalScale = 6;

            base.InitColumnTypeMap();
        }

        public override void OnAfterInitColumnTypeMap()
        {
            base.OnAfterInitColumnTypeMap();

            DbTypeMap.Set<TimeSpan>(DbType.DateTime, TimeColumnDefinition);
            DbTypeMap.Set<TimeSpan?>(DbType.DateTime, TimeColumnDefinition);

            //throws unknown type exceptions in parameterized queries, e.g: p.DbType = DbType.SByte
            DbTypeMap.Set<sbyte>(DbType.Byte, IntColumnDefinition);
            DbTypeMap.Set<ushort>(DbType.Int16, IntColumnDefinition);
            DbTypeMap.Set<uint>(DbType.Int32, IntColumnDefinition);
            DbTypeMap.Set<ulong>(DbType.Int64, LongColumnDefinition);
        }

        public override string GetQuotedValue(string paramValue)
        {
            return (UseUnicode ? "N'" : "'") + paramValue.Replace("'", "''") + "'";
        }

        public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
        {
            var isFullConnectionString = connectionString.Contains(";");

            if (!isFullConnectionString)
            {
                var filePath = connectionString;

                var filePathWithExt = filePath.ToLower().EndsWith(".mdf")
                    ? filePath
                    : filePath + ".mdf";

                var fileName = Path.GetFileName(filePathWithExt);
                var dbName = fileName.Substring(0, fileName.Length - ".mdf".Length);

                connectionString = string.Format(
                @"Data Source=.\SQLEXPRESS;AttachDbFilename={0};Initial Catalog={1};Integrated Security=True;User Instance=True;",
                    filePathWithExt, dbName);
            }

            if (options != null)
            {
                foreach (var option in options)
                {
                    if (option.Key.ToLower() == "read only")
                    {
                        if (option.Value.ToLower() == "true")
                        {
                            connectionString += "Mode = Read Only;";
                        }
                        continue;
                    }
                    connectionString += option.Key + "=" + option.Value + ";";
                }
            }

            return new SqlConnection(connectionString);
        }

        public override string GetQuotedTableName(ModelDefinition modelDef)
        {
            if (!modelDef.IsInSchema)
                return base.GetQuotedTableName(modelDef);

            var escapedSchema = modelDef.Schema.Replace(".", "\".\"");
            return string.Format("\"{0}\".\"{1}\"", escapedSchema, NamingStrategy.GetTableName(modelDef.ModelName));
        }

        public override void SetDbValue(FieldDefinition fieldDef, IDataReader reader, int colIndex, object instance)
        {
            if (fieldDef.IsRowVersion)
            {
                var bytes = reader.GetValue(colIndex) as byte[];
                if (bytes != null)
                {
                    var ulongValue = ConvertToULong(bytes);
                    try
                    {
                        fieldDef.SetValueFn(instance, ulongValue);
                    }
                    catch (NullReferenceException ignore) { }
                }
            }
            else
            {
                base.SetDbValue(fieldDef, reader, colIndex, instance);
            }
        }

        public override object ConvertDbValue(object value, Type type)
        {
            try
            {
                if (value == null || value is DBNull) return null;

                if (type == typeof(bool) && !(value is bool))
                {
                    var intVal = Convert.ToInt32(value.ToString());
                    return intVal != 0;
                }

                if (type == typeof(TimeSpan) && value is DateTime)
                {
                    var dateTimeValue = (DateTime)value;
                    return dateTimeValue - timeSpanOffset;
                }

                if (_ensureUtc && type == typeof(DateTime))
                {
                    var result = base.ConvertDbValue(value, type);
                    if (result is DateTime)
                        return DateTime.SpecifyKind((DateTime)result, DateTimeKind.Utc);
                    return result;
                }

                if (type == typeof(byte[]))
                    return value;

                return base.ConvertDbValue(value, type);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public override string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

            if (fieldType == typeof(Guid))
            {
                var guidValue = (Guid)value;
                return string.Format("CAST('{0}' AS UNIQUEIDENTIFIER)", guidValue);
            }
            if (fieldType == typeof(DateTime))
            {
                var dateValue = (DateTime)value;
                if (_ensureUtc && dateValue.Kind == DateTimeKind.Local)
                    dateValue = dateValue.ToUniversalTime();
                const string iso8601Format = "yyyyMMdd HH:mm:ss.fff";
                return base.GetQuotedValue(dateValue.ToString(iso8601Format, CultureInfo.InvariantCulture), typeof(string));
            }
            if (fieldType == typeof(DateTimeOffset))
            {
                var dateValue = (DateTimeOffset)value;
                const string iso8601Format = "yyyyMMdd HH:mm:ss.fff zzz";
                return base.GetQuotedValue(dateValue.ToString(iso8601Format, CultureInfo.InvariantCulture), typeof(string));
            }
            if (fieldType == typeof(bool))
            {
                var boolValue = (bool)value;
                return base.GetQuotedValue(boolValue ? 1 : 0, typeof(int));
            }
            if (fieldType == typeof(string))
            {
                return GetQuotedValue(value.ToString());
            }

            if (fieldType == typeof(byte[]))
            {
                return "0x" + BitConverter.ToString((byte[])value).Replace("-", "");
            }

            return base.GetQuotedValue(value, fieldType);
        }

        protected override string GetUndefinedColumnDefinition(Type fieldType, int? fieldLength)
        {
            return string.Format(StringLengthColumnDefinitionFormat, fieldLength.HasValue ? fieldLength.Value.ToString() : "MAX");
        }

        protected bool _useDateTime2;
        public void UseDatetime2(bool shouldUseDatetime2)
        {
            _useDateTime2 = shouldUseDatetime2;
            DateTimeColumnDefinition = shouldUseDatetime2 ? "datetime2" : "datetime";
            base.DbTypeMap.Set<DateTime>(shouldUseDatetime2 ? DbType.DateTime2 : DbType.DateTime, DateTimeColumnDefinition);
            base.DbTypeMap.Set<DateTime?>(shouldUseDatetime2 ? DbType.DateTime2 : DbType.DateTime, DateTimeColumnDefinition);
        }

        protected bool _ensureUtc;
        public void EnsureUtc(bool shouldEnsureUtc)
        {
            _ensureUtc = shouldEnsureUtc;
        }

        public override SqlExpression<T> SqlExpression<T>()
        {
            return new SqlServerExpression<T>(this);
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName)
        {
            var sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {0}"
                .SqlFmt(tableName);

            //if (!string.IsNullOrEmpty(schemaName))
            //    sql += " AND TABLE_SCHEMA = {0}".SqlFmt(schemaName);

            dbCmd.CommandText = sql;
            var result = dbCmd.LongScalar();

            return result > 0;
        }

        public override bool UseUnicode
        {
            get
            {
                return useUnicode;
            }
            set
            {
                useUnicode = value;
                if (useUnicode && this.DefaultStringLength > 4000)
                {
                    this.DefaultStringLength = 4000;
                }
            }
        }

        public override string GetForeignKeyOnDeleteClause(ForeignKeyConstraint foreignKey)
        {
            return "RESTRICT" == (foreignKey.OnDelete ?? "").ToUpper()
                ? ""
                : base.GetForeignKeyOnDeleteClause(foreignKey);
        }

        public override string GetForeignKeyOnUpdateClause(ForeignKeyConstraint foreignKey)
        {
            return "RESTRICT" == (foreignKey.OnDelete ?? "").ToUpper()
                ? ""
                : base.GetForeignKeyOnUpdateClause(foreignKey);
        }

        public override string GetDropForeignKeyConstraints(ModelDefinition modelDef)
        {
            //TODO: find out if this should go in base class?

            var sb = new StringBuilder();
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.ForeignKey != null)
                {
                    var foreignKeyName = fieldDef.ForeignKey.GetForeignKeyName(
                        modelDef,
                        GetModelDefinition(fieldDef.ForeignKey.ReferenceType),
                        NamingStrategy,
                        fieldDef);

                    var tableName = GetQuotedTableName(modelDef);
                    sb.AppendLine("IF EXISTS (SELECT name FROM sys.foreign_keys WHERE name = '{0}')".Fmt(foreignKeyName));
                    sb.AppendLine("BEGIN");
                    sb.AppendLine("  ALTER TABLE {0} DROP {1};".Fmt(tableName, foreignKeyName));
                    sb.AppendLine("END");
                }
            }

            return sb.ToString();
        }

        public override string ToAddColumnStatement(Type modelType, FieldDefinition fieldDef)
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

            return string.Format("ALTER TABLE {0} ADD {1};",
                                 GetQuotedTableName(GetModel(modelType).ModelName),
                                 column);
        }

        public override string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef)
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

            return string.Format("ALTER TABLE {0} ALTER COLUMN {1};",
                                 GetQuotedTableName(GetModel(modelType).ModelName),
                                 column);
        }

        public override string ToChangeColumnNameStatement(Type modelType, FieldDefinition fieldDef, string oldColumnName)
        {
            var objectName = string.Format("{0}.{1}",
                NamingStrategy.GetTableName(GetModel(modelType).ModelName),
                oldColumnName);

            return string.Format("EXEC sp_rename {0}, {1}, {2};",
                                 GetQuotedValue(objectName),
                                 GetQuotedValue(fieldDef.FieldName),
                                 GetQuotedValue("COLUMN"));
        }

        public override string GetColumnDefinition(string fieldName, Type fieldType, bool isPrimaryKey, bool autoIncrement,
            bool isNullable, bool isRowVersion, int? fieldLength, int? scale, string defaultValue, string customFieldDefinition)
        {
            if (isRowVersion)
                return "{0} rowversion NOT NULL".Fmt(fieldName);

            var definition = base.GetColumnDefinition(fieldName, fieldType, isPrimaryKey, autoIncrement,
                isNullable, isRowVersion, fieldLength, scale, defaultValue, customFieldDefinition);

            if (fieldType == typeof(Decimal) && fieldLength != DefaultDecimalPrecision && scale != DefaultDecimalScale)
            {
                string validDecimal = String.Format("DECIMAL({0},{1})",
                    fieldLength.GetValueOrDefault(DefaultDecimalPrecision),
                    scale.GetValueOrDefault(DefaultDecimalScale));
                definition = definition.Replace(DecimalColumnDefinition, validDecimal);
            }

            return definition;
        }

        public override string ToSelectStatement(ModelDefinition modelDef,
            string selectExpression,
            string bodyExpression,
            string orderByExpression = null,
            int? offset = null,
            int? rows = null)
        {
            var sb = new StringBuilder(selectExpression);
            sb.Append(bodyExpression);

            if (!offset.HasValue && !rows.HasValue)
                return sb + orderByExpression;

            if (offset.HasValue && offset.Value < 0)
                throw new ArgumentException(string.Format("Skip value:'{0}' must be>=0", offset.Value));

            if (rows.HasValue && rows.Value < 0)
                throw new ArgumentException(string.Format("Rows value:'{0}' must be>=0", rows.Value));

            var skip = offset.HasValue ? offset.Value : 0;
            var take = rows.HasValue ? rows.Value : int.MaxValue;

            var selectType = selectExpression.StartsWithIgnoreCase("SELECT DISTINCT") ? "SELECT DISTINCT" : "SELECT";

            //Temporary hack till we come up with a more robust paging sln for SqlServer
            if (skip == 0)
            {
                var sql = sb + orderByExpression;

                if (take == int.MaxValue)
                    return sql;

                if (sql.Length < "SELECT".Length) return sql;
                sql = selectType + " TOP " + take + sql.Substring(selectType.Length);
                return sql;
            }

            // Required because ordering is done by Windowing function
            if (string.IsNullOrEmpty(orderByExpression))
            {
                if (modelDef.PrimaryKey == null)
                    throw new ApplicationException("Malformed model, no PrimaryKey defined");

                orderByExpression = string.Format("ORDER BY {0}",
                    OrmLiteConfig.DialectProvider.GetQuotedColumnName(modelDef.PrimaryKey.FieldName));
            }

            var ret = string.Format(
                "SELECT * FROM (SELECT ROW_NUMBER() OVER ({1}) As RowNum, {0} {2}) AS RowConstrainedResult WHERE RowNum > {3} AND RowNum <= {4}",
                selectExpression.Substring(selectType.Length),
                orderByExpression,
                bodyExpression,
                skip,
                take == int.MaxValue ? take : skip + take);

            return ret;
        }
    }
}
