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

        [Obsolete("Use db.UpdateOnly(model, db.From<T>())")]
        public static int UpdateOnly<T>(this IDbCommand dbCmd, T model, Func<SqlExpression<T>, SqlExpression<T>> onlyFields)
        {
            return dbCmd.UpdateOnly(model, onlyFields(dbCmd.GetDialectProvider().SqlExpression<T>()));
        }
    }
}