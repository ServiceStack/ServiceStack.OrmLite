#if NET45
// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
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
    }
}

#endif