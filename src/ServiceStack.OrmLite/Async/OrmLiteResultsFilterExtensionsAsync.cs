using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite.Async
{
    public static class OrmLiteResultsFilterExtensionsAsync
    {
        internal static Task<int> ExecNonQueryAsync(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            if (anonType != null)
                dbCmd.SetParameters(anonType, (bool)false);

            dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.ExecuteSql(dbCmd));

            return OrmLiteConfig.DialectProvider.ExecuteNonQueryAsync(dbCmd, token);
        }

        internal static Task<int> ExecNonQueryAsync(this IDbCommand dbCmd, string sql, IDictionary<string, object> dict, CancellationToken token)
        {
            if (dict != null)
                dbCmd.SetParameters(dict, (bool)false);

            dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.ExecuteSql(dbCmd));

            return OrmLiteConfig.DialectProvider.ExecuteNonQueryAsync(dbCmd, token);
        }

        internal static Task<int> ExecNonQueryAsync(this IDbCommand dbCmd, CancellationToken token)
        {
            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.ExecuteSql(dbCmd));

            return OrmLiteConfig.DialectProvider.ExecuteNonQueryAsync(dbCmd, token);
        }

        public static Task<List<T>> ConvertToListAsync<T>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.GetList<T>(dbCmd));

            return dbCmd.ExecReaderAsync(dbCmd.CommandText, token)
                        .Then(reader => reader.ConvertToListAsync<T>(token)).Unwrap();
        }

        public static Task<IList> ConvertToListAsync(this IDbCommand dbCmd, Type refType, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.GetRefList(dbCmd, refType));

            return dbCmd.ExecReaderAsync(dbCmd.CommandText, token)
                        .Then(reader => reader.ConvertToListAsync(refType, token)).Unwrap();
        }

        internal static Task<List<T>> ExprConvertToListAsync<T>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.GetList<T>(dbCmd));

            return dbCmd.ExecReaderAsync(dbCmd.CommandText, token)
                        .Then(reader => reader.ExprConvertToListAsync<T>(token)).Unwrap();
        }

        public static Task<T> ConvertToAsync<T>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.GetSingle<T>(dbCmd));

            return dbCmd.ExecReaderAsync(dbCmd.CommandText, token)
                        .Then(reader => reader.ConvertToAsync<T>(token)).Unwrap();
        }

        internal static Task<object> ConvertToAsync(this IDbCommand dbCmd, Type refType, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.GetRefSingle(dbCmd, refType));

            return dbCmd.ExecReaderAsync(dbCmd.CommandText, token)
                        .Then(reader => reader.ConvertToAsync(refType, token)).Unwrap();
        }

        public static Task<T> ScalarAsync<T>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.GetScalar<T>(dbCmd));

            return dbCmd.ExecReaderAsync(dbCmd.CommandText, token)
                        .Then(reader => reader.ScalarAsync<T>(token)).Unwrap();
        }

        public static Task<object> ScalarAsync(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.GetScalar(dbCmd));

            return OrmLiteConfig.DialectProvider.ExecuteScalarAsync(dbCmd, token);
        }

        internal static Task<long> ExecLongScalarAsync(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.GetLongScalar(dbCmd));

            return dbCmd.LongScalarAsync(token);
        }

        internal static Task<T> ExprConvertToAsync<T>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.GetSingle<T>(dbCmd));

            return dbCmd.ExecReaderAsync(dbCmd.CommandText, token)
                        .Then(reader => reader.ExprConvertToAsync<T>(token)).Unwrap();
        }

        internal static Task<List<T>> ColumnAsync<T>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.GetColumn<T>(dbCmd));

            return dbCmd.ExecReaderAsync(dbCmd.CommandText, token)
                        .Then(reader => reader.ColumnAsync<T>(token)).Unwrap();
        }

        internal static Task<HashSet<T>> ColumnDistinctAsync<T>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.GetColumnDistinct<T>(dbCmd));

            return dbCmd.ExecReaderAsync(dbCmd.CommandText, token)
                        .Then(reader => reader.ColumnDistinctAsync<T>(token)).Unwrap();
        }

        internal static Task<Dictionary<K, V>> DictionaryAsync<K, V>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.GetDictionary<K, V>(dbCmd));

            return dbCmd.ExecReaderAsync(dbCmd.CommandText, token)
                         .Then(reader => reader.DictionaryAsync<K, V>(token)).Unwrap();
        }

        internal static Task<Dictionary<K, List<V>>> LookupAsync<K, V>(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return AsyncUtils.FromResult(OrmLiteConfig.ResultsFilter.GetLookup<K,V>(dbCmd));

            return dbCmd.ExecReaderAsync(dbCmd.CommandText, token)
                         .Then(reader => reader.LookupAsync<K,V>(token)).Unwrap();
        }
    }
}