using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.SqlServer 
{
    public class SqlServerOrmLiteDialectProvider : OrmLiteDialectProviderBase<SqlServerOrmLiteDialectProvider>
	{
		public static SqlServerOrmLiteDialectProvider Instance = new SqlServerOrmLiteDialectProvider();

		private static DateTime timeSpanOffset = new DateTime(1900,01,01);

		public SqlServerOrmLiteDialectProvider()
		{
			base.AutoIncrementDefinition = "IDENTITY(1,1)";
			StringColumnDefinition = UseUnicode ?  "NVARCHAR(4000)" : "VARCHAR(8000)";
			base.GuidColumnDefinition = "UniqueIdentifier";
			base.RealColumnDefinition = "FLOAT";
		    base.BoolColumnDefinition = "BIT";
			base.DecimalColumnDefinition = "DECIMAL(38,6)";
			base.TimeColumnDefinition = "TIME"; //SQLSERVER 2008+
		    base.BlobColumnDefinition = "VARBINARY(MAX)";

			base.InitColumnTypeMap();
		}

        public override string GetQuotedParam(string paramValue)
        {
            return (UseUnicode ? "N'" : "'") + paramValue.Replace("'", "''") + "'";
        }

		public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
		{
			var isFullConnectionString = connectionString.Contains(";");

			if (!isFullConnectionString)
			{
				var filePath = connectionString;

				var filePathWithExt = filePath.ToLower().EndsWith(".mdf")
					? filePath 
					: filePath + ".mdf";

				var fileName = Path.GetFileName(filePathWithExt);
				var dbName = fileName.Substring(0, fileName.Length - ".mdf".Length);

				connectionString = string.Format(
				@"Data Source=.\SQLEXPRESS;AttachDbFilename={0};Initial Catalog={1};Integrated Security=True;User Instance=True;",
					filePathWithExt, dbName);
			}

			if (options != null)
			{
				foreach (var option in options)
				{
					if (option.Key.ToLower() == "read only")
					{
						if (option.Value.ToLower() == "true")
						{
							connectionString += "Mode = Read Only;";
						}
						continue;
					}
					connectionString += option.Key + "=" + option.Value + ";";
				}
			}

			return new SqlConnection(connectionString);
		}

        public override string GetQuotedTableName(ModelDefinition modelDef)
        {
            if (!modelDef.IsInSchema)
                return base.GetQuotedTableName(modelDef);

            var escapedSchema = modelDef.Schema.Replace(".", "\".\"");
            return string.Format("\"{0}\".\"{1}\"", escapedSchema, NamingStrategy.GetTableName(modelDef.ModelName));
        }

		public override object ConvertDbValue(object value, Type type)
		{
			try
			{
				if (value == null || value is DBNull) return null;

				if (type == typeof(bool) && !(value is bool))
				{
					var intVal = Convert.ToInt32(value.ToString());
					return intVal != 0;
				}

				if (type == typeof(TimeSpan) && value is DateTime)
				{
					var dateTimeValue = (DateTime)value;
					return dateTimeValue - timeSpanOffset;
				}

                if (type == typeof(byte[]))
                    return value;

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
				return string.Format("CAST('{0}' AS UNIQUEIDENTIFIER)", guidValue);
			}
			if (fieldType == typeof(DateTime))
			{
				var dateValue = (DateTime)value;
				const string iso8601Format = "yyyyMMdd HH:mm:ss.fff";
				return base.GetQuotedValue(dateValue.ToString(iso8601Format), typeof(string));
			}
			if (fieldType == typeof(bool))
			{
				var boolValue = (bool)value;
				return base.GetQuotedValue(boolValue ? 1 : 0, typeof(int));
			}
			if(fieldType == typeof(string)) {
                return GetQuotedParam(value.ToString());
			}

            if (fieldType == typeof(byte[]))
            {
                return "0x" + BitConverter.ToString((byte[])value).Replace("-", "");
            }

			return base.GetQuotedValue(value, fieldType);


		}

		public override long GetLastInsertId(IDbCommand dbCmd)
		{
			dbCmd.CommandText = "SELECT SCOPE_IDENTITY()";
			return dbCmd.GetLongScalar();
		}

		public override SqlExpressionVisitor<T> ExpressionVisitor<T>()
		{
			return new SqlServerExpressionVisitor<T>();
		}

		public override bool DoesTableExist(IDbCommand dbCmd, string tableName)
		{
			var sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = {0}"
				.SqlFormat(tableName);

			//if (!string.IsNullOrEmpty(schemaName))
			//    sql += " AND TABLE_SCHEMA = {0}".SqlFormat(schemaName);

			dbCmd.CommandText = sql;
			var result = dbCmd.GetLongScalar();
			
			return result > 0;
		}

        public override bool UseUnicode
        {
            get
            {
                return useUnicode;
            }
            set
            {
                useUnicode = value;
                if (useUnicode && this.DefaultStringLength > 4000)
                {
                    this.DefaultStringLength = 4000;
                }

                // UpdateStringColumnDefinitions(); is called by changing DefaultStringLength 
            }
        }
	}
}