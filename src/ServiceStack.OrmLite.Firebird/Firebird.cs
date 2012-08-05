using ServiceStack.OrmLite.Firebird;

namespace ServiceStack.OrmLite
{
    public static class FirebirdDialect
    {
        public static IOrmLiteDialectProvider Provider { get { return FirebirdOrmLiteDialectProvider.Instance; } }
    }
}