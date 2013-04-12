using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
namespace ServiceStack.OrmLite
{
	public static class Sql
	{
		public static bool In<T>(T value, params object[] list)
		{
			if(value == null)
				return false;

			foreach (var obj in list)
			{
				if (obj == null) continue;
				if (obj.ToString() == value.ToString()) return true;
			}

			return false;
		}

		public static string Desc<T>(T value) {
			return  value==null? "": value.ToString() + " DESC";
		}
		
		public static string As<T>( T value, object asValue) {
			return  value==null? "": string.Format("{0} AS {1}", value.ToString(), asValue);
		}
		
		public static T Sum<T>( T value)  {
			return value;
		}
		
		public static T Count<T>( T value)  {
			return value;
		}
		
		public static T Min<T>( T value)  {
			return value;
		}
		
		public static T Max<T>( T value)  {
			return value;
		}
		
		public static T Avg<T>( T value)  {
			return value;
		}
	}
		
}

