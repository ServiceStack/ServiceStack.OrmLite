using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.VistaDB
{
    public class VistaDbExpression<T> : SqlExpression<T>
    {
        public VistaDbExpression(IOrmLiteDialectProvider dialectProvider) 
            : base(dialectProvider) {}

        public override void PrepareUpdateStatement(IDbCommand dbCmd, T item, bool excludeDefaults = false)
        {
            CopyParamsTo(dbCmd);

            var setFields = StringBuilderCache.Allocate();

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

                setFields
                    .Append(DialectProvider.GetQuotedColumnName(fieldDef.FieldName))
                    .Append("=")
                    .Append(DialectProvider.GetUpdateParam(dbCmd, value, fieldDef));
            }

            var strFields = StringBuilderCache.ReturnAndFree(setFields);
            if (strFields.Length == 0)
                throw new ArgumentException("No non-null or non-default values were provided for type: " + typeof(T).Name);

            dbCmd.CommandText = $"UPDATE {DialectProvider.GetQuotedTableName(ModelDef)} SET {strFields} {WhereExpression}";
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
                ? $"substring({quotedColumn}, {startIndex}, {length.Value})"
                : $"substring({quotedColumn}, {startIndex}, LEN({quotedColumn}) - {startIndex} + 1)";
        }

        protected override PartialSqlString ToLengthPartialString(object arg)
        {
            return new PartialSqlString($"LEN({arg})");
        }
    }
}
