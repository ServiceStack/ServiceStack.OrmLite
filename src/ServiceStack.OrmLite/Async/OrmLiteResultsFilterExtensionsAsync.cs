﻿#if ASYNC
// Copyright (c) ServiceStack, Inc. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteResultsFilterExtensionsAsync
    {
        internal static ILog Log = LogManager.GetLogger(typeof(OrmLiteResultsFilterExtensionsAsync));

        public static Task<int> ExecNonQueryAsync(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token = default(CancellationToken))
        {
            if (anonType != null)
                dbCmd.SetParameters(anonType.ToObjectDictionary(), (bool)false, sql:ref sql);

            dbCmd.CommandText = sql;

            OrmLiteConfig.BeforeExecFilter?.Invoke(dbCmd);

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.ExecuteSql(dbCmd).InTask();

            if (Log.IsDebugEnabled)
                Log.DebugCommand(dbCmd);

            return dbCmd.GetDialectProvider().ExecuteNonQueryAsync(dbCmd, token);
        }

        public static Task<int> ExecNonQueryAsync(this IDbCommand dbCmd, string sql, Dictionary<string, object> dict, CancellationToken token = default(CancellationToken))
        {
            if (dict != null)
                dbCmd.SetParameters(dict, (bool)false, sql:ref sql);

            dbCmd.CommandText = sql;

            OrmLiteConfig.BeforeExecFilter?.Invoke(dbCmd);

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.ExecuteSql(dbCmd).InTask();

            if (Log.IsDebugEnabled)
                Log.DebugCommand(dbCmd);

            return dbCmd.GetDialectProvider().ExecuteNonQueryAsync(dbCmd, token);
        }

        public static Task<int> ExecNonQueryAsync(this IDbCommand dbCmd, CancellationToken token = default(CancellationToken))
        {
            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.ExecuteSql(dbCmd).InTask();

            OrmLiteConfig.BeforeExecFilter?.Invoke(dbCmd);

            if (Log.IsDebugEnabled)
                Log.DebugCommand(dbCmd);

            return dbCmd.GetDialectProvider().ExecuteNonQueryAsync(dbCmd, token);
        }

        public static Task<List<T>> ConvertToListAsync<T>(this IDbCommand dbCmd)
        {
            return dbCmd.ConvertToListAsync<T>(null, default(CancellationToken));
        }

        public static async Task<List<T>> ConvertToListAsync<T>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetList<T>(dbCmd);

            var dialectProvider = dbCmd.GetDialectProvider();
            using (var reader = await dbCmd.ExecReaderAsync(dbCmd.CommandText, token))
            {
                if (OrmLiteUtils.IsScalar<T>())
                    return await reader.ColumnAsync<T>(dialectProvider, token);

                return await reader.ConvertToListAsync<T>(dialectProvider, null, token);
            }
        }

        public static Task<IList> ConvertToListAsync(this IDbCommand dbCmd, Type refType)
        {
            return dbCmd.ConvertToListAsync(refType, null, default(CancellationToken));
        }

        public static async Task<IList> ConvertToListAsync(this IDbCommand dbCmd, Type refType, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetRefList(dbCmd, refType);

            var dialectProvider = dbCmd.GetDialectProvider();
            using (var reader = await dbCmd.ExecReaderAsync(dbCmd.CommandText, token))
            {
                return await reader.ConvertToListAsync(dialectProvider, refType, token);
            }
        }

        internal static async Task<List<T>> ExprConvertToListAsync<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, HashSet<string> onlyFields, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            dbCmd.SetParameters(sqlParams);

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetList<T>(dbCmd);

            var dialectProvider = dbCmd.GetDialectProvider();
            using (var reader = await dbCmd.ExecReaderAsync(dbCmd.CommandText, token))
            {
                return await reader.ConvertToListAsync<T>(dialectProvider, onlyFields, token);
            }
        }

        public static Task<T> ConvertToAsync<T>(this IDbCommand dbCmd)
        {
            return dbCmd.ConvertToAsync<T>(null, default(CancellationToken));
        }

        public static async Task<T> ConvertToAsync<T>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetSingle<T>(dbCmd);

            var dialectProvider = dbCmd.GetDialectProvider();
            using (var reader = await dbCmd.ExecReaderAsync(dbCmd.CommandText, token))
            {
                return await reader.ConvertToAsync<T>(dialectProvider, token);
            }
        }

        internal static async Task<object> ConvertToAsync(this IDbCommand dbCmd, Type refType, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetRefSingle(dbCmd, refType);

            var dialectProvider = dbCmd.GetDialectProvider();
            using (var reader = await dbCmd.ExecReaderAsync(dbCmd.CommandText, token))
            {
                return await reader.ConvertToAsync(dialectProvider, refType, token);
            }           
        }

        public static Task<T> ScalarAsync<T>(this IDbCommand dbCmd)
        {
            return dbCmd.ScalarAsync<T>(null, default(CancellationToken));
        }

        public static Task<T> ScalarAsync<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            return dbCmd.SetParameters(sqlParams).ScalarAsync<T>(sql, token);
        }

        public static async Task<T> ScalarAsync<T>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetScalar<T>(dbCmd);

            var dialectProvider = dbCmd.GetDialectProvider();
            using (var reader = await dbCmd.ExecReaderAsync(dbCmd.CommandText, token))
            {
                return await reader.ScalarAsync<T>(dialectProvider, token);
            }
        }

        public static Task<object> ScalarAsync(this IDbCommand dbCmd)
        {
            return dbCmd.ScalarAsync((string)null, default(CancellationToken));
        }

        public static Task<object> ScalarAsync(this IDbCommand dbCmd, ISqlExpression expression, CancellationToken token)
        {
            dbCmd.PopulateWith(expression);

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetScalar(dbCmd).InTask();

            return dbCmd.GetDialectProvider().ExecuteScalarAsync(dbCmd, token);
        }

        public static Task<object> ScalarAsync(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetScalar(dbCmd).InTask();

            return dbCmd.GetDialectProvider().ExecuteScalarAsync(dbCmd, token);
        }

        public static Task<long> ExecLongScalarAsync(this IDbCommand dbCmd)
        {
            return dbCmd.ExecLongScalarAsync(null, default(CancellationToken));
        }

        public static Task<long> ExecLongScalarAsync(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetLongScalar(dbCmd).InTask();

            return dbCmd.LongScalarAsync(token);
        }

        internal static async Task<T> ExprConvertToAsync<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            dbCmd.SetParameters(sqlParams);

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetSingle<T>(dbCmd);

            var dialectProvider = dbCmd.GetDialectProvider();
            using (var reader = await dbCmd.ExecReaderAsync(dbCmd.CommandText, token))
            {
                return await reader.ConvertToAsync<T>(dialectProvider, token);
            }
        }

        internal static Task<List<T>> ColumnAsync<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            return dbCmd.SetParameters(sqlParams).ColumnAsync<T>(sql, token);
        }

        internal static async Task<List<T>> ColumnAsync<T>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetColumn<T>(dbCmd);

            var dialectProvider = dbCmd.GetDialectProvider();
            using (var reader = await dbCmd.ExecReaderAsync(dbCmd.CommandText, token))
            {
                return await reader.ColumnAsync<T>(dialectProvider, token);
            }
        }

        internal static Task<HashSet<T>> ColumnDistinctAsync<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            return dbCmd.SetParameters(sqlParams).ColumnDistinctAsync<T>(sql, token);
        }

        internal static async Task<HashSet<T>> ColumnDistinctAsync<T>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetColumnDistinct<T>(dbCmd);

            var dialectProvider = dbCmd.GetDialectProvider();
            using (var reader = await dbCmd.ExecReaderAsync(dbCmd.CommandText, token))
            {
                return await reader.ColumnDistinctAsync<T>(dialectProvider, token);
            }
        }

        internal static Task<Dictionary<K, V>> DictionaryAsync<K, V>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            return dbCmd.SetParameters(sqlParams).DictionaryAsync<K, V>(sql, token);
        }

        internal static async Task<Dictionary<K, V>> DictionaryAsync<K, V>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetDictionary<K, V>(dbCmd);

            var dialectProvider = dbCmd.GetDialectProvider();
            using (var reader = await dbCmd.ExecReaderAsync(dbCmd.CommandText, token))
            {
                return await reader.DictionaryAsync<K, V>(dialectProvider, token);
            }
        }

        internal static Task<List<KeyValuePair<K, V>>> KeyValuePairsAsync<K, V>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            return dbCmd.SetParameters(sqlParams).KeyValuePairsAsync<K, V>(sql, token);
        }

        internal static async Task<List<KeyValuePair<K, V>>> KeyValuePairsAsync<K, V>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetKeyValuePairs<K, V>(dbCmd);

            var dialectProvider = dbCmd.GetDialectProvider();
            using (var reader = await dbCmd.ExecReaderAsync(dbCmd.CommandText, token))
            {
                return await reader.KeyValuePairsAsync<K, V>(dialectProvider, token);
            }
        }

        internal static Task<Dictionary<K, List<V>>> LookupAsync<K, V>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            return dbCmd.SetParameters(sqlParams).LookupAsync<K, V>(sql, token);
        }

        internal static async Task<Dictionary<K, List<V>>> LookupAsync<K, V>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.GetLookup<K, V>(dbCmd);

            var dialectProvider = dbCmd.GetDialectProvider();
            using (var reader = await dbCmd.ExecReaderAsync(dbCmd.CommandText, token))
            {
                return await reader.LookupAsync<K, V>(dialectProvider, token);
            }
        }
    }
}
#endif