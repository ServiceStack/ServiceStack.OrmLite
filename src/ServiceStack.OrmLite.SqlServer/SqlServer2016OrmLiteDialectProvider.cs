using System;
using System.Data;
using System.Text;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServer2016OrmLiteDialectProvider : SqlServer2014OrmLiteDialectProvider
    {
        public static new SqlServer2016OrmLiteDialectProvider Instance = new SqlServer2016OrmLiteDialectProvider();
    }
}