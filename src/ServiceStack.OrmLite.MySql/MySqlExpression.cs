using System.Collections.Generic;
using System.Linq.Expressions;

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

        protected override string ToCast(string quotedColName)
        {
            return string.Format("cast({0} as char(1000))", quotedColName);
        }
    }
}
