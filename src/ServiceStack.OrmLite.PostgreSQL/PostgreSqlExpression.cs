namespace ServiceStack.OrmLite.PostgreSQL
{
    public class PostgreSqlExpression<T> : SqlExpression<T>
    {
        public PostgreSqlExpression(IOrmLiteDialectProvider dialectProvider)
            : base(dialectProvider) {}
    }
}