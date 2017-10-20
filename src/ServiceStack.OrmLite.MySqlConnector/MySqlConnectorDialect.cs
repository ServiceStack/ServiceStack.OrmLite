namespace ServiceStack.OrmLite.MySql
{
    public static class MySqlConnectorDialect
    {
        public static IOrmLiteDialectProvider Provider => MySqlConnectorDialectProvider.Instance;
    }
}