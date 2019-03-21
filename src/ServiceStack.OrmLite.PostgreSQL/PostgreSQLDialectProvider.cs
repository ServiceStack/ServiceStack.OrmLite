using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Npgsql.TypeMapping;
using NpgsqlTypes;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Converters;
using ServiceStack.OrmLite.PostgreSQL.Converters;
using ServiceStack.OrmLite.Support;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.PostgreSQL
{
    public class PostgreSqlDialectProvider : OrmLiteDialectProviderBase<PostgreSqlDialectProvider>
    {
        public static PostgreSqlDialectProvider Instance = new PostgreSqlDialectProvider();

        public bool UseReturningForLastInsertId { get; set; } = true;

        public string AutoIdGuidFunction { get; set; } = "uuid_generate_v4()";

        public PostgreSqlDialectProvider()
        {
            base.AutoIncrementDefinition = "";
            base.ParamString = ":";
            base.SelectIdentitySql = "SELECT LASTVAL()";
            this.NamingStrategy = new PostgreSqlNamingStrategy();
            this.StringSerializer = new JsonStringSerializer();
            
            base.InitColumnTypeMap();

            this.RowVersionConverter = new PostgreSqlRowVersionConverter();

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

            //TODO provide support for pgsql native data structures:
            RegisterConverter<string[]>(new PostgreSqlStringArrayConverter());
            RegisterConverter<int[]>(new PostgreSqlIntArrayConverter());
            RegisterConverter<long[]>(new PostgreSqlLongArrayConverter());
            RegisterConverter<float[]>(new PostgreSqlFloatArrayConverter());
            RegisterConverter<double[]>(new PostgreSqlDoubleArrayConverter());
            RegisterConverter<decimal[]>(new PostgreSqlDecimalArrayConverter());
            RegisterConverter<DateTime[]>(new PostgreSqlDateTimeTimeStampArrayConverter());
            RegisterConverter<DateTimeOffset[]>(new PostgreSqlDateTimeOffsetTimeStampTzArrayConverter());

            this.Variables = new Dictionary<string, string>
            {
                { OrmLiteVariables.SystemUtc, "now() at time zone 'utc'" },
                { OrmLiteVariables.MaxText, "TEXT" },
                { OrmLiteVariables.MaxTextUnicode, "TEXT" },
                { OrmLiteVariables.True, SqlBool(true) },                
                { OrmLiteVariables.False, SqlBool(false) },                
            };
        }

        public bool UseHstore
        {
            set
            {
                if (value)
                {
                    RegisterConverter<IDictionary<string, string>>(new PostgreSqlHstoreConverter());
                    RegisterConverter<Dictionary<string, string>>(new PostgreSqlHstoreConverter());
                }
                else
                {
                    RemoveConverter<IDictionary<string, string>>();
                    RemoveConverter<Dictionary<string, string>>();
                }
            }
        }

        private bool normalize;
        public bool Normalize
        {
            get => normalize;
            set
            {
                normalize = value;
                NamingStrategy = normalize
                    ? new OrmLiteNamingStrategyBase()
                    : new PostgreSqlNamingStrategy();
            }            
        }

        //https://www.postgresql.org/docs/7.3/static/sql-keywords-appendix.html
        public static HashSet<string> ReservedWords = new HashSet<string>(new[]
        {
            "ALL",
            "ANALYSE",
            "ANALYZE",
            "AND",
            "ANY",
            "AS",
            "ASC",
            "AUTHORIZATION",
            "BETWEEN",
            "BINARY",
            "BOTH",
            "CASE",
            "CAST",
            "CHECK",
            "COLLATE",
            "COLUMN",
            "CONSTRAINT",
            "CURRENT_DATE",
            "CURRENT_TIME",
            "CURRENT_TIMESTAMP",
            "CURRENT_USER",
            "DEFAULT",
            "DEFERRABLE",
            "DISTINCT",
            "DO",
            "ELSE",
            "END",
            "EXCEPT",
            "FOR",
            "FOREIGN",
            "FREEZE",
            "FROM",
            "FULL",
            "HAVING",
            "ILIKE",
            "IN",
            "INITIALLY",
            "INNER",
            "INTERSECT",
            "INTO",
            "IS",
            "ISNULL",
            "JOIN",
            "LEADING",
            "LEFT",
            "LIKE",
            "LIMIT",
            "LOCALTIME",
            "LOCALTIMESTAMP",
            "NEW",
            "NOT",
            "NOTNULL",
            "NULL",
            "OFF",
            "OFFSET",
            "OLD",
            "ON",
            "ONLY",
            "OR",
            "ORDER",
            "OUTER",
            "OVERLAPS",
            "PLACING",
            "PRIMARY",
            "REFERENCES",
            "RIGHT",
            "SELECT",
            "SESSION_USER",
            "SIMILAR",
            "SOME",
            "TABLE",
            "THEN",
            "TO",
            "TRAILING",
            "TRUE",
            "UNION",
            "UNIQUE",
            "USER",
            "USING",
            "VERBOSE",
            "WHEN",
            "WHERE",
        }, StringComparer.OrdinalIgnoreCase);

        public override string GetColumnDefinition(FieldDefinition fieldDef)
        {
            if (fieldDef.IsRowVersion)
                return null;

            string fieldDefinition = null;
            if (fieldDef.CustomFieldDefinition != null)
            {
                fieldDefinition = ResolveFragment(fieldDef.CustomFieldDefinition);
            }
            else
            {
                if (fieldDef.AutoIncrement)
                {
                    if (fieldDef.ColumnType == typeof(long))
                        fieldDefinition = "bigserial";
                    else if (fieldDef.ColumnType == typeof(int))
                        fieldDefinition = "serial";
                }
                else
                {
                    fieldDefinition = GetColumnTypeDefinition(fieldDef.ColumnType, fieldDef.FieldLength, fieldDef.Scale);
                }
            }

            var sql = StringBuilderCache.Allocate();
            sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldDef.FieldName), fieldDefinition);

            if (fieldDef.IsPrimaryKey)
            {
                sql.Append(" PRIMARY KEY");
            }
            else
            {
                if (fieldDef.IsNullable)
                {
                    sql.Append(" NULL");
                }
                else
                {
                    sql.Append(" NOT NULL");
                }
            }

            if (fieldDef.IsUniqueConstraint)
            {
                sql.Append(" UNIQUE");
            }

            var defaultValue = GetDefaultValue(fieldDef);
            if (!string.IsNullOrEmpty(defaultValue))
            {
                sql.AppendFormat(DefaultValueFormat, defaultValue);
            }

            var definition = StringBuilderCache.ReturnAndFree(sql);
            return definition;
        }

        public override string GetAutoIdDefaultValue(FieldDefinition fieldDef)
        {
            return fieldDef.FieldType == typeof(Guid)
                ? AutoIdGuidFunction
                : null;
        }

        protected override bool ShouldSkipInsert(FieldDefinition fieldDef) => 
            fieldDef.ShouldSkipInsert() || fieldDef.AutoId;

        protected virtual bool ShouldReturnOnInsert(ModelDefinition modelDef, FieldDefinition fieldDef) =>
            fieldDef.ReturnOnInsert || (fieldDef.IsPrimaryKey && fieldDef.AutoIncrement && HasInsertReturnValues(modelDef)) || fieldDef.AutoId;

        public override bool HasInsertReturnValues(ModelDefinition modelDef) =>
            modelDef.FieldDefinitions.Any(x => x.ReturnOnInsert || (x.AutoId && x.FieldType == typeof(Guid)));

        public override void PrepareParameterizedInsertStatement<T>(IDbCommand cmd, ICollection<string> insertFields = null)
        {
            var sbColumnNames = StringBuilderCache.Allocate();
            var sbColumnValues = StringBuilderCacheAlt.Allocate();
            var sbReturningColumns = StringBuilderCacheAlt.Allocate();
            var modelDef = OrmLiteUtils.GetModelDefinition(typeof(T));

            cmd.Parameters.Clear();

            var fieldDefs = GetInsertFieldDefinitions(modelDef, insertFields);
            foreach (var fieldDef in fieldDefs)
            {
                if (ShouldReturnOnInsert(modelDef, fieldDef))
                {
                    sbReturningColumns.Append(sbReturningColumns.Length == 0 ? " RETURNING " : ",");
                    sbReturningColumns.Append(GetQuotedColumnName(fieldDef.FieldName));
                }

                if (ShouldSkipInsert(fieldDef) && !fieldDef.AutoId)
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

            foreach (var fieldDef in modelDef.AutoIdFields) // need to include any AutoId fields that weren't included 
            {
                if (fieldDefs.Contains(fieldDef))
                    continue;

                sbReturningColumns.Append(sbReturningColumns.Length == 0 ? " RETURNING " : ",");
                sbReturningColumns.Append(GetQuotedColumnName(fieldDef.FieldName));
            }

            var strReturning = StringBuilderCacheAlt.ReturnAndFree(sbReturningColumns);
            cmd.CommandText = sbColumnNames.Length > 0
                ? $"INSERT INTO {GetQuotedTableName(modelDef)} ({StringBuilderCache.ReturnAndFree(sbColumnNames)}) " +
                  $"VALUES ({StringBuilderCacheAlt.ReturnAndFree(sbColumnValues)}){strReturning}"
                : $"INSERT INTO {GetQuotedTableName(modelDef)} DEFAULT VALUES{strReturning}";
        }
        
        //Convert xmin into an integer so it can be used in comparisons
        public const string RowVersionFieldComparer = "int8in(xidout(xmin))";

        public override SelectItem GetRowVersionSelectColumn(FieldDefinition field, string tablePrefix = null)
        {
            return new SelectItemColumn(this, "xmin", field.FieldName, tablePrefix);
        }

        public override string GetRowVersionColumn(FieldDefinition field, string tablePrefix = null)
        {
            return RowVersionFieldComparer;
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

        public override string ToTableNamesStatement(string schema)
        {
            var sql = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'";

            return schema != null 
                ? sql + " AND table_schema = {0}".SqlFmt(this, schema) 
                : sql + " AND table_schema = 'public'";
        }

        public override string ToTableNamesWithRowCountsStatement(bool live, string schema)
        {
            return live
                ? null 
                : "SELECT relname, reltuples FROM pg_class JOIN pg_catalog.pg_namespace n ON n.oid = pg_class.relnamespace WHERE relkind = 'r' AND nspname = {0}".SqlFmt(this, schema ?? "public");
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName, string schema = null)
        {
            var sql = !Normalize || ReservedWords.Contains(tableName)
                ? "SELECT COUNT(*) FROM pg_class WHERE relname = {0}".SqlFmt(tableName)
                : "SELECT COUNT(*) FROM pg_class WHERE lower(relname) = {0}".SqlFmt(tableName.ToLower());

            var conn = dbCmd.Connection;
            if (conn != null)
            {
                var builder = new NpgsqlConnectionStringBuilder(conn.ConnectionString);
                if (schema == null)
                    schema = builder.SearchPath;
                
                // If a search path (schema) is specified, and there is only one, then assume the CREATE TABLE directive should apply to that schema.
                if (!string.IsNullOrEmpty(schema) && !schema.Contains(","))
                {
                    sql = !Normalize || ReservedWords.Contains(schema)
                        ? "SELECT COUNT(*) FROM pg_class JOIN pg_catalog.pg_namespace n ON n.oid = pg_class.relnamespace WHERE relname = {0} AND nspname = {1}".SqlFmt(tableName, schema)
                        : "SELECT COUNT(*) FROM pg_class JOIN pg_catalog.pg_namespace n ON n.oid = pg_class.relnamespace WHERE lower(relname) = {0} AND lower(nspname) = {1}".SqlFmt(tableName.ToLower(), schema.ToLower());
                }
            }

            var result = dbCmd.ExecLongScalar(sql);

            return result > 0;
        }
        
        public override bool DoesSchemaExist(IDbCommand dbCmd, string schemaName)
        {
            dbCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = '{GetSchemaName(schemaName).SqlParam()}');";
            var query = dbCmd.ExecuteScalar();
            return query as bool? ?? false;
        }

        public override string ToCreateSchemaStatement(string schemaName)
        {
            var sql = $"CREATE SCHEMA {GetSchemaName(schemaName)}";
            return sql;
        }

        public override bool DoesColumnExist(IDbConnection db, string columnName, string tableName, string schema = null)
        {
            var sql = !Normalize || ReservedWords.Contains(tableName)
                ? "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName".SqlFmt(tableName)
                : "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE lower(TABLE_NAME) = @tableName".SqlFmt(tableName.ToLower());

            sql += !Normalize || ReservedWords.Contains(columnName)
                ? " AND COLUMN_NAME = @columnName".SqlFmt(columnName)
                : " AND lower(COLUMN_NAME) = @columnName".SqlFmt(columnName.ToLower());

            if (schema != null)
            {
                sql += !Normalize || ReservedWords.Contains(schema)
                    ? " AND TABLE_SCHEMA = @schema"
                    : " AND lower(TABLE_SCHEMA) = @schema";

                if (Normalize)
                    schema = schema.ToLower();
            }

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

        public override string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef)
        {
            var columnDefinition = GetColumnDefinition(fieldDef);
            var modelName = GetQuotedTableName(GetModel(modelType));

            var parts = columnDefinition.SplitOnFirst(' ');
            var columnName = parts[0];
            var columnType = parts[1];

            var notNull = columnDefinition.Contains("NOT NULL");

            var nullLiteral = notNull ? " NOT NULL" : " NULL";
            columnType = columnType.Replace(nullLiteral, "");

            var nullSql = notNull 
                ? "SET NOT NULL" 
                : "DROP NOT NULL";

            var sql = $"ALTER TABLE {modelName}\n" 
                    + $"  ALTER COLUMN {columnName} TYPE {columnType},\n"
                    + $"  ALTER COLUMN {columnName} {nullSql}";

            return sql;
        }

        public override string GetQuotedTableName(string tableName, string schema = null)
        {
            return !Normalize || ReservedWords.Contains(tableName) || (schema != null && ReservedWords.Contains(schema)) || tableName.Contains(' ')
                ? base.GetQuotedTableName(tableName, schema)
                : schema != null
                    ? schema + "." + tableName
                    : tableName;
        }

        public override string GetQuotedName(string name)
        {
            return !Normalize || ReservedWords.Contains(name) || name.Contains(' ')
                ? base.GetQuotedName(name)
                : name;
        }

        public override string GetQuotedTableName(ModelDefinition modelDef)
        {
            if (!modelDef.IsInSchema)
                return base.GetQuotedTableName(modelDef);
            if (Normalize && !ReservedWords.Contains(modelDef.ModelName) && !ReservedWords.Contains(modelDef.Schema))
                return NamingStrategy.GetSchemaName(modelDef.Schema) + "." + NamingStrategy.GetTableName(modelDef.ModelName);

            string escapedSchema = NamingStrategy.GetSchemaName(modelDef.Schema).Replace(".", "\".\"");
            return $"\"{escapedSchema}\".\"{NamingStrategy.GetTableName(modelDef.ModelName)}\"";
        }
        
        public override string GetLastInsertIdSqlSuffix<T>()
        {
            if (SelectIdentitySql == null)
                throw new NotImplementedException("Returning last inserted identity is not implemented on this DB Provider.");

            if (UseReturningForLastInsertId)
            {
                var modelDef = GetModel(typeof(T));
                var pkName = NamingStrategy.GetColumnName(modelDef.PrimaryKey.FieldName);
                return !Normalize
                    ? $" RETURNING \"{pkName}\""
                    : " RETURNING " + pkName;
            }

            return "; " + SelectIdentitySql;
        }

        public static Dictionary<string, NpgsqlDbType> NativeTypes = new Dictionary<string, NpgsqlDbType> {
            { "json", NpgsqlDbType.Json },
            { "jsonb", NpgsqlDbType.Jsonb },
            { "hstore", NpgsqlDbType.Hstore },
            { "text[]", NpgsqlDbType.Array | NpgsqlDbType.Text },
            { "integer[]", NpgsqlDbType.Array | NpgsqlDbType.Integer },
            { "bigint[]", NpgsqlDbType.Array | NpgsqlDbType.Bigint },
            { "real[]", NpgsqlDbType.Array | NpgsqlDbType.Real },
            { "double precision[]", NpgsqlDbType.Array | NpgsqlDbType.Double },
            { "numeric[]", NpgsqlDbType.Array | NpgsqlDbType.Numeric },
            { "timestamp[]", NpgsqlDbType.Array | NpgsqlDbType.Timestamp },
            { "timestamp with time zone[]", NpgsqlDbType.Array | NpgsqlDbType.TimestampTz },
        };
        
        public override void SetParameter(FieldDefinition fieldDef, IDbDataParameter p)
        {
            if (fieldDef.CustomFieldDefinition != null &&
                NativeTypes.TryGetValue(fieldDef.CustomFieldDefinition, out var npgsqlDbType))
            {
                p.ParameterName = this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName));
                ((NpgsqlParameter) p).NpgsqlDbType = npgsqlDbType;
            }
            else
            {
                base.SetParameter(fieldDef, p);
            }
        }

        public virtual bool UseRawValue(string columnDef) => columnDef?.EndsWith("[]") == true;

        protected override object GetValue<T>(FieldDefinition fieldDef, object obj)
        {
            if (fieldDef.CustomFieldDefinition != null && NativeTypes.ContainsKey(fieldDef.CustomFieldDefinition)
                && UseRawValue(fieldDef.CustomFieldDefinition))
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

        public override string SqlConflict(string sql, string conflictResolution)
        {
            //https://www.postgresql.org/docs/current/static/sql-insert.html
            return sql + " ON CONFLICT " + (conflictResolution == ConflictResolution.Ignore
                       ? " DO NOTHING"
                       : conflictResolution);
        }

        public override string SqlConcat(IEnumerable<object> args) => string.Join(" || ", args);

        public override string SqlCurrency(string fieldOrValue, string currencySymbol) => currencySymbol == "$"
            ? fieldOrValue + "::text::money::text"
            : "replace(" + fieldOrValue + "::text::money::text,'$','" + currencySymbol + "')";

        public override string SqlCast(object fieldOrValue, string castAs) => 
            $"({fieldOrValue})::{castAs}";

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

#if ASYNC
        public override Task OpenAsync(IDbConnection db, CancellationToken token = default(CancellationToken))
        {
            return Unwrap(db).OpenAsync(token);
        }

        public override Task<IDataReader> ExecuteReaderAsync(IDbCommand cmd, CancellationToken token = default(CancellationToken))
        {
            return Unwrap(cmd).ExecuteReaderAsync(token).Then(x => (IDataReader)x);
        }

        public override Task<int> ExecuteNonQueryAsync(IDbCommand cmd, CancellationToken token = default(CancellationToken))
        {
            return Unwrap(cmd).ExecuteNonQueryAsync(token);
        }

        public override Task<object> ExecuteScalarAsync(IDbCommand cmd, CancellationToken token = default(CancellationToken))
        {
            return Unwrap(cmd).ExecuteScalarAsync(token);
        }

        public override Task<bool> ReadAsync(IDataReader reader, CancellationToken token = default(CancellationToken))
        {
            return Unwrap(reader).ReadAsync(token);
        }

        public override async Task<List<T>> ReaderEach<T>(IDataReader reader, Func<T> fn, CancellationToken token = default(CancellationToken))
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

        public override async Task<Return> ReaderEach<Return>(IDataReader reader, Action fn, Return source, CancellationToken token = default(CancellationToken))
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

        public override async Task<T> ReaderRead<T>(IDataReader reader, Func<T> fn, CancellationToken token = default(CancellationToken))
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
