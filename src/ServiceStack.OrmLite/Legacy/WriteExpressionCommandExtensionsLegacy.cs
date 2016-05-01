using System;
using System.Data;

namespace ServiceStack.OrmLite.Legacy
{
    [Obsolete(Messages.LegacyApi)]
    internal static class WriteExpressionCommandExtensionsLegacy
    {
        [Obsolete("Use db.InsertOnly(obj, db.From<T>())")]
        public static void InsertOnly<T>(this IDbCommand dbCmd, T obj, Func<SqlExpression<T>, SqlExpression<T>> onlyFields)
        {
            dbCmd.InsertOnly(obj, onlyFields(dbCmd.GetDialectProvider().SqlExpression<T>()));
        }
    }
}