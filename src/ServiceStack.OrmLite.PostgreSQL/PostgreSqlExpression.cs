namespace ServiceStack.OrmLite.PostgreSQL
{
    public class PostgreSqlExpression<T> : SqlExpression<T>
    {
        public override string LimitExpression
        {
            get
            {
                if (!Rows.HasValue) return "";
                string offset;
                if (Offset.HasValue)
                {
                    offset = string.Format(" OFFSET {0}", Offset.Value);
                }
                else
                {
                    offset = string.Empty;
                }
                return string.Format("LIMIT {0}{1}", Rows.Value, offset);
            }
        }

        public override SqlExpression<T> Clone()
        {
            return CopyTo(new PostgreSqlExpression<T>());
        }
    }
}