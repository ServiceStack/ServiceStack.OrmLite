using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite.Oracle
{
	public class OracleSqlExpressionVisitor<T>:SqlExpressionVisitor<T>
	{
        protected override object VisitColumnAccessMethod(MethodCallExpression m)
        {
            if (m.Method.Name == "Substring")
            {
                List<Object> args = this.VisitExpressionList(m.Arguments);
                var quotedColName = Visit(m.Object);
                var startIndex = Int32.Parse(args[0].ToString()) + 1;
                if (args.Count == 2)
                {
                    var length = Int32.Parse(args[1].ToString());
                    return new PartialSqlString(string.Format("subStr({0},{1},{2})",
                                                              quotedColName,
                                                              startIndex,
                                                              length));
                }

                return new PartialSqlString(string.Format("subStr({0},{1})",
                                                          quotedColName,
                                                          startIndex));
            }
            return base.VisitColumnAccessMethod(m);
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
            idx2 -= sql.Substring(idx2, 1) == "\"" ? 1 : 0;
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

