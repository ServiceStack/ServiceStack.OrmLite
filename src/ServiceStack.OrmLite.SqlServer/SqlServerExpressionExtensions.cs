using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServer
{
    public static class SqlServerExpressionExtensions
    {
        public static SqlExpression<T> JoinWithHint<T, Target>(this SqlExpression<T> expression, Expression<Func<T, Target, bool>> joinExpr, string sqlServerTableHint)
        {
            var sqlExpression = new SqlServerExpression<T>(expression.DialectProvider);

            return sqlExpression.JoinWithHint(joinExpr, sqlServerTableHint);
        }

        public static SqlExpression<T> LeftJoinWithHint<T, Target>(this SqlExpression<T> expression, Expression<Func<T, Target, bool>> joinExpr, string sqlServerTableHint)
        {
            var sqlExpression = new SqlServerExpression<T>(expression.DialectProvider);

            return sqlExpression.LeftJoinWithHint(joinExpr, sqlServerTableHint);

        }

        public static SqlExpression<T> RightJoinWithHint<T, Target>(this SqlExpression<T> expression, Expression<Func<T, Target, bool>> joinExpr, string sqlServerTableHint)
        {
            var sqlExpression = new SqlServerExpression<T>(expression.DialectProvider);

            return sqlExpression.RightJoinWithHint(joinExpr, sqlServerTableHint);

        }
    }
}
