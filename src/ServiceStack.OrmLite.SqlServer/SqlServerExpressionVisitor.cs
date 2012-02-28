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
						OrmLiteConfig.DialectProvider.GetColumnNameDelimited(RemoveQuote(args[0].ToString())));
				case "ToString":
					return r.ToString();
				default:
					Console.WriteLine("******* Returning '{0}' for '{1}' *******", r, m.Method.Name);

					StringBuilder s2 = new StringBuilder();
					foreach (Object e in args)
					{
						s2.AppendFormat(",{0}",
										OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()));
					}
					return string.Format("{0}({1}{2})", m.Method.Name, r, s2.ToString());

			}

		}

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