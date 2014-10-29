#if NET45
// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Data;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteWriteApiAsync
    {
        /// <summary>
        /// Execute any arbitrary raw SQL.
        /// </summary>
        /// <returns>number of rows affected</returns>
        public static Task<int> ExecuteSqlAsync(this IDbConnection dbConn, string sql, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.ExecuteSqlAsync(sql, token));
        }

        /// <summary>
        /// Insert 1 POCO, use selectIdentity to retrieve the last insert AutoIncrement id (if any). E.g:
        /// <para>var id = db.Insert(new Person { Id = 1, FirstName = "Jimi }, selectIdentity:true)</para>
        /// </summary>
        public static Task<long> InsertAsync<T>(this IDbConnection dbConn, T obj, bool selectIdentity = false, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertAsync(obj, selectIdentity, token));
        }

        /// <summary>
        /// Insert 1 or more POCOs in a transaction. E.g:
        /// <para>db.Insert(new Person { Id = 1, FirstName = "Tupac", LastName = "Shakur", Age = 25 },</para>
        /// <para>          new Person { Id = 2, FirstName = "Biggie", LastName = "Smalls", Age = 24 })</para>
        /// </summary>
        public static Task InsertAsync<T>(this IDbConnection dbConn, CancellationToken token, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertAsync(token, objs));
        }
        public static Task InsertAsync<T>(this IDbConnection dbConn, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertAsync(default(CancellationToken), objs));
        }

        /// <summary>
        /// Insert a collection of POCOs in a transaction. E.g:
        /// <para>db.InsertAll(new[] { new Person { Id = 9, FirstName = "Biggie", LastName = "Smalls", Age = 24 } })</para>
        /// </summary>
        public static Task InsertAllAsync<T>(this IDbConnection dbConn, IEnumerable<T> objs, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertAllAsync(objs, token));
        }

        /// <summary>
        /// Updates 1 POCO. All fields are updated except for the PrimaryKey which is used as the identity selector. E.g:
        /// <para>db.Update(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 })</para>
        /// </summary>
        public static Task<int> UpdateAsync<T>(this IDbConnection dbConn, T obj, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateAsync(obj, token));
        }

        /// <summary>
        /// Updates 1 or more POCOs in a transaction. E.g:
        /// <para>db.Update(new Person { Id = 1, FirstName = "Tupac", LastName = "Shakur", Age = 25 },</para>
        /// <para>new Person { Id = 2, FirstName = "Biggie", LastName = "Smalls", Age = 24 })</para>
        /// </summary>
        public static Task<int> UpdateAsync<T>(this IDbConnection dbConn, CancellationToken token, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateAsync(objs, token));
        }
        public static Task<int> UpdateAsync<T>(this IDbConnection dbConn, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateAsync(default(CancellationToken), objs));
        }

        /// <summary>
        /// Updates 1 or more POCOs in a transaction. E.g:
        /// <para>db.UpdateAll(new[] { new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 } })</para>
        /// </summary>
        public static Task<int> UpdateAllAsync<T>(this IDbConnection dbConn, IEnumerable<T> objs, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateAllAsync(objs, token));
        }

        /// <summary>
        /// Delete rows using an anonymous type filter. E.g:
        /// <para>db.Delete&lt;Person&gt;(new { FirstName = "Jimi", Age = 27 })</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static Task<int> DeleteAsync<T>(this IDbConnection dbConn, object anonFilter, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync<T>(anonFilter, token));
        }

        /// <summary>
        /// Delete 1 or more rows in a transaction using an anonymous type filter. E.g:
        /// <para>db.Delete&lt;Person&gt;(new { FirstName = "Jimi", Age = 27 }, new { FirstName = "Janis", Age = 27 })</para>
        /// </summary>
        public static Task<int> DeleteAsync<T>(this IDbConnection dbConn, CancellationToken token, params object[] anonFilters)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync<T>(token, anonFilters));
        }
        public static Task<int> DeleteAsync<T>(this IDbConnection dbConn, params object[] anonFilters)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync<T>(default(CancellationToken), anonFilters));
        }

        /// <summary>
        /// Delete 1 row using all fields in the filter. E.g:
        /// <para>db.Delete(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 })</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static Task<int> DeleteAsync<T>(this IDbConnection dbConn, T allFieldsFilter, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync<T>(allFieldsFilter, token));
        }

        /// <summary>
        /// Delete 1 or more rows in a transaction using all fields in the filter. E.g:
        /// <para>db.Delete(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 })</para>
        /// </summary>
        public static Task<int> DeleteAsync<T>(this IDbConnection dbConn, CancellationToken token = default(CancellationToken), params T[] allFieldsFilters)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync<T>(token, allFieldsFilters));
        }

        /// <summary>
        /// Delete 1 or more rows using only field with non-default values in the filter. E.g:
        /// <para>db.DeleteNonDefaults(new Person { FirstName = "Jimi", Age = 27 })</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static Task<int> DeleteNonDefaultsAsync<T>(this IDbConnection dbConn, T nonDefaultsFilter, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteNonDefaultsAsync(token, nonDefaultsFilter));
        }

        /// <summary>
        /// Delete 1 or more rows in a transaction using only field with non-default values in the filter. E.g:
        /// <para>db.DeleteNonDefaults(new Person { FirstName = "Jimi", Age = 27 }, 
        /// new Person { FirstName = "Janis", Age = 27 })</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static Task<int> DeleteNonDefaultsAsync<T>(this IDbConnection dbConn, CancellationToken token, params T[] nonDefaultsFilters)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteNonDefaultsAsync(token, nonDefaultsFilters));
        }
        public static Task<int> DeleteNonDefaultsAsync<T>(this IDbConnection dbConn, params T[] nonDefaultsFilters)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteNonDefaultsAsync(default(CancellationToken), nonDefaultsFilters));
        }

        /// <summary>
        /// Delete 1 row by the PrimaryKey. E.g:
        /// <para>db.DeleteById&lt;Person&gt;(1)</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static Task<int> DeleteByIdAsync<T>(this IDbConnection dbConn, object id, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteByIdAsync<T>(id, token));
        }

        /// <summary>
        /// Delete 1 row by the PrimaryKey where the rowVersion matches the optimistic concurrency field. 
        /// Will throw <exception cref="OptimisticConcurrencyException">RowModefiedExeption</exception> if the 
        /// row does not exist or has a different row version.
        /// E.g: <para>db.DeleteById&lt;Person&gt;(1)</para>
        /// </summary>
        public static Task DeleteByIdAsync<T>(this IDbConnection dbConn, object id, ulong rowVersion, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteByIdAsync<T>(id, rowVersion, token));
        }

        /// <summary>
        /// Delete all rows identified by the PrimaryKeys. E.g:
        /// <para>db.DeleteById&lt;Person&gt;(new[] { 1, 2, 3 })</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static Task<int> DeleteByIdsAsync<T>(this IDbConnection dbConn, IEnumerable idValues, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteByIdsAsync<T>(idValues, token));
        }

        /// <summary>
        /// Delete all rows in the generic table type. E.g:
        /// <para>db.DeleteAll&lt;Person&gt;()</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static Task<int> DeleteAllAsync<T>(this IDbConnection dbConn, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAllAsync<T>(token));
        }

        /// <summary>
        /// Delete all rows in the runtime table type. E.g:
        /// <para>db.DeleteAll(typeof(Person))</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static Task<int> DeleteAll(this IDbConnection dbConn, Type tableType, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAllAsync(tableType, token));
        }

        /// <summary>
        /// Delete rows using a SqlFormat filter. E.g:
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static Task<int> DeleteFmtAsync<T>(this IDbConnection dbConn, CancellationToken token, string sqlFilter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteFmtAsync<T>(token, sqlFilter, filterParams));
        }
        public static Task<int> DeleteFmtAsync<T>(this IDbConnection dbConn, string sqlFilter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteFmtAsync<T>(default(CancellationToken), sqlFilter, filterParams));
        }

        /// <summary>
        /// Delete rows from the runtime table type using a SqlFormat filter. E.g:
        /// </summary>
        /// <para>db.DeleteFmt(typeof(Person), "Age = {0}", 27)</para>
        /// <returns>number of rows deleted</returns>
        public static Task<int> DeleteFmtAsync(this IDbConnection dbConn, CancellationToken token, Type tableType, string sqlFilter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteFmtAsync(token, tableType, sqlFilter, filterParams));
        }
        public static Task<int> DeleteFmtAsync(this IDbConnection dbConn, Type tableType, string sqlFilter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteFmtAsync(default(CancellationToken), tableType, sqlFilter, filterParams));
        }

        /// <summary>
        /// Insert a new row or update existing row. Returns true if a new row was inserted. 
        /// Optional references param decides whether to save all related references as well. E.g:
        /// <para>db.SaveAsync(customer, references:true)</para>
        /// </summary>
        /// <returns>true if a row was inserted; false if it was updated</returns>
        public static Task<bool> SaveAsync<T>(this IDbConnection dbConn, T obj, bool references = false, CancellationToken token = default(CancellationToken))
        {
            if (!references)
                return dbConn.Exec(dbCmd => dbCmd.SaveAsync(obj, token));

            return dbConn.Exec(dbCmd => 
                dbCmd.SaveAsync(obj, token).Then(ret => 
                    dbCmd.SaveAllReferencesAsync(obj, token).Then(t => 
                        ret))) as Task<bool>;
        }

        /// <summary>
        /// Insert new rows or update existing rows. Return number of rows added E.g:
        /// <para>db.SaveAsync(new Person { Id = 10, FirstName = "Amy", LastName = "Winehouse", Age = 27 })</para>
        /// </summary>
        /// <returns>number of rows added</returns>
        public static Task<int> SaveAsync<T>(this IDbConnection dbConn, CancellationToken token, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.SaveAsync(token, objs));
        }
        public static Task<int> SaveAsync<T>(this IDbConnection dbConn, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.SaveAsync(default(CancellationToken), objs));
        }

        /// <summary>
        /// Insert new rows or update existing rows. Return number of rows added E.g:
        /// <para>db.SaveAllAsync(new [] { new Person { Id = 10, FirstName = "Amy", LastName = "Winehouse", Age = 27 } })</para>
        /// </summary>
        /// <returns>number of rows added</returns>
        public static Task<int> SaveAllAsync<T>(this IDbConnection dbConn, IEnumerable<T> objs, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.SaveAllAsync(objs, token));
        }

        /// <summary>
        /// Populates all related references on the instance with its primary key and saves them. Uses '(T)Id' naming convention. E.g:
        /// <para>db.SaveAllReferences(customer)</para> 
        /// </summary>
        public static Task SaveAllReferencesAsync<T>(this IDbConnection dbConn, T instance, CancellationToken token=default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.SaveAllReferencesAsync(instance, token));
        }

        /// <summary>
        /// Populates the related references with the instance primary key and saves them. Uses '(T)Id' naming convention. E.g:
        /// <para>db.SaveReference(customer, customer.Orders)</para> 
        /// </summary>
        public static Task SaveReferencesAsync<T, TRef>(this IDbConnection dbConn, CancellationToken token, T instance, params TRef[] refs)
        {
            return dbConn.Exec(dbCmd => dbCmd.SaveReferencesAsync(token, instance, refs));
        }
        public static Task SaveReferencesAsync<T, TRef>(this IDbConnection dbConn, T instance, params TRef[] refs)
        {
            return dbConn.Exec(dbCmd => dbCmd.SaveReferencesAsync(default(CancellationToken), instance, refs));
        }

        /// <summary>
        /// Populates the related references with the instance primary key and saves them. Uses '(T)Id' naming convention. E.g:
        /// <para>db.SaveReference(customer, customer.Orders)</para> 
        /// </summary>
        public static Task SaveReferencesAsync<T, TRef>(this IDbConnection dbConn, T instance, List<TRef> refs, CancellationToken token=default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.SaveReferencesAsync(token, instance, refs.ToArray()));
        }

        /// <summary>
        /// Populates the related references with the instance primary key and saves them. Uses '(T)Id' naming convention. E.g:
        /// <para>db.SaveReferences(customer, customer.Orders)</para> 
        /// </summary>
        public static Task SaveReferencesAsync<T, TRef>(this IDbConnection dbConn, T instance, IEnumerable<TRef> refs, CancellationToken token)
        {
            return dbConn.Exec(dbCmd => dbCmd.SaveReferencesAsync(token, instance, refs.ToArray()));
        }

        // Procedures
        public static Task ExecuteProcedureAsync<T>(this IDbConnection dbConn, T obj, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.ExecuteProcedureAsync(obj, token));
        }
    }
}
#endif