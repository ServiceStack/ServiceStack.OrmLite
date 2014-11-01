using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteReadExpressionsApi
    {
        public static T Exec<T>(this IDbConnection dbConn, Func<IDbCommand, T> filter)
        {
            return OrmLiteConfig.ExecFilter.Exec(dbConn, filter);
        }

        public static void Exec(this IDbConnection dbConn, Action<IDbCommand> filter)
        {
            OrmLiteConfig.ExecFilter.Exec(dbConn, filter);
        }

        public static Task<T> Exec<T>(this IDbConnection dbConn, Func<IDbCommand, Task<T>> filter)
        {
            return OrmLiteConfig.ExecFilter.Exec(dbConn, filter);
        }

        public static Task Exec(this IDbConnection dbConn, Func<IDbCommand, Task> filter)
        {
            return OrmLiteConfig.ExecFilter.Exec(dbConn, filter);
        }

        public static IEnumerable<T> ExecLazy<T>(this IDbConnection dbConn, Func<IDbCommand, IEnumerable<T>> filter)
        {
            return OrmLiteConfig.ExecFilter.ExecLazy(dbConn, filter);
        }

        /// <summary>
        /// Create a new SqlExpression builder allowing typed LINQ-like queries.
        /// </summary>
        [Obsolete("Use From<T>")]
        public static SqlExpression<T> SqlExpression<T>(this IDbConnection dbConn)
        {
            return OrmLiteConfig.ExecFilter.SqlExpression<T>(dbConn);
        }

        /// <summary>
        /// Creates a new SqlExpression builder allowing typed LINQ-like queries.
        /// Alias for SqlExpression.
        /// </summary>
        public static SqlExpression<T> From<T>(this IDbConnection dbConn)
        {
            return OrmLiteConfig.ExecFilter.SqlExpression<T>(dbConn);
        }

        public static SqlExpression<T> From<T, JoinWith>(this IDbConnection dbConn, Expression<Func<T, JoinWith, bool>> joinExpr=null)
        {
            var sql = OrmLiteConfig.ExecFilter.SqlExpression<T>(dbConn);
            sql.Join<T,JoinWith>(joinExpr);
            return sql;
        }

        /// <summary>
        /// Creates a new SqlExpression builder for the specified type using a user-defined FROM sql expression.
        /// </summary>
        public static SqlExpression<T> From<T>(this IDbConnection dbConn, string fromExpression)
        {
            var expr = OrmLiteConfig.ExecFilter.SqlExpression<T>(dbConn);
            expr.From(fromExpression);
            return expr;
        }

        /// <summary>
        /// Open a Transaction in OrmLite
        /// </summary>
        public static IDbTransaction OpenTransaction(this IDbConnection dbConn)
        {
            return new OrmLiteTransaction(dbConn, dbConn.BeginTransaction());
        }

        /// <summary>
        /// Open a Transaction in OrmLite
        /// </summary>
        public static IDbTransaction OpenTransaction(this IDbConnection dbConn, IsolationLevel isolationLevel)
        {
            return new OrmLiteTransaction(dbConn, dbConn.BeginTransaction(isolationLevel));
        }

        /// <summary>
        /// Returns results from using a LINQ Expression. E.g:
        /// <para>db.Select&lt;Person&gt;(x =&gt; x.Age &gt; 40)</para>
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(predicate));
        }

        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select&lt;Person&gt;(q =&gt; q.Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(expression));
        }

        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select(db.From&lt;Person&gt;().Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, SqlExpression<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(expression));
        }

        /// <summary>
        /// Project results from a number of joined tables into a different model
        /// </summary>
        public static List<Into> Select<Into, From>(this IDbConnection dbConn, SqlExpression<From> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<Into, From>(expression));
        }

        /// <summary>
        /// Project results from a number of joined tables into a different model
        /// </summary>
        public static List<Into> Select<Into, From>(this IDbConnection dbConn, Func<SqlExpression<From>, SqlExpression<From>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<Into, From>(expression));
        }

        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select(db.From&lt;Person&gt;().Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, ISqlExpression expression, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.SqlList<T>(expression.SelectInto<T>(), anonType));
        }

        /// <summary>
        /// Returns a single result from using a LINQ Expression. E.g:
        /// <para>db.Single&lt;Person&gt;(x =&gt; x.Age == 42)</para>
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single(predicate));
        }

        /// <summary>
        /// Returns a single result from using an SqlExpression lambda. E.g:
        /// <para>db.Single&lt;Person&gt;(q =&gt; q.Where(x =&gt; x.Age == 42))</para>
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single(expression));
        }

        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select&lt;Person&gt;(x =&gt; x.Age &gt; 40)</para>
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, SqlExpression<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single(expression));
        }

        /// <summary>
        /// Returns a scalar result from using an SqlExpression lambda. E.g:
        /// <para>db.Scalar&lt;Person, int&gt;(x =&gt; Sql.Max(x.Age))</para>
        /// </summary>
        public static TKey Scalar<T, TKey>(this IDbConnection dbConn, Expression<Func<T, TKey>> field)
        {
            return dbConn.Exec(dbCmd => dbCmd.Scalar(field));
        }

        /// <summary>
        /// Returns a scalar result from using an SqlExpression lambda. E.g:
        /// <para>db.Scalar&lt;Person, int&gt;(x =&gt; Sql.Max(x.Age), , x =&gt; x.Age &lt; 50)</para>
        /// </summary>        
        public static TKey Scalar<T, TKey>(this IDbConnection dbConn,
            Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.Scalar(field, predicate));
        }

        /// <summary>
        /// Returns the count of rows that match the LINQ expression, E.g:
        /// <para>db.Count&lt;Person&gt;(x =&gt; x.Age &lt; 50)</para>
        /// </summary>
        public static long Count<T>(this IDbConnection dbConn, Expression<Func<T, bool>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Count(expression));
        }

        /// <summary>
        /// Returns the count of rows that match the SqlExpression lambda, E.g:
        /// <para>db.Count&lt;Person&gt;(q =&gt; q.Where(x =&gt; x.Age &lt; 50))</para>
        /// </summary>
        public static long Count<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Count(expression));
        }

        /// <summary>
        /// Returns the count of rows that match the supplied SqlExpression, E.g:
        /// <para>db.Count(db.From&lt;Person&gt;().Where(x =&gt; x.Age &lt; 50))</para>
        /// </summary>
        public static long Count<T>(this IDbConnection dbConn, SqlExpression<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Count(expression));
        }

        public static long Count<T>(this IDbConnection dbConn)
        {
            var expression = dbConn.GetDialectProvider().SqlExpression<T>();
            return dbConn.Exec(dbCmd => dbCmd.Count(expression));
        }

        /// <summary>
        /// Return the number of rows returned by the supplied expression
        /// </summary>
        public static long RowCount<T>(this IDbConnection dbConn, SqlExpression<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.RowCount(expression));
        }

        /// <summary>
        /// Return the number of rows returned by the supplied sql
        /// </summary>
        public static long RowCount(this IDbConnection dbConn, string sql)
        {
            return dbConn.Exec(dbCmd => dbCmd.RowCount(sql));
        }

        /// <summary>
        /// Returns results with references from using a LINQ Expression. E.g:
        /// <para>db.LoadSelect&lt;Person&gt;(x =&gt; x.Age &gt; 40)</para>
        /// </summary>
        public static List<T> LoadSelect<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelect(predicate));
        }

        /// <summary>
        /// Returns results with references from using an SqlExpression lambda. E.g:
        /// <para>db.LoadSelect&lt;Person&gt;(q =&gt; q.Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static List<T> LoadSelect<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelect(expression));
        }

        /// <summary>
        /// Returns results with references from using an SqlExpression lambda. E.g:
        /// <para>db.LoadSelect(db.From&lt;Person&gt;().Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static List<T> LoadSelect<T>(this IDbConnection dbConn, SqlExpression<T> expression = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelect(expression));
        }

        /// <summary>
        /// Project results with references from a number of joined tables into a different model
        /// </summary>
        public static List<Into> LoadSelect<Into, From>(this IDbConnection dbConn, SqlExpression<From> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelect<Into, From>(expression));
        }
    }
}