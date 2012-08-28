using ServiceStack.OrmLite.Oracle;

namespace ServiceStack.OrmLite
{
    public static class OracleDialect
    {
        public static IOrmLiteDialectProvider Provider { get { return OracleOrmLiteDialectProvider.Instance; } }
    }
}