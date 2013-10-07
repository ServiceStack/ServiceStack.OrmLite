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
using ServiceStack.Text;

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

        internal static void CreateTable<T>(this IDbCommand dbCmd, bool overwrite = false)
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

        internal static int Update<T>(this IDbCommand dbCmd, T obj)
        {
            OrmLiteConfig.DialectProvider.PrepareParameterizedUpdateStatement<T>(dbCmd);

            OrmLiteConfig.DialectProvider.SetParameterValues<T>(dbCmd, obj);

            return dbCmd.ExecuteNonQuery();
        }

        internal static void Update<T>(this IDbCommand dbCmd, params T[] objs)
        {
            dbCmd.UpdateAll(objs);
        }

        internal static void UpdateAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
		{
            try
            {
                var dialectProvider = OrmLiteConfig.DialectProvider;

                dialectProvider.PrepareParameterizedUpdateStatement<T>(dbCmd);

                using (var dbTrans = dbCmd.Connection.BeginTransaction())
                {
                    dbCmd.Transaction = dbTrans;

                    foreach (var obj in objs)
                    {
                        dialectProvider.SetParameterValues<T>(dbCmd, obj);

                        dbCmd.ExecuteNonQuery();
                    }

                    dbTrans.Commit();
                }
            }
            catch
            {
                if (dbCmd.Transaction != null)
                    dbCmd.Transaction.Rollback();
            }
        }

        internal static int Delete<T>(this IDbCommand dbCmd, object anonType)
        {
            OrmLiteConfig.DialectProvider.PrepareParameterizedDeleteStatement<T>(dbCmd, anonType.AllFields<T>());

            OrmLiteConfig.DialectProvider.SetParameterValues<T>(dbCmd, anonType);

            return dbCmd.ExecuteNonQuery();
        }

        internal static int DeleteNonDefaults<T>(this IDbCommand dbCmd, T filter)
        {
            OrmLiteConfig.DialectProvider.PrepareParameterizedDeleteStatement<T>(dbCmd, filter.NonDefaultFields<T>());

            OrmLiteConfig.DialectProvider.SetParameterValues<T>(dbCmd, filter);

            return dbCmd.ExecuteNonQuery();
        }

        internal static void Delete<T>(this IDbCommand dbCmd, params object[] objs)
        {
            if (objs.Length == 0) return;

            DeleteAll<T>(dbCmd, objs[0].AllFields<T>(), objs);
        }

        internal static void DeleteNonDefaults<T>(this IDbCommand dbCmd, params T[] filters)
        {
            if (filters.Length == 0) return;

            DeleteAll<T>(dbCmd, filters[0].NonDefaultFields<T>(), filters.Map(x => (object)x));
        }

        private static void DeleteAll<T>(IDbCommand dbCmd, ICollection<string> deleteFields, IEnumerable<object> objs)
        {
            try
            {
                var dialectProvider = OrmLiteConfig.DialectProvider;

                dialectProvider.PrepareParameterizedDeleteStatement<T>(dbCmd, deleteFields);

                using (var dbTrans = dbCmd.Connection.BeginTransaction())
                {
                    dbCmd.Transaction = dbTrans;

                    foreach (var obj in objs)
                    {
                        dialectProvider.SetParameterValues<T>(dbCmd, obj);

                        dbCmd.ExecuteNonQuery();
                    }

                    dbTrans.Commit();
                }
            }
            catch
            {
                if (dbCmd.Transaction != null)
                    dbCmd.Transaction.Rollback();
            }
        }

        internal static int DeleteById<T>(this IDbCommand dbCmd, object id)
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

            return dbCmd.ExecuteSql(sql);
        }

        internal static int DeleteByIds<T>(this IDbCommand dbCmd, IEnumerable idValues)
        {
            var sqlIn = idValues.GetIdsInSql();
            if (sqlIn == null) return 0;

            var modelDef = ModelDefinition<T>.Definition;

            var sql = String.Format("DELETE FROM {0} WHERE {1} IN ({2})",
                OrmLiteConfig.DialectProvider.GetQuotedTableName(modelDef),
                OrmLiteConfig.DialectProvider.GetQuotedColumnName(modelDef.PrimaryKey.FieldName),
                sqlIn);

            return dbCmd.ExecuteSql(sql);
        }

        internal static int DeleteAll<T>(this IDbCommand dbCmd)
        {
            return DeleteAll(dbCmd, typeof(T));
        }

        internal static int DeleteAll(this IDbCommand dbCmd, Type tableType)
        {
			return dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteStatement(tableType, null));
		}

        internal static int DeleteFmt<T>(this IDbCommand dbCmd, string sqlFilter, params object[] filterParams)
        {
            return DeleteFmt(dbCmd, typeof(T), sqlFilter, filterParams);
        }

        internal static int DeleteFmt(this IDbCommand dbCmd, Type tableType, string sqlFilter, params object[] filterParams)
        {
            return dbCmd.ExecuteSql(OrmLiteConfig.DialectProvider.ToDeleteStatement(tableType, sqlFilter, filterParams));
        }

        internal static bool Save<T>(this IDbCommand dbCmd, T obj)
        {
            var id = obj.GetId();
            var existingRow = dbCmd.SingleById<T>(id);
            if (Equals(existingRow, default(T)))
            {
                dbCmd.Insert(obj);
                return true;
            }
            
            dbCmd.Update(obj);
            return false;
        }

        internal static long Insert<T>(this IDbCommand dbCmd, T obj, bool selectIdentity = false)
        {
            OrmLiteConfig.DialectProvider.PrepareParameterizedInsertStatement<T>(dbCmd);

            OrmLiteConfig.DialectProvider.SetParameterValues<T>(dbCmd, obj);

            if (selectIdentity)
                return OrmLiteConfig.DialectProvider.InsertAndGetLastInsertId<T>(dbCmd);

            dbCmd.ExecuteNonQuery();
            return -1;
        }

        internal static void Insert<T>(this IDbCommand dbCmd, params T[] objs)
        {
            InsertAll(dbCmd, objs);
        }

        internal static void InsertAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
		{
            try
            {
                var dialectProvider = OrmLiteConfig.DialectProvider;

                dialectProvider.PrepareParameterizedInsertStatement<T>(dbCmd);

                using (var dbTrans = dbCmd.Connection.BeginTransaction())
                {
                    dbCmd.Transaction = dbTrans;

                    foreach (var obj in objs)
                    {
                        dialectProvider.SetParameterValues<T>(dbCmd, obj);

                        dbCmd.ExecuteNonQuery();
                    }

                    dbTrans.Commit();
                }
            }
            catch
            {
                if (dbCmd.Transaction != null)
                    dbCmd.Transaction.Rollback();
            }
        }

        internal static int Save<T>(this IDbCommand dbCmd, params T[] objs)
        {
            return SaveAll(dbCmd, objs);
        }

        internal static int SaveAll<T>(this IDbCommand dbCmd, IEnumerable<T> objs)
        {
            var saveRows = objs.ToList();

            var firstRow = saveRows.FirstOrDefault();
            if (Equals(firstRow, default(T))) return 0;

            var defaultIdValue = firstRow.GetId().GetType().GetDefaultValue();

            var idMap = defaultIdValue != null
                ? saveRows.Where(x => !defaultIdValue.Equals(x.GetId())).ToSafeDictionary(x => x.GetId())
                : saveRows.Where(x => x.GetId() != null).ToSafeDictionary(x => x.GetId());

            var existingRowsMap = dbCmd.SelectByIds<T>(idMap.Keys).ToDictionary(x => x.GetId());

            var rowsAdded = 0;
            using (var dbTrans = dbCmd.Connection.BeginTransaction())
            {
                dbCmd.Transaction = dbTrans;

                foreach (var saveRow in saveRows)
                {
                    var id = saveRow.GetId();
                    if (id != defaultIdValue && existingRowsMap.ContainsKey(id))
                    {
                        dbCmd.Update(saveRow);
                    }
                    else
                    {
                        dbCmd.Insert(saveRow);
                        rowsAdded++;
                    }
                }

                dbTrans.Commit();
            }
            return rowsAdded;
        }
		
		// Procedures
        internal static void ExecuteProcedure<T>(this IDbCommand dbCommand, T obj)
        {
			string sql = OrmLiteConfig.DialectProvider.ToExecuteProcedureStatement(obj);
			dbCommand.CommandType = CommandType.StoredProcedure;
			dbCommand.ExecuteSql(sql);
		}
    }
}
