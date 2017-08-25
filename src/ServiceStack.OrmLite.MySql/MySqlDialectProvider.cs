using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ServiceStack.Data;
using ServiceStack.OrmLite.Converters;
using ServiceStack.OrmLite.MySql.Converters;
using ServiceStack.OrmLite.MySql.DataAnnotations;
using ServiceStack.OrmLite.Support;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.MySql
{
    public class MySqlDialectProvider : OrmLiteDialectProviderBase<MySqlDialectProvider>
    {
        public static MySqlDialectProvider Instance = new MySqlDialectProvider();

        private const string TextColumnDefinition = "TEXT";

        public MySqlDialectProvider()
        {
            base.AutoIncrementDefinition = "AUTO_INCREMENT";
            base.DefaultValueFormat = " DEFAULT {0}";
            base.SelectIdentitySql = "SELECT LAST_INSERT_ID()";

            base.InitColumnTypeMap();

            base.RegisterConverter<string>(new MySqlStringConverter());
            base.RegisterConverter<char[]>(new MySqlCharArrayConverter());
            base.RegisterConverter<bool>(new MySqlBoolConverter());

            base.RegisterConverter<byte>(new MySqlByteConverter());
            base.RegisterConverter<sbyte>(new MySqlSByteConverter());
            base.RegisterConverter<short>(new MySqlInt16Converter());
            base.RegisterConverter<ushort>(new MySqlUInt16Converter());
            base.RegisterConverter<int>(new MySqlInt32Converter());
            base.RegisterConverter<uint>(new MySqlUInt32Converter());

            base.RegisterConverter<decimal>(new MySqlDecimalConverter());

            base.RegisterConverter<Guid>(new MySqlGuidConverter());
            base.RegisterConverter<DateTime>(new MySqlDateTimeConverter());
            base.RegisterConverter<DateTimeOffset>(new MySqlDateTimeOffsetConverter());

            this.Variables = new Dictionary<string, string>
            {
                { OrmLiteVariables.SystemUtc, "CURRENT_TIMESTAMP" },
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

        public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
        {
            return new MySqlConnection(connectionString);
        }

        public override string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

            if (fieldType == typeof(byte[]))
                return "0x" + BitConverter.ToString((byte[])value).Replace("-", "");

            return base.GetQuotedValue(value, fieldType);
        }

        public override string GetTableName(string table, string schema = null)
        {
            return schema != null
                ? string.Format("{0}_{1}",
                    NamingStrategy.GetSchemaName(schema),
                    NamingStrategy.GetTableName(table))
                : NamingStrategy.GetTableName(table);
        }

        public override string GetQuotedTableName(string tableName, string schema = null)
        {
            return GetQuotedName(GetTableName(tableName, schema));
        }

        public override string GetQuotedName(string name)
        {
            return string.Format("`{0}`", name);
        }

        public override SqlExpression<T> SqlExpression<T>()
        {
            return new MySqlExpression<T>(this);
        }

        public override IDbDataParameter CreateParam()
        {
            return new MySqlParameter();
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
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.CustomSelect != null)
                    continue;

                if (sbColumns.Length != 0) sbColumns.Append(", \n  ");

                sbColumns.Append(GetColumnDefinition(fieldDef));

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
            var sql = string.Format(
                "CREATE TABLE {0} \n(\n  {1}{2} \n); \n", GetQuotedTableName(modelDef), 
                StringBuilderCache.ReturnAndFree(sbColumns), 
                StringBuilderCacheAlt.ReturnAndFree(sbConstraints));

            return sql;
        }

        public string GetColumnDefinition(FieldDefinition fieldDef)
        {
            if (fieldDef.PropertyInfo.FirstAttribute<TextAttribute>() != null)
            {
                var sql = StringBuilderCache.Allocate();
                sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldDef.FieldName), TextColumnDefinition);
                sql.Append(fieldDef.IsNullable ? " NULL" : " NOT NULL");
                return StringBuilderCache.ReturnAndFree(sql);
            }

            var ret = base.GetColumnDefinition(fieldDef);
            if (fieldDef.IsRowVersion)
                return ret + " DEFAULT 1";

            return ret;
        }

        public override string SqlCurrency(string fieldOrValue, string currencySymbol) => 
            SqlConcat(new []{ "'" + currencySymbol + "'", "cast(" + fieldOrValue + " as decimal(15,2))" });

        protected MySqlConnection Unwrap(IDbConnection db)
        {
            return (MySqlConnection)db.ToDbConnection();
        }

        protected MySqlCommand Unwrap(IDbCommand cmd)
        {
            return (MySqlCommand)cmd.ToDbCommand();
        }

        protected MySqlDataReader Unwrap(IDataReader reader)
        {
            return (MySqlDataReader)reader;
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
