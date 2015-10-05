using System;
using System.Text;

namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServerExpression<T> : SqlExpression<T>
    {
        public SqlServerExpression(IOrmLiteDialectProvider dialectProvider)
            : base(dialectProvider) {}

        public override string ToUpdateStatement(T item, bool excludeDefaults = false)
        {
            return SqlServerExpressionUtils.ToSqlServerUpdateStatement(this, item, excludeDefaults);
        }

        public override string GetSubstringSql(object quotedColumn, int startIndex, int? length = null)
        {
            return length != null
                ? string.Format("substring({0}, {1}, {2})", quotedColumn, startIndex, length.Value)
                : string.Format("substring({0}, {1}, LEN({0}) - {1} + 1)", quotedColumn, startIndex);
        }

        public override SqlExpression<T> OrderByRandom()
        {
            return base.OrderBy("NEWID()");
        }
    }

    public class SqlServerParameterizedSqlExpression<T> : ParameterizedSqlExpression<T>
    {
        public SqlServerParameterizedSqlExpression(IOrmLiteDialectProvider dialectProvider)
            : base(dialectProvider) {}

        public override string ToUpdateStatement(T item, bool excludeDefaults = false)
        {
            return SqlServerExpressionUtils.ToSqlServerUpdateStatement(this, item, excludeDefaults);
        }

        public override string GetSubstringSql(object quotedColumn, int startIndex, int? length = null)
        {
            return length != null
                ? string.Format("substring({0}, {1}, {2})", quotedColumn, startIndex, length.Value)
                : string.Format("substring({0}, {1}, LEN({0}) - {1} + 1)", quotedColumn, startIndex);
        }

        public override SqlExpression<T> OrderByRandom()
        {
            return base.OrderBy("NEWID()");
        }

        protected override void ConvertToPlaceholderAndParameter(ref object right)
        {
            var paramName = Params.Count.ToString();
            var paramValue = right;
            var parameter = CreateParam(paramName, paramValue);

            // Prevents a new plan cache for each different string length. Every string is parameterized as NVARCHAR(max) 
            if (parameter.DbType == System.Data.DbType.String)
                parameter.Size = -1;

            Params.Add(parameter);

            right = parameter.ParameterName;
        }
    }

    internal class SqlServerExpressionUtils
    {
        internal static string ToSqlServerUpdateStatement<T>(SqlExpression<T> q, T item, bool excludeDefaults = false)
        {
            var modelDef = q.ModelDef;
            var dialectProvider = q.DialectProvider;

            var setFields = new StringBuilder();

            foreach (var fieldDef in modelDef.FieldDefinitions)
            {
                if (fieldDef.ShouldSkipUpdate()) continue;
                if (fieldDef.IsRowVersion) continue;
                if (q.UpdateFields.Count > 0
                    && !q.UpdateFields.Contains(fieldDef.Name)
                    || fieldDef.AutoIncrement)
                    continue; // added

                var value = fieldDef.GetValue(item);
                if (excludeDefaults
                    && (value == null || (!fieldDef.IsNullable && value.Equals(value.GetType().GetDefaultValue()))))
                    continue;

                fieldDef.GetQuotedValue(item, dialectProvider);

                if (setFields.Length > 0)
                    setFields.Append(", ");

                setFields
                    .Append(dialectProvider.GetQuotedColumnName(fieldDef.FieldName))
                    .Append("=")
                    .Append(dialectProvider.GetQuotedValue(value, fieldDef.FieldType));
            }

            if (setFields.Length == 0)
                throw new ArgumentException("No non-null or non-default values were provided for type: " + typeof(T).Name);

            return string.Format("UPDATE {0} SET {1} {2}",
                dialectProvider.GetQuotedTableName(modelDef), setFields, q.WhereExpression);
        }
    }
}