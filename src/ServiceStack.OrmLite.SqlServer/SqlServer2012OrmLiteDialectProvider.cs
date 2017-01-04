using System.Data;
using System.Text;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.SqlServer
{
    public class SqlServer2012OrmLiteDialectProvider : SqlServerOrmLiteDialectProvider
    {
        public new static SqlServer2012OrmLiteDialectProvider Instance = new SqlServer2012OrmLiteDialectProvider();

        public override string ToSelectStatement(ModelDefinition modelDef,
            string selectExpression,
            string bodyExpression,
            string orderByExpression = null,
            int? offset = null,
            int? rows = null)
        {
            var sb = StringBuilderCache.Allocate()
                .Append(selectExpression)
                .Append(bodyExpression);

            if (orderByExpression != null)
                sb.Append(orderByExpression);

            if (offset != null || rows != null)
            {
                if (orderByExpression.IsEmpty())
                {
                    var orderBy = offset == null && rows == 1 //Avoid for Single requests
                        ? "1"
                        : this.GetQuotedColumnName(modelDef, modelDef.PrimaryKey);

                    sb.Append(" ORDER BY " + orderBy);
                }

                sb.Append(" OFFSET ").Append(offset.GetValueOrDefault()).Append(" ROWS");

                if (rows != null)
                    sb.Append(" FETCH NEXT ").Append(rows.Value).Append(" ROWS ONLY");
            }

            return StringBuilderCache.ReturnAndFree(sb);
        }

        public override void AppendFieldCondition(StringBuilder sqlFilter, FieldDefinition fieldDef, IDbCommand cmd)
        {
            if (!isSpatialField(fieldDef))
            {
                base.AppendFieldCondition(sqlFilter, fieldDef, cmd);
            }
            else 
            {
                sqlFilter
                    .Append(GetQuotedColumnName(fieldDef.FieldName))
                    .Append(".STEquals(")
                    .Append(this.GetParam(SanitizeFieldNameForParamName(fieldDef.FieldName)))
                    .Append(") = 1");
 
                AddParameter(cmd, fieldDef);
            }
        }

        public override void AppendNullFieldCondition(StringBuilder sqlFilter, FieldDefinition fieldDef)
        {
            if (!isSpatialField(fieldDef))
            {
                base.AppendNullFieldCondition(sqlFilter, fieldDef);
            }
            else
            {
                sqlFilter
                    .Append(GetQuotedColumnName(fieldDef.FieldName))
                    .Append(".IsNull = 1");
            }
        }

        private bool isSpatialField(FieldDefinition fieldDef) => 
            fieldDef.FieldType.Name == "SqlGeography" || fieldDef.FieldType.Name == "SqlGeometry";
    }
}