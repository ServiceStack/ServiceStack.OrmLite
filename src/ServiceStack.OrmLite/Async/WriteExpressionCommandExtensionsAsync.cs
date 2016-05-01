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
        [Obsolete("Use db.UpdateOnlyAsync(model, db.From<T>())")]
        internal static Task<int> UpdateOnlyAsync<T>(this IDbCommand dbCmd, T model, Func<SqlExpression<T>, SqlExpression<T>> onlyFields, CancellationToken token)
        {
            return dbCmd.UpdateOnlyAsync(model, onlyFields(dbCmd.GetDialectProvider().SqlExpression<T>()), token);
        }

        internal static Task<int> UpdateOnlyAsync<T>(this IDbCommand dbCmd, T model, SqlExpression<T> onlyFields, CancellationToken token)
        {
            dbCmd.UpdateOnlySql(model, onlyFields);
            return dbCmd.ExecNonQueryAsync(token);
        }

        internal static Task<int> UpdateOnlyAsync<T, TKey>(this IDbCommand dbCmd, T obj,
            Expression<Func<T, TKey>> onlyFields,
            Expression<Func<T, bool>> where, 
            CancellationToken token)
        {
            if (onlyFields == null)
                throw new ArgumentNullException("onlyFields");

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Update(onlyFields);
            q.Where(where);
            return dbCmd.UpdateOnlyAsync(obj, q, token);
        }

        internal static Task<int> UpdateNonDefaultsAsync<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> obj, CancellationToken token)
        {
            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, item);

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(obj);
            q.PrepareUpdateStatement(dbCmd, item, excludeDefaults: true);
            return dbCmd.ExecNonQueryAsync(token);
        }

        internal static Task<int> UpdateAsync<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> expression, CancellationToken token)
        {
            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, item);

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(expression);
            q.PrepareUpdateStatement(dbCmd, item);
            return dbCmd.ExecNonQueryAsync(token);
        }

        internal static Task<int> UpdateAsync<T>(this IDbCommand dbCmd, object updateOnly, Expression<Func<T, bool>> where, CancellationToken token)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            var whereSql = q.Where(@where).WhereExpression;
            q.CopyParamsTo(dbCmd);
            dbCmd.PrepareUpdateAnonSql<T>(dbCmd.GetDialectProvider(), updateOnly, whereSql);

            return dbCmd.ExecNonQueryAsync(token);
        }

        internal static Task<int> UpdateFmtAsync<T>(this IDbCommand dbCmd, string set, string where, CancellationToken token)
        {
            return dbCmd.UpdateFmtAsync(typeof(T).GetModelDefinition().ModelName, set, where, token);
        }

        internal static Task<int> UpdateFmtAsync(this IDbCommand dbCmd, string table, string set, string where, CancellationToken token)
        {
            var sql = Legacy.WriteExpressionCommandExtensionsLegacy.UpdateFmtSql(dbCmd.GetDialectProvider(), table, set, @where);
            return dbCmd.ExecuteSqlAsync(sql, token);
        }

        [Obsolete("Use db.InsertOnlyAsync(obj, db.From<T>())")]
        internal static Task InsertOnlyAsync<T>(this IDbCommand dbCmd, T obj, Func<SqlExpression<T>, SqlExpression<T>> onlyFields, CancellationToken token)
        {
            return dbCmd.InsertOnlyAsync(obj, onlyFields(dbCmd.GetDialectProvider().SqlExpression<T>()), token);
        }

        internal static Task InsertOnlyAsync<T>(this IDbCommand dbCmd, T obj, SqlExpression<T> onlyFields, CancellationToken token)
        {
            if (OrmLiteConfig.InsertFilter != null)
                OrmLiteConfig.InsertFilter(dbCmd, obj);

            var sql = dbCmd.GetDialectProvider().ToInsertRowStatement(dbCmd, obj, onlyFields.InsertFields);
            return dbCmd.ExecuteSqlAsync(sql, token);
        }

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> where, CancellationToken token)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(where);
            return dbCmd.DeleteAsync(q, token);
        }

        [Obsolete("Use db.DeleteAsync(db.From<T>())")]
        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> where, CancellationToken token)
        {
            return dbCmd.DeleteAsync(where(dbCmd.GetDialectProvider().SqlExpression<T>()), token);
        }

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, SqlExpression<T> q, CancellationToken token)
        {
            var sql = q.ToDeleteRowStatement();
            return dbCmd.ExecuteSqlAsync(sql, q.Params, token);
        }

        internal static Task<int> DeleteFmtAsync<T>(this IDbCommand dbCmd, string where, CancellationToken token)
        {
            return dbCmd.DeleteFmtAsync(typeof(T).GetModelDefinition().ModelName, where, token);
        }

        internal static Task<int> DeleteFmtAsync(this IDbCommand dbCmd, string table, string where, CancellationToken token)
        {
            var sql = Legacy.WriteExpressionCommandExtensionsLegacy.DeleteFmtSql(dbCmd.GetDialectProvider(), table, @where);
            return dbCmd.ExecuteSqlAsync(sql, token);
        }
    }
}
#endif