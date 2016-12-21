using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    internal static class WriteExpressionCommandExtensions
    {
        public static int UpdateOnly<T>(this IDbCommand dbCmd, T model, SqlExpression<T> onlyFields)
        {
            UpdateOnlySql(dbCmd, model, onlyFields);
            return dbCmd.ExecNonQuery();
        }

        internal static void UpdateOnlySql<T>(this IDbCommand dbCmd, T model, SqlExpression<T> onlyFields)
        {
            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, model);

            var fieldsToUpdate = onlyFields.UpdateFields.Count == 0
                ? onlyFields.GetAllFields()
                : onlyFields.UpdateFields;

            onlyFields.CopyParamsTo(dbCmd);

            dbCmd.GetDialectProvider().PrepareUpdateRowStatement(dbCmd, model, fieldsToUpdate);

            if (!onlyFields.WhereExpression.IsNullOrEmpty())
                dbCmd.CommandText += " " + onlyFields.WhereExpression;
        }

        internal static int UpdateOnly<T>(this IDbCommand dbCmd, T obj,
            Expression<Func<T, object>> onlyFields = null,
            Expression<Func<T, bool>> where = null)
        {
            if (onlyFields == null)
                throw new ArgumentNullException(nameof(onlyFields));

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Update(onlyFields);
            q.Where(where);
            return dbCmd.UpdateOnly(obj, q);
        }

        internal static int UpdateOnly<T>(this IDbCommand dbCmd, T obj,
            string[] onlyFields = null,
            Expression<Func<T, bool>> where = null)
        {
            if (onlyFields == null)
                throw new ArgumentNullException(nameof(onlyFields));

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Update(onlyFields);
            q.Where(where);
            return dbCmd.UpdateOnly(obj, q);
        }

        internal static int UpdateOnly<T>(this IDbCommand dbCmd,
            Expression<Func<T>> updateFields,
            SqlExpression<T> q)
        {
            return dbCmd.InitUpdateOnly(updateFields, q).ExecNonQuery();
        }

        internal static IDbCommand InitUpdateOnly<T>(this IDbCommand dbCmd, Expression<Func<T>> updateFields, SqlExpression<T> q)
        {
            if (updateFields == null)
                throw new ArgumentNullException(nameof(updateFields));

            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, updateFields.EvalFactoryFn());

            q.CopyParamsTo(dbCmd);

            var updateFieldValues = updateFields.AssignedValues();
            dbCmd.GetDialectProvider().PrepareUpdateRowStatement<T>(dbCmd, updateFieldValues, q.WhereExpression);

            return dbCmd;
        }

        public static int UpdateAdd<T>(this IDbCommand dbCmd,
            Expression<Func<T>> updateFields,
            SqlExpression<T> q)
        {
            return dbCmd.InitUpdateAdd(updateFields, q).ExecNonQuery();
        }

        internal static IDbCommand InitUpdateAdd<T>(this IDbCommand dbCmd, Expression<Func<T>> updateFields, SqlExpression<T> q)
        {
            if (updateFields == null)
                throw new ArgumentNullException(nameof(updateFields));

            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, updateFields.EvalFactoryFn());

            q.CopyParamsTo(dbCmd);

            var updateFieldValues = updateFields.AssignedValues();
            dbCmd.GetDialectProvider().PrepareUpdateRowAddStatement<T>(dbCmd, updateFieldValues, q.WhereExpression);

            return dbCmd;
        }

        public static int UpdateNonDefaults<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> obj)
        {
            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, item);

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(obj);
            q.PrepareUpdateStatement(dbCmd, item, excludeDefaults: true);
            return dbCmd.ExecNonQuery();
        }

        public static int Update<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> expression)
        {
            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, item);

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
            var sql = StringBuilderCache.Allocate();
            var modelDef = typeof(T).GetModelDefinition();
            var fields = modelDef.FieldDefinitionsArray;

            var updateFields = new List<FieldDefinition>();
            foreach (var setField in updateOnly.GetType().GetPublicProperties())
            {
                var fieldDef = fields.FirstOrDefault(x => string.Equals(x.Name, setField.Name, StringComparison.OrdinalIgnoreCase));
                if (fieldDef == null) continue;
                updateFields.Add(fieldDef);
                if (fieldDef.ShouldSkipUpdate()) continue;

                if (sql.Length > 0)
                    sql.Append(", ");

                var value = setField.GetPropertyGetterFn()(updateOnly);
                sql
                    .Append(dialectProvider.GetQuotedColumnName(fieldDef.FieldName))
                    .Append("=")
                    .Append(dialectProvider.AddParam(dbCmd, value, fieldDef).ParameterName);
            }

            dialectProvider.AddDefaultUpdateFields(dbCmd, modelDef, updateFields, sql, "");

            dbCmd.CommandText = $"UPDATE {dialectProvider.GetQuotedTableName(modelDef)} " +
                                $"SET {StringBuilderCache.ReturnAndFree(sql)} {whereSql}";
        }

        public static void InsertOnly<T>(this IDbCommand dbCmd, T obj, string[] onlyFields)
        {
            OrmLiteConfig.InsertFilter?.Invoke(dbCmd, obj);

            var sql = dbCmd.GetDialectProvider().ToInsertRowStatement(dbCmd, obj, onlyFields);
            dbCmd.ExecuteSql(sql);
        }

        public static int InsertOnly<T>(this IDbCommand dbCmd, Expression<Func<T>> insertFields)
        {
            return dbCmd.InitInsertOnly(insertFields).ExecNonQuery();
        }

        internal static IDbCommand InitInsertOnly<T>(this IDbCommand dbCmd, Expression<Func<T>> insertFields)
        {
            if (insertFields == null)
                throw new ArgumentNullException(nameof(insertFields));

            OrmLiteConfig.InsertFilter?.Invoke(dbCmd, insertFields.EvalFactoryFn());

            var fieldValuesMap = insertFields.AssignedValues();
            dbCmd.GetDialectProvider().PrepareInsertRowStatement<T>(dbCmd, fieldValuesMap);
            return dbCmd;
        }

        public static int Delete<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> where)
        {
            var ev = dbCmd.GetDialectProvider().SqlExpression<T>();
            ev.Where(where);
            return dbCmd.Delete(ev);
        }

        public static int Delete<T>(this IDbCommand dbCmd, SqlExpression<T> where)
        {
            var sql = where.ToDeleteRowStatement();
            return dbCmd.ExecuteSql(sql, where.Params);
        }
    }
}

