//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2010 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using ServiceStack.Logging;
using ServiceStack.Text;
using System.Diagnostics;
using ServiceStack.Common.Extensions;

namespace ServiceStack.OrmLite
{
 
    public abstract class OrmLiteDialectProviderBase
        : IOrmLiteDialectProvider
    {
		private static readonly ILog Log = LogManager.GetLogger(typeof(IOrmLiteDialectProvider));

        [Conditional("DEBUG")]
        private static void LogDebug(string fmt, params object[] args)
        {
            if (args.Length > 0)
                Log.DebugFormat(fmt, args);
            else
                Log.Debug(fmt);
        }
		
		
        #region ADO.NET supported types
        /* ADO.NET UNDERSTOOD DATA TYPES:
			COUNTER	DbType.Int64
			AUTOINCREMENT	DbType.Int64
			IDENTITY	DbType.Int64
			LONG	DbType.Int64
			TINYINT	DbType.Byte
			INTEGER	DbType.Int64
			INT	DbType.Int32
			VARCHAR	DbType.String
			NVARCHAR	DbType.String
			CHAR	DbType.String
			NCHAR	DbType.String
			TEXT	DbType.String
			NTEXT	DbType.String
			STRING	DbType.String
			DOUBLE	DbType.Double
			FLOAT	DbType.Double
			REAL	DbType.Single
			BIT	DbType.Boolean
			YESNO	DbType.Boolean
			LOGICAL	DbType.Boolean
			BOOL	DbType.Boolean
			NUMERIC	DbType.Decimal
			DECIMAL	DbType.Decimal
			MONEY	DbType.Decimal
			CURRENCY	DbType.Decimal
			TIME	DbType.DateTime
			DATE	DbType.DateTime
			TIMESTAMP	DbType.DateTime
			DATETIME	DbType.DateTime
			BLOB	DbType.Binary
			BINARY	DbType.Binary
			VARBINARY	DbType.Binary
			IMAGE	DbType.Binary
			GENERAL	DbType.Binary
			OLEOBJECT	DbType.Binary
			GUID	DbType.Guid
			UNIQUEIDENTIFIER	DbType.Guid
			MEMO	DbType.String
			NOTE	DbType.String
			LONGTEXT	DbType.String
			LONGCHAR	DbType.String
			SMALLINT	DbType.Int16
			BIGINT	DbType.Int64
			LONGVARCHAR	DbType.String
			SMALLDATE	DbType.DateTime
			SMALLDATETIME	DbType.DateTime
		 */
        #endregion

        private static ILog log = LogManager.GetLogger(typeof(OrmLiteDialectProviderBase));

        public string StringLengthNonUnicodeColumnDefinitionFormat = "VARCHAR({0})";
        public string StringLengthUnicodeColumnDefinitionFormat = "NVARCHAR({0})";

        //Set by Constructor and UpdateStringColumnDefinitions()
        public string StringColumnDefinition;
        public string StringLengthColumnDefinitionFormat;

        public string AutoIncrementDefinition = "AUTOINCREMENT"; //SqlServer express limit
        public string IntColumnDefinition = "INTEGER";
        public string LongColumnDefinition = "BIGINT";
        public string GuidColumnDefinition = "GUID";
        public string BoolColumnDefinition = "BOOL";
        public string RealColumnDefinition = "DOUBLE";
        public string DecimalColumnDefinition = "DECIMAL";
        public string BlobColumnDefinition = "BLOB";
        public string DateTimeColumnDefinition = "DATETIME";
        public string TimeColumnDefinition = "DATETIME";

        protected OrmLiteDialectProviderBase()
        {
            InitColumnTypeMap();
            UpdateStringColumnDefinitions();
        }
				
		
		private int defaultDecimalPrecision=18;
		private int defaultDecimalScale=12;
				
		public int DefaultDecimalPrecision{
			get { return defaultDecimalPrecision;}
			set { defaultDecimalPrecision=value;}
		}
				
		public int DefaultDecimalScale{
			get { return defaultDecimalScale;}
			set { defaultDecimalScale=value;}
		}
		
		
        private int defaultStringLength = 8000; //SqlServer express limit
        public int DefaultStringLength
        {
            get
            {
                return defaultStringLength;
            }
            set
            {
                defaultStringLength = value;
                UpdateStringColumnDefinitions();
            }
        }

        private bool useUnicode;
        public virtual bool UseUnicode
        {
            get
            {
                return useUnicode;
            }
            set
            {
                useUnicode = value;
                UpdateStringColumnDefinitions();
            }
        }

        private void UpdateStringColumnDefinitions()
        {
            this.StringLengthColumnDefinitionFormat = useUnicode
                ? StringLengthUnicodeColumnDefinitionFormat
                : StringLengthNonUnicodeColumnDefinitionFormat;

            this.StringColumnDefinition = string.Format(
                this.StringLengthColumnDefinitionFormat, DefaultStringLength);
        }

        protected void InitColumnTypeMap()
        {
            DbTypes<string>.Set(DbType.String, StringColumnDefinition);
			DbTypes<char>.Set(DbType.StringFixedLength, StringColumnDefinition);
			DbTypes<char?>.Set(DbType.StringFixedLength, StringColumnDefinition);
            DbTypes<char[]>.Set(DbType.String, StringColumnDefinition);
			DbTypes<bool>.Set(DbType.Boolean, BoolColumnDefinition);
			DbTypes<bool?>.Set(DbType.Boolean, BoolColumnDefinition);
			DbTypes<Guid>.Set(DbType.Guid, GuidColumnDefinition);
			DbTypes<Guid?>.Set(DbType.Guid, GuidColumnDefinition);
			DbTypes<DateTime>.Set(DbType.DateTime, DateTimeColumnDefinition);
			DbTypes<DateTime?>.Set(DbType.DateTime, DateTimeColumnDefinition);
			DbTypes<TimeSpan>.Set(DbType.Time, TimeColumnDefinition);
			DbTypes<TimeSpan?>.Set(DbType.Time, TimeColumnDefinition);
			DbTypes<DateTimeOffset>.Set(DbType.Time, TimeColumnDefinition);
			DbTypes<DateTimeOffset?>.Set(DbType.Time, TimeColumnDefinition);

			DbTypes<byte>.Set(DbType.Byte, IntColumnDefinition);
			DbTypes<byte?>.Set(DbType.Byte, IntColumnDefinition);
			DbTypes<sbyte>.Set(DbType.SByte, IntColumnDefinition);
			DbTypes<sbyte?>.Set(DbType.SByte, IntColumnDefinition);
			DbTypes<short>.Set(DbType.Int16, IntColumnDefinition);
			DbTypes<short?>.Set(DbType.Int16, IntColumnDefinition);
			DbTypes<ushort>.Set(DbType.UInt16, IntColumnDefinition);
			DbTypes<ushort?>.Set(DbType.UInt16, IntColumnDefinition);
			DbTypes<int>.Set(DbType.Int32, IntColumnDefinition);
			DbTypes<int?>.Set(DbType.Int32, IntColumnDefinition);
			DbTypes<uint>.Set(DbType.UInt32, IntColumnDefinition);
			DbTypes<uint?>.Set(DbType.UInt32, IntColumnDefinition);

			DbTypes<long>.Set(DbType.Int64, LongColumnDefinition);
			DbTypes<long?>.Set(DbType.Int64, LongColumnDefinition);
			DbTypes<ulong>.Set(DbType.UInt64, LongColumnDefinition);
			DbTypes<ulong?>.Set(DbType.UInt64, LongColumnDefinition);

			DbTypes<float>.Set(DbType.Single, RealColumnDefinition);
			DbTypes<float?>.Set(DbType.Single, RealColumnDefinition);
			DbTypes<double>.Set(DbType.Double, RealColumnDefinition);
			DbTypes<double?>.Set(DbType.Double, RealColumnDefinition);

			DbTypes<decimal>.Set(DbType.Decimal, DecimalColumnDefinition);
			DbTypes<decimal?>.Set(DbType.Decimal, DecimalColumnDefinition);

            DbTypes<byte[]>.Set(DbType.Binary, BlobColumnDefinition);
        }

        public string DefaultValueFormat = " DEFAULT ({0})";

        public virtual bool ShouldQuoteValue(Type fieldType)
        {
            string fieldDefinition;
            if (!DbTypes.ColumnTypeMap.TryGetValue(fieldType, out fieldDefinition))
            {
                fieldDefinition = this.GetUndefinedColumnDefintion(fieldType);
            }

            return fieldDefinition != IntColumnDefinition
                   && fieldDefinition != LongColumnDefinition
                   && fieldDefinition != RealColumnDefinition
                   && fieldDefinition != DecimalColumnDefinition
                   && fieldDefinition != BoolColumnDefinition;
        }

        public virtual object ConvertDbValue(object value, Type type)
        {
            if (value == null || value.GetType() == typeof(DBNull)) return null;

            if (value.GetType() == type)
            {
                return value;
            }

            if (type.IsValueType)
            {
                if (type == typeof(float))
                    return value is double ? (float)((double)value) : (float)value;

                if (type == typeof(double))
                    return value is float ? (double)((float)value) : (double)value;

                if (type == typeof(decimal))
                    return (decimal)value;
            }

            if (type == typeof(string))
                return value;

            try
            {
                var convertedValue = TypeSerializer.DeserializeFromString(value.ToString(), type);
                return convertedValue;
            }
            catch (Exception )
            {
                log.ErrorFormat("Error ConvertDbValue trying to convert {0} into {1}",
                    value, type.Name);
                throw;
            }
        }

        public virtual string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

            if (!fieldType.UnderlyingSystemType.IsValueType && fieldType != typeof(string))
            {
                if (TypeSerializer.CanCreateFromString(fieldType))
                {
                    return "'" + EscapeParam(TypeSerializer.SerializeToString(value)) + "'";
                }

                throw new NotSupportedException(
                    string.Format("Property of type: {0} is not supported", fieldType.FullName));
            }

            if (fieldType == typeof(float))
                return ((float)value).ToString(CultureInfo.InvariantCulture);

            if (fieldType == typeof(double))
                return ((double)value).ToString(CultureInfo.InvariantCulture);

            if (fieldType == typeof(decimal))
                return ((decimal)value).ToString(CultureInfo.InvariantCulture);

            return ShouldQuoteValue(fieldType)
                    ? "'" + EscapeParam(value) + "'"
                    : value.ToString();
        }

        public abstract IDbConnection CreateConnection(string filePath, Dictionary<string, string> options);

        public virtual string EscapeParam(object paramValue)
        {
            return paramValue.ToString().Replace("'", "''");
        }

        public virtual string GetTableNameDelimited(ModelDefinition modelDef)
        {
            return string.Format("\"{0}\"", modelDef.ModelName);
        }

        protected virtual string GetUndefinedColumnDefintion(Type fieldType)
        {
            if (TypeSerializer.CanCreateFromString(fieldType))
            {
                return this.StringColumnDefinition;
            }

            throw new NotSupportedException(
                string.Format("Property of type: {0} is not supported", fieldType.FullName));
        }

        public virtual string GetColumnDefinition(string fieldName, Type fieldType, 
			bool isPrimaryKey, bool autoIncrement, bool isNullable, 
			int? fieldLength, int? scale, string defaultValue)
        {
            string fieldDefinition;

            if (fieldType == typeof(string))
            {
                fieldDefinition = string.Format(StringLengthColumnDefinitionFormat, fieldLength.GetValueOrDefault(DefaultStringLength));
            }
            else
            {
                if (!DbTypes.ColumnTypeMap.TryGetValue(fieldType, out fieldDefinition))
                {
                    fieldDefinition = this.GetUndefinedColumnDefintion(fieldType);
                }
            }

            var sql = new StringBuilder();
            sql.AppendFormat("\"{0}\" {1}", fieldName, fieldDefinition);

            if (isPrimaryKey)
            {
                sql.Append(" PRIMARY KEY");
                if (autoIncrement)
                {
                    sql.Append(" ").Append(AutoIncrementDefinition);
                }
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

        public abstract long GetLastInsertId(IDbCommand command);
		
			
		public virtual string ToSelectStatement( Type tableType,  string sqlFilter, params object[] filterParams){
			
			var sql = new StringBuilder();
			const string SelectStatement = "SELECT ";
			var modelDef= tableType.GetModelDefinition();
			var isFullSelectStatement =
                !string.IsNullOrEmpty(sqlFilter)
				&& sqlFilter.Length > SelectStatement.Length
				&& sqlFilter.Substring(0, SelectStatement.Length).ToUpper().Equals(SelectStatement);

			if (isFullSelectStatement) return sqlFilter.SqlFormat(filterParams);

		    sql.AppendFormat("SELECT {0} FROM {1}", tableType.GetColumnNames(),
		                     OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef));
			if (!string.IsNullOrEmpty(sqlFilter))
			{
				sqlFilter = sqlFilter.SqlFormat(filterParams);
				if (!sqlFilter.StartsWith("ORDER ", StringComparison.InvariantCultureIgnoreCase)
					&& !sqlFilter.StartsWith("LIMIT ", StringComparison.InvariantCultureIgnoreCase))
				{
					sql.Append(" WHERE ");
				}
				sql.Append(sqlFilter);
			}

			return sql.ToString();
			
		}
		
		public virtual string ToInsertRowStatement( object objWithProperties,  IDbCommand command)
        {
            var sbColumnNames = new StringBuilder();
            var sbColumnValues = new StringBuilder();
			var modelDef= objWithProperties.GetType().GetModelDefinition();
                    
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.AutoIncrement) continue;

                if (sbColumnNames.Length > 0) sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(string.Format("\"{0}\"", fieldDef.FieldName));
                    sbColumnValues.Append(fieldDef.GetQuotedValue(objWithProperties));
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToInsertRowStatement(): " + ex.Message, ex);
                    throw;
                }
            }

            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2});",
                                    OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef), sbColumnNames, sbColumnValues);

            return sql;
        }
		
		public virtual string ToUpdateRowStatement(object objWithProperties){
			var sqlFilter = new StringBuilder();
            var sql = new StringBuilder();
			var modelDef= objWithProperties.GetType().GetModelDefinition();
            
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                try
                {
                    if (fieldDef.IsPrimaryKey)
                    {
                        if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");

                        sqlFilter.AppendFormat("\"{0}\" = {1}", fieldDef.FieldName, fieldDef.GetQuotedValue(objWithProperties));

                        continue;
                    }

                    if (sql.Length > 0) sql.Append(",");
                    sql.AppendFormat("\"{0}\" = {1}", fieldDef.FieldName, fieldDef.GetQuotedValue(objWithProperties));
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToUpdateRowStatement(): " + ex.Message, ex);
                }
            }

            var updateSql = string.Format("UPDATE {0} SET {1} WHERE {2}",
                OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef), sql, sqlFilter);

            return updateSql;
		}
		
		
		public virtual string ToDeleteRowStatement(object objWithProperties)
        {
            var sqlFilter = new StringBuilder();
			var modelDef= objWithProperties.GetType().GetModelDefinition();
            
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                try
                {
                    if (fieldDef.IsPrimaryKey)
                    {
                        if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");

                        sqlFilter.AppendFormat("\"{0}\" = {1}", fieldDef.FieldName, fieldDef.GetQuotedValue(objWithProperties));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToDeleteRowStatement(): " + ex.Message, ex);
                }
            }

            var deleteSql = string.Format("DELETE FROM {0} WHERE {1}",
                OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef), sqlFilter);

            return deleteSql;
        }
		
		public virtual string ToCreateTableStatement( Type tableType)
        {
            var sbColumns = new StringBuilder();
            var sbConstraints = new StringBuilder();

            var modelDef = tableType.GetModelDefinition();
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (sbColumns.Length != 0) sbColumns.Append(", \n  ");

                var columnDefinition = OrmLiteConfig.DialectProvider.GetColumnDefinition(
                    fieldDef.FieldName,
                    fieldDef.FieldType,
                    fieldDef.IsPrimaryKey,
                    fieldDef.AutoIncrement,
                    fieldDef.IsNullable,
                    fieldDef.FieldLength,
					null,
                    fieldDef.DefaultValue);

                sbColumns.Append(columnDefinition);

                if (fieldDef.ReferencesType == null) continue;

                var refModelDef = fieldDef.ReferencesType.GetModelDefinition();
                sbConstraints.AppendFormat(", \n\n  CONSTRAINT \"FK_{0}_{1}\" FOREIGN KEY (\"{2}\") REFERENCES \"{3}\" (\"{4}\")",
                    modelDef.ModelName, refModelDef.ModelName, fieldDef.FieldName, refModelDef.ModelName, modelDef.PrimaryKey.FieldName);
            }
            var sql = new StringBuilder(string.Format(
                "CREATE TABLE {0} \n(\n  {1}{2} \n); \n", OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef), sbColumns, sbConstraints));

            return sql.ToString();
        }
		
		public virtual List<string> ToCreateIndexStatements(Type tableType)
        {
            var sqlIndexes = new List<string>();

            var modelDef = tableType.GetModelDefinition();
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (!fieldDef.IsIndexed) continue;

                var indexName = GetIndexName(fieldDef.IsUnique, modelDef.ModelName.SafeVarName(), fieldDef.FieldName);

                sqlIndexes.Add(
                    ToCreateIndexStatement(fieldDef.IsUnique, indexName, modelDef, fieldDef.FieldName));
            }

            foreach (var compositeIndex in modelDef.CompositeIndexes)
            {
                var indexName = GetIndexName(compositeIndex.Unique, modelDef.ModelName.SafeVarName(),
                    string.Join("_", compositeIndex.FieldNames.ToArray()));

                var indexNames = string.Join("\" ASC, \"", compositeIndex.FieldNames.ToArray());

                sqlIndexes.Add(
                    ToCreateIndexStatement(compositeIndex.Unique, indexName, modelDef, indexNames));
            }

            return sqlIndexes;
        }

        protected virtual string GetIndexName(bool isUnique, string modelName, string fieldName)
        {
            return string.Format("{0}idx_{1}_{2}", isUnique ? "u" : "", modelName, fieldName).ToLower();
        }

        protected virtual string ToCreateIndexStatement(bool isUnique, string indexName, ModelDefinition modelDef, string fieldName)
        {
            return string.Format("CREATE {0} INDEX {1} ON {2} (\"{3}\" ASC); \n",
                    isUnique ? "UNIQUE" : "", indexName, OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef), fieldName);
        }
		
		public virtual string GetColumnNames(ModelDefinition modelDef){
			return modelDef.GetColumnNames();
		}
		
		
		public virtual List<string> ToCreateSequenceStatements(Type tableType){
			return new List<string>();
		}
		
		// TODO : make abstract  ??
		public virtual string ToExistStatement( Type fromTableType,
			object objWithProperties,
			string sqlFilter,
			params object[] filterParams){
			throw new NotImplementedException();
		}
		
		// TODO : make abstract  ??
		public virtual string ToSelectFromProcedureStatement(object fromObjWithProperties,
		                                          Type outputModelType,       
		                                          string sqlFilter, 
		                                          params object[] filterParams){
			throw new NotImplementedException();
		}
		
		// TODO : make abstract  ??
		public virtual string ToExecuteProcedureStatement(object objWithProperties){
			throw new NotImplementedException();
		}
		
		protected static ModelDefinition GetModel( Type modelType){
			return modelType.GetModelDefinition();
		}	
		
		
		public static string IdField{
			get {return  OrmLiteConfigExtensions.IdField ;}
		}
	
		public virtual string GetFieldNameDelimited(string fieldName){
			return string.Format("\"{0}\"", fieldName);
		}
		
		
    }
}