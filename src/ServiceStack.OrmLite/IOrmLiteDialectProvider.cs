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

        // 

        string ToSelectStatement(Type tableType, string sqlFilter, params object[] filterParams);

        string ToInsertRowStatement(object objWithProperties, IDbCommand command);
        string ToInsertRowStatement(object objWithProperties, IList<string> InsertFields, IDbCommand command);

        IDbCommand CreateParameterizedInsertStatement(object objWithProperties, IDbConnection connection);
        IDbCommand CreateParameterizedInsertStatement(object objWithProperties, IList<string> insertFields,
                                                      IDbConnection connection);

        void ReParameterizeInsertStatement(object objWithProperties, IDbCommand command);
        void ReParameterizeInsertStatement(object objWithProperties, IList<string> insertFields, IDbCommand command);

        string ToUpdateRowStatement(object objWithProperties);
        string ToUpdateRowStatement(object objWithProperties, IList<string> UpdateFields);

        IDbCommand CreateParameterizedUpdateStatement(object objWithProperties, IDbConnection connection);
        IDbCommand CreateParameterizedUpdateStatement(object objWithProperties, IList<string> updateFields, IDbConnection connection);

        string ToDeleteRowStatement(object objWithProperties);
        string ToDeleteStatement(Type tableType, string sqlFilter, params object[] filterParams);

        IDbCommand CreateParameterizedDeleteStatement(object objWithProperties, IDbConnection connection);

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
    }
}