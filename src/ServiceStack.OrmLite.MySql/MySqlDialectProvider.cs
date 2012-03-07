using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using ServiceStack.Common.Extensions;
using ServiceStack.OrmLite.MySql.DataAnnotations;

namespace ServiceStack.OrmLite.MySql
{
    public class MySqlDialectProvider : OrmLiteDialectProviderBase
    {
        public static MySqlDialectProvider Instance = new MySqlDialectProvider();

        private string TextColumnDefinition = "TEXT";

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

		public override bool DoesTableExist(IDbCommand dbCmd, string tableName)
		{
			//Same as SQL Server apparently?
			var sql = ("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES " +
				"WHERE TABLE_NAME = {0} AND " +
				"TABLE_SCHEMA = {1}")
				.SqlFormat(tableName, dbCmd.Connection.Database);

			//if (!string.IsNullOrEmpty(schemaName))
			//    sql += " AND TABLE_SCHEMA = {0}".SqlFormat(schemaName);

			dbCmd.CommandText = sql;
			var result = dbCmd.GetLongScalar();

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

                if (fieldDef.ReferencesType == null) continue;

                var refModelDef = GetModel(fieldDef.ReferencesType);
                sbConstraints.AppendFormat(
                    ", \n\n  CONSTRAINT {0} FOREIGN KEY ({1}) REFERENCES {2} ({3})",
                    GetQuotedName(string.Format("FK_{0}_{1}_{2}", modelDef.ModelName,
                                                                 refModelDef.ModelName, fieldDef.FieldName)),
                    GetQuotedColumnName(fieldDef.FieldName),
                    GetQuotedTableName(refModelDef),
                    GetQuotedColumnName(refModelDef.PrimaryKey.FieldName));
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

            return base.GetColumnDefinition(
                fieldDefinition.FieldName, 
                fieldDefinition.FieldType,
                fieldDefinition.IsPrimaryKey, 
                fieldDefinition.AutoIncrement, 
                fieldDefinition.IsNullable, 
                fieldDefinition.FieldLength, 
                null, 
                fieldDefinition.DefaultValue);
        }
	}
}
