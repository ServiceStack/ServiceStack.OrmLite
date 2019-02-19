namespace ServiceStack.OrmLite.Oracle
{
    public class OracleDialect
    {
        public static IOrmLiteDialectProvider Provider => OracleOrmLiteDialectProvider.Instance;
        public static OracleOrmLiteDialectProvider Instance => OracleOrmLiteDialectProvider.Instance;
    }
}