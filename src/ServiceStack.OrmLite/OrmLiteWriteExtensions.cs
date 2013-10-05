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
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteWriteExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OrmLiteWriteExtensions));
        private const string UseDbConnectionExtensions = "Use IDbConnection Extensions instead";

        [Conditional("DEBUG")]
        private static void LogDebug(string fmt, params object[] args)
        {
            if (args.Length > 0)
                Log.DebugFormat(fmt, args);
            else
                Log.Debug(fmt);
        }

        internal static void CreateTables(this IDbCommand dbCmd, bool overwrite, params Type[] tableTypes)
        {
            foreach (var tableType in tableTypes)
            {
                CreateTable(dbCmd, overwrite, tableType);
            }
        }

        internal static void CreateTable<T>(this IDbCommand dbCmd, bool overwrite = true)
        {
            var tableType = typeof(T);
            CreateTable(dbCmd, overwrite, tableType);
        }

        internal static void CreateTable(this IDbCommand dbCmd, bool overwrite, Type modelType)
        {
            var modelDef = modelType.GetModelDefinition();

			var dialectProvider = OrmLiteConfig.DialectProvider;
			var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef.ModelName);
			var tableExists = dialectProvider.DoesTableExist(dbCmd, tableName);
			if (overwrite && tableExists)
            {
                DropTable(dbCmd, modelDef);
            	tableExists = false;
            }

            try
            {
				if (!tableExists)
				{
					ExecuteSql(dbCmd, dialectProvider.ToCreateTableStatement(modelType));

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

                var dialectProvider = OrmLiteConfig.DialectProvider;
                var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef.ModelName);

                if (OrmLiteConfig.DialectProvider.DoesTableExist(dbCmd, tableName))
                {
                    var dropTableFks = OrmLiteConfig.DialectProvider.GetDropForeignKeyConstraints(modelDef);
                    if (!String.IsNullOrEmpty(dropTableFks))
                    {
                        dbCmd.ExecuteSql(dropTableFks);
                    }
                    dbCmd.ExecuteSql("DROP TABLE " + OrmLiteConfig.DialectProvider.GetQuotedTableName(modelDef));
                }
            }
            catch (Exception ex)
            {
                Log.DebugFormat("Could not drop table '{0}': {1}", modelDef.ModelName, ex.Message);
            }
        }

        internal static string LastSql(this IDbCommand dbCmd)
        {
            return dbCmd.CommandText;
        }

        internal static int ExecuteSql(this IDbCommand dbCmd, string sql)
        {
            LogDebug(sql);

            dbCmd.CommandText = sql;
            return dbCmd.ExecuteNonQuery();
        }

        private static bool IgnoreAlreadyExistsError(Exception ex)
        {
            //ignore Sqlite table already exists error
            const string sqliteAlreadyExistsError = "already exists";
            const string sqlServerAlreadyExistsError = "There is already an object named";
			 return ex.Message.Contains(sqliteAlreadyExistsError)
                   || ex.Message.Contains(sqlServerAlreadyExistsError)	;
        }
		
		private static bool IgnoreAlreadyExistsGeneratorError(Exception ex)
        {
            const string fbError = "attempt to store duplicate value";
            return ex.Message.Contains(fbError);
        }
		
        public static T PopulateWithSqlReader<T>(this T objWithProperties, IDataReader dataReader)
        {
            var fieldDefs = ModelDefinition<T>.Definition.AllFieldDefinitionsArray;

            return PopulateWithSqlReader(objWithProperties, dataReader, fieldDefs, null);
        }

        public static int GetColumnIndex(this IDataReader dataReader, string fieldName)
        {
            try
            {
                return dataReader.GetOrdinal(OrmLiteConfig.DialectProvider.NamingStrategy.GetColumnName(fieldName));
            }
            catch (IndexOutOfRangeException ignoreNotFoundExInSomeProviders)
            {
                return NotFound;
            }
        }

    	private const int NotFound = -1;
        public static T PopulateWithSqlReader<T>(this T objWithProperties, IDataReader dataReader, FieldDefinition[] fieldDefs, Dictionary<string, int> indexCache)
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
                            index = dataReader.GetColumnIndex(fieldDef.FieldName);
                            if (index == NotFound)
                            {
                                index = TryGuessColumnIndex(fieldDef.FieldName, dataReader);
                            }

                            indexCache.Add(fieldDef.Name, index);
                        }
                    }
                    else
                    {
                        index = dataReader.GetColumnIndex(fieldDef.FieldName);
                        if (index == NotFound)
                        {
                            index = TryGuessColumnIndex(fieldDef.FieldName, dataReader);
                        }
                    }
                       
					if (index == NotFound) continue;
					var value = dataReader.GetValue(index);
					fieldDef.SetValue(objWithProperties, value);
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
            var fieldCount = dataReader.FieldCount;
            for (var i = 0; i < fieldCount; i++)
            {
                var dbFieldName = dataReader.GetName(i);

                // First guess: Maybe the DB field has underscores? (most common)
                // e.g. CustomerId (C#) vs customer_id (DB)
                var dbFieldNameWithNoUnderscores = dbFieldName.Replace("_", "");
                if (String.Compare(fieldName, dbFieldNameWithNoUnderscores, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return i;
                }

                // Next guess: Maybe the DB field has special characters?
                // e.g. Quantity (C#) vs quantity% (DB)
                var dbFieldNameSanitized = AllowedPropertyCharsRegex.Replace(dbFieldName, String.Empty);
                if (String.Compare(fieldName, dbFieldNameSanitized, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return i;
                }

                // Next guess: Maybe the DB field has special characters *and* has underscores?
                // e.g. Quantity (C#) vs quantity_% (DB)
                if (String.Compare(fieldName, dbFieldNameSanitized.Replace("_", String.Empty), StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return i;
                }

                // Next guess: Maybe the DB field has some prefix that we don't have in our C# field?
                // e.g. CustomerId (C#) vs t130CustomerId (DB)
                if (dbFieldName.EndsWith(fieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }

                // Next guess: Maybe the DB field has some prefix that we don't have in our C# field *and* has underscores?
                // e.g. CustomerId (C#) vs t130_CustomerId (DB)
                if (dbFieldNameWithNoUnderscores.EndsWith(fieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }

                // Next guess: Maybe the DB field has some prefix that we don't have in our C# field *and* has special characters?
                // e.g. CustomerId (C#) vs t130#CustomerId (DB)
                if (dbFieldNameSanitized.EndsWith(fieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }

                // Next guess: Maybe the DB field has some prefix that we don't have in our C# field *and* has underscores *and* has special characters?
                // e.g. CustomerId (C#) vs t130#Customer_I#d (DB)
                if (dbFieldNameSanitized.Replace("_", "").EndsWith(fieldName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return i;
                }
            }

            return NotFound;
        }

        internal static void Update<T>(this IDbCommand dbCmd, T obj)
        {
            using (var updateStmt = dbCmd.CreateUpdateStatement(obj))
            {
                dbCmd.CopyParameterizedStatementTo(updateStmt);
            }
            dbCmd.ExecuteNonQuery();
        }

        internal static void UpdateAll<T>(this IDbCommand dbCmd, params T[] objs)
        {
            foreach (var obj in objs)
            {
                dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToUpdateRowStatement(obj));
            }
        }

        internal static void UpdateAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToUpdateRowStatement(obj));
			}
		}

        internal static IDbCommand CreateUpdateStatement<T>(this IDbCommand dbCmd, T obj)
        {
            return OrmLiteConfig.DialectProvider.CreateParameterizedUpdateStatement(dbCmd, obj);
        }

        internal static void CopyParameterizedStatementTo(this IDbCommand dbCmd, IDbCommand tmpStmt)
        {
            dbCmd.CommandText = tmpStmt.CommandText;

            //Instead of creating new generic DbParameters, copy them from the "dummy" IDbCommand, 
            //to keep provider specific information. E.g: SqlServer "datetime2" dbtype
            //We must first create a temp list, as DbParam can't belong to two DbCommands
            var tmpParams = new List<IDbDataParameter>(tmpStmt.Parameters.Count);
            tmpParams.AddRange(tmpStmt.Parameters.Cast<IDbDataParameter>());

            tmpStmt.Parameters.Clear();

            tmpParams.ForEach(x => dbCmd.Parameters.Add(x));
        }

        internal static void Delete<T>(this IDbCommand dbCmd, params T[] objs)
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteRowStatement(obj));
			}
		}

        internal static void DeleteAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteRowStatement(obj));
			}
		}

        internal static void DeleteByIds<T>(this IDbCommand dbCmd, IEnumerable idValues)
        {
            var sqlIn = idValues.GetIdsInSql();
            if (sqlIn == null) return;

            var modelDef = ModelDefinition<T>.Definition;

            var sql = String.Format("DELETE FROM {0} WHERE {1} IN ({2})",
                OrmLiteConfig.DialectProvider.GetQuotedTableName(modelDef),
                OrmLiteConfig.DialectProvider.GetQuotedColumnName(modelDef.PrimaryKey.FieldName),
                sqlIn);

            dbCmd.ExecuteSql(sql);
        }

        internal static void DeleteById<T>(this IDbCommand dbCmd, object id)
        {
            var modelDef = ModelDefinition<T>.Definition;
            var idParamString = OrmLiteConfig.DialectProvider.GetParam();

            var sql = String.Format("DELETE FROM {0} WHERE {1} = {2}",
                OrmLiteConfig.DialectProvider.GetQuotedTableName(modelDef),
                OrmLiteConfig.DialectProvider.GetQuotedColumnName(modelDef.PrimaryKey.FieldName),
                idParamString);

            var idParam = dbCmd.CreateParameter();
            idParam.ParameterName = idParamString;
            idParam.Value = id;
            dbCmd.Parameters.Add(idParam);
            
            dbCmd.ExecuteSql(sql);
        }

        internal static void DeleteAll<T>(this IDbCommand dbCmd)
        {
            DeleteAll(dbCmd, typeof(T));
        }

        internal static void DeleteAll(this IDbCommand dbCmd, Type tableType)
        {
			dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteStatement(tableType, null));
		}

        internal static void DeleteFmt<T>(this IDbCommand dbCmd, string sqlFilter, params object[] filterParams)
        {
            DeleteFmt(dbCmd, typeof(T), sqlFilter, filterParams);
        }

        internal static void DeleteFmt(this IDbCommand dbCmd, Type tableType, string sqlFilter, params object[] filterParams)
        {
            dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteStatement(tableType, sqlFilter, filterParams));
        }

        internal static void Save<T>(this IDbCommand dbCmd, T obj)
        {
            var id = obj.GetId();
            var existingRow = dbCmd.SingleById<T>(id);
            if (Equals(existingRow, default(T)))
            {
                dbCmd.InsertAll(obj);
            }
            else
            {
                dbCmd.UpdateAll(obj);
            }
        }

        internal static long Insert<T>(this IDbCommand dbCmd, T obj, bool selectIdentity = false)
        {
            using (var insertStmt = dbCmd.CreateInsertStatement(obj))
            {
                dbCmd.CopyParameterizedStatementTo(insertStmt);
            }

            if (selectIdentity)
                return OrmLiteConfig.DialectProvider.InsertAndGetLastInsertId<T>(dbCmd);

            dbCmd.ExecuteNonQuery();
            return -1;
        }

        internal static void InsertAll<T>(this IDbCommand dbCmd, params T[] objs)
        {
            foreach (var obj in objs)
            {
                dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToInsertRowStatement(dbCmd, obj));
            }
        }

        internal static void InsertAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToInsertRowStatement(dbCmd, obj));
			}
		}

        internal static IDbCommand CreateInsertStatement<T>(this IDbCommand command, T obj)
        {
            return OrmLiteConfig.DialectProvider.CreateParameterizedInsertStatement(command, obj);
        }

        internal static void Save<T>(this IDbCommand dbCmd, params T[] objs)
        {
            SaveAll(dbCmd, objs);
        }

        internal static void SaveAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
        {
            var saveRows = objs.ToList();

            var firstRow = saveRows.FirstOrDefault();
            if (Equals(firstRow, default(T))) return;

            var defaultIdValue = firstRow.GetId().GetType().GetDefaultValue();

            var idMap = defaultIdValue != null
                ? saveRows.Where(x => !defaultIdValue.Equals(x.GetId())).ToDictionary(x => x.GetId())
                : saveRows.Where(x => x.GetId() != null).ToDictionary(x => x.GetId());

            var existingRowsMap = dbCmd.SelectByIds<T>(idMap.Keys).ToDictionary(x => x.GetId());

            using (var dbTrans = dbCmd.Connection.BeginTransaction())
            {
                dbCmd.Transaction = dbTrans;

                foreach (var saveRow in saveRows)
                {
                    var id = saveRow.GetId();
                    if (id != defaultIdValue && existingRowsMap.ContainsKey(id))
                    {
                        dbCmd.UpdateAll(saveRow);
                    }
                    else
                    {
                        dbCmd.InsertAll(saveRow);
                    }
                }

                dbTrans.Commit();
            }
        }

        internal static IDbTransaction BeginTransaction(this IDbCommand dbCmd)
        {
            var dbTrans = dbCmd.Connection.BeginTransaction();
            dbCmd.ClearFilters();
            dbCmd.Transaction = dbTrans;
            return dbTrans;
        }

        internal static IDbTransaction BeginTransaction(this IDbCommand dbCmd, IsolationLevel isolationLevel)
        {
            var dbTrans = dbCmd.Connection.BeginTransaction(isolationLevel);
            dbCmd.ClearFilters();
            dbCmd.Transaction = dbTrans;
            return dbTrans;
        }
		
		// Procedures
        internal static void ExecuteProcedure<T>(this IDbCommand dbCommand, T obj)
        {
			string sql = OrmLiteConfig.DialectProvider.ToExecuteProcedureStatement(obj);
			dbCommand.CommandType= CommandType.StoredProcedure;
			dbCommand.ExecuteSql(sql);
		}
    }
}
