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
using System.Text;
using ServiceStack.Common.Extensions;
using ServiceStack.Common.Utils;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteWriteExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OrmLiteWriteExtensions));

        [Conditional("DEBUG")]
        private static void LogDebug(string fmt, params object[] args)
        {
            if (args.Length > 0)
                Log.DebugFormat(fmt, args);
            else
                Log.Debug(fmt);
        }

        public static void CreateTables(this IDbCommand dbCmd, bool overwrite, params Type[] tableTypes)
        {
            foreach (var tableType in tableTypes)
            {
                CreateTable(dbCmd, overwrite, tableType);
            }
        }

        public static void CreateTable<T>(this IDbCommand dbCmd)
            where T : new()
        {
            var tableType = typeof(T);
            CreateTable(dbCmd, false, tableType);
        }

        public static void CreateTable<T>(this IDbCommand dbCmd, bool overwrite)
            where T : new()
        {
            var tableType = typeof(T);
            CreateTable(dbCmd, overwrite, tableType);
        }

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
        
        public static void DropTable<T>(this IDbCommand dbCmd)
            where T : new()
        {
            DropTable(dbCmd, ModelDefinition<T>.Definition);
        }

        private static void DropTable(IDbCommand dbCmd, ModelDefinition modelDef)
        {
            try
            {
                dbCmd.ExecuteSql(string.Format("DROP TABLE {0};", OrmLiteConfig.DialectProvider.GetQuotedTableName(modelDef)));
            }
            catch (Exception)
            {
                //Log.DebugFormat("Cannot drop non-existing table '{0}': {1}", modelDef.ModelName, ex.Message);
            }
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
		
		//DEFINE GENERATOR failed
		//
		
		private static bool IgnoreAlreadyExistsGeneratorError(Exception ex)
        {
            const string fbError = "attempt to store duplicate value";
            return ex.Message.Contains(fbError);
                   
        }
		
		
        public static T PopulateWithSqlReader<T>(this T objWithProperties, IDataReader dataReader)
        {
            var fieldDefs = ModelDefinition<T>.Definition.FieldDefinitions.ToArray();

            return PopulateWithSqlReader(objWithProperties, dataReader, fieldDefs);
        }

        public static T PopulateWithSqlReader<T>(this T objWithProperties, IDataReader dataReader, FieldDefinition[] fieldDefs)
        {
			try
			{
				foreach (var fieldDef in fieldDefs)
				{
					var index = dataReader.GetOrdinal(OrmLiteConfig.DialectProvider.NamingStrategy.GetColumnName(fieldDef.FieldName));
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
		
        
		
        

		public static void Update<T>(this IDbCommand dbCmd, params T[] objs)
			where T : new()
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql( OrmLiteConfig.DialectProvider.ToUpdateRowStatement( obj)); 
			}
		}

		public static void UpdateAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
			where T : new()
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToUpdateRowStatement(obj));
			}
		}

        public static IDbCommand CreateUpdateStatement<T>(this IDbConnection connection, T obj)
            where T : new()
        {
            return OrmLiteConfig.DialectProvider.CreateParameterizedUpdateStatement(obj, connection);
        }

		public static void Delete<T>(this IDbCommand dbCmd, params T[] objs)
			where T : new()
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteRowStatement(obj));
			}
		}

		public static void DeleteAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
			where T : new()
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteRowStatement(obj));
			}
		}

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

        public static void DeleteAll<T>(this IDbCommand dbCmd)
        {
            DeleteAll(dbCmd, typeof(T));
        }

        public static void DeleteAll(this IDbCommand dbCmd, Type tableType)
        {
			dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteStatement(tableType, null));
		}

        public static void Delete<T>(this IDbCommand dbCmd, string sqlFilter, params object[] filterParams)
            where T : new()
        {
            Delete(dbCmd, typeof(T), sqlFilter, filterParams);
        }

        public static void Delete(this IDbCommand dbCmd, Type tableType, string sqlFilter, params object[] filterParams)
        {
            dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteStatement(tableType, sqlFilter, filterParams));
        }

        
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

		public static void Insert<T>(this IDbCommand dbCmd, params T[] objs)
			where T : new()
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToInsertRowStatement(obj, dbCmd));
			}
		}

		public static void InsertAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
			where T : new()
		{
			foreach (var obj in objs)
			{
				dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToInsertRowStatement(obj, dbCmd));
			}
		}

        public static IDbCommand CreateInsertStatement<T>(this IDbConnection connection, T obj)
            where T: new()
        {
            return OrmLiteConfig.DialectProvider.CreateParameterizedInsertStatement(obj, connection);
        }

        public static void ReparameterizeInsert<T>(this IDbCommand command, T obj)
            where T : new()
        {
            OrmLiteConfig.DialectProvider.ReParameterizeInsertStatement(obj, command);
        }

        public static void Save<T>(this IDbCommand dbCmd, params T[] objs)
            where T : new()
        {
            SaveAll(dbCmd, objs);
        }

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

        public static IDbTransaction BeginTransaction(this IDbCommand dbCmd)
        {
            var dbTrans = dbCmd.Connection.BeginTransaction();
            dbCmd.ClearFilters();
            dbCmd.Transaction = dbTrans;
            return dbTrans;
        }

        public static IDbTransaction BeginTransaction(this IDbCommand dbCmd, IsolationLevel isolationLevel)
        {
            var dbTrans = dbCmd.Connection.BeginTransaction(isolationLevel);
            dbCmd.ClearFilters();
            dbCmd.Transaction = dbTrans;
            return dbTrans;
        }
		
		
		// Procedures
		
		public static void ExecuteProcedure<T>(this IDbCommand dbCommand, T obj){	
			
			string sql= OrmLiteConfig.DialectProvider.ToExecuteProcedureStatement(obj);
			dbCommand.CommandType= CommandType.StoredProcedure;
			dbCommand.ExecuteSql(sql);
		}
		
    }
}
