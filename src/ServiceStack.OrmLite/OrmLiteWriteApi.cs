using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ServiceStack.Data;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteWriteApi
    {
        /// <summary>
        /// Get the last SQL statement that was executed.
        /// </summary>
        public static string GetLastSql(this IDbConnection dbConn)
        {
            var ormLiteConn = dbConn as OrmLiteConnection;
            return ormLiteConn != null ? ormLiteConn.LastCommandText : null;
        }

        /// <summary>
        /// Execute any arbitrary raw SQL.
        /// </summary>
        /// <returns>number of rows affected</returns>
        public static int ExecuteSql(this IDbConnection dbConn, string sql)
        {
            return dbConn.Exec(dbCmd => dbCmd.ExecuteSql(sql));
        }

        /// <summary>
        /// Insert 1 POCO, use selectIdentity to retrieve the last insert AutoIncrement id (if any). E.g:
        /// <para>var id = db.Insert(new Person { Id = 1, FirstName = "Jimi }, selectIdentity:true)</para>
        /// </summary>
        public static long Insert<T>(this IDbConnection dbConn, T obj, bool selectIdentity = false)
        {
            return dbConn.Exec(dbCmd => dbCmd.Insert(obj, selectIdentity));
        }

        /// <summary>
        /// Insert 1 or more POCOs in a transaction. E.g:
        /// <para>db.Insert(new Person { Id = 1, FirstName = "Tupac", LastName = "Shakur", Age = 25 },</para>
        /// <para>          new Person { Id = 2, FirstName = "Biggie", LastName = "Smalls", Age = 24 })</para>
        /// </summary>
        public static void Insert<T>(this IDbConnection dbConn, params T[] objs)
        {
            dbConn.Exec(dbCmd => dbCmd.Insert(objs));
        }

        /// <summary>
        /// Insert a collection of POCOs in a transaction. E.g:
        /// <para>db.InsertAll(new[] { new Person { Id = 9, FirstName = "Biggie", LastName = "Smalls", Age = 24 } })</para>
        /// </summary>
        public static void InsertAll<T>(this IDbConnection dbConn, IEnumerable<T> objs)
        {
            dbConn.Exec(dbCmd => dbCmd.InsertAll(objs));
        }

        /// <summary>
        /// Updates 1 POCO. All fields are updated except for the PrimaryKey which is used as the identity selector. E.g:
        /// <para>db.Update(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 })</para>
        /// </summary>
        public static int Update<T>(this IDbConnection dbConn, T obj)
        {
            return dbConn.Exec(dbCmd => dbCmd.Update(obj));
        }

        /// <summary>
        /// Updates 1 or more POCOs in a transaction. E.g:
        /// <para>db.Update(new Person { Id = 1, FirstName = "Tupac", LastName = "Shakur", Age = 25 },</para>
        /// <para>new Person { Id = 2, FirstName = "Biggie", LastName = "Smalls", Age = 24 })</para>
        /// </summary>
        public static int Update<T>(this IDbConnection dbConn, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.Update(objs));
        }

        /// <summary>
        /// Updates 1 or more POCOs in a transaction. E.g:
        /// <para>db.UpdateAll(new[] { new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 } })</para>
        /// </summary>
        public static int UpdateAll<T>(this IDbConnection dbConn, IEnumerable<T> objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateAll(objs));
        }

        /// <summary>
        /// Delete rows using an anonymous type filter. E.g:
        /// <para>db.Delete&lt;Person&gt;(new { FirstName = "Jimi", Age = 27 })</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static int Delete<T>(this IDbConnection dbConn, object anonFilter)
        {
            return dbConn.Exec(dbCmd => dbCmd.Delete<T>(anonFilter));
        }

        /// <summary>
        /// Delete 1 or more rows in a transaction using an anonymous type filter. E.g:
        /// <para>db.Delete&lt;Person&gt;(new { FirstName = "Jimi", Age = 27 }, new { FirstName = "Janis", Age = 27 })</para>
        /// </summary>
        public static int Delete<T>(this IDbConnection dbConn, params object[] anonFilters)
        {
            return dbConn.Exec(dbCmd => dbCmd.Delete<T>(anonFilters));
        }

        /// <summary>
        /// Delete 1 row using all fields in the filter. E.g:
        /// <para>db.Delete(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 })</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static int Delete<T>(this IDbConnection dbConn, T allFieldsFilter)
        {
            return dbConn.Exec(dbCmd => dbCmd.Delete<T>(allFieldsFilter));
        }

        /// <summary>
        /// Delete 1 or more rows in a transaction using all fields in the filter. E.g:
        /// <para>db.Delete(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 })</para>
        /// </summary>
        public static int Delete<T>(this IDbConnection dbConn, params T[] allFieldsFilters)
        {
            return dbConn.Exec(dbCmd => dbCmd.Delete<T>(allFieldsFilters));
        }

        /// <summary>
        /// Delete 1 or more rows using only field with non-default values in the filter. E.g:
        /// <para>db.DeleteNonDefaults(new Person { FirstName = "Jimi", Age = 27 })</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static int DeleteNonDefaults<T>(this IDbConnection dbConn, T nonDefaultsFilter)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteNonDefaults(nonDefaultsFilter));
        }

        /// <summary>
        /// Delete 1 or more rows in a transaction using only field with non-default values in the filter. E.g:
        /// <para>db.DeleteNonDefaults(new Person { FirstName = "Jimi", Age = 27 }, 
        /// new Person { FirstName = "Janis", Age = 27 })</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static int DeleteNonDefaults<T>(this IDbConnection dbConn, params T[] nonDefaultsFilters)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteNonDefaults(nonDefaultsFilters));
        }

        /// <summary>
        /// Delete 1 row by the PrimaryKey. E.g:
        /// <para>db.DeleteById&lt;Person&gt;(1)</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static int DeleteById<T>(this IDbConnection dbConn, object id)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteById<T>(id));
        }

        /// <summary>
        /// Delete 1 row by the PrimaryKey where the rowVersion matches the optimistic concurrency field. 
        /// Will throw <exception cref="OptimisticConcurrencyException">RowModefiedExeption</exception> if the 
        /// row does not exist or has a different row version.
        /// E.g: <para>db.DeleteById&lt;Person&gt;(1)</para>
        /// </summary>
        public static void DeleteById<T>(this IDbConnection dbConn, object id, ulong rowVersion)
        {
            dbConn.Exec(dbCmd => dbCmd.DeleteById<T>(id, rowVersion));
        }

        /// <summary>
        /// Delete all rows identified by the PrimaryKeys. E.g:
        /// <para>db.DeleteById&lt;Person&gt;(new[] { 1, 2, 3 })</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static int DeleteByIds<T>(this IDbConnection dbConn, IEnumerable idValues)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteByIds<T>(idValues));
        }

        /// <summary>
        /// Delete all rows in the generic table type. E.g:
        /// <para>db.DeleteAll&lt;Person&gt;()</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static int DeleteAll<T>(this IDbConnection dbConn)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAll<T>());
        }

        /// <summary>
        /// Delete all rows in the runtime table type. E.g:
        /// <para>db.DeleteAll(typeof(Person))</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static int DeleteAll(this IDbConnection dbConn, Type tableType)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAll(tableType));
        }

        /// <summary>
        /// Delete rows using a SqlFormat filter. E.g:
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static int DeleteFmt<T>(this IDbConnection dbConn, string sqlFilter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteFmt<T>(sqlFilter, filterParams));
        }

        /// <summary>
        /// Delete rows from the runtime table type using a SqlFormat filter. E.g:
        /// </summary>
        /// <para>db.DeleteFmt(typeof(Person), "Age = {0}", 27)</para>
        /// <returns>number of rows deleted</returns>
        public static int DeleteFmt(this IDbConnection dbConn, Type tableType, string sqlFilter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteFmt(tableType, sqlFilter, filterParams));
        }

        /// <summary>
        /// Insert a new row or update existing row. Returns true if a new row was inserted. 
        /// Optional references param decides whether to save all related references as well. E.g:
        /// <para>db.Save(customer, references:true)</para>
        /// </summary>
        /// <returns>true if a row was inserted; false if it was updated</returns>
        public static bool Save<T>(this IDbConnection dbConn, T obj, bool references = false)
        {
            if (!references)
                return dbConn.Exec(dbCmd => dbCmd.Save(obj));

            return dbConn.Exec(dbCmd =>
            {
                var ret = dbCmd.Save(obj);
                dbCmd.SaveAllReferences(obj);
                return ret;
            });
        }

        /// <summary>
        /// Insert new rows or update existing rows. Return number of rows added E.g:
        /// <para>db.Save(new Person { Id = 10, FirstName = "Amy", LastName = "Winehouse", Age = 27 })</para>
        /// </summary>
        /// <returns>number of rows added</returns>
        public static int Save<T>(this IDbConnection dbConn, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.Save(objs));
        }

        /// <summary>
        /// Insert new rows or update existing rows. Return number of rows added E.g:
        /// <para>db.SaveAll(new [] { new Person { Id = 10, FirstName = "Amy", LastName = "Winehouse", Age = 27 } })</para>
        /// </summary>
        /// <returns>number of rows added</returns>
        public static int SaveAll<T>(this IDbConnection dbConn, IEnumerable<T> objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.SaveAll(objs));
        }

        /// <summary>
        /// Populates all related references on the instance with its primary key and saves them. Uses '(T)Id' naming convention. E.g:
        /// <para>db.SaveAllReferences(customer)</para> 
        /// </summary>
        public static void SaveAllReferences<T>(this IDbConnection dbConn, T instance)
        {
            dbConn.Exec(dbCmd => dbCmd.SaveAllReferences(instance));
        }

        /// <summary>
        /// Populates the related references with the instance primary key and saves them. Uses '(T)Id' naming convention. E.g:
        /// <para>db.SaveReference(customer, customer.Orders)</para> 
        /// </summary>
        public static void SaveReferences<T, TRef>(this IDbConnection dbConn, T instance, params TRef[] refs)
        {
            dbConn.Exec(dbCmd => dbCmd.SaveReferences(instance, refs));
        }

        /// <summary>
        /// Populates the related references with the instance primary key and saves them. Uses '(T)Id' naming convention. E.g:
        /// <para>db.SaveReference(customer, customer.Orders)</para> 
        /// </summary>
        public static void SaveReferences<T, TRef>(this IDbConnection dbConn, T instance, List<TRef> refs)
        {
            dbConn.Exec(dbCmd => dbCmd.SaveReferences(instance, refs.ToArray()));
        }

        /// <summary>
        /// Populates the related references with the instance primary key and saves them. Uses '(T)Id' naming convention. E.g:
        /// <para>db.SaveReferences(customer, customer.Orders)</para> 
        /// </summary>
        public static void SaveReferences<T, TRef>(this IDbConnection dbConn, T instance, IEnumerable<TRef> refs)
        {
            dbConn.Exec(dbCmd => dbCmd.SaveReferences(instance, refs.ToArray()));
        }

        // Procedures
        public static void ExecuteProcedure<T>(this IDbConnection dbConn, T obj)
        {
            dbConn.Exec(dbCmd => dbCmd.ExecuteProcedure(obj));
        }
    }
}