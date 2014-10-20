using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public static class WriteExtensions
    {
        public static int UpdateOnly<T>(this IDbCommand dbCmd, T model, Func<SqlExpression<T>, SqlExpression<T>> onlyFields)
        {
            return dbCmd.UpdateOnly(model, onlyFields(OrmLiteConfig.DialectProvider.SqlExpression<T>()));
        }

        public static int UpdateOnly<T>(this IDbCommand dbCmd, T model, SqlExpression<T> onlyFields)
        {
            var sql = UpdateOnlySql(dbCmd, model, onlyFields);
            return dbCmd.ExecuteSql(sql);
        }

        internal static string UpdateOnlySql<T>(this IDbCommand dbCmd, T model, SqlExpression<T> onlyFields)
        {
            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, model);

            var fieldsToUpdate = onlyFields.UpdateFields.Count == 0
                ? onlyFields.GetAllFields()
                : onlyFields.UpdateFields;

            var sql = OrmLiteConfig.DialectProvider.ToUpdateRowStatement(model, fieldsToUpdate);

            if (!onlyFields.WhereExpression.IsNullOrEmpty()) sql += " " + onlyFields.WhereExpression;
            return sql;
        }

        public static int UpdateOnly<T, TKey>(this IDbCommand dbCmd, T obj,
            Expression<Func<T, TKey>> onlyFields = null,
            Expression<Func<T, bool>> where = null)
        {
            if (onlyFields == null)
                throw new ArgumentNullException("onlyFields");

            var q = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            q.Update(onlyFields);
            q.Where(where);
            return dbCmd.UpdateOnly(obj, q);
        }

        public static int UpdateNonDefaults<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> obj)
        {
            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, item);

            var q = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            q.Where(obj);
            var sql = q.ToUpdateStatement(item, excludeDefaults: true);
            return dbCmd.ExecuteSql(sql);
        }

        public static int Update<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> expression)
        {
            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, item);

            var q = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            q.Where(expression);
            var sql = q.ToUpdateStatement(item);
            return dbCmd.ExecuteSql(sql);
        }

        public static int Update<T>(this IDbCommand dbCmd, object updateOnly, Expression<Func<T, bool>> where = null)
        {
            var updateSql = UpdateSql(updateOnly, where);
            return dbCmd.ExecuteSql(updateSql);
        }

        internal static string UpdateSql<T>(object updateOnly, Expression<Func<T, bool>> @where)
        {
            var dialectProvider = OrmLiteConfig.DialectProvider;
            var ev = dialectProvider.SqlExpression<T>();
            var whereSql = ev.Where(@where).WhereExpression;
            var sql = new StringBuilder();
            var modelDef = typeof(T).GetModelDefinition();
            var fields = modelDef.FieldDefinitionsArray;

            foreach (var setField in updateOnly.GetType().GetPublicProperties())
            {
                var fieldDef = fields.FirstOrDefault(x =>
                                                     string.Equals(x.Name, setField.Name, StringComparison.OrdinalIgnoreCase));
                if (fieldDef == null || fieldDef.ShouldSkipUpdate()) continue;

                if (sql.Length > 0)
                    sql.Append(", ");

                sql.AppendFormat("{0}={1}",
                    dialectProvider.GetQuotedColumnName(fieldDef.FieldName),
                    dialectProvider.GetQuotedValue(setField.GetPropertyGetterFn()(updateOnly), fieldDef.FieldType));
            }

            var updateSql = string.Format("UPDATE {0} SET {1} {2}",
                dialectProvider.GetQuotedTableName(modelDef), sql, whereSql);
            return updateSql;
        }

        public static int UpdateFmt<T>(this IDbCommand dbCmd, string set = null, string where = null)
        {
            return dbCmd.UpdateFmt(typeof(T).GetModelDefinition().ModelName, set, where);
        }

        public static int UpdateFmt(this IDbCommand dbCmd, string table = null, string set = null, string where = null)
        {
            var sql = UpdateFmtSql(table, set, @where);
            return dbCmd.ExecuteSql(sql.ToString());
        }

        internal static StringBuilder UpdateFmtSql(string table, string set, string @where)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (set == null)
                throw new ArgumentNullException("set");

            var sql = new StringBuilder("UPDATE ");
            sql.Append(OrmLiteConfig.DialectProvider.GetQuotedTableName(table));
            sql.Append(" SET ");
            sql.Append(set.SqlVerifyFragment());
            if (!string.IsNullOrEmpty(@where))
            {
                sql.Append(" WHERE ");
                sql.Append(@where.SqlVerifyFragment());
            }
            return sql;
        }

        public static void InsertOnly<T>(this IDbCommand dbCmd, T obj, Func<SqlExpression<T>, SqlExpression<T>> onlyFields)
        {
            dbCmd.InsertOnly(obj, onlyFields(OrmLiteConfig.DialectProvider.SqlExpression<T>()));
        }

        public static void InsertOnly<T>(this IDbCommand dbCmd, T obj, SqlExpression<T> onlyFields)
        {
            if (OrmLiteConfig.InsertFilter != null)
                OrmLiteConfig.InsertFilter(dbCmd, obj);

            var sql = OrmLiteConfig.DialectProvider.ToInsertRowStatement(dbCmd, obj, onlyFields.InsertFields);
            dbCmd.ExecuteSql(sql);
        }

        public static int Delete<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> where)
        {
            var ev = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            ev.Where(where);
            return dbCmd.Delete(ev);
        }

        public static int Delete<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> where)
        {
            return dbCmd.Delete(where(OrmLiteConfig.DialectProvider.SqlExpression<T>()));
        }

        public static int Delete<T>(this IDbCommand dbCmd, SqlExpression<T> where)
        {
            var sql = where.ToDeleteRowStatement();
            return dbCmd.ExecuteSql(sql);
        }

        public static int DeleteFmt<T>(this IDbCommand dbCmd, string where = null)
        {
            return dbCmd.DeleteFmt(typeof(T).GetModelDefinition().ModelName, where);
        }

        public static int DeleteFmt(this IDbCommand dbCmd, string table = null, string where = null)
        {
            var sql = DeleteFmtSql(table, @where);
            return dbCmd.ExecuteSql(sql.ToString());
        }

        internal static StringBuilder DeleteFmtSql(string table, string @where)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (@where == null)
                throw new ArgumentNullException("where");

            var sql = new StringBuilder();
            sql.AppendFormat("DELETE FROM {0} WHERE {1}",
                             OrmLiteConfig.DialectProvider.GetQuotedTableName(table),
                             @where.SqlVerifyFragment());
            return sql;
        }
    }
}

