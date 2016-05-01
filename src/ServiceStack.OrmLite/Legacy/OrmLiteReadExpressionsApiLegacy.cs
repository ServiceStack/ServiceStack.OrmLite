using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite.Legacy
{
    public static class OrmLiteReadExpressionsApiLegacy
    {
        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select&lt;Person&gt;(q =&gt; q.Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        [Obsolete("Use db.Select(db.From<T>())")]
        public static List<T> Select<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(expression));
        }

        /// <summary>
        /// Project results from a number of joined tables into a different model
        /// </summary>
        [Obsolete("Use db.Select<Into>(db.From<From>())")]
        public static List<Into> Select<Into, From>(this IDbConnection dbConn, SqlExpression<From> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<Into, From>(expression));
        }

        /// <summary>
        /// Project results from a number of joined tables into a different model
        /// </summary>
        [Obsolete("Use db.Select<Into, From>(db.From<T>())")]
        public static List<Into> Select<Into, From>(this IDbConnection dbConn, Func<SqlExpression<From>, SqlExpression<From>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select<Into, From>(expression));
        }
    }
}