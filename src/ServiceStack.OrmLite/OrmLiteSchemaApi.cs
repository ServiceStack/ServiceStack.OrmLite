using System;
using System.Data;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteSchemaApi
    {
        /// <summary>
        /// Checks whether a Table Exists. E.g:
        /// <para>db.TableExists("Person")</para>
        /// </summary>
        public static bool TableExists(this IDbConnection dbConn, string tableName)
        {
            return dbConn.GetDialectProvider().DoesTableExist(dbConn, tableName);
        }

        /// <summary>
        /// Checks whether a Table Exists. E.g:
        /// <para>db.TableExists&lt;Person&gt;()</para>
        /// </summary>
        public static bool TableExists<T>(this IDbConnection dbConn)
        {
            var dialectProvider = dbConn.GetDialectProvider();
            var modelDef = typeof(T).GetModelDefinition();
            var tableName = dialectProvider.NamingStrategy.GetTableName(modelDef);
            return dialectProvider.DoesTableExist(dbConn, tableName);
        }

        /// <summary>
        /// Create DB Tables from the schemas of runtime types. E.g:
        /// <para>db.CreateTables(typeof(Table1), typeof(Table2))</para> 
        /// </summary>
        public static void CreateTables(this IDbConnection dbConn, bool overwrite, params Type[] tableTypes)
        {
            dbConn.Exec(dbCmd => dbCmd.CreateTables(overwrite, tableTypes));
        }

        /// <summary>
        /// Create DB Table from the schema of the runtime type. Use overwrite to drop existing Table. E.g:
        /// <para>db.CreateTable(true, typeof(Table))</para> 
        /// </summary>
        public static void CreateTable(this IDbConnection dbConn, bool overwrite, Type modelType)
        {
            dbConn.Exec(dbCmd => dbCmd.CreateTable(overwrite, modelType));
        }

        /// <summary>
        /// Only Create new DB Tables from the schemas of runtime types if they don't already exist. E.g:
        /// <para>db.CreateTableIfNotExists(typeof(Table1), typeof(Table2))</para> 
        /// </summary>
        public static void CreateTableIfNotExists(this IDbConnection dbConn, params Type[] tableTypes)
        {
            dbConn.Exec(dbCmd => dbCmd.CreateTables(overwrite: false, tableTypes: tableTypes));
        }

        /// <summary>
        /// Drop existing DB Tables and re-create them from the schemas of runtime types. E.g:
        /// <para>db.DropAndCreateTables(typeof(Table1), typeof(Table2))</para> 
        /// </summary>
        public static void DropAndCreateTables(this IDbConnection dbConn, params Type[] tableTypes)
        {
            dbConn.Exec(dbCmd => dbCmd.CreateTables(overwrite: true, tableTypes: tableTypes));
        }

        /// <summary>
        /// Create a DB Table from the generic type. Use overwrite to drop the existing table or not. E.g:
        /// <para>db.CreateTable&lt;Person&gt;(overwrite=false) //default</para> 
        /// <para>db.CreateTable&lt;Person&gt;(overwrite=true)</para> 
        /// </summary>
        public static void CreateTable<T>(this IDbConnection dbConn, bool overwrite = false)
        {
            dbConn.Exec(dbCmd => dbCmd.CreateTable<T>(overwrite));
        }

        /// <summary>
        /// Only create a DB Table from the generic type if it doesn't already exist. E.g:
        /// <para>db.CreateTableIfNotExists&lt;Person&gt;()</para> 
        /// </summary>
        public static void CreateTableIfNotExists<T>(this IDbConnection dbConn)
        {
            dbConn.Exec(dbCmd => dbCmd.CreateTable<T>(false));
        }

        /// <summary>
        /// Only create a DB Table from the runtime type if it doesn't already exist. E.g:
        /// <para>db.CreateTableIfNotExists(typeof(Person))</para> 
        /// </summary>
        public static void CreateTableIfNotExists(this IDbConnection dbConn, Type modelType)
        {
            dbConn.Exec(dbCmd => dbCmd.CreateTable(false, modelType));
        }

        /// <summary>
        /// Drop existing table if exists and re-create a DB Table from the generic type. E.g:
        /// <para>db.DropAndCreateTable&lt;Person&gt;()</para> 
        /// </summary>
        public static void DropAndCreateTable<T>(this IDbConnection dbConn)
        {
            dbConn.Exec(dbCmd => dbCmd.CreateTable<T>(true));
        }

        /// <summary>
        /// Drop existing table if exists and re-create a DB Table from the runtime type. E.g:
        /// <para>db.DropAndCreateTable(typeof(Person))</para> 
        /// </summary>
        public static void DropAndCreateTable(this IDbConnection dbConn, Type modelType)
        {
            dbConn.Exec(dbCmd => dbCmd.CreateTable(true, modelType));
        }

        /// <summary>
        /// Drop any existing tables from their runtime types. E.g:
        /// <para>db.DropTables(typeof(Table1),typeof(Table2))</para> 
        /// </summary>
        public static void DropTables(this IDbConnection dbConn, params Type[] tableTypes)
        {
            dbConn.Exec(dbCmd => dbCmd.DropTables(tableTypes));
        }

        /// <summary>
        /// Drop any existing tables from the runtime type. E.g:
        /// <para>db.DropTable(typeof(Person))</para> 
        /// </summary>
        public static void DropTable(this IDbConnection dbConn, Type modelType)
        {
            dbConn.Exec(dbCmd => dbCmd.DropTable(modelType));
        }

        /// <summary>
        /// Drop any existing tables from the generic type. E.g:
        /// <para>db.DropTable&lt;Person&gt;()</para> 
        /// </summary>
        public static void DropTable<T>(this IDbConnection dbConn)
        {
            dbConn.Exec(dbCmd => dbCmd.DropTable<T>());
        }

         
    }
}