//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2013 Service Stack LLC. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public interface IOrmLiteDialectProvider
    {
        IOrmLiteExecFilter ExecFilter { get; set; }

        int DefaultStringLength { get; set; }

        string ParamString { get; set; }

        bool UseUnicode { get; set; }

        string EscapeWildcards(string value);

        INamingStrategy NamingStrategy { get; set; }

        IStringSerializer StringSerializer { get; set; }

        /// <summary>
        /// Quote the string so that it can be used inside an SQL-expression
        /// Escape quotes inside the string
        /// </summary>
        /// <param name="paramValue"></param>
        /// <returns></returns>
        string GetQuotedValue(string paramValue);

        void SetDbValue(FieldDefinition fieldDef, IDataReader reader, int colIndex, object instance);

        object ConvertDbValue(object value, Type type);

        string GetQuotedValue(object value, Type fieldType);

        IDbConnection CreateConnection(string filePath, Dictionary<string, string> options);

        string GetQuotedTableName(ModelDefinition modelDef);

        string GetQuotedTableName(string tableName);

        string GetQuotedColumnName(string columnName);

        string GetQuotedName(string columnName);

        string SanitizeFieldNameForParamName(string fieldName);

        string GetColumnDefinition(
            string fieldName, Type fieldType, bool isPrimaryKey, bool autoIncrement,
            bool isNullable, 
            bool isRowVersion,
            int? fieldLength,
            int? scale, 
            string defaultValue,
            string customFieldDefinition);

        long GetLastInsertId(IDbCommand command);

        long InsertAndGetLastInsertId<T>(IDbCommand dbCmd);

        string ToSelectStatement(Type tableType, string sqlFilter, params object[] filterParams);

        string ToSelectStatement(ModelDefinition modelDef, string selectExpression, string bodyExpression, string orderByExpression = null, int? offset = null, int? rows = null);

        string ToInsertRowStatement(IDbCommand command, object objWithProperties, ICollection<string> InsertFields = null);

        void PrepareParameterizedInsertStatement<T>(IDbCommand cmd, ICollection<string> insertFields = null);

        bool PrepareParameterizedUpdateStatement<T>(IDbCommand cmd, ICollection<string> updateFields = null);

        bool PrepareParameterizedDeleteStatement<T>(IDbCommand cmd, ICollection<string> deleteFields = null);

        void SetParameterValues<T>(IDbCommand dbCmd, object obj);

        string ToUpdateRowStatement(object objWithProperties, ICollection<string> UpdateFields = null);

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

        string ToExecuteProcedureStatement(object objWithProperties);

        string ToCreateTableStatement(Type tableType);
        string ToPostCreateTableStatement(ModelDefinition modelDef);
        string ToPostDropTableStatement(ModelDefinition modelDef);

        List<string> ToCreateIndexStatements(Type tableType);
        List<string> ToCreateSequenceStatements(Type tableType);
        string ToCreateSequenceStatement(Type tableType, string sequenceName);

        List<string> SequenceList(Type tableType);

        bool DoesTableExist(IDbConnection db, string tableName);
        bool DoesTableExist(IDbCommand dbCmd, string tableName);

        bool DoesSequenceExist(IDbCommand dbCmd, string sequencName);

        string GetRowVersionColumnName(FieldDefinition field);
        string GetColumnNames(ModelDefinition modelDef);

        SqlExpression<T> SqlExpression<T>();

        DbType GetColumnDbType(Type valueType);
        string GetColumnTypeDefinition(Type fieldType);

        //DDL
        string GetDropForeignKeyConstraints(ModelDefinition modelDef);

        string ToAddColumnStatement(Type modelType, FieldDefinition fieldDef);
        string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef);
        string ToChangeColumnNameStatement(Type modelType, FieldDefinition fieldDef, string oldColumnName);
        string ToAddForeignKeyStatement<T, TForeign>(Expression<Func<T, object>> field,
                                                     Expression<Func<TForeign, object>> foreignField,
                                                     OnFkOption onUpdate,
                                                     OnFkOption onDelete,
                                                     string foreignKeyName = null);
        string ToCreateIndexStatement<T>(Expression<Func<T, object>> field,
                                         string indexName = null, bool unique = false);
    }
}