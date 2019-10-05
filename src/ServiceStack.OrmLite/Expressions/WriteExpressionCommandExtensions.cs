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
        public static int UpdateOnly<T>(this IDbCommand dbCmd,
            T model,
            SqlExpression<T> onlyFields,
            Action<IDbCommand> commandFilter = null)
        {
            UpdateOnlySql(dbCmd, model, onlyFields);
            commandFilter?.Invoke(dbCmd);
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
            Expression<Func<T, bool>> where = null,
            Action<IDbCommand> commandFilter = null)
        {
            if (onlyFields == null)
                throw new ArgumentNullException(nameof(onlyFields));

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Update(onlyFields);
            q.Where(where);
            return dbCmd.UpdateOnly(obj, q, commandFilter);
        }

        internal static int UpdateOnly<T>(this IDbCommand dbCmd, T obj,
            string[] onlyFields = null,
            Expression<Func<T, bool>> where = null,
            Action<IDbCommand> commandFilter = null)
        {
            if (onlyFields == null)
                throw new ArgumentNullException(nameof(onlyFields));

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Update(onlyFields);
            q.Where(where);
            return dbCmd.UpdateOnly(obj, q, commandFilter);
        }

        internal static int UpdateOnly<T>(this IDbCommand dbCmd,
            Expression<Func<T>> updateFields,
            SqlExpression<T> q,
            Action<IDbCommand> commandFilter = null)
        {
            var cmd = dbCmd.InitUpdateOnly(updateFields, q);
            commandFilter?.Invoke(cmd);
            return cmd.ExecNonQuery();
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

        internal static int UpdateOnly<T>(this IDbCommand dbCmd,
            Expression<Func<T>> updateFields,
            string whereExpression,
            IEnumerable<IDbDataParameter> dbParams,
            Action<IDbCommand> commandFilter = null)
        {
            var cmd = dbCmd.InitUpdateOnly(updateFields, whereExpression, dbParams);
            commandFilter?.Invoke(cmd);
            return cmd.ExecNonQuery();
        }

        internal static IDbCommand InitUpdateOnly<T>(this IDbCommand dbCmd, Expression<Func<T>> updateFields, string whereExpression, IEnumerable<IDbDataParameter> sqlParams)
        {
            if (updateFields == null)
                throw new ArgumentNullException(nameof(updateFields));

            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, updateFields.EvalFactoryFn());

            dbCmd.SetParameters(sqlParams);

            var updateFieldValues = updateFields.AssignedValues();
            dbCmd.GetDialectProvider().PrepareUpdateRowStatement<T>(dbCmd, updateFieldValues, whereExpression);

            return dbCmd;
        }
        
        public static int UpdateAdd<T>(this IDbCommand dbCmd,
            Expression<Func<T>> updateFields,
            SqlExpression<T> q,
            Action<IDbCommand> commandFilter)
        {
            var cmd = dbCmd.InitUpdateAdd(updateFields, q);
            commandFilter?.Invoke(cmd);
            return cmd.ExecNonQuery();
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

        public static int UpdateOnly<T>(this IDbCommand dbCmd,
            Dictionary<string, object> updateFields,
            Expression<Func<T, bool>> where,
            Action<IDbCommand> commandFilter = null)
        {
            if (updateFields == null)
                throw new ArgumentNullException(nameof(updateFields));

            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, updateFields.FromObjectDictionary<T>());

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(where);
            q.PrepareUpdateStatement(dbCmd, updateFields);
            commandFilter?.Invoke(dbCmd);

            return dbCmd.ExecNonQuery();
        }

        public static int UpdateNonDefaults<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> where)
        {
            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, item);

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(@where);
            q.PrepareUpdateStatement(dbCmd, item, excludeDefaults: true);
            return dbCmd.ExecNonQuery();
        }

        public static int Update<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> expression, Action<IDbCommand> commandFilter = null)
        {
            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, item);

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            q.Where(expression);
            q.PrepareUpdateStatement(dbCmd, item);
            commandFilter?.Invoke(dbCmd);
            return dbCmd.ExecNonQuery();
        }

        public static int Update<T>(this IDbCommand dbCmd, object updateOnly, Expression<Func<T, bool>> where = null, Action<IDbCommand> commandFilter = null)
        {
            OrmLiteConfig.UpdateFilter?.Invoke(dbCmd, updateOnly);

            var q = dbCmd.GetDialectProvider().SqlExpression<T>();
            var whereSql = q.Where(where).WhereExpression;
            q.CopyParamsTo(dbCmd);
            dbCmd.PrepareUpdateAnonSql<T>(dbCmd.GetDialectProvider(), updateOnly, whereSql);

            commandFilter?.Invoke(dbCmd);
            return dbCmd.ExecNonQuery();
        }

        internal static void PrepareUpdateAnonSql<T>(this IDbCommand dbCmd, IOrmLiteDialectProvider dialectProvider, object updateOnly, string whereSql)
        {
            var sql = StringBuilderCache.Allocate();
            var modelDef = typeof(T).GetModelDefinition();
            var fields = modelDef.FieldDefinitionsArray;

            foreach (var setField in updateOnly.GetType().GetPublicProperties())
            {
                var fieldDef = fields.FirstOrDefault(x => string.Equals(x.Name, setField.Name, StringComparison.OrdinalIgnoreCase));
                if (fieldDef == null || fieldDef.ShouldSkipUpdate()) continue;

                var value = setField.CreateGetter()(updateOnly);
                if (string.IsNullOrEmpty(whereSql) && (fieldDef.IsPrimaryKey || fieldDef.AutoIncrement))
                {
                    whereSql = $"WHERE {dialectProvider.GetQuotedColumnName(fieldDef.FieldName)} = {dialectProvider.AddQueryParam(dbCmd, value, fieldDef).ParameterName}";
                    continue;
                }

                if (sql.Length > 0)
                    sql.Append(", ");

                sql
                    .Append(dialectProvider.GetQuotedColumnName(fieldDef.FieldName))
                    .Append("=")
                    .Append(dialectProvider.AddUpdateParam(dbCmd, value, fieldDef).ParameterName);
            }

            dbCmd.CommandText = $"UPDATE {dialectProvider.GetQuotedTableName(modelDef)} " +
                                $"SET {StringBuilderCache.ReturnAndFree(sql)} {whereSql}";
        }

        public static long InsertOnly<T>(this IDbCommand dbCmd, T obj, string[] onlyFields, bool selectIdentity)
        {
            OrmLiteConfig.InsertFilter?.Invoke(dbCmd, obj);

            var dialectProvider = dbCmd.GetDialectProvider();
            var sql = dialectProvider.ToInsertRowStatement(dbCmd, obj, onlyFields);

            if (selectIdentity)
                return dbCmd.ExecLongScalar(sql + dialectProvider.GetLastInsertIdSqlSuffix<T>());

            return dbCmd.ExecuteSql(sql);
        }

        public static long InsertOnly<T>(this IDbCommand dbCmd, Expression<Func<T>> insertFields, bool selectIdentity)
        {
            dbCmd.InitInsertOnly(insertFields);

            if (selectIdentity)
                return dbCmd.ExecLongScalar(dbCmd.CommandText + dbCmd.GetDialectProvider().GetLastInsertIdSqlSuffix<T>());

            return dbCmd.ExecuteNonQuery();
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

