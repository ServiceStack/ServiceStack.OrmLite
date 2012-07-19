using System;
using System.Data;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
    public static class WriteConnectionExtensions
    {
        public static int UpdateOnly<T>(this IDbConnection dbConn, T model, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields)
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateOnly(model, onlyFields));
        }

        public static int UpdateOnly<T>(this IDbConnection dbConn, T model, SqlExpressionVisitor<T> onlyFields)
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateOnly(model, onlyFields));
        }

        public static int UpdateOnly<T, TKey>(this IDbConnection dbConn, T obj,
            Expression<Func<T, TKey>> onlyFields = null,
            Expression<Func<T, bool>> where = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateOnly(obj, onlyFields, where));
        }

        public static int UpdateNonDefaults<T>(this IDbConnection dbConn, T item, Expression<Func<T, bool>> where)
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateNonDefaults(item, where));
        }

        public static int Update<T>(this IDbConnection dbConn, T item, Expression<Func<T, bool>> where)
        {
            return dbConn.Exec(dbCmd => dbCmd.Update(item, where));
        }

        public static int Update<T>(this IDbConnection dbConn, object updateOnly, Expression<Func<T, bool>> where = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.Update(updateOnly, where));
        }

        public static int Update<T>(this IDbConnection dbConn, string set = null, string where = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.Update<T>(set, where));
        }

        public static int Update(this IDbConnection dbConn, string table = null, string set = null, string where = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.Update(table, set, where));
        }

        public static void InsertOnly<T>(this IDbConnection dbConn, T obj, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields)
        {
            dbConn.Exec(dbCmd => dbCmd.InsertOnly(obj, onlyFields));
        }

        public static void InsertOnly<T>(this IDbConnection dbConn, T obj, SqlExpressionVisitor<T> onlyFields)
        {
            dbConn.Exec(dbCmd => dbCmd.InsertOnly(obj, onlyFields));
        }

        public static int Delete<T>(this IDbConnection dbConn, Expression<Func<T, bool>> where)
        {
            return dbConn.Exec(dbCmd => dbCmd.Delete(where));
        }

        public static int Delete<T>(this IDbConnection dbConn, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> where)
        {
            return dbConn.Exec(dbCmd => dbCmd.Delete(where));
        }

        public static int Delete<T>(this IDbConnection dbConn, SqlExpressionVisitor<T> where)
        {
            return dbConn.Exec(dbCmd => dbCmd.Delete(where));
        }

        public static int Delete<T>(this IDbConnection dbConn, string where = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.Delete<T>(where));
        }

        public static int Delete(this IDbConnection dbConn, string table = null, string where = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.Delete(table, where));
        }         
    }
}