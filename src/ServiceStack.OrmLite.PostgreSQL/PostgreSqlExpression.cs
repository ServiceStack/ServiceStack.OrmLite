using System.Linq;

namespace ServiceStack.OrmLite.PostgreSQL
{
    public class PostgreSqlExpression<T> : SqlExpression<T>
    {
        public PostgreSqlExpression(IOrmLiteDialectProvider dialectProvider)
            : base(dialectProvider) {}

        protected override string GetQuotedColumnName(ModelDefinition tableDef, string memberName)
        {
            if (useFieldName)
            {
                var fd = tableDef.FieldDefinitions.FirstOrDefault(x => x.Name == memberName);
                if (fd != null && fd.IsRowVersion && !PrefixFieldWithTableName)
                {
                    return PostgreSQLDialectProvider.RowVersionFieldComparer;
                }

                return base.GetQuotedColumnName(tableDef, memberName);
            }
            return memberName;
        }
    }
}