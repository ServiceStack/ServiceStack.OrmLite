#if NET45
// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    internal static class ReadExpressionCommandExtensionsAsync
    {
        internal static Task<List<T>> SelectAsync<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression, CancellationToken token)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            var sql = expression(expr).SelectInto<T>();
            return dbCmd.ExprConvertToListAsync<T>(sql, token);
        }

        internal static Task<List<Into>> SelectAsync<Into, From>(this IDbCommand dbCmd, Func<SqlExpression<From>, SqlExpression<From>> expression, CancellationToken token)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<From>();
            string sql = expression(expr).SelectInto<Into>();
            return dbCmd.ExprConvertToListAsync<Into>(sql, token);
        }

        internal static Task<List<Into>> SelectAsync<Into, From>(this IDbCommand dbCmd, SqlExpression<From> expression, CancellationToken token)
        {
            string sql = expression.SelectInto<Into>();
            return dbCmd.ExprConvertToListAsync<Into>(sql, token);
        }

        internal static Task<List<T>> SelectAsync<T>(this IDbCommand dbCmd, SqlExpression<T> expression, CancellationToken token)
        {
            string sql = expression.SelectInto<T>();
            return dbCmd.ExprConvertToListAsync<T>(sql, token);
        }

        internal static Task<List<T>> SelectAsync<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate, CancellationToken token)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            string sql = expr.Where(predicate).SelectInto<T>();

            return dbCmd.ExprConvertToListAsync<T>(sql, token);
        }

        internal static Task<T> SingleAsync<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression, CancellationToken token)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            return dbCmd.SingleAsync(expression(expr), token);
        }

        internal static Task<T> SingleAsync<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate, CancellationToken token)
        {
            var ev = OrmLiteConfig.DialectProvider.SqlExpression<T>();

            return SingleAsync(dbCmd, ev.Where(predicate), token);
        }

        internal static Task<T> SingleAsync<T>(this IDbCommand dbCmd, SqlExpression<T> expression, CancellationToken token)
        {
            string sql = expression.Limit(1).SelectInto<T>();

            return dbCmd.ExprConvertToAsync<T>(sql, token);
        }

        public static Task<TKey> ScalarAsync<T, TKey>(this IDbCommand dbCmd, Expression<Func<T, TKey>> field, CancellationToken token)
        {
            var ev = dbCmd.GetDialectProvider().SqlExpression<T>();
            ev.Select(field);
            var sql = ev.SelectInto<T>();
            return dbCmd.ScalarAsync<TKey>(sql, token);
        }

        internal static Task<TKey> ScalarAsync<T, TKey>(this IDbCommand dbCmd,
            Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate, CancellationToken token)
        {
            var ev = dbCmd.GetDialectProvider().SqlExpression<T>();
            ev.Select(field).Where(predicate);
            string sql = ev.SelectInto<T>();
            return dbCmd.ScalarAsync<TKey>(sql, token);
        }

        internal static Task<long> CountAsync<T>(this IDbCommand dbCmd, CancellationToken token)
        {
            var expression = dbCmd.GetDialectProvider().SqlExpression<T>();
            var sql = expression.ToCountStatement();
            return GetCountAsync(dbCmd, sql, token);
        }

        internal static Task<long> CountAsync<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression, CancellationToken token)
        {
            var expr = dbCmd.GetDialectProvider().SqlExpression<T>();
            var sql = expression(expr).ToCountStatement();
            return GetCountAsync(dbCmd, sql, token);
        }

        internal static Task<long> CountAsync<T>(this IDbCommand dbCmd, SqlExpression<T> expression, CancellationToken token)
        {
            var sql = expression.ToCountStatement();
            return GetCountAsync(dbCmd, sql, token);
        }

        internal static Task<long> CountAsync<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate, CancellationToken token)
        {
            var ev = dbCmd.GetDialectProvider().SqlExpression<T>();
            ev.Where(predicate);
            var sql = ev.ToCountStatement();
            return GetCountAsync(dbCmd, sql, token);
        }

        internal static Task<long> GetCountAsync(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            return dbCmd.ColumnAsync<long>(sql, token).Then(x => x.Sum());
        }

        internal static Task<long> RowCountAsync<T>(this IDbCommand dbCmd, SqlExpression<T> expression, CancellationToken token)
        {
            var sql = "SELECT COUNT(*) FROM ({0}) AS COUNT".Fmt(expression.ToSelectStatement(), token);
            return dbCmd.ScalarAsync<long>(sql, token);
        }

        internal static Task<long> RowCountAsync(this IDbCommand dbCmd, string sql, CancellationToken token)
        {
            return dbCmd.ScalarAsync<long>("SELECT COUNT(*) FROM ({0}) AS COUNT".Fmt(sql), token);
        }

        internal static Task<List<T>> LoadSelectAsync<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression, CancellationToken token = default(CancellationToken))
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            expr = expression(expr);
            return dbCmd.LoadListWithReferences<T, T>(expr, token);
        }

        internal static Task<List<T>> LoadSelectAsync<T>(this IDbCommand dbCmd, SqlExpression<T> expression = null, CancellationToken token = default(CancellationToken))
        {
            return dbCmd.LoadListWithReferences<T, T>(expression, token);
        }

        internal static Task<List<Into>> LoadSelectAsync<Into, From>(this IDbCommand dbCmd, SqlExpression<From> expression, CancellationToken token = default(CancellationToken))
        {
            return dbCmd.LoadListWithReferences<Into, From>(expression, token);
        }

        internal static Task<List<T>> LoadSelectAsync<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate, CancellationToken token = default(CancellationToken))
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>().Where(predicate);
            return dbCmd.LoadListWithReferences<T, T>(expr, token);
        }

        internal static Task<T> ExprConvertToAsync<T>(this IDataReader dataReader, IOrmLiteDialectProvider dialectProvider, CancellationToken token)
        {
            return dialectProvider.ReaderRead(dataReader,
                () => dataReader.ConvertTo<T>(dialectProvider), token);
        }

        internal static Task<List<T>> ExprConvertToListAsync<T>(this IDataReader dataReader, IOrmLiteDialectProvider dialectProvider, CancellationToken token)
        {
            var fieldDefs = ModelDefinition<T>.Definition.AllFieldDefinitionsArray;

            var indexCache = dataReader.GetIndexFieldsCache(ModelDefinition<T>.Definition);

            return dialectProvider.ReaderEach(dataReader, () =>
            {
                var row = OrmLiteUtilExtensions.CreateInstance<T>();
                row.PopulateWithSqlReader(dialectProvider, dataReader, fieldDefs, indexCache);
                return row;
            }, token);
        }

        internal static Task<List<T>> Select<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate, CancellationToken token)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            string sql = expr.Where(predicate).SelectInto<T>();

            return dbCmd.ExprConvertToListAsync<T>(sql, token);
        }

    }
}
#endif