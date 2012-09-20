using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

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

            //Broken SQL:
            /*
                SELECT Paged.* FROM 
                (SELECT ROW_NUMBER() OVER (ORDER BY Id) AS __RowNum, PagedTable.* 
                FROM (
                WITH __Data AS (SELECT "Id" ,...., ROW_NUMBER() OVER(ORDER BY "Id") AS [_#_]
                FROM "CVProject")
                SELECT "Id" ,... FROM __Data WHERE [_#_] BETWEEN 1 AND 1) AS PagedTable) AS Paged 
                WHERE __RowNum > 0 AND __RowNum <= 1 

             * ERROR: 
             * 
                Msg 156, Level 15, State 1, Line 4
                Incorrect syntax near the keyword 'WITH'.
                Msg 319, Level 15, State 1, Line 4
                Incorrect syntax near the keyword 'with'. If this statement is a common table expression, an xmlnamespaces clause or a change tracking context clause, the previous statement must be terminated with a semicolon.
                Msg 102, Level 15, State 1, Line 6
                Incorrect syntax near ')'.
             */
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

	    protected override object VisitMethodCall(MethodCallExpression m)
	    {
	        return base.VisitMethodCall(m);

            //var args = this.VisitExpressionList(m.Arguments);

            //object r;
            //if (m.Object != null)
            //    r = Visit(m.Object);
            //else
            //{
            //    r = args[0];
            //    args.RemoveAt(0);
            //}

            //switch (m.Method.Name)
            //{
            //    case "ToUpper":
            //        return string.Format("upper({0})", r);
            //    case "ToLower":
            //        return string.Format("lower({0})", r);
            //    case "StartsWith": //LEFT( title, 1) = '#'
            //        return string.Format("upper({0}) like '{1}%'", r, RemoveQuote(args[0].ToString().ToUpper()));
            //    case "EndsWith":
            //        return string.Format("upper({0}) like '%{1}'", r, RemoveQuote(args[0].ToString().ToUpper()));
            //    case "Contains":
            //        return string.Format("upper({0}) like '%{1}%'", r, RemoveQuote(args[0].ToString()).ToUpper());
            //    case "Substring":
            //        var startIndex = Int32.Parse(args[0].ToString()) + 1;
            //        if (args.Count == 2)
            //        {
            //            var length = Int32.Parse(args[1].ToString());
            //            return string.Format("substring({0}, {1}, {2})",
            //                             r,
            //                             startIndex,
            //                             length);
            //        }
            //        else
            //            return string.Format("substring({0}, {1})",
            //                             r,
            //                             startIndex);
            //    case "Round":
            //    case "Floor":
            //    case "Ceiling":
            //    case "Coalesce":
            //    case "Abs":
            //    case "Sum":
            //        return string.Format("{0}({1}{2})",
            //                             m.Method.Name,
            //                             r,
            //                             args.Count == 1 ? string.Format(",{0}", args[0]) : "");
            //    case "Concat":
            //        var s = new StringBuilder();
            //        foreach (var e in args)
            //        {
            //            s.AppendFormat(" || {0}", e);
            //        }
            //        return string.Format("{0}{1}", r, s);

            //    case "In":

            //        var member = Expression.Convert(m.Arguments[1], typeof(object));
            //        var lambda = Expression.Lambda<Func<object>>(member);
            //        var getter = lambda.Compile();

            //        var inArgs = getter() as object[];
					
            //        var sIn = new StringBuilder();
            //        foreach (var e in inArgs)
            //        {
            //            if(e.GetType().ToString()!="System.Collections.Generic.List`1[System.Object]"){
            //                sIn.AppendFormat("{0}{1}",
            //                             sIn.Length>0 ? ",":"",
            //                             OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()) );
            //            }
            //            else{
            //            var listArgs= e as IList<Object>;
            //                foreach(Object el in listArgs){
            //                    sIn.AppendFormat("{0}{1}",
            //                             sIn.Length>0 ? ",":"",
            //                             OrmLiteConfig.DialectProvider.GetQuotedValue(el, el.GetType()) );
            //                }
            //            }
            //        }

            //        return string.Format("{0} {1} ({2})", r, m.Method.Name, sIn.ToString());
            //    case "Desc":
            //        return string.Format("{0} DESC", r);
            //    case "As":
            //        return string.Format("{0} As {1}", r,
            //            OrmLiteConfig.DialectProvider.GetQuotedColumnName(RemoveQuoteFromAlias(args[0].ToString())));
            //    case "ToString":
            //        return r.ToString();
            //    default:
            //        StringBuilder s2 = new StringBuilder();
            //        foreach (Object e in args)
            //        {
            //            s2.AppendFormat(",{0}",
            //                            OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()));
            //        }
            //        return string.Format("{0}({1}{2})", m.Method.Name, r, s2.ToString());
            //}

		}

        protected override object VisitColumnAccessMethod(MethodCallExpression m)
        {
            List<Object> args = this.VisitExpressionList(m.Arguments);
            var quotedColName = Visit(m.Object);
            var statement = "";

            switch (m.Method.Name)
            {
                case "ToUpper":
                    statement = string.Format("upper({0})", quotedColName);
                    break;
                case "ToLower":
                    statement = string.Format("lower({0})", quotedColName);
                    break;
                case "StartsWith": //LEFT( title, 1) = '#'
                    statement = string.Format("upper({0}) like '{1}%'", quotedColName, RemoveQuote(args[0].ToString().ToUpper()));
                    break;
                case "EndsWith":
                    statement = string.Format("upper({0}) like '%{1}'", quotedColName, RemoveQuote(args[0].ToString().ToUpper()));
                    break;
                case "Contains":
                    statement = string.Format("upper({0}) like '%{1}%'", quotedColName, RemoveQuote(args[0].ToString()).ToUpper());
                    break;
                case "Substring":
                    var startIndex = Int32.Parse(args[0].ToString()) + 1;
                    if (args.Count == 2)
                    {
                        var length = Int32.Parse(args[1].ToString());
                        statement = string.Format("substring({0}, {1}, {2})",
                                         quotedColName,
                                         startIndex,
                                         length);
                    }
                    else
                        statement = string.Format("substring({0}, {1})",
                                         quotedColName,
                                         startIndex);
                    break;
                default:
                    throw new NotSupportedException();
            }
            return new PartialSqlString(statement);
        }

        protected override object VisitSqlMethodCall(MethodCallExpression m)
        {
            List<Object> args = this.VisitExpressionList(m.Arguments);
            object quotedColName = args[0];
            args.RemoveAt(0);

            var statement = "";

            switch (m.Method.Name)
            {
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

					statement = string.Format("{0} {1} ({2})", quotedColName, m.Method.Name, sIn.ToString());
                    break;
                case "Desc":
                    statement = string.Format("{0} DESC", quotedColName);
                    break;
                case "As":
                    statement = string.Format("{0} As {1}", quotedColName,
                        OrmLiteConfig.DialectProvider.GetQuotedColumnName(RemoveQuoteFromAlias(args[0].ToString())));
                    break;
                case "Sum":
                case "Count":
                case "Min":
                case "Max":
                case "Avg":
                    statement = string.Format("{0}({1}{2})",
                                         m.Method.Name,
                                         quotedColName,
                                         args.Count == 1 ? string.Format(",{0}", args[0]) : "");
                    break;
                default:
                    throw new NotSupportedException();
            }

            return new PartialSqlString(statement);
        }


		public override string LimitExpression
		{
			get
			{
				return "";
			}
		}


        //Modified paging code from https://github.com/markrendle/Simple.Data/blob/master/Simple.Data.SqlServer/SqlQueryPager.cs
        protected override string ApplyPaging(string sql)
        {
            return sql;

            //paging related code was moved to this.ToSelectStatement()

            //if (!Rows.HasValue)
            //    return sql;
            //if (!Skip.HasValue)
            //{
            //    Skip = 0;
            //}

            ////SEE broken comment in: ToSelectStatement()
            //if (Skip == 0) return sql; //short-circuit for only Take queries

            //var builder = new StringBuilder("WITH __Data AS (SELECT ");

            //var match = ColumnExtract.Match(sql);
            //var columns = match.Groups[1].Value.Trim();
            //var fromEtc = match.Groups[2].Value.Trim();

            //builder.Append(columns);

            //var orderBy = ExtractOrderBy(columns, ref fromEtc);

            //builder.AppendFormat(", ROW_NUMBER() OVER({0}) AS [_#_]", orderBy);
            //builder.AppendLine();
            //builder.Append(fromEtc);
            //builder.AppendLine(")");
            //builder.AppendFormat("SELECT {0} FROM __Data WHERE [_#_] BETWEEN {1} AND {2}", DequalifyColumns(columns),
            //                     Skip.Value + 1, Skip.Value + Rows.Value);

            //return builder.ToString();
        }

        private static readonly Regex ColumnExtract = new Regex(@"SELECT\s*(.*)\s*(FROM.*)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        private static readonly Regex SelectMatch = new Regex(@"^SELECT\s*", RegexOptions.IgnoreCase);

        private static string DequalifyColumns(string original)
        {
            var q = from part in original.Split(',')
                    select part.Substring(Math.Max(part.LastIndexOf('.') + 1, part.LastIndexOf('[')));
            if (q != null)
                return string.Join(",", q.ToArray());
            else
                return "";
        }

        private static string ExtractOrderBy(string columns, ref string fromEtc)
        {
            string orderBy;
            int index = fromEtc.IndexOf("ORDER BY", StringComparison.InvariantCultureIgnoreCase);
            if (index > -1)
            {
                orderBy = fromEtc.Substring(index).Trim();
                fromEtc = fromEtc.Remove(index).Trim();
            }
            else
            {
                orderBy = "ORDER BY " + columns.Split(',').First().Trim();

                var aliasIndex = orderBy.IndexOf(" AS [", StringComparison.InvariantCultureIgnoreCase);

                if (aliasIndex > -1)
                {
                    orderBy = orderBy.Substring(0, aliasIndex);
                }
            }
            return orderBy;
        }

	}
}