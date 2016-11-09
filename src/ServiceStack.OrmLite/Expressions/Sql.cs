using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace ServiceStack.OrmLite
{
    public static class Sql
    {
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

        public static bool In<T, TItem>(T value, params TItem[] list) => value != null && Flatten(list).Any(obj => obj.ToString() == value.ToString());

        public static bool In<T, TItem>(T value, SqlExpression<TItem> query) => value != null && query != null;

        public static string Desc<T>(T value) => value == null ? "" : value + " DESC";

        public static string As<T>(T value, object asValue) => value == null ? "" : $"{value} AS {asValue}";

        public static T Sum<T>(T value) => value;

        public static string Sum(string value) => $"SUM({value})";

        public static T Count<T>(T value) => value;

        public static T CountDistinct<T>(T value) => value;

        public static string Count(string value) => $"COUNT({value})";

        public static T Min<T>(T value) => value;

        public static string Min(string value) => $"MIN({value})";

        public static T Max<T>(T value) => value;

        public static string Max(string value) => $"MAX({value})";

        public static T Avg<T>(T value) => value;

        public static string Avg(string value) => $"AVG({value})";

        public static T AllFields<T>(T item) => item;

        public static string JoinAlias<T>(T property, string tableAlias) => tableAlias;

        public static string Custom(string customSql) => customSql;
    }

}

