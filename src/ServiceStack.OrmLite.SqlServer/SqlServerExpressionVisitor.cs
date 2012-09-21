using System;

namespace ServiceStack.OrmLite.SqlServer
{
	public class SqlServerExpressionVisitor<T> : SqlExpressionVisitor<T>
	{
	    public override string ToSelectStatement()
        {
            if (!Skip.HasValue && !Rows.HasValue)
                return base.ToSelectStatement();

            AssertValidSkipRowValues();

            var skip = Skip.HasValue ? Skip.Value : 0;
            var take = Rows.HasValue ? Rows.Value : int.MaxValue;

	        var sql = "";

            //Temporary hack till we come up with a more robust paging sln for SqlServer
            if (skip == 0)
            {
                if (take == int.MaxValue)
                    return base.ToSelectStatement();

                sql = base.ToSelectStatement();
                if (sql == null || sql.Length < "SELECT".Length) return sql;
                sql = "SELECT TOP " + take + " " + sql.Substring("SELECT".Length, sql.Length - "SELECT".Length);
                return sql;
            }
	        
            var orderBy = !String.IsNullOrEmpty(OrderByExpression)
	                          ? OrderByExpression
	                          : BuildOrderByIdExpression();

	        OrderByExpression = String.Empty; // Required because ordering is done by Windowing function

            //todo: review needed only check against sql server 2008 R2

	        var selectExpression = SelectExpression.Remove(SelectExpression.IndexOf("FROM")).Trim(); //0
	        var tableName = OrmLiteConfig.DialectProvider.GetQuotedTableName(ModelDef).Trim(); //2
	        var statement = string.Format("{0} {1} {2}", WhereExpression, GroupByExpression, HavingExpression).Trim(); 

	        var retVal = string.Format(
	            "{0} FROM (SELECT ROW_NUMBER() OVER ({1}) As RowNum, * FROM {2} {3} ) AS RowConstrainedResult WHERE RowNum > {4} AND RowNum <= {5}",
	            selectExpression,
	            orderBy,
	            tableName,
                statement,
	            skip,
	            skip + take);

	        return retVal;
        }

        protected virtual void AssertValidSkipRowValues()
        {
            if (Skip.HasValue && Skip.Value < 0)
                throw new ArgumentException(String.Format("Skip value:'{0}' must be>=0", Skip.Value));

            if (Rows.HasValue && Rows.Value <0)
                throw new ArgumentException(string.Format("Rows value:'{0}' must be>=0", Rows.Value));
        }

	    protected virtual string BuildOrderByIdExpression()
	    {
	        if (ModelDef.PrimaryKey == null)
                throw new ApplicationException("Malformed model, no PrimaryKey defined");

	        return String.Format("ORDER BY {0}", ModelDef.PrimaryKey.FieldName);
	    }

		public override string LimitExpression
		{
			get
			{
				return "";
			}
		}
	}
}