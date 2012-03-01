using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

namespace ServiceStack.OrmLite.MySql
{
    public class MySqlDialectProvider : OrmLiteDialectProviderBase
    {
        public static MySqlDialectProvider Instance = new MySqlDialectProvider();

        private MySqlDialectProvider()
        {
            base.AutoIncrementDefinition = "AUTO_INCREMENT";
            base.IntColumnDefinition = "int(11)";
            base.BoolColumnDefinition = "bit(1)";
            base.TimeColumnDefinition = "time";
            base.DecimalColumnDefinition = "decimal(38,6)";
            base.GuidColumnDefinition = "char(32)";
            base.DefaultStringLength = 255;
            base.InitColumnTypeMap();
        }

        public override string EscapeParam(object paramValue)
        {
            return paramValue.ToString().Replace("\\", "\\\\").Replace("'", @"\'");
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
                const string iso8601Format = "yyyy-MM-dd HH:mm:ss.fff";
                return base.GetQuotedValue(dateValue.ToString(iso8601Format), typeof(string));
            }
            if (fieldType == typeof(Guid))
            {
                var guidValue = (Guid)value;
                return base.GetQuotedValue(guidValue.ToString("N"), typeof(string));
            }

            return base.GetQuotedValue(value, fieldType);
        }

        public override object ConvertDbValue(object value, Type type)
        {
            if (value == null || value is DBNull) return null;

            if (type == typeof(bool))
            {
                var intVal = int.Parse(value.ToString());
                return intVal != 0;
            }

            return base.ConvertDbValue(value, type);
        }

        public override string GetQuotedTableName(ModelDefinition modelDef)
        {
            return string.Format("`{0}`", NamingStrategy.GetTableName(modelDef.ModelName));
        }

		public override string GetQuotedColumnName(string columnName)
		{
			return string.Format("`{0}`", NamingStrategy.GetColumnName(columnName));
		}

        public override string GetQuotedName(string name)
        {
			return string.Format("`{0}`", name);
        }

        public override long GetLastInsertId(IDbCommand command)
        {
            command.CommandText = "SELECT LAST_INSERT_ID()";
            var result = command.ExecuteScalar();
            if (result is DBNull) return default(long);
            return Convert.ToInt64(result);
        }
        
        public override SqlExpressionVisitor<T> ExpressionVisitor<T> ()
		{
			return new MySqlExpressionVisitor<T>();
		}
    }
}
