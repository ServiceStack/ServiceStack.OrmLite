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
        SqlServerMdf = 1 << 2,
        SqlServer2008 = 1 << 3,
        SqlServer2012 = 1 << 4,
        SqlServer2014 = 1 << 5,
        SqlServer2016 = 1 << 6,
        SqlServer2017 = 1 << 7,
        
        PostgreSql9 = 1 << 8,
        PostgreSql10 = 1 << 9,
        PostgreSql11 = 1 << 10,
        
        MySql5_5 = 1 << 11,
        MySql10_1 = 1 << 12,
        MySql10_2 = 1 << 13,
        MySql10_3 = 1 << 14,
        MySql10_4 = 1 << 15,
        
        Oracle10 = 1 << 16,
        Oracle11 = 1 << 17,
        Oracle12 = 1 << 18,
        Oracle18 = 1 << 19,
        
        Firebird = 1 << 20,
        
        VistaDb = 1 << 21,
        
        // any versions
        AnyPostgreSql = PostgreSql9 | PostgreSql10 | PostgreSql11,
        AnyMySql = MySql5_5 | MySql10_1 | MySql10_2 | MySql10_3 | MySql10_4,
        AnySqlServer = SqlServer | SqlServer2008 | SqlServer2012 | SqlServer2014 | SqlServer2016 | SqlServer2017 | SqlServerMdf,
        AnyOracle = Oracle10 | Oracle11 | Oracle12 | Oracle18,
        
        // db groups
        BaseSupported = MySql5_5 | PostgreSql9 | SqlServer | Sqlite,
        Supported = Sqlite | AnySqlServer | AnyPostgreSql | AnyMySql,
        BaseCommunity = Firebird | Oracle10 | VistaDb,
        Community = Firebird | AnyOracle | VistaDb,
        DockerDb =  AnySqlServer | AnyPostgreSql | AnyMySql | Community,
        
        // all
        All = Supported | Community
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