using System;
using System.Data;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
    public abstract partial class ParameterizedSqlExpression<T> : SqlExpression<T>
    {
        protected bool visitedExpressionIsTableColumn = false;
        protected bool SkipParameterizationForThisExpression { get; set; }

        protected ParameterizedSqlExpression(IOrmLiteDialectProvider dialectProvider) 
            : base(dialectProvider)
        {
            SkipParameterizationForThisExpression = false;
        }

        protected internal override object Visit(Expression exp)
        {
            visitedExpressionIsTableColumn = false;
            return base.Visit(exp);
        }

        protected internal override object VisitJoin(Expression exp)
        {
            SkipParameterizationForThisExpression = true;
            var visitedExpression = Visit(exp);
            SkipParameterizationForThisExpression = false;
            return visitedExpression;
        }

        protected virtual void ConvertToPlaceholderAndParameter(ref object right)
        {
            if (!OrmLiteConfig.UseParameterizeSqlExpressions)
                return;

            var parameter = AddParam(right);

            right = parameter.ParameterName;
        }

        public override object GetValue(object value, Type type)
        {
            if ((!OrmLiteConfig.UseParameterizeSqlExpressions) || SkipParameterizationForThisExpression)
                return DialectProvider.GetQuotedValue(value, type);

            var paramValue = DialectProvider.GetParamValue(value, type);
            return paramValue ?? "null";
        }

        protected override void VisitFilter(string operand, object originalLeft, object originalRight, ref object left, ref object right)
        {
            if (SkipParameterizationForThisExpression || visitedExpressionIsTableColumn)
                return;

            if (originalLeft is EnumMemberAccess && originalRight is EnumMemberAccess)
                return;

            if (operand == "AND" || operand == "OR" || operand == "is" || operand == "is not")
                return;

            ConvertToPlaceholderAndParameter(ref right);
        }

        protected override void OnVisitMemberType(Type modelType)
        {
            var tableDef = modelType.GetModelDefinition();
            if (tableDef != null)
                visitedExpressionIsTableColumn = true;
        }
    }
}