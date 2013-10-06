﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteReadConnectionExtensions
    {
        /// <summary>
        /// Returns results from the active connection.
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn) 
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<T>());
        }

        /// <summary>
        /// Returns results from using sql. E.g:
        /// <para>  - db.Select&lt;Person&gt;("Age > 40")</para>
        /// <para>  - db.Select&lt;Person&gt;("SELECT * FROM Person WHERE Age > 40")</para>
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, string sql)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<T>(sql));
        }

        /// <summary>
        /// Returns results from using a parameterized query. E.g:
        /// <para>  - db.Select&lt;Person&gt;("Age > @age", new { age = 40})</para>
        /// <para>  - db.Select&lt;Person&gt;("SELECT * FROM Person WHERE Age > @age", new { age = 40})</para>
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, string sql, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<T>(sql, anonType));
        }

        /// <summary>
        /// Returns results from using a parameterized query. E.g:
        /// <para>  - db.Select&lt;Person&gt;("Age > @age", new Dictionary&lt;string, object&gt; { { "age", 40 } })</para>
        /// <para>  - db.Select&lt;Person&gt;("SELECT * FROM Person WHERE Age > @age", new Dictionary&lt;string, object&gt; { { "age", 40 } })</para>
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, string sql, Dictionary<string, object> dict)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<T>(sql, dict));
        }

        /// <summary>
        /// Returns results from using an SqlFormat query. E.g:
        /// <para>  - db.SelectFmt&lt;Person&gt;("Age > {0}", 40)</para>
        /// <para>  - db.SelectFmt&lt;Person&gt;("SELECT * FROM Person WHERE Age > {0}", 40)</para>
        /// </summary>
        public static List<T> SelectFmt<T>(this IDbConnection dbConn, string sqlFormat, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectFmt<T>(sqlFormat, filterParams));
        }

        /// <summary>
        /// Returns a partial subset of results from the specified tableType. E.g:
        /// <para>  - db.Select&lt;EntityWithId&gt;(typeof(Person))</para>
        /// <para>  - </para>
        /// </summary>
        public static List<TModel> Select<TModel>(this IDbConnection dbConn, Type fromTableType)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<TModel>(fromTableType));
        }

        /// <summary>
        /// Returns a partial subset of results from the specified tableType using a SqlFormat query. E.g:
        /// <para>  - db.SelectFmt&lt;EntityWithId&gt;(typeof(Person), "Age > {0}", 40)</para>
        /// </summary>
        public static List<TModel> SelectFmt<TModel>(this IDbConnection dbConn, Type fromTableType, string sqlFormat, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectFmt<TModel>(fromTableType, sqlFormat, filterParams));
        }

        /// <summary>
        /// Returns results from using a single name, value filter. E.g:
        /// <para>  - db.Where<Person>("Age", 27)</para>
        /// </summary>
        public static List<T> Where<T>(this IDbConnection dbConn, string name, object value)
        {
            return dbConn.Exec(dbCmd => dbCmd.Where<T>(name, value));
        }

        /// <summary>
        /// Returns results from using an anonymous type filter. E.g:
        /// <para>  - db.Where<Person>(new { Age = 27 })</para>
        /// </summary>
        public static List<T> Where<T>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.Where<T>(anonType));
        }

        /// <summary>
        /// Returns results using the supplied primary key ids. E.g:
        /// <para>  - db.SelectByIds<Person>(new[] { 1, 2, 3 })</para>
        /// </summary>
        public static List<T> SelectByIds<T>(this IDbConnection dbConn, IEnumerable idValues)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectByIds<T>(idValues));
        }

        /// <summary>
        /// Query results using the non-default values in the supplied partially populated POCO example. E.g:
        /// <para>  - db.SelectByExample(new Person { Id = 1 })</para>
        /// </summary>
        public static List<T> SelectByExample<T>(this IDbConnection dbConn, T byExample)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectByExample<T>(byExample));
        }

        /// <summary>
        /// Query results using the non-default values in the supplied partially populated POCO example. E.g:
        /// <para>  - db.SelectByExample("Age > @Age", new Person { Age = 42 })</para>
        /// </summary>
        public static List<T> SelectByExample<T>(this IDbConnection dbConn, string sql, T byExample)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectByExample<T>(sql, byExample));
        }

        /// <summary>
        /// Returns a lazyily loaded stream of results. E.g:
        /// <para>  - db.SelectLazy<Person>()</para>
        /// </summary>
        public static IEnumerable<T> SelectLazy<T>(this IDbConnection dbConn)
        {
            return dbConn.ExecLazy(dbCmd => dbCmd.SelectLazy<T>());
        }

        /// <summary>
        /// Returns a lazyily loaded stream of results using a parameterized query. E.g:
        /// <para>  - db.SelectLazy<Person>("Age > @age", new { age = 40 })</para>
        /// </summary>
        public static IEnumerable<T> SelectLazy<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.ExecLazy(dbCmd => dbCmd.SelectLazy<T>(sql, anonType));
        }

        /// <summary>
        /// Returns a lazyily loaded stream of results using an SqlFilter query. E.g:
        /// <para>  - db.SelectLazyFmt<Person>("Age > {0}", 40)</para>
        /// </summary>
        public static IEnumerable<T> SelectLazyFmt<T>(this IDbConnection dbConn, string sqlFormat, params object[] filterParams)
        {
            return dbConn.ExecLazy(dbCmd => dbCmd.SelectLazyFmt<T>(sqlFormat, filterParams));
        }

        /// <summary>
        /// Returns a stream of results that are lazily loaded using a parameterized query. E.g:
        /// <para>  - db.WhereLazy<Person>(new { Age = 27 })</para>
        /// </summary>
        public static IEnumerable<T> WhereLazy<T>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.ExecLazy(dbCmd => dbCmd.WhereLazy<T>(anonType));
        }

        /// <summary>
        /// Returns the first result using a parameterized query. E.g:
        /// <para>  - db.Single<Person>(new { Age = 42 })</para>
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single<T>(anonType));
        }

        /// <summary>
        /// Returns results from using a single name, value filter. E.g:
        /// <para>  - db.Single<Person>("Age = @age", new { age = 42 })</para>
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single<T>(sql, anonType));
        }

        /// <summary>
        /// Returns the first result using a SqlFormat query. E.g:
        /// <para>  - db.SingleFmt<Person>("Age = {0}", 42)</para>
        /// </summary>
        public static T SingleFmt<T>(this IDbConnection dbConn, string sqlFormat, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.SingleFmt<T>(sqlFormat, filterParams));
        }

        /// <summary>
        /// Returns the first result using a primary key id. E.g:
        /// <para>  - db.SingleById<Person>(1)</para>
        /// </summary>
        public static T SingleById<T>(this IDbConnection dbConn, object idValue)
        {
            return dbConn.Exec(dbCmd => dbCmd.SingleById<T>(idValue));
        }

        /// <summary>
        /// Returns the first result using a name, value filter. E.g:
        /// <para>  - db.SingleWhere<Person>("Age", 42)</para>
        /// </summary>
        public static T SingleWhere<T>(this IDbConnection dbConn, string name, object value)
        {
            return dbConn.Exec(dbCmd => dbCmd.SingleWhere<T>(name, value));
        }

        /// <summary>
        /// Returns a single scalar value using a parameterized query. E.g:
        /// <para>  - db.Scalar<int>("SELECT COUNT(*) FROM Person WHERE Age > @age", new { age = 40 })</para>
        /// </summary>
        public static T Scalar<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.Scalar<T>(sql, anonType));
        }

        /// <summary>
        /// Returns a single scalar value using an SqlFormat query. E.g:
        /// <para>  - db.ScalarFmt<int>("SELECT COUNT(*) FROM Person WHERE Age > {0}", 40)</para>
        /// </summary>
        public static T ScalarFmt<T>(this IDbConnection dbConn, string sqlFormat, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.ScalarFmt<T>(sqlFormat, sqlParams));
        }

        /// <summary>
        /// Returns the first column in a List using a SqlFormat query. E.g:
        /// <para>  - db.Column<string>("SELECT LastName FROM Person WHERE Age = @age", new { age = 27 })</para>
        /// </summary>
        public static List<T> Column<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.Column<T>(sql, anonType));
        }

        /// <summary>
        /// Returns the first column in a List using a SqlFormat query. E.g:
        /// <para>  - db.ColumnFmt<string>("SELECT LastName FROM Person WHERE Age = {0}", 27)</para>
        /// </summary>
        public static List<T> ColumnFmt<T>(this IDbConnection dbConn, string sqlFormat, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.ColumnFmt<T>(sqlFormat, sqlParams));
        }

        /// <summary>
        /// Returns the distinct first column values in a HashSet using an SqlFormat query. E.g:
        /// <para>  - db.ColumnDistinct<int>("SELECT Age FROM Person WHERE Age &lt; @age", new { age = 50 })</para>
        /// </summary>
        public static HashSet<T> ColumnDistinct<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.ColumnDistinct<T>(sql, anonType));
        }

        /// <summary>
        /// Returns the distinct first column values in a HashSet using an SqlFormat query. E.g:
        /// <para>  - db.ColumnDistinctFmt<int>("SELECT Age FROM Person WHERE Age &lt; {0}", 50)</para>
        /// </summary>
        public static HashSet<T> ColumnDistinctFmt<T>(this IDbConnection dbConn, string sqlFormat, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.ColumnDistinctFmt<T>(sqlFormat, sqlParams));
        }

        /// <summary>
        /// Returns an Dictionary&lt;K, List&lt;V&gt;&gt; grouping made from the first two columns using an parameterized query. E.g:
        /// <para>  - db.Lookup<int, string>("SELECT Age, LastName FROM Person WHERE Age &lt; @age", new { age = 50 })</para>
        /// </summary>
        public static Dictionary<K, List<V>> Lookup<K, V>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.Lookup<K, V>(sql, anonType));
        }

        /// <summary>
        /// Returns an Dictionary&lt;K, List&lt;V&gt;&gt; grouping made from the first two columns using an SqlFormat query. E.g:
        /// <para>  - db.LookupFmt<int, string>("SELECT Age, LastName FROM Person WHERE Age &lt; {0}", 50)</para>
        /// </summary>
        public static Dictionary<K, List<V>> LookupFmt<K, V>(this IDbConnection dbConn, string sqlFormat, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.LookupFmt<K, V>(sqlFormat, sqlParams));
        }

        /// <summary>
        /// Returns a Dictionary from the first 2 columns: Column 1 (Keys), Column 2 (Values) using sql. E.g:
        /// <para>  - db.Dictionary<int, string>("SELECT Id, LastName FROM Person WHERE Age &lt; @age", new { age = 50 })</para>
        /// </summary>
        public static Dictionary<K, V> Dictionary<K, V>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.Dictionary<K, V>(sql, anonType));
        }

        /// <summary>
        /// Returns a Dictionary from the first 2 columns: Column 1 (Keys), Column 2 (Values) using an SqlFormat query. E.g:
        /// <para>  - db.DictionaryFmt<int, string>("SELECT Id, LastName FROM Person WHERE Age &lt; {0}", 50)</para>
        /// </summary>
        public static Dictionary<K, V> DictionaryFmt<K, V>(this IDbConnection dbConn, string sqlFormat, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.DictionaryFmt<K, V>(sqlFormat, sqlParams));
        }

        /// <summary>
        /// Returns true if the Query returns any records, using a parameterized query. E.g:
        /// <para>  - db.Exists<Person>("Age = @age", new { age = 42 })</para>
        /// <para>  - db.Exists<Person>("SELECT * FROM Person WHERE Age = @age", new { age = 42 })</para>
        /// </summary>
        public static bool Exists<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.Exists<T>(sql, anonType));
        }

        /// <summary>
        /// Returns true if the Query returns any records, using an SqlFormat query. E.g:
        /// <para>  - db.ExistsFmt<Person>("Age = {0}", 42)</para>
        /// <para>  - db.ExistsFmt<Person>("SELECT * FROM Person WHERE Age = {0}", 42)</para>
        /// </summary>
        public static bool ExistsFmt<T>(this IDbConnection dbConn, string sqlFormat, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.ExistsFmt<T>(sqlFormat, filterParams));
        }
        
        /// <summary>
        /// Returns the first column in a List using a parameterized query. E.g:
        /// <para>  - db.SqlColumn<string>("SELECT LastName FROM Person WHERE Age &lt; @age", new { age = 50 })</para>
        /// </summary>
        public static List<T> SqlColumn<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlColumn<T>(sql, anonType));
        }

        /// <summary>
        /// Returns a single Scalar value using a parameterized query. E.g:
        /// <para>  - db.SqlColumn<string>("SELECT LastName FROM Person WHERE Age &lt; @age", new Dictionary<string, object> { { "age", 50 } })</para>
        /// </summary>
        public static List<T> SqlColumn<T>(this IDbConnection dbConn, string sql, Dictionary<string, object> dict)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlColumn<T>(sql, dict));
        }

        /// <summary>
        /// Returns a single Scalar value using a parameterized query. E.g:
        /// <para>  - db.SqlScalar<int>("SELECT COUNT(*) FROM Person WHERE Age &lt; @age", new { age = 50 })</para>
        /// </summary>
        public static T SqlScalar<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlScalar<T>(sql, anonType));
        }

        /// <summary>
        /// Returns a single Scalar value using a parameterized query. E.g:
        /// <para>  - db.SqlScalar<int>("SELECT COUNT(*) FROM Person WHERE Age &lt; @age", new Dictionary<string, object> { { "age", 50 } })</para>
        /// </summary>
        public static T SqlScalar<T>(this IDbConnection dbConn, string sql, Dictionary<string, object> dict)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlScalar<T>(sql, dict));
        }

        /// <summary>
        /// Returns the last insert Id made from this connection.
        /// </summary>
        public static long LastInsertId(this IDbConnection dbConn)
        {
            return dbConn.Exec(dbCmd => dbCmd.LastInsertId());
        }

        /// <summary>
        /// Executes a raw sql non-query using sql. E.g:
        /// <para>  - var rowsAffected = db.ExecuteNonQuery("UPDATE Person SET LastName={0} WHERE Id={1}".SqlFormat("WaterHouse", 7))</para>
        /// </summary>
        /// <returns>number of rows affected</returns>
        public static int ExecuteNonQuery(this IDbConnection dbConn, string sql)
        {
            return dbConn.Exec(dbCmd => dbCmd.ExecuteNonQuery(sql));
        }

        /// <summary>
        /// Executes a raw sql non-query using a parameterized query. E.g:
        /// <para>  - var rowsAffected = db.ExecuteNonQuery("UPDATE Person SET LastName=@name WHERE Id=@id", new { name = "WaterHouse", id = 7 })</para>
        /// </summary>
        /// <returns>number of rows affected</returns>
        public static int ExecuteNonQuery(this IDbConnection dbConn, string sql, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.ExecuteNonQuery(sql, anonType));
        }

        /// <summary>
        /// Executes a raw sql non-query using a parameterized query.
        /// </summary>
        /// <returns>number of rows affected</returns>
        public static int ExecuteNonQuery(this IDbConnection dbConn, string sql, Dictionary<string, object> dict)
        {
            return dbConn.Exec(dbCmd => dbCmd.ExecuteNonQuery(sql, dict));
        }

        /// <summary>
        /// Returns results from a Stored Procedure, using a parameterized query.
        /// </summary>
        public static List<TOutputModel> SqlProcedure<TOutputModel>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlProcedure<TOutputModel>(anonType));
        }

        /// <summary>
        /// Returns results from a Stored Procedure using an SqlFormat query. E.g:
        /// <para>  - </para>
        /// </summary>
        public static List<TOutputModel> SqlProcedure<TOutputModel>(this IDbConnection dbConn,
            object anonType,
            string sqlFilter,
            params object[] filterParams)
            where TOutputModel : new()
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlProcedureFmt<TOutputModel>(
                anonType, sqlFilter, filterParams));
        }

        /// <summary>
        /// Returns the scalar result as a long.
        /// </summary>
        public static long LongScalar(this IDbConnection dbConn)
        {
            return dbConn.Exec(dbCmd => dbCmd.LongScalar());
        }			
    }
}
