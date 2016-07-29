using System.Collections.Generic;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite.Firebird
{
    public class FirebirdSqlExpression<T> : SqlExpression<T>
    {
        public FirebirdSqlExpression(IOrmLiteDialectProvider dialectProvider) 
            : base(dialectProvider) {}

        protected override object VisitColumnAccessMethod(MethodCallExpression m)
        {
            var args = this.VisitExpressionList(m.Arguments);
            var quotedColName = Visit(m.Object);
            var statement = "";

            switch (m.Method.Name)
            {
                case "Trim":
                    statement = string.Format("trim({0})", quotedColName);
                    break;
                case "LTrim":
                    statement = string.Format("trim(leading from {0})", quotedColName);
                    break;
                case "RTrim":
                    statement = string.Format("trim(trailing from {0})", quotedColName);
                    break;
                default:
                    return base.VisitColumnAccessMethod(m);
            }
            return new PartialSqlString(statement);
        }

        protected override PartialSqlString ToConcatPartialString(List<object> args)
        {
            return new PartialSqlString(string.Join(" || ", args));
        }
    }
}

