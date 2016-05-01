#if NET45
// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite.Legacy
{
    public static class OrmLiteReadApiAsyncLegacy
    {
        /// <summary>
        /// Returns results from using an SqlFormat query. E.g:
        /// <para>db.SelectFmt&lt;Person&gt;("Age &gt; {0}", 40)</para>
        /// <para>db.SelectFmt&lt;Person&gt;("SELECT * FROM Person WHERE Age &gt; {0}", 40)</para>
        /// </summary>
        [Obsolete(Messages.LegacyApi)]
        public static Task<List<T>> SelectFmtAsync<T>(this IDbConnection dbConn, CancellationToken token, string sqlFormat, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectFmtAsync<T>(token, sqlFormat, filterParams));
        }
        [Obsolete(Messages.LegacyApi)]
        public static Task<List<T>> SelectFmtAsync<T>(this IDbConnection dbConn, string sqlFormat, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectFmtAsync<T>(default(CancellationToken), sqlFormat, filterParams));
        }

        /// <summary>
        /// Returns a partial subset of results from the specified tableType using a SqlFormat query. E.g:
        /// <para>db.SelectFmt&lt;EntityWithId&gt;(typeof(Person), "Age &gt; {0}", 40)</para>
        /// </summary>
        [Obsolete(Messages.LegacyApi)]
        public static Task<List<TModel>> SelectFmtAsync<TModel>(this IDbConnection dbConn, CancellationToken token, Type fromTableType, string sqlFormat, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectFmtAsync<TModel>(token, fromTableType, sqlFormat, filterParams));
        }
        [Obsolete(Messages.LegacyApi)]
        public static Task<List<TModel>> SelectFmtAsync<TModel>(this IDbConnection dbConn, Type fromTableType, string sqlFormat, params object[] filterParams)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectFmtAsync<TModel>(default(CancellationToken), fromTableType, sqlFormat, filterParams));
        }
    }
}

#endif