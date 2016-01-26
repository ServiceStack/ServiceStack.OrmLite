using System.Text;

namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServer2012OrmLiteDialectProvider : SqlServerOrmLiteDialectProvider
    {
        public static SqlServer2012OrmLiteDialectProvider Instance = new SqlServer2012OrmLiteDialectProvider();

        public override string ToSelectStatement(ModelDefinition modelDef,
            string selectExpression,
            string bodyExpression,
            string orderByExpression = null,
            int? offset = null,
            int? rows = null)
        {
            var sb = new StringBuilder(selectExpression)
                .Append(bodyExpression);

            if (orderByExpression != null)
                sb.Append(orderByExpression);

            if (offset != null || rows != null)
            {
                if (orderByExpression.IsEmpty())
                    sb.Append(" ORDER BY 1");

                sb.Append(" OFFSET ").Append(offset.GetValueOrDefault()).Append(" ROWS");

                if (rows != null)
                    sb.Append(" FETCH NEXT ").Append(rows.Value).Append(" ROWS ONLY");
            }

            return sb.ToString();
        }
    }
}