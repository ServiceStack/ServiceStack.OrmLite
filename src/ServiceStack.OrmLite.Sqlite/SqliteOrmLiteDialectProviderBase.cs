using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using ServiceStack.OrmLite.Sqlite.Converters;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Sqlite
{
    public abstract class SqliteOrmLiteDialectProviderBase : OrmLiteDialectProviderBase<SqliteOrmLiteDialectProviderBase>
    {
        protected SqliteOrmLiteDialectProviderBase()
        {
            base.SelectIdentitySql = "SELECT last_insert_rowid()";

            base.InitColumnTypeMap();

            OrmLiteConfig.DeoptimizeReader = true;
            base.RegisterConverter<DateTime>(new SqliteSystemDataDateTimeConverter());
            //Old behavior using native sqlite3.dll
            //base.RegisterConverter<DateTime>(new SqliteNativeDateTimeConverter());

            base.RegisterConverter<string>(new SqliteStringConverter());
            base.RegisterConverter<DateTimeOffset>(new SqliteDateTimeOffsetConverter());
            base.RegisterConverter<Guid>(new SqliteGuidConverter());
            base.RegisterConverter<bool>(new SqliteBoolConverter());
            base.RegisterConverter<byte[]>(new SqliteByteArrayConverter());
#if NETSTANDARD1_3            
            base.RegisterConverter<char>(new SqliteCharConverter());
#endif
            this.Variables = new Dictionary<string, string>
            {
                { OrmLiteVariables.SystemUtc, "CURRENT_TIMESTAMP" },
            };
        }

        public static string Password { get; set; }
        public static bool UTF8Encoded { get; set; }
        public static bool ParseViaFramework { get; set; }

        public static string RowVersionTriggerFormat = "{0}RowVersionUpdateTrigger";

        public override string ToPostDropTableStatement(ModelDefinition modelDef)
        {
            if (modelDef.RowVersion != null)
            {
                var triggerName = GetTriggerName(modelDef);
                return $"DROP TRIGGER IF EXISTS {GetQuotedName(triggerName)}";
            }

            return null;
        }

        private string GetTriggerName(ModelDefinition modelDef)
        {
            return RowVersionTriggerFormat.Fmt(GetTableName(modelDef));
        }

        public override string ToPostCreateTableStatement(ModelDefinition modelDef)
        {
            if (modelDef.RowVersion != null)
            {
                var triggerName = GetTriggerName(modelDef);
                var tableName = GetTableName(modelDef);
                var triggerBody = string.Format("UPDATE {0} SET {1} = OLD.{1} + 1 WHERE {2} = NEW.{2};",
                    tableName, 
                    modelDef.RowVersion.FieldName.SqlColumn(this), 
                    modelDef.PrimaryKey.FieldName.SqlColumn(this));

                var sql = $"CREATE TRIGGER {triggerName} BEFORE UPDATE ON {tableName} FOR EACH ROW BEGIN {triggerBody} END;";

                return sql;
            }

            return null;
        }

        public static string CreateFullTextCreateTableStatement(object objectWithProperties)
        {
            var sbColumns = StringBuilderCache.Allocate();
            foreach (var propertyInfo in objectWithProperties.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var columnDefinition = (sbColumns.Length == 0)
                    ? $"{propertyInfo.Name} TEXT PRIMARY KEY"
                    : $", {propertyInfo.Name} TEXT";

                sbColumns.AppendLine(columnDefinition);
            }

            var tableName = objectWithProperties.GetType().Name;
            var sql = $"CREATE VIRTUAL TABLE \"{tableName}\" USING FTS3 ({StringBuilderCache.ReturnAndFree(sbColumns)});";

            return sql;
        }

        public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
        {
            var isFullConnectionString = connectionString.Contains(";");
            var connString = StringBuilderCache.Allocate();
            if (!isFullConnectionString)
            {
                if (connectionString != ":memory:")
                {
                    var existingDir = Path.GetDirectoryName(connectionString);
                    if (!string.IsNullOrEmpty(existingDir) && !Directory.Exists(existingDir))
                    {
                        Directory.CreateDirectory(existingDir);
                    }
                }
#if NETSTANDARD1_3
                connString.AppendFormat(@"Data Source={0};", connectionString.Trim());
#else
                connString.AppendFormat(@"Data Source={0};Version=3;New=True;Compress=True;", connectionString.Trim());
#endif
            }
            else
            {
                connString.Append(connectionString);
            }
            if (!string.IsNullOrEmpty(Password))
            {
                connString.AppendFormat("Password={0};", Password);
            }
            if (UTF8Encoded)
            {
                connString.Append("UseUTF16Encoding=True;");
            }

            if (options != null)
            {
                foreach (var option in options)
                {
                    connString.AppendFormat("{0}={1};", option.Key, option.Value);
                }
            }

            return CreateConnection(StringBuilderCache.ReturnAndFree(connString));
        }

        protected abstract IDbConnection CreateConnection(string connectionString);

        public override string GetSchemaName(string schema)
        {
            return schema != null
                ? NamingStrategy.GetSchemaName(schema).Replace(".", "_")
                : NamingStrategy.GetSchemaName(schema);
        }

        public override string GetTableName(string table, string schema=null)
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

        public override SqlExpression<T> SqlExpression<T>()
        {
            return new SqliteExpression<T>(this);
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName, string schema = null)
        {
            var sql = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name = {0}"
                .SqlFmt(GetTableName(tableName, schema));

            dbCmd.CommandText = sql;
            var result = dbCmd.LongScalar();

            return result > 0;
        }

        public override bool DoesColumnExist(IDbConnection db, string columnName, string tableName, string schema = null)
        {
            var sql = "PRAGMA table_info({0})"
                .SqlFmt(GetTableName(tableName, schema));

            var columns = db.SqlList<Dictionary<string, object>>(sql);
            foreach (var column in columns)
            {
                object name;
                if (column.TryGetValue("name", out name) && name.ToString().EqualsIgnoreCase(columnName))
                    return true;
            }
            return false;
        }

        public override string GetColumnDefinition(FieldDefinition fieldDef)
        {
            // http://www.sqlite.org/lang_createtable.html#rowid
            var ret = base.GetColumnDefinition(fieldDef);
            if (fieldDef.IsPrimaryKey)
                return ret.Replace(" BIGINT ", " INTEGER ");
            if (fieldDef.IsRowVersion)
                return ret + " DEFAULT 1";

            return ret;
        }
    }

    public static class SqliteExtensions
    {
        public static IOrmLiteDialectProvider Configure(this IOrmLiteDialectProvider provider,
            string password = null, bool parseViaFramework = false, bool utf8Encoding = false)
        {
            if (password != null)
                SqliteOrmLiteDialectProviderBase.Password = password;
            if (parseViaFramework)
                SqliteOrmLiteDialectProviderBase.ParseViaFramework = true;
            if (utf8Encoding)
                SqliteOrmLiteDialectProviderBase.UTF8Encoded = true;

            return provider;
        }
    }
}
