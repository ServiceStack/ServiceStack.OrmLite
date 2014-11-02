using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ServiceStack.Data;
using ServiceStack.OrmLite.MySql.DataAnnotations;
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
            base.IntColumnDefinition = "int(11)";
            base.BoolColumnDefinition = "tinyint(1)";
            base.DecimalColumnDefinition = "decimal(38,6)";
            base.GuidColumnDefinition = "char(36)";
            base.DefaultStringLength = 255;
            base.MaxStringColumnDefinition = "TEXT";
            base.InitColumnTypeMap();
            base.DefaultValueFormat = " DEFAULT '{0}'";
            base.SelectIdentitySql = "SELECT LAST_INSERT_ID()";
        }

        public override void OnAfterInitColumnTypeMap()
        {
            DbTypeMap.Set<Guid>(DbType.String, GuidColumnDefinition);
            DbTypeMap.Set<Guid?>(DbType.String, GuidColumnDefinition);
            DbTypeMap.Set<DateTimeOffset>(DbType.DateTimeOffset, StringColumnDefinition);
            DbTypeMap.Set<DateTimeOffset?>(DbType.DateTimeOffset, StringColumnDefinition);
        }

        public static string RowVersionTriggerFormat = "{0}RowVersionUpdateTrigger";

        public override string ToPostDropTableStatement(ModelDefinition modelDef)
        {
            if (modelDef.RowVersion != null)
            {
                var triggerName = RowVersionTriggerFormat.Fmt(modelDef.ModelName);
                return "DROP TRIGGER IF EXISTS {0}".Fmt(GetQuotedTableName(triggerName));
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
                    triggerName, modelDef.ModelName, triggerBody);

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

            if (fieldType == typeof(DateTime))
            {
                var dateValue = (DateTime)value;
                /*
                 * ms not contained in format. MySql ignores ms part anyway
                 * 
                 * for more details see: http://dev.mysql.com/doc/refman/5.1/en/datetime.html
                 */
                const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

                return base.GetQuotedValue(dateValue.ToString(dateTimeFormat), typeof(string));
            }

            if (fieldType == typeof(byte[]))
            {
                return "0x" + BitConverter.ToString((byte[])value).Replace("-", "");
            }

            return base.GetQuotedValue(value, fieldType);
        }

        public override object ConvertDbValue(object value, Type type)
        {
            if (value == null || value is DBNull) return null;

            if (type == typeof(bool))
            {
                return
                    value is bool
                        ? value
                        : (int.Parse(value.ToString()) != 0); //backward compatibility (prev version mapped bool as bit(1))
            }

            if (type == typeof(byte[]))
                return value;

            return base.ConvertDbValue(value, type);
        }

        public override string GetQuotedTableName(ModelDefinition modelDef)
        {
            return string.Format("`{0}`", NamingStrategy.GetTableName(modelDef.ModelName));
        }

        public override string GetQuotedTableName(string tableName)
        {
            return string.Format("`{0}`", NamingStrategy.GetTableName(tableName));
        }

        public override string GetQuotedColumnName(string columnName)
        {
            return string.Format("`{0}`", NamingStrategy.GetColumnName(columnName));
        }

        public override string GetQuotedName(string name)
        {
            return string.Format("`{0}`", name);
        }

        public override SqlExpression<T> SqlExpression<T>()
        {
            return new MySqlExpression<T>(this);
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName)
        {
            //Same as SQL Server apparently?
            var sql = ("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES " +
                "WHERE TABLE_NAME = {0} AND " +
                "TABLE_SCHEMA = {1}")
                .SqlFmt(tableName, dbCmd.Connection.Database);

            //if (!string.IsNullOrEmpty(schemaName))
            //    sql += " AND TABLE_SCHEMA = {0}".SqlFmt(schemaName);

            dbCmd.CommandText = sql;
            var result = dbCmd.LongScalar();

            return result > 0;
        }

        public override string ToCreateTableStatement(Type tableType)
        {
            var sbColumns = new StringBuilder();
            var sbConstraints = new StringBuilder();

            var modelDef = GetModel(tableType);
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (sbColumns.Length != 0) sbColumns.Append(", \n  ");

                sbColumns.Append(GetColumnDefinition(fieldDef));

                if (fieldDef.ForeignKey == null) continue;

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
            var sql = new StringBuilder(string.Format(
                "CREATE TABLE {0} \n(\n  {1}{2} \n); \n", GetQuotedTableName(modelDef), sbColumns, sbConstraints));

            return sql.ToString();
        }

        public string GetColumnDefinition(FieldDefinition fieldDefinition)
        {
            if (fieldDefinition.PropertyInfo.FirstAttribute<TextAttribute>() != null)
            {
                var sql = new StringBuilder();
                sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldDefinition.FieldName), TextColumnDefinition);
                sql.Append(fieldDefinition.IsNullable ? " NULL" : " NOT NULL");
                return sql.ToString();
            }

            var ret = base.GetColumnDefinition(
                fieldDefinition.FieldName,
                fieldDefinition.ColumnType,
                fieldDefinition.IsPrimaryKey,
                fieldDefinition.AutoIncrement,
                fieldDefinition.IsNullable,
                fieldDefinition.IsRowVersion,
                fieldDefinition.FieldLength,
                null,
                fieldDefinition.DefaultValue,
                fieldDefinition.CustomFieldDefinition);

            if (fieldDefinition.IsRowVersion)
                return ret + " DEFAULT 1";

            return ret;
        }

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
