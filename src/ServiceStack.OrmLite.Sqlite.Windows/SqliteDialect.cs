using ServiceStack.OrmLite.Sqlite;

namespace ServiceStack.OrmLite
{
    public static class SqliteDialect
    {
        public static IOrmLiteDialectProvider Provider => SqliteOrmLiteDialectProvider.Instance;
    }
}