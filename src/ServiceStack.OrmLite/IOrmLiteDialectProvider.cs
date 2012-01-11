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

namespace ServiceStack.OrmLite
{
	public interface IOrmLiteDialectProvider
	{
		int DefaultStringLength { get; set; }
		
		bool UseUnicode { get; set; }

		string EscapeParam(object paramValue);

		object ConvertDbValue(object value, Type type);

		string GetQuotedValue(object value, Type fieldType);

		IDbConnection CreateConnection(string filePath, Dictionary<string, string> options);

	    string GetTableNameDelimited(ModelDefinition modelDef);

		string GetColumnDefinition(
			string fieldName, Type fieldType, bool isPrimaryKey, bool autoIncrement, 
			bool isNullable, int? fieldLength, 
			int? scale, string defaultValue);

		long GetLastInsertId(IDbCommand command);
				
		// 
						
		string ToSelectStatement( Type tableType, string sqlFilter, params object[] filterParams);
		
		string ToInsertRowStatement( object objWithProperties, IDbCommand command);
		
		string ToUpdateRowStatement(object objWithProperties);
		
		string ToDeleteRowStatement(object objWithProperties);
		
		string ToExistStatement( Type fromTableType,
			object objWithProperties,
			string sqlFilter,
			params object[] filterParams);
		
		string ToSelectFromProcedureStatement(object fromObjWithProperties,
		                                          Type outputModelType,       
		                                          string sqlFilter, 
		                                          params object[] filterParams);
		
		string ToExecuteProcedureStatement(object objWithProperties);
		
		string ToCreateTableStatement(Type tableType);
		
		List<string> ToCreateIndexStatements(Type tableType);
		List<string> ToCreateSequenceStatements(Type tableType);
		
	}
	
}