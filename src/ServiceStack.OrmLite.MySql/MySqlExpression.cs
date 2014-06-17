namespace ServiceStack.OrmLite.MySql
{
    /// <summary>
    /// Description of MySqlExpressionVisitor.
    /// </summary>
    public class MySqlExpression<T> : SqlExpression<T>
    {
        public MySqlExpression(IOrmLiteDialectProvider dialectProvider)
            : base(dialectProvider) {}
    }
}
