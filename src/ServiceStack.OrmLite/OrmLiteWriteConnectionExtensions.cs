﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteWriteConnectionExtensions
    {
        public static bool TableExists(this IDbConnection dbConn, string tableName)
        {
            return dbConn.GetDialectProvider().DoesTableExist(dbConn, tableName);
        }

        public static void CreateTables(this IDbConnection dbConn, bool overwrite, params Type[] tableTypes)
        {
            dbConn.Exec(dbCmd => dbCmd.CreateTables(overwrite, tableTypes));
        }

        public static void CreateTable<T>(this IDbConnection dbConn)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.CreateTable<T>());
        }

        public static void CreateTable<T>(this IDbConnection dbConn, bool overwrite)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.CreateTable<T>(overwrite));
        }

        public static void CreateTable(this IDbConnection dbConn, bool overwrite, Type modelType)
        {
            dbConn.Exec(dbCmd => dbCmd.CreateTable(overwrite, modelType));
        }

        public static void DropTables(this IDbConnection dbConn, params Type[] tableTypes)
        {
            dbConn.Exec(dbCmd => dbCmd.DropTables(tableTypes));
        }

        public static void DropTable(this IDbConnection dbConn, Type modelType)
        {
            dbConn.Exec(dbCmd => dbCmd.DropTable(modelType));
        }

        public static void DropTable<T>(this IDbConnection dbConn)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.DropTable<T>());
        }

        public static string GetLastSql(this IDbConnection dbConn)
        {
            return ReadConnectionExtensions.LastCommandText;
        }

        public static int ExecuteSql(this IDbConnection dbConn, string sql)
        {
            return dbConn.Exec(dbCmd => dbCmd.ExecuteSql(sql));
        }

        public static void Update<T>(this IDbConnection dbConn, params T[] objs)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.Update(objs));
        }

        public static void UpdateAll<T>(this IDbConnection dbConn, IEnumerable<T> objs)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.UpdateAll(objs));
        }

        public static void Delete<T>(this IDbConnection dbConn, params T[] objs)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.Delete(objs));
        }

        public static void DeleteAll<T>(this IDbConnection dbConn, IEnumerable<T> objs)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.DeleteAll(objs));
        }

        public static void DeleteById<T>(this IDbConnection dbConn, object id)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.DeleteById<T>(id));
        }

        public static void DeleteByIds<T>(this IDbConnection dbConn, IEnumerable idValues)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.DeleteByIds<T>(idValues));
        }

        public static void DeleteAll<T>(this IDbConnection dbConn)
        {
            dbConn.Exec(dbCmd => dbCmd.DeleteAll<T>());
        }

        public static void DeleteAll(this IDbConnection dbConn, Type tableType)
        {
            dbConn.Exec(dbCmd => dbCmd.DeleteAll(tableType));
        }

        public static void Delete<T>(this IDbConnection dbConn, string sqlFilter, params object[] filterParams)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.Delete<T>(sqlFilter, filterParams));
        }

        public static void Delete(this IDbConnection dbConn, Type tableType, string sqlFilter, params object[] filterParams)
        {
            dbConn.Exec(dbCmd => dbCmd.Delete(tableType, sqlFilter, filterParams));
        }

        public static void Save<T>(this IDbConnection dbConn, T obj)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.Save(obj));
        }

        public static void Insert<T>(this IDbConnection dbConn, params T[] objs)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.Insert(objs));
        }

        public static void InsertAll<T>(this IDbConnection dbConn, IEnumerable<T> objs)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.InsertAll(objs));
        }

        public static void Save<T>(this IDbConnection dbConn, params T[] objs)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.Save(objs));
        }

        public static void SaveAll<T>(this IDbConnection dbConn, IEnumerable<T> objs)
            where T : new()
        {
            dbConn.Exec(dbCmd => dbCmd.SaveAll(objs));
        }

        public static IDbTransaction BeginTransaction(this IDbConnection dbConn)
        {
            return dbConn.Exec(dbCmd => dbCmd.BeginTransaction());
        }

        public static IDbTransaction BeginTransaction(this IDbConnection dbConn, IsolationLevel isolationLevel)
        {
            return dbConn.Exec(dbCmd => dbCmd.BeginTransaction(isolationLevel));
        }

        // Procedures
        public static void ExecuteProcedure<T>(this IDbConnection dbConn, T obj)
        {
            dbConn.Exec(dbCmd => dbCmd.ExecuteProcedure(obj));
        }
    }
}