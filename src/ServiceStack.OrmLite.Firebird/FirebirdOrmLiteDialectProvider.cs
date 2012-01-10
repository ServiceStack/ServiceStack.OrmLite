using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using ServiceStack.Common.Utils;
using System.Text;
using FirebirdSql.Data.FirebirdClient;
using ServiceStack.OrmLite;

namespace ServiceStack.OrmLite.Firebird
{
	public class FirebirdOrmLiteDialectProvider:OrmLiteDialectProviderBase
	{
		public static FirebirdOrmLiteDialectProvider Instance = new FirebirdOrmLiteDialectProvider();
		
		internal  long LastInsertId{
			get; set;
		}
		
		public FirebirdOrmLiteDialectProvider()
		{
			base.BoolColumnDefinition = base.IntColumnDefinition;
			base.GuidColumnDefinition = "CHAR(16) character set octets";
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
				return string.Format("CAST('{0}' AS CHAR(16) character set octets)", guidValue);  // TODO : check this !!!
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
			
			sql.AppendFormat("SELECT {0} FROM \"{1}\"", 
			                 modelDef.GetColumnNames(), 
			                 modelDef.ModelName);
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
				if( (fieldDef.AutoIncrement && string.IsNullOrEmpty(fieldDef.Sequence) ) 
				   || fieldDef.IsComputed) continue;
				
				if( !string.IsNullOrEmpty(fieldDef.Sequence)  && dbCommand!=null ){
					PropertyInfo pi = ReflectionUtils.GetPropertyInfo(tableType, fieldDef.Name);
					
					var result = GetNextValue(dbCommand, fieldDef.Sequence );
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
					sbColumnNames.Append(string.Format("\"{0}\"", fieldDef.FieldName));
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

			var sql = string.Format("INSERT INTO \"{0}\" ({1}) VALUES ({2});",
									modelDef.ModelName, sbColumnNames, sbColumnValues);

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

						sqlFilter.AppendFormat("\"{0}\" = {1}", fieldDef.FieldName, fieldDef.GetQuotedValue(objWithProperties));
							
						continue;
					}

					if (sql.Length > 0) sql.Append(",");
					sql.AppendFormat("\"{0}\" = {1}", fieldDef.FieldName, fieldDef.GetQuotedValue(objWithProperties));
				}
				catch (Exception )
				{
					throw;
				}
			}

			var updateSql = string.Format("UPDATE \"{0}\" SET {1} WHERE {2}",
				modelDef.ModelName, sql, sqlFilter);

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

						sqlFilter.AppendFormat("\"{0}\" = {1}", fieldDef.FieldName, fieldDef.GetQuotedValue(objWithProperties));
					}
				}
				catch (Exception )
				{
					throw;
				}
			}

			var deleteSql = string.Format("DELETE FROM \"{0}\" WHERE {1}",
				modelDef.ModelName, sqlFilter);

			return deleteSql;
		}
		
		
		
		public override string ToExistStatement( Type fromTableType,
			object objWithProperties,
			string sqlFilter,
			params object[] filterParams)
		{
			
			var fromModelDef= OrmLiteDialectProviderBase.GetModel(fromTableType);
			var sql = new StringBuilder();
			sql.AppendFormat("SELECT 1 FROM \"{0}\"", 
			                 fromModelDef.ModelName);
			
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
								filter.AppendFormat("\"{0}\" = {1}", fieldDef.FieldName,
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
								filter.AppendFormat("\"{0}\" = {1}", fieldDef.FieldName, fieldDef.GetQuotedValue(objWithProperties));	
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
			sql.AppendFormat("SELECT {0} FROM  \"{1}\" {2}{3}{4}  \n", 
							OrmLiteDialectProviderBase.GetModel(outputModelType).GetColumnNames(),
			                modelDef.ModelName,
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
			
			var sql = string.Format("EXECUTE PROCEDURE \"{0}\" {1}{2}{3};",
									modelDef.ModelName,  
			                        sbColumnValues.Length>0?"(":"",
			                        sbColumnValues,
			                        sbColumnValues.Length>0?")":"");
			
			return sql;
		}

		
		
		public  long GetNextValue( IDbCommand dbCmd, string sequence) 
		{
			dbCmd.CommandText = string.Format("select next value for \"{0}\" from RDB$DATABASE",sequence);
			long result = (long) dbCmd.ExecuteScalar();
			LastInsertId=  result;
			return  result;	
			
		}
		
	}
}

