using System;
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
        /// Returns results from using sql.
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, string sql)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<T>(sql));
        }

        /// <summary>
        /// Returns results from using a parameterized query.
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, string sql, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<T>(sql, anonType));
        }

        /// <summary>
        /// Returns results from using a parameterized query.
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, string sql, Dictionary<string, object> dict)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<T>(sql, dict));
        }

        /// <summary>
        /// Returns results from using an SqlFormat query.
        /// </summary>
        public static List<T> SelectFmt<T>(this IDbConnection dbConn, string sqlFilter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectFmt<T>(sqlFilter, filterParams));
        }

        /// <summary>
        /// Returns a subset of results from the specified tableType.
        /// </summary>
        public static List<TModel> Select<TModel>(this IDbConnection dbConn, Type fromTableType)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<TModel>(fromTableType));
        }

        /// <summary>
        /// Returns a subset of results from the specified tableType using a SqlFormat query.
        /// </summary>
        public static List<TModel> SelectFmt<TModel>(this IDbConnection dbConn, Type fromTableType, string sqlFilter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectFmt<TModel>(fromTableType, sqlFilter, filterParams));
        }

        /// <summary>
        /// Returns results from using a single name, value filter.
        /// </summary>
        public static List<T> Where<T>(this IDbConnection dbConn, string name, object value)
        {
            return dbConn.Exec(dbCmd => dbCmd.Where<T>(name, value));
        }

        /// <summary>
        /// Returns results from using an anonymous type filter.
        /// </summary>
        public static List<T> Where<T>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.Where<T>(anonType));
        }

        /// <summary>
        /// Returns results using the supplied primary key ids.
        /// </summary>
        public static List<T> SelectByIds<T>(this IDbConnection dbConn, IEnumerable idValues)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectByIds<T>(idValues));
        }

        /// <summary>
        /// Query results using the non-default values in the supplied partially populated POCO example.
        /// </summary>
        public static List<T> SelectByExample<T>(this IDbConnection dbConn, T byExample)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectByExample<T>(byExample));
        }

        /// <summary>
        /// Query results using the non-default values in the supplied partially populated POCO example.
        /// </summary>
        public static List<T> SelectByExample<T>(this IDbConnection dbConn, string sql, T byExample)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectByExample<T>(sql, byExample));
        }

        /// <summary>
        /// Returns a lazyily loaded stream of results.
        /// </summary>
        public static IEnumerable<T> Lazy<T>(this IDbConnection dbConn)
        {
            return dbConn.ExecLazy(dbCmd => dbCmd.Lazy<T>());
        }

        /// <summary>
        /// Returns a lazyily loaded stream of results using a parameterized query.
        /// </summary>
        public static IEnumerable<T> Lazy<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.ExecLazy(dbCmd => dbCmd.Lazy<T>(sql, anonType));
        }

        /// <summary>
        /// Returns a stream of results that are lazily loaded using a parameterized query.
        /// </summary>
        public static IEnumerable<T> LazyWhere<T>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.ExecLazy(dbCmd => dbCmd.LazyWhere<T>(anonType));
        }

        /// <summary>
        /// Returns a lazyily loaded stream of results using an SqlFilter query.
        /// </summary>
        public static IEnumerable<T> LazyFmt<T>(this IDbConnection dbConn, string filter, params object[] filterParams)
        {
            return dbConn.ExecLazy(dbCmd => dbCmd.LazyFmt<T>(filter, filterParams));
        }

        /// <summary>
        /// Returns the first result using a SqlFormat query.
        /// </summary>
        public static T SingleFmt<T>(this IDbConnection dbConn, string filter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.SingleFmt<T>(filter, filterParams));
        }

        /// <summary>
        /// Returns the first result using a primary key id.
        /// </summary>
        public static T SingleById<T>(this IDbConnection dbConn, object idValue)
        {
            return dbConn.Exec(dbCmd => dbCmd.SingleById<T>(idValue));
        }

        /// <summary>
        /// Returns the first result using a name, value filter.
        /// </summary>
        public static T SingleWhere<T>(this IDbConnection dbConn, string name, object value)
        {
            return dbConn.Exec(dbCmd => dbCmd.SingleWhere<T>(name, value));
        }

        /// <summary>
        /// Returns the first result using a parameterized query.
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single<T>(anonType));
        }

        /// <summary>
        /// Returns results from using a single name, value filter.
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single<T>(sql, anonType));
        }

        /// <summary>
        /// Returns a single scalar value using a parameterized query.
        /// </summary>
        public static T Scalar<T>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.Scalar<T>(anonType));
        }

        /// <summary>
        /// Returns a single scalar value using a parameterized query.
        /// </summary>
        public static T Scalar<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.Scalar<T>(sql, anonType));
        }

        /// <summary>
        /// Returns a single scalar value using an SqlFormat query.
        /// </summary>
        public static T ScalarFmt<T>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.ScalarFmt<T>(sql, sqlParams));
        }

        /// <summary>
        /// Returns the first column in a List using sql.
        /// </summary>
        public static List<T> SqlColumn<T>(this IDbConnection dbConn, string sql)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlColumn<T>(sql));
        }

        /// <summary>
        /// Returns the first column in a List using a parameterized query.
        /// </summary>
        public static List<T> SqlColumn<T>(this IDbConnection dbConn, string sql, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlColumn<T>(sql, anonType));
        }

        /// <summary>
        /// Returns a single Scalar value using a parameterized query.
        /// </summary>
        public static List<T> SqlColumn<T>(this IDbConnection dbConn, string sql, Dictionary<string, object> dict)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlColumn<T>(sql, dict));
        }

        /// <summary>
        /// Returns a single Scalar value using a parameterized query.
        /// </summary>
        public static T SqlScalar<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlScalar<T>(sql, anonType));
        }

        /// <summary>
        /// Returns a single Scalar value using a parameterized query.
        /// </summary>
        public static T SqlScalar<T>(this IDbConnection dbConn, string sql, Dictionary<string, object> dict)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlScalar<T>(sql, dict));
        }

        /// <summary>
        /// Returns the first column in a List using a SqlFormat query.
        /// </summary>
        public static List<T> Column<T>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.Column<T>(sql, sqlParams));
        }

        /// <summary>
        /// Returns the distinct first column values in a HashSet using an SqlFormat query.
        /// </summary>
        public static HashSet<T> ColumnDistinctFmt<T>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.ColumnDistinctFmt<T>(sql, sqlParams));
        }

        /// <summary>
        /// Returns an Dictionary{K, List{V}} grouping made from the first two columns using an SqlFormat query.
        /// </summary>
        public static Dictionary<K, List<V>> LookupFmt<K, V>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.LookupFmt<K, V>(sql, sqlParams));
        }

        /// <summary>
        /// Returns a Dictionary from the first 2 columns: Column 1 (Keys), Column 2 (Values) using sql.
        /// </summary>
        public static Dictionary<K, V> Dictionary<K, V>(this IDbConnection dbConn, string sql)
        {
            return dbConn.Exec(dbCmd => dbCmd.Dictionary<K, V>(sql));
        }

        /// <summary>
        /// Returns a Dictionary from the first 2 columns: Column 1 (Keys), Column 2 (Values) using an SqlFormat query.
        /// </summary>
        public static Dictionary<K, V> DictionaryFmt<K, V>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.DictionaryFmt<K, V>(sql, sqlParams));
        }

        /// <summary>
        /// Returns true if the Query returns any records, using an SqlFormat query.
        /// </summary>
        public static bool ExistsFmt<T>(this IDbConnection dbConn, string sqlFilter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.ExistsFmt<T>(null, sqlFilter, filterParams));
        }

        /// <summary>
        /// Returns true if the Query returns any records, using a parameterized query.
        /// </summary>
        public static bool Exists<T>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.Exists<T>(anonType));
        }

        /// <summary>
        /// Returns the last insert Id made from this connection.
        /// </summary>
        public static long LastInsertId(this IDbConnection dbConn)
        {
            return dbConn.Exec(dbCmd => dbCmd.LastInsertId());
        }

        /// <summary>
        /// Executes a raw sql non-query using sql.
        /// </summary>
        /// <returns>number of rows affected</returns>
        public static int ExecuteNonQuery(this IDbConnection dbConn, string sql)
        {
            return dbConn.Exec(dbCmd => dbCmd.ExecuteNonQuery(sql));
        }

        /// <summary>
        /// Executes a raw sql non-query using a parameterized query.
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
        /// Returns results from a Stored Procedure using an SqlFormat query.
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
