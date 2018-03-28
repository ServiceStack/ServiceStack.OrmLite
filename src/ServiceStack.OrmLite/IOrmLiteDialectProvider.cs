//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2013 ServiceStack, Inc. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public interface IOrmLiteDialectProvider
    {
        void RegisterConverter<T>(IOrmLiteConverter converter);

        /// <summary>
        /// Invoked when a DB Connection is opened
        /// </summary>
        Action<IDbConnection> OnOpenConnection { get; set; } 
            

        IOrmLiteExecFilter ExecFilter { get; set; }

        /// <summary>
        /// Gets the explicit Converter registered for a specific type
        /// </summary>
        IOrmLiteConverter GetConverter(Type type);

        /// <summary>
        /// Return best matching converter, falling back to Enum, Value or Ref Type Converters
        /// </summary>
        IOrmLiteConverter GetConverterBestMatch(Type type);

        IOrmLiteConverter GetConverterBestMatch(FieldDefinition fieldDef);

        string ParamString { get; set; }

        string EscapeWildcards(string value);

        INamingStrategy NamingStrategy { get; set; }

        IStringSerializer StringSerializer { get; set; }

        Func<string, string> ParamNameFilter { get; set; }

        /// <summary>
        /// Quote the string so that it can be used inside an SQL-expression
        /// Escape quotes inside the string
        /// </summary>
        /// <param name="paramValue"></param>
        /// <returns></returns>
        string GetQuotedValue(string paramValue);

        string GetQuotedValue(object value, Type fieldType);

        string GetDefaultValue(Type tableType, string fieldName);

        string GetDefaultValue(FieldDefinition fieldDef);

        bool HasInsertReturnValues(ModelDefinition modelDef);

        object GetParamValue(object value, Type fieldType);

        object ToDbValue(object value, Type type);

        object FromDbValue(object value, Type type);

        object GetValue(IDataReader reader, int columnIndex, Type type);

        int GetValues(IDataReader reader, object[] values);

        IDbConnection CreateConnection(string filePath, Dictionary<string, string> options);

        string GetTableName(ModelDefinition modelDef);

        string GetTableName(string tableName, string schema = null);

        string GetQuotedTableName(ModelDefinition modelDef);

        string GetQuotedTableName(string tableName, string schema=null);

        string GetQuotedColumnName(string columnName);

        string GetQuotedName(string columnName);

        string SanitizeFieldNameForParamName(string fieldName);

        string GetColumnDefinition(FieldDefinition fieldDef);

        long GetLastInsertId(IDbCommand command);

        string GetLastInsertIdSqlSuffix<T>();

        string ToSelectStatement(Type tableType, string sqlFilter, params object[] filterParams);

        string ToSelectStatement(ModelDefinition modelDef, string selectExpression, string bodyExpression, string orderByExpression = null, int? offset = null, int? rows = null);

        string ToInsertRowStatement(IDbCommand cmd, object objWithProperties, ICollection<string> InsertFields = null);

        void PrepareParameterizedInsertStatement<T>(IDbCommand cmd, ICollection<string> insertFields = null);

        /// <returns>If had RowVersion</returns>
        bool PrepareParameterizedUpdateStatement<T>(IDbCommand cmd, ICollection<string> updateFields = null);

        /// <returns>If had RowVersion</returns>
        bool PrepareParameterizedDeleteStatement<T>(IDbCommand cmd, IDictionary<string, object> delteFieldValues);

        void PrepareStoredProcedureStatement<T>(IDbCommand cmd, T obj);

        void SetParameterValues<T>(IDbCommand dbCmd, object obj);

        void SetParameter(FieldDefinition fieldDef, IDbDataParameter p);

        Dictionary<string, FieldDefinition> GetFieldDefinitionMap(ModelDefinition modelDef);

        object GetFieldValue(FieldDefinition fieldDef, object value);
        object GetFieldValue(Type fieldType, object value);

        void PrepareUpdateRowStatement(IDbCommand dbCmd, object objWithProperties, ICollection<string> UpdateFields = null);

        void PrepareUpdateRowStatement<T>(IDbCommand dbCmd, Dictionary<string, object> args, string sqlFilter);

        void PrepareUpdateRowAddStatement<T>(IDbCommand dbCmd, Dictionary<string, object> args, string sqlFilter);

        void PrepareInsertRowStatement<T>(IDbCommand dbCmd, Dictionary<string, object> args);

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

        bool DoesTableExist(IDbConnection db, string tableName, string schema = null);
        bool DoesTableExist(IDbCommand dbCmd, string tableName, string schema = null);
        bool DoesColumnExist(IDbConnection db, string columnName, string tableName, string schema = null);
        bool DoesSequenceExist(IDbCommand dbCmd, string sequencName);

        void DropColumn(IDbConnection db, Type modelType, string columnName);

        object FromDbRowVersion(Type fieldType,  object value);

        SelectItem GetRowVersionColumnName(FieldDefinition field, string tablePrefix = null);

        string GetColumnNames(ModelDefinition modelDef);
        SelectItem[] GetColumnNames(ModelDefinition modelDef, bool tableQualified);

        SqlExpression<T> SqlExpression<T>();

        IDbDataParameter CreateParam();

        void InitDbParam(IDbDataParameter dbParam, Type columnType);

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

        //Async
        Task OpenAsync(IDbConnection db, CancellationToken token = default(CancellationToken));
        Task<IDataReader> ExecuteReaderAsync(IDbCommand cmd, CancellationToken token = default(CancellationToken));
        Task<int> ExecuteNonQueryAsync(IDbCommand cmd, CancellationToken token = default(CancellationToken));
        Task<object> ExecuteScalarAsync(IDbCommand cmd, CancellationToken token = default(CancellationToken));
        Task<bool> ReadAsync(IDataReader reader, CancellationToken token = default(CancellationToken));
        Task<List<T>> ReaderEach<T>(IDataReader reader, Func<T> fn, CancellationToken token = default(CancellationToken));
        Task<Return> ReaderEach<Return>(IDataReader reader, Action fn, Return source, CancellationToken token = default(CancellationToken));
        Task<T> ReaderRead<T>(IDataReader reader, Func<T> fn, CancellationToken token = default(CancellationToken));

        Task<long> InsertAndGetLastInsertIdAsync<T>(IDbCommand dbCmd, CancellationToken token);
    
        string GetLoadChildrenSubSelect<From>(SqlExpression<From> expr);
        string ToRowCountStatement(string innerSql);

        string ToUpdateStatement<T>(IDbCommand dbCmd, T item, ICollection<string> updateFields = null);
        string ToInsertStatement<T>(IDbCommand dbCmd, T item, ICollection<string> insertFields = null);
        string MergeParamsIntoSql(string sql, IEnumerable<IDbDataParameter> dbParams);

        string SqlConflict(string sql, string conflictResolution);

        string SqlConcat(IEnumerable<object> args);
        string SqlCurrency(string fieldOrValue);
        string SqlCurrency(string fieldOrValue, string currencySymbol);
        string SqlBool(bool value);
        string SqlLimit(int? offset = null, int? rows = null);
        string SqlCast(object fieldOrValue, string castAs);
    }
}