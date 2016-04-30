namespace ServiceStack.OrmLite.MySql
{
    public class MySqlExpression<T> : SqlExpression<T>
    {
        public MySqlExpression(IOrmLiteDialectProvider dialectProvider)
            : base(dialectProvider) {}
    }
}
