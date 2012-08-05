using ServiceStack.OrmLite.SqlServer;

namespace ServiceStack.OrmLite
{
    public static class SqlServerDialect
    {
        public static IOrmLiteDialectProvider Provider { get { return SqlServerOrmLiteDialectProvider.Instance; } }
    }
}