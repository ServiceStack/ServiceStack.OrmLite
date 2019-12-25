#if ASYNC
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    internal static class WriteExpressionCommandExtensionsAsync
    {
        internal static Task<int> UpdateOnlyAsync<T>(this IDbCommand dbCmd, T model, SqlExpression<T> onlyFields, Action<IDbCommand> commandFilter, CancellationToken token)
        {
            dbCmd.UpdateOnlySql(model, onlyFields);
            commandFilter?.Invoke(dbCmd);
            return dbCmd.ExecNonQueryAsync(token);
        }

        internal static Task<int> UpdateOnlyAsync<T>(this IDbCommand dbCmd, T obj,
            Expression<Func<T, object>> onlyFields,
            Expression<Func<T, bool>> where,
            Action<IDbCommand> commandFilter,
            CancellationToken token)
        {
            if (onlyFields == null)
                throw new ArgumentNullException(nameof(onlyFields));

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Update(onlyFields);
            q.Where(where);
            return dbCmd.UpdateOnlyAsync(obj, q, commandFilter, token);
        }

        internal static Task<int> UpdateOnlyAsync<T>(this IDbCommand dbCmd, T obj,
            string[] onlyFields,
            Expression<Func<T, bool>> where,
            Action<IDbCommand> commandFilter,
            CancellationToken token)
        {
            if (onlyFields == null)
                throw new ArgumentNullException(nameof(onlyFields));

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Update(onlyFields);
            q.Where(where);
            return dbCmd.UpdateOnlyAsync(obj, q, commandFilter, token);
        }

        internal static Task<int> UpdateOnlyAsync<T>(this IDbCommand dbCmd,
            Expression<Func<T>> updateFields,
            SqlExpression<T> q,
            Action<IDbCommand> commandFilter,
            CancellationToken token)
        {
            var cmd = dbCmd.InitUpdateOnly(updateFields, q);
            commandFilter?.Invoke(cmd);
            return cmd.ExecNonQueryAsync(token);
        }

        internal static Task<int> UpdateOnlyAsync<T>(this IDbCommand dbCmd,
            Expression<Func<T>> updateFields,
            string whereExpression,
            IEnumerable<IDbDataParameter> sqlParams,
            Action<IDbCommand> commandFilter,
            CancellationToken token)
        {
            var cmd = dbCmd.InitUpdateOnly(updateFields, whereExpression, sqlParams);
            commandFilter?.Invoke(cmd);
            return cmd.ExecNonQueryAsync(token);
        }

        public static Task<int> UpdateAddAsync<T>(this IDbCommand dbCmd,
            Expression<Func<T>> updateFields,
            SqlExpression<T> q,
            Action<IDbCommand> commandFilter,
            CancellationToken token)
        {
            var cmd = dbCmd.InitUpdateAdd(updateFields, q);
            commandFilter?.Invoke(cmd);
            return cmd.ExecNonQueryAsync(token);
        }

        public static async Task<int> UpdateOnlyAsync<T>(this IDbCommand cmd,
            Dictionary<string, object> updateFields,
            Expression<Func<T, bool>> where,
            Action<IDbCommand> commandFilter = null,
            CancellationToken token = default(CancellationToken))
        {
            if (updateFields == null)
                throw new ArgumentNullException(nameof(updateFields));

            await cmd.OnUpdateAsync(updateFields.FromObjectDictionary<T>()).ConfigureAwait(false);

            var q = cmd.GetDialectProvider().SqlExpression<T>();
            q.Where(where);
            q.PrepareUpdateStatement(cmd, updateFields);
            commandFilter?.Invoke(cmd);

            return await cmd.ExecNonQueryAsync(token).ConfigureAwait(false);
        }

        internal static async Task<int> UpdateNonDefaultsAsync<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> obj, CancellationToken token)
        {
            await  dbCmd.OnUpdateAsync(item).ConfigureAwait(false);
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(obj);
            q.PrepareUpdateStatement(dbCmd, item, excludeDefaults: true);
            return await dbCmd.ExecNonQueryAsync(token).ConfigureAwait(false);
        }

        internal static async Task<int> UpdateAsync<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> expression, Action<IDbCommand> commandFilter, CancellationToken token)
        {
            await dbCmd.OnUpdateAsync(item).ConfigureAwait(false);
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(expression);
            q.PrepareUpdateStatement(dbCmd, item);
            commandFilter?.Invoke(dbCmd);
            return await dbCmd.ExecNonQueryAsync(token).ConfigureAwait(false);
        }

        internal static async Task<int> UpdateAsync<T>(this IDbCommand dbCmd, object updateOnly, Expression<Func<T, bool>> where, Action<IDbCommand> commandFilter, CancellationToken token)
        {
            await dbCmd.OnUpdateAsync(updateOnly).ConfigureAwait(false);

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            var whereSql = q.Where(where).WhereExpression;
            q.CopyParamsTo(dbCmd);
            dbCmd.PrepareUpdateAnonSql<T>(dbCmd.GetDialectProvider(), updateOnly, whereSql);
            commandFilter?.Invoke(dbCmd);

            return await dbCmd.ExecNonQueryAsync(token).ConfigureAwait(false);
        }

        internal static async Task InsertOnlyAsync<T>(this IDbCommand dbCmd, T obj, string[] onlyFields, CancellationToken token)
        {
            await dbCmd.OnInsertAsync(obj).ConfigureAwait(false);
            var sql = dbCmd.GetDialectProvider().ToInsertRowStatement(dbCmd, obj, onlyFields);
            await dbCmd.ExecuteSqlAsync(sql, token).ConfigureAwait(false);
        }

        public static Task<int> InsertOnlyAsync<T>(this IDbCommand dbCmd, Expression<Func<T>> insertFields, CancellationToken token)
        {
            return dbCmd.InitInsertOnly(insertFields).ExecNonQueryAsync(token);
        }

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> where, CancellationToken token)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(where);
            return dbCmd.DeleteAsync(q, token);
        }

        internal static Task<int> DeleteAsync<T>(this IDbCommand dbCmd, SqlExpression<T> q, CancellationToken token)
        {
            var sql = q.ToDeleteRowStatement();
            return dbCmd.ExecuteSqlAsync(sql, q.Params, token);
        }
    }
}

#endif