using System;
using System.Data;
using System.Text;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServer2017OrmLiteDialectProvider : SqlServer2016OrmLiteDialectProvider
    {
        public new static SqlServer2017OrmLiteDialectProvider Instance = new SqlServer2017OrmLiteDialectProvider();
    }
}