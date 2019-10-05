﻿#if ASYNC
// Copyright (c) ServiceStack, Inc. All Rights Reserved.
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
        /// Execute any arbitrary raw SQL with db params.
        /// </summary>
        /// <returns>number of rows affected</returns>
        public static Task<int> ExecuteSqlAsync(this IDbConnection dbConn, string sql, object dbParams, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.ExecuteSqlAsync(sql, dbParams, token));
        }

        /// <summary>
        /// Insert 1 POCO, use selectIdentity to retrieve the last insert AutoIncrement id (if any). E.g:
        /// <para>var id = db.Insert(new Person { Id = 1, FirstName = "Jimi }, selectIdentity:true)</para>
        /// </summary>
        public static Task<long> InsertAsync<T>(this IDbConnection dbConn, T obj, bool selectIdentity = false, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertAsync(obj, commandFilter: null, selectIdentity: selectIdentity, token: token));
        }

        /// <summary>
        /// Insert 1 POCO, use selectIdentity to retrieve the last insert AutoIncrement id (if any). E.g:
        /// <para>var id = db.Insert(new Person { Id = 1, FirstName = "Jimi }, selectIdentity:true)</para>
        /// </summary>
        public static Task<long> InsertAsync<T>(this IDbConnection dbConn, T obj, Action<IDbCommand> commandFilter, bool selectIdentity = false, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertAsync(obj, commandFilter: commandFilter, selectIdentity: selectIdentity, token: token));
        }

        /// <summary>
        /// Insert 1 or more POCOs in a transaction. E.g:
        /// <para>db.InsertAsync(new Person { Id = 1, FirstName = "Tupac", LastName = "Shakur", Age = 25 },</para>
        /// <para>               new Person { Id = 2, FirstName = "Biggie", LastName = "Smalls", Age = 24 })</para>
        /// </summary>
        public static Task InsertAsync<T>(this IDbConnection dbConn, CancellationToken token, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertAsync(commandFilter:null, token:token, objs:objs));
        }
        public static Task InsertAsync<T>(this IDbConnection dbConn, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertAsync(commandFilter:null, token:default(CancellationToken), objs:objs));
        }

        /// <summary>
        /// Insert 1 or more POCOs in a transaction and modify populated IDbCommand with a commandFilter. E.g:
        /// <para>db.InsertAsync(dbCmd => applyFilter(dbCmd), token, </para>
        /// <para>               new Person { Id = 1, FirstName = "Tupac", LastName = "Shakur", Age = 25 },</para>
        /// <para>               new Person { Id = 2, FirstName = "Biggie", LastName = "Smalls", Age = 24 })</para>
        /// </summary>
        public static Task InsertAsync<T>(this IDbConnection dbConn, Action<IDbCommand> commandFilter, CancellationToken token, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertAsync(commandFilter:commandFilter, token:token, objs:objs));
        }

        /// <summary>
        /// Insert 1 or more POCOs in a transaction using Table default values when defined. E.g:
        /// <para>db.InsertUsingDefaultsAsync(new Person { FirstName = "Tupac", LastName = "Shakur" },</para>
        /// <para>                            new Person { FirstName = "Biggie", LastName = "Smalls" })</para>
        /// </summary>
        public static Task InsertUsingDefaultsAsync<T>(this IDbConnection dbConn, T[] objs, CancellationToken token=default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertUsingDefaultsAsync(objs, token));
        }

        /// <summary>
        /// Insert results from SELECT SqlExpression, use selectIdentity to retrieve the last insert AutoIncrement id (if any). E.g:
        /// <para>db.InsertIntoSelectAsync&lt;Contact&gt;(db.From&lt;Person&gt;().Select(x => new { x.Id, Surname == x.LastName }))</para>
        /// </summary>
        public static Task<long> InsertIntoSelectAsync<T>(this IDbConnection dbConn, ISqlExpression query, CancellationToken token=default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertIntoSelectAsync<T>(query, commandFilter: null, token:token));
        }

        /// <summary>
        /// Insert results from SELECT SqlExpression, use selectIdentity to retrieve the last insert AutoIncrement id (if any). E.g:
        /// <para>db.InsertIntoSelectAsync&lt;Contact&gt;(db.From&lt;Person&gt;().Select(x => new { x.Id, Surname == x.LastName }))</para>
        /// </summary>
        public static Task<long> InsertIntoSelectAsync<T>(this IDbConnection dbConn, ISqlExpression query, Action<IDbCommand> commandFilter, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertIntoSelectAsync<T>(query, commandFilter: commandFilter, token:token));
        }

        
        /// <summary>
        /// Insert a collection of POCOs in a transaction. E.g:
        /// <para>db.InsertAllAsync(new[] { new Person { Id = 9, FirstName = "Biggie", LastName = "Smalls", Age = 24 } })</para>
        /// </summary>
        public static Task InsertAllAsync<T>(this IDbConnection dbConn, IEnumerable<T> objs, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertAllAsync(objs, commandFilter:null, token:token));
        }

        /// <summary>
        /// Insert a collection of POCOs in a transaction and modify populated IDbCommand with a commandFilter. E.g:
        /// <para>db.InsertAllAsync(new[] { new Person { Id = 9, FirstName = "Biggie", LastName = "Smalls", Age = 24 } },</para>
        /// <para>                  dbCmd => applyFilter(dbCmd))</para>
        /// </summary>
        public static Task InsertAllAsync<T>(this IDbConnection dbConn, IEnumerable<T> objs, Action<IDbCommand> commandFilter, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertAllAsync(objs, commandFilter:commandFilter, token:token));
        }

        /// <summary>
        /// Updates 1 POCO. All fields are updated except for the PrimaryKey which is used as the identity selector. E.g:
        /// <para>db.Update(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 })</para>
        /// </summary>
        public static Task<int> UpdateAsync<T>(this IDbConnection dbConn, T obj, Action<IDbCommand> commandFilter = null, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateAsync(obj, token, commandFilter));
        }

        /// <summary>
        /// Updates 1 or more POCOs in a transaction. E.g:
        /// <para>db.Update(new Person { Id = 1, FirstName = "Tupac", LastName = "Shakur", Age = 25 },</para>
        /// <para>new Person { Id = 2, FirstName = "Biggie", LastName = "Smalls", Age = 24 })</para>
        /// </summary>
        public static Task<int> UpdateAsync<T>(this IDbConnection dbConn, CancellationToken token, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateAsync(commandFilter:null, token: token, objs: objs));
        }
        public static Task<int> UpdateAsync<T>(this IDbConnection dbConn, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateAsync(commandFilter: null, token: default(CancellationToken), objs: objs));
        }
        public static Task<int> UpdateAsync<T>(this IDbConnection dbConn, Action<IDbCommand> commandFilter, CancellationToken token, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateAsync(commandFilter: commandFilter, token: token, objs: objs));
        }
        public static Task<int> UpdateAsync<T>(this IDbConnection dbConn, Action<IDbCommand> commandFilter, params T[] objs)
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateAsync(commandFilter: commandFilter, token:default(CancellationToken), objs: objs));
        }

        /// <summary>
        /// Updates 1 or more POCOs in a transaction. E.g:
        /// <para>db.UpdateAll(new[] { new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 } })</para>
        /// </summary>
        public static Task<int> UpdateAllAsync<T>(this IDbConnection dbConn, IEnumerable<T> objs, Action<IDbCommand> commandFilter = null, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateAllAsync(objs, commandFilter, token));
        }

        /// <summary>
        /// Delete rows using an anonymous type commandFilter. E.g:
        /// <para>db.Delete&lt;Person&gt;(new { FirstName = "Jimi", Age = 27 })</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static Task<int> DeleteAsync<T>(this IDbConnection dbConn, object anonFilter, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync<T>(anonFilter, token));
        }

        /// <summary>
        /// Delete 1 row using all fields in the commandFilter. E.g:
        /// <para>db.Delete(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 })</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static Task<int> DeleteAsync<T>(this IDbConnection dbConn, T allFieldsFilter, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync(allFieldsFilter, token));
        }

        /// <summary>
        /// Delete 1 or more rows in a transaction using all fields in the commandFilter. E.g:
        /// <para>db.Delete(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 })</para>
        /// </summary>
        public static Task<int> DeleteAsync<T>(this IDbConnection dbConn, CancellationToken token = default(CancellationToken), params T[] allFieldsFilters)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync(token, allFieldsFilters));
        }
        public static Task<int> DeleteAsync<T>(this IDbConnection dbConn, params T[] allFieldsFilters)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync(default(CancellationToken), allFieldsFilters));
        }

        /// <summary>
        /// Delete 1 or more rows using only field with non-default values in the commandFilter. E.g:
        /// <para>db.DeleteNonDefaults(new Person { FirstName = "Jimi", Age = 27 })</para>
        /// </summary>
        /// <returns>number of rows deleted</returns>
        public static Task<int> DeleteNonDefaultsAsync<T>(this IDbConnection dbConn, T nonDefaultsFilter, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteNonDefaultsAsync(nonDefaultsFilter, token));
        }

        /// <summary>
        /// Delete 1 or more rows in a transaction using only field with non-default values in the commandFilter. E.g:
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

        public static Task<int> DeleteAsync<T>(this IDbConnection dbConn, string sqlFilter, object anonType, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync<T>(sqlFilter, anonType, token));
        }

        public static Task<int> DeleteAsync(this IDbConnection dbConn, Type tableType, string sqlFilter, object anonType, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync(tableType, sqlFilter, anonType, token));
        }

        /// <summary>
        /// Insert a new row or update existing row. Returns true if a new row was inserted. 
        /// Optional references param decides whether to save all related references as well. E.g:
        /// <para>db.SaveAsync(customer, references:true)</para>
        /// </summary>
        /// <returns>true if a row was inserted; false if it was updated</returns>
        public static async Task<bool> SaveAsync<T>(this IDbConnection dbConn, T obj, bool references = false, CancellationToken token = default(CancellationToken))
        {
            if (!references)
                return await dbConn.Exec(dbCmd => dbCmd.SaveAsync(obj, token));

            return await dbConn.Exec(async dbCmd =>
            {
                var ret = await dbCmd.SaveAsync(obj, token);
                await dbCmd.SaveAllReferencesAsync(obj, token);
                return ret;
            });
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