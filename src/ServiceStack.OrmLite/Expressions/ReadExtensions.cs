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
			using (var reader = dbCmd.ExecReader(sql))
			{
				return ConvertToList<T>(reader);
			}
		}

	    internal static List<T> Select<T>(this IDbCommand dbCmd, SqlExpression<T> expression)
        {
            string sql = expression.ToSelectStatement();
            using (var reader = dbCmd.ExecReader(sql))
            {
                return ConvertToList<T>(reader);
            }
        }

	    internal static List<T> Select<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
        {
            var expr = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            string sql = expr.Where(predicate).ToSelectStatement();
            using (var reader = dbCmd.ExecReader(sql))
            {
                return ConvertToList<T>(reader);
            }
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
			using (var dbReader = dbCmd.ExecReader(sql))
			{
				return ConvertTo<T>(dbReader);
			}
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
		
		private static T ConvertTo<T>(IDataReader dataReader)
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
		
		private static List<T> ConvertToList<T>(IDataReader dataReader)
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

        /*
        private static T PopulateWithSqlReader<T>( T objWithProperties, IDataReader dataReader, FieldDefinition[] fieldDefs)
        {
            foreach (var fieldDef in fieldDefs)
            {
                try{
                    // NOTE: this is a nasty ineffeciency here when we're calling this for multiple rows!
                    // we should only call GetOrdinal once per column per result set
                    // and one could only wish for a -1 return instead of an IndexOutOfRangeException...
                    // how to get -1 ? 
                    //If the index of the named field is not found, an IndexOutOfRangeException is thrown.
                    //http://msdn.microsoft.com/en-us/library/system.data.idatarecord.getordinal(v=vs.100).aspx
					
                    var index = dataReader.GetColumnIndex(fieldDef.FieldName);
                    var value = dataReader.GetValue(index);
                    fieldDef.SetValue(objWithProperties, value);
                }
                catch(IndexOutOfRangeException){
                    // just ignore not retrived fields
                }
            }
		
			
            return objWithProperties;
        }
        */
        // First FirstOrDefault  // Use LIMIT to retrive only one row ! someone did it

	}
}

