//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2010 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using ServiceStack.Common.Utils;
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

        [Obsolete(UseDbConnectionExtensions)]
        public static void CreateTables(this IDbCommand dbCmd, bool overwrite, params Type[] tableTypes)
        {
            foreach (var tableType in tableTypes)
            {
                CreateTable(dbCmd, overwrite, tableType);
            }
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void CreateTable<T>(this IDbCommand dbCmd)
            where T : new()
        {
            var tableType = typeof(T);
            CreateTable(dbCmd, false, tableType);
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void CreateTable<T>(this IDbCommand dbCmd, bool overwrite)
            where T : new()
        {
            var tableType = typeof(T);
            CreateTable(dbCmd, overwrite, tableType);
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void CreateTable(this IDbCommand dbCmd, bool overwrite, Type modelType)
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

        [Obsolete(UseDbConnectionExtensions)]
        public static void DropTable<T>(this IDbCommand dbCmd)
            where T : new()
        {
            DropTable(dbCmd, ModelDefinition<T>.Definition);
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void DropTable(this IDbCommand dbCmd, Type modelType)
        {
            DropTable(dbCmd, modelType.GetModelDefinition());
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void DropTables(this IDbCommand dbCmd, params Type[] tableTypes)
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
                if (OrmLiteConfig.DialectProvider.DoesTableExist(dbCmd, modelDef.ModelName))
                {
                    var dropTableFks = OrmLiteConfig.DialectProvider.GetDropForeignKeyConstraints(modelDef);
                    if (!string.IsNullOrEmpty(dropTableFks))
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

        [Obsolete(UseDbConnectionExtensions)]
        public static string GetLastSql(this IDbCommand dbCmd)
        {
            return dbCmd.CommandText;
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static int ExecuteSql(this IDbCommand dbCmd, string sql)
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
		
		//DEFINE GENERATOR failed
		//
		
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
                    int index = NotFound;
                    if (indexCache != null)
                    {
                        if (!indexCache.TryGetValue(fieldDef.Name, out index))
                        {
                            index = dataReader.GetColumnIndex(fieldDef.FieldName);
                            indexCache.Add(fieldDef.Name, index);
                        }
                    }
                    else
                    {
                        index = dataReader.GetColumnIndex(fieldDef.FieldName);
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

        [Obsolete(UseDbConnectionExtensions)]
        public static void Update<T>(this IDbCommand dbCmd, params T[] objs)
			where T : new()
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToUpdateRowStatement(obj)); 
			}
		}

        [Obsolete(UseDbConnectionExtensions)]
        public static void UpdateAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
			where T : new()
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToUpdateRowStatement(obj));
			}
		}

        internal static IDbCommand CreateUpdateStatement<T>(this IDbConnection connection, T obj)
            where T : new()
        {
            return OrmLiteConfig.DialectProvider.CreateParameterizedUpdateStatement(obj, connection);
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void Delete<T>(this IDbCommand dbCmd, params T[] objs)
			where T : new()
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteRowStatement(obj));
			}
		}

        [Obsolete(UseDbConnectionExtensions)]
        public static void DeleteAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
			where T : new()
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteRowStatement(obj));
			}
		}

        [Obsolete(UseDbConnectionExtensions)]
        public static void DeleteById<T>(this IDbCommand dbCmd, object id)
            where T : new()
        {
            var modelDef = ModelDefinition<T>.Definition;

            var sql = string.Format("DELETE FROM {0} WHERE {1} = {2}",
                OrmLiteConfig.DialectProvider.GetQuotedTableName(modelDef),
                OrmLiteConfig.DialectProvider.GetQuotedColumnName(modelDef.PrimaryKey.FieldName),
                OrmLiteConfig.DialectProvider.GetQuotedValue(id, id.GetType()));


            dbCmd.ExecuteSql(sql);
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void DeleteByIds<T>(this IDbCommand dbCmd, IEnumerable idValues)
            where T : new()
        {
            var sqlIn = idValues.GetIdsInSql();
            if (sqlIn == null) return;

            var modelDef = ModelDefinition<T>.Definition;

            var sql = string.Format("DELETE FROM {0} WHERE {1} IN ({2})",
                OrmLiteConfig.DialectProvider.GetQuotedTableName(modelDef),
                OrmLiteConfig.DialectProvider.GetQuotedColumnName(modelDef.PrimaryKey.FieldName),
                sqlIn);

            dbCmd.ExecuteSql(sql);
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void DeleteByIdParam<T>(this IDbCommand dbCmd, object id)
        {
            var modelDef = ModelDefinition<T>.Definition;
            var idParamString = OrmLiteConfig.DialectProvider.ParamString+"0";

            var sql = string.Format("DELETE FROM {0} WHERE {1} = {2}",
                OrmLiteConfig.DialectProvider.GetQuotedTableName(modelDef),
                OrmLiteConfig.DialectProvider.GetQuotedColumnName(modelDef.PrimaryKey.FieldName),
                idParamString);

            var idParam = dbCmd.CreateParameter();
            idParam.ParameterName = idParamString;
            idParam.Value = id;
            dbCmd.Parameters.Add(idParam);
            
            dbCmd.ExecuteSql(sql);
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void DeleteAll<T>(this IDbCommand dbCmd)
        {
            DeleteAll(dbCmd, typeof(T));
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void DeleteAll(this IDbCommand dbCmd, Type tableType)
        {
			dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteStatement(tableType, null));
		}

        [Obsolete(UseDbConnectionExtensions)]
        public static void Delete<T>(this IDbCommand dbCmd, string sqlFilter, params object[] filterParams)
            where T : new()
        {
            Delete(dbCmd, typeof(T), sqlFilter, filterParams);
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void Delete(this IDbCommand dbCmd, Type tableType, string sqlFilter, params object[] filterParams)
        {
            dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteStatement(tableType, sqlFilter, filterParams));
        }


        [Obsolete(UseDbConnectionExtensions)]
        public static void Save<T>(this IDbCommand dbCmd, T obj)
            where T : new()
        {
            var id = obj.GetId();
            var existingRow = dbCmd.GetByIdOrDefault<T>(id);
            if (Equals(existingRow, default(T)))
            {
                dbCmd.Insert(obj);
            }
            else
            {
                dbCmd.Update(obj);
            }
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void Insert<T>(this IDbCommand dbCmd, params T[] objs)
			where T : new()
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToInsertRowStatement(obj, dbCmd));
			}
		}

        [Obsolete(UseDbConnectionExtensions)]
        public static void InsertAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
			where T : new()
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToInsertRowStatement(obj, dbCmd));
			}
		}

        internal static IDbCommand CreateInsertStatement<T>(this IDbConnection connection, T obj)
            where T: new()
        {
            return OrmLiteConfig.DialectProvider.CreateParameterizedInsertStatement(obj, connection);
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void Save<T>(this IDbCommand dbCmd, params T[] objs)
            where T : new()
        {
            SaveAll(dbCmd, objs);
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static void SaveAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
            where T : new()
        {
            var saveRows = objs.ToList();

            var firstRow = saveRows.FirstOrDefault();
            if (Equals(firstRow, default(T))) return;

            var defaultIdValue = ReflectionUtils.GetDefaultValue(firstRow.GetId().GetType());

            var idMap = defaultIdValue != null
                ? saveRows.Where(x => !defaultIdValue.Equals(x.GetId())).ToDictionary(x => x.GetId())
                : saveRows.Where(x => x.GetId() != null).ToDictionary(x => x.GetId());

            var existingRowsMap = dbCmd.GetByIds<T>(idMap.Keys).ToDictionary(x => x.GetId());

            using (var dbTrans = dbCmd.Connection.BeginTransaction())
            {
                dbCmd.Transaction = dbTrans;

                foreach (var saveRow in saveRows)
                {
                    var id = IdUtils.GetId(saveRow);
                    if (id != defaultIdValue && existingRowsMap.ContainsKey(id))
                    {
                        dbCmd.Update(saveRow);
                    }
                    else
                    {
                        dbCmd.Insert(saveRow);
                    }
                }

                dbTrans.Commit();
            }
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static IDbTransaction BeginTransaction(this IDbCommand dbCmd)
        {
            var dbTrans = dbCmd.Connection.BeginTransaction();
            dbCmd.ClearFilters();
            dbCmd.Transaction = dbTrans;
            return dbTrans;
        }

        [Obsolete(UseDbConnectionExtensions)]
        public static IDbTransaction BeginTransaction(this IDbCommand dbCmd, IsolationLevel isolationLevel)
        {
            var dbTrans = dbCmd.Connection.BeginTransaction(isolationLevel);
            dbCmd.ClearFilters();
            dbCmd.Transaction = dbTrans;
            return dbTrans;
        }
		
		// Procedures
        [Obsolete(UseDbConnectionExtensions)]
        public static void ExecuteProcedure<T>(this IDbCommand dbCommand, T obj)
        {	
			
			string sql= OrmLiteConfig.DialectProvider.ToExecuteProcedureStatement(obj);
			dbCommand.CommandType= CommandType.StoredProcedure;
			dbCommand.ExecuteSql(sql);
		}
		
    }
}
