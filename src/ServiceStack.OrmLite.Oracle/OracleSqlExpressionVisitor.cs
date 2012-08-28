using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using ServiceStack.OrmLite;

namespace ServiceStack.OrmLite.Oracle
{
	public class OracleSqlExpressionVisitor<T>:SqlExpressionVisitor<T>
	{
		public OracleSqlExpressionVisitor ():base()
		{
		}

        protected override string VisitMethodCall(MethodCallExpression m)
        {
            List<Object> args = this.VisitExpressionList(m.Arguments);

            Object r;
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
                case "StartsWith":
                    return string.Format("upper({0}) like '{1}%' ", r, RemoveQuote(args[0].ToString()).ToUpper());
                case "EndsWith":
                    return string.Format("upper({0}) like '%{1}'", r, RemoveQuote(args[0].ToString()).ToUpper());
                case "Contains":
                    return string.Format("upper({0}) like '%{1}%'", r, RemoveQuote(args[0].ToString()).ToUpper());
                case "Substring":
                    var startIndex = Int32.Parse(args[0].ToString()) + 1;
                    if (args.Count == 2)
                    {
                        var length = Int32.Parse(args[1].ToString());
                        return string.Format("subStr({0},{1},{2})",
                                         r,
                                         startIndex,
                                         length);
                    }
                    else
                        return string.Format("subStr({0},{1})",
                                         r,
                                         startIndex);

                case "Ceiling":
                    return string.Format("{0}({1}{2})",
                                         "Ceil",
                                         r,
                                         args.Count == 1 ? string.Format(",{0}", args[0]) : "");

                case "Coalesce":
                    return string.Format("{0}({1}{2})",
                                         m.Method.Name,
                                         r,
                                         args.Count == 1 ? string.Format(",{0}", args[0]) : "");
                case "Round":
                case "Floor":
                case "Abs":
                case "Sum":
                    return string.Format("{0}({1}{2})",
                                         m.Method.Name,
                                         r,
                                         args.Count == 1 ? string.Format(",{0}", args[0]) : "");
                case "Concat":
                    StringBuilder s = new StringBuilder();
                    foreach (Object e in args)
                    {
                        s.AppendFormat(" || {0}", e);
                    }
                    return string.Format("{0}{1}", r, s.ToString());

                case "In":

                    var member = Expression.Convert(m.Arguments[1], typeof(object));
                    var lambda = Expression.Lambda<Func<object>>(member);
                    var getter = lambda.Compile();

                    var inArgs = getter() as object[];

                    StringBuilder sIn = new StringBuilder();
                    foreach (Object e in inArgs)
                    {
                        if (e.GetType().ToString() != "System.Collections.Generic.List`1[System.Object]")
                        {
                            sIn.AppendFormat("{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                         OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()));
                        }
                        else
                        {
                            var listArgs = e as IList<Object>;
                            foreach (Object el in listArgs)
                            {
                                sIn.AppendFormat("{0}{1}",
                                         sIn.Length > 0 ? "," : "",
                                         OrmLiteConfig.DialectProvider.GetQuotedValue(el, el.GetType()));
                            }
                        }
                    }

                    return string.Format("{0} {1} ({2})", r, m.Method.Name, sIn.ToString());
                case "Desc":
                    return string.Format("{0} DESC", r);
                case "Alias":
                case "As":
                    return string.Format("{0} As {1}", r,
                        OrmLiteConfig.DialectProvider.GetQuotedColumnName(RemoveQuoteFromAlias(RemoveQuote(args[0].ToString()))));
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

		public override string LimitExpression{
			get
            {
                return "";
			}
		}

        //from Simple.Data.Oracle implementation https://github.com/flq/Simple.Data.Oracle/blob/master/Simple.Data.Oracle/OraclePager.cs
        private static string UpdateWithOrderByIfNecessary(string sql)
        {
            if (sql.IndexOf("order by ", StringComparison.InvariantCultureIgnoreCase) != -1)
                return sql;
            var col = GetFirstColumn(sql);
            return sql + " ORDER BY " + col;
        }

        private static string GetFirstColumn(string sql)
        {
            var idx1 = sql.IndexOf("select") + 7;
            var idx2 = sql.IndexOf(",", idx1);
            return sql.Substring(idx1, idx2 - 7).Trim();
        }

        protected override string ApplyPaging(string sql)
        {
            if (!Rows.HasValue) 
                return sql;
            if (!Skip.HasValue)
            {
                Skip = 0;
            }
            sql = UpdateWithOrderByIfNecessary(sql);
            var sb = new StringBuilder();
            sb.AppendLine("SELECT * FROM (");
            sb.AppendLine("SELECT \"_ss_ormlite_1_\".*, ROWNUM RNUM FROM (");
            sb.Append(sql);
            sb.AppendLine(") \"_ss_ormlite_1_\"");
            sb.AppendFormat("WHERE ROWNUM <= {0} + {1}) \"_ss_ormlite_2_\" ", Skip.Value, Rows.Value);
            sb.AppendFormat("WHERE \"_ss_ormlite_2_\".RNUM > {0}", Skip.Value);

            return sb.ToString();

        }
				
	}
}

