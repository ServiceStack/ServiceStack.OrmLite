﻿#if NET45
// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Logging;
using ServiceStack.OrmLite.Support;

namespace ServiceStack.OrmLite
{
    internal static class OrmLiteReadCommandExtensionsAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OrmLiteReadCommandExtensionsAsync));

        internal static Task<IDataReader> ExecReaderAsync(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            dbCmd.CommandTimeout = OrmLiteConfig.CommandTimeout;
            dbCmd.CommandText = sql;

            if (Log.IsDebugEnabled)
                Log.DebugCommand(dbCmd);

            return dbCmd.GetDialectProvider().ExecuteReaderAsync(dbCmd, token);
        }

        internal static Task<IDataReader> ExecReaderAsync(this IDbCommand dbCmd, string sql, IEnumerable<IDataParameter> parameters, CancellationToken token)
        {
            dbCmd.CommandTimeout = OrmLiteConfig.CommandTimeout;
            dbCmd.CommandText = sql;
            dbCmd.Parameters.Clear();

            foreach (var param in parameters)
            {
                dbCmd.Parameters.Add(param);
            }

            if (Log.IsDebugEnabled)
                Log.DebugCommand(dbCmd);

            return dbCmd.GetDialectProvider().ExecuteReaderAsync(dbCmd, token);
        }

        internal static Task<List<T>> SelectAsync<T>(this IDbCommand dbCmd, CancellationToken token)
        {
            return SelectFmtAsync<T>(dbCmd, token, (string)null);
        }

        internal static Task<List<T>> SelectFmtAsync<T>(this IDbCommand dbCmd, CancellationToken token, string sqlFilter, params object[] filterParams)
        {
            return dbCmd.ConvertToListAsync<T>(
                dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sqlFilter, filterParams), token);
        }

        internal static Task<List<TModel>> SelectAsync<TModel>(this IDbCommand dbCmd, Type fromTableType, CancellationToken token)
        {
            return SelectFmtAsync<TModel>(dbCmd, token, fromTableType, null);
        }

        internal static Task<List<TModel>> SelectFmtAsync<TModel>(this IDbCommand dbCmd, CancellationToken token, Type fromTableType, string sqlFilter, params object[] filterParams)
        {
            var sql = OrmLiteReadCommandExtensions.ToSelectFmt<TModel>(dbCmd.GetDialectProvider(), fromTableType, sqlFilter, filterParams);
            return dbCmd.ConvertToListAsync<TModel>(sql.ToString(), token);
        }

        internal static Task<T> SelectByIdFmtAsync<T>(this IDbCommand dbCmd, object idValue, CancellationToken token)
        {
            return dbCmd.SingleFmtAsync<T>(token, dbCmd.GetDialectProvider().GetQuotedColumnName(ModelDefinition<T>.PrimaryKeyName) + " = {0}".SqlFmt(idValue));
        }

        internal static Task<T> SingleFmtAsync<T>(this IDbCommand dbCmd, CancellationToken token, string filter, params object[] filterParams)
        {
            return dbCmd.ConvertToAsync<T>(dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), filter, filterParams), token);
        }

        internal static Task<List<T>> SelectByIdsAsync<T>(this IDbCommand dbCmd, IEnumerable idValues, CancellationToken token)
        {
            var sql = idValues.GetIdsInSql();
            return sql == null
                ? new List<T>().InTask()
                : SelectFmtAsync<T>(dbCmd, token, dbCmd.GetDialectProvider().GetQuotedColumnName(ModelDefinition<T>.PrimaryKeyName) + " IN (" + sql + ")");
        }

        internal static Task<T> SingleByIdAsync<T>(this IDbCommand dbCmd, object value, CancellationToken token)
        {
            if (!dbCmd.CanReuseParam<T>(ModelDefinition<T>.PrimaryKeyName))
                dbCmd.SetFilter<T>(ModelDefinition<T>.PrimaryKeyName, value);

            ((IDbDataParameter)dbCmd.Parameters[0]).Value = value;

            return dbCmd.ConvertToAsync<T>(null, token);
        }

        internal static Task<T> SingleWhereAsync<T>(this IDbCommand dbCmd, string name, object value, CancellationToken token)
        {
            if (!dbCmd.CanReuseParam<T>(name))
                dbCmd.SetFilter<T>(name, value);

            ((IDbDataParameter)dbCmd.Parameters[0]).Value = value;

            return dbCmd.ConvertToAsync<T>(null, token);
        }

        internal static Task<T> SingleAsync<T>(this IDbCommand dbCmd, object anonType, CancellationToken token)
        {
            return dbCmd.SetFilters<T>(anonType, excludeDefaults: false).ConvertToAsync<T>(null, token);
        }

        internal static Task<T> SingleAsync<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            return OrmLiteUtils.IsScalar<T>()
                ? dbCmd.ScalarAsync<T>(sql, sqlParams, token)
                : dbCmd.SetParameters(sqlParams).ConvertToAsync<T>(dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql), token);
        }

        internal static Task<T> SingleAsync<T>(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            return OrmLiteUtils.IsScalar<T>()
                ? dbCmd.ScalarAsync<T>(sql, anonType, token)
                : dbCmd.SetParameters<T>(anonType, excludeDefaults: false).ConvertToAsync<T>(dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql), token);
        }

        internal static Task<List<T>> WhereAsync<T>(this IDbCommand dbCmd, string name, object value, CancellationToken token)
        {
            if (!dbCmd.CanReuseParam<T>(name))
                dbCmd.SetFilter<T>(name, value);

            ((IDbDataParameter)dbCmd.Parameters[0]).Value = value;

            return dbCmd.ConvertToListAsync<T>(null, token);
        }

        internal static Task<List<T>> WhereAsync<T>(this IDbCommand dbCmd, object anonType, CancellationToken token)
        {
            return dbCmd.SetFilters<T>(anonType).ConvertToListAsync<T>(null, token);
        }

        internal static Task<List<T>> SelectAsync<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            dbCmd.SetParameters(sqlParams).CommandText = dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql);
            return dbCmd.ConvertToListAsync<T>(null, token);
        }

        internal static Task<List<T>> SelectAsync<T>(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            dbCmd.SetParameters<T>(anonType, excludeDefaults: false).CommandText = dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql);
            return dbCmd.ConvertToListAsync<T>(null, token);
        }

        internal static Task<List<T>> SelectAsync<T>(this IDbCommand dbCmd, string sql, Dictionary<string, object> dict, CancellationToken token)
        {
            dbCmd.SetParameters(dict, excludeDefaults: false).CommandText = dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql);
            return dbCmd.ConvertToListAsync<T>(null, token);
        }

        internal static Task<List<T>> SqlListAsync<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            dbCmd.SetParameters(sqlParams).CommandText = sql;
            return dbCmd.ConvertToListAsync<T>(null, token);
        }

        internal static Task<List<T>> SqlListAsync<T>(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            dbCmd.SetParameters<T>(anonType, excludeDefaults: false).CommandText = sql;
            return dbCmd.ConvertToListAsync<T>(null, token);
        }

        internal static Task<List<T>> SqlListAsync<T>(this IDbCommand dbCmd, string sql, Dictionary<string, object> dict, CancellationToken token)
        {
            dbCmd.SetParameters(dict, excludeDefaults: false).CommandText = sql;
            return dbCmd.ConvertToListAsync<T>(null, token);
        }

        internal static Task<List<T>> SqlListAsync<T>(this IDbCommand dbCmd, string sql, Action<IDbCommand> dbCmdFilter, CancellationToken token)
        {
            if (dbCmdFilter != null) dbCmdFilter(dbCmd);
            dbCmd.CommandText = sql;

            return dbCmd.ConvertToListAsync<T>(null, token);
        }

        internal static Task<List<T>> SqlColumnAsync<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            dbCmd.SetParameters(sqlParams).CommandText = sql;
            return dbCmd.ConvertToListAsync<T>(null, token);
        }

        internal static Task<List<T>> SqlColumnAsync<T>(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            dbCmd.SetParameters(anonType, excludeDefaults: false).CommandText = sql;
            return dbCmd.ConvertToListAsync<T>(null, token);
        }

        internal static Task<List<T>> SqlColumnAsync<T>(this IDbCommand dbCmd, string sql, Dictionary<string, object> dict, CancellationToken token)
        {
            dbCmd.SetParameters(dict, excludeDefaults: false).CommandText = sql;
            return dbCmd.ConvertToListAsync<T>(null, token);
        }

        internal static Task<T> SqlScalarAsync<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            return dbCmd.SetParameters(sqlParams).ScalarAsync<T>(sql, token);
        }

        internal static Task<T> SqlScalarAsync<T>(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            return dbCmd.SetParameters<T>(anonType, excludeDefaults: false).ScalarAsync<T>(sql, token);
        }

        internal static Task<T> SqlScalarAsync<T>(this IDbCommand dbCmd, string sql, Dictionary<string, object> dict, CancellationToken token)
        {
            return dbCmd.SetParameters(dict, excludeDefaults: false).ScalarAsync<T>(sql, token);
        }

        internal static Task<List<T>> SelectNonDefaultsAsync<T>(this IDbCommand dbCmd, object filter, CancellationToken token)
        {
            return dbCmd.SetFilters<T>(filter, excludeDefaults: true).ConvertToListAsync<T>(null, token);
        }

        internal static Task<List<T>> SelectNonDefaultsAsync<T>(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            return dbCmd.SetParameters<T>(anonType, excludeDefaults: true).ConvertToListAsync<T>(dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql), token);
        }

        internal static Task<T> ScalarAsync<T>(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            return dbCmd.SetParameters<T>(anonType, excludeDefaults: false).ScalarAsync<T>(sql, token);
        }

        internal static Task<T> ScalarFmtAsync<T>(this IDbCommand dbCmd, CancellationToken token, string sql, params object[] sqlParams)
        {
            return dbCmd.ScalarAsync<T>(sql.SqlFmt(sqlParams), token);
        }

        internal static Task<T> ScalarAsync<T>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider, CancellationToken token)
        {
            return dialectProvider.ReaderRead(reader, () => 
                OrmLiteReadCommandExtensions.ToScalar<T>(dialectProvider, reader), token);
        }

        public static Task<long> LongScalarAsync(this IDbCommand dbCmd, CancellationToken token)
        {
            return dbCmd.GetDialectProvider().ExecuteScalarAsync(dbCmd, token)
                                .Then(OrmLiteReadCommandExtensions.ToLong);
        }

        internal static Task<List<T>> ColumnAsync<T>(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeDefaults: false);

            return dbCmd.ColumnAsync<T>(dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql), token);
        }

        internal static Task<List<T>> ColumnFmtAsync<T>(this IDbCommand dbCmd, CancellationToken token, string sql, params object[] sqlParams)
        {
            return dbCmd.ColumnAsync<T>(sql.SqlFmt(sqlParams), token);
        }

        internal static Task<List<T>> ColumnAsync<T>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider, CancellationToken token)
        {
            return dialectProvider.ReaderEach(reader, () =>
            {
                var value = dialectProvider.FromDbValue(reader, 0, typeof(T));
                return value == DBNull.Value ? default(T) : value;
            }, token)
            .Then(x =>
            {
                var columValues = new List<T>();
                x.Each(o => columValues.Add((T)o));
                return columValues;
            });
        }

        internal static Task<HashSet<T>> ColumnDistinctAsync<T>(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeDefaults: false);

            return dbCmd.ColumnDistinctAsync<T>(sql, token);
        }

        internal static Task<HashSet<T>> ColumnDistinctFmtAsync<T>(this IDbCommand dbCmd, CancellationToken token, string sql, params object[] sqlParams)
        {
            return dbCmd.ColumnDistinctAsync<T>(sql.SqlFmt(sqlParams), token);
        }

        internal static Task<HashSet<T>> ColumnDistinctAsync<T>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider, CancellationToken token)
        {
            return dialectProvider.ReaderEach(reader, () =>
            {
                var value = dialectProvider.FromDbValue(reader, 0, typeof(T));
                return value == DBNull.Value ? default(T) : value;
            }, token)
            .Then(x =>
            {
                var columValues = new HashSet<T>();
                x.Each(o => columValues.Add((T)o));
                return columValues;
            });
        }

        internal static Task<Dictionary<K, List<V>>> LookupAsync<K, V>(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            if (anonType != null) 
                dbCmd.SetParameters(anonType, (bool)false);

            return dbCmd.LookupAsync<K, V>(sql, token);
        }

        internal static Task<Dictionary<K, List<V>>> LookupFmtAsync<K, V>(this IDbCommand dbCmd, CancellationToken token, string sql, params object[] sqlParams)
        {
            return dbCmd.LookupAsync<K, V>(sql.SqlFmt(sqlParams), token);
        }

        internal static Task<Dictionary<K, List<V>>> LookupAsync<K, V>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider, CancellationToken token)
        {
            var lookup = new Dictionary<K, List<V>>();

            return dialectProvider.ReaderEach(reader, () =>
            {
                var key = (K)dialectProvider.FromDbValue(reader, 0, typeof(K));
                var value = (V)dialectProvider.FromDbValue(reader, 1, typeof(V));

                List<V> values;
                if (!lookup.TryGetValue(key, out values))
                {
                    values = new List<V>();
                    lookup[key] = values;
                }
                values.Add(value);
            }, lookup, token);
        }

        internal static Task<Dictionary<K, V>> DictionaryAsync<K, V>(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            if (anonType != null) 
                dbCmd.SetParameters(anonType, excludeDefaults: false);

            return dbCmd.DictionaryAsync<K, V>(sql, token);
        }

        internal static Task<Dictionary<K, V>> DictionaryFmtAsync<K, V>(this IDbCommand dbCmd, CancellationToken token, string sqlFormat, params object[] sqlParams)
        {
            return dbCmd.DictionaryAsync<K, V>(sqlFormat.SqlFmt(sqlParams), token);
        }

        internal static Task<Dictionary<K, V>> DictionaryAsync<K, V>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider, CancellationToken token)
        {
            var map = new Dictionary<K, V>();

            return dialectProvider.ReaderEach(reader, () =>
            {
                var key = (K)dialectProvider.FromDbValue(reader, 0, typeof(K));
                var value = (V)dialectProvider.FromDbValue(reader, 1, typeof(V));
                map.Add(key, value);
            }, map, token);
        }

        internal static Task<bool> ExistsAsync<T>(this IDbCommand dbCmd, object anonType, CancellationToken token)
        {
            if (anonType != null) dbCmd.SetParameters(anonType, excludeDefaults: true);

            var sql = dbCmd.GetFilterSql<T>();

            return dbCmd.ScalarAsync(sql, token).Then(x => x != null);
        }

        internal static Task<bool> ExistsAsync<T>(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            if (anonType != null) dbCmd.SetParameters(anonType, (bool)false);

            return dbCmd.ScalarAsync(dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql), token)
                .Then(x => x != null);
        }

        internal static Task<bool> ExistsFmtAsync<T>(this IDbCommand dbCmd, CancellationToken token, string sqlFilter, params object[] filterParams)
        {
            var fromTableType = typeof(T);
            return dbCmd.ScalarAsync(dbCmd.GetDialectProvider().ToSelectStatement(fromTableType, sqlFilter, filterParams), token)
                .Then(x => x != null);
        }

        // procedures ...		
        internal static Task<List<TOutputModel>> SqlProcedureAsync<TOutputModel>(this IDbCommand dbCommand, object fromObjWithProperties, CancellationToken token)
        {
            return SqlProcedureFmtAsync<TOutputModel>(dbCommand, token, fromObjWithProperties, String.Empty);
        }

        internal static Task<List<TOutputModel>> SqlProcedureFmtAsync<TOutputModel>(this IDbCommand dbCmd, CancellationToken token,
            object fromObjWithProperties,
            string sqlFilter,
            params object[] filterParams)
        {
            var modelType = typeof(TOutputModel);

            string sql = dbCmd.GetDialectProvider().ToSelectFromProcedureStatement(
                fromObjWithProperties, modelType, sqlFilter, filterParams);

            return dbCmd.ConvertToListAsync<TOutputModel>(sql, token);
        }

        internal static async Task<T> LoadSingleByIdAsync<T>(this IDbCommand dbCmd, object value, string[] include = null, CancellationToken token = default(CancellationToken))
        {
            var row = await dbCmd.SingleByIdAsync<T>(value, token);
            if (row == null)
                return default(T);

            await dbCmd.LoadReferencesAsync(row, include, token);

            return row;
        }

        public static async Task LoadReferencesAsync<T>(this IDbCommand dbCmd, T instance, string[] include = null, CancellationToken token = default(CancellationToken))
        {
            var loadRef = new LoadReferencesAsync<T>(dbCmd, instance);
            var fieldDefs = loadRef.FieldDefs;

            if (!include.IsEmpty())
            {
                // Check that any include values aren't reference fields of the specified type
                var fields = fieldDefs.Select(q => q.FieldName);
                var invalid = include.Except<string>(fields).ToList();
                if (invalid.Count > 0)
                    throw new ArgumentException("Fields '{0}' are not Reference Properties of Type '{1}'".Fmt(invalid.Join("', '"), typeof(T).Name));

                fieldDefs = fieldDefs.Where(fd => include.Contains(fd.FieldName)).ToList();
            }

            foreach (var fieldDef in fieldDefs)
            {
                dbCmd.Parameters.Clear();
                var listInterface = fieldDef.FieldType.GetTypeWithGenericInterfaceOf(typeof(IList<>));
                if (listInterface != null)
                {
                    await loadRef.SetRefFieldList(fieldDef, listInterface.GenericTypeArguments()[0], token);
                }
                else
                {
                    await loadRef.SetRefField(fieldDef, fieldDef.FieldType, token);
                }
            }
        }

        internal static async Task<List<Into>> LoadListWithReferences<Into, From>(this IDbCommand dbCmd, SqlExpression<From> expr = null, string[] include = null, CancellationToken token = default(CancellationToken))
        {
            var loadList = new LoadListAsync<Into, From>(dbCmd, expr);

            var fieldDefs = loadList.FieldDefs;
            if (!include.IsEmpty())
            {
                // Check that any include values aren't reference fields of the specified From type
                var fields = fieldDefs.Select(q => q.FieldName);
                var invalid = include.Except<string>(fields).ToList();
                if (invalid.Count > 0)
                    throw new ArgumentException("Fields '{0}' are not Reference Properties of Type '{1}'".Fmt(invalid.Join("', '"), typeof(From).Name));

                fieldDefs = loadList.FieldDefs.Where(fd => include.Contains(fd.FieldName)).ToList();
            }

            foreach (var fieldDef in fieldDefs)
            {
                var listInterface = fieldDef.FieldType.GetTypeWithGenericInterfaceOf(typeof(IList<>));
                if (listInterface != null)
                {
                    await loadList.SetRefFieldListAsync(fieldDef, listInterface.GenericTypeArguments()[0], token);
                }
                else
                {
                    await loadList.SetRefFieldAsync(fieldDef, fieldDef.FieldType, token);
                }
            }

            return loadList.ParentResults;
        }
    }
}
#endif
