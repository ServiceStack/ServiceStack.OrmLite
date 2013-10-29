using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace ServiceStack.OrmLite
{
	public static class Sql
	{
        public static bool In<T, TItem>(T value, params TItem[] list)
        {
            return value != null && Flatten(list).Any(obj => obj.ToString() == value.ToString());
        }

	    public static List<object> Flatten(IEnumerable list)
        {
            var ret = new List<object>();
	        if (list == null) return ret;

            foreach (var item in list)
            {
                if (item == null) continue;

                var arr = item as IEnumerable;
                if (arr != null && !(item is string))
                {
                    ret.AddRange(arr.Cast<object>());
                }
                else
                {
                    ret.Add(item);
                }
            }
            return ret;
        }

	    public static string Desc<T>(T value)
        {
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

