using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite.Tests
{
    [Flags]
    public enum Dialect
    {
        Sqlite = 1,
        SqlServer = 2,
        SqlServer2008 = 4,
        SqlServer2012 = 8,
        PostgreSql = 16,
        MySql = 32,
        SqlServerMdf = 64,
        Oracle = 128,
        Firebird = 256,
        VistaDb = 512,
        AnySqlServer = SqlServer | SqlServer2008 | SqlServer2012 | SqlServerMdf,
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