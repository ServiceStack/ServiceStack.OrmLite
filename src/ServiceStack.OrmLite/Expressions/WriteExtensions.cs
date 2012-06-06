using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ServiceStack.Common.Extensions;
using ServiceStack.Common.Utils;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite
{
	public static class WriteExtensions
	{
		
		public static int Update<T>(this IDbCommand dbCmd, T obj, SqlExpressionVisitor<T> expression )
			where T : new()
		{
            IList<string> uf;
            if(expression.UpdateFields.Count==0)
                uf= expression.GetAllFields();
            else
                uf= expression.UpdateFields;

			string sql = OrmLiteConfig.DialectProvider.ToUpdateRowStatement( obj, uf);

            if(!expression.WhereExpression.IsNullOrEmpty()) sql= sql+expression.WhereExpression; 
			return dbCmd.ExecuteSql( sql); 	
		}
		
		
		public static void Insert<T>(this IDbCommand dbCmd, T obj , SqlExpressionVisitor<T> expression )
		where T : new()
		{
			string sql= OrmLiteConfig.DialectProvider.ToInsertRowStatement(obj, 
			                                                               expression.InsertFields,
			                                                                dbCmd);
			dbCmd.ExecuteSql(sql);
		}
		
		
		public static int Delete<T>(this IDbCommand dbCmd,  SqlExpressionVisitor<T> expression )
			where T : new()
		{
			string sql = expression.ToDeleteRowStatement();
			return dbCmd.ExecuteSql( sql); 	
		}
		

	}
}

