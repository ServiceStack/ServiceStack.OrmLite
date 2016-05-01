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

        /// <summary>
        /// Use an SqlExpression to select which fields to update and construct the where expression, E.g: 
        /// 
        ///   db.UpdateOnly(new Person { FirstName = "JJ" }, ev => ev.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
        ///   UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("FirstName" = 'Jimi')
        /// 
        ///   What's not in the update expression doesn't get updated. No where expression updates all rows. E.g:
        /// 
        ///   db.UpdateOnly(new Person { FirstName = "JJ", LastName = "Hendo" }, ev => ev.Update(p => p.FirstName));
        ///   UPDATE "Person" SET "FirstName" = 'JJ'
        /// </summary>
        [Obsolete("Use db.UpdateOnly(model, db.From<T>())")]
        public static int UpdateOnly<T>(this IDbConnection dbConn, T model, Func<SqlExpression<T>, SqlExpression<T>> onlyFields)
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateOnly(model, onlyFields));
        }
    }
}