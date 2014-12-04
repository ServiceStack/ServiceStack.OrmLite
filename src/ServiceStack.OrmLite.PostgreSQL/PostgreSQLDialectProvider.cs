using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.PostgreSQL
{
    public class PostgreSQLDialectProvider : OrmLiteDialectProviderBase<PostgreSQLDialectProvider>
    {
        public static PostgreSQLDialectProvider Instance = new PostgreSQLDialectProvider();
        const string textColumnDefinition = "text";

        public bool UseReturningForLastInsertId { get; set; }

        public PostgreSQLDialectProvider()
        {
            base.AutoIncrementDefinition = "";
            base.IntColumnDefinition = "integer";
            base.BoolColumnDefinition = "boolean";
            base.TimeColumnDefinition = "time";
            base.DateTimeColumnDefinition = "timestamp";
            base.DateTimeOffsetColumnDefinition = "timestamp";
            base.DecimalColumnDefinition = "numeric(38,6)";
            base.GuidColumnDefinition = "uuid";
            base.ParamString = ":";
            base.BlobColumnDefinition = "bytea";
            base.RealColumnDefinition = "double precision";
            base.StringLengthColumnDefinitionFormat = textColumnDefinition;
            //there is no "n"varchar in postgres. All strings are either unicode or non-unicode, inherited from the database.
            base.StringLengthUnicodeColumnDefinitionFormat = "character varying({0})";
            base.StringLengthNonUnicodeColumnDefinitionFormat = "character varying({0})";
            base.MaxStringColumnDefinition = "TEXT";
            base.InitColumnTypeMap();
            base.SelectIdentitySql = "SELECT LASTVAL()";
            this.UseReturningForLastInsertId = true;
            this.NamingStrategy = new PostgreSqlNamingStrategy();
            this.StringSerializer = new JsonStringSerializer();
        }

        public override void OnAfterInitColumnTypeMap()
        {
            DbTypeMap.Set<TimeSpan>(DbType.Time, "interval");
            DbTypeMap.Set<TimeSpan?>(DbType.Time, "interval");
            DbTypeMap.Set<DateTimeOffset>(DbType.DateTimeOffset, DateTimeOffsetColumnDefinition);
            DbTypeMap.Set<DateTimeOffset?>(DbType.DateTimeOffset, DateTimeOffsetColumnDefinition);

            //throws unknown type exceptions in parameterized queries, e.g: p.DbType = DbType.SByte
            DbTypeMap.Set<sbyte>(DbType.Byte, IntColumnDefinition);
            DbTypeMap.Set<ushort>(DbType.Int16, IntColumnDefinition);
            DbTypeMap.Set<uint>(DbType.Int32, IntColumnDefinition);
            DbTypeMap.Set<ulong>(DbType.Int64, LongColumnDefinition);

            base.OnAfterInitColumnTypeMap();
        }

        public override string GetColumnDefinition(
            string fieldName,
            Type fieldType,
            bool isPrimaryKey,
            bool autoIncrement,
            bool isNullable, 
            bool isRowVersion,
            int? fieldLength,
            int? scale,
            string defaultValue,
            string customFieldDefinition)
        {
            if (isRowVersion)
                return null;

            string fieldDefinition = null;
            if (customFieldDefinition != null)
            {
                fieldDefinition = customFieldDefinition;
            }
            else if (fieldType == typeof(string))
            {
                fieldDefinition = fieldLength == int.MaxValue
                    ? MaxStringColumnDefinition
                    : fieldLength != null ?
                        string.Format(StringLengthColumnDefinitionFormat, fieldLength) :
                        textColumnDefinition;
            }
            else
            {
                if (autoIncrement)
                {
                    if (fieldType == typeof(long))
                        fieldDefinition = "bigserial";
                    else if (fieldType == typeof(int))
                        fieldDefinition = "serial";
                }
                else
                {
                    fieldDefinition = GetColumnTypeDefinition(fieldType);
                }
            }

            var sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldName), fieldDefinition);

            if (isPrimaryKey)
            {
                sql.Append(" PRIMARY KEY");
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

        //Convert xmin into an integer so it can be used in comparisons
        public const string RowVersionFieldComparer = "int8in(xidout(xmin))";

        public override string GetRowVersionColumnName(FieldDefinition field)
        {
            return "xmin as " + GetQuotedColumnName(field.FieldName);
        }

        public override void AppendFieldCondition(StringBuilder sqlFilter, FieldDefinition fieldDef, IDbCommand cmd)
        {
            var columnName = fieldDef.IsRowVersion
                ? RowVersionFieldComparer
                : GetQuotedColumnName(fieldDef.FieldName);
            
            sqlFilter
                .Append(columnName)
                .Append("=")
                .Append(this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName)));

            AddParameter(cmd, fieldDef);
        }

        public override string GetQuotedValue(string paramValue)
        {
            return "'" + paramValue.Replace("'", @"''") + "'";
        }

        public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
        {
            return new NpgsqlConnection(connectionString);
        }

        public override string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

            if (fieldType == typeof(DateTime))
            {
                var dateValue = (DateTime)value;
                const string iso8601Format = "yyyy-MM-dd HH:mm:ss.fff";
                return base.GetQuotedValue(dateValue.ToString(iso8601Format), typeof(string));
            }
            if (fieldType == typeof(DateTimeOffset))
            {
                var dateValue = (DateTimeOffset)value;
                const string iso8601Format = "yyyy-MM-dd HH:mm:ss.fff zzz";
                return base.GetQuotedValue(dateValue.ToString(iso8601Format), typeof(string));
            }
            if (fieldType == typeof(Guid))
            {
                var guidValue = (Guid)value;
                return base.GetQuotedValue(guidValue.ToString("N"), typeof(string));
            }
            if (fieldType == typeof(byte[]))
            {
                return "E'" + ToBinary(value) + "'";
            }
            if (fieldType.IsArray && typeof(string).IsAssignableFrom(fieldType.GetElementType()))
            {
                var stringArray = (string[])value;
                return ToArray(stringArray);
            }
            if (fieldType.IsArray && typeof(int).IsAssignableFrom(fieldType.GetElementType()))
            {
                var integerArray = (int[])value;
                return ToArray(integerArray);
            }
            if (fieldType.IsArray && typeof(long).IsAssignableFrom(fieldType.GetElementType()))
            {
                var longArray = (long[])value;
                return ToArray(longArray);
            }

            return base.GetQuotedValue(value, fieldType);
        }

        public override object ConvertDbValue(object value, Type type)
        {
            if (value == null || value is DBNull) return null;

            if (type == typeof(byte[])) { return value; }

            return base.ConvertDbValue(value, type);
        }

        public override SqlExpression<T> SqlExpression<T>()
        {
            return new PostgreSqlExpression<T>(this);
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName)
        {
            var sql = "SELECT COUNT(*) FROM pg_class WHERE relname = {0}"
                .SqlFmt(tableName);
            var conn = dbCmd.Connection;
            if (conn != null)
            {
                var builder = new NpgsqlConnectionStringBuilder(conn.ConnectionString);
                // If a search path (schema) is specified, and there is only one, then assume the CREATE TABLE directive should apply to that schema.
                if (!String.IsNullOrEmpty(builder.SearchPath) && !builder.SearchPath.Contains(","))
                    sql = "SELECT COUNT(*) FROM pg_class JOIN pg_catalog.pg_namespace n ON n.oid = pg_class.relnamespace WHERE relname = {0} AND nspname = {1}"
                          .SqlFmt(tableName, builder.SearchPath);
            }
            dbCmd.CommandText = sql;
            var result = dbCmd.LongScalar();

            return result > 0;
        }

        public override string ToExecuteProcedureStatement(object objWithProperties)
        {
            var sbColumnValues = new StringBuilder();

            var tableType = objWithProperties.GetType();
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (sbColumnValues.Length > 0) sbColumnValues.Append(",");
                try
                {
                    sbColumnValues.Append(fieldDef.GetQuotedValue(objWithProperties));
                }
                catch (Exception)
                {
                    throw;
                }
            }

            var sql = string.Format("{0} {1}{2}{3};",
                GetQuotedTableName(modelDef),
                sbColumnValues.Length > 0 ? "(" : "",
                sbColumnValues,
                sbColumnValues.Length > 0 ? ")" : "");

            return sql;
        }

        public override string GetQuotedTableName(ModelDefinition modelDef)
        {
            if (!modelDef.IsInSchema)
            {
                return base.GetQuotedTableName(modelDef);
            }
            string escapedSchema = modelDef.Schema.Replace(".", "\".\"");
            return string.Format("\"{0}\".\"{1}\"", escapedSchema, base.NamingStrategy.GetTableName(modelDef.ModelName));
        }

        /// <summary>
        /// based on Npgsql2's source: Npgsql2\src\NpgsqlTypes\NpgsqlTypeConverters.cs
        /// </summary>
        /// <param name="TypeInfo"></param>
        /// <param name="NativeData"></param>
        /// <param name="ForExtendedQuery"></param>
        /// <returns></returns>
        internal static String ToBinary(Object NativeData)
        {
            var byteArray = (Byte[])NativeData;
            var res = new StringBuilder(byteArray.Length * 5);
            foreach (byte b in byteArray)
                if (b >= 0x20 && b < 0x7F && b != 0x27 && b != 0x5C)
                    res.Append((char)b);
                else
                    res.Append("\\\\")
                        .Append((char)('0' + (7 & (b >> 6))))
                        .Append((char)('0' + (7 & (b >> 3))))
                        .Append((char)('0' + (7 & b)));
            return res.ToString();
        }

        internal string ToArray<T>(T[] source)
        {
            var values = new StringBuilder();
            foreach (var value in source)
            {
                if (values.Length > 0) values.Append(",");
                values.Append(base.GetQuotedValue(value, typeof(T)));
            }
            return "ARRAY[" + values + "]";
        }

        public override long InsertAndGetLastInsertId<T>(IDbCommand dbCmd)
        {
            if (SelectIdentitySql == null)
                throw new NotImplementedException("Returning last inserted identity is not implemented on this DB Provider.");

            if (UseReturningForLastInsertId)
            {
                var modelDef = GetModel(typeof(T));
                var pkName = NamingStrategy.GetColumnName(modelDef.PrimaryKey.FieldName);
                dbCmd.CommandText += " RETURNING " + pkName;                
            }
            else
            {
                dbCmd.CommandText += "; " + SelectIdentitySql;
            }

            return dbCmd.ExecLongScalar();
        }

        public override void SetParameter(FieldDefinition fieldDef, IDbDataParameter p)
        {
            if (fieldDef.CustomFieldDefinition == "json")
            {
                p.ParameterName = this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName));
                ((NpgsqlParameter) p).NpgsqlDbType = NpgsqlDbType.Json;
                return;
            }
            if (fieldDef.CustomFieldDefinition == "text[]")
            {
                p.ParameterName = this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName));
                ((NpgsqlParameter)p).NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Text;
                return;
            }
            if (fieldDef.CustomFieldDefinition == "integer[]")
            {
                p.ParameterName = this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName));
                ((NpgsqlParameter) p).NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer;
                return;
            }
            if (fieldDef.CustomFieldDefinition == "bigint[]")
            {
                p.ParameterName = this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName));
                ((NpgsqlParameter) p).NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Bigint;
                return;
            }
            base.SetParameter(fieldDef, p);
        }
        protected override object GetValue<T>(FieldDefinition fieldDef, object obj)
        {
            if (fieldDef.CustomFieldDefinition == "text[]")
            {
                return fieldDef.GetValue(obj);
            }
            if (fieldDef.CustomFieldDefinition == "integer[]")
            {
                return fieldDef.GetValue(obj);
            }
            if (fieldDef.CustomFieldDefinition == "bigint[]")
            {
                return fieldDef.GetValue(obj);
            }
            return base.GetValue<T>(fieldDef, obj);
        }

        public override void PrepareStoredProcedureStatement<T>(IDbCommand cmd, T obj)
        {
            var tableType = obj.GetType();
            var modelDef = GetModel(tableType);

            cmd.CommandText = GetQuotedTableName(modelDef);
            cmd.CommandType = CommandType.StoredProcedure;

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                var p = cmd.CreateParameter();
                SetParameter(fieldDef, p);
                cmd.Parameters.Add(p);
            }

            SetParameterValues<T>(cmd, obj);
        }
    }
}
