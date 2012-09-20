using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite.PostgreSQL
{
	public class PostgreSQLExpressionVisitor<T>:SqlExpressionVisitor<T>
	{
	    protected override object VisitMethodCall(MethodCallExpression m)
		{
		    return base.VisitMethodCall(m);

            //List<Object> args = this.VisitExpressionList(m.Arguments);
			
            //Object r ;
            //if( m.Object!=null)
            //    r=Visit(m.Object);
            //else{
            //    r= args[0];
            //    args.RemoveAt(0);
            //}
            						
            //switch(m.Method.Name){
            //case "ToUpper":
            //    return string.Format("upper({0})",r);
            //case "ToLower":
            //    return string.Format("lower({0})",r);
            //case "StartsWith": //LEFT( title, 1) = '#'
            //    return string.Format("upper({0}) like '{1}%'",r,RemoveQuote(args[0].ToString().ToUpper()) );
            //case "EndsWith":
            //    return string.Format("upper({0}) like '%{1}'",r,RemoveQuote(args[0].ToString().ToUpper()) );
            //case "Contains":	
            //    return string.Format("upper({0}) like '%{1}%'",r,RemoveQuote(args[0].ToString()).ToUpper() );
            //case "Substring":
            //    var startIndex = Int32.Parse(args[0].ToString() ) +1;
            //    if (args.Count==2){
            //        var length = Int32.Parse(  args[1].ToString() );
            //        return string.Format("substring({0} from {1} for {2})",
            //                         r,
            //                         startIndex,
            //                         length); 
            //    }	
            //    else
            //        return string.Format("substring({0} from {1})",
            //                         r,
            //                         startIndex); 	                     				
            //case "Round":
            //case "Floor":
            //case "Ceiling":	
            //case "Coalesce":
            //case "Abs":
            //case "Sum":
            //    return string.Format("{0}({1}{2})", 
            //                         m.Method.Name,
            //                         r, 
            //                         args.Count==1? string.Format(",{0}", args[0]):"" );
            //case "Concat":
            //    StringBuilder s = new StringBuilder();
            //    foreach(Object e in args ){
            //        s.AppendFormat( " || {0}", e);
            //    }
            //    return string.Format("{0}{1}",r, s.ToString());
			
            //case "In":
								
            //    var member = Expression.Convert(m.Arguments[1], typeof(object));
            //    var lambda = Expression.Lambda<Func<object>>(member);
            //    var getter = lambda.Compile();
				
            //    var inArgs = getter() as object[];
								
            //    StringBuilder sIn = new StringBuilder();
            //    foreach(Object e in inArgs ){
            //        if(e.GetType().ToString()!="System.Collections.Generic.List`1[System.Object]"){
            //            sIn.AppendFormat("{0}{1}",
            //                         sIn.Length>0 ? ",":"",
            //                         OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()) );
            //        }
            //        else{
            //        var listArgs= e as IList<Object>;
            //            foreach(Object el in listArgs){
            //                sIn.AppendFormat("{0}{1}",
            //                         sIn.Length>0 ? ",":"",
            //                         OrmLiteConfig.DialectProvider.GetQuotedValue(el, el.GetType()) );
            //            }
            //        }
            //    }
												
            //    return string.Format("{0} {1} ({2})", r, m.Method.Name,  sIn.ToString() );
            //case "Desc":
            //    return string.Format("{0} DESC", r);
            //case "As":
            //    return string.Format("{0} As {1}", r, 
            //        OrmLiteConfig.DialectProvider.GetQuotedColumnName(RemoveQuoteFromAlias(args[0].ToString())));
            //case "ToString":
            //    return r.ToString();
            //default:	
            //    StringBuilder s2 = new StringBuilder();
            //    foreach(Object e in args ){
            //        s2.AppendFormat(",{0}", 
            //                        OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()) );
            //    }
            //    return string.Format("{0}({1}{2})",m.Method.Name,r, s2.ToString());	
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
                        statement = string.Format("substring({0} from {1} for {2})",
                                         quotedColName,
                                         startIndex,
                                         length);
                    }
                    else
                        statement = string.Format("substring({0} from {1})",
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
								
				    StringBuilder sIn = new StringBuilder();
				    foreach(Object e in inArgs ){
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
				
		public override string LimitExpression{
			get{
				if(!Rows.HasValue) return "";
				string offset;
				if(Skip.HasValue){
					offset= string.Format(" OFFSET {0}", Skip.Value );
				}
				else{
					offset=string.Empty;
				}
				return string.Format("LIMIT {0}{1}", Rows.Value, offset);                   
			}
		}
		
	}
}