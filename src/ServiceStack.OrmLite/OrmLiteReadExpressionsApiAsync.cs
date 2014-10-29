#if NET45
// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteReadExpressionsApiAsync
    {
        /// <summary>
        /// Returns results from using a LINQ Expression. E.g:
        /// <para>db.Select&lt;Person&gt;(x =&gt; x.Age &gt; 40)</para>
        /// </summary>
        public static Task<List<T>> SelectAsync<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectAsync(predicate, token));
        }

        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select&lt;Person&gt;(q =&gt; q.Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static Task<List<T>> SelectAsync<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectAsync(expression, token));
        }

        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select(db.From&lt;Person&gt;().Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static Task<List<T>> SelectAsync<T>(this IDbConnection dbConn, SqlExpression<T> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectAsync(expression, token));
        }

        /// <summary>
        /// Project results from a number of joined tables into a different model
        /// </summary>
        public static Task<List<Into>> SelectAsync<Into, From>(this IDbConnection dbConn, SqlExpression<From> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectAsync<Into, From>(expression, token));
        }

        /// <summary>
        /// Project results from a number of joined tables into a different model
        /// </summary>
        public static Task<List<Into>> SelectAsync<Into, From>(this IDbConnection dbConn, Func<SqlExpression<From>, SqlExpression<From>> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectAsync<Into, From>(expression, token));
        }

        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select(db.From&lt;Person&gt;().Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        //public static Task<List<T>> SelectAsync<T>(this IDbConnection dbConn, ISqlExpression expression, object anonType, CancellationToken token)
        //{
        //    return dbConn.Exec(dbCmd => dbCmd.SqlList<T>(expression.SelectInto<T>(), anonType));
        //}

        /// <summary>
        /// Returns a single result from using a LINQ Expression. E.g:
        /// <para>db.Single&lt;Person&gt;(x =&gt; x.Age == 42)</para>
        /// </summary>
        public static Task<T> SingleAsync<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.SingleAsync(predicate, token));
        }

        /// <summary>
        /// Returns a single result from using an SqlExpression lambda. E.g:
        /// <para>db.Single&lt;Person&gt;(q =&gt; q.Where(x =&gt; x.Age == 42))</para>
        /// </summary>
        public static Task<T> SingleAsync<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.SingleAsync(expression, token));
        }

        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select&lt;Person&gt;(x =&gt; x.Age &gt; 40)</para>
        /// </summary>
        public static Task<T> SingleAsync<T>(this IDbConnection dbConn, SqlExpression<T> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.SingleAsync(expression, token));
        }

        /// <summary>
        /// Returns a scalar result from using an SqlExpression lambda. E.g:
        /// <para>db.Scalar&lt;Person, int&gt;(x =&gt; Sql.Max(x.Age))</para>
        /// </summary>
        public static Task<TKey> ScalarAsync<T, TKey>(this IDbConnection dbConn, Expression<Func<T, TKey>> field, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.ScalarAsync(field, token));
        }

        /// <summary>
        /// Returns a scalar result from using an SqlExpression lambda. E.g:
        /// <para>db.Scalar&lt;Person, int&gt;(x =&gt; Sql.Max(x.Age), , x =&gt; x.Age &lt; 50)</para>
        /// </summary>        
        public static Task<TKey> ScalarAsync<T, TKey>(this IDbConnection dbConn,
            Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.ScalarAsync(field, predicate, token));
        }

        /// <summary>
        /// Returns the count of rows that match the LINQ expression, E.g:
        /// <para>db.Count&lt;Person&gt;(x =&gt; x.Age &lt; 50)</para>
        /// </summary>
        public static Task<long> CountAsync<T>(this IDbConnection dbConn, Expression<Func<T, bool>> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.CountAsync(expression, token));
        }

        /// <summary>
        /// Returns the count of rows that match the SqlExpression lambda, E.g:
        /// <para>db.Count&lt;Person&gt;(q =&gt; q.Where(x =&gt; x.Age &lt; 50))</para>
        /// </summary>
        public static Task<long> CountAsync<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.CountAsync(expression, token));
        }

        /// <summary>
        /// Returns the count of rows that match the supplied SqlExpression, E.g:
        /// <para>db.Count(db.From&lt;Person&gt;().Where(x =&gt; x.Age &lt; 50))</para>
        /// </summary>
        public static Task<long> CountAsync<T>(this IDbConnection dbConn, SqlExpression<T> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.CountAsync(expression, token));
        }

        public static Task<long> CountAsync<T>(this IDbConnection dbConn, CancellationToken token = default(CancellationToken))
        {
            var expression = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            return dbConn.Exec(dbCmd => dbCmd.CountAsync(expression, token));
        }

        /// <summary>
        /// Return the number of rows returned by the supplied expression
        /// </summary>
        public static Task<long> RowCountAsync<T>(this IDbConnection dbConn, SqlExpression<T> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.RowCountAsync(expression, token));
        }

        /// <summary>
        /// Return the number of rows returned by the supplied sql
        /// </summary>
        public static Task<long> RowCountAsync(this IDbConnection dbConn, string sql, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.RowCountAsync(sql, token));
        }

        /// <summary>
        /// Returns results with references from using a LINQ Expression. E.g:
        /// <para>db.LoadSelectAsync&lt;Person&gt;(x =&gt; x.Age &gt; 40)</para>
        /// </summary>
        public static Task<List<T>> LoadSelectAsync<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelectAsync(predicate));
        }

        /// <summary>
        /// Returns results with references from using an SqlExpression lambda. E.g:
        /// <para>db.LoadSelectAsync&lt;Person&gt;(q =&gt; q.Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static Task<List<T>> LoadSelectAsync<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelectAsync(expression));
        }

        /// <summary>
        /// Returns results with references from using an SqlExpression lambda. E.g:
        /// <para>db.LoadSelectAsync(db.From&lt;Person&gt;().Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static Task<List<T>> LoadSelectAsync<T>(this IDbConnection dbConn, SqlExpression<T> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelectAsync(expression));
        }

        /// <summary>
        /// Project results with references from a number of joined tables into a different model
        /// </summary>
        public static Task<List<Into>> LoadSelectAsync<Into, From>(this IDbConnection dbConn, SqlExpression<From> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelectAsync<Into, From>(expression));
        }         
    }

}
#endif