namespace ServiceStack.OrmLite.MySqlConnector
{
    public static class MySqlConnectorDialect
    {
        public static IOrmLiteDialectProvider Provider => MySqlConnectorDialectProvider.Instance;
    }
}