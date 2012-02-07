﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Npgsql;

namespace ServiceStack.OrmLite.PostgreSQL
{
	public class PostgreSQLDialectProvider : OrmLiteDialectProviderBase
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
			base.DefaultStringLength = 255;
			base.ParamString = ":";
			base.BlobColumnDefinition = "bytea";
			base.RealColumnDefinition = "double precision";
			base.InitColumnTypeMap();
			DbTypes<TimeSpan>.Set(DbType.Time, "Interval");
			DbTypes<TimeSpan?>.Set(DbType.Time, "Interval");
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
				fieldDefinition = string.Format(StringLengthColumnDefinitionFormat, fieldLength.GetValueOrDefault(DefaultStringLength));
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
				else if (!DbTypes.ColumnTypeMap.TryGetValue(fieldType, out fieldDefinition))
				{
					fieldDefinition = this.GetUndefinedColumnDefintion(fieldType);
				}
			}

			var sql = new StringBuilder();
			sql.AppendFormat("{0} {1}", GetNameDelimited(fieldName), fieldDefinition);

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

		public override string EscapeParam(object paramValue)
		{
			return paramValue.ToString().Replace("'", @"\'");
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

		public override string GetTableNameDelimited(ModelDefinition modelDef)
		{
			return string.Format("\"{0}\"", modelDef.ModelName);
		}

		public override string GetNameDelimited(string columnName)
		{
			return string.Format("\"{0}\"", columnName);
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
	}
}
