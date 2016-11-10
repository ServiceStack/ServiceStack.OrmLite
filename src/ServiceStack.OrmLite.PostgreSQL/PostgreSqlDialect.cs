using ServiceStack.OrmLite.PostgreSQL;

namespace ServiceStack.OrmLite
{
    public static class PostgreSqlDialect
    {
        public static IOrmLiteDialectProvider Provider => PostgreSqlDialectProvider.Instance;
    }
}