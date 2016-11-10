namespace ServiceStack.OrmLite.Oracle
{
    public class OracleDialect
    {
        public static IOrmLiteDialectProvider Provider => OracleOrmLiteDialectProvider.Instance;
    }
}