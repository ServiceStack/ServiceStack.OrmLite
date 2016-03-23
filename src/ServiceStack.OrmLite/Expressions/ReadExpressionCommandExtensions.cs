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
        [Obsolete("Use db.Select(db.From<T>())")]
        internal static List<T> Select<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            string sql = expression(q).SelectInto<T>();

            return dbCmd.ExprConvertToList<T>(sql, q.Params, onlyFields: q.OnlyFields);
        }

        [Obsolete("Use db.Select<Into,From>(db.From<From>())")]
        internal static List<Into> Select<Into, From>(this IDbCommand dbCmd, Func<SqlExpression<From>, SqlExpression<From>> expression)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<From>();
            string sql = expression(q).SelectInto<Into>();

            return dbCmd.ExprConvertToList<Into>(sql, q.Params, onlyFields: q.OnlyFields);
        }

        internal static List<Into> Select<Into, From>(this IDbCommand dbCmd, SqlExpression<From> q)
        {
            string sql = q.SelectInto<Into>();
            return dbCmd.ExprConvertToList<Into>(sql, q.Params, onlyFields: q.OnlyFields);
        }

        internal static List<T> Select<T>(this IDbCommand dbCmd, SqlExpression<T> q)
        {
            string sql = q.SelectInto<T>();

            return dbCmd.ExprConvertToList<T>(sql, q.Params, onlyFields: q.OnlyFields);
        }

        internal static List<T> Select<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            string sql = q.Where(predicate).SelectInto<T>();

            return dbCmd.ExprConvertToList<T>(sql, q.Params);
        }

        [Obsolete("Use db.Single(db.From<T>())")]
        internal static T Single<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            return dbCmd.Single(expression(q));
        }

        internal static T Single<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();

            return Single(dbCmd, q.Where(predicate));
        }

        internal static T Single<T>(this IDbCommand dbCmd, SqlExpression<T> q)
        {
            string sql = q.Limit(1).SelectInto<T>();

            return dbCmd.ExprConvertTo<T>(sql, q.Params, onlyFields:q.OnlyFields);
        }

        public static TKey Scalar<T, TKey>(this IDbCommand dbCmd, SqlExpression<T> expression)
        {
            var sql = expression.SelectInto<T>();
            return dbCmd.Scalar<TKey>(sql, expression.Params);
        }

        public static TKey Scalar<T, TKey>(this IDbCommand dbCmd, Expression<Func<T, TKey>> field)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Select(field);
            var sql = q.SelectInto<T>();
            return dbCmd.Scalar<TKey>(sql, q.Params);
        }

        internal static TKey Scalar<T, TKey>(this IDbCommand dbCmd,
            Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Select(field).Where(predicate);
            string sql = q.SelectInto<T>();
            return dbCmd.Scalar<TKey>(sql, q.Params);
        }

        internal static long Count<T>(this IDbCommand dbCmd)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            var sql = q.ToCountStatement();
            return GetCount(dbCmd, sql, q.Params);
        }

        [Obsolete("Use db.Count(db.From<T>())")]
        internal static long Count<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            var sql = expression(q).ToCountStatement();
            return GetCount(dbCmd, sql, q.Params);
        }

        internal static long Count<T>(this IDbCommand dbCmd, SqlExpression<T> expression)
        {
            var sql = expression.ToCountStatement();
            return GetCount(dbCmd, sql, expression.Params);
        }

        internal static long Count<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(predicate);
            var sql = q.ToCountStatement();
            return GetCount(dbCmd, sql, q.Params);
        }

        internal static long GetCount(this IDbCommand dbCmd, string sql)
        {
            return dbCmd.Column<long>(sql).Sum();
        }

        internal static long GetCount(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams)
        {
            return dbCmd.Column<long>(sql, sqlParams).Sum();
        }

        internal static long RowCount<T>(this IDbCommand dbCmd, SqlExpression<T> expression)
        {
            //ORDER BY throws when used in subselects in SQL Server. Removing OrderBy() clause since it doesn't impact results
            var countExpr = expression.Clone().OrderBy(); 
            return dbCmd.Scalar<long>(dbCmd.GetDialectProvider().ToRowCountStatement(countExpr.ToSelectStatement()));
        }

        internal static long RowCount(this IDbCommand dbCmd, string sql)
        {
            return dbCmd.Scalar<long>(dbCmd.GetDialectProvider().ToRowCountStatement(sql));
        }

        [Obsolete("Use db.LoadSelect(db.From<T>())")]
        internal static List<T> LoadSelect<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression, IEnumerable<string> include = null)
        {
            var expr = dbCmd.GetDialectProvider().SqlExpression<T>();
            expr = expression(expr);
            return dbCmd.LoadListWithReferences<T, T>(expr, include);
        }

        internal static List<T> LoadSelect<T>(this IDbCommand dbCmd, SqlExpression<T> expression = null, IEnumerable<string> include = null)
        {
            return dbCmd.LoadListWithReferences<T, T>(expression, include);
        }

        internal static List<Into> LoadSelect<Into, From>(this IDbCommand dbCmd, SqlExpression<From> expression, IEnumerable<string> include = null)
        {
            return dbCmd.LoadListWithReferences<Into, From>(expression, include);
        }

        internal static List<T> LoadSelect<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate, IEnumerable<string> include = null)
        {
            var expr = dbCmd.GetDialectProvider().SqlExpression<T>().Where(predicate);
            return dbCmd.LoadListWithReferences<T, T>(expr, include);
        }
    }
}

