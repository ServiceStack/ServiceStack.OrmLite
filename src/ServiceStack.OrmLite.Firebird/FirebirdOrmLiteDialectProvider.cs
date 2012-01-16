using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using ServiceStack.Common.Utils;
using System.Text;
using FirebirdSql.Data.FirebirdClient;
using ServiceStack.OrmLite;
using ServiceStack.Common.Extensions;

namespace ServiceStack.OrmLite.Firebird
{
	public class FirebirdOrmLiteDialectProvider:OrmLiteDialectProviderBase
	{
		private readonly List<string> RESERVED = new List<string>( new string[] 
		{"USER","ORDER","PASSWORD", "ACTIVE","LEFT","DOUBLE", "FLOAT", "DECIMAL"} );
		
		public static FirebirdOrmLiteDialectProvider Instance = new FirebirdOrmLiteDialectProvider();
		
		internal  long LastInsertId{
			get; set;
		}
		
		public FirebirdOrmLiteDialectProvider()
		{
			base.BoolColumnDefinition = base.IntColumnDefinition;
			base.GuidColumnDefinition = "VARCHAR(37)";
			base.AutoIncrementDefinition= string.Empty;
			base.DateTimeColumnDefinition="TIMESTAMP";
			base.TimeColumnDefinition = "TIME";
			base.RealColumnDefinition= "FLOAT";
			base.DefaultStringLength=128;
			base.InitColumnTypeMap();
			
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
				var guidValue = (Guid)value;
				return string.Format("CAST('{0}' AS {1})", guidValue, GuidColumnDefinition);  // TODO : check this !!!
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
			
			//if(fieldType== typeof(TimeSpan) ){
			//	return string.Format("'{0}'", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") );
			//}
			
			return base.GetQuotedValue(value, fieldType);
		}
		
		public override string ToSelectStatement(Type tableType,  string sqlFilter, params object[] filterParams)
		{
			var sql = new StringBuilder();
			const string SelectStatement = "SELECT ";
			var modelDef= OrmLiteDialectProviderBase.GetModel(tableType);
			var isFullSelectStatement = 
				!string.IsNullOrEmpty(sqlFilter)
				&& sqlFilter.Length > SelectStatement.Length
				&& sqlFilter.Substring(0, SelectStatement.Length).ToUpper().Equals(SelectStatement);

			if (isFullSelectStatement) 	return sqlFilter.SqlFormat(filterParams);
			
			sql.AppendFormat("SELECT {0} FROM {1}", 
			                 GetColumnNames(modelDef), 
			                 GetTableNameDelimited(modelDef));
			if (!string.IsNullOrEmpty(sqlFilter))
			{
				sqlFilter = sqlFilter.SqlFormat(filterParams);
				if (!sqlFilter.StartsWith("ORDER ", StringComparison.InvariantCultureIgnoreCase)
					&& !sqlFilter.StartsWith("ROWS ", StringComparison.InvariantCultureIgnoreCase)) // ROWS <m> [TO <n>])
				{
					sql.Append(" WHERE ");
				}
				sql.Append(sqlFilter);
			}
			return sql.ToString();
		}
		
		
		public override string ToInsertRowStatement(object objWithProperties,IDbCommand dbCommand)
		{
			var sbColumnNames = new StringBuilder();
			var sbColumnValues = new StringBuilder();

			var tableType = objWithProperties.GetType();
			var modelDef= OrmLiteDialectProviderBase.GetModel(tableType);
			
			foreach (var fieldDef in modelDef.FieldDefinitions)
			{
				
				if( fieldDef.IsComputed ) continue;
				
				if( (fieldDef.AutoIncrement || ! string.IsNullOrEmpty(fieldDef.Sequence) 
					|| fieldDef.Name== OrmLiteDialectProviderBase.IdField ) 
					&& dbCommand!=null ) {
	
					if( fieldDef.AutoIncrement &&  string.IsNullOrEmpty(fieldDef.Sequence) ){
						fieldDef.Sequence= Sequence( modelDef.IsInSchema? modelDef.Schema+"_"+modelDef.ModelName: modelDef.ModelName,
							fieldDef.FieldName, fieldDef.Sequence);
					}
				
					PropertyInfo pi = ReflectionUtils.GetPropertyInfo(tableType, fieldDef.Name);
					
					var result = GetNextValue(dbCommand, fieldDef.Sequence, pi.GetValue(objWithProperties,  new object[] { }) );
					if( pi.PropertyType== typeof(String))
						ReflectionUtils.SetProperty(objWithProperties, pi,  result.ToString());	
					else if(pi.PropertyType== typeof(Int16))
						ReflectionUtils.SetProperty(objWithProperties, pi, Convert.ToInt16(result));	
					else if(pi.PropertyType== typeof(Int32))
						ReflectionUtils.SetProperty(objWithProperties, pi, Convert.ToInt32(result));	
					else
						ReflectionUtils.SetProperty(objWithProperties, pi, Convert.ToInt64( result));			
				}
				

				if (sbColumnNames.Length > 0) sbColumnNames.Append(",");
				if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

				try
				{
					sbColumnNames.Append(string.Format("{0}", Quote(fieldDef.FieldName)));
					if( ! string.IsNullOrEmpty( fieldDef.Sequence )  &&  dbCommand==null )
						sbColumnValues.Append(string.Format("@{0}",fieldDef.Name));
					else
						sbColumnValues.Append(fieldDef.GetQuotedValue(objWithProperties));
				}
				catch (Exception )
				{
					throw;
				}
			}

			var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2});",
									GetTableNameDelimited(modelDef), sbColumnNames, sbColumnValues);
						
			return sql;
		}

		
		public override string ToUpdateRowStatement(object objWithProperties)
		{
			var sqlFilter = new StringBuilder();
			var sql = new StringBuilder();
			var tableType = objWithProperties.GetType();
			var modelDef= OrmLiteDialectProviderBase.GetModel(tableType);
									
			foreach (var fieldDef in modelDef.FieldDefinitions)
			{
				if( fieldDef.IsComputed) continue;
				
				try
				{
					if (fieldDef.IsPrimaryKey || fieldDef.Name== OrmLiteDialectProviderBase.IdField )
					{
						if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");

						sqlFilter.AppendFormat("{0} = {1}", Quote(fieldDef.FieldName), fieldDef.GetQuotedValue(objWithProperties));
							
						continue;
					}

					if (sql.Length > 0) sql.Append(",");
					sql.AppendFormat("{0} = {1}", Quote(fieldDef.FieldName), fieldDef.GetQuotedValue(objWithProperties));
				}
				catch (Exception )
				{
					throw;
				}
			}

			var updateSql = string.Format("UPDATE {0} SET {1} WHERE {2}",
				GetTableNameDelimited(modelDef), sql, sqlFilter);

			return updateSql;
		}
				
		
		public override string ToDeleteRowStatement( object objWithProperties)
		{
			var tableType = objWithProperties.GetType();
			var modelDef= OrmLiteDialectProviderBase.GetModel(tableType);
			
			var sqlFilter = new StringBuilder();
			
			foreach (var fieldDef in modelDef.FieldDefinitions)
			{
				try
				{
					if (fieldDef.IsPrimaryKey || fieldDef.Name== OrmLiteDialectProviderBase.IdField)
					{
						if (sqlFilter.Length > 0) sqlFilter.Append(" AND ");

						sqlFilter.AppendFormat("{0} = {1}", Quote(fieldDef.FieldName), fieldDef.GetQuotedValue(objWithProperties));
					}
				}
				catch (Exception )
				{
					throw;
				}
			}

			var deleteSql = string.Format("DELETE FROM {0} WHERE {1}",
				GetTableNameDelimited(modelDef), sqlFilter);

			return deleteSql;
		}
		
		
		public override string ToCreateTableStatement( Type tableType){
			var sbColumns = new StringBuilder();
            var sbConstraints = new StringBuilder();
			var sbPk= new StringBuilder();			
						
            var modelDef = OrmLiteDialectProviderBase.GetModel( tableType);
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
				if(fieldDef.IsPrimaryKey) {
					if( sbPk.Length !=0) sbPk.AppendFormat(",{0}", Quote(fieldDef.FieldName));
					else sbPk.AppendFormat("{0}", Quote(fieldDef.FieldName));
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

                if (fieldDef.ReferencesType == null) continue;

                var refModelDef = OrmLiteDialectProviderBase.GetModel( fieldDef.ReferencesType);
                sbConstraints.AppendFormat(", \n\n  CONSTRAINT {0} FOREIGN KEY ({1}) REFERENCES {2} ({3})",
                    Quote( string.Format("FK_{0}_{1}",modelDef.IsInSchema? modelDef.Schema+"_"+ modelDef.ModelName: modelDef.ModelName,
													  refModelDef.IsInSchema? refModelDef.Schema+"_"+ refModelDef.ModelName: refModelDef.ModelName ) ),
					Quote(fieldDef.FieldName), 
					GetTableNameDelimited(refModelDef), 
					Quote(refModelDef.PrimaryKey.FieldName));
            }
			
			if( sbPk.Length !=0) sbColumns.AppendFormat(", \n  PRIMARY KEY({0})", sbPk.ToString());
			
            var sql = new StringBuilder(string.Format(
                "CREATE TABLE {0} \n(\n  {1}{2} \n); \n",
				GetTableNameDelimited(modelDef),
				sbColumns,
				sbConstraints));
			
			return sql.ToString();
			
		}
		
		public override List<string> ToCreateSequenceStatements(Type tableType){
			List<string> gens = new  List<string>();
			
			var modelDef = OrmLiteDialectProviderBase.GetModel( tableType);
			
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
				if(fieldDef.AutoIncrement || ! fieldDef.Sequence.IsNullOrEmpty()){
			
				 	 gens.Add("CREATE GENERATOR " + Sequence( (modelDef.IsInSchema? modelDef.Schema+"_"+ modelDef.ModelName: modelDef.ModelName), fieldDef.FieldName, fieldDef.Sequence) +";" );
					
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
                fieldDefinition = string.Format(StringLengthColumnDefinitionFormat, fieldLength.GetValueOrDefault(DefaultStringLength));
            }
            else if( fieldType==typeof(Decimal) ){
				fieldDefinition= string.Format("{0} ({1},{2})", DecimalColumnDefinition, 
					fieldLength.GetValueOrDefault(DefaultDecimalPrecision),
					scale.GetValueOrDefault(DefaultDecimalScale) );
			}
            else {
                if (!DbTypes.ColumnTypeMap.TryGetValue(fieldType, out fieldDefinition))
                {
                    fieldDefinition = this.GetUndefinedColumnDefintion(fieldType);
                }
            }

            var sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", Quote(fieldName), fieldDefinition);

            
            if ( ! isNullable)
            {
                sql.Append(" NOT NULL");
            }
            

            if (!string.IsNullOrEmpty(defaultValue))
            {
                sql.AppendFormat(DefaultValueFormat, defaultValue);
            }

            return sql.ToString();
		}
		
		
		public override List<string> ToCreateIndexStatements(Type tableType)
        {
            var sqlIndexes = new List<string>();

            var modelDef = OrmLiteDialectProviderBase.GetModel(tableType);
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (!fieldDef.IsIndexed) continue;

                var indexName = GetIndexName(fieldDef.IsUnique,
					( modelDef.IsInSchema ? modelDef.Schema +"_"+ modelDef.ModelName: modelDef.ModelName ).SafeVarName(), 
					fieldDef.FieldName);

                sqlIndexes.Add(
                    ToCreateIndexStatement(fieldDef.IsUnique, indexName, modelDef, fieldDef.FieldName));
            }

            foreach (var compositeIndex in modelDef.CompositeIndexes)
            {
                var indexName = GetIndexName(compositeIndex.Unique, 
					( modelDef.IsInSchema ? modelDef.Schema +"_"+ modelDef.ModelName: modelDef.ModelName ).SafeVarName(),
                    string.Join("_", compositeIndex.FieldNames.ToArray()));

                var indexNames = string.Join(",", compositeIndex.FieldNames.ToArray());

                sqlIndexes.Add(
                    ToCreateIndexStatement(compositeIndex.Unique, indexName, modelDef, indexNames));
            }

            return sqlIndexes;
        }
		
		protected override string ToCreateIndexStatement(bool isUnique, string indexName, ModelDefinition modelDef, string fieldName)
        {
            return string.Format("CREATE {0} INDEX {1} ON {2} ({3} ); \n",
				isUnique ? "UNIQUE" : "", 
				indexName, 
				GetTableNameDelimited(modelDef),
				Quote(fieldName));
        }
		
		
		public override string ToExistStatement( Type fromTableType,
			object objWithProperties,
			string sqlFilter,
			params object[] filterParams)
		{
			
			var fromModelDef= OrmLiteDialectProviderBase.GetModel(fromTableType);
			var sql = new StringBuilder();
			sql.AppendFormat("SELECT 1 FROM {0}", 
			                 GetTableNameDelimited(fromModelDef));
			
			var filter = new StringBuilder();
			
			if(objWithProperties!=null){
				var tableType = objWithProperties.GetType();
				
				if(fromTableType!=tableType){
					int i=0;
					List<FieldDefinition> fpk= new List<FieldDefinition>();					
					var modelDef = OrmLiteDialectProviderBase.GetModel(tableType);
					
					foreach(var def in modelDef.FieldDefinitions){
						if( def.IsPrimaryKey) fpk.Add(def);
					}
					
					foreach (var fieldDef in fromModelDef.FieldDefinitions)
					{
						if( fieldDef.IsComputed) continue;
						try{
						
							if ( fieldDef.ReferencesType !=null 
							    && OrmLiteDialectProviderBase.GetModel( fieldDef.ReferencesType).ModelName == modelDef.ModelName ){
								if (filter.Length > 0) filter.Append(" AND ");
								filter.AppendFormat("{0} = {1}", Quote(fieldDef.FieldName),
								                    fpk[i].GetQuotedValue(objWithProperties));	
								i++;
								continue;
							}
						}
	
						catch (Exception ){
							throw;
						}
					}	
					
				}
				else{
					var modelDef = OrmLiteDialectProviderBase.GetModel(tableType);
					foreach (var fieldDef in modelDef.FieldDefinitions)
					{
						if( fieldDef.IsComputed) continue;
						try{
						
							if ( fieldDef.IsPrimaryKey ){
								if (filter.Length > 0) filter.Append(" AND ");
								filter.AppendFormat("{0} = {1}",Quote(fieldDef.FieldName), fieldDef.GetQuotedValue(objWithProperties));	
								continue;
							}
						}
	
						catch (Exception ){
							throw;
						}
					}
				}
				
								
				if( filter.Length>0) sql.AppendFormat(" WHERE {0} ", filter.ToString());
			}	
			
			if (!string.IsNullOrEmpty(sqlFilter))
			{
				sqlFilter = sqlFilter.SqlFormat(filterParams);
				if (!sqlFilter.StartsWith("ORDER ", StringComparison.InvariantCultureIgnoreCase)
					&& !sqlFilter.StartsWith("ROWS ", StringComparison.InvariantCultureIgnoreCase)) // ROWS <m> [TO <n>])
				{
					sql.Append( filter.Length>0? " AND  ": " WHERE ");
				}
				sql.Append(sqlFilter);
			}
			 		
			StringBuilder s = new StringBuilder("select 1  from RDB$DATABASE where");
			s.AppendFormat(" exists ({0})", sql.ToString() );
			return s.ToString();
		}
		
	
		
		public override string ToSelectFromProcedureStatement(object fromObjWithProperties,
		                                          Type outputModelType,       
		                                          string sqlFilter, 
		                                          params object[] filterParams)
		
		{
						
			var sbColumnValues = new StringBuilder();
			
			Type fromTableType = fromObjWithProperties.GetType();
			
			var modelDef = OrmLiteDialectProviderBase.GetModel(fromTableType);
			
			foreach (var fieldDef in modelDef.FieldDefinitions)
			{	
				if (sbColumnValues.Length > 0) sbColumnValues.Append(",");

				try
				{
					sbColumnValues.Append( fieldDef.GetQuotedValue(fromObjWithProperties) );	
				}
				catch (Exception )
				{	
					throw;
				}
			}
				
			
			StringBuilder sql = new StringBuilder();
			sql.AppendFormat("SELECT {0} FROM  {1} {2}{3}{4}  \n", 
							GetColumnNames( OrmLiteDialectProviderBase.GetModel(outputModelType) ),
			                GetTableNameDelimited(modelDef),
			                sbColumnValues.Length>0?"(":"",
			                sbColumnValues.ToString(),
			                sbColumnValues.Length>0?")":"");
			
			
			if(!string.IsNullOrEmpty(sqlFilter)){
				sqlFilter = sqlFilter.SqlFormat(filterParams);
				if (!sqlFilter.StartsWith("ORDER ", StringComparison.InvariantCultureIgnoreCase)
						&& !sqlFilter.StartsWith("ROWS ", StringComparison.InvariantCultureIgnoreCase)) // ROWS <m> [TO <n>]
				{
					sql.Append(" WHERE ");
				}
				sql.Append(sqlFilter);
			}
			
			return sql.ToString();
			
		}
		
		
		
		public override string ToExecuteProcedureStatement(object objWithProperties){
			
			var sbColumnValues = new StringBuilder();
			
			var tableType = objWithProperties.GetType();
			var modelDef = OrmLiteDialectProviderBase.GetModel(tableType);
			
			foreach (var fieldDef in modelDef.FieldDefinitions)
			{
				if (sbColumnValues.Length > 0) sbColumnValues.Append(",");
				try
				{
					sbColumnValues.Append( fieldDef.GetQuotedValue(objWithProperties) );
					
				}
				catch (Exception )
				{
					throw;
				}
			}
			
			var sql = string.Format("EXECUTE PROCEDURE {0} {1}{2}{3};",
									GetTableNameDelimited(modelDef),  
			                        sbColumnValues.Length>0?"(":"",
			                        sbColumnValues,
			                        sbColumnValues.Length>0?")":"");
			
			return sql;
		}

		
		
		private  object GetNextValue( IDbCommand dbCmd, string sequence, object value) 
		{
			Object retObj;
			
			if (value.ToString() != "0"){
				long nv;
				if (long.TryParse(value.ToString(), out nv) ){
					LastInsertId=nv;
					retObj= LastInsertId;
				}
				else{
					LastInsertId=0;
					retObj =value;
				}
				return retObj;
				
			}
			
			dbCmd.CommandText = string.Format("select next value for {0} from RDB$DATABASE",Quote(sequence));
			long result = (long) dbCmd.ExecuteScalar();
			LastInsertId=  result;
			return  result;	
			
		}
		
		public bool QuoteNames{
			get; set;
		}
		
		private string Quote(string name){
			
			return QuoteNames? 
				string.Format("\"{0}\"",name):
				RESERVED.Contains( name.ToUpper() )? string.Format("\"{0}\"",name): name;
			
		}
		
		public override string GetColumnNames(ModelDefinition modelDef){
			if(QuoteNames) return modelDef.GetColumnNames();
			else{
				var sqlColumns = new StringBuilder();
            	modelDef.FieldDefinitions.ForEach(x => 
                	sqlColumns.AppendFormat("{0}{1} ", sqlColumns.Length > 0 ? "," : "",Quote( x.FieldName )));

	        	return sqlColumns.ToString();
			}
				
		}
		

		public override string GetFieldNameDelimited(string fieldName){
			return Quote(fieldName);
		}
		
		public override string GetTableNameDelimited(ModelDefinition modelDef)
        {
            if (!modelDef.IsInSchema)
                return Quote(modelDef.ModelName);

            return Quote(string.Format("{0}_{1}", modelDef.Schema, modelDef.ModelName));
        }
       		
		
		private string Sequence(string modelName,string fieldName, string sequence){
			
			return sequence.IsNullOrEmpty() ?
				Quote( modelName+"_"+ fieldName+"_GEN"):
				Quote(sequence);	
		}
		
	}
}

/*
 DEBUG: Ignoring existing generator 'CREATE GENERATOR ModelWFDT_Id_GEN;': unsuccessful metadata update
DEFINE GENERATOR failed
attempt to store duplicate value (visible to active transactions) in unique index "RDB$INDEX_11" 
*/

