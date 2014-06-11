namespace ServiceStack.OrmLite.PostgreSQL
{
    public class PostgreSqlExpression<T> : SqlExpression<T>
    {
        public override SqlExpression<T> Clone()
        {
            return CopyTo(new PostgreSqlExpression<T>());
        }
    }
}