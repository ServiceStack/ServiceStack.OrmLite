namespace ServiceStack.OrmLite.MySql
{
    /// <summary>
    /// Description of MySqlExpressionVisitor.
    /// </summary>
    public class MySqlExpression<T> : SqlExpression<T>
    {
        public override SqlExpression<T> Clone()
        {
            return CopyTo(new MySqlExpression<T>());
        }
    }
}
