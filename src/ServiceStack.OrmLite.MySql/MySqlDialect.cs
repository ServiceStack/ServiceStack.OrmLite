using ServiceStack.OrmLite.MySql;

namespace ServiceStack.OrmLite
{
    public static class MySqlDialect
    {
        public static IOrmLiteDialectProvider Provider { get { return MySqlDialectProvider.Instance; } }
    }
}