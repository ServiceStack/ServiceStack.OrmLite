#if NET45
// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite.Legacy
{
    [Obsolete(Messages.LegacyApi)]
    internal static class OrmLiteReadCommandExtensionsAsyncLegacy
    {
        internal static Task<T> SingleFmtAsync<T>(this IDbCommand dbCmd, CancellationToken token, string filter, params object[] filterParams)
        {
            return dbCmd.ConvertToAsync<T>(dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), filter, filterParams), token);
        }

        internal static Task<List<T>> SelectFmtAsync<T>(this IDbCommand dbCmd, CancellationToken token, string sqlFilter, params object[] filterParams)
        {
            return dbCmd.ConvertToListAsync<T>(
                dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sqlFilter, filterParams), token);
        }

        internal static Task<List<TModel>> SelectFmtAsync<TModel>(this IDbCommand dbCmd, CancellationToken token, Type fromTableType, string sqlFilter, params object[] filterParams)
        {
            var sql = OrmLiteReadCommandExtensionsLegacy.ToSelectFmt<TModel>(dbCmd.GetDialectProvider(), fromTableType, sqlFilter, filterParams);
            return dbCmd.ConvertToListAsync<TModel>(sql, token);
        }
    }
}

#endif