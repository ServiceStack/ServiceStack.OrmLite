using ServiceStack.OrmLite.SqlServer;

namespace ServiceStack.OrmLite
{
    public static class SqlServerDialect
    {
        public static IOrmLiteDialectProvider Provider => SqlServerOrmLiteDialectProvider.Instance;
    }

    public static class SqlServer2012Dialect
    {
        public static IOrmLiteDialectProvider Provider => SqlServer2012OrmLiteDialectProvider.Instance;
    }

    public static class SqlServer2014Dialect
    {
        public static IOrmLiteDialectProvider Provider => SqlServer2014OrmLiteDialectProvider.Instance;
    }

    public static class SqlServer2016Dialect
    {
        public static IOrmLiteDialectProvider Provider => SqlServer2016OrmLiteDialectProvider.Instance;
    }

    public static class SqlServer2017Dialect
    {
        public static IOrmLiteDialectProvider Provider => SqlServer2017OrmLiteDialectProvider.Instance;
    }
}