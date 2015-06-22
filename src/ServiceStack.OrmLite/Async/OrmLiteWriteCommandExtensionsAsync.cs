#if NET45
// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Data;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite
{
    internal static class OrmLiteWriteCommandExtensionsAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OrmLiteWriteCommandExtensionsAsync));

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

            return dbCmd.GetDialectProvider().ExecuteNonQueryAsync(dbCmd);
        }

        internal static Task<int> UpdateAsync<T>(this IDbCommand dbCmd, T obj, CancellationToken token)
        {
            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, obj);

            var dialectProvider = dbCmd.GetDialectProvider();
            var hadRowVersion = dialectProvider.PrepareParameterizedUpdateStatement<T>(dbCmd);
            if (string.IsNullOrEmpty(dbCmd.CommandText))
                return TaskResult.Zero;

            dialectProvider.SetParameterValues<T>(dbCmd, obj);

            return dialectProvider.ExecuteNonQueryAsync(dbCmd)
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

            var dialectProvider = dbCmd.GetDialectProvider();

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

                if (t.IsFaulted)
                    throw t.Exception;

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

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, T filter, CancellationToken token)
        {
            return dbCmd.DeleteAsync<T>((object)filter, token);
        }

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, object anonType, CancellationToken token)
        {
            var dialectProvider = dbCmd.GetDialectProvider();

            var hadRowVersion = dialectProvider.PrepareParameterizedDeleteStatement<T>(
                dbCmd, anonType.AllFieldsMap<T>());

            dialectProvider.SetParameterValues<T>(dbCmd, anonType);

            return AssertRowsUpdatedAsync(dbCmd, hadRowVersion, token);
        }

        internal static Task<int> DeleteNonDefaultsAsync<T>(this IDbCommand dbCmd, T filter, CancellationToken token)
        {
            var dialectProvider = dbCmd.GetDialectProvider();
            var hadRowVersion = dialectProvider.PrepareParameterizedDeleteStatement<T>(
                dbCmd, filter.AllFieldsMap<T>().NonDefaultsOnly());

            dialectProvider.SetParameterValues<T>(dbCmd, filter);

            return AssertRowsUpdatedAsync(dbCmd, hadRowVersion, token);
        }

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, CancellationToken token, params T[] objs)
        {
            if (objs.Length == 0) 
                return TaskResult.Zero;

            return DeleteAllAsync(dbCmd, objs, fieldValuesFn:null, token: token);
        }

        internal static Task<int> DeleteNonDefaultsAsync<T>(this IDbCommand dbCmd, CancellationToken token, params T[] filters)
        {
            if (filters.Length == 0)
                return TaskResult.Zero;

            return DeleteAllAsync(dbCmd, filters, o => o.AllFieldsMap<T>().NonDefaultsOnly(), token);
        }

        private static Task<int> DeleteAllAsync<T>(IDbCommand dbCmd, IEnumerable<T> objs, Func<object, Dictionary<string, object>> fieldValuesFn = null, CancellationToken token=default(CancellationToken))
        {
            IDbTransaction dbTrans = null;

            int count = 0;
            if (dbCmd.Transaction == null)
                dbCmd.Transaction = dbTrans = dbCmd.Connection.BeginTransaction();

            var dialectProvider = dbCmd.GetDialectProvider();

            return objs.EachAsync((obj, i) =>
            {
                var fieldValues = fieldValuesFn != null
                    ? fieldValuesFn(obj)
                    : obj.AllFieldsMap<T>();

                dialectProvider.PrepareParameterizedDeleteStatement<T>(dbCmd, fieldValues);

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

            var sql = OrmLiteWriteCommandExtensions.GetDeleteByIdsSql<T>(sqlIn, dbCmd.GetDialectProvider());

            return dbCmd.ExecuteSqlAsync(sql, token);
        }

        internal static Task<int> DeleteAllAsync<T>(this IDbCommand dbCmd, CancellationToken token)
        {
            return DeleteAllAsync(dbCmd, typeof(T), token);
        }

        internal static Task<int> DeleteAllAsync(this IDbCommand dbCmd, Type tableType, CancellationToken token)
        {
            var dialectProvider = dbCmd.GetDialectProvider();
            return dbCmd.ExecuteSqlAsync(dialectProvider.ToDeleteStatement(tableType, null), token);
        }

        internal static Task<int> DeleteFmtAsync<T>(this IDbCommand dbCmd, CancellationToken token, string sqlFilter, params object[] filterParams)
        {
            return DeleteFmtAsync(dbCmd, token, typeof(T), sqlFilter, filterParams);
        }

        internal static Task<int> DeleteFmtAsync(this IDbCommand dbCmd, CancellationToken token, Type tableType, string sqlFilter, params object[] filterParams)
        {
            var dialectProvider = dbCmd.GetDialectProvider();
            return dbCmd.ExecuteSqlAsync(dialectProvider.ToDeleteStatement(tableType, sqlFilter, filterParams), token);
        }

        internal static Task<long> InsertAsync<T>(this IDbCommand dbCmd, T obj, bool selectIdentity, CancellationToken token)
        {
            if (OrmLiteConfig.InsertFilter != null)
                OrmLiteConfig.InsertFilter(dbCmd, obj);

            var dialectProvider = dbCmd.GetDialectProvider();

            dialectProvider.PrepareParameterizedInsertStatement<T>(dbCmd);
            dialectProvider.SetParameterValues<T>(dbCmd, obj);

            if (selectIdentity)
                return dialectProvider.InsertAndGetLastInsertIdAsync<T>(dbCmd, token);

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

            var dialectProvider = dbCmd.GetDialectProvider();

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

                if (t.IsFaulted)
                    throw t.Exception;
            });
        }


        internal static Task<int> SaveAsync<T>(this IDbCommand dbCmd, CancellationToken token, params T[] objs)
        {
            return SaveAllAsync(dbCmd, objs, token);
        }

        internal static async Task<bool> SaveAsync<T>(this IDbCommand dbCmd, T obj, CancellationToken token)
        {
            var modelDef = typeof(T).GetModelDefinition();
            var id = modelDef.GetPrimaryKey(obj);
            var existingRow = id != null ? await dbCmd.SingleByIdAsync<T>(id, token) : default(T);

            if (Equals(existingRow, default(T)))
            {
                if (modelDef.HasAutoIncrementId)
                {

                    var newId = await dbCmd.InsertAsync(obj, selectIdentity: true, token:token);
                    var safeId = dbCmd.GetDialectProvider().ConvertDbValue(newId, modelDef.PrimaryKey.FieldType);
                    modelDef.PrimaryKey.SetValueFn(obj, safeId);
                    id = newId;
                }
                else
                {
                    await dbCmd.InsertAsync(token, obj);
                }

                if (modelDef.RowVersion != null)
                    modelDef.RowVersion.SetValueFn(obj, await dbCmd.GetRowVersionAsync(modelDef, id, token));

                return true;
            }

            await dbCmd.UpdateAsync(obj, token);

            if (modelDef.RowVersion != null)
                modelDef.RowVersion.SetValueFn(obj, await dbCmd.GetRowVersionAsync(modelDef, id, token));

            return false;
        }

        internal static async Task<int> SaveAllAsync<T>(this IDbCommand dbCmd, IEnumerable<T> objs, CancellationToken token)
        {
            var saveRows = objs.ToList();

            var firstRow = saveRows.FirstOrDefault();
            if (Equals(firstRow, default(T))) return 0;

            var modelDef = typeof(T).GetModelDefinition();

            var firstRowId = modelDef.GetPrimaryKey(firstRow);
            var defaultIdValue = firstRowId != null ? firstRowId.GetType().GetDefaultValue() : null;

            var idMap = defaultIdValue != null
                ? saveRows.Where(x => !defaultIdValue.Equals(modelDef.GetPrimaryKey(x))).ToSafeDictionary(x => modelDef.GetPrimaryKey(x))
                : saveRows.Where(x => modelDef.GetPrimaryKey(x) != null).ToSafeDictionary(x => modelDef.GetPrimaryKey(x));

            var existingRowsMap = (await dbCmd.SelectByIdsAsync<T>(idMap.Keys, token)).ToDictionary(x => modelDef.GetPrimaryKey(x));

            var rowsAdded = 0;

            IDbTransaction dbTrans = null;

            if (dbCmd.Transaction == null)
                dbCmd.Transaction = dbTrans = dbCmd.Connection.BeginTransaction();

            var dialectProvider = dbCmd.GetDialectProvider();

            try
            {
                foreach (var row in saveRows)
                {
                    var id = modelDef.GetPrimaryKey(row);
                    if (id != defaultIdValue && existingRowsMap.ContainsKey(id))
                    {
                        if (OrmLiteConfig.UpdateFilter != null)
                            OrmLiteConfig.UpdateFilter(dbCmd, row);

                        await dbCmd.UpdateAsync(row, token);
                    }
                    else
                    {
                        if (modelDef.HasAutoIncrementId)
                        {
                            var newId = await dbCmd.InsertAsync(row, selectIdentity: true, token:token);
                            var safeId = dialectProvider.ConvertDbValue(newId, modelDef.PrimaryKey.FieldType);
                            modelDef.PrimaryKey.SetValueFn(row, safeId);
                            id = newId;
                        }
                        else
                        {
                            if (OrmLiteConfig.InsertFilter != null)
                                OrmLiteConfig.InsertFilter(dbCmd, row);

                            await dbCmd.InsertAsync(token, row);
                        }

                        rowsAdded++;
                    }

                    if (modelDef.RowVersion != null)
                        modelDef.RowVersion.SetValueFn(row, await dbCmd.GetRowVersionAsync(modelDef, id, token));
                }

                if (dbTrans != null)
                    dbTrans.Commit();
            }
            finally
            {
                if (dbTrans != null)
                    dbTrans.Dispose();
            }

            return rowsAdded;
        }

        public static async Task SaveAllReferencesAsync<T>(this IDbCommand dbCmd, T instance, CancellationToken token)
        {
            var modelDef = ModelDefinition<T>.Definition;
            var pkValue = modelDef.PrimaryKey.GetValue(instance);

            var fieldDefs = modelDef.AllFieldDefinitionsArray.Where(x => x.IsReference);
            foreach (var fieldDef in fieldDefs)
            {
                var listInterface = fieldDef.FieldType.GetTypeWithGenericInterfaceOf(typeof(IList<>));
                if (listInterface != null)
                {
                    var refType = listInterface.GenericTypeArguments()[0];
                    var refModelDef = refType.GetModelDefinition();

                    var refField = modelDef.GetRefFieldDef(refModelDef, refType);

                    var results = (IEnumerable)fieldDef.GetValue(instance);
                    if (results != null)
                    {
                        foreach (var oRef in results)
                        {
                            refField.SetValueFn(oRef, pkValue);
                        }

                        await dbCmd.CreateTypedApi(refType).SaveAllAsync(results, token);
                    }
                }
                else
                {
                    var refType = fieldDef.FieldType;
                    var refModelDef = refType.GetModelDefinition();

                    var refSelf = modelDef.GetSelfRefFieldDefIfExists(refModelDef, fieldDef);

                    var result = fieldDef.GetValue(instance);
                    var refField = refSelf == null
                        ? modelDef.GetRefFieldDef(refModelDef, refType)
                        : modelDef.GetRefFieldDefIfExists(refModelDef);

                    if (result != null)
                    {
                        if (refField != null)
                            refField.SetValueFn(result, pkValue);

                        await dbCmd.CreateTypedApi(refType).SaveAsync(result, token);

                        //Save Self Table.RefTableId PK
                        if (refSelf != null)
                        {
                            var refPkValue = refModelDef.PrimaryKey.GetValue(result);
                            refSelf.SetValueFn(instance, refPkValue);
                            await dbCmd.UpdateAsync(instance, token);
                        }
                    }
                }
            }
        }

        public static async Task SaveReferencesAsync<T, TRef>(this IDbCommand dbCmd, CancellationToken token, T instance, params TRef[] refs)
        {
            var modelDef = ModelDefinition<T>.Definition;
            var pkValue = modelDef.PrimaryKey.GetValue(instance);

            var refType = typeof(TRef);
            var refModelDef = ModelDefinition<TRef>.Definition;

            var refSelf = modelDef.GetSelfRefFieldDefIfExists(refModelDef, null);

            foreach (var oRef in refs)
            {
                var refField = refSelf == null
                    ? modelDef.GetRefFieldDef(refModelDef, refType)
                    : modelDef.GetRefFieldDefIfExists(refModelDef);

                if (refField != null)
                    refField.SetValueFn(oRef, pkValue);
            }

            await dbCmd.SaveAllAsync(refs, token);

            foreach (var oRef in refs)
            {
                //Save Self Table.RefTableId PK
                if (refSelf != null)
                {
                    var refPkValue = refModelDef.PrimaryKey.GetValue(oRef);
                    refSelf.SetValueFn(instance, refPkValue);
                    await dbCmd.UpdateAsync(instance, token);
                }
            }
        }

        // Procedures
        internal static Task ExecuteProcedureAsync<T>(this IDbCommand dbCommand, T obj, CancellationToken token)
        {
            var dialectProvider = dbCommand.GetDialectProvider();
            string sql = dialectProvider.ToExecuteProcedureStatement(obj);
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
#endif