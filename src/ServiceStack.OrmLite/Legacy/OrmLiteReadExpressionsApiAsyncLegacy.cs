#if NET45
// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite.Legacy
{
    [Obsolete(Messages.LegacyApi)]
    public static class OrmLiteReadExpressionsApiAsyncLegacy
    {
        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select&lt;Person&gt;(q =&gt; q.Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        [Obsolete("Use db.SelectAsync(db.From<T>())")]
        public static Task<List<T>> SelectAsync<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectAsync(expression, token));
        }

        /// <summary>
        /// Project results from a number of joined tables into a different model
        /// </summary>
        [Obsolete("Use db.SelectAsync<Into, From>(db.From<T>())")]
        public static Task<List<Into>> SelectAsync<Into, From>(this IDbConnection dbConn, Func<SqlExpression<From>, SqlExpression<From>> expression, CancellationToken token = default(CancellationToken))
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectAsync<Into, From>(expression, token));
        }
    }
}

#endif