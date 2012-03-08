using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Mono.Data.Sqlite;
using ServiceStack.Text.Common;

namespace ServiceStack.OrmLite.Sqlite
{
	public class SqliteOrmLiteDialectProvider 
		: OrmLiteDialectProviderBase
	{
		public static SqliteOrmLiteDialectProvider Instance = new SqliteOrmLiteDialectProvider();

		public SqliteOrmLiteDialectProvider()
		{
			base.DateTimeColumnDefinition = base.StringColumnDefinition;
			base.BoolColumnDefinition = base.IntColumnDefinition;
			base.GuidColumnDefinition = "CHAR(32)";


			base.InitColumnTypeMap();
		}

		public static string CreateFullTextCreateTableStatement(object objectWithProperties)
		{
			var sbColumns = new StringBuilder();
			foreach (var propertyInfo in objectWithProperties.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var columnDefinition = (sbColumns.Length == 0)
					? string.Format("{0} TEXT PRIMARY KEY", propertyInfo.Name)
					: string.Format(", {0} TEXT", propertyInfo.Name);

				sbColumns.AppendLine(columnDefinition);
			}

			var tableName = objectWithProperties.GetType().Name;
			var sql = string.Format("CREATE VIRTUAL TABLE \"{0}\" USING FTS3 ({1});", tableName, sbColumns);

			return sql;
		}

		public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
		{
			var isFullConnectionString = connectionString.Contains(";");

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
				connectionString =
					@"Data Source=" + connectionString + ";Version=3;New=True;Compress=True;";
			}

			if (options != null)
			{
				foreach (var option in options)
				{
					connectionString += option.Key + "=" + option.Value + ";";
				}
			}

			return new SqliteConnection(connectionString);
		}

        public override string GetQuotedTableName(ModelDefinition modelDef)
        {
            if (!modelDef.IsInSchema)
                return base.GetQuotedTableName(modelDef);

            return string.Format("\"{0}_{1}\"", modelDef.Schema, modelDef.ModelName);
        }

		public override object ConvertDbValue(object value, Type type)
		{
			if (value == null || value is DBNull) return null;

			if (type == typeof(bool))
			{
				var intVal = int.Parse(value.ToString());
				return intVal != 0;
			}
			if (type == typeof(TimeSpan))
			{
				var dateValue = value as DateTime?;
				if (dateValue != null)
				{
					var now = DateTime.Now;
					var todayWithoutTime = new DateTime(now.Year, now.Month, now.Day);
					var ts = dateValue.Value - todayWithoutTime;
					return ts;
				}
			}

			try
			{
				return base.ConvertDbValue(value, type);
			}
			catch (Exception ex)
			{				
				throw;
			}
		}

		public override string GetQuotedValue(object value, Type fieldType)
		{
			if (value == null) return "NULL";

			if (fieldType == typeof(Guid))
			{
				var guidValue = (Guid)value;
				return base.GetQuotedValue(guidValue.ToString("N"), typeof(string));
			}
			if (fieldType == typeof(DateTime))
			{
				var dateValue = (DateTime)value;
				return base.GetQuotedValue(
					DateTimeSerializer.ToShortestXsdDateTimeString(dateValue), 
					typeof(string));
			}
			if (fieldType == typeof(bool))
			{
				var boolValue = (bool)value;
				return base.GetQuotedValue(boolValue ? 1 : 0, typeof(int));
			}

			return base.GetQuotedValue(value, fieldType);
		}

		public override long GetLastInsertId(IDbCommand dbCmd)
		{
			dbCmd.CommandText = "SELECT last_insert_rowid()";
			var result = dbCmd.ExecuteScalar();
			return (long)result;
		}
		
		public override SqlExpressionVisitor<T> ExpressionVisitor<T>()
		{
			return new SqliteExpressionVisitor<T>();
		}

		public override bool DoesTableExist(IDbCommand dbCmd, string tableName)
		{
			var sql = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name = {0}"
				.SqlFormat(tableName);

			dbCmd.CommandText = sql;
			var result = dbCmd.GetLongScalar();

			return result > 0;
		}

		public override string GetColumnDefinition(string fieldName, Type fieldType, bool isPrimaryKey, bool autoIncrement, bool isNullable, int? fieldLength, int? scale, string defaultValue)
		{
			var ret = base.GetColumnDefinition(fieldName, fieldType, isPrimaryKey, autoIncrement, isNullable, fieldLength, scale, defaultValue);
			if (isPrimaryKey)
				return ret.Replace(" BIGINT ", " INTEGER ");
			return ret;
		}
	}
}