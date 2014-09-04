using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace ServiceStack.OrmLite
{
    public static class ReadExtensions
    {
        public static SqlExpression<T> SqlExpression<T>()
        {
            return OrmLiteConfig.DialectProvider.SqlExpression<T>();
        }

        internal static List<T> Select<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            string sql = expression(expr).SelectInto<T>();

            return dbCmd.ExprConvertToList<T>(sql);
        }

        internal static List<Into> Select<Into, From>(this IDbCommand dbCmd, Func<SqlExpression<From>, SqlExpression<From>> expression)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<From>();
            string sql = expression(expr).SelectInto<Into>();

            return dbCmd.ExprConvertToList<Into>(sql);
        }

        internal static List<Into> Select<Into, From>(this IDbCommand dbCmd, SqlExpression<From> expression)
        {
            string sql = expression.SelectInto<Into>();
            return dbCmd.ExprConvertToList<Into>(sql);
        }

        internal static List<T> Select<T>(this IDbCommand dbCmd, SqlExpression<T> expression)
        {
            string sql = expression.SelectInto<T>();

            return dbCmd.ExprConvertToList<T>(sql);
        }

        internal static List<T> Select<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            string sql = expr.Where(predicate).SelectInto<T>();

            return dbCmd.ExprConvertToList<T>(sql);
        }

        internal static T Single<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            return dbCmd.Single(expression(expr));
        }

        internal static T Single<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var ev = OrmLiteConfig.DialectProvider.SqlExpression<T>();

            return Single(dbCmd, ev.Where(predicate));
        }

        internal static T Single<T>(this IDbCommand dbCmd, SqlExpression<T> expression)
        {
            string sql = expression.Limit(1).SelectInto<T>();

            return dbCmd.ExprConvertTo<T>(sql);
        }

        public static TKey Scalar<T, TKey>(this IDbCommand dbCmd, Expression<Func<T, TKey>> field)
        {
            var ev = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            ev.Select(field);
            var sql = ev.SelectInto<T>();
            return dbCmd.Scalar<TKey>(sql);
        }

        internal static TKey Scalar<T, TKey>(this IDbCommand dbCmd,
            Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate)
        {
            var ev = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            ev.Select(field).Where(predicate);
            string sql = ev.SelectInto<T>();
            return dbCmd.Scalar<TKey>(sql);
        }

        internal static long Count<T>(this IDbCommand dbCmd)
        {
            var expression = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            var sql = expression.ToCountStatement();
            return GetCount(dbCmd, sql);
        }

        internal static long Count<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            var sql = expression(expr).ToCountStatement();
            return GetCount(dbCmd, sql);
        }

        internal static long Count<T>(this IDbCommand dbCmd, SqlExpression<T> expression)
        {
            var sql = expression.ToCountStatement();
            return GetCount(dbCmd, sql);
        }

        internal static long Count<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var ev = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            ev.Where(predicate);
            var sql = ev.ToCountStatement();
            return GetCount(dbCmd, sql);
        }

        internal static long GetCount(this IDbCommand dbCmd, string sql)
        {
            return dbCmd.Column<long>(sql).Sum();
        }

        internal static List<T> LoadSelect<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            expr = expression(expr);
            return dbCmd.LoadListWithReferences<T, T>(expr);
        }

        internal static List<T> LoadSelect<T>(this IDbCommand dbCmd, SqlExpression<T> expression = null)
        {
            return dbCmd.LoadListWithReferences<T, T>(expression);
        }

        internal static List<Into> LoadSelect<Into, From>(this IDbCommand dbCmd, SqlExpression<From> expression)
        {
            return dbCmd.LoadListWithReferences<Into, From>(expression);
        }

        internal static List<T> LoadSelect<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>().Where(predicate);
            return dbCmd.LoadListWithReferences<T, T>(expr);
        }

        internal static T ExprConvertTo<T>(this IDataReader dataReader)
        {
            var fieldDefs = ModelDefinition<T>.Definition.AllFieldDefinitionsArray;
            var dialectProvider = OrmLiteConfig.DialectProvider;

            using (dataReader)
            {
                if (dataReader.Read())
                {
                    var row = OrmLiteUtilExtensions.CreateInstance<T>();

                    var namingStrategy = OrmLiteConfig.DialectProvider.NamingStrategy;

                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        var fieldDef = fieldDefs.FirstOrDefault(x =>
                            namingStrategy.GetColumnName(x.FieldName).ToUpper() == dataReader.GetName(i).ToUpper());

                        dialectProvider.SetDbValue(fieldDef, dataReader, i, row);
                    }

                    return row;
                }
                return default(T);
            }
        }

        internal static List<T> ExprConvertToList<T>(this IDataReader dataReader)
        {
            var fieldDefs = ModelDefinition<T>.Definition.AllFieldDefinitionsArray;
            var fieldDefCache = new Dictionary<int, FieldDefinition>();
            var dialectProvider = OrmLiteConfig.DialectProvider;

            var to = new List<T>();
            using (dataReader)
            {
               var indexCache = dataReader.GetIndexFieldsCache(ModelDefinition<T>.Definition);
                while (dataReader.Read())
                {
                    var row = OrmLiteUtilExtensions.CreateInstance<T>();
                    row.PopulateWithSqlReader(dataReader, fieldDefs, indexCache);
                    to.Add(row);
                }
            }
            return to;
        }

    }
}

