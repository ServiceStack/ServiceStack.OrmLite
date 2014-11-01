#if NET45
using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    internal static class WriteExpressionCommandExtensionsAsync
    {
        internal static Task<int> UpdateOnlyAsync<T>(this IDbCommand dbCmd, T model, Func<SqlExpression<T>, SqlExpression<T>> onlyFields, CancellationToken token)
        {
            return dbCmd.UpdateOnlyAsync(model, onlyFields(OrmLiteConfig.DialectProvider.SqlExpression<T>()), token);
        }

        internal static Task<int> UpdateOnlyAsync<T>(this IDbCommand dbCmd, T model, SqlExpression<T> onlyFields, CancellationToken token)
        {
            var sql = dbCmd.UpdateOnlySql(model, onlyFields);
            return dbCmd.ExecuteSqlAsync(sql, token);
        }

        internal static Task<int> UpdateOnlyAsync<T, TKey>(this IDbCommand dbCmd, T obj,
            Expression<Func<T, TKey>> onlyFields,
            Expression<Func<T, bool>> where, 
            CancellationToken token)
        {
            if (onlyFields == null)
                throw new ArgumentNullException("onlyFields");

            var q = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            q.Update(onlyFields);
            q.Where(where);
            return dbCmd.UpdateOnlyAsync(obj, q, token);
        }

        internal static Task<int> UpdateNonDefaultsAsync<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> obj, CancellationToken token)
        {
            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, item);

            var q = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            q.Where(obj);
            var sql = q.ToUpdateStatement(item, excludeDefaults: true);
            return dbCmd.ExecuteSqlAsync(sql, token);
        }

        internal static Task<int> UpdateAsync<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> expression, CancellationToken token)
        {
            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, item);

            var q = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            q.Where(expression);
            var sql = q.ToUpdateStatement(item);
            return dbCmd.ExecuteSqlAsync(sql, token);
        }

        internal static Task<int> UpdateAsync<T>(this IDbCommand dbCmd, object updateOnly, Expression<Func<T, bool>> where, CancellationToken token)
        {
            var updateSql = WriteExpressionCommandExtensions.UpdateSql(dbCmd.GetDialectProvider(), updateOnly, where);
            return dbCmd.ExecuteSqlAsync(updateSql, token);
        }

        internal static Task<int> UpdateFmtAsync<T>(this IDbCommand dbCmd, string set, string where, CancellationToken token)
        {
            return dbCmd.UpdateFmtAsync(typeof(T).GetModelDefinition().ModelName, set, where, token);
        }

        internal static Task<int> UpdateFmtAsync(this IDbCommand dbCmd, string table, string set, string where, CancellationToken token)
        {
            var sql = WriteExpressionCommandExtensions.UpdateFmtSql(dbCmd.GetDialectProvider(), table, set, @where);
            return dbCmd.ExecuteSqlAsync(sql.ToString(), token);
        }

        internal static Task InsertOnlyAsync<T>(this IDbCommand dbCmd, T obj, Func<SqlExpression<T>, SqlExpression<T>> onlyFields, CancellationToken token)
        {
            return dbCmd.InsertOnlyAsync(obj, onlyFields(OrmLiteConfig.DialectProvider.SqlExpression<T>()), token);
        }

        internal static Task InsertOnlyAsync<T>(this IDbCommand dbCmd, T obj, SqlExpression<T> onlyFields, CancellationToken token)
        {
            if (OrmLiteConfig.InsertFilter != null)
                OrmLiteConfig.InsertFilter(dbCmd, obj);

            var sql = OrmLiteConfig.DialectProvider.ToInsertRowStatement(dbCmd, obj, onlyFields.InsertFields);
            return dbCmd.ExecuteSqlAsync(sql, token);
        }

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> where, CancellationToken token)
        {
            var ev = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            ev.Where(where);
            return dbCmd.DeleteAsync(ev, token);
        }

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> where, CancellationToken token)
        {
            return dbCmd.DeleteAsync(where(OrmLiteConfig.DialectProvider.SqlExpression<T>()), token);
        }

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, SqlExpression<T> where, CancellationToken token)
        {
            var sql = where.ToDeleteRowStatement();
            return dbCmd.ExecuteSqlAsync(sql, token);
        }

        internal static Task<int> DeleteFmtAsync<T>(this IDbCommand dbCmd, string where, CancellationToken token)
        {
            return dbCmd.DeleteFmtAsync(typeof(T).GetModelDefinition().ModelName, where, token);
        }

        internal static Task<int> DeleteFmtAsync(this IDbCommand dbCmd, string table, string where, CancellationToken token)
        {
            var sql = WriteExpressionCommandExtensions.DeleteFmtSql(dbCmd.GetDialectProvider(), table, @where);
            return dbCmd.ExecuteSqlAsync(sql.ToString(), token);
        }
    }
}
#endif