using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using ServiceStack.OrmLite;

namespace ServiceStack.OrmLite.Sqlite
{
	/// <summary>
	/// Description of SqliteExpressionVisitor.
	/// </summary>
	public class SqliteExpressionVisitor<T>: SqlExpressionVisitor<T>
	{
		public SqliteExpressionVisitor():base()
		{
		}
		
		protected override string VisitMethodCall(MethodCallExpression m)
        {           
			List<Object> args = this.VisitExpressionList(m.Arguments);
			
            object r ;
			if (m.Object!=null)
				r=Visit(m.Object);
			else {
				r= args[0];
				args.RemoveAt(0);
			}
            						
			switch (m.Method.Name) {
			case "ToUpper":
				return string.Format("upper({0})",r);
			case "ToLower":
				return string.Format("lower({0})",r);
			case "StartsWith": 
				return string.Format("{0} like '{1}%'",r,RemoveQuote(args[0].ToString()) );
			case "EndsWith":
				return string.Format("{0} like '%{1}'",r,RemoveQuote(args[0].ToString()) );
			case "Contains":	
				return string.Format("{0} like '%{1}%'",r,RemoveQuote(args[0].ToString()) );
			case "Substring":
				var startIndex = Int32.Parse(args[0].ToString() ) +1;
				if (args.Count==2){
					var length = Int32.Parse(  args[1].ToString() );
					return string.Format("substr({0}, {1}, {2})",
				                     r,
				                     startIndex,
				                     length); 
				}	
				else
					return string.Format("substr({0}, {1})",
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
				                     args.Count==1? string.Format(",{0}", args[0]):"" );
			case "Concat":
				var s = new StringBuilder();
				foreach (var e in args) {
					s.AppendFormat( " || {0}", e);
				}
				return string.Format("{0}{1}", r, s);
			
			case "In":
								
				var member = Expression.Convert(m.Arguments[1], typeof(object));
			    var lambda = Expression.Lambda<Func<object>>(member);
    			var getter = lambda.Compile();
				
				var inArgs = getter() as object[];
				
				var sIn = new StringBuilder();
				foreach (var e in inArgs) {
					if(e.GetType().ToString()!="System.Collections.Generic.List`1[System.Object]"){
						sIn.AppendFormat("{0}{1}",
					                 sIn.Length>0 ? ",":"",
					                 OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()));
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
				return string.Format("{0} {1} ({2})", r, m.Method.Name,  sIn);
				
			case "Desc":
				return string.Format("{0} DESC", r);
			case "Alias":
			case "As":
				return string.Format("{0} As {1}", r, 
					OrmLiteConfig.DialectProvider.GetQuotedColumnName(RemoveQuoteFromAlias(args[0].ToString())));
			case "ToString":
				return r.ToString();
			default:
				var s2 = new StringBuilder();
				foreach (var e in args) {
					s2.AppendFormat(",{0}", 
					                OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()) );
				}
				return string.Format("{0}({1}{2})", m.Method.Name, r, s2);				
			}
        }
	}
}
