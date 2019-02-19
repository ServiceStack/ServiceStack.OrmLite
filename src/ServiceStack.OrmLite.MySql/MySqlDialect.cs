using ServiceStack.OrmLite.MySql;

namespace ServiceStack.OrmLite
{
    public static class MySqlDialect
    {
        public static IOrmLiteDialectProvider Provider => MySqlDialectProvider.Instance;
        public static MySqlDialectProvider Instance => MySqlDialectProvider.Instance;
    }
}