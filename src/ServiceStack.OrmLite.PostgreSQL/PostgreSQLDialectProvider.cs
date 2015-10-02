using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Npgsql;
using NpgsqlTypes;
using ServiceStack.OrmLite.Converters;
using ServiceStack.OrmLite.PostgreSQL.Converters;
using ServiceStack.OrmLite.Support;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.PostgreSQL
{
    [Obsolete("Use PostgreSqlDialectProvider")]
    public class PostgreSQLDialectProvider : PostgreSqlDialectProvider { }

    public class PostgreSqlDialectProvider : OrmLiteDialectProviderBase<PostgreSqlDialectProvider>
    {
        public static PostgreSqlDialectProvider Instance = new PostgreSqlDialectProvider();

        public bool UseReturningForLastInsertId { get; set; }

        public PostgreSqlDialectProvider()
        {
            base.AutoIncrementDefinition = "";
            base.ParamString = ":";
            base.SelectIdentitySql = "SELECT LASTVAL()";
            this.UseReturningForLastInsertId = true;
            this.NamingStrategy = new PostgreSqlNamingStrategy();
            this.StringSerializer = new JsonStringSerializer();

            base.InitColumnTypeMap();

            RegisterConverter<string>(new PostgreSqlStringConverter());
            RegisterConverter<char[]>(new PostgreSqlCharArrayConverter());

            RegisterConverter<bool>(new PostgreSqlBoolConverter());
            RegisterConverter<Guid>(new PostgreSqlGuidConverter());

            RegisterConverter<DateTime>(new PostgreSqlDateTimeConverter());
            RegisterConverter<DateTimeOffset>(new PostgreSqlDateTimeOffsetConverter());


            RegisterConverter<sbyte>(new PostrgreSqlSByteConverter());
            RegisterConverter<ushort>(new PostrgreSqlUInt16Converter());
            RegisterConverter<uint>(new PostrgreSqlUInt32Converter());
            RegisterConverter<ulong>(new PostrgreSqlUInt64Converter());

            RegisterConverter<float>(new PostrgreSqlFloatConverter());
            RegisterConverter<double>(new PostrgreSqlDoubleConverter());
            RegisterConverter<decimal>(new PostrgreSqlDecimalConverter());

            RegisterConverter<byte[]>(new PostrgreSqlByteArrayConverter());

            //TODO provide support for pgsql native datastructures:
            //RegisterConverter<string[]>(new PostgreSqlStringArrayConverter());
            //RegisterConverter<int[]>(new PostgreSqlIntArrayConverter());
            //RegisterConverter<long[]>(new PostgreSqlLongArrayConverter());

            this.Variables = new Dictionary<string, string>
            {
                { OrmLiteVariables.SystemUtc, "now() at time zone 'utc'" },
            };
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
                    fieldDefinition = GetColumnTypeDefinition(fieldType, fieldLength, scale);
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

            var definition = sql.ToString();

            return definition;
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

        public override SqlExpression<T> SqlExpression<T>()
        {
            return new PostgreSqlExpression<T>(this);
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName, string schema = null)
        {
            var sql = "SELECT COUNT(*) FROM pg_class WHERE relname = {0}"
                .SqlFmt(tableName);

            var conn = dbCmd.Connection;
            if (conn != null)
            {
                var builder = new NpgsqlConnectionStringBuilder(conn.ConnectionString);
                if (schema == null)
                    schema = builder.SearchPath;
                
                // If a search path (schema) is specified, and there is only one, then assume the CREATE TABLE directive should apply to that schema.
                if (!string.IsNullOrEmpty(schema) && !schema.Contains(","))
                    sql = "SELECT COUNT(*) FROM pg_class JOIN pg_catalog.pg_namespace n ON n.oid = pg_class.relnamespace WHERE relname = {0} AND nspname = {1}"
                          .SqlFmt(tableName, schema);
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
            if (fieldDef.CustomFieldDefinition == "jsonb")
            {
                p.ParameterName = this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName));
                ((NpgsqlParameter)p).NpgsqlDbType = NpgsqlDbType.Jsonb;
                return;
            }
            if (fieldDef.CustomFieldDefinition == "hstore")
            {
                p.ParameterName = this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName));
                ((NpgsqlParameter)p).NpgsqlDbType = NpgsqlDbType.Hstore;
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
