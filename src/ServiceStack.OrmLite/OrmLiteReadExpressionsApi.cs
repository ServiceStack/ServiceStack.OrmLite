using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteReadExpressionsApi
    {
        public static T Exec<T>(this IDbConnection dbConn, Func<IDbCommand, T> filter)
        {
            return dbConn.GetExecFilter().Exec(dbConn, filter);
        }

        public static void Exec(this IDbConnection dbConn, Action<IDbCommand> filter)
        {
            dbConn.GetExecFilter().Exec(dbConn, filter);
        }

        public static Task<T> Exec<T>(this IDbConnection dbConn, Func<IDbCommand, Task<T>> filter)
        {
            return dbConn.GetExecFilter().Exec(dbConn, filter);
        }

        public static Task Exec(this IDbConnection dbConn, Func<IDbCommand, Task> filter)
        {
            return dbConn.GetExecFilter().Exec(dbConn, filter);
        }

        public static IEnumerable<T> ExecLazy<T>(this IDbConnection dbConn, Func<IDbCommand, IEnumerable<T>> filter)
        {
            return dbConn.GetExecFilter().ExecLazy(dbConn, filter);
        }

        public static IDbCommand Exec(this IDbConnection dbConn, Func<IDbCommand, IDbCommand> filter)
        {
            return dbConn.GetExecFilter().Exec(dbConn, filter);
        }

        public static Task<IDbCommand> Exec(this IDbConnection dbConn, Func<IDbCommand, Task<IDbCommand>> filter)
        {
            return dbConn.GetExecFilter().Exec(dbConn, filter);
        }

        /// <summary>
        /// Creates a new SqlExpression builder allowing typed LINQ-like queries.
        /// Alias for SqlExpression.
        /// </summary>
        public static SqlExpression<T> From<T>(this IDbConnection dbConn)
        {
            return dbConn.GetExecFilter().SqlExpression<T>(dbConn);
        }

        public static SqlExpression<T> From<T, JoinWith>(this IDbConnection dbConn, Expression<Func<T, JoinWith, bool>> joinExpr=null)
        {
            var sql = dbConn.GetExecFilter().SqlExpression<T>(dbConn);
            sql.Join<T,JoinWith>(joinExpr);
            return sql;
        }

        /// <summary>
        /// Creates a new SqlExpression builder for the specified type using a user-defined FROM sql expression.
        /// </summary>
        public static SqlExpression<T> From<T>(this IDbConnection dbConn, string fromExpression)
        {
            var expr = dbConn.GetExecFilter().SqlExpression<T>(dbConn);
            expr.From(fromExpression);
            return expr;
        }

        public static JoinFormatDelegate JoinAlias(this IDbConnection dbConn, string alias)
        {
            return (dialect, tableDef, expr) =>
                $"{dialect.GetQuotedTableName(tableDef)} {alias} {expr.Replace(dialect.GetQuotedTableName(tableDef), dialect.GetQuotedTableName(alias))}";
        }

        public static string GetTableName<T>(this IDbConnection db)
        {
            return db.GetDialectProvider().GetTableName(ModelDefinition<T>.Definition);
        }

        public static string GetQuotedTableName<T>(this IDbConnection db)
        {
            return db.GetDialectProvider().GetQuotedTableName(ModelDefinition<T>.Definition);
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
        /// Create a managed OrmLite IDbCommand
        /// </summary>
        public static IDbCommand OpenCommand(this IDbConnection dbConn)
        {
            return dbConn.GetExecFilter().CreateCommand(dbConn);
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
        /// <para>db.Select(db.From&lt;Person&gt;().Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, SqlExpression<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(expression));
        }

        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select(db.From&lt;Person&gt;().Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, ISqlExpression expression, object anonType = null)
        {
            if (anonType != null)
                return dbConn.Exec(dbCmd => dbCmd.SqlList<T>(expression.SelectInto<T>(), anonType));

            if (expression.Params != null && expression.Params.Any())
                return dbConn.Exec(dbCmd => dbCmd.SqlList<T>(expression.SelectInto<T>(), expression.Params.ToDictionary(param => param.ParameterName, param => param.Value)));

            return dbConn.Exec(dbCmd => dbCmd.SqlList<T>(expression.SelectInto<T>(), expression.Params));
        }

        public static List<Tuple<T, T2>> SelectMulti<T, T2>(this IDbConnection dbConn, SqlExpression<T> expression) => dbConn.Exec(dbCmd => dbCmd.SelectMulti<T, T2>(expression));

        public static List<Tuple<T, T2, T3>> SelectMulti<T, T2, T3>(this IDbConnection dbConn, SqlExpression<T> expression) => dbConn.Exec(dbCmd => dbCmd.SelectMulti<T, T2, T3>(expression));

        public static List<Tuple<T, T2, T3, T4>> SelectMulti<T, T2, T3, T4>(this IDbConnection dbConn, SqlExpression<T> expression) => dbConn.Exec(dbCmd => dbCmd.SelectMulti<T, T2, T3, T4>(expression));

        public static List<Tuple<T, T2, T3, T4, T5>> SelectMulti<T, T2, T3, T4, T5>(this IDbConnection dbConn, SqlExpression<T> expression) => dbConn.Exec(dbCmd => dbCmd.SelectMulti<T, T2, T3, T4, T5>(expression));

        public static List<Tuple<T, T2, T3, T4, T5, T6>> SelectMulti<T, T2, T3, T4, T5, T6>(this IDbConnection dbConn, SqlExpression<T> expression) => dbConn.Exec(dbCmd => dbCmd.SelectMulti<T, T2, T3, T4, T5, T6>(expression));

        public static List<Tuple<T, T2, T3, T4, T5, T6, T7>> SelectMulti<T, T2, T3, T4, T5, T6, T7>(this IDbConnection dbConn, SqlExpression<T> expression) => dbConn.Exec(dbCmd => dbCmd.SelectMulti<T, T2, T3, T4, T5, T6, T7>(expression));


        public static List<Tuple<T, T2>> SelectMulti<T, T2>(this IDbConnection dbConn, SqlExpression<T> expression, string[] tableSelects) => dbConn.Exec(dbCmd => dbCmd.SelectMulti<T, T2>(expression, tableSelects));

        public static List<Tuple<T, T2, T3>> SelectMulti<T, T2, T3>(this IDbConnection dbConn, SqlExpression<T> expression, string[] tableSelects) => dbConn.Exec(dbCmd => dbCmd.SelectMulti<T, T2, T3>(expression, tableSelects));

        public static List<Tuple<T, T2, T3, T4>> SelectMulti<T, T2, T3, T4>(this IDbConnection dbConn, SqlExpression<T> expression, string[] tableSelects) => dbConn.Exec(dbCmd => dbCmd.SelectMulti<T, T2, T3, T4>(expression, tableSelects));

        public static List<Tuple<T, T2, T3, T4, T5>> SelectMulti<T, T2, T3, T4, T5>(this IDbConnection dbConn, SqlExpression<T> expression, string[] tableSelects) => dbConn.Exec(dbCmd => dbCmd.SelectMulti<T, T2, T3, T4, T5>(expression, tableSelects));

        public static List<Tuple<T, T2, T3, T4, T5, T6>> SelectMulti<T, T2, T3, T4, T5, T6>(this IDbConnection dbConn, SqlExpression<T> expression, string[] tableSelects) => dbConn.Exec(dbCmd => dbCmd.SelectMulti<T, T2, T3, T4, T5, T6>(expression, tableSelects));

        public static List<Tuple<T, T2, T3, T4, T5, T6, T7>> SelectMulti<T, T2, T3, T4, T5, T6, T7>(this IDbConnection dbConn, SqlExpression<T> expression, string[] tableSelects) => dbConn.Exec(dbCmd => dbCmd.SelectMulti<T, T2, T3, T4, T5, T6, T7>(expression, tableSelects));

        /// <summary>
        /// Returns a single result from using a LINQ Expression. E.g:
        /// <para>db.Single&lt;Person&gt;(x =&gt; x.Age == 42)</para>
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single(predicate));
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
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Single(db.From&lt;Person&gt;().Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, ISqlExpression expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single<T>(expression.SelectInto<T>(), expression.Params));
        }

        /// <summary>
        /// Returns a scalar result from using an SqlExpression lambda. E.g:
        /// <para>db.Scalar&lt;Person, int&gt;(x =&gt; Sql.Max(x.Age))</para>
        /// </summary>
        public static TKey Scalar<T, TKey>(this IDbConnection dbConn, Expression<Func<T, object>> field)
        {
            return dbConn.Exec(dbCmd => dbCmd.Scalar<T, TKey>(field));
        }

        /// <summary>
        /// Returns a scalar result from using an SqlExpression lambda. E.g:
        /// <para>db.Scalar&lt;Person, int&gt;(x =&gt; Sql.Max(x.Age), , x =&gt; x.Age &lt; 50)</para>
        /// </summary>        
        public static TKey Scalar<T, TKey>(this IDbConnection dbConn,
            Expression<Func<T, object>> field, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.Scalar<T, TKey>(field, predicate));
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
        public static long RowCount(this IDbConnection dbConn, string sql, object anonType = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.RowCount(sql, anonType));
        }

        /// <summary>
        /// Return the number of rows returned by the supplied sql and db params
        /// </summary>
        public static long RowCount(this IDbConnection dbConn, string sql, IEnumerable<IDbDataParameter> sqlParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.RowCount(sql, sqlParams));
        }

        /// <summary>
        /// Returns results with references from using a LINQ Expression. E.g:
        /// <para>db.LoadSelect&lt;Person&gt;(x =&gt; x.Age &gt; 40)</para>
        /// </summary>
        public static List<T> LoadSelect<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate, string[] include = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelect(predicate, include));
        }

        /// <summary>
        /// Returns results with references from using a LINQ Expression. E.g:
        /// <para>db.LoadSelect&lt;Person&gt;(x =&gt; x.Age &gt; 40, include: x => new { x.PrimaryAddress })</para>
        /// </summary>
        public static List<T> LoadSelect<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> include)
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelect(predicate, include.GetFieldNames()));
        }

        /// <summary>
        /// Returns results with references from using an SqlExpression lambda. E.g:
        /// <para>db.LoadSelect(db.From&lt;Person&gt;().Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static List<T> LoadSelect<T>(this IDbConnection dbConn, SqlExpression<T> expression = null, string[] include = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelect(expression, include));
        }

        /// <summary>
        /// Returns results with references from using an SqlExpression lambda. E.g:
        /// <para>db.LoadSelect(db.From&lt;Person&gt;().Where(x =&gt; x.Age &gt; 40), include:q.OnlyFields)</para>
        /// </summary>
        public static List<T> LoadSelect<T>(this IDbConnection dbConn, SqlExpression<T> expression, IEnumerable<string> include)
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelect(expression, include));
        }

        /// <summary>
        /// Returns results with references from using an SqlExpression lambda. E.g:
        /// <para>db.LoadSelect(db.From&lt;Person&gt;().Where(x =&gt; x.Age &gt; 40), include: x => new { x.PrimaryAddress })</para>
        /// </summary>
        public static List<T> LoadSelect<T>(this IDbConnection dbConn, SqlExpression<T> expression, Expression<Func<T, object>> include)
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelect(expression, include.GetFieldNames()));
        }

        /// <summary>
        /// Project results with references from a number of joined tables into a different model
        /// </summary>
        public static List<Into> LoadSelect<Into, From>(this IDbConnection dbConn, SqlExpression<From> expression, string[] include = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelect<Into, From>(expression, include));
        }

        /// <summary>
        /// Project results with references from a number of joined tables into a different model
        /// </summary>
        public static List<Into> LoadSelect<Into, From>(this IDbConnection dbConn, SqlExpression<From> expression, IEnumerable<string> include)
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelect<Into, From>(expression, include));
        }

        /// <summary>
        /// Project results with references from a number of joined tables into a different model
        /// </summary>
        public static List<Into> LoadSelect<Into, From>(this IDbConnection dbConn, SqlExpression<From> expression, Expression<Func<Into, object>> include)
        {
            return dbConn.Exec(dbCmd => dbCmd.LoadSelect<Into, From>(expression, include.GetFieldNames()));
        }
    }
}