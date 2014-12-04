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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ServiceStack.Data;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteWriteCommandExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OrmLiteWriteCommandExtensions));

        private static void LogDebug(string fmt)
        {
            Log.Debug(fmt);
        }

        internal static void CreateTables(this IDbCommand dbCmd, bool overwrite, params Type[] tableTypes)
        {
            foreach (var tableType in tableTypes)
            {
                CreateTable(dbCmd, overwrite, tableType);
            }
        }

        internal static void CreateTable<T>(this IDbCommand dbCmd, bool overwrite = false)
        {
            var tableType = typeof(T);
            CreateTable(dbCmd, overwrite, tableType);
        }

        internal static void CreateTable(this IDbCommand dbCmd, bool overwrite, Type modelType)
        {
            var modelDef = modelType.GetModelDefinition();

            var dialectProvider = dbCmd.GetDialectProvider();
            var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef);
            var tableExists = dialectProvider.DoesTableExist(dbCmd, tableName);
            if (overwrite && tableExists)
            {
                if (modelDef.PreDropTableSql != null)
                {
                    ExecuteSql(dbCmd, modelDef.PreDropTableSql);
                }

                DropTable(dbCmd, modelDef);

                var postDropTableSql = dialectProvider.ToPostDropTableStatement(modelDef);
                if (postDropTableSql != null)
                {
                    ExecuteSql(dbCmd, postDropTableSql);
                }

                if (modelDef.PostDropTableSql != null)
                {
                    ExecuteSql(dbCmd, modelDef.PostDropTableSql);
                }

                tableExists = false;
            }

            try
            {
                if (!tableExists)
                {
                    if (modelDef.PreCreateTableSql != null)
                    {
                        ExecuteSql(dbCmd, modelDef.PreCreateTableSql);
                    }

                    ExecuteSql(dbCmd, dialectProvider.ToCreateTableStatement(modelType));

                    var postCreateTableSql = dialectProvider.ToPostCreateTableStatement(modelDef);
                    if (postCreateTableSql != null)
                    {
                        ExecuteSql(dbCmd, postCreateTableSql);
                    }

                    if (modelDef.PostCreateTableSql != null)
                    {
                        ExecuteSql(dbCmd, modelDef.PostCreateTableSql);
                    }

                    var sqlIndexes = dialectProvider.ToCreateIndexStatements(modelType);
                    foreach (var sqlIndex in sqlIndexes)
                    {
                        try
                        {
                            dbCmd.ExecuteSql(sqlIndex);
                        }
                        catch (Exception exIndex)
                        {
                            if (IgnoreAlreadyExistsError(exIndex))
                            {
                                Log.DebugFormat("Ignoring existing index '{0}': {1}", sqlIndex, exIndex.Message);
                                continue;
                            }
                            throw;
                        }
                    }

                    var sequenceList = dialectProvider.SequenceList(modelType);
                    if (sequenceList.Count > 0)
                    {
                        foreach (var seq in sequenceList)
                        {
                            if (dialectProvider.DoesSequenceExist(dbCmd, seq) == false)
                            {
                                var seqSql = dialectProvider.ToCreateSequenceStatement(modelType, seq);
                                dbCmd.ExecuteSql(seqSql);
                            }
                        }
                    }
                    else
                    {
                        var sequences = dialectProvider.ToCreateSequenceStatements(modelType);
                        foreach (var seq in sequences)
                        {

                            try
                            {
                                dbCmd.ExecuteSql(seq);
                            }
                            catch (Exception ex)
                            {
                                if (IgnoreAlreadyExistsGeneratorError(ex))
                                {
                                    Log.DebugFormat("Ignoring existing generator '{0}': {1}", seq, ex.Message);
                                    continue;
                                }
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (IgnoreAlreadyExistsError(ex))
                {
                    Log.DebugFormat("Ignoring existing table '{0}': {1}", modelDef.ModelName, ex.Message);
                    return;
                }
                throw;
            }
        }

        internal static void DropTable<T>(this IDbCommand dbCmd)
        {
            DropTable(dbCmd, ModelDefinition<T>.Definition);
        }

        internal static void DropTable(this IDbCommand dbCmd, Type modelType)
        {
            DropTable(dbCmd, modelType.GetModelDefinition());
        }

        internal static void DropTables(this IDbCommand dbCmd, params Type[] tableTypes)
        {
            foreach (var modelDef in tableTypes.Select(type => type.GetModelDefinition()))
            {
                DropTable(dbCmd, modelDef);
            }
        }

        private static void DropTable(IDbCommand dbCmd, ModelDefinition modelDef)
        {
            try
            {
                var dialectProvider = dbCmd.GetDialectProvider();
                var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef);

                if (dialectProvider.DoesTableExist(dbCmd, tableName))
                {
                    if (modelDef.PreDropTableSql != null)
                    {
                        ExecuteSql(dbCmd, modelDef.PreDropTableSql);
                    }

                    var dropTableFks = dialectProvider.GetDropForeignKeyConstraints(modelDef);
                    if (!string.IsNullOrEmpty(dropTableFks))
                    {
                        dbCmd.ExecuteSql(dropTableFks);
                    }
                    dbCmd.ExecuteSql("DROP TABLE " + dialectProvider.GetQuotedTableName(modelDef));

                    if (modelDef.PostDropTableSql != null)
                    {
                        ExecuteSql(dbCmd, modelDef.PostDropTableSql);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.DebugFormat("Could not drop table '{0}': {1}", modelDef.ModelName, ex.Message);
                throw;
            }
        }

        internal static string LastSql(this IDbCommand dbCmd)
        {
            return dbCmd.CommandText;
        }

        internal static int ExecuteSql(this IDbCommand dbCmd, string sql)
        {
            if (Log.IsDebugEnabled)
                LogDebug(sql);

            dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.ExecuteSql(dbCmd);
            }

            return dbCmd.ExecuteNonQuery();
        }

        private static bool IgnoreAlreadyExistsError(Exception ex)
        {
            //ignore Sqlite table already exists error
            const string sqliteAlreadyExistsError = "already exists";
            const string sqlServerAlreadyExistsError = "There is already an object named";
            return ex.Message.Contains(sqliteAlreadyExistsError)
                  || ex.Message.Contains(sqlServerAlreadyExistsError);
        }

        private static bool IgnoreAlreadyExistsGeneratorError(Exception ex)
        {
            const string fbError = "attempt to store duplicate value";
            return ex.Message.Contains(fbError);
        }

        public static T PopulateWithSqlReader<T>(this T objWithProperties, IOrmLiteDialectProvider dialectProvider, IDataReader dataReader)
        {
            var fieldDefs = ModelDefinition<T>.Definition.AllFieldDefinitionsArray;

            return PopulateWithSqlReader(objWithProperties, dialectProvider, dataReader, fieldDefs, null);
        }

        public static int GetColumnIndex(this IDataReader dataReader, IOrmLiteDialectProvider dialectProvider, string fieldName)
        {
            try
            {
                return dataReader.GetOrdinal(dialectProvider.NamingStrategy.GetColumnName(fieldName));
            }
            catch (IndexOutOfRangeException ignoreNotFoundExInSomeProviders)
            {
                return NotFound;
            }
        }

        internal static int FindColumnIndex(this IDataReader dataReader, IOrmLiteDialectProvider dialectProvider, FieldDefinition fieldDef)
        {
            var index = NotFound;
            index = dataReader.GetColumnIndex(dialectProvider, fieldDef.FieldName);
            if (index == NotFound)
            {
                index = TryGuessColumnIndex(fieldDef.FieldName, dataReader);
            }
            // Try fallback to original field name when overriden by alias
            if (index == NotFound && fieldDef.Alias != null && !OrmLiteConfig.DisableColumnGuessFallback)
            {
                index = dataReader.GetColumnIndex(dialectProvider, fieldDef.Name);
                if (index == NotFound)
                {
                    index = TryGuessColumnIndex(fieldDef.Name, dataReader);
                }
            }

            return index;
        }

        private const int NotFound = -1;
        public static T PopulateWithSqlReader<T>(this T objWithProperties, IOrmLiteDialectProvider dialectProvider, IDataReader dataReader, FieldDefinition[] fieldDefs, Dictionary<string, int> indexCache)
        {
            try
            {
                foreach (var fieldDef in fieldDefs)
                {
                    int index;
                    if (indexCache != null)
                    {
                        if (!indexCache.TryGetValue(fieldDef.Name, out index))
                        {
                            index = FindColumnIndex(dataReader, dialectProvider, fieldDef);

                            indexCache.Add(fieldDef.Name, index);
                        }
                    }
                    else
                    {
                        index = FindColumnIndex(dataReader, dialectProvider, fieldDef);
                    }

                    dialectProvider.SetDbValue(fieldDef, dataReader, index, objWithProperties);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return objWithProperties;
        }

        private static readonly Regex AllowedPropertyCharsRegex = new Regex(@"[^0-9a-zA-Z_]",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static int TryGuessColumnIndex(string fieldName, IDataReader dataReader)
        {
            if (OrmLiteConfig.DisableColumnGuessFallback)
                return NotFound;

            var fieldCount = dataReader.FieldCount;
            for (var i = 0; i < fieldCount; i++)
            {
                var dbFieldName = dataReader.GetName(i);

                // First guess: Maybe the DB field has underscores? (most common)
                // e.g. CustomerId (C#) vs customer_id (DB)
                var dbFieldNameWithNoUnderscores = dbFieldName.Replace("_", "");
                if (string.Compare(fieldName, dbFieldNameWithNoUnderscores, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return i;
                }

                // Next guess: Maybe the DB field has special characters?
                // e.g. Quantity (C#) vs quantity% (DB)
                var dbFieldNameSanitized = AllowedPropertyCharsRegex.Replace(dbFieldName, string.Empty);
                if (string.Compare(fieldName, dbFieldNameSanitized, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return i;
                }

                // Next guess: Maybe the DB field has special characters *and* has underscores?
                // e.g. Quantity (C#) vs quantity_% (DB)
                if (string.Compare(fieldName, dbFieldNameSanitized.Replace("_", string.Empty), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return i;
                }

                // Next guess: Maybe the DB field has some prefix that we don't have in our C# field?
                // e.g. CustomerId (C#) vs t130CustomerId (DB)
                if (dbFieldName.EndsWith(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }

                // Next guess: Maybe the DB field has some prefix that we don't have in our C# field *and* has underscores?
                // e.g. CustomerId (C#) vs t130_CustomerId (DB)
                if (dbFieldNameWithNoUnderscores.EndsWith(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }

                // Next guess: Maybe the DB field has some prefix that we don't have in our C# field *and* has special characters?
                // e.g. CustomerId (C#) vs t130#CustomerId (DB)
                if (dbFieldNameSanitized.EndsWith(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }

                // Next guess: Maybe the DB field has some prefix that we don't have in our C# field *and* has underscores *and* has special characters?
                // e.g. CustomerId (C#) vs t130#Customer_I#d (DB)
                if (dbFieldNameSanitized.Replace("_", "").EndsWith(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }

                // Cater for Naming Strategies like PostgreSQL that has lower_underscore names
                if (dbFieldNameSanitized.Replace("_", "").EndsWith(fieldName.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return NotFound;
        }

        internal static int Update<T>(this IDbCommand dbCmd, T obj)
        {
            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, obj);

            var dialectProvider = dbCmd.GetDialectProvider();
            var hadRowVersion = dialectProvider.PrepareParameterizedUpdateStatement<T>(dbCmd);
            if (string.IsNullOrEmpty(dbCmd.CommandText))
                return 0;

            dialectProvider.SetParameterValues<T>(dbCmd, obj);

            var rowsUpdated = dbCmd.ExecNonQuery();

            if (hadRowVersion && rowsUpdated == 0)
                throw new OptimisticConcurrencyException();

            return rowsUpdated;
        }

        internal static int Update<T>(this IDbCommand dbCmd, params T[] objs)
        {
            return dbCmd.UpdateAll(objs);
        }

        internal static int UpdateAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
        {
            IDbTransaction dbTrans = null;

            int count = 0;
            try
            {
                if (dbCmd.Transaction == null)
                    dbCmd.Transaction = dbTrans = dbCmd.Connection.BeginTransaction();

                var dialectProvider = dbCmd.GetDialectProvider();

                var hadRowVersion = dialectProvider.PrepareParameterizedUpdateStatement<T>(dbCmd);
                if (string.IsNullOrEmpty(dbCmd.CommandText))
                    return 0;

                foreach (var obj in objs)
                {
                    if (OrmLiteConfig.UpdateFilter != null)
                        OrmLiteConfig.UpdateFilter(dbCmd, obj);

                    dialectProvider.SetParameterValues<T>(dbCmd, obj);

                    var rowsUpdated = dbCmd.ExecNonQuery();
                    if (hadRowVersion && rowsUpdated == 0) 
                        throw new OptimisticConcurrencyException();

                    count += rowsUpdated;                
                }

                if (dbTrans != null)
                    dbTrans.Commit();
            }
            finally
            {
                if (dbTrans != null)
                    dbTrans.Dispose();
            }

            return count;
        }

        private static int AssertRowsUpdated(IDbCommand dbCmd, bool hadRowVersion)
        {
            var rowsUpdated = dbCmd.ExecNonQuery();
            if (hadRowVersion && rowsUpdated == 0)
                throw new OptimisticConcurrencyException();

            return rowsUpdated;
        }

        internal static int Delete<T>(this IDbCommand dbCmd, object anonType)
        {
            var dialectProvider = dbCmd.GetDialectProvider();
            var hadRowVersion = dialectProvider.PrepareParameterizedDeleteStatement<T>(dbCmd, anonType.AllFields<T>());

            dialectProvider.SetParameterValues<T>(dbCmd, anonType);

            return AssertRowsUpdated(dbCmd, hadRowVersion);
        }

        internal static int DeleteNonDefaults<T>(this IDbCommand dbCmd, T filter)
        {
            var dialectProvider = dbCmd.GetDialectProvider();
            var hadRowVersion = dialectProvider.PrepareParameterizedDeleteStatement<T>(dbCmd, filter.NonDefaultFields<T>());

            dialectProvider.SetParameterValues<T>(dbCmd, filter);

            return AssertRowsUpdated(dbCmd, hadRowVersion);
        }

        internal static int Delete<T>(this IDbCommand dbCmd, params object[] objs)
        {
            if (objs.Length == 0) return 0;

            return DeleteAll<T>(dbCmd, objs[0].AllFields<T>(), objs);
        }

        internal static int DeleteNonDefaults<T>(this IDbCommand dbCmd, params T[] filters)
        {
            if (filters.Length == 0) return 0;

            return DeleteAll<T>(dbCmd, filters[0].NonDefaultFields<T>(), filters.Map(x => (object)x));
        }

        private static int DeleteAll<T>(IDbCommand dbCmd, ICollection<string> deleteFields, IEnumerable<object> objs)
        {
            IDbTransaction dbTrans = null;

            int count = 0;
            try
            {
                if (dbCmd.Transaction == null)
                    dbCmd.Transaction = dbTrans = dbCmd.Connection.BeginTransaction();

                var dialectProvider = dbCmd.GetDialectProvider();

                dialectProvider.PrepareParameterizedDeleteStatement<T>(dbCmd, deleteFields);

                foreach (var obj in objs)
                {
                    dialectProvider.SetParameterValues<T>(dbCmd, obj);

                    count += dbCmd.ExecNonQuery();
                }

                if (dbTrans != null)
                    dbTrans.Commit();
            }
            finally
            {
                if (dbTrans != null)
                    dbTrans.Dispose();
            }

            return count;
        }

        internal static int DeleteById<T>(this IDbCommand dbCmd, object id)
        {
            var sql = DeleteByIdSql<T>(dbCmd, id);

            return dbCmd.ExecuteSql(sql);
        }

        internal static string DeleteByIdSql<T>(this IDbCommand dbCmd, object id)
        {
            var modelDef = ModelDefinition<T>.Definition;
            var dialectProvider = dbCmd.GetDialectProvider();
            var idParamString = dialectProvider.GetParam();

            var sql = string.Format("DELETE FROM {0} WHERE {1} = {2}",
                dialectProvider.GetQuotedTableName(modelDef),
                dialectProvider.GetQuotedColumnName(modelDef.PrimaryKey.FieldName),
                idParamString);

            var idParam = dbCmd.CreateParameter();
            idParam.ParameterName = idParamString;
            idParam.Value = id;
            dbCmd.Parameters.Add(idParam);
            return sql;
        }

        internal static void DeleteById<T>(this IDbCommand dbCmd, object id, ulong rowVersion)
        {
            var sql = DeleteByIdSql<T>(dbCmd, id, rowVersion);

            var rowsAffected = dbCmd.ExecuteSql(sql);
            if (rowsAffected == 0)
                throw new OptimisticConcurrencyException("The row was modified or deleted since the last read");
        }

        internal static string DeleteByIdSql<T>(this IDbCommand dbCmd, object id, ulong rowVersion)
        {
            var modelDef = ModelDefinition<T>.Definition;
            var dialectProvider = dbCmd.GetDialectProvider();

            dbCmd.Parameters.Clear();

            var idParam = dbCmd.CreateParameter();
            idParam.ParameterName = dialectProvider.GetParam();
            idParam.Value = id;
            dbCmd.Parameters.Add(idParam);

            var rowVersionField = modelDef.RowVersion;
            if (rowVersionField == null)
                throw new InvalidOperationException(
                    "Cannot use DeleteById with rowVersion for model type without a row version column");

            var rowVersionParam = dbCmd.CreateParameter();
            rowVersionParam.ParameterName = dialectProvider.GetParam("rowVersion");
            rowVersionParam.Value = rowVersion;
            dbCmd.Parameters.Add(rowVersionParam);

            var sql = string.Format("DELETE FROM {0} WHERE {1} = {2} AND {3} = {4}",
                dialectProvider.GetQuotedTableName(modelDef),
                dialectProvider.GetQuotedColumnName(modelDef.PrimaryKey.FieldName),
                idParam.ParameterName,
                dialectProvider.GetQuotedColumnName(rowVersionField.FieldName),
                rowVersionParam.ParameterName);

            return sql;
        }

        internal static int DeleteByIds<T>(this IDbCommand dbCmd, IEnumerable idValues)
        {
            var sqlIn = idValues.GetIdsInSql();
            if (sqlIn == null) return 0;

            var sql = GetDeleteByIdsSql<T>(sqlIn, dbCmd.GetDialectProvider());

            return dbCmd.ExecuteSql(sql);
        }

        internal static string GetDeleteByIdsSql<T>(string sqlIn, IOrmLiteDialectProvider dialectProvider)
        {
            var modelDef = ModelDefinition<T>.Definition;

            var sql = string.Format("DELETE FROM {0} WHERE {1} IN ({2})",
                dialectProvider.GetQuotedTableName(modelDef),
                dialectProvider.GetQuotedColumnName(modelDef.PrimaryKey.FieldName),
                sqlIn);
            return sql;
        }

        internal static int DeleteAll<T>(this IDbCommand dbCmd)
        {
            return DeleteAll(dbCmd, typeof(T));
        }

        internal static int DeleteAll(this IDbCommand dbCmd, Type tableType)
        {
            return dbCmd.ExecuteSql(dbCmd.GetDialectProvider().ToDeleteStatement(tableType, null));
        }

        internal static int DeleteFmt<T>(this IDbCommand dbCmd, string sqlFilter, params object[] filterParams)
        {
            return DeleteFmt(dbCmd, typeof(T), sqlFilter, filterParams);
        }

        internal static int DeleteFmt(this IDbCommand dbCmd, Type tableType, string sqlFilter, params object[] filterParams)
        {
            return dbCmd.ExecuteSql(dbCmd.GetDialectProvider().ToDeleteStatement(tableType, sqlFilter, filterParams));
        }

        internal static long Insert<T>(this IDbCommand dbCmd, T obj, bool selectIdentity = false)
        {
            if (OrmLiteConfig.InsertFilter != null)
                OrmLiteConfig.InsertFilter(dbCmd, obj);

            var dialectProvider = dbCmd.GetDialectProvider();
            dialectProvider.PrepareParameterizedInsertStatement<T>(dbCmd);

            dialectProvider.SetParameterValues<T>(dbCmd, obj);

            if (selectIdentity)
                return dialectProvider.InsertAndGetLastInsertId<T>(dbCmd);

            return dbCmd.ExecNonQuery();
        }

        internal static void Insert<T>(this IDbCommand dbCmd, params T[] objs)
        {
            InsertAll(dbCmd, objs);
        }

        internal static void InsertAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
        {
            IDbTransaction dbTrans = null;

            try
            {
                if (dbCmd.Transaction == null)
                    dbCmd.Transaction = dbTrans = dbCmd.Connection.BeginTransaction();

                var dialectProvider = dbCmd.GetDialectProvider();

                dialectProvider.PrepareParameterizedInsertStatement<T>(dbCmd);

                foreach (var obj in objs)
                {
                    if (OrmLiteConfig.InsertFilter != null)
                        OrmLiteConfig.InsertFilter(dbCmd, obj);

                    dialectProvider.SetParameterValues<T>(dbCmd, obj);

                    dbCmd.ExecNonQuery();
                }

                if (dbTrans != null)
                    dbTrans.Commit();
            }
            finally
            {
                if (dbTrans != null)
                    dbTrans.Dispose();
            }
        }

        internal static int Save<T>(this IDbCommand dbCmd, params T[] objs)
        {
            return SaveAll(dbCmd, objs);
        }

        internal static bool Save<T>(this IDbCommand dbCmd, T obj)
        {
            var id = obj.GetId();
            var existingRow = id != null ? dbCmd.SingleById<T>(id) : default(T);
            var modelDef = typeof(T).GetModelDefinition();

            if (Equals(existingRow, default(T)))
            {
                if (modelDef.HasAutoIncrementId)
                {
                    var dialectProvider = dbCmd.GetDialectProvider();
                    var newId = dbCmd.Insert(obj, selectIdentity: true);
                    var safeId = dialectProvider.ConvertDbValue(newId, modelDef.PrimaryKey.FieldType);
                    modelDef.PrimaryKey.SetValueFn(obj, safeId);
                    id = newId;
                }
                else
                {
                    dbCmd.Insert(obj);
                }

                if (modelDef.RowVersion != null)
                    modelDef.RowVersion.SetValueFn(obj, dbCmd.GetRowVersion(modelDef, id));
                
                return true;
            }

            dbCmd.Update(obj);
            
            if (modelDef.RowVersion != null)
                modelDef.RowVersion.SetValueFn(obj, dbCmd.GetRowVersion(modelDef, id));

            return false;
        }

        internal static int SaveAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
        {
            var saveRows = objs.ToList();

            var firstRow = saveRows.FirstOrDefault();
            if (Equals(firstRow, default(T))) return 0;

            var firstRowId = firstRow.GetId();
            var defaultIdValue = firstRowId != null ? firstRowId.GetType().GetDefaultValue() : null;

            var idMap = defaultIdValue != null
                ? saveRows.Where(x => !defaultIdValue.Equals(x.GetId())).ToSafeDictionary(x => x.GetId())
                : saveRows.Where(x => x.GetId() != null).ToSafeDictionary(x => x.GetId());

            var existingRowsMap = dbCmd.SelectByIds<T>(idMap.Keys).ToDictionary(x => x.GetId());

            var modelDef = typeof(T).GetModelDefinition();

            var rowsAdded = 0;

            IDbTransaction dbTrans = null;

            if (dbCmd.Transaction == null)
                dbCmd.Transaction = dbTrans = dbCmd.Connection.BeginTransaction();

            try
            {
                foreach (var row in saveRows)
                {
                    var id = row.GetId();
                    if (id != defaultIdValue && existingRowsMap.ContainsKey(id))
                    {
                        if (OrmLiteConfig.UpdateFilter != null)
                            OrmLiteConfig.UpdateFilter(dbCmd, row);

                        dbCmd.Update(row);
                    }
                    else
                    {
                        if (modelDef.HasAutoIncrementId)
                        {
                            var dialectProvider = dbCmd.GetDialectProvider();
                            var newId = dbCmd.Insert(row, selectIdentity: true);
                            var safeId = dialectProvider.ConvertDbValue(newId, modelDef.PrimaryKey.FieldType);
                            modelDef.PrimaryKey.SetValueFn(row, safeId);
                            id = newId;
                        }
                        else
                        {
                            if (OrmLiteConfig.InsertFilter != null)
                                OrmLiteConfig.InsertFilter(dbCmd, row);

                            dbCmd.Insert(row);
                        }

                        rowsAdded++;
                    }

                    if (modelDef.RowVersion != null)
                        modelDef.RowVersion.SetValueFn(row, dbCmd.GetRowVersion(modelDef, id));
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

        internal static void SaveAllReferences<T>(this IDbCommand dbCmd, T instance)
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

                        dbCmd.CreateTypedApi(refType).SaveAll(results);
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

                        dbCmd.CreateTypedApi(refType).Save(result);

                        //Save Self Table.RefTableId PK
                        if (refSelf != null)
                        {
                            var refPkValue = refModelDef.PrimaryKey.GetValue(result);
                            refSelf.SetValueFn(instance, refPkValue);
                            dbCmd.Update(instance);
                        }
                    }
                }
            }
        }

        internal static void SaveReferences<T, TRef>(this IDbCommand dbCmd, T instance, params TRef[] refs)
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

            dbCmd.SaveAll(refs);

            foreach (var oRef in refs)
            {
                //Save Self Table.RefTableId PK
                if (refSelf != null)
                {
                    var refPkValue = refModelDef.PrimaryKey.GetValue(oRef);
                    refSelf.SetValueFn(instance, refPkValue);
                    dbCmd.Update(instance);
                }
            }
        }

        // Procedures
        internal static void ExecuteProcedure<T>(this IDbCommand dbCmd, T obj)
        {
            var dialectProvider = dbCmd.GetDialectProvider();
            dialectProvider.PrepareStoredProcedureStatement(dbCmd, obj);
            dbCmd.ExecuteNonQuery();
        }

        internal static ulong GetRowVersion(this IDbCommand dbCmd, ModelDefinition modelDef, object id)
        {
            var sql = RowVersionSql(dbCmd, modelDef, id);
            return dbCmd.Scalar<ulong>(sql);
        }

        internal static string RowVersionSql(this IDbCommand dbCmd, ModelDefinition modelDef, object id)
        {
            var dialectProvider = dbCmd.GetDialectProvider();
            var idParamString = dialectProvider.GetParam();

            var sql = string.Format("SELECT {0} FROM {1} WHERE {2} = {3}",
                dialectProvider.GetRowVersionColumnName(modelDef.RowVersion),
                dialectProvider.GetQuotedTableName(modelDef),
                dialectProvider.GetQuotedColumnName(modelDef.PrimaryKey.FieldName),
                idParamString);

            dbCmd.Parameters.Clear();
            var idParam = dbCmd.CreateParameter();
            idParam.ParameterName = idParamString;
            idParam.Value = id;
            dbCmd.Parameters.Add(idParam);
            return sql;
        }
    }
}
