using System;
using ServiceStack.OrmLite;
namespace ServiceStack.OrmLite.Firebird
{
	public class FirebirdSqlExpressionVisitor<T>:SqlExpressionVisitor<T>
	{
		public FirebirdSqlExpressionVisitor ():base()
		{
		}
	}
}

