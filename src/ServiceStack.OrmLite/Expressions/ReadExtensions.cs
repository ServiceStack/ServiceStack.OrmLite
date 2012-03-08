using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
	public static class ReadExtensions
	{
		public static List<T> Select<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
			where T : new()
		{
			var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
			string sql = ev.Where(predicate).ToSelectStatement();
			using (var reader = dbCmd.ExecReader(sql))
			{
				return ConvertToList<T>(reader);
			}
		}

		public static List<T> Select<T>(this IDbCommand dbCmd, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression)
			where T : new()
		{
			var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
			string sql = expression(ev).ToSelectStatement();
			using (var reader = dbCmd.ExecReader(sql))
			{
				return ConvertToList<T>(reader);
			}
		}
		
		public static List<T> Select<T>(this IDbCommand dbCmd, SqlExpressionVisitor<T> expression)
			where T : new()
		{
			string sql = expression.ToSelectStatement();
			using (var reader = dbCmd.ExecReader(sql))
			{
				return ConvertToList<T>(reader);
			}
		}
		
		public static T First<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
			where T : new()
		{
			var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
			
			return First(dbCmd, ev.Where(predicate).Limit(1));
		}
		
		public static T First<T>(this IDbCommand dbCmd,  SqlExpressionVisitor<T> expression)
			where T : new()
		{
			var result = FirstOrDefault(dbCmd, expression);
			if (Equals(result, default(T)))
			{
				throw new ArgumentNullException(string.Format(
					"{0}: '{1}' does not exist", typeof(T).Name, expression.WhereExpression));
			}
			return result;
		}
		
		public static T FirstOrDefault<T>(this IDbCommand dbCmd, Expression<Func<T, bool>> predicate)
			where T : new()
		{
			var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
			
			return FirstOrDefault(dbCmd, ev.Where(predicate).Limit(1));
		}

		public static T FirstOrDefault<T>(this IDbCommand dbCmd, SqlExpressionVisitor<T> expression)
			where T : new()
		{
			string sql= expression.ToSelectStatement();
			using (var dbReader = dbCmd.ExecReader(sql))
			{
				return ConvertTo<T>(dbReader);
			}
		}

		/// <summary>
		/// e.g. dbCmd.GetScalar&lt;MyClass, DateTime&gt;(myClass => Sql.Max(myClass.Timestamp));
		/// </summary>
		public static TKey GetScalar<T, TKey>(this IDbCommand dbCmd, Expression<Func<T, TKey>> field)
			where TKey : new()
		{
			var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
			ev.Select(field);
			var sql = ev.ToSelectStatement();
			return dbCmd.GetScalar<TKey>(sql);
		}
		
		private static T ConvertTo<T>(IDataReader dataReader)
            where T : new()
        {
			var fieldDefs = ModelDefinition<T>.Definition.FieldDefinitionsArray;

			using (dataReader)
			{
				if (dataReader.Read())
				{
					var row = new T();
					PopulateWithSqlReader(row, dataReader, fieldDefs);
					return row;
				}
				return default(T);
			}
		}
		
		
		private static List<T> ConvertToList<T>(IDataReader dataReader)
			where T : new()
		{
            var fieldDefs = ModelDefinition<T>.Definition.FieldDefinitionsArray;

			var to = new List<T>();
			using (dataReader)
			{
				while (dataReader.Read())
				{
					var row = new T();
					PopulateWithSqlReader(row, dataReader, fieldDefs);
					to.Add(row);
				}
			}
			return to;
		}
		
		private static T PopulateWithSqlReader<T>( T objWithProperties, IDataReader dataReader, FieldDefinition[] fieldDefs)
        {
			foreach (var fieldDef in fieldDefs)
			{
				try{
					// NOTE: this is a nasty ineffeciency here when we're calling this for multiple rows!
					// we should only call GetOrdinal once per column per result set
					// and one could only wish for a -1 return instead of an IndexOutOfRangeException...
					var index = dataReader.GetOrdinal(fieldDef.FieldName);
					var value = dataReader.GetValue(index);
					fieldDef.SetValue(objWithProperties, value);
				}
				catch{
					// just ignore not retrived fields
				}
			}
		
			
			return objWithProperties;
        }
		
		// First FirstOrDefault  // Use LIMIT to retrive only one row !
	}
}

