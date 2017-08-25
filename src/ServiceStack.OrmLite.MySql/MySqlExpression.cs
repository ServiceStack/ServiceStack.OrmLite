namespace ServiceStack.OrmLite.MySql
{
    public class MySqlExpression<T> : SqlExpression<T>
    {
        public MySqlExpression(IOrmLiteDialectProvider dialectProvider)
            : base(dialectProvider) {}

        protected override string ToCast(string quotedColName)
        {
            return $"cast({quotedColName} as char(1000))";
        }

        public override string ToDeleteRowStatement()
        {
            return base.tableDefs.Count > 1
                ? $"DELETE {DialectProvider.GetQuotedTableName(modelDef)} {FromExpression} {WhereExpression}"
                : base.ToDeleteRowStatement();
        }

        protected override string CreateInSubQuerySql(object quotedColName, string subSelect)
        {
            return $"{quotedColName} IN (SELECT * FROM ({subSelect})  SubQuery)";
        }
    }
}
