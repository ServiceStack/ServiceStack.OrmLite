using System;
using System.Data;
using ServiceStack.Text;

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

        public static int UpdateFmt<T>(this IDbCommand dbCmd, string set = null, string where = null)
        {
            return dbCmd.UpdateFmt(typeof(T).GetModelDefinition().ModelName, set, where);
        }

        public static int UpdateFmt(this IDbCommand dbCmd, string table = null, string set = null, string where = null)
        {
            var sql = UpdateFmtSql(dbCmd.GetDialectProvider(), table, set, @where);
            return dbCmd.ExecuteSql(sql);
        }

        internal static string UpdateFmtSql(IOrmLiteDialectProvider dialectProvider, string table, string set, string @where)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (set == null)
                throw new ArgumentNullException("set");

            var sql = StringBuilderCache.Allocate();
            sql.Append("UPDATE ");
            sql.Append(dialectProvider.GetQuotedTableName(table));
            sql.Append(" SET ");
            sql.Append(set.SqlVerifyFragment());
            if (!string.IsNullOrEmpty(@where))
            {
                sql.Append(" WHERE ");
                sql.Append(@where.SqlVerifyFragment());
            }
            return StringBuilderCache.ReturnAndFree(sql);
        }
    }
}