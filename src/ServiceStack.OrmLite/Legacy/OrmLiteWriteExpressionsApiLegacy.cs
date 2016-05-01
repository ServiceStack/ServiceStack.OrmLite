using System;
using System.Data;

namespace ServiceStack.OrmLite.Legacy
{
    public static class OrmLiteWriteExpressionsApiLegacy
    {
        /// <summary>
        /// Insert only fields in POCO specified by the SqlExpression lambda. E.g:
        /// <para>db.InsertOnly(new Person { FirstName = "Amy", Age = 27 }, q =&gt; q.Insert(p =&gt; new { p.FirstName, p.Age }))</para>
        /// </summary>
        [Obsolete("Use db.InsertOnly(obj, db.From<T>())")]
        public static void InsertOnly<T>(this IDbConnection dbConn, T obj, Func<SqlExpression<T>, SqlExpression<T>> onlyFields)
        {
            dbConn.Exec(dbCmd => dbCmd.InsertOnly(obj, onlyFields));
        }
    }
}