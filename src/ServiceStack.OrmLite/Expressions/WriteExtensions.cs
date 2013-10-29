using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ServiceStack.Common;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public static class WriteExtensions
    {
        /// <summary>
        /// Use an expression visitor to select which fields to update and construct the where expression, E.g: 
        /// 
        ///   dbCmd.UpdateOnly(new Person { FirstName = "JJ" }, ev => ev.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
        ///   UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("FirstName" = 'Jimi')
        /// 
        ///   What's not in the update expression doesn't get updated. No where expression updates all rows. E.g:
        /// 
        ///   dbCmd.UpdateOnly(new Person { FirstName = "JJ", LastName = "Hendo" }, ev => ev.Update(p => p.FirstName));
        ///   UPDATE "Person" SET "FirstName" = 'JJ'
        /// </summary>
        public static int UpdateOnly<T>(this IDbCommand dbCmd, T model, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields)
        {
            return dbCmd.UpdateOnly(model, onlyFields(OrmLiteConfig.DialectProvider.ExpressionVisitor<T>()));
        }

        /// <summary>
        /// Use an expression visitor to select which fields to update and construct the where expression, E.g: 
        /// 
        ///   var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor&gt;Person&lt;());
        ///   dbCmd.UpdateOnly(new Person { FirstName = "JJ" }, ev.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
        ///   UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("FirstName" = 'Jimi')
        /// 
        ///   What's not in the update expression doesn't get updated. No where expression updates all rows. E.g:
        /// 
        ///   dbCmd.UpdateOnly(new Person { FirstName = "JJ", LastName = "Hendo" }, ev.Update(p => p.FirstName));
        ///   UPDATE "Person" SET "FirstName" = 'JJ'
        /// </summary>
        public static int UpdateOnly<T>(this IDbCommand dbCmd, T model, SqlExpressionVisitor<T> onlyFields)
        {
            var fieldsToUpdate = onlyFields.UpdateFields.Count == 0
                ? onlyFields.GetAllFields()
                : onlyFields.UpdateFields;

            var sql = OrmLiteConfig.DialectProvider.ToUpdateRowStatement(model, fieldsToUpdate);

            if (!onlyFields.WhereExpression.IsNullOrEmpty()) sql += " " + onlyFields.WhereExpression;
            return dbCmd.ExecuteSql(sql);
        }

        /// <summary>
        /// Update record, updating only fields specified in updateOnly that matches the where condition (if any), E.g:
        /// 
        ///   dbCmd.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName, p => p.LastName == "Hendrix");
        ///   UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("LastName" = 'Hendrix')
        ///
        ///   dbCmd.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName);
        ///   UPDATE "Person" SET "FirstName" = 'JJ'
        /// </summary>
        public static int UpdateOnly<T, TKey>(this IDbCommand dbCmd, T obj,
            Expression<Func<T, TKey>> onlyFields = null,
            Expression<Func<T, bool>> where = null)
        {
            if (onlyFields == null)
                throw new ArgumentNullException("onlyFields");

            var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
            ev.Update(onlyFields);
            ev.Where(where);
            return dbCmd.UpdateOnly(obj, ev);
        }

        /// <summary>
        /// Updates all non-default values set on item matching the where condition (if any). E.g
        /// 
        ///   dbCmd.UpdateNonDefault(new Person { FirstName = "JJ" }, p => p.FirstName == "Jimi");
        ///   UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("FirstName" = 'Jimi')
        /// </summary>
        public static int UpdateNonDefaults<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> where)
        {
            var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
            ev.Where(where);
            var sql = ev.ToUpdateStatement(item, excludeDefaults: true);
            return dbCmd.ExecuteSql(sql);
        }

        /// <summary>
        /// Updates all values set on item matching the where condition (if any). E.g
        /// 
        ///   dbCmd.UpdateNonDefault(new Person { FirstName = "JJ" }, p => p.FirstName == "Jimi");
        ///   UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("FirstName" = 'Jimi')
        /// 
        ///   dbCmd.Update(new Person { Id = 1, FirstName = "JJ" }, p => p.LastName == "Hendrix");
        ///   UPDATE "Person" SET "Id" = 1,"FirstName" = 'JJ',"LastName" = NULL,"Age" = 0 WHERE ("LastName" = 'Hendrix')
        /// </summary>
        public static int Update<T>(this IDbCommand dbCmd, T item, Expression<Func<T, bool>> where)
        {
            var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
            ev.Where(where);
            var sql = ev.ToUpdateStatement(item);
            return dbCmd.ExecuteSql(sql);
        }

        /// <summary>
        /// Updates all matching fields populated on anonymousType that matches where condition (if any). E.g:
        /// 
        ///   dbCmd.Update&lt;Person&gt;(new { FirstName = "JJ" }, p => p.LastName == "Hendrix");
        ///   UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("LastName" = 'Hendrix')
        /// </summary>
        public static int Update<T>(this IDbCommand dbCmd, object updateOnly, Expression<Func<T, bool>> where = null)
        {
            var dialectProvider = OrmLiteConfig.DialectProvider;
            var ev = dialectProvider.ExpressionVisitor<T>();
            var whereSql = ev.Where(where).WhereExpression;
            var sql = new StringBuilder();
            var modelDef = typeof(T).GetModelDefinition();
            var fields = modelDef.FieldDefinitionsArray;

            foreach (var setField in updateOnly.GetType().GetPublicProperties())
            {
                var fieldDef = fields.FirstOrDefault(x => 
                    string.Equals(x.Name, setField.Name, StringComparison.InvariantCultureIgnoreCase));
                if (fieldDef == null) continue;

                if (sql.Length > 0) sql.Append(",");
                sql.AppendFormat("{0} = {1}", 
                    dialectProvider.GetQuotedColumnName(fieldDef.FieldName),
                    dialectProvider.GetQuotedValue(setField.GetPropertyGetterFn()(updateOnly), fieldDef.FieldType));
            }

            var updateSql = string.Format("UPDATE {0} SET {1} {2}",
                dialectProvider.GetQuotedTableName(modelDef), sql, whereSql);

            return dbCmd.ExecuteSql(updateSql);
        }

        /// <summary>
        /// Flexible Update method to succinctly execute a free-text update statement using optional params. E.g:
        /// 
        ///   dbCmd.Update&lt;Person&gt;(set:"FirstName = {0}".Params("JJ"), where:"LastName = {0}".Params("Hendrix"));
        ///   UPDATE "Person" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'
        /// </summary>
        public static int Update<T>(this IDbCommand dbCmd, string set = null, string where = null)
        {
            return dbCmd.Update(typeof(T).GetModelDefinition().ModelName, set, where);
        }

        /// <summary>
        /// Flexible Update method to succinctly execute a free-text update statement using optional params. E.g.
        /// 
        ///   dbCmd.Update(table:"Person", set: "FirstName = {0}".Params("JJ"), where: "LastName = {0}".Params("Hendrix"));
        ///   UPDATE "Person" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'
        /// </summary>
        public static int Update(this IDbCommand dbCmd, string table = null, string set = null, string where = null)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (set == null)
                throw new ArgumentNullException("set");

            var sql = new StringBuilder("UPDATE ");
            sql.Append(OrmLiteConfig.DialectProvider.GetQuotedTableName(table));
            sql.Append(" SET ");
            sql.Append(set);
            if (!string.IsNullOrEmpty(where))
            {
                sql.Append(" WHERE ");
                sql.Append(where);
            }

            return dbCmd.ExecuteSql(sql.ToString());
        }

        /// <summary>
        /// Using an Expression Visitor to only Insert the fields specified, e.g:
        /// 
        ///   dbCmd.InsertOnly(new Person { FirstName = "Amy" }, ev => ev.Insert(p => new { p.FirstName }));
        ///   INSERT INTO "Person" ("FirstName") VALUES ('Amy');
        /// </summary>
        public static void InsertOnly<T>(this IDbCommand dbCmd, T obj, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> onlyFields)
        {
            dbCmd.InsertOnly(obj, onlyFields(OrmLiteConfig.DialectProvider.ExpressionVisitor<T>()));
        }

        /// <summary>
        /// Using an Expression Visitor to only Insert the fields specified, e.g:
        /// 
        ///   var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor&gt;Person&lt;());
        ///   dbCmd.InsertOnly(new Person { FirstName = "Amy" }, ev.Insert(p => new { p.FirstName }));
        ///   INSERT INTO "Person" ("FirstName") VALUES ('Amy');
        /// </summary>
        public static void InsertOnly<T>(this IDbCommand dbCmd, T obj, SqlExpressionVisitor<T> onlyFields)
        {
            var sql = OrmLiteConfig.DialectProvider.ToInsertRowStatement(dbCmd, obj, onlyFields.InsertFields);
            dbCmd.ExecuteSql(sql);
        }

        /// <summary>
        /// Delete the rows that matches the where expression, e.g:
        /// 
        ///   dbCmd.Delete&lt;Person&gt;(p => p.Age == 27);
        ///   DELETE FROM "Person" WHERE ("Age" = 27)
        /// </summary>
        public static int Delete<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> where)
        {
            var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
            ev.Where(where);
            return dbCmd.Delete(ev);
        }

        /// <summary>
        /// Delete the rows that matches the where expression, e.g:
        /// 
        ///   dbCmd.Delete&lt;Person&gt;(ev => ev.Where(p => p.Age == 27));
        ///   DELETE FROM "Person" WHERE ("Age" = 27)
        /// </summary>
        public static int Delete<T>(this IDbCommand dbCmd, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> where)
        {
            return dbCmd.Delete(where(OrmLiteConfig.DialectProvider.ExpressionVisitor<T>()));
        }

        /// <summary>
        /// Delete the rows that matches the where expression, e.g:
        /// 
        ///   var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor&gt;Person&lt;());
        ///   dbCmd.Delete&lt;Person&gt;(ev.Where(p => p.Age == 27));
        ///   DELETE FROM "Person" WHERE ("Age" = 27)
        /// </summary>
        public static int Delete<T>(this IDbCommand dbCmd, SqlExpressionVisitor<T> where)
        {
            var sql = where.ToDeleteRowStatement();
            return dbCmd.ExecuteSql(sql);
        }

        /// <summary>
        /// Flexible Delete method to succinctly execute a delete statement using free-text where expression. E.g.
        /// 
        ///   dbCmd.Delete&lt;Person&gt;(where:"Age = {0}".Params(27));
        ///   DELETE FROM "Person" WHERE Age = 27
        /// </summary>
        public static int Delete<T>(this IDbCommand dbCmd, string where = null)
        {
            return dbCmd.Delete(typeof(T).GetModelDefinition().ModelName, where);
        }

        /// <summary>
        /// Flexible Delete method to succinctly execute a delete statement using free-text where expression. E.g.
        /// 
        ///   dbCmd.Delete(table:"Person", where: "Age = {0}".Params(27));
        ///   DELETE FROM "Person" WHERE Age = 27
        /// </summary>
        public static int Delete(this IDbCommand dbCmd, string table = null, string where = null)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (where == null)
                throw new ArgumentNullException("where");

            var sql = new StringBuilder();
            sql.AppendFormat("DELETE FROM {0} WHERE {1}",  OrmLiteConfig.DialectProvider.GetQuotedTableName(table), where);

            return dbCmd.ExecuteSql(sql.ToString());
        }

    }
}

