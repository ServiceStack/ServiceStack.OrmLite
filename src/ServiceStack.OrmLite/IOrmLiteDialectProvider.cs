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
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
    public interface IOrmLiteDialectProvider
    {
        int DefaultStringLength { get; set; }

        string ParamString { get; set; }

        bool UseUnicode { get; set; }

        INamingStrategy NamingStrategy { get; set; }

        /// <summary>
        /// Quote the string so that it can be used inside an SQL-expression
        /// Escape quotes inside the string
        /// </summary>
        /// <param name="paramValue"></param>
        /// <returns></returns>
        string GetQuotedParam(string paramValue);

        object ConvertDbValue(object value, Type type);

        string GetQuotedValue(object value, Type fieldType);

        IDbConnection CreateConnection(string filePath, Dictionary<string, string> options);

        string GetQuotedTableName(ModelDefinition modelDef);

        string GetQuotedTableName(string tableName);

        string GetQuotedColumnName(string columnName);

        string GetQuotedName(string columnName);

        string GetColumnDefinition(
            string fieldName, Type fieldType, bool isPrimaryKey, bool autoIncrement,
            bool isNullable, int? fieldLength,
            int? scale, string defaultValue);

        long GetLastInsertId(IDbCommand command);

        long InsertAndGetLastInsertId<T>(IDbCommand dbCmd);

        string ToSelectStatement(Type tableType, string sqlFilter, params object[] filterParams);

        string ToInsertRowStatement(IDbCommand command, object objWithProperties, ICollection<string> InsertFields = null);

        IDbCommand CreateParameterizedInsertStatement(IDbConnection connection, object objWithProperties, ICollection<string> insertFields = null);

        void ReParameterizeInsertStatement(IDbCommand command, object objWithProperties, ICollection<string> insertFields = null);

        string ToUpdateRowStatement(object objWithProperties, ICollection<string> UpdateFields = null);

        IDbCommand CreateParameterizedUpdateStatement(IDbConnection connection, object objWithProperties, ICollection<string> updateFields = null);

        string ToDeleteRowStatement(object objWithProperties);
        string ToDeleteStatement(Type tableType, string sqlFilter, params object[] filterParams);

        IDbCommand CreateParameterizedDeleteStatement(IDbConnection connection, object objWithProperties);

        string ToExistStatement(Type fromTableType,
            object objWithProperties,
            string sqlFilter,
            params object[] filterParams);

        string ToSelectFromProcedureStatement(object fromObjWithProperties,
            Type outputModelType,
            string sqlFilter,
            params object[] filterParams);

        string ToCountStatement(Type fromTableType, string sqlFilter, params object[] filterParams);

        string ToExecuteProcedureStatement(object objWithProperties);

        string ToCreateTableStatement(Type tableType);

        List<string> ToCreateIndexStatements(Type tableType);
        List<string> ToCreateSequenceStatements(Type tableType);        
        string ToCreateSequenceStatement(Type tableType, string sequenceName);

        List<string> SequenceList(Type tableType);

        bool DoesTableExist(IDbConnection db, string tableName);
        bool DoesTableExist(IDbCommand dbCmd, string tableName);

        bool DoesSequenceExist(IDbCommand dbCmd, string sequencName);

        string GetColumnNames(ModelDefinition modelDef);

        SqlExpressionVisitor<T> ExpressionVisitor<T>();

        DbType GetColumnDbType(Type valueType);
        string GetColumnTypeDefinition(Type fieldType);

        string GetDropForeignKeyConstraints(ModelDefinition modelDef);

		#region DDL 
		string ToAddColumnStatement (Type modelType, FieldDefinition fieldDef);
		string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef);
		string ToChangeColumnNameStatement(Type modelType, FieldDefinition fieldDef, string oldColumnName);
		string ToAddForeignKeyStatement<T,TForeign>(Expression<Func<T,object>> field,
		                                             Expression<Func<TForeign,object>> foreignField,
		                                             OnFkOption onUpdate,
		                                             OnFkOption onDelete,
		                                             string foreignKeyName=null);
		string ToCreateIndexStatement<T>(Expression<Func<T,object>> field,
		                                 string indexName=null, bool unique=false);
		#endregion DDL

    }
}