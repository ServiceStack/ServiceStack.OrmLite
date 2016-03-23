using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    internal static class WriteExpressionCommandExtensions
    {
        [Obsolete("Use db.UpdateOnly(model, db.From<T>())")]
        public static int UpdateOnly<T>(this IDbCommand dbCmd, T model, Func<SqlExpression<T>, SqlExpression<T>> onlyFields)
        {
            return dbCmd.UpdateOnly(model, onlyFields(dbCmd.GetDialectProvider().SqlExpression<T>()));
        }

        public static int UpdateOnly<T>(this IDbCommand dbCmd, T model, SqlExpression<T> onlyFields)
        {
            UpdateOnlySql(dbCmd, model, onlyFields);
            return dbCmd.ExecNonQuery();
        }

        internal static void UpdateOnlySql<T>(this IDbCommand dbCmd, T model, SqlExpression<T> onlyFields)
        {
            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, model);

            var fieldsToUpdate = onlyFields.UpdateFields.Count == 0
                ? onlyFields.GetAllFields()
                : onlyFields.UpdateFields;

            onlyFields.CopyParamsTo(dbCmd);

            dbCmd.GetDialectProvider().PrepareUpdateRowStatement(dbCmd, model, fieldsToUpdate);

            if (!onlyFields.WhereExpression.IsNullOrEmpty())
                dbCmd.CommandText += " " + onlyFields.WhereExpression;
        }

        public static int UpdateOnly<T, TKey>(this IDbCommand dbCmd, T obj,
            Expression<Func<T, TKey>> onlyFields = null,
            Expression<Func<T, bool>> where = null)
        {
            if (onlyFields == null)
                throw new ArgumentNullException("onlyFields");

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Update(onlyFields);
            q.Where(where);
            return dbCmd.UpdateOnly(obj, q);
        }

        public static int UpdateNonDefaults<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> obj)
        {
            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, item);

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(obj);
            q.PrepareUpdateStatement(dbCmd, item, excludeDefaults: true);
            return dbCmd.ExecNonQuery();
        }

        public static int Update<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> expression)
        {
            if (OrmLiteConfig.UpdateFilter != null)
                OrmLiteConfig.UpdateFilter(dbCmd, item);

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(expression);
            q.PrepareUpdateStatement(dbCmd, item);
            return dbCmd.ExecNonQuery();
        }

        public static int Update<T>(this IDbCommand dbCmd, object updateOnly, Expression<Func<T, bool>> where = null)
        {
            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            var whereSql = q.Where(where).WhereExpression;
            q.CopyParamsTo(dbCmd);
            dbCmd.PrepareUpdateAnonSql<T>(dbCmd.GetDialectProvider(), updateOnly, whereSql);

            return dbCmd.ExecNonQuery();
        }

        internal static void PrepareUpdateAnonSql<T>(this IDbCommand dbCmd, IOrmLiteDialectProvider dialectProvider, object updateOnly, string whereSql)
        {
            var sql = new StringBuilder();
            var modelDef = typeof(T).GetModelDefinition();
            var fields = modelDef.FieldDefinitionsArray;

            foreach (var setField in updateOnly.GetType().GetPublicProperties())
            {
                var fieldDef = fields.FirstOrDefault(x => string.Equals(x.Name, setField.Name, StringComparison.OrdinalIgnoreCase));
                if (fieldDef == null || fieldDef.ShouldSkipUpdate()) continue;

                if (sql.Length > 0)
                    sql.Append(", ");

                var value = setField.GetPropertyGetterFn()(updateOnly);
                sql
                    .Append(dialectProvider.GetQuotedColumnName(fieldDef.FieldName))
                    .Append("=")
                    .Append(dialectProvider.AddParam(dbCmd, value, fieldDef.ColumnType).ParameterName);
            }

            dbCmd.CommandText = string.Format("UPDATE {0} SET {1} {2}",
                dialectProvider.GetQuotedTableName(modelDef), sql, whereSql);
        }

        public static int UpdateFmt<T>(this IDbCommand dbCmd, string set = null, string where = null)
        {
            return dbCmd.UpdateFmt(typeof(T).GetModelDefinition().ModelName, set, where);
        }

        public static int UpdateFmt(this IDbCommand dbCmd, string table = null, string set = null, string where = null)
        {
            var sql = UpdateFmtSql(dbCmd.GetDialectProvider(), table, set, @where);
            return dbCmd.ExecuteSql(sql.ToString());
        }

        internal static StringBuilder UpdateFmtSql(IOrmLiteDialectProvider dialectProvider, string table, string set, string @where)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (set == null)
                throw new ArgumentNullException("set");

            var sql = new StringBuilder("UPDATE ");
            sql.Append(dialectProvider.GetQuotedTableName(table));
            sql.Append(" SET ");
            sql.Append(set.SqlVerifyFragment());
            if (!string.IsNullOrEmpty(@where))
            {
                sql.Append(" WHERE ");
                sql.Append(@where.SqlVerifyFragment());
            }
            return sql;
        }

        [Obsolete("Use db.InsertOnly(obj, db.From<T>())")]
        public static void InsertOnly<T>(this IDbCommand dbCmd, T obj, Func<SqlExpression<T>, SqlExpression<T>> onlyFields)
        {
            dbCmd.InsertOnly(obj, onlyFields(dbCmd.GetDialectProvider().SqlExpression<T>()));
        }

        public static void InsertOnly<T>(this IDbCommand dbCmd, T obj, SqlExpression<T> onlyFields)
        {
            if (OrmLiteConfig.InsertFilter != null)
                OrmLiteConfig.InsertFilter(dbCmd, obj);

            var sql = dbCmd.GetDialectProvider().ToInsertRowStatement(dbCmd, obj, onlyFields.InsertFields);
            dbCmd.ExecuteSql(sql);
        }

        public static int Delete<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> where)
        {
            var ev = dbCmd.GetDialectProvider().SqlExpression<T>();
            ev.Where(where);
            return dbCmd.Delete(ev);
        }

        [Obsolete("Use db.Delete(db.From<T>())")]
        public static int Delete<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> where)
        {
            return dbCmd.Delete(where(dbCmd.GetDialectProvider().SqlExpression<T>()));
        }

        public static int Delete<T>(this IDbCommand dbCmd, SqlExpression<T> where)
        {
            var sql = where.ToDeleteRowStatement();
            return dbCmd.ExecuteSql(sql, where.Params);
        }

        public static int DeleteFmt<T>(this IDbCommand dbCmd, string where = null)
        {
            return dbCmd.DeleteFmt(typeof(T).GetModelDefinition().ModelName, where);
        }

        public static int DeleteFmt(this IDbCommand dbCmd, string table = null, string where = null)
        {
            var sql = DeleteFmtSql(dbCmd.GetDialectProvider(), table, @where);
            return dbCmd.ExecuteSql(sql.ToString());
        }

        internal static StringBuilder DeleteFmtSql(IOrmLiteDialectProvider dialectProvider, string table, string @where)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (@where == null)
                throw new ArgumentNullException("where");

            var sql = new StringBuilder();
            sql.AppendFormat("DELETE FROM {0} WHERE {1}",
                             dialectProvider.GetQuotedTableName(table),
                             @where.SqlVerifyFragment());
            return sql;
        }
    }
}

