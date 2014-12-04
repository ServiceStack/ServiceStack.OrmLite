using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace ServiceStack.OrmLite
{
    internal static class ReadExpressionCommandExtensions
    {
        internal static List<T> Select<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            var expr = dbCmd.GetDialectProvider().SqlExpression<T>();
            string sql = expression(expr).SelectInto<T>();

            return dbCmd.ExprConvertToList<T>(sql);
        }

        internal static List<Into> Select<Into, From>(this IDbCommand dbCmd, Func<SqlExpression<From>, SqlExpression<From>> expression)
        {
            var expr = dbCmd.GetDialectProvider().SqlExpression<From>();
            string sql = expression(expr).SelectInto<Into>();

            return dbCmd.ExprConvertToList<Into>(sql);
        }

        internal static List<Into> Select<Into, From>(this IDbCommand dbCmd, SqlExpression<From> expression)
        {
            string sql = expression.SelectInto<Into>();
            return dbCmd.ExprConvertToList<Into>(sql);
        }

        internal static List<T> Select<T>(this IDbCommand dbCmd, SqlExpression<T> expression)
        {
            string sql = expression.SelectInto<T>();

            return dbCmd.ExprConvertToList<T>(sql);
        }

        internal static List<T> Select<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var expr = dbCmd.GetDialectProvider().SqlExpression<T>();
            string sql = expr.Where(predicate).SelectInto<T>();

            return dbCmd.ExprConvertToList<T>(sql);
        }

        internal static T Single<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            var expr = dbCmd.GetDialectProvider().SqlExpression<T>();
            return dbCmd.Single(expression(expr));
        }

        internal static T Single<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var ev = dbCmd.GetDialectProvider().SqlExpression<T>();

            return Single(dbCmd, ev.Where(predicate));
        }

        internal static T Single<T>(this IDbCommand dbCmd, SqlExpression<T> expression)
        {
            string sql = expression.Limit(1).SelectInto<T>();

            return dbCmd.ExprConvertTo<T>(sql);
        }

        public static TKey Scalar<T, TKey>(this IDbCommand dbCmd, Expression<Func<T, TKey>> field)
        {
            var ev = dbCmd.GetDialectProvider().SqlExpression<T>();
            ev.Select(field);
            var sql = ev.SelectInto<T>();
            return dbCmd.Scalar<TKey>(sql);
        }

        internal static TKey Scalar<T, TKey>(this IDbCommand dbCmd,
            Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate)
        {
            var ev = dbCmd.GetDialectProvider().SqlExpression<T>();
            ev.Select(field).Where(predicate);
            string sql = ev.SelectInto<T>();
            return dbCmd.Scalar<TKey>(sql);
        }

        internal static long Count<T>(this IDbCommand dbCmd)
        {
            var expression = dbCmd.GetDialectProvider().SqlExpression<T>();
            var sql = expression.ToCountStatement();
            return GetCount(dbCmd, sql);
        }

        internal static long Count<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            var expr = dbCmd.GetDialectProvider().SqlExpression<T>();
            var sql = expression(expr).ToCountStatement();
            return GetCount(dbCmd, sql);
        }

        internal static long Count<T>(this IDbCommand dbCmd, SqlExpression<T> expression)
        {
            var sql = expression.ToCountStatement();
            return GetCount(dbCmd, sql);
        }

        internal static long Count<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var ev = dbCmd.GetDialectProvider().SqlExpression<T>();
            ev.Where(predicate);
            var sql = ev.ToCountStatement();
            return GetCount(dbCmd, sql);
        }

        internal static long GetCount(this IDbCommand dbCmd, string sql)
        {
            return dbCmd.Column<long>(sql).Sum();
        }

        internal static long RowCount<T>(this IDbCommand dbCmd, SqlExpression<T> expression)
        {
            return dbCmd.Scalar<long>(dbCmd.GetDialectProvider().ToRowCountStatement(expression.ToSelectStatement()));
        }

        internal static long RowCount(this IDbCommand dbCmd, string sql)
        {
            return dbCmd.Scalar<long>(dbCmd.GetDialectProvider().ToRowCountStatement(sql));
        }

        internal static List<T> LoadSelect<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            var expr = dbCmd.GetDialectProvider().SqlExpression<T>();
            expr = expression(expr);
            return dbCmd.LoadListWithReferences<T, T>(expr);
        }

        internal static List<T> LoadSelect<T>(this IDbCommand dbCmd, SqlExpression<T> expression = null)
        {
            return dbCmd.LoadListWithReferences<T, T>(expression);
        }

        internal static List<Into> LoadSelect<Into, From>(this IDbCommand dbCmd, SqlExpression<From> expression)
        {
            return dbCmd.LoadListWithReferences<Into, From>(expression);
        }

        internal static List<T> LoadSelect<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var expr = dbCmd.GetDialectProvider().SqlExpression<T>().Where(predicate);
            return dbCmd.LoadListWithReferences<T, T>(expr);
        }
    }
}

