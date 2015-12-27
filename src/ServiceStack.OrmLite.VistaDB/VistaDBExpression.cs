using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite.VistaDB
{
    public class VistaDbExpression<T> : ParameterizedSqlExpression<T>
    {
        public VistaDbExpression(IOrmLiteDialectProvider dialectProvider) 
            : base(dialectProvider) {}

        public override void PrepareUpdateStatement(IDbCommand dbCmd, T item, bool excludeDefaults = false)
        {
            CopyParamsTo(dbCmd);

            var setFields = new StringBuilder();

            foreach (var fieldDef in ModelDef.FieldDefinitions)
            {
                if (UpdateFields.Count > 0 && !UpdateFields.Contains(fieldDef.Name) || fieldDef.AutoIncrement)
                    continue; // added

                var value = fieldDef.GetValue(item);
                if (excludeDefaults
                    && (value == null || (!fieldDef.IsNullable && value.Equals(value.GetType().GetDefaultValue()))))
                    continue;

                if (setFields.Length > 0)
                    setFields.Append(", ");

                var param = DialectProvider.AddParam(dbCmd, value);
                setFields
                    .Append(DialectProvider.GetQuotedColumnName(fieldDef.FieldName))
                    .Append("=")
                    .Append(param.ParameterName);
            }

            if (setFields.Length == 0)
                throw new ArgumentException("No non-null or non-default values were provided for type: " + typeof(T).Name);

            dbCmd.CommandText = string.Format("UPDATE {0} SET {1} {2}",
                DialectProvider.GetQuotedTableName(ModelDef), setFields, WhereExpression);
        }

        protected override object VisitColumnAccessMethod(MethodCallExpression m)
        {
            if (m.Arguments.Count == 1 && m.Method.Name == "Equals")
            {
                return Visit(
                    Expression.Equal(
                        Expression.Convert(m.Object, typeof(object)),
                        Expression.Convert(m.Arguments.First(), typeof(object))));
            }
            else
            {
                return base.VisitColumnAccessMethod(m);
            }
        }

        public override string GetSubstringSql(object quotedColumn, int startIndex, int? length = null)
        {
            return length != null
                ? string.Format("substring({0}, {1}, {2})", quotedColumn, startIndex, length.Value)
                : string.Format("substring({0}, {1}, LEN({0}) - {1} + 1)", quotedColumn, startIndex);
        }
    }
}
