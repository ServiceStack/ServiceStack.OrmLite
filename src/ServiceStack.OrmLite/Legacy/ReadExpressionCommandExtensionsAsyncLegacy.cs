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
    internal static class ReadExpressionCommandExtensionsAsyncLegacy
    {
        [Obsolete("Use db.SelectAsync(db.From<T>())")]
        internal static Task<List<T>> SelectAsync<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression, CancellationToken token)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            var sql = expression(q).SelectInto<T>();
            return dbCmd.ExprConvertToListAsync<T>(sql, q.Params, token);
        }

        [Obsolete("Use db.SelectAsync(db.From<T>())")]
        internal static Task<List<Into>> SelectAsync<Into, From>(this IDbCommand dbCmd, Func<SqlExpression<From>, SqlExpression<From>> expression, CancellationToken token)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<From>();
            string sql = expression(q).SelectInto<Into>();
            return dbCmd.ExprConvertToListAsync<Into>(sql, q.Params, token);
        }

    }
}

#endif