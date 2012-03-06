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

		private string paramString = "@";
		public string ParamString
		{
			get { return paramString; }
			set { paramString = value; }
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

		private INamingStrategy namingStrategy = new OrmLiteNamingStrategyBase();
		public INamingStrategy NamingStrategy
		{
			get
			{
				return namingStrategy;
			}
			set
			{
				namingStrategy = value;
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
                fieldDefinition = this.GetUndefinedColumnDefinition(fieldType, null);
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

        public virtual string GetQuotedTableName(ModelDefinition modelDef)
        {
            return string.Format("\"{0}\"", namingStrategy.GetTableName(modelDef.ModelName));
        }

		public virtual string GetQuotedColumnName(string columnName)
		{
			return string.Format("\"{0}\"", namingStrategy.GetColumnName(columnName));
		}

        public virtual string GetQuotedName(string name)
        {
			return string.Format("\"{0}\"", name);
        }

        protected virtual string GetUndefinedColumnDefinition(Type fieldType, int? fieldLength)
        {
            if (TypeSerializer.CanCreateFromString(fieldType))
            {
                return string.Format(StringLengthColumnDefinitionFormat, fieldLength.GetValueOrDefault(DefaultStringLength));
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
                    fieldDefinition = this.GetUndefinedColumnDefinition(fieldType, fieldLength);
                }
            }

            var sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldName), fieldDefinition);

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
				&& sqlFilter.TrimStart().StartsWith(SelectStatement, StringComparison.OrdinalIgnoreCase);

			if (isFullSelectStatement) return sqlFilter.SqlFormat(filterParams);

		    sql.AppendFormat("SELECT {0} FROM {1}", tableType.GetColumnNames(),
		                     GetQuotedTableName(modelDef));
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
			return ToInsertRowStatement(objWithProperties, new List<string>(), command);
        }
		
		public virtual string ToInsertRowStatement( object objWithProperties, IList<string>insertFields, IDbCommand command){
			
			if( insertFields==null ) insertFields = new List<string>(); 
			var sbColumnNames = new StringBuilder();
            var sbColumnValues = new StringBuilder();
			var modelDef= objWithProperties.GetType().GetModelDefinition();
                    
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.AutoIncrement) continue;
				//insertFields contains Property "Name" of fields to insert ( that's how expressions work )
				if( insertFields.Count>0 && !insertFields.Contains( fieldDef.Name )) continue;
				
                if (sbColumnNames.Length > 0) sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(GetQuotedColumnName(fieldDef.FieldName));
                    sbColumnValues.Append(fieldDef.GetQuotedValue(objWithProperties));
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToInsertRowStatement(): " + ex.Message, ex);
                    throw;
                }
            }

            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2});",
                                    GetQuotedTableName(modelDef), sbColumnNames, sbColumnValues);

            return sql;
		}

        public virtual IDbCommand CreateParameterizedInsertStatement(object objWithProperties, IDbConnection connection)
        {
            return CreateParameterizedInsertStatement(objWithProperties, null, connection);
        }

        public virtual IDbCommand CreateParameterizedInsertStatement(object objWithProperties, IList<string> insertFields, IDbConnection connection)
        {
            if (insertFields == null) insertFields = new List<string>();
            var sbColumnNames = new StringBuilder();
            var sbColumnValues = new StringBuilder();
            var modelDef = objWithProperties.GetType().GetModelDefinition();

            var command = connection.CreateCommand();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.AutoIncrement) continue;
                //insertFields contains Property "Name" of fields to insert ( that's how expressions work )
                if (insertFields.Count > 0 && !insertFields.Contains(fieldDef.Name)) continue;

                if (sbColumnNames.Length > 0) sbColumnNames.Append(",");
                if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

                try
                {
                    sbColumnNames.Append(GetQuotedColumnName(fieldDef.FieldName));
                    sbColumnValues.Append(ParamString)
                                  .Append(fieldDef.FieldName);

                    AddParameterForFieldToCommand(command, fieldDef, objWithProperties);                    
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in CreateParameterizedInsertStatement(): " + ex.Message, ex);
                    throw;
                }
            }

            command.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2});",
                                                GetQuotedTableName(modelDef), sbColumnNames, sbColumnValues);
            return command;
        }

        public void ReParameterizeInsertStatement(object objWithProperties, IDbCommand command)
        {
            ReParameterizeInsertStatement(objWithProperties, null, command);
        }

        public void ReParameterizeInsertStatement(object objWithProperties, IList<string> insertFields, IDbCommand command)
        {
            if (insertFields == null) insertFields = new List<string>();
            var modelDef = objWithProperties.GetType().GetModelDefinition();

            command.Parameters.Clear();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.AutoIncrement) continue;
                //insertFields contains Property "Name" of fields to insert ( that's how expressions work )
                if (insertFields.Count > 0 && !insertFields.Contains(fieldDef.Name)) continue;

                try
                {
                    AddParameterForFieldToCommand(command, fieldDef, objWithProperties);                 
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ReParameterizeInsertStatement(): " + ex.Message, ex);
                    throw;
                }
            }
        }

        private void AddParameterForFieldToCommand(IDbCommand command, FieldDefinition fieldDef, object objWithProperties)
        {
            var p = command.CreateParameter();
            p.ParameterName = String.Format("{0}{1}", ParamString, fieldDef.FieldName);
            
            if (DbTypes.ColumnDbTypeMap.ContainsKey(fieldDef.FieldType))
            {
                p.DbType = DbTypes.ColumnDbTypeMap[fieldDef.FieldType];
                p.Value = GetValueOrDbNull(fieldDef, objWithProperties);
            } else
            {
                var unquotedVal = fieldDef.GetQuotedValue(objWithProperties)
                                          .TrimStart('\'')
                                          .TrimEnd('\'');
                p.DbType = DbType.String;
                p.Value = GetValueOrDbNull(unquotedVal);
            }
           

            command.Parameters.Add(p);
        }

        private object GetValueOrDbNull(FieldDefinition fieldDef, object objWithProperties)
        {
            return fieldDef.GetValue(objWithProperties) ?? DBNull.Value;
        }
	
        private object GetValueOrDbNull(String value)
        {
            if (String.IsNullOrEmpty(value))
                return DBNull.Value;

            return value;
        }

		public virtual string ToUpdateRowStatement(object objWithProperties){
			return ToUpdateRowStatement(objWithProperties, new List<string>());
		}
		
		public virtual string ToUpdateRowStatement(object objWithProperties, IList<string> updateFields){
			
			if (updateFields==null) updateFields= new List<string>();
			var sqlFilter = new StringBuilder();
            var sql = new StringBuilder();
			var modelDef= objWithProperties.GetType().GetModelDefinition();
            
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                try
                {
                    if (fieldDef.IsPrimaryKey && updateFields.Count==0)
                    {
                        if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");

                        sqlFilter.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName), fieldDef.GetQuotedValue(objWithProperties));

                        continue;
                    }
					
					if( updateFields.Count>0 && !updateFields.Contains( fieldDef.Name )) continue;
                    if (sql.Length > 0) sql.Append(",");
					sql.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName), fieldDef.GetQuotedValue(objWithProperties));
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToUpdateRowStatement(): " + ex.Message, ex);
                }
            }

            var updateSql = string.Format("UPDATE {0} SET {1} {2}",
                GetQuotedTableName(modelDef), sql, (sqlFilter.Length>0? "WHERE "+ sqlFilter:""));
				

            return updateSql;
		}

        public virtual IDbCommand CreateParameterizedUpdateStatement(object objWithProperties, IDbConnection connection)
        {
            return CreateParameterizedUpdateStatement(objWithProperties, null, connection);
        }

        public virtual IDbCommand CreateParameterizedUpdateStatement(object objWithProperties, IList<string> updateFields, IDbConnection connection)
        {
            if (updateFields == null) updateFields = new List<string>();
            var sqlFilter = new StringBuilder();
            var sql = new StringBuilder();
            var modelDef = objWithProperties.GetType().GetModelDefinition();

            var command = connection.CreateCommand();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                try
                {
                    if (fieldDef.IsPrimaryKey && updateFields.Count == 0)
                    {
                        if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");

						sqlFilter.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName), String.Concat(ParamString, fieldDef.Name));
                        AddParameterForFieldToCommand(command, fieldDef, objWithProperties);

                        continue;
                    }

                    if (updateFields.Count > 0 && !updateFields.Contains(fieldDef.Name)) continue;
                    if (sql.Length > 0) sql.Append(",");
                    sql.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName), String.Concat(ParamString, fieldDef.Name));

                    AddParameterForFieldToCommand(command, fieldDef, objWithProperties);
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in CreateParameterizedUpdateStatement(): " + ex.Message, ex);
                }
            }

            command.CommandText = string.Format("UPDATE {0} SET {1} {2}", GetQuotedTableName(modelDef), sql, (sqlFilter.Length > 0 ? "WHERE " + sqlFilter : ""));
            return command;
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

                        sqlFilter.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName), fieldDef.GetQuotedValue(objWithProperties));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("ERROR in ToDeleteRowStatement(): " + ex.Message, ex);
                }
            }

            var deleteSql = string.Format("DELETE FROM {0} WHERE {1}",
                GetQuotedTableName(modelDef), sqlFilter);

            return deleteSql;
        }
		
		public virtual string ToDeleteStatement(Type tableType, string sqlFilter, params object[] filterParams)
        {
            var sql = new StringBuilder();
            const string deleteStatement = "DELETE ";

            var isFullDeleteStatement =
                !string.IsNullOrEmpty(sqlFilter)
                && sqlFilter.Length > deleteStatement.Length
                && sqlFilter.Substring(0, deleteStatement.Length).ToUpper().Equals(deleteStatement);

            if (isFullDeleteStatement) return sqlFilter.SqlFormat(filterParams);

            var modelDef = tableType.GetModelDefinition();
            sql.AppendFormat("DELETE FROM {0}", GetQuotedTableName(modelDef));
            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFormat(filterParams);
                sql.Append(" WHERE ");
                sql.Append(sqlFilter);
            }

            return sql.ToString();
        }
		
		public virtual string ToCreateTableStatement(Type tableType)
        {
             var sbColumns = new StringBuilder();
            var sbConstraints = new StringBuilder();

            var modelDef = tableType.GetModelDefinition();
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (sbColumns.Length != 0) sbColumns.Append(", \n  ");

                var columnDefinition = GetColumnDefinition(
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

                var indexNames = string.Join(" ASC, ",
                                             compositeIndex.FieldNames.ConvertAll(
                                                 n => GetQuotedName(n)).ToArray());

                sqlIndexes.Add(
                    ToCreateIndexStatement(compositeIndex.Unique, indexName, modelDef, indexNames, true));
            }

            return sqlIndexes;
        }

    	public virtual bool DoesTableExist(IDbCommand dbCmd, string tableName)
		{
			return false;
		}

        protected virtual string GetIndexName(bool isUnique, string modelName, string fieldName)
        {
            return string.Format("{0}idx_{1}_{2}", isUnique ? "u" : "", modelName, fieldName).ToLower();
        }

        protected virtual string ToCreateIndexStatement(bool isUnique, string indexName, ModelDefinition modelDef, string fieldName, bool isCombined = false)
        {
            return string.Format("CREATE {0} INDEX {1} ON {2} ({3} ASC); \n",
                                 isUnique ? "UNIQUE" : "", indexName,
                                 GetQuotedTableName(modelDef),
                                 (isCombined) ? fieldName : GetQuotedColumnName(fieldName));
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
	
		
		public virtual SqlExpressionVisitor<T> ExpressionVisitor<T>(){
			throw new NotImplementedException();
		}
		
    }
}