using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite.Legacy
{
    [Obsolete(Messages.LegacyApi)]
    public static class OrmLiteReadApiLegacy
    {
        /// <summary>
        /// Returns results from using an SqlFormat query. E.g:
        /// <para>db.SelectFmt&lt;Person&gt;("Age &gt; {0}", 40)</para>
        /// <para>db.SelectFmt&lt;Person&gt;("SELECT * FROM Person WHERE Age &gt; {0}", 40)</para>
        /// </summary>
        [Obsolete(Messages.LegacyApi)]
        public static List<T> SelectFmt<T>(this IDbConnection dbConn, string sqlFormat, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectFmt<T>(sqlFormat, filterParams));
        }

        /// <summary>
        /// Returns a partial subset of results from the specified tableType using a SqlFormat query. E.g:
        /// <para>db.SelectFmt&lt;EntityWithId&gt;(typeof(Person), "Age &gt; {0}", 40)</para>
        /// </summary>
        [Obsolete(Messages.LegacyApi)]
        public static List<TModel> SelectFmt<TModel>(this IDbConnection dbConn, Type fromTableType, string sqlFormat, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectFmt<TModel>(fromTableType, sqlFormat, filterParams));
        }

        /// <summary>
        /// Returns a lazyily loaded stream of results using an SqlFilter query. E.g:
        /// <para>db.SelectLazyFmt&lt;Person&gt;("Age &gt; {0}", 40)</para>
        /// </summary>
        [Obsolete(Messages.LegacyApi)]
        public static IEnumerable<T> SelectLazyFmt<T>(this IDbConnection dbConn, string sqlFormat, params object[] filterParams)
        {
            return dbConn.ExecLazy(dbCmd => dbCmd.SelectLazyFmt<T>(sqlFormat, filterParams));
        }
    }
}