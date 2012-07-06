using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ServiceStack.Common.Extensions;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public static class WriteExtensions
    {
        //TODO: Verify this is the behaviour we want, then document it
        public static int Update<T>(this IDbCommand dbCmd, T obj, SqlExpressionVisitor<T> whereFilterFn)
        {
            var fieldsToUpdate = whereFilterFn.UpdateFields.Count == 0
                ? whereFilterFn.GetAllFields()
                : whereFilterFn.UpdateFields;

            var sql = OrmLiteConfig.DialectProvider.ToUpdateRowStatement(obj, fieldsToUpdate);

            if (!whereFilterFn.WhereExpression.IsNullOrEmpty()) sql += whereFilterFn.WhereExpression;
            return dbCmd.ExecuteSql(sql);
        }

        /// <summary>
        /// Updates all non-default values set on item matching the where condition (if any)
        /// </summary>
        public static int Update<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> whereFilter = null)
        {
            var dialectProvider = OrmLiteConfig.DialectProvider;
            var ev = dialectProvider.ExpressionVisitor<T>();
            ev.Where(whereFilter);
            var sql = ev.ToUpdateStatement(item);
            return dbCmd.ExecuteSql(sql);
        }

        /// <summary>
        /// Updates all matching fields populated on anonymousType that matches where condition (if any)
        /// </summary>
        public static int Update<T>(this IDbCommand dbCmd, object updateFields, Expression<Func<T, bool>> whereFilterFn = null)
        {
            var dialectProvider = OrmLiteConfig.DialectProvider;
            var ev = dialectProvider.ExpressionVisitor<T>();
            var whereSql = ev.Where(whereFilterFn).WhereExpression;
            var sql = new StringBuilder();
            var modelDef = typeof(T).GetModelDefinition();
            var fields = modelDef.FieldDefinitionsArray;

            foreach (var setField in updateFields.GetType().GetPublicProperties())
            {
                var fieldDef = fields.FirstOrDefault(x => string.Equals(x.Name, setField.Name, StringComparison.InvariantCultureIgnoreCase));
                if (fieldDef == null) continue;

                if (sql.Length > 0) sql.Append(",");
                sql.AppendFormat("{0} = {1}", 
                    dialectProvider.GetQuotedColumnName(fieldDef.FieldName),
                    dialectProvider.GetQuotedValue(setField.GetPropertyGetterFn()(updateFields), fieldDef.FieldType));
            }

            var updateSql = string.Format("UPDATE {0} SET {1} {2}",
                dialectProvider.GetQuotedTableName(modelDef), sql, whereSql);

            return dbCmd.ExecuteSql(updateSql);
        }

        public static int Update<T, TKey>(this IDbCommand dbCmd, T obj,
            Expression<Func<T, TKey>> fields,
            Expression<Func<T, bool>> predicate)
        {
            var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
            ev.Update(fields);
            ev.Where(predicate);
            return dbCmd.Update(obj, ev);
        }

        /// <summary>
        /// Flexible Update method to succinctly execute a free-text update statement using optional params.
        /// Use "Field = {0}".SqlFormat("valueToEscape") extension method to correctly escape params.
        /// </summary>
        public static int Update<T>(this IDbCommand dbCmd, string set = null, string where = null)
        {
            return dbCmd.Update(typeof(T).GetModelDefinition().ModelName, set, where);
        }

        /// <summary>
        /// Flexible Update method to succinctly execute a free-text update statement using optional params.
        /// Use "Field = {0}".SqlFormat("valueToEscape") extension method to correctly escape params.
        /// </summary>
        public static int Update(this IDbCommand dbCmd, string table = null, string set = null, string where = null)
        {
            if (set == null)
                throw new ArgumentNullException("set");

            var sql = new StringBuilder("UPDATE ");
            sql.Append(table);
            sql.Append(" SET ");
            sql.Append(set);
            if (!string.IsNullOrEmpty(where))
            {
                sql.Append(" WHERE ");
                sql.Append(where);
            }

            return dbCmd.ExecuteSql(sql.ToString());
        }

        public static void Insert<T>(this IDbCommand dbCmd, T obj, SqlExpressionVisitor<T> expression)
        {
            var sql = OrmLiteConfig.DialectProvider.ToInsertRowStatement(obj, expression.InsertFields, dbCmd);
            dbCmd.ExecuteSql(sql);
        }

        public static int Delete<T>(this IDbCommand dbCmd, SqlExpressionVisitor<T> expression)
        {
            string sql = expression.ToDeleteRowStatement();
            return dbCmd.ExecuteSql(sql);
        }

        public static int Delete<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
            ev.Where(predicate);
            return dbCmd.Delete(ev);
        }

        public static int Delete<T>(this IDbCommand dbCmd, string where = null)
        {
            return dbCmd.Delete(typeof(T).GetModelDefinition().ModelName, where);
        }

        /// <summary>
        /// Flexible Delete method to succinctly execute a free-text update statement using optional params.
        /// Use "Field = {0}".SqlFormat("valueToEscape") extension method to correctly escape params.
        /// </summary>
        public static int Delete(this IDbCommand dbCmd, string table = null, string where = null)
        {
            if (where == null)
                throw new ArgumentNullException("where");

            var sql = new StringBuilder("DELETE FROM ");
            sql.Append(table);
            sql.Append(" WHERE ");
            sql.Append(where);

            return dbCmd.ExecuteSql(sql.ToString());
        }

    }
}

