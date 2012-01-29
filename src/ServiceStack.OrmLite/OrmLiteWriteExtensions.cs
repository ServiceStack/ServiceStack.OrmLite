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

        public static string ToCreateTableStatement(this Type tableType)
        {
            var sbColumns = new StringBuilder();
            var sbConstraints = new StringBuilder();

            var modelDef = tableType.GetModelDefinition();
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (sbColumns.Length != 0) sbColumns.Append(", \n  ");

                var columnDefinition = OrmLiteConfig.DialectProvider.GetColumnDefinition(
                    fieldDef.FieldName,
                    fieldDef.FieldType,
                    fieldDef.IsPrimaryKey,
                    fieldDef.AutoIncrement,
                    fieldDef.IsNullable,
                    fieldDef.FieldLength,
                    fieldDef.DefaultValue);

                sbColumns.Append(columnDefinition);

                if (fieldDef.ReferencesType == null) continue;

                var refModelDef = fieldDef.ReferencesType.GetModelDefinition();
                sbConstraints.AppendFormat(
                    ", \n\n  CONSTRAINT {0} FOREIGN KEY ({1}) REFERENCES {2} ({3})",
                    OrmLiteConfig.DialectProvider.GetNameDelimited(string.Format("FK_{0}_{1}", modelDef.ModelName,
                                                                                       refModelDef.ModelName)),
                    OrmLiteConfig.DialectProvider.GetNameDelimited(fieldDef.FieldName),
                    OrmLiteConfig.DialectProvider.GetTableNameDelimited(refModelDef),
                    OrmLiteConfig.DialectProvider.GetNameDelimited(refModelDef.PrimaryKey.FieldName));
            }
            var sql = new StringBuilder(string.Format(
                "CREATE TABLE {0} \n(\n  {1}{2} \n); \n", OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef), sbColumns, sbConstraints));

            return sql.ToString();
        }

        public static List<string> ToCreateIndexStatements(this Type tableType)
        {
            var sqlIndexes = new List<string>();

            var modelDef = tableType.GetModelDefinition();
            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (!fieldDef.IsIndexed) continue;

                var indexName = GetIndexName(fieldDef.IsUnique, modelDef.ModelName.SafeVarName(), fieldDef.FieldName);

                sqlIndexes.Add(
                    ToCreateIndexStatement(fieldDef.IsUnique, indexName, modelDef, fieldDef.FieldName));
            }

            foreach (var compositeIndex in modelDef.CompositeIndexes)
            {
                var indexName = GetIndexName(compositeIndex.Unique, modelDef.ModelName.SafeVarName(),
                    string.Join("_", compositeIndex.FieldNames.ToArray()));

                var indexNames = string.Join(" ASC, ",
                                             compositeIndex.FieldNames.ConvertAll(
                                                 n => OrmLiteConfig.DialectProvider.GetNameDelimited(n)).ToArray());

                sqlIndexes.Add(
                    ToCreateIndexStatement(compositeIndex.Unique, indexName, modelDef, indexNames, true));
            }

            return sqlIndexes;
        }

        private static string GetIndexName(bool isUnique, string modelName, string fieldName)
        {
            return string.Format("{0}idx_{1}_{2}", isUnique ? "u" : "", modelName, fieldName).ToLower();
        }

        private static string ToCreateIndexStatement(bool isUnique, string indexName, ModelDefinition modelDef, string fieldName, bool isCombined = false)
        {
            return string.Format("CREATE {0} INDEX {1} ON {2} ({3} ASC); \n",
                                 isUnique ? "UNIQUE" : "", indexName,
                                 OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef),
                                 (isCombined) ? fieldName : OrmLiteConfig.DialectProvider.GetNameDelimited(fieldName));
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
            if (overwrite)
            {
                DropTable(dbCmd, modelDef);
            }

            try
            {
                ExecuteSql(dbCmd, ToCreateTableStatement(modelType));

                var sqlIndexes = ToCreateIndexStatements(modelType);
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
                dbCmd.ExecuteSql(string.Format("DROP TABLE {0};", OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef)));
            }
            catch (Exception )
            {
                //Log.DebugFormat("Cannot drop non-existing table '{0}': {1}", modelDef.ModelName, ex.Message);
            }
        }

        private static void ExecuteSql(this IDbCommand dbCmd, string sql)
        {
            LogDebug(sql);

            dbCmd.CommandText = sql;
            dbCmd.ExecuteNonQuery();
        }

        private static bool IgnoreAlreadyExistsError(Exception ex)
        {
            //ignore Sqlite table already exists error
            const string sqliteAlreadyExistsError = "already exists";
            const string sqlServerAlreadyExistsError = "There is already an object named";
            return ex.Message.Contains(sqliteAlreadyExistsError)
                   || ex.Message.Contains(sqlServerAlreadyExistsError);
        }

        public static T PopulateWithSqlReader<T>(this T objWithProperties, IDataReader dataReader)
        {
            var fieldDefs = ModelDefinition<T>.Definition.FieldDefinitions.ToArray();

            return PopulateWithSqlReader(objWithProperties, dataReader, fieldDefs);
        }

        public static T PopulateWithSqlReader<T>(this T objWithProperties, IDataReader dataReader, FieldDefinition[] fieldDefs)
        {
            foreach (var fieldDef in fieldDefs)
            {
				var value = dataReader.GetValue(dataReader.GetOrdinal(fieldDef.FieldName));
                fieldDef.SetValue(objWithProperties, value);
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
                                    OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef),
                                    OrmLiteConfig.DialectProvider.GetNameDelimited(modelDef.PrimaryKey.FieldName),
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
                                    OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef),
                                    OrmLiteConfig.DialectProvider.GetNameDelimited(modelDef.PrimaryKey.FieldName),
                                    sqlIn);

            dbCmd.ExecuteSql(sql);
        }

        public static void DeleteAll<T>(this IDbCommand dbCmd)
        {
            DeleteAll(dbCmd, typeof(T));
        }

        public static void DeleteAll(this IDbCommand dbCmd, Type tableType)
        {
			dbCmd.ExecuteSql(ToDeleteStatement(tableType, null));
		}

        public static void Delete<T>(this IDbCommand dbCmd, string sqlFilter, params object[] filterParams)
            where T : new()
        {
            Delete(dbCmd, typeof(T), sqlFilter, filterParams);
        }

        public static void Delete(this IDbCommand dbCmd, Type tableType, string sqlFilter, params object[] filterParams)
        {
            dbCmd.ExecuteSql(ToDeleteStatement(tableType, sqlFilter, filterParams));
        }

        public static string ToDeleteStatement(this Type tableType, string sqlFilter, params object[] filterParams)
        {
            var sql = new StringBuilder();
            const string deleteStatement = "DELETE ";

            var isFullDeleteStatement =
                !string.IsNullOrEmpty(sqlFilter)
                && sqlFilter.Length > deleteStatement.Length
                && sqlFilter.Substring(0, deleteStatement.Length).ToUpper().Equals(deleteStatement);

            if (isFullDeleteStatement) return sqlFilter.SqlFormat(filterParams);

            var modelDef = tableType.GetModelDefinition();
            sql.AppendFormat("DELETE FROM {0}", OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef));
            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFormat(filterParams);
                sql.Append(" WHERE ");
                sql.Append(sqlFilter);
            }

            return sql.ToString();
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