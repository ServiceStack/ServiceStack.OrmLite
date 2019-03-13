using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite.Tests
{
    [Flags]
    public enum Dialect
    {
        Sqlite         = 1,
        SqlServer = 1 << 1,
        SqlServer2008 = 1 << 2,
        SqlServer2012 = 1 << 3,
        PostgreSql9 = 1 << 4,
        PostgreSql10 = 1 << 5,
        PostgreSql11 = 1 << 6,
        MySql5_5 = 1 << 7,
        MySql10_1 = 1 << 8,
        MySql10_2 = 1 << 9,
        MySql10_3 = 1 << 10,
        MySql10_4 = 1 << 11,
        SqlServerMdf = 1 << 12,
        Oracle = 1 << 13,
        Firebird = 1 << 14,
        VistaDb = 1 << 15,
        SqlServer2014 = 1 << 16,
        SqlServer2016 = 1 << 17,
        SqlServer2017 = 1 << 18,
        PostgreSql = PostgreSql9 | PostgreSql10 | PostgreSql11,
        MySql = MySql5_5 | MySql10_1 | MySql10_2 | MySql10_3 | MySql10_4,
        AnySqlServer = SqlServer | SqlServer2008 | SqlServer2012 | SqlServer2014 | SqlServer2016 | SqlServer2017 | SqlServerMdf,
    }

    public static class TestHelpers
    {
        public static string NormalizeSql(this string sql)
        {
            return sql.ToLower()
                .Replace("\"", "")
                .Replace("`", "")
                .Replace("_", "")
                .Replace(":", "@")   //postgresql
                .Replace("\n", " ")
                .TrimEnd(); 
        }

        public static string PreNormalizeSql(this string sql, IDbConnection db)
        {
            var paramString = db.GetDialectProvider().ParamString;
            if (paramString.Equals("@"))
                return sql;
            return sql.Replace("@", paramString);
        }

        public static List<int> AdjustIds(this IEnumerable<int> ids, int initialId)
        {
            var result = new List<int>();
            foreach (var id in ids)
                result.Add(id + initialId);
            return result;
        }
    }
}