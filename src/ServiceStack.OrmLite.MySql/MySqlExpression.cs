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

    public class MySqlParameterizedExpression<T> : ParameterizedSqlExpression<T>
    {
        public MySqlParameterizedExpression(IOrmLiteDialectProvider dialectProvider)
            : base(dialectProvider) {}
    }
}
