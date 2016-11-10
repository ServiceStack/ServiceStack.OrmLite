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
}