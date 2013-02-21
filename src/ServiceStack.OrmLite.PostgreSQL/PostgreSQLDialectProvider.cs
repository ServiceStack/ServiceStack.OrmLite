using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Npgsql;

namespace ServiceStack.OrmLite.PostgreSQL
{
    public class PostgreSQLDialectProvider : OrmLiteDialectProviderBase<PostgreSQLDialectProvider>
	{
		public static PostgreSQLDialectProvider Instance = new PostgreSQLDialectProvider();

		private PostgreSQLDialectProvider()
		{
			base.AutoIncrementDefinition = "";
			base.IntColumnDefinition = "integer";
			base.BoolColumnDefinition = "boolean";
			base.TimeColumnDefinition = "time";
			base.DateTimeColumnDefinition = "timestamp";
			base.DecimalColumnDefinition = "numeric(38,6)";
			base.GuidColumnDefinition = "uuid";
			base.ParamString = ":";
			base.BlobColumnDefinition = "bytea";
			base.RealColumnDefinition = "double precision";
			base.StringLengthColumnDefinitionFormat = "text";
			base.InitColumnTypeMap();

            DbTypeMap.Set<TimeSpan>(DbType.Time, "Interval");
            DbTypeMap.Set<TimeSpan?>(DbType.Time, "Interval");
		}

		public override string GetColumnDefinition(
			string fieldName,
			Type fieldType,
			bool isPrimaryKey,
			bool autoIncrement,
			bool isNullable,
			int? fieldLength,
			int? scale,
			string defaultValue)
		{
			string fieldDefinition = null;
			if (fieldType == typeof(string))
			{
				if (fieldLength != null)
				{
					fieldDefinition = UseUnicode
						? string.Format(base.StringLengthUnicodeColumnDefinitionFormat, fieldLength)
						: string.Format(base.StringLengthNonUnicodeColumnDefinitionFormat, fieldLength);
				}
				else
				{
					fieldDefinition = StringLengthColumnDefinitionFormat;
				}
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

        public override string GetQuotedParam(string paramValue)
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
		
			return base.ConvertDbValue(value, type);
		}

		public override long GetLastInsertId(IDbCommand command)
		{
			command.CommandText = "SELECT LASTVAL()";
			var result = command.ExecuteScalar();
			if (result is DBNull)
				return default(long);
			return Convert.ToInt64(result);
		}
		
		public override SqlExpressionVisitor<T> ExpressionVisitor<T>()
		{
			return new PostgreSQLExpressionVisitor<T>();
		}

		public override bool DoesTableExist(IDbCommand dbCmd, string tableName)
		{
			var sql = "SELECT COUNT(*) FROM pg_class WHERE relname = {0}"
				.SqlFormat(tableName);

			dbCmd.CommandText = sql;
			var result = dbCmd.GetLongScalar();

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
	}
}
