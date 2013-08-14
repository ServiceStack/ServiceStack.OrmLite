using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteReadConnectionExtensions
    {
        public static List<T> Select<T>(this IDbConnection dbConn) 
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<T>());
        }

        public static List<T> Select<T>(this IDbConnection dbConn, string sqlFilter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<T>(sqlFilter, filterParams));
        }

        public static List<TModel> Select<TModel>(this IDbConnection dbConn, Type fromTableType)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<TModel>(fromTableType));
        }

        public static List<TModel> Select<TModel>(this IDbConnection dbConn, Type fromTableType, string sqlFilter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<TModel>(fromTableType, sqlFilter, filterParams));
        }

        public static IEnumerable<T> Each<T>(this IDbConnection dbConn)
        {
            return dbConn.ExecLazy(dbCmd => dbCmd.Each<T>());
        }

        public static IEnumerable<T> Each<T>(this IDbConnection dbConn, string filter, params object[] filterParams)
        {
            return dbConn.ExecLazy(dbCmd => dbCmd.Each<T>(filter, filterParams));
        }
        
        public static T First<T>(this IDbConnection dbConn, string filter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.First<T>(filter, filterParams));
        }

        /// <summary>
        /// Alias for First
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, string filter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.First<T>(filter, filterParams));
        }

        public static T First<T>(this IDbConnection dbConn, string filter)
        {
            return dbConn.Exec(dbCmd => dbCmd.First<T>(filter));
        }

        /// <summary>
        /// Alias for First
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, string filter)
        {
            return dbConn.Exec(dbCmd => dbCmd.First<T>(filter));
        }

        public static T FirstOrDefault<T>(this IDbConnection dbConn, string filter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.FirstOrDefault<T>(filter, filterParams));
        }

        /// <summary>
        /// Alias for FirstOrDefault
        /// </summary>
        public static T SingleOrDefault<T>(this IDbConnection dbConn, string filter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.FirstOrDefault<T>(filter, filterParams));
        }

        public static T FirstOrDefault<T>(this IDbConnection dbConn, string filter)
        {
            return dbConn.Exec(dbCmd => dbCmd.FirstOrDefault<T>(filter));
        }

        /// <summary>
        /// Alias for FirstOrDefault
        /// </summary>
        public static T SingleOrDefault<T>(this IDbConnection dbConn, string filter)
        {
            return dbConn.Exec(dbCmd => dbCmd.FirstOrDefault<T>(filter));
        }

        public static T GetById<T>(this IDbConnection dbConn, object idValue)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetById<T>(idValue));
        }

        /// <summary>
        /// Performs an GetById() except argument is passed as a parameter to the generated SQL
        /// </summary>
        public static T GetByIdParam<T>(this IDbConnection dbConn, object idValue)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetByIdParam<T>(idValue));
        }

        /// <summary>
        /// Alias for GetById
        /// </summary>
        public static T Id<T>(this IDbConnection dbConn, object idValue)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetById<T>(idValue));
        }
        
        public static T QueryById<T>(this IDbConnection dbConn, object value) 
        {
            return dbConn.Exec(dbCmd => dbCmd.QueryById<T>(value));
        }

        public static T SingleWhere<T>(this IDbConnection dbConn, string name, object value)
        {
            return dbConn.Exec(dbCmd => dbCmd.SingleWhere<T>(name, value));
        }

        public static T QuerySingle<T>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.QuerySingle<T>(anonType));
        }

        public static T QuerySingle<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.QuerySingle<T>(sql, anonType));
        }

        public static List<T> Where<T>(this IDbConnection dbConn, string name, object value)
        {
            return dbConn.Exec(dbCmd => dbCmd.Where<T>(name, value));
        }

        public static List<T> Where<T>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.Where<T>(anonType));
        }

        public static List<T> Where<T>(this IDbConnection dbConn, System.Linq.Expressions.Expression<Func<T,bool>> Predicate)
        {
            return (List<T>) dbConn.Select<T>(Predicate);
        }

        public static List<T> Query<T>(this IDbConnection dbConn, string sql)
        {
            return dbConn.Exec(dbCmd => dbCmd.Query<T>(sql));
        }

        public static List<T> Query<T>(this IDbConnection dbConn, string sql, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.Query<T>(sql, anonType));
        }

        public static List<T> Query<T>(this IDbConnection dbConn, string sql, Dictionary<string, object> dict)
        {
            return dbConn.Exec(dbCmd => dbCmd.Query<T>(sql, dict));
        }

        public static int ExecuteNonQuery(this IDbConnection dbConn, string sql)
        {
            return dbConn.Exec(dbCmd => dbCmd.ExecuteNonQuery(sql));
        }

        public static int ExecuteNonQuery(this IDbConnection dbConn, string sql, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.ExecuteNonQuery(sql, anonType));
        }

        public static int ExecuteNonQuery(this IDbConnection dbConn, string sql, Dictionary<string, object> dict)
        {
            return dbConn.Exec(dbCmd => dbCmd.ExecuteNonQuery(sql, dict));
        }

        public static T QueryScalar<T>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.QueryScalar<T>(anonType));
        }

        public static T QueryScalar<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.QueryScalar<T>(sql, anonType));
        }
        
        public static List<T> SqlList<T>(this IDbConnection dbConn, string sql)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlList<T>(sql));
        }

        public static List<T> SqlList<T>(this IDbConnection dbConn, string sql, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlList<T>(sql, anonType));
        }

        public static List<T> SqlList<T>(this IDbConnection dbConn, string sql, Dictionary<string, object> dict)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlList<T>(sql, dict));
        }

        public static T SqlScalar<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlScalar<T>(sql, anonType));
        }

        public static T SqlScalar<T>(this IDbConnection dbConn, string sql, Dictionary<string, object> dict)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlScalar<T>(sql, dict));
        }
        
        public static List<T> ByExampleWhere<T>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.Exec(dbCmd => dbCmd.ByExampleWhere<T>(anonType));
        }

        public static List<T> ByExampleWhere<T>(this IDbConnection dbConn, object anonType, bool excludeNulls)
        {
            return dbConn.Exec(dbCmd => dbCmd.ByExampleWhere<T>(anonType, excludeNulls));
        }

        public static List<T> QueryByExample<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.QueryByExample<T>(sql, anonType));
        }

        public static IEnumerable<T> QueryEach<T>(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.ExecLazy(dbCmd => dbCmd.QueryEach<T>(sql, anonType));
        }

        public static IEnumerable<T> EachWhere<T>(this IDbConnection dbConn, object anonType)
        {
            return dbConn.ExecLazy(dbCmd => dbCmd.EachWhere<T>(anonType));
        }

        public static T GetByIdOrDefault<T>(this IDbConnection dbConn, object idValue)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetByIdOrDefault<T>(idValue));
        }

        /// <summary>
        /// Alias for GetByIds
        /// </summary>
        public static T IdOrDefault<T>(this IDbConnection dbConn, object idValue)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetByIdOrDefault<T>(idValue));
        }

        public static List<T> GetByIds<T>(this IDbConnection dbConn, IEnumerable idValues)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetByIds<T>(idValues));
        }

        /// <summary>
        /// Alias for GetByIds
        /// </summary>
        public static List<T> Ids<T>(this IDbConnection dbConn, IEnumerable idValues)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetByIds<T>(idValues));
        }

        public static T GetScalar<T>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetScalar<T>(sql, sqlParams));
        }

        /// <summary>
        /// Alias for Scalar
        /// </summary>
        public static T Scalar<T>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetScalar<T>(sql, sqlParams));
        }

        public static long GetLastInsertId(this IDbConnection dbConn)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetLastInsertId());
        }

        public static List<T> GetFirstColumn<T>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetFirstColumn<T>(sql, sqlParams));
        }

        public static List<T> GetList<T>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetList<T>(sql, sqlParams));
        }

        /// <summary>
        /// Alias for GetList
        /// </summary>
        public static List<T> List<T>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetList<T>(sql, sqlParams));
        }

        public static HashSet<T> GetFirstColumnDistinct<T>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetFirstColumnDistinct<T>(sql, sqlParams));
        }

        public static HashSet<T> GetHashSet<T>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetHashSet<T>(sql, sqlParams));
        }

        /// <summary>
        /// Alias for GetHashSet
        /// </summary>
        public static HashSet<T> HashSet<T>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetHashSet<T>(sql, sqlParams));
        }

        public static Dictionary<K, List<V>> GetLookup<K, V>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetLookup<K, V>(sql, sqlParams));
        }

        /// <summary>
        /// Alias for GetLookup
        /// </summary>
        public static Dictionary<K, List<V>> Lookup<K, V>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetLookup<K, V>(sql, sqlParams));
        }

        public static Dictionary<K, V> GetDictionary<K, V>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetDictionary<K, V>(sql, sqlParams));
        }

        /// <summary>
        /// Alias for GetDictionary
        /// </summary>
        public static Dictionary<K, V> Dictionary<K, V>(this IDbConnection dbConn, string sql, params object[] sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetDictionary<K, V>(sql, sqlParams));
        }

        // somo aditional methods

        public static bool HasChildren<T>(this IDbConnection dbConn, object record)
        {
            return dbConn.Exec(dbCmd => dbCmd.HasChildren<T>(record));
        }

        public static bool Exists<T>(this IDbConnection dbConn, string sqlFilter, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.Exists<T>(sqlFilter, filterParams));
        }

        public static bool Exists<T>(this IDbConnection dbConn, object record)
        {
            return dbConn.Exec(dbCmd => dbCmd.Exists<T>(record));
        }

        // procedures ...		
        public static List<TOutputModel> SelectFromProcedure<TOutputModel>(this IDbConnection dbConn,
            object fromObjWithProperties)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectFromProcedure<TOutputModel>(fromObjWithProperties));
        }
        
        public static List<TOutputModel> SelectFromProcedure<TOutputModel>(this IDbConnection dbConn,
            object fromObjWithProperties,
            string sqlFilter,
            params object[] filterParams)
            where TOutputModel : new()
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectFromProcedure<TOutputModel>(
                fromObjWithProperties, sqlFilter, filterParams));
        }

        public static long GetLongScalar(this IDbConnection dbConn)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetLongScalar());
        }			
    }
}
