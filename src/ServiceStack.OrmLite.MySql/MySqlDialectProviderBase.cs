using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Data;
using ServiceStack.OrmLite.Converters;
using ServiceStack.OrmLite.MySql.Converters;
using ServiceStack.OrmLite.MySql.DataAnnotations;
using ServiceStack.OrmLite.Support;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.MySql
{
    public abstract class MySqlDialectProviderBase<TDialect> : OrmLiteDialectProviderBase<TDialect> where TDialect : IOrmLiteDialectProvider
    {

        private const string TextColumnDefinition = "TEXT";

        public MySqlDialectProviderBase()
        {
            AutoIncrementDefinition = "AUTO_INCREMENT";
            DefaultValueFormat = " DEFAULT {0}";
            base.SelectIdentitySql = "SELECT LAST_INSERT_ID()";

            InitColumnTypeMap();

            RegisterConverter<string>(new MySqlStringConverter());
            RegisterConverter<char[]>(new MySqlCharArrayConverter());
            RegisterConverter<bool>(new MySqlBoolConverter());

            RegisterConverter<byte>(new MySqlByteConverter());
            RegisterConverter<sbyte>(new MySqlSByteConverter());
            RegisterConverter<short>(new MySqlInt16Converter());
            RegisterConverter<ushort>(new MySqlUInt16Converter());
            RegisterConverter<int>(new MySqlInt32Converter());
            RegisterConverter<uint>(new MySqlUInt32Converter());

            RegisterConverter<decimal>(new MySqlDecimalConverter());

            RegisterConverter<Guid>(new MySqlGuidConverter());
            RegisterConverter<DateTimeOffset>(new MySqlDateTimeOffsetConverter());

            Variables = new Dictionary<string, string>
            {
                { OrmLiteVariables.SystemUtc, "CURRENT_TIMESTAMP" },
                { OrmLiteVariables.MaxText, "LONGTEXT" },
                { OrmLiteVariables.MaxTextUnicode, "LONGTEXT" },
                { OrmLiteVariables.True, SqlBool(true) },                
                { OrmLiteVariables.False, SqlBool(false) },                
            };
        }

        public static string RowVersionTriggerFormat = "{0}RowVersionUpdateTrigger";

        public override string GetLoadChildrenSubSelect<From>(SqlExpression<From> expr)
        {
            return $"SELECT * FROM ({base.GetLoadChildrenSubSelect(expr)}) AS SubQuery";
        }
        public override string ToPostDropTableStatement(ModelDefinition modelDef)
        {
            if (modelDef.RowVersion != null)
            {
                var triggerName = RowVersionTriggerFormat.Fmt(GetTableName(modelDef));
                return "DROP TRIGGER IF EXISTS {0}".Fmt(GetQuotedName(triggerName));
            }
            return null;
        }

        public override string ToPostCreateTableStatement(ModelDefinition modelDef)
        {
            if (modelDef.RowVersion != null)
            {
                var triggerName = RowVersionTriggerFormat.Fmt(modelDef.ModelName);
                var triggerBody = "SET NEW.{0} = OLD.{0} + 1;".Fmt(
                    modelDef.RowVersion.FieldName.SqlColumn(this));

                var sql = "CREATE TRIGGER {0} BEFORE UPDATE ON {1} FOR EACH ROW BEGIN {2} END;".Fmt(
                    triggerName, GetTableName(modelDef), triggerBody);

                return sql;
            }

            return null;
        }

        public override string GetQuotedValue(string paramValue)
        {
            return "'" + paramValue.Replace("\\", "\\\\").Replace("'", @"\'") + "'";
        }

        public override string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

            if (fieldType == typeof(byte[]))
                return "0x" + BitConverter.ToString((byte[])value).Replace("-", "");

            return base.GetQuotedValue(value, fieldType);
        }

        public override string GetTableName(string table, string schema = null) => GetTableName(table, schema, useStrategy: true);

        public override string GetTableName(string table, string schema, bool useStrategy)
        {
            if (useStrategy)
            {
                return schema != null
                    ? $"{NamingStrategy.GetSchemaName(schema)}_{NamingStrategy.GetTableName(table)}"
                    : NamingStrategy.GetTableName(table);
            }
            
            return schema != null
                ? $"{schema}_{table}"
                : table;
        }

        public override string GetQuotedTableName(string tableName, string schema = null)
        {
            return GetQuotedName(GetTableName(tableName, schema));
        }

        public override string GetQuotedName(string name) => $"`{name}`";
        public override string GetQuotedColumnName(string columnName) => $"`{columnName}`";

        public override SqlExpression<T> SqlExpression<T>()
        {
            return new MySqlExpression<T>(this);
        }

        public override string GetQuotedName(string name, string schema) => GetQuotedName(name); //schema name is embedded in table name in MySql

        public override string ToTableNamesStatement(string schema)
        {
            return schema == null 
                ? "SELECT table_name FROM information_schema.tables WHERE table_type='BASE TABLE' AND table_schema = DATABASE()"
                : "SELECT table_name FROM information_schema.tables WHERE table_type='BASE TABLE' AND table_schema = DATABASE() AND table_name LIKE {0}".SqlFmt(this, GetTableName("",schema) + "%");
        }

        public override string ToTableNamesWithRowCountsStatement(bool live, string schema)
        {
            if (live)
                return null;
            
            return schema == null 
                ? "SELECT table_name, table_rows FROM information_schema.tables WHERE table_type='BASE TABLE' AND table_schema = DATABASE()"
                : "SELECT table_name, table_rows FROM information_schema.tables WHERE table_type='BASE TABLE' AND table_schema = DATABASE() AND table_name LIKE {0}".SqlFmt(this, GetTableName("",schema) + "%");
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName, string schema = null)
        {
            var sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {0} AND TABLE_SCHEMA = {1}"
                .SqlFmt(GetTableName(tableName, schema), dbCmd.Connection.Database);

            var result = dbCmd.ExecLongScalar(sql);

            return result > 0;
        }

        public override bool DoesColumnExist(IDbConnection db, string columnName, string tableName, string schema = null)
        {
            tableName = GetTableName(tableName, schema);
            var sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS"
                    + " WHERE TABLE_NAME = @tableName AND COLUMN_NAME = @columnName AND TABLE_SCHEMA = @schema"
                .SqlFmt(tableName, columnName);

            var result = db.SqlScalar<long>(sql, new { tableName, columnName, schema = db.Database });

            return result > 0;
        }

        public override string ToCreateTableStatement(Type tableType)
        {
            var sbColumns = StringBuilderCache.Allocate();
            var sbConstraints = StringBuilderCache.Allocate();

            var modelDef = GetModel(tableType);
            foreach (var fieldDef in CreateTableFieldsStrategy(modelDef))
            {
                if (fieldDef.CustomSelect != null)
                    continue;

                if (sbColumns.Length != 0) sbColumns.Append(", \n  ");

                sbColumns.Append(GetColumnDefinition(fieldDef));
                
                var sqlConstraint = GetCheckConstraint(modelDef, fieldDef);
                if (sqlConstraint != null)
                {
                    sbConstraints.Append(",\n" + sqlConstraint);
                }

                if (fieldDef.ForeignKey == null || OrmLiteConfig.SkipForeignKeys)
                    continue;

                var refModelDef = GetModel(fieldDef.ForeignKey.ReferenceType);
                sbConstraints.AppendFormat(
                    ", \n\n  CONSTRAINT {0} FOREIGN KEY ({1}) REFERENCES {2} ({3})",
                    GetQuotedName(fieldDef.ForeignKey.GetForeignKeyName(modelDef, refModelDef, NamingStrategy, fieldDef)),
                    GetQuotedColumnName(fieldDef.FieldName),
                    GetQuotedTableName(refModelDef),
                    GetQuotedColumnName(refModelDef.PrimaryKey.FieldName));

                if (!string.IsNullOrEmpty(fieldDef.ForeignKey.OnDelete))
                    sbConstraints.AppendFormat(" ON DELETE {0}", fieldDef.ForeignKey.OnDelete);

                if (!string.IsNullOrEmpty(fieldDef.ForeignKey.OnUpdate))
                    sbConstraints.AppendFormat(" ON UPDATE {0}", fieldDef.ForeignKey.OnUpdate);
            }

            var uniqueConstraints = GetUniqueConstraints(modelDef);
            if (uniqueConstraints != null)
            {
                sbConstraints.Append(",\n" + uniqueConstraints);
            }

            var sql = $"CREATE TABLE {GetQuotedTableName(modelDef)} \n(\n  {StringBuilderCache.ReturnAndFree(sbColumns)}{StringBuilderCacheAlt.ReturnAndFree(sbConstraints)} \n); \n";

            return sql;
        }
        
        public override bool DoesSchemaExist(IDbCommand dbCmd, string schemaName)
        {
            // to maintain existing schema table prefixing behaviour, all schema will exist
            return true;
            dbCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{schemaName.SqlParam()}')";
            var query = dbCmd.ExecuteScalar();
            return query as bool? ?? false;
        }

        public override string ToCreateSchemaStatement(string schemaName)
        {
            // to maintain existing table prefixing behaviour, just return 1;
            return "SELECT 1";
            var sql = $"CREATE SCHEMA {GetSchemaName(schemaName)}";
            return sql;
        }

        public override string GetColumnDefinition(FieldDefinition fieldDef)
        {
            if (fieldDef.PropertyInfo?.HasAttribute<TextAttribute>() == true)
            {
                var sql = StringBuilderCache.Allocate();
                sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldDef.FieldName), TextColumnDefinition);
                sql.Append(fieldDef.IsNullable ? " NULL" : " NOT NULL");
                return StringBuilderCache.ReturnAndFree(sql);
            }

            var ret = base.GetColumnDefinition(fieldDef);
            if (fieldDef.IsRowVersion)
                return $"{ret} DEFAULT 1";

            return ret;
        }

        public override string SqlConflict(string sql, string conflictResolution)
        {
            var parts = sql.SplitOnFirst(' ');
            return parts[0] + " " + conflictResolution + " " + parts[1];
        }

        public override string SqlCurrency(string fieldOrValue, string currencySymbol) =>
            SqlConcat(new[] { "'" + currencySymbol + "'", "cast(" + fieldOrValue + " as decimal(15,2))" });

        public override string SqlCast(object fieldOrValue, string castAs) => 
            castAs == Sql.VARCHAR
                ? $"CAST({fieldOrValue} AS CHAR(1000))"
                : $"CAST({fieldOrValue} AS {castAs})";

        public override string SqlBool(bool value) => value ? "1" : "0";

        protected DbConnection Unwrap(IDbConnection db)
        {
            return (DbConnection)db.ToDbConnection();
        }

        protected DbCommand Unwrap(IDbCommand cmd)
        {
            return (DbCommand)cmd.ToDbCommand();
        }

        protected DbDataReader Unwrap(IDataReader reader)
        {
            return (DbDataReader)reader;
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