using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

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
			string sql = expression(expr).ToSelectStatement();

            return dbCmd.ExprConvertToList<T>(sql);
		}

	    internal static List<T> Select<T>(this IDbCommand dbCmd, SqlExpression<T> expression)
        {
            string sql = expression.ToSelectStatement();

            return dbCmd.ExprConvertToList<T>(sql);
        }

	    internal static List<T> Select<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            string sql = expr.Where(predicate).ToSelectStatement();

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
            string sql = expression.Limit(1).ToSelectStatement();

            return dbCmd.ExprConvertTo<T>(sql);
		}

		public static TKey Scalar<T, TKey>(this IDbCommand dbCmd, Expression<Func<T, TKey>> field)
		{
			var ev = OrmLiteConfig.DialectProvider.SqlExpression<T>();
			ev.Select(field);
			var sql = ev.ToSelectStatement();
			return dbCmd.Scalar<TKey>(sql);
		}

	    internal static TKey Scalar<T,TKey>(this IDbCommand dbCmd,
            Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate)
		{
			var ev = OrmLiteConfig.DialectProvider.SqlExpression<T>();
			ev.Select(field).Where(predicate);
			string sql = ev.ToSelectStatement();
			return dbCmd.Scalar<TKey>(sql);
		}

	    internal static long Count<T>(this IDbCommand dbCmd)
        {
            SqlExpression<T> expression = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            string sql = expression.ToCountStatement();
            return dbCmd.Scalar<long>(sql);
        }

        internal static long Count<T>(this IDbCommand dbCmd, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            string sql = expression(expr).ToCountStatement();
            return dbCmd.Scalar<long>(sql);
        }

	    internal static long Count<T>(this IDbCommand dbCmd, SqlExpression<T> expression)
        {
            string sql = expression.ToCountStatement();
            return dbCmd.Scalar<long>(sql);
        }

	    internal static long Count<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var ev = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            ev.Where(predicate);
            string sql = ev.ToCountStatement();
            return dbCmd.Scalar<long>(sql);
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

					for (int i = 0; i<dataReader.FieldCount; i++)
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
				while (dataReader.Read())
				{
                    var row = OrmLiteUtilExtensions.CreateInstance<T>();

				    var namingStrategy = dialectProvider.NamingStrategy;

					for (int i = 0; i<dataReader.FieldCount; i++)
					{
						FieldDefinition fieldDef;
						if (!fieldDefCache.TryGetValue(i, out fieldDef))
						{
						    fieldDef = fieldDefs.FirstOrDefault(x =>
						        namingStrategy.GetColumnName(x.FieldName).ToUpper() == dataReader.GetName(i).ToUpper());
							
                            fieldDefCache[i] = fieldDef;
						}

                        dialectProvider.SetDbValue(fieldDef, dataReader, i, row);
					}
					to.Add(row);
				}
			}
			return to;
		}
        
	}
}

