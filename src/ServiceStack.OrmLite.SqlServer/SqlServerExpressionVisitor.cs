using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using ServiceStack.OrmLite;

namespace ServiceStack.OrmLite.SqlServer
{
	public class SqlServerExpressionVisitor<T> : SqlExpressionVisitor<T>
	{
		public SqlServerExpressionVisitor() : base()
		{
		}

        public override string ToSelectStatement()
        {
            if (!Skip.HasValue && !Rows.HasValue)
                return base.ToSelectStatement();

            AssertValidSkipRowValues();

            var buffer = new StringBuilder();
            var orderBy = !String.IsNullOrEmpty(OrderByExpression)
                              ? OrderByExpression
                              : BuildOrderByIdExpression();

            var skip = Skip.HasValue ? Skip.Value : 0;
            var take = Rows.HasValue ? Rows.Value : int.MaxValue;

            OrderByExpression = String.Empty; // Required because ordering is done by
                                              // Windowing function

            buffer.AppendFormat("SELECT Paged.* FROM \n" + 
                                "(SELECT ROW_NUMBER() OVER ({0}) AS __RowNum, PagedTable.* \n" +
                                "FROM (\n{1}) AS PagedTable) AS Paged \n", orderBy, base.ToSelectStatement());

            buffer.AppendFormat("WHERE __RowNum > {0} AND __RowNum <= {1} \n", skip, skip + take);

            return buffer.ToString();
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

	    protected override string VisitMethodCall(MethodCallExpression m)
		{
			var args = this.VisitExpressionList(m.Arguments);

			object r;
			if (m.Object != null)
				r = Visit(m.Object);
			else
			{
				r = args[0];
				args.RemoveAt(0);
			}

			switch (m.Method.Name)
			{
				case "ToUpper":
					return string.Format("upper({0})", r);
				case "ToLower":
					return string.Format("lower({0})", r);
				case "StartsWith": //LEFT( title, 1) = '#'
					return string.Format("upper({0}) like '{1}%'", r, RemoveQuote(args[0].ToString().ToUpper()));
				case "EndsWith":
					return string.Format("upper({0}) like '%{1}'", r, RemoveQuote(args[0].ToString().ToUpper()));
				case "Contains":
					return string.Format("upper({0}) like '%{1}%'", r, RemoveQuote(args[0].ToString()).ToUpper());
				case "Substring":
					var startIndex = Int32.Parse(args[0].ToString()) + 1;
					if (args.Count == 2)
					{
						var length = Int32.Parse(args[1].ToString());
						return string.Format("substring({0}, {1}, {2})",
										 r,
										 startIndex,
										 length);
					}
					else
						return string.Format("substring({0}, {1})",
										 r,
										 startIndex);
				case "Round":
				case "Floor":
				case "Ceiling":
				case "Coalesce":
				case "Abs":
				case "Sum":
					return string.Format("{0}({1}{2})",
										 m.Method.Name,
										 r,
										 args.Count == 1 ? string.Format(",{0}", args[0]) : "");
				case "Concat":
					var s = new StringBuilder();
					foreach (var e in args)
					{
						s.AppendFormat(" || {0}", e);
					}
					return string.Format("{0}{1}", r, s);

				case "In":

					var member = Expression.Convert(m.Arguments[1], typeof(object));
					var lambda = Expression.Lambda<Func<object>>(member);
					var getter = lambda.Compile();

					var inArgs = getter() as object[];
					
					var sIn = new StringBuilder();
					foreach (var e in inArgs)
					{
						if(e.GetType().ToString()!="System.Collections.Generic.List`1[System.Object]"){
							sIn.AppendFormat("{0}{1}",
						                 sIn.Length>0 ? ",":"",
						                 OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()) );
						}
						else{
						var listArgs= e as IList<Object>;
							foreach(Object el in listArgs){
								sIn.AppendFormat("{0}{1}",
						                 sIn.Length>0 ? ",":"",
						                 OrmLiteConfig.DialectProvider.GetQuotedValue(el, el.GetType()) );
							}
						}
					}

					return string.Format("{0} {1} ({2})", r, m.Method.Name, sIn.ToString());
				case "Desc":
					return string.Format("{0} DESC", r);
				case "As":
					return string.Format("{0} As {1}", r,
						OrmLiteConfig.DialectProvider.GetQuotedColumnName(RemoveQuoteFromAlias(args[0].ToString())));
				case "ToString":
					return r.ToString();
				default:
					StringBuilder s2 = new StringBuilder();
					foreach (Object e in args)
					{
						s2.AppendFormat(",{0}",
										OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()));
					}
					return string.Format("{0}({1}{2})", m.Method.Name, r, s2.ToString());
			}

		}


        /*
        private dynamic BuildPagedResult(string sql = "", string primaryKeyField = "", string where = "", string orderBy = "", string columns = "*", int pageSize = 20, int currentPage = 1, params object[] args)
        {
            dynamic result = new ExpandoObject();
            var countSQL = "";
            if (!string.IsNullOrEmpty(sql))
                countSQL = string.Format("SELECT COUNT({0}) FROM ({1}) AS PagedTable", primaryKeyField, sql);
            else
                countSQL = string.Format("SELECT COUNT({0}) FROM {1}", PrimaryKeyField, TableName);

            if (String.IsNullOrEmpty(orderBy))
            {
                orderBy = string.IsNullOrEmpty(primaryKeyField) ? PrimaryKeyField : primaryKeyField;
            }

            if (!string.IsNullOrEmpty(where))
            {
                if (!where.Trim().StartsWith("where", StringComparison.CurrentCultureIgnoreCase))
                {
                    where = " WHERE " + where;
                }
            }

            var query = "";
            if (!string.IsNullOrEmpty(sql))
                query = string.Format("SELECT {0} FROM (SELECT ROW_NUMBER() OVER (ORDER BY {2}) AS Row, {0} FROM ({3}) AS PagedTable {4}) AS Paged ", columns, pageSize, orderBy, sql, where);
            else
                query = string.Format("SELECT {0} FROM (SELECT ROW_NUMBER() OVER (ORDER BY {2}) AS Row, {0} FROM {3} {4}) AS Paged ", columns, pageSize, orderBy, TableName, where);

            var pageStart = (currentPage - 1) * pageSize;
            query += string.Format(" WHERE Row > {0} AND Row <={1}", pageStart, (pageStart + pageSize));
            countSQL += where;
            result.TotalRecords = Scalar(countSQL, args);
            result.TotalPages = result.TotalRecords / pageSize;
            if (result.TotalRecords % pageSize > 0)
                result.TotalPages += 1;
            result.Items = Query(string.Format(query, columns, TableName), args);
            return result;
        }
        */
        


		//Not supported SQL Server solution sucks
		//http://stackoverflow.com/questions/2135418/equivalent-of-limit-and-offset-for-sql-server
		public override string LimitExpression
		{
			get
			{
				return null;
				//if (!Rows.HasValue) return "";
				//string offset;
				//if (Skip.HasValue)
				//{
				//    offset = string.Format(" OFFSET {0}", Skip.Value);
				//}
				//else
				//{
				//    offset = string.Empty;
				//}
				//return string.Format("LIMIT {0}{1}", Rows.Value, offset);
			}
		}


	}
}