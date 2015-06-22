#if NET45
using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteWriteExpressionsApiAsync
    {
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
        public static Task<int> UpdateOnlyAsync<T>(this IDbConnection dbConn, T model, Func<SqlExpression<T>, SqlExpression<T>> onlyFields, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateOnlyAsync(model, onlyFields, token));
        }

        /// <summary>
        /// Use an SqlExpression to select which fields to update and construct the where expression, E.g: 
        /// 
        ///   var q = db.From&gt;Person&lt;());
        ///   db.UpdateOnly(new Person { FirstName = "JJ" }, q.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
        ///   UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("FirstName" = 'Jimi')
        /// 
        ///   What's not in the update expression doesn't get updated. No where expression updates all rows. E.g:
        /// 
        ///   db.UpdateOnly(new Person { FirstName = "JJ", LastName = "Hendo" }, ev.Update(p => p.FirstName));
        ///   UPDATE "Person" SET "FirstName" = 'JJ'
        /// </summary>
        public static Task<int> UpdateOnlyAsync<T>(this IDbConnection dbConn, T model, SqlExpression<T> onlyFields, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateOnlyAsync(model, onlyFields, token));
        }

        /// <summary>
        /// Update record, updating only fields specified in updateOnly that matches the where condition (if any), E.g:
        /// 
        ///   db.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName, p => p.LastName == "Hendrix");
        ///   UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("LastName" = 'Hendrix')
        ///
        ///   db.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName);
        ///   UPDATE "Person" SET "FirstName" = 'JJ'
        /// </summary>
        public static Task<int> UpdateOnlyAsync<T, TKey>(this IDbConnection dbConn, T obj,
            Expression<Func<T, TKey>> onlyFields = null,
            Expression<Func<T, bool>> where = null,
            CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateOnlyAsync(obj, onlyFields, where, token));
        }

        /// <summary>
        /// Updates all non-default values set on item matching the where condition (if any). E.g
        /// 
        ///   db.UpdateNonDefaults(new Person { FirstName = "JJ" }, p => p.FirstName == "Jimi");
        ///   UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("FirstName" = 'Jimi')
        /// </summary>
        public static Task<int> UpdateNonDefaultsAsync<T>(this IDbConnection dbConn, T item, Expression<Func<T, bool>> obj, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateNonDefaultsAsync(item, obj, token));
        }

        /// <summary>
        /// Updates all values set on item matching the where condition (if any). E.g
        /// 
        ///   db.Update(new Person { Id = 1, FirstName = "JJ" }, p => p.LastName == "Hendrix");
        ///   UPDATE "Person" SET "Id" = 1,"FirstName" = 'JJ',"LastName" = NULL,"Age" = 0 WHERE ("LastName" = 'Hendrix')
        /// </summary>
        public static Task<int> UpdateAsync<T>(this IDbConnection dbConn, T item, Expression<Func<T, bool>> where, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateAsync(item, where, token));
        }

        /// <summary>
        /// Updates all matching fields populated on anonymousType that matches where condition (if any). E.g:
        /// 
        ///   db.Update&lt;Person&gt;(new { FirstName = "JJ" }, p => p.LastName == "Hendrix");
        ///   UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("LastName" = 'Hendrix')
        /// </summary>
        public static Task<int> UpdateAsync<T>(this IDbConnection dbConn, object updateOnly, Expression<Func<T, bool>> where = null, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateAsync(updateOnly, where, token));
        }

        /// <summary>
        /// Flexible Update method to succinctly execute a free-text update statement using optional params. E.g:
        /// 
        ///   db.Update&lt;Person&gt;(set:"FirstName = {0}".Params("JJ"), where:"LastName = {0}".Params("Hendrix"));
        ///   UPDATE "Person" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'
        /// </summary>
        public static Task<int> UpdateFmtAsync<T>(this IDbConnection dbConn, string set = null, string where = null, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateFmtAsync<T>(set, where, token));
        }

        /// <summary>
        /// Flexible Update method to succinctly execute a free-text update statement using optional params. E.g.
        /// 
        ///   db.Update(table:"Person", set: "FirstName = {0}".Params("JJ"), where: "LastName = {0}".Params("Hendrix"));
        ///   UPDATE "Person" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'
        /// </summary>
        public static Task<int> UpdateFmtAsync(this IDbConnection dbConn, string table = null, string set = null, string where = null, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.UpdateFmtAsync(table, set, where, token));
        }

        /// <summary>
        /// Insert only fields in POCO specified by the SqlExpression lambda. E.g:
        /// <para>db.InsertOnly(new Person { FirstName = "Amy", Age = 27 }, q =&gt; q.Insert(p =&gt; new { p.FirstName, p.Age }))</para>
        /// </summary>
        public static Task InsertOnlyAsync<T>(this IDbConnection dbConn, T obj, Func<SqlExpression<T>, SqlExpression<T>> onlyFields, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertOnlyAsync(obj, onlyFields, token));
        }

        /// <summary>
        /// Using an SqlExpression to only Insert the fields specified, e.g:
        /// 
        ///   db.InsertOnly(new Person { FirstName = "Amy" }, q => q.Insert(p => new { p.FirstName }));
        ///   INSERT INTO "Person" ("FirstName") VALUES ('Amy');
        /// </summary>
        public static Task InsertOnlyAsync<T>(this IDbConnection dbConn, T obj, SqlExpression<T> onlyFields, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.InsertOnlyAsync(obj, onlyFields, token));
        }

        /// <summary>
        /// Delete the rows that matches the where expression, e.g:
        /// 
        ///   db.Delete&lt;Person&gt;(p => p.Age == 27);
        ///   DELETE FROM "Person" WHERE ("Age" = 27)
        /// </summary>
        public static Task<int> DeleteAsync<T>(this IDbConnection dbConn, Expression<Func<T, bool>> where, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync(where, token));
        }

        /// <summary>
        /// Delete the rows that matches the where expression, e.g:
        /// 
        ///   db.Delete&lt;Person&gt;(ev => ev.Where(p => p.Age == 27));
        ///   DELETE FROM "Person" WHERE ("Age" = 27)
        /// </summary>
        public static Task<int> DeleteAsync<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> where, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync(where, token));
        }

        /// <summary>
        /// Delete the rows that matches the where expression, e.g:
        /// 
        ///   var q = db.From&gt;Person&lt;());
        ///   db.Delete&lt;Person&gt;(q.Where(p => p.Age == 27));
        ///   DELETE FROM "Person" WHERE ("Age" = 27)
        /// </summary>
        public static Task<int> DeleteAsync<T>(this IDbConnection dbConn, SqlExpression<T> where, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteAsync(where, token));
        }

        /// <summary>
        /// Flexible Delete method to succinctly execute a delete statement using free-text where expression. E.g.
        /// 
        ///   db.Delete&lt;Person&gt;(where:"Age = {0}".Params(27));
        ///   DELETE FROM "Person" WHERE Age = 27
        /// </summary>
        public static Task<int> DeleteFmtAsync<T>(this IDbConnection dbConn, string where, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteFmtAsync<T>(where, token));
        }
        public static Task<int> DeleteFmtAsync<T>(this IDbConnection dbConn, string where = null)
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteFmtAsync<T>(where, default(CancellationToken)));
        }

        /// <summary>
        /// Flexible Delete method to succinctly execute a delete statement using free-text where expression. E.g.
        /// 
        ///   db.Delete(table:"Person", where: "Age = {0}".Params(27));
        ///   DELETE FROM "Person" WHERE Age = 27
        /// </summary>
        public static Task<int> DeleteFmtAsync(this IDbConnection dbConn, string table = null, string where = null, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.DeleteFmtAsync(table, where, token));
        }
    }
}
#endif