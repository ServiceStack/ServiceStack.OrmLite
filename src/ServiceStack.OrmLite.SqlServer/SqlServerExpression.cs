using System;
using System.Data;
using System.Text;
using ServiceStack.OrmLite.SqlServer.Converters;

namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServerExpression<T> : SqlExpression<T>
    {
        public SqlServerExpression(IOrmLiteDialectProvider dialectProvider)
            : base(dialectProvider) {}

        public override void PrepareUpdateStatement(IDbCommand dbCmd, T item, bool excludeDefaults = false)
        {
            SqlServerExpressionUtils.PrepareSqlServerUpdateStatement(dbCmd, this, item, excludeDefaults);
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

        public override void PrepareUpdateStatement(IDbCommand dbCmd, T item, bool excludeDefaults = false)
        {
            SqlServerExpressionUtils.PrepareSqlServerUpdateStatement(dbCmd, this, item, excludeDefaults);
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

        protected override void VisitFilter(string operand, object originalLeft, object originalRight, ref object left, ref object right)
        {
            base.VisitFilter(operand, originalLeft, originalRight, ref left, ref right);

            if (originalRight is TimeSpan && DialectProvider.GetConverter<TimeSpan>() is SqlServerTimeConverter)
            {
                right = "CAST({0} AS TIME)".Fmt(right);
            }
        }
    }

    internal class SqlServerExpressionUtils
    {
        internal static void PrepareSqlServerUpdateStatement<T>(IDbCommand dbCmd, SqlExpression<T> q, T item, bool excludeDefaults = false)
        {
            q.CopyParamsTo(dbCmd);

            var modelDef = q.ModelDef;
            var DialectProvider = q.DialectProvider;

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

                if (setFields.Length > 0)
                    setFields.Append(", ");

                var param = DialectProvider.AddParam(dbCmd, value, fieldDef.ColumnType);
                setFields
                    .Append(DialectProvider.GetQuotedColumnName(fieldDef.FieldName))
                    .Append("=")
                    .Append(param.ParameterName);
            }

            if (setFields.Length == 0)
                throw new ArgumentException("No non-null or non-default values were provided for type: " + typeof(T).Name);

            dbCmd.CommandText = string.Format("UPDATE {0} SET {1} {2}",
                DialectProvider.GetQuotedTableName(modelDef), setFields, q.WhereExpression);
        }
    }
}