using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite.Legacy
{
    [Obsolete(Messages.LegacyApi)]
    internal static class ReadExpressionCommandExtensionsLegacy
    {
        [Obsolete("Use db.Select(db.From<T>())")]
        internal static List<T> Select<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            string sql = expression(q).SelectInto<T>();

            return dbCmd.ExprConvertToList<T>(sql, q.Params, onlyFields: q.OnlyFields);
        }

        [Obsolete("Use db.Select<Into,From>(db.From<From>())")]
        internal static List<Into> Select<Into, From>(this IDbCommand dbCmd, Func<SqlExpression<From>, SqlExpression<From>> expression)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<From>();
            string sql = expression(q).SelectInto<Into>();

            return dbCmd.ExprConvertToList<Into>(sql, q.Params, onlyFields: q.OnlyFields);
        }

        internal static List<Into> Select<Into, From>(this IDbCommand dbCmd, SqlExpression<From> q)
        {
            string sql = q.SelectInto<Into>();
            return dbCmd.ExprConvertToList<Into>(sql, q.Params, onlyFields: q.OnlyFields);
        }
    }
}