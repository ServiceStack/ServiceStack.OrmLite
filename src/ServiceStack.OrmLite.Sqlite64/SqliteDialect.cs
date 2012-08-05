using ServiceStack.OrmLite.Sqlite;

namespace ServiceStack.OrmLite
{
    public static class SqliteDialect
    {
        public static IOrmLiteDialectProvider Provider { get { return SqliteOrmLiteDialectProvider.Instance; } }
    }
}