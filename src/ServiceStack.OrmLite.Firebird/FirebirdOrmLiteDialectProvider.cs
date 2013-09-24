using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using ServiceStack.Common.Utils;
using System.Text;
using FirebirdSql.Data.FirebirdClient;
using ServiceStack.Common;

namespace ServiceStack.OrmLite.Firebird
{
    public class FirebirdOrmLiteDialectProvider : OrmLiteDialectProviderBase<FirebirdOrmLiteDialectProvider>
	{
		private readonly List<string> RESERVED = new List<string>(new[] {
			"USER","ORDER","PASSWORD", "ACTIVE","LEFT","DOUBLE", "FLOAT", "DECIMAL","STRING", "DATE","DATETIME", "TYPE","TIMESTAMP",
			"INDEX","UNIQUE", "PRIMARY", "KEY", "ALTER", "DROP", "CREATE", "DELETE", "VALUES"
		});
		
		public static FirebirdOrmLiteDialectProvider Instance = new FirebirdOrmLiteDialectProvider();
		
		internal long LastInsertId { get; set; }
		protected bool CompactGuid;

		internal const string DefaultGuidDefinition = "VARCHAR(37)";
		internal const string CompactGuidDefinition = "CHAR(16) CHARACTER SET OCTETS";

		public FirebirdOrmLiteDialectProvider()
			: this(false)
		{
		}

		public FirebirdOrmLiteDialectProvider(bool compactGuid)
		{
			CompactGuid = compactGuid;
			base.BoolColumnDefinition = base.IntColumnDefinition;
			base.GuidColumnDefinition = CompactGuid ? CompactGuidDefinition : DefaultGuidDefinition;
			base.AutoIncrementDefinition= string.Empty;
			base.DateTimeColumnDefinition="TIMESTAMP";
			base.TimeColumnDefinition = "TIME";
			base.RealColumnDefinition= "FLOAT";
			base.DefaultStringLength=128;
			base.InitColumnTypeMap();
			DefaultValueFormat = " DEFAULT '{0}'";
		}
		
		public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
		{
			if (options != null)
			{
				foreach (var option in options)
				{
					connectionString += option.Key + "=" + option.Value + ";";
				}
			}
			
			return new FbConnection(connectionString);
		}
		
		public override long GetLastInsertId(IDbCommand dbCmd)
		{
			return LastInsertId;			
		}
		
		public override object ConvertDbValue(object value, Type type)
		{
			if (value == null || value is DBNull) return null;

			if (type == typeof(bool))
			{
				var intVal = int.Parse(value.ToString());
				return intVal != 0;
			}
			
			if(type == typeof(System.Double))
				return double.Parse(value.ToString());

			if (type == typeof(Guid) && BitConverter.IsLittleEndian) // TODO: check big endian
			{
				if (CompactGuid)
				{
					byte[] raw = ((Guid)value).ToByteArray();
					return new Guid(System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt32(raw, 0)),
						System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(raw, 4)),
						System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt16(raw, 6)),
						raw[8], raw[9], raw[10], raw[11], raw[12], raw[13], raw[14], raw[15]);
				}
				return new Guid(value.ToString());
			}
			
			if (type == typeof(byte[]) && value.GetType() == typeof(byte[]))
                		return value;

			try
			{
				return base.ConvertDbValue(value, type);
			}
			catch (Exception )
			{				
				throw;
			}
		}

		public override string GetQuotedValue(object value, Type fieldType)
		{
						
			if (value == null) return "NULL";

			if (fieldType == typeof(Guid))
			{
				if (CompactGuid)
					return "X'" + ((Guid)value).ToString("N") + "'";
				else
					return string.Format("CAST('{0}' AS {1})", (Guid)value, DefaultGuidDefinition);  // TODO : check this !!!
			}
			if (fieldType == typeof(DateTime) || fieldType == typeof( DateTime?) )
			{
				var dateValue = (DateTime)value;
				string iso8601Format= dateValue.ToString("yyyy-MM-dd HH:mm:ss.fff").EndsWith("00:00:00.000")?
						"yyyy-MM-dd"
						:"yyyy-MM-dd HH:mm:ss.fff";		
				return base.GetQuotedValue(dateValue.ToString(iso8601Format), typeof(string));
			}
			if (fieldType == typeof(bool ?) || fieldType == typeof(bool))
			{
				var boolValue = (bool)value;
				return base.GetQuotedValue(boolValue ? "1" : "0", typeof(string));
			}
			
			if (fieldType == typeof(decimal ?) || fieldType == typeof(decimal) ||
				fieldType == typeof(double ?) || fieldType == typeof(double)  ||
				fieldType == typeof(float ?) || fieldType == typeof(float)  ){
				var s = base.GetQuotedValue( value, fieldType);
				if (s.Length>20) s= s.Substring(0,20);
				return "'" + s + "'"; // when quoted exception is more clear!
			}
			
			return base.GetQuotedValue(value, fieldType);
		}
		
		public override string ToSelectStatement(Type tableType,  string sqlFilter, params object[] filterParams)
		{
			var sql = new StringBuilder();
			const string SelectStatement = "SELECT ";
			var modelDef = GetModel(tableType);
			var isFullSelectStatement = 
				!string.IsNullOrEmpty(sqlFilter)
				&& sqlFilter.Length > SelectStatement.Length
				&& sqlFilter.Substring(0, SelectStatement.Length).ToUpper().Equals(SelectStatement);

			if (isFullSelectStatement) 	return sqlFilter.SqlFormat(filterParams);
			
			sql.AppendFormat("SELECT {0} \nFROM {1}", 
			                 GetColumnNames(modelDef), 
			                 GetQuotedTableName(modelDef));
			if (!string.IsNullOrEmpty(sqlFilter))
			{
				sqlFilter = sqlFilter.SqlFormat(filterParams);
				if (!sqlFilter.StartsWith("\nORDER ", StringComparison.InvariantCultureIgnoreCase)
					&& !sqlFilter.StartsWith("\nROWS ", StringComparison.InvariantCultureIgnoreCase)) // ROWS <m> [TO <n>])
				{
					sql.Append("\nWHERE ");
				}
				sql.Append(sqlFilter);
			}
			return sql.ToString();
		}
		
		public override string ToInsertRowStatement(IDbCommand dbCommand, object objWithProperties, ICollection<string> insertFields = null)
		{
            if (insertFields == null)
                insertFields = new List<string>();

            var sbColumnNames = new StringBuilder();
			var sbColumnValues = new StringBuilder();

			var tableType = objWithProperties.GetType();
			var modelDef= GetModel(tableType);
			
			foreach (var fieldDef in modelDef.FieldDefinitions)
			{
				
				if( fieldDef.IsComputed ) continue;
				if( insertFields.Count>0 && ! insertFields.Contains( fieldDef.Name )) continue;
				
				if( (fieldDef.AutoIncrement || ! string.IsNullOrEmpty(fieldDef.Sequence)
					|| fieldDef.Name == OrmLiteConfig.IdField) 
					&& dbCommand != null) 
				{
	
					if (fieldDef.AutoIncrement &&  string.IsNullOrEmpty(fieldDef.Sequence))
					{
						fieldDef.Sequence = Sequence(
							(modelDef.IsInSchema
								? modelDef.Schema+"_"+NamingStrategy.GetTableName(modelDef.ModelName)
								: NamingStrategy.GetTableName(modelDef.ModelName)), 
							fieldDef.FieldName, fieldDef.Sequence);
					}
				
					PropertyInfo pi = tableType.GetProperty(fieldDef.Name, 
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

					var result = GetNextValue(dbCommand, fieldDef.Sequence, pi.GetValue(objWithProperties,  new object[] { }) );
					if (pi.PropertyType == typeof(String))
						ReflectionUtils.SetProperty(objWithProperties, pi,  result.ToString());	
					else if(pi.PropertyType == typeof(Int16))
						ReflectionUtils.SetProperty(objWithProperties, pi, Convert.ToInt16(result));	
					else if(pi.PropertyType == typeof(Int32))
						ReflectionUtils.SetProperty(objWithProperties, pi, Convert.ToInt32(result));	
					else if(pi.PropertyType == typeof(Guid))
						ReflectionUtils.SetProperty(objWithProperties, pi, result);
					else
						ReflectionUtils.SetProperty(objWithProperties, pi, Convert.ToInt64(result));
				}
				
				if (sbColumnNames.Length > 0) sbColumnNames.Append(",");
				if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

				try
				{
					sbColumnNames.Append(string.Format("{0}",GetQuotedColumnName(fieldDef.FieldName)));
					if (!string.IsNullOrEmpty(fieldDef.Sequence) && dbCommand==null)
						sbColumnValues.Append(string.Format("@{0}",fieldDef.Name));
					else
						sbColumnValues.Append(fieldDef.GetQuotedValue(objWithProperties));
				}
				catch (Exception)
				{
					throw;
				}
			}

			var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2});",
									GetQuotedTableName(modelDef), sbColumnNames, sbColumnValues);
						
			return sql;
		}

		
		public override string ToUpdateRowStatement(object objWithProperties, ICollection<string> updateFields=null)
		{
			if (updateFields == null) 
				updateFields = new List<string>();
				
			var sqlFilter = new StringBuilder();
			var sql = new StringBuilder();
			var tableType = objWithProperties.GetType();
			var modelDef = GetModel(tableType);
									
			foreach (var fieldDef in modelDef.FieldDefinitions)
			{
				if( fieldDef.IsComputed) continue;
				
				try
				{
					if ((fieldDef.IsPrimaryKey || fieldDef.Name == OrmLiteConfig.IdField) 
						&& updateFields.Count == 0)
					{
						if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");

						sqlFilter.AppendFormat("{0} = {1}", 
							GetQuotedColumnName(fieldDef.FieldName),
							fieldDef.GetQuotedValue(objWithProperties));
							
						continue;
					}
					if (updateFields.Count>0 && !updateFields.Contains(fieldDef.Name)) continue;
					if (sql.Length > 0) sql.Append(",");
					sql.AppendFormat("{0} = {1}", 
						GetQuotedColumnName(fieldDef.FieldName), 
						fieldDef.GetQuotedValue(objWithProperties));
				}
				catch (Exception)
				{
					throw;
				}
			}

			var updateSql = string.Format("UPDATE {0} \nSET {1} {2}",
				GetQuotedTableName(modelDef), sql, (sqlFilter.Length>0? "\nWHERE "+ sqlFilter:""));

			return updateSql;
		}
				
		
		public override string ToDeleteRowStatement(object objWithProperties)
		{
			var tableType = objWithProperties.GetType();
			var modelDef = GetModel(tableType);
			
			var sqlFilter = new StringBuilder();
			
			foreach (var fieldDef in modelDef.FieldDefinitions)
			{
				try
				{
					if (fieldDef.IsPrimaryKey || fieldDef.Name == OrmLiteConfig.IdField)
					{
						if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");
						sqlFilter.AppendFormat("{0} = {1}", 
							GetQuotedColumnName(fieldDef.FieldName), 
							fieldDef.GetQuotedValue(objWithProperties));
					}
				}
				catch (Exception)
				{
					throw;
				}
			}

			var deleteSql = string.Format("DELETE FROM {0} WHERE {1}",
				GetQuotedTableName(modelDef), sqlFilter);

			return deleteSql;
		}
		
		public override string ToCreateTableStatement(Type tableType)
		{
			var sbColumns = new StringBuilder();
            var sbConstraints = new StringBuilder();
			var sbPk= new StringBuilder();			
						
            var modelDef = GetModel(tableType);
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
				if (fieldDef.IsPrimaryKey) 
				{
					sbPk.AppendFormat(sbPk.Length != 0 ? ",{0}" : "{0}", GetQuotedColumnName(fieldDef.FieldName));
				}
			
                if (sbColumns.Length != 0) sbColumns.Append(", \n  ");

                var columnDefinition = GetColumnDefinition(
                    fieldDef.FieldName,
                    fieldDef.FieldType,
                    fieldDef.IsPrimaryKey,
                    fieldDef.AutoIncrement,
                    fieldDef.IsNullable,
                    fieldDef.FieldLength,
					fieldDef.Scale,
                    fieldDef.DefaultValue);

                sbColumns.Append(columnDefinition);

                if (fieldDef.ForeignKey == null) continue;

                var refModelDef = GetModel(fieldDef.ForeignKey.ReferenceType);
								
                sbConstraints.AppendFormat(", \n\n  CONSTRAINT {0} FOREIGN KEY ({1}) REFERENCES {2} ({3})",
                    GetQuotedName(fieldDef.ForeignKey.GetForeignKeyName(modelDef, refModelDef, NamingStrategy, fieldDef)),
					GetQuotedColumnName(fieldDef.FieldName), 
					GetQuotedTableName(refModelDef), 
					GetQuotedColumnName(refModelDef.PrimaryKey.FieldName));

				sbConstraints.Append(GetForeignKeyOnDeleteClause(fieldDef.ForeignKey));
				sbConstraints.Append(GetForeignKeyOnUpdateClause(fieldDef.ForeignKey));
            }
			
			if (sbPk.Length !=0) sbColumns.AppendFormat(", \n  PRIMARY KEY({0})", sbPk);
			
            var sql = new StringBuilder(string.Format(
                "CREATE TABLE {0} \n(\n  {1}{2} \n); \n",
				GetQuotedTableName(modelDef),
				sbColumns,
				sbConstraints));
			
			return sql.ToString();
			
		}
		
		public override List<string> ToCreateSequenceStatements(Type tableType){
			var gens = new  List<string>();			
			var modelDef = GetModel(tableType);
			
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
				if (fieldDef.AutoIncrement || ! fieldDef.Sequence.IsNullOrEmpty())
				{
				 	 gens.Add("CREATE GENERATOR " 
						 + Sequence( (modelDef.IsInSchema
							? modelDef.Schema+"_" + NamingStrategy.GetTableName(modelDef.ModelName)
							: NamingStrategy.GetTableName(modelDef.ModelName)), fieldDef.FieldName, fieldDef.Sequence) +";" );					
				}
			}
			return gens;
		}
		
		public override string GetColumnDefinition (string fieldName, Type fieldType, 
			bool isPrimaryKey, bool autoIncrement, bool isNullable, 
			int? fieldLength, int? scale, string defaultValue)
		{
			string fieldDefinition;

            if (fieldType == typeof(string))
            {
                fieldDefinition = string.Format(StringLengthColumnDefinitionFormat,
				                                fieldLength.GetValueOrDefault(DefaultStringLength));
            }
            else if( fieldType==typeof(Decimal) ){
				fieldDefinition= string.Format("{0} ({1},{2})", DecimalColumnDefinition, 
					fieldLength.GetValueOrDefault(DefaultDecimalPrecision),
					scale.GetValueOrDefault(DefaultDecimalScale) );
			}
            else 
			{
				fieldDefinition = GetColumnTypeDefinition(fieldType);
			}

            var sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", GetQuotedColumnName(fieldName), fieldDefinition);

			if (!string.IsNullOrEmpty(defaultValue))
			{
				sql.AppendFormat(DefaultValueFormat, defaultValue);
			}

            if (!isNullable)
            {
                sql.Append(" NOT NULL");
            }           

            return sql.ToString();
		}
		
		
		public override List<string> ToCreateIndexStatements(Type tableType)
        {
            var sqlIndexes = new List<string>();

            var modelDef = GetModel(tableType);
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (!fieldDef.IsIndexed) continue;

                var indexName = GetIndexName(
					fieldDef.IsUnique,
					(modelDef.IsInSchema 
						? modelDef.Schema + "_" + modelDef.ModelName
						: modelDef.ModelName).SafeVarName(), 
					fieldDef.FieldName);

                sqlIndexes.Add(
                    ToCreateIndexStatement(fieldDef.IsUnique, indexName, modelDef, fieldDef.FieldName,false));
            }

            foreach (var compositeIndex in modelDef.CompositeIndexes)
            {
                var indexName = GetCompositeIndexNameWithSchema(compositeIndex, modelDef);
                var indexNames = string.Join(",", compositeIndex.FieldNames.ToArray());

                sqlIndexes.Add(
                    ToCreateIndexStatement(compositeIndex.Unique, indexName, modelDef, indexNames,false));
            }

            return sqlIndexes;
        }

        protected override string ToCreateIndexStatement(bool isUnique, string indexName, ModelDefinition modelDef, string fieldName, bool isCombined)
        {
            return string.Format("CREATE {0} INDEX {1} ON {2} ({3} ); \n",
				isUnique ? "UNIQUE" : "", 
				indexName, 
				GetQuotedTableName(modelDef),
				GetQuotedColumnName(fieldName));
        }
		
		
		public override string ToExistStatement( Type fromTableType,
			object objWithProperties,
			string sqlFilter,
			params object[] filterParams)
		{
			
			var fromModelDef = GetModel(fromTableType);
			var sql = new StringBuilder();
			sql.AppendFormat("SELECT 1 \nFROM {0}", GetQuotedTableName(fromModelDef));
			
			var filter = new StringBuilder();
			
			if(objWithProperties!=null){
				var tableType = objWithProperties.GetType();
				
				if(fromTableType!=tableType){
					int i=0;
					var fpk= new List<FieldDefinition>();					
					var modelDef = GetModel(tableType);
					
					foreach (var def in modelDef.FieldDefinitions)
					{
						if (def.IsPrimaryKey) fpk.Add(def);
					}
					
					foreach (var fieldDef in fromModelDef.FieldDefinitions)
					{
						if (fieldDef.IsComputed) continue;
						try
						{
							if (fieldDef.ForeignKey !=null
								&& GetModel(fieldDef.ForeignKey.ReferenceType).ModelName == modelDef.ModelName)
							{
								if (filter.Length > 0) filter.Append(" AND ");
								filter.AppendFormat("{0} = {1}", GetQuotedColumnName(fieldDef.FieldName),
									fpk[i].GetQuotedValue(objWithProperties));	
								i++;
							}
						}	
						catch (Exception)
						{
							throw;
						}
					}	
					
				}
				else
				{
					var modelDef = GetModel(tableType);
					foreach (var fieldDef in modelDef.FieldDefinitions)
					{
						if (fieldDef.IsComputed) continue;
						try
						{
							if (fieldDef.IsPrimaryKey)
							{
								if (filter.Length > 0) filter.Append(" AND ");
								filter.AppendFormat("{0} = {1}",
									GetQuotedColumnName(fieldDef.FieldName),
									fieldDef.GetQuotedValue(objWithProperties));
							}
						}
						catch (Exception)
						{
							throw;
						}
					}
				}
												
				if (filter.Length>0) sql.AppendFormat("\nWHERE {0} ", filter);
			}	
			
			if (!string.IsNullOrEmpty(sqlFilter))
			{
				sqlFilter = sqlFilter.SqlFormat(filterParams);
				if (!sqlFilter.StartsWith("\nORDER ", StringComparison.InvariantCultureIgnoreCase)
					&& !sqlFilter.StartsWith("\nROWS ", StringComparison.InvariantCultureIgnoreCase)) // ROWS <m> [TO <n>])
				{
					sql.Append( filter.Length>0? " AND  ": "\nWHERE ");
				}
				sql.Append(sqlFilter);
			}
			 		
			var sb = new StringBuilder("select 1  from RDB$DATABASE where");
			sb.AppendFormat(" exists ({0})", sql.ToString() );
			return sb.ToString();
		}
		
		public override string ToSelectFromProcedureStatement(
			object fromObjWithProperties,
			Type outputModelType,       
			string sqlFilter, 
			params object[] filterParams)		
		{
						
			var sbColumnValues = new StringBuilder();
			
			Type fromTableType = fromObjWithProperties.GetType();
			
			var modelDef = GetModel(fromTableType);
			
			foreach (var fieldDef in modelDef.FieldDefinitions)
			{	
				if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

				try
				{
					sbColumnValues.Append( fieldDef.GetQuotedValue(fromObjWithProperties) );	
				}
				catch (Exception)
				{	
					throw;
				}
			}
			
			var sql = new StringBuilder();
			sql.AppendFormat("SELECT {0} \nFROM  {1} {2}{3}{4}  \n",
				GetColumnNames(GetModel(outputModelType)),
				GetQuotedTableName(modelDef),
				sbColumnValues.Length > 0 ? "(" : "",
				sbColumnValues,
				sbColumnValues.Length > 0 ? ")" : "");
			
			if (!string.IsNullOrEmpty(sqlFilter))
			{
				sqlFilter = sqlFilter.SqlFormat(filterParams);
				if (!sqlFilter.StartsWith("\nORDER ", StringComparison.InvariantCultureIgnoreCase)
					&& !sqlFilter.StartsWith("\nROWS ", StringComparison.InvariantCultureIgnoreCase)) // ROWS <m> [TO <n>]
				{
					sql.Append("\nWHERE ");
				}
				sql.Append(sqlFilter);
			}
			
			return sql.ToString();		
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
			
			var sql = string.Format("EXECUTE PROCEDURE {0} {1}{2}{3};",
				GetQuotedTableName(modelDef),  
				sbColumnValues.Length > 0 ? "(" : "",
				sbColumnValues,
				sbColumnValues.Length > 0 ? ")" : "");
			
			return sql;
		}
		
		private object GetNextValue(IDbCommand dbCmd, string sequence, object value) 
		{
			Object retObj;
			
			if (value.ToString() != "0")
			{
				long nv;
				if (long.TryParse(value.ToString(), out nv))
				{
					LastInsertId=nv;
					retObj= LastInsertId;
				}
				else
				{
					LastInsertId=0;
					retObj =value;
				}
				return retObj;
				
			}
			
			dbCmd.CommandText = string.Format("select next value for {0} from RDB$DATABASE",Quote(sequence));
			long result = (long) dbCmd.ExecuteScalar();
			LastInsertId = result;
			return  result;				
		}
		
		public bool QuoteNames { get; set; }
		
		private string Quote(string name)
		{
			return QuoteNames
				? string.Format("\"{0}\"",name)
				: RESERVED.Contains(name.ToUpper())
					? string.Format("\"{0}\"", name)
					: name;			
		}
		
		public override string GetColumnNames(ModelDefinition modelDef)
		{
			if (QuoteNames) return modelDef.GetColumnNames();
			var sqlColumns = new StringBuilder();
			modelDef.FieldDefinitions.ForEach(x => 
				sqlColumns.AppendFormat("{0} {1}", 
				sqlColumns.Length > 0 ? "," : "",
				GetQuotedColumnName(x.FieldName)));

			return sqlColumns.ToString();
		}

		public override string GetQuotedName(string fieldName)
		{
			return Quote(fieldName);
		}
		
		public override string GetQuotedTableName(ModelDefinition modelDef)
        {
            if (!modelDef.IsInSchema)
                return Quote(NamingStrategy.GetTableName(modelDef.ModelName));

            return Quote(string.Format("{0}_{1}", modelDef.Schema,
				NamingStrategy.GetTableName(modelDef.ModelName)));
        }
       	
        public override string GetQuotedTableName(string tableName)
        {
            return Quote(NamingStrategy.GetTableName(tableName));
        }


		public override string GetQuotedColumnName(string fieldName)
		{
			return Quote(NamingStrategy.GetColumnName(fieldName));
		}
		
		private string Sequence(string modelName,string fieldName, string sequence)
		{
			return sequence.IsNullOrEmpty() 
				? Quote(modelName + "_" + fieldName + "_GEN")
				: Quote(sequence);	
		}
		
		public override SqlExpressionVisitor<T> ExpressionVisitor<T> ()
		{
			return new FirebirdSqlExpressionVisitor<T>();
		}
		
		public override bool DoesTableExist(IDbCommand dbCmd, string tableName)
		{
			if (!QuoteNames & !RESERVED.Contains(tableName.ToUpper()))
			{
				tableName = tableName.ToUpper();
			}
			
			var sql = "SELECT count(*) FROM rdb$relations " +
				"WHERE rdb$system_flag = 0 AND rdb$view_blr IS NULL AND rdb$relation_name ={0}"
				.SqlFormat(tableName);

			dbCmd.CommandText = sql; 
			var result = dbCmd.GetLongScalar();

			return result > 0;
		}


		public override string GetForeignKeyOnDeleteClause(ForeignKeyConstraint foreignKey)
		{
			return (!string.IsNullOrEmpty(foreignKey.OnDelete) && foreignKey.OnDelete.ToUpper()!="RESTRICT" )? " ON DELETE " + foreignKey.OnDelete : "";
		}
		
		public override string GetForeignKeyOnUpdateClause(ForeignKeyConstraint foreignKey)
		{
			return (!string.IsNullOrEmpty(foreignKey.OnUpdate) && foreignKey.OnUpdate.ToUpper()!="RESTRICT" )? " ON UPDATE " + foreignKey.OnUpdate : "";
		}

		#region DDL
		public override string ToAddColumnStatement(Type modelType, FieldDefinition fieldDef){
			
			var column = GetColumnDefinition(fieldDef.FieldName,
			                                 fieldDef.FieldType,
			                                 fieldDef.IsPrimaryKey,
			                                 fieldDef.AutoIncrement,
			                                 fieldDef.IsNullable,
			                                 fieldDef.FieldLength,
			                                 fieldDef.Scale,
			                                 fieldDef.DefaultValue);
			return string.Format("ALTER TABLE {0} ADD {1} ;",
			                     GetQuotedTableName(GetModel(modelType).ModelName),
			                     column);
		}
		
		public override string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef)
		{
			
			var column = GetColumnDefinition(fieldDef.FieldName,
			                                 fieldDef.FieldType,
			                                 fieldDef.IsPrimaryKey,
			                                 fieldDef.AutoIncrement,
			                                 fieldDef.IsNullable,
			                                 fieldDef.FieldLength,
			                                 fieldDef.Scale,
			                                 fieldDef.DefaultValue);
			return string.Format("ALTER TABLE {0} ALTER {1} ;",
			                     GetQuotedTableName(GetModel(modelType).ModelName),
			                     column);
		}
		
		public override string ToChangeColumnNameStatement(Type modelType,
		                                                   FieldDefinition fieldDef,
		                                                   string oldColumnName)
		{	
			return string.Format("ALTER TABLE {0} ALTER {1} TO {2} ;",
			                     GetQuotedTableName(GetModel(modelType).ModelName),
			                     GetQuotedColumnName(oldColumnName),
			                     GetQuotedColumnName(fieldDef.FieldName));
		}
		#endregion DDL
	}
}

/*
DEBUG: Ignoring existing generator 'CREATE GENERATOR ModelWFDT_Id_GEN;': unsuccessful metadata update
DEFINE GENERATOR failed
attempt to store duplicate value (visible to active transactions) in unique index "RDB$INDEX_11" 
*/

