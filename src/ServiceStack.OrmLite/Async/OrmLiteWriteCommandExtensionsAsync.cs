#if ASYNC
// Copyright (c) ServiceStack, Inc. All Rights Reserved.
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

        internal static Task<int> ExecuteSqlAsync(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams, CancellationToken token)
        {
            return dbCmd.SetParameters(sqlParams).ExecuteSqlAsync(sql, token);
        }

        internal static Task<int> ExecuteSqlAsync(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.ExecuteSql(dbCmd).InTask();

            if (Log.IsDebugEnabled)
                Log.DebugCommand(dbCmd);

            return dbCmd.GetDialectProvider().ExecuteNonQueryAsync(dbCmd, token);
        }

        internal static Task<int> ExecuteSqlAsync(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            if (anonType != null)
                dbCmd.SetParameters(anonType.ToObjectDictionary(), excludeDefaults: false);

            dbCmd.CommandText = sql;

            if (Log.IsDebugEnabled)
                Log.DebugCommand(dbCmd);

            if (OrmLiteConfig.ResultsFilter != null)
                return OrmLiteConfig.ResultsFilter.ExecuteSql(dbCmd).InTask();

            return dbCmd.GetDialectProvider().ExecuteNonQueryAsync(dbCmd, token);
        }

        internal static Task<int> UpdateAsync<T>(this IDbCommand dbCmd, T obj, CancellationToken token, Action<IDbCommand> commandFilter)
        {
            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, obj);

            var dialectProvider = dbCmd.GetDialectProvider();
            var hadRowVersion = dialectProvider.PrepareParameterizedUpdateStatement<T>(dbCmd);
            if (string.IsNullOrEmpty(dbCmd.CommandText))
                return TaskResult.Zero;

            dialectProvider.SetParameterValues<T>(dbCmd, obj);
            commandFilter?.Invoke(dbCmd);

            return dialectProvider.ExecuteNonQueryAsync(dbCmd, token)
                .Then(rowsUpdated =>
                {
                    if (hadRowVersion && rowsUpdated == 0)
                        throw new OptimisticConcurrencyException();
                    return rowsUpdated;
                });
        }

        internal static Task<int> UpdateAsync<T>(this IDbCommand dbCmd, CancellationToken token, Action<IDbCommand> commandFilter, T[] objs)
        {
            return dbCmd.UpdateAllAsync(objs, token, commandFilter);
        }

        internal static Task<int> UpdateAllAsync<T>(this IDbCommand dbCmd, IEnumerable<T> objs, CancellationToken token, Action<IDbCommand> commandFilter)
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
                OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, obj);

                dialectProvider.SetParameterValues<T>(dbCmd, obj);
                commandFilter?.Invoke(dbCmd);
                commandFilter = null;

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

                dbTrans?.Dispose();

                if (t.IsFaulted)
                    throw t.Exception;

                return count;
            }, token);
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

                dbTrans?.Dispose();

                return count;
            }, token);
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
            var sqlIn = dbCmd.SetIdsInSqlParams(idValues);
            if (string.IsNullOrEmpty(sqlIn))
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

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, string sql, object anonType, CancellationToken token)
        {
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeDefaults: false, sql: ref sql);
            return dbCmd.ExecuteSqlAsync(dbCmd.GetDialectProvider().ToDeleteStatement(typeof(T), sql), token);
        }

        internal static Task<int> DeleteAsync(this IDbCommand dbCmd, Type tableType, string sql, object anonType, CancellationToken token)
        {
            if (anonType != null) dbCmd.SetParameters(tableType, anonType, excludeDefaults: false, sql: ref sql);
            return dbCmd.ExecuteSqlAsync(dbCmd.GetDialectProvider().ToDeleteStatement(tableType, sql), token);
        }

        internal static Task<long> InsertAsync<T>(this IDbCommand dbCmd, T obj, bool selectIdentity, CancellationToken token)
        {
            OrmLiteConfig.InsertFilter?.Invoke(dbCmd, obj);

            var dialectProvider = dbCmd.GetDialectProvider();

            dialectProvider.PrepareParameterizedInsertStatement<T>(dbCmd,
                insertFields: OrmLiteUtils.GetNonDefaultValueInsertFields(obj));

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
                OrmLiteConfig.InsertFilter?.Invoke(dbCmd, obj);

                dialectProvider.SetParameterValues<T>(dbCmd, obj);

                return dbCmd.ExecNonQueryAsync(token);
            })
            .ContinueWith(t =>
            {
                if (dbTrans != null && t.IsSuccess())
                    dbTrans.Commit();

                dbTrans?.Dispose();

                if (t.IsFaulted)
                    throw t.Exception;
            }, token);
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
                    var safeId = dbCmd.GetDialectProvider().FromDbValue(newId, modelDef.PrimaryKey.FieldType);
                    modelDef.PrimaryKey.SetValueFn(obj, safeId);
                    id = newId;
                }
                else
                {
                    await dbCmd.InsertAsync(token, obj);
                }

                modelDef.RowVersion?.SetValueFn(obj, await dbCmd.GetRowVersionAsync(modelDef, id, token));

                return true;
            }

            await dbCmd.UpdateAsync(obj, token, null);

            modelDef.RowVersion?.SetValueFn(obj, await dbCmd.GetRowVersionAsync(modelDef, id, token));

            return false;
        }

        internal static async Task<int> SaveAllAsync<T>(this IDbCommand dbCmd, IEnumerable<T> objs, CancellationToken token)
        {
            var saveRows = objs.ToList();

            var firstRow = saveRows.FirstOrDefault();
            if (Equals(firstRow, default(T))) return 0;

            var modelDef = typeof(T).GetModelDefinition();

            var firstRowId = modelDef.GetPrimaryKey(firstRow);
            var defaultIdValue = firstRowId?.GetType().GetDefaultValue();

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
                        OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, row);

                        await dbCmd.UpdateAsync(row, token, null);
                    }
                    else
                    {
                        if (modelDef.HasAutoIncrementId)
                        {
                            var newId = await dbCmd.InsertAsync(row, selectIdentity: true, token:token);
                            var safeId = dialectProvider.FromDbValue(newId, modelDef.PrimaryKey.FieldType);
                            modelDef.PrimaryKey.SetValueFn(row, safeId);
                            id = newId;
                        }
                        else
                        {
                            OrmLiteConfig.InsertFilter?.Invoke(dbCmd, row);

                            await dbCmd.InsertAsync(token, row);
                        }

                        rowsAdded++;
                    }

                    modelDef.RowVersion?.SetValueFn(row, await dbCmd.GetRowVersionAsync(modelDef, id, token));
                }

                dbTrans?.Commit();
            }
            finally
            {
                dbTrans?.Dispose();
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
                    var refType = listInterface.GetGenericArguments()[0];
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
                        refField?.SetValueFn(result, pkValue);

                        await dbCmd.CreateTypedApi(refType).SaveAsync(result, token);

                        //Save Self Table.RefTableId PK
                        if (refSelf != null)
                        {
                            var refPkValue = refModelDef.PrimaryKey.GetValue(result);
                            refSelf.SetValueFn(instance, refPkValue);
                            await dbCmd.UpdateAsync(instance, token, null);
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

                refField?.SetValueFn(oRef, pkValue);
            }

            await dbCmd.SaveAllAsync(refs, token);

            foreach (var oRef in refs)
            {
                //Save Self Table.RefTableId PK
                if (refSelf != null)
                {
                    var refPkValue = refModelDef.PrimaryKey.GetValue(oRef);
                    refSelf.SetValueFn(instance, refPkValue);
                    await dbCmd.UpdateAsync(instance, token, null);
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

        internal static Task<object> GetRowVersionAsync(this IDbCommand dbCmd, ModelDefinition modelDef, object id, CancellationToken token)
        {
            var sql = dbCmd.RowVersionSql(modelDef, id);
            return dbCmd.ScalarAsync<object>(sql, token)
                .Success(x => dbCmd.GetDialectProvider().FromDbRowVersion(modelDef.RowVersion.FieldType, x));
        }
    }
}
#endif