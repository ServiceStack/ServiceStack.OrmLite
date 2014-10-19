// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Data;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Async
{
    public static class OrmLiteWriteExtensionsAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OrmLiteWriteExtensionsAsync));

        private static void LogDebug(string fmt)
        {
            Log.Debug(fmt);
        }

        internal static Task<int> ExecuteSqlAsync(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            if (Log.IsDebugEnabled)
                LogDebug(sql);

            dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.ExecuteSql(dbCmd).InTask();

            return OrmLiteConfig.DialectProvider.ExecuteNonQueryAsync(dbCmd);
        }

        internal static Task<int> UpdateAsync<T>(this IDbCommand dbCmd, T obj, CancellationToken token)
        {
            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, obj);

            var hadRowVersion = OrmLiteConfig.DialectProvider.PrepareParameterizedUpdateStatement<T>(dbCmd);
            if (string.IsNullOrEmpty(dbCmd.CommandText))
                return TaskResult.Zero;

            OrmLiteConfig.DialectProvider.SetParameterValues<T>(dbCmd, obj);

            return OrmLiteConfig.DialectProvider.ExecuteNonQueryAsync(dbCmd)
                .Then(rowsUpdated =>
                {
                    if (hadRowVersion && rowsUpdated == 0)
                        throw new OptimisticConcurrencyException();
                    return rowsUpdated;
                });
        }

        internal static Task<int> UpdateAsync<T>(this IDbCommand dbCmd, CancellationToken token, params T[] objs)
        {
            return dbCmd.UpdateAllAsync(objs, token);
        }

        internal static Task<int> UpdateAllAsync<T>(this IDbCommand dbCmd, IEnumerable<T> objs, CancellationToken token)
        {
            IDbTransaction dbTrans = null;

            int count = 0;
            if (dbCmd.Transaction == null)
                dbCmd.Transaction = dbTrans = dbCmd.Connection.BeginTransaction();

            var dialectProvider = OrmLiteConfig.DialectProvider;

            var hadRowVersion = dialectProvider.PrepareParameterizedUpdateStatement<T>(dbCmd);
            if (string.IsNullOrEmpty(dbCmd.CommandText))
                return TaskResult.Zero;

            return objs.EachAsync((obj, i) =>
            {
                if (OrmLiteConfig.UpdateFilter != null)
                    OrmLiteConfig.UpdateFilter(dbCmd, obj);

                dialectProvider.SetParameterValues<T>(dbCmd, obj);

                return dbCmd.ExecNonQueryAsync(token).Then(rowsUpdated =>
                {
                    if (hadRowVersion && rowsUpdated == 0)
                        throw new OptimisticConcurrencyException();

                    count += rowsUpdated;
                    return count;
                });
            }).ContinueWith(t =>
            {
                if (dbTrans != null && t.IsSuccess())
                    dbTrans.Commit();

                if (dbTrans != null)
                    dbTrans.Dispose();

                return count;
            });
        }

        private static Task<int> AssertRowsUpdatedAsync(IDbCommand dbCmd, bool hadRowVersion, CancellationToken token)
        {
            return dbCmd.ExecNonQueryAsync(token).Then(rowsUpdated =>
            {
                if (hadRowVersion && rowsUpdated == 0)
                    throw new OptimisticConcurrencyException();

                return rowsUpdated;
            });
        }

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, object anonType, CancellationToken token)
        {
            var hadRowVersion = OrmLiteConfig.DialectProvider.PrepareParameterizedDeleteStatement<T>(dbCmd, anonType.AllFields<T>());

            OrmLiteConfig.DialectProvider.SetParameterValues<T>(dbCmd, anonType);

            return AssertRowsUpdatedAsync(dbCmd, hadRowVersion, token);
        }

        internal static Task<int> DeleteNonDefaultsAsync<T>(this IDbCommand dbCmd, T filter, CancellationToken token)
        {
            var hadRowVersion = OrmLiteConfig.DialectProvider.PrepareParameterizedDeleteStatement<T>(dbCmd, filter.NonDefaultFields<T>());

            OrmLiteConfig.DialectProvider.SetParameterValues<T>(dbCmd, filter);

            return AssertRowsUpdatedAsync(dbCmd, hadRowVersion, token);
        }

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, CancellationToken token, params object[] objs)
        {
            if (objs.Length == 0) 
                return TaskResult.Zero;

            return DeleteAllAsync<T>(dbCmd, objs[0].AllFields<T>(), objs, token);
        }

        internal static Task<int> DeleteNonDefaultsAsync<T>(this IDbCommand dbCmd, CancellationToken token, params T[] filters)
        {
            if (filters.Length == 0)
                return TaskResult.Zero;

            return DeleteAllAsync<T>(dbCmd, filters[0].NonDefaultFields<T>(), filters.Map(x => (object)x), token);
        }

        private static Task<int> DeleteAllAsync<T>(IDbCommand dbCmd, ICollection<string> deleteFields, IEnumerable<object> objs, CancellationToken token)
        {
            IDbTransaction dbTrans = null;

            int count = 0;
            if (dbCmd.Transaction == null)
                dbCmd.Transaction = dbTrans = dbCmd.Connection.BeginTransaction();

            var dialectProvider = OrmLiteConfig.DialectProvider;
            dialectProvider.PrepareParameterizedDeleteStatement<T>(dbCmd, deleteFields);

            return objs.EachAsync((obj, i) =>
            {
                dialectProvider.SetParameterValues<T>(dbCmd, obj);
                return dbCmd.ExecNonQueryAsync(token)
                    .Then(rowsAffected => count += rowsAffected);
            })
            .ContinueWith(t => {
                if (dbTrans != null && t.IsSuccess())
                    dbTrans.Commit();

                if (dbTrans != null)
                    dbTrans.Dispose();

                return count;
            });
        }

        internal static Task<int> DeleteByIdAsync<T>(this IDbCommand dbCmd, object id, CancellationToken token)
        {
            var sql = dbCmd.DeleteByIdSql<T>(id);
            return dbCmd.ExecuteSqlAsync(sql, token);
        }

        internal static Task DeleteByIdAsync<T>(this IDbCommand dbCmd, object id, ulong rowVersion, CancellationToken token)
        {
            var sql = dbCmd.DeleteByIdSql<T>(id, rowVersion);

            return dbCmd.ExecuteSqlAsync(sql, token).Then(rowsAffected => {
                if (rowsAffected == 0)
                    throw new OptimisticConcurrencyException("The row was modified or deleted since the last read");

                return TaskResult.Finished;
            });
        }

        internal static Task<int> DeleteByIdsAsync<T>(this IDbCommand dbCmd, IEnumerable idValues, CancellationToken token)
        {
            var sqlIn = idValues.GetIdsInSql();
            if (sqlIn == null) 
                return TaskResult.Zero;

            var sql = OrmLiteWriteExtensions.GetDeleteByIdsSql<T>(sqlIn);

            return dbCmd.ExecuteSqlAsync(sql, token);
        }

        internal static Task<int> DeleteAllAsync<T>(this IDbCommand dbCmd, CancellationToken token)
        {
            return DeleteAllAsync(dbCmd, typeof(T), token);
        }

        internal static Task<int> DeleteAllAsync(this IDbCommand dbCmd, Type tableType, CancellationToken token)
        {
            return dbCmd.ExecuteSqlAsync(OrmLiteConfig.DialectProvider.ToDeleteStatement(tableType, null), token);
        }

        internal static Task<int> DeleteFmtAsync<T>(this IDbCommand dbCmd, CancellationToken token, string sqlFilter, params object[] filterParams)
        {
            return DeleteFmtAsync(dbCmd, token, typeof(T), sqlFilter, filterParams);
        }

        internal static Task<int> DeleteFmtAsync(this IDbCommand dbCmd, CancellationToken token, Type tableType, string sqlFilter, params object[] filterParams)
        {
            return dbCmd.ExecuteSqlAsync(OrmLiteConfig.DialectProvider.ToDeleteStatement(tableType, sqlFilter, filterParams), token);
        }

        internal static Task<long> InsertAsync<T>(this IDbCommand dbCmd, T obj, bool selectIdentity, CancellationToken token)
        {
            if (OrmLiteConfig.InsertFilter != null)
                OrmLiteConfig.InsertFilter(dbCmd, obj);

            OrmLiteConfig.DialectProvider.PrepareParameterizedInsertStatement<T>(dbCmd);
            OrmLiteConfig.DialectProvider.SetParameterValues<T>(dbCmd, obj);

            if (selectIdentity)
                return OrmLiteConfig.DialectProvider.InsertAndGetLastInsertIdAsync<T>(dbCmd, token);

            return dbCmd.ExecNonQueryAsync(token).Then(i => (long)i);
        }

        internal static Task InsertAsync<T>(this IDbCommand dbCmd, CancellationToken token, params T[] objs)
        {
            return InsertAllAsync(dbCmd, objs, token);
        }

        internal static Task InsertAllAsync<T>(this IDbCommand dbCmd, IEnumerable<T> objs, CancellationToken token)
        {
            IDbTransaction dbTrans = null;

            if (dbCmd.Transaction == null)
                dbCmd.Transaction = dbTrans = dbCmd.Connection.BeginTransaction();

            var dialectProvider = OrmLiteConfig.DialectProvider;

            dialectProvider.PrepareParameterizedInsertStatement<T>(dbCmd);

            return objs.EachAsync((obj, i) =>
            {
                if (OrmLiteConfig.InsertFilter != null)
                    OrmLiteConfig.InsertFilter(dbCmd, obj);

                dialectProvider.SetParameterValues<T>(dbCmd, obj);

                return dbCmd.ExecNonQueryAsync(token);
            })
            .ContinueWith(t =>
            {
                if (dbTrans != null && t.IsSuccess())
                    dbTrans.Commit();

                if (dbTrans != null)
                    dbTrans.Dispose();
            });
        }

        // Procedures
        internal static Task ExecuteProcedureAsync<T>(this IDbCommand dbCommand, T obj, CancellationToken token)
        {
            string sql = OrmLiteConfig.DialectProvider.ToExecuteProcedureStatement(obj);
            dbCommand.CommandType = CommandType.StoredProcedure;
            return dbCommand.ExecuteSqlAsync(sql, token);
        }

        internal static Task<ulong> GetRowVersionAsync(this IDbCommand dbCmd, ModelDefinition modelDef, object id, CancellationToken token)
        {
            var sql = dbCmd.RowVersionSql(modelDef, id);
            return dbCmd.ScalarAsync<ulong>(sql, token);
        }
    }
}
