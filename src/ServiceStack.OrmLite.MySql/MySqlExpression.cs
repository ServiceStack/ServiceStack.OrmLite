using System.Collections.Generic;

namespace ServiceStack.OrmLite.MySql
{
    public class MySqlExpression<T> : SqlExpression<T>
    {
        public MySqlExpression(IOrmLiteDialectProvider dialectProvider)
            : base(dialectProvider) {}

        protected override PartialSqlString ToConcatPartialString(List<object> args)
        {
            return new PartialSqlString(string.Format("CONCAT({0})", string.Join(", ", args)));
        }
    }
}
