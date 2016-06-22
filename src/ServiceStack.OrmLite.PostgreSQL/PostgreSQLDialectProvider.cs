using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using ServiceStack.OrmLite.Converters;
using ServiceStack.OrmLite.PostgreSQL.Converters;
using ServiceStack.OrmLite.Support;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.PostgreSQL
{
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
            RegisterConverter<int[]>(new PostgreSqlIntArrayConverter());
            RegisterConverter<long[]>(new PostgreSqlLongArrayConverter());

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

            var sql = StringBuilderCache.Allocate();
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

            var definition = StringBuilderCache.ReturnAndFree(sql);
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

        public override IDbDataParameter CreateParam()
        {
            return new NpgsqlParameter();
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

            var result = dbCmd.ExecLongScalar(sql);

            return result > 0;
        }

        public override bool DoesColumnExist(IDbConnection db, string columnName, string tableName, string schema = null)
        {
            var sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName AND COLUMN_NAME = @columnName"
                .SqlFmt(tableName, columnName);

            if (schema != null)
                sql += " AND TABLE_SCHEMA = @schema";

            var result = db.SqlScalar<long>(sql, new { tableName, columnName, schema });

            return result > 0;
        }

        public override string ToExecuteProcedureStatement(object objWithProperties)
        {
            var sbColumnValues = StringBuilderCache.Allocate();

            var tableType = objWithProperties.GetType();
            var modelDef = GetModel(tableType);

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (sbColumnValues.Length > 0) sbColumnValues.Append(",");
                sbColumnValues.Append(fieldDef.GetQuotedValue(objWithProperties));
            }

            var colValues = StringBuilderCache.ReturnAndFree(sbColumnValues);
            var sql = string.Format("{0} {1}{2}{3};",
                GetQuotedTableName(modelDef),
                colValues.Length > 0 ? "(" : "",
                colValues,
                colValues.Length > 0 ? ")" : "");

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
                dbCmd.CommandText += " RETURNING \"{0}\"".Fmt(pkName);                
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


        protected NpgsqlConnection Unwrap(IDbConnection db)
        {
            return (NpgsqlConnection)db.ToDbConnection();
        }

        protected NpgsqlCommand Unwrap(IDbCommand cmd)
        {
            return (NpgsqlCommand)cmd.ToDbCommand();
        }

        protected NpgsqlDataReader Unwrap(IDataReader reader)
        {
            return (NpgsqlDataReader)reader;
        }

#if NET45
        public override Task OpenAsync(IDbConnection db, CancellationToken token)
        {
            return Unwrap(db).OpenAsync(token);
        }

        public override Task<IDataReader> ExecuteReaderAsync(IDbCommand cmd, CancellationToken token)
        {
            return Unwrap(cmd).ExecuteReaderAsync(token).Then(x => (IDataReader)x);
        }

        public override Task<int> ExecuteNonQueryAsync(IDbCommand cmd, CancellationToken token)
        {
            return Unwrap(cmd).ExecuteNonQueryAsync(token);
        }

        public override Task<object> ExecuteScalarAsync(IDbCommand cmd, CancellationToken token)
        {
            return Unwrap(cmd).ExecuteScalarAsync(token);
        }

        public override Task<bool> ReadAsync(IDataReader reader, CancellationToken token)
        {
            return Unwrap(reader).ReadAsync(token);
        }

        public override async Task<List<T>> ReaderEach<T>(IDataReader reader, Func<T> fn, CancellationToken token)
        {
            try
            {
                var to = new List<T>();
                while (await ReadAsync(reader, token).ConfigureAwait(false))
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

        public override async Task<Return> ReaderEach<Return>(IDataReader reader, Action fn, Return source, CancellationToken token)
        {
            try
            {
                while (await ReadAsync(reader, token).ConfigureAwait(false))
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

        public override async Task<T> ReaderRead<T>(IDataReader reader, Func<T> fn, CancellationToken token)
        {
            try
            {
                if (await ReadAsync(reader, token).ConfigureAwait(false))
                    return fn();

                return default(T);
            }
            finally
            {
                reader.Dispose();
            }
        }
#endif
    }
}
