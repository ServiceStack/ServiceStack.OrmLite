using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
	public abstract class SqlExpressionVisitor<T>  //where T: new()
	{
				
		private Expression<Func<T, bool>> wherePredicate;
		private Expression<Func<T, bool>> havingPredicate;
		private string orderBy= string.Empty;
		private string groupBy= string.Empty;
		List<string> updateFields= new List<string>();
		List<string> insertFields= new List<string>();
		private string selectExpression= string.Empty;
		private string sep= string.Empty;
		private ModelDefinition modelDef;
		private bool useFieldName=false;
		private string whereExpression;
		private string havingExpression;
		
		protected  int? Rows{ get ; private set;}
		protected  int? FromRow { get ; private set;}
		
				
		public SqlExpressionVisitor ()
		{
			modelDef = typeof(T).GetModelDefinition();
		}
				
		
		public virtual SqlExpressionVisitor<T> Update<TKey>(Expression<Func<T, TKey>> fields){
			sep=string.Empty;
			useFieldName=false;
			updateFields=Visit(fields).Split(',').ToList();
			return this;
		}
		
		public virtual SqlExpressionVisitor<T> Insert<TKey>(Expression<Func<T, TKey>> fields){
			sep=string.Empty;
			useFieldName=false;
			insertFields=Visit(fields).Split(',').ToList();
			return this;
		}
		
		
		public virtual SqlExpressionVisitor<T> Select(){
			return Select<T>(null);
		}
		
		public virtual SqlExpressionVisitor<T> Select<TKey>(Expression<Func<T, TKey>> fields){
			sep=string.Empty;
			useFieldName=true;
			selectExpression=Visit(fields);
			return this;
		}
		
		public virtual IList<string> UpdateFields{
			get{
				return updateFields;	
			}
		}
		
		public virtual IList<string> InsertFields{
			get {
				return insertFields;
			}
		}
		
		public virtual string SelectExpression{
			get{
				return 
					(!string.IsNullOrEmpty(selectExpression) ?
						string.Format("SELECT {0} \nFROM {1}",
						selectExpression,
						OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef)):
						selectExpression);
			}
			set{
				selectExpression=value;
			}
		}
		
		public virtual SqlExpressionVisitor<T> Limit(int fromrow, int rows ){
			Rows=rows;
			FromRow= fromrow;
			return this;
		}
		
		public virtual SqlExpressionVisitor<T> Limit(int rows){
			FromRow= rows;
			Rows=null;
			return this;
		}
		
		public virtual SqlExpressionVisitor<T> Limit(){
			FromRow= null;
			Rows=null;
			return this;
		}
		
		public virtual string LimitExpression{
			get{
				if(!FromRow.HasValue) return "";
				if(FromRow.Value<=0)
					throw new ArgumentException("FromRow value must be>0");
				string rows;
				if(Rows.HasValue){
					if( Rows.Value<0) {
						throw new ArgumentException("Rows  value must be>=0");
					}
					rows= string.Format("TO {0}", FromRow.Value+Rows.Value-1 );
				}
				else{
					rows=string.Empty;
				}
				return string.Format("ROWS {0} {1}", FromRow.Value, rows);                   
			}
		}
		
		public virtual SqlExpressionVisitor<T> Where(){
			return Where(null);
		}
		
		public virtual SqlExpressionVisitor<T> Where(Expression<Func<T, bool>> predicate){
			whereExpression=string.Empty;
			wherePredicate= predicate;
			return this;
		}
		
		public virtual SqlExpressionVisitor<T> Having(){
			return Having(null);
		}
		
		public virtual SqlExpressionVisitor<T> Having(Expression<Func<T, bool>> predicate){
			havingExpression=string.Empty;
			havingPredicate= predicate;
			return this;
		}
		
		
		public virtual SqlExpressionVisitor<T> OrderBy(){
			orderBy=string.Empty;
			return this;
		}
	
		public virtual SqlExpressionVisitor<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector){
			sep=string.Empty;
			useFieldName=true;
			orderBy = Visit(keySelector);
			return this;
		}	
		
		public virtual SqlExpressionVisitor<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector){
			sep=string.Empty;
			useFieldName=true;
			orderBy = Visit(keySelector);
			if( !string.IsNullOrEmpty(orderBy))
				orderBy=orderBy+" DESC";
			return this;
		}
		
		public virtual SqlExpressionVisitor<T> GroupBy(){
			groupBy=string.Empty;
			return this;
		}
				
		public virtual SqlExpressionVisitor<T> GroupBy<TKey>(Expression<Func<T, TKey>> keySelector){
			sep=string.Empty;
			useFieldName=true;
			groupBy = Visit(keySelector);
			return this;
		}
		
		
		public virtual string WhereExpression{
			get{
				if(wherePredicate!=null ) {
					
					useFieldName=true;
					sep=" ";
					whereExpression= Visit( wherePredicate );
					
					if( whereExpression=="1" || whereExpression=="'1'") 
						whereExpression= string.Empty;
					else if (whereExpression=="0" || whereExpression=="'0'") 
						whereExpression= "1=0";
					
				}
				return !string.IsNullOrEmpty(whereExpression)?
					string.Format("WHERE {0}", whereExpression):
					whereExpression;
			}
			set{	
				wherePredicate= null;
				whereExpression= value;
			}
			
		}
		
		public virtual string HavingExpression{
			get{
				if(havingPredicate !=null ) {
					useFieldName=true;
					sep=" ";
					havingExpression= Visit( havingPredicate );
				}
				
				return !string.IsNullOrEmpty(havingExpression)?
					string.Format("HAVING {0}", havingExpression):
					havingExpression;
			}
			set{	
				havingPredicate= null;
				havingExpression= value;
			}
		}
		
		
		public virtual string OrderByExpression{
			get {
				return string.IsNullOrEmpty(orderBy)?
					string.Empty:
					string.Format("ORDER BY {0}",orderBy);
			}
			set{
				orderBy = value;
			}
		}
		
		
		public virtual string GroupByExpression{
			get {
				return string.IsNullOrEmpty(groupBy)?
					string.Empty:
					string.Format("GROUP BY {0}",groupBy);
			}
			set{
				groupBy = value;
			}
		}
		
		protected virtual string Visit(Expression exp){
			
			if(exp==null) return string.Empty;
			switch (exp.NodeType){
			case ExpressionType.Lambda:
				return VisitLambda( exp as LambdaExpression);
			case ExpressionType.MemberAccess:
				return VisitMemberAccess( exp as MemberExpression);
			case ExpressionType.Constant:
				return VisitConstant(exp as ConstantExpression);
			case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return "("+ VisitBinary(exp as BinaryExpression)+")";
				case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary(exp as UnaryExpression);
				case ExpressionType.Parameter:
                    return VisitParameter(exp as ParameterExpression);
				case ExpressionType.Call:
                    return VisitMethodCall(exp as MethodCallExpression);
				case ExpressionType.New:
                    return VisitNew(exp as NewExpression);
				case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray(exp as NewArrayExpression);
			default:
				Console.WriteLine("************* NodeType is :'{0}' **********",  exp.NodeType);
				return  exp.ToString();
			}
		}
		
		
		protected virtual string VisitLambda(LambdaExpression lambda){
			if (lambda.Body.NodeType==ExpressionType.MemberAccess   && sep==" "){
				MemberExpression m = lambda.Body as MemberExpression;
				
				if(m.Expression != null ){
					string r = VisitMemberAccess( m);
					return string.Format("{0}={1}", r, GetQuotedTrueValue()	);
				}
				
			}
			return Visit( lambda.Body);	
		}
		
		
		protected virtual string VisitBinary(BinaryExpression b)
        {
			string left,right;	
			var operand = BindOperant( b.NodeType ) ;   //sep= " " ??
			if(operand=="AND" || operand=="OR"){
				MemberExpression m = b.Left as MemberExpression;
				if(m!=null && m.Expression != null ){
					string r = VisitMemberAccess( m);
					left= string.Format("{0}={1}", r,GetQuotedTrueValue());
				}
				else{
					left= Visit( b.Left);
				}
				m = b.Right as MemberExpression;
				if(m!=null &&  m.Expression != null ){
					string r = VisitMemberAccess( m);
					right= string.Format("{0}={1}", r,GetQuotedTrueValue());
				}
				else{
					right = Visit(b.Right);		
				}
			}
			else{
				left= Visit( b.Left);
				right = Visit(b.Right);		
			}
						
			if(operand=="=" && right=="null") operand="is";
			else if(operand=="<>" && right=="null") operand="is not";
			else if (operand=="=" || operand=="<>"){
				if( IsTrueExpression(right) ) right= GetQuotedTrueValue();
				else if (IsFalseExpression(right) )right = GetQuotedFalseValue(); 
				
				if( IsTrueExpression(left) ) left= GetQuotedTrueValue();
				else if (IsFalseExpression(left) ) left = GetQuotedFalseValue(); 
				
			}
			
			switch(operand){
			case "MOD":
			case "COALESCE":
				return string.Format("{0}({1},{2})", operand, left, right);
			default:
				return  left+sep+operand+sep+right;
			}				
        }
		
		
		protected virtual string  VisitMemberAccess(MemberExpression m)
        {
			if(m.Expression != null){
				string o = GetFieldName( m.Member.Name );
				return o;
			}
			else{
				var member = Expression.Convert(m, typeof(object));
			    var lambda = Expression.Lambda<Func<object>>(member);
    			var getter = lambda.Compile();
				object o = getter();
				return  OrmLiteConfig.DialectProvider.GetQuotedValue(o, o.GetType());
			}
		}
		
		
		protected virtual string VisitNew(NewExpression nex)
        {            
			// TODO : check !
            var member = Expression.Convert(nex, typeof(object));
			var lambda = Expression.Lambda<Func<object>>(member);
			try{
    			var getter = lambda.Compile();
				object o = getter();
				return  OrmLiteConfig.DialectProvider.GetQuotedValue(o, o.GetType());
			}
			catch(System.InvalidOperationException){ // FieldName ?
				List<Object> exprs = VisitExpressionList(nex.Arguments);
            	StringBuilder r = new StringBuilder();                                     
            	foreach(Object e in exprs){
					r.AppendFormat("{0}{1}",
					               r.Length>0? ",":"",
					               e );
				}
				return r.ToString();
			}
            
        }
		
		
		protected virtual string VisitParameter(ParameterExpression p)
        {
            return p.Name;    
        }
		
		protected virtual string VisitConstant(ConstantExpression c)
        {
			if( c.Value== null ) 
				return "null";
			else if( c.Value.GetType()== typeof(bool) ){ 
				object o = OrmLiteConfig.DialectProvider.GetQuotedValue(c.Value, c.Value.GetType());
				return string.Format("({0}={1})",GetQuotedTrueValue(),o);
			}
			else
  				return OrmLiteConfig.DialectProvider.GetQuotedValue(c.Value, c.Value.GetType());
		}
		
		protected virtual string VisitUnary(UnaryExpression u)
        {
			switch (u.NodeType){
			case ExpressionType.Not:
				string o= Visit(u.Operand);
				if( IsFieldName(o) ) o= o+"="+ OrmLiteConfig.DialectProvider.GetQuotedValue(true,typeof(bool));
				return "NOT ("+o+")";
			default:
				return Visit(u.Operand);
					
			}

        }

		
		protected virtual string VisitMethodCall(MethodCallExpression m)
        {           
			List<Object> args = this.VisitExpressionList(m.Arguments);
			
            Object r ;
			if( m.Object!=null)
				r=Visit(m.Object);
			else{
				r= args[0];
				args.RemoveAt(0);
			}
            						
			switch(m.Method.Name){
			case "ToUpper":
				return string.Format("upper({0})",r);
			case "ToLower":
				return string.Format("lower({0})",r);
			case "StartsWith":
				return string.Format("{0} starting with {1} ",r, args[0] );
			case "EndsWith":
				return string.Format("{0} like '%{1}'",r,RemoveQuote(args[0].ToString()) );
			case "Contains":	
				return string.Format("{0} like '%{1}%'",r,RemoveQuote(args[0].ToString()) );
			case "Substring":
				var startIndex = Int32.Parse(args[0].ToString() ) +1;
				if (args.Count==2){
					var length = Int32.Parse(  args[1].ToString() );
					return string.Format("substring({0} from {1} for {2})",
				                     r,
				                     startIndex,
				                     length); 
				}	
				else
					return string.Format("substring({0} from {1})",
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
				StringBuilder s = new StringBuilder();
				foreach(Object e in args ){
					s.AppendFormat( " || {0}", e);
				}
				return string.Format("{0}{1}",r, s.ToString());
			
			case "In":
								
				var member = Expression.Convert(m.Arguments[1], typeof(object));
			    var lambda = Expression.Lambda<Func<object>>(member);
    			var getter = lambda.Compile();
				
				var inArgs = getter() as IList<Object>;
				
				
				StringBuilder sIn = new StringBuilder();
				foreach(Object e in inArgs ){
					sIn.AppendFormat("{0}{1}",
					                 sIn.Length>0 ? ",":"",
					                 OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()) );
				}
												
				return string.Format("{0} {1} ({2})", r, m.Method.Name,  sIn.ToString() );
			case "Desc":
				return string.Format("{0} DESC", r);
			case "As":
				return string.Format("{0} As {1}", r, 
					OrmLiteConfig.DialectProvider.GetNameDelimited( RemoveQuote( args[0].ToString() ) ) );
			case "ToString":
				return r.ToString();
			default:
				Console.WriteLine("******* Returning '{0}' for '{1}' *******", r, m.Method.Name);
				
				StringBuilder s2 = new StringBuilder();
				foreach(Object e in args ){
					s2.AppendFormat(",{0}", 
					                OrmLiteConfig.DialectProvider.GetQuotedValue(e, e.GetType()) );
				}
				return string.Format("{0}({1}{2})",m.Method.Name,r, s2.ToString());
				
			}
			
        }
		
		protected virtual List<Object> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Object> list = new List<Object>();
            for (int i = 0, n = original.Count; i < n; i++)
            {
				if (original[i].NodeType==ExpressionType.NewArrayInit ||
                 original[i].NodeType==ExpressionType.NewArrayBounds ){
				
					list.AddRange( VisitNewArrayFromExpressionList( original[i] as NewArrayExpression)) ;
				}
				else 
					list.Add(  Visit(original[i]) );                
                    
            }            
            return list;
        }
		
		protected virtual string VisitNewArray(NewArrayExpression na)
        {

            List<Object> exprs = VisitExpressionList(na.Expressions);
            StringBuilder r = new StringBuilder();                                     
            foreach(Object e in exprs){
				r.Append( r.Length>0? ","+e: e );
			}
			
			return r.ToString();
        }
		
		protected virtual List<Object> VisitNewArrayFromExpressionList(NewArrayExpression na)
        {

            List<Object> exprs = VisitExpressionList(na.Expressions);
            return exprs;
        }
		
		
		protected virtual string BindOperant(ExpressionType e){
			
			switch (e){
			case ExpressionType.Equal:
				return "=";
			case ExpressionType.NotEqual:
				return "<>";
			case ExpressionType.GreaterThan:
				return ">";           
			case ExpressionType.GreaterThanOrEqual:
				return ">=";           	
			case ExpressionType.LessThan:
				return "<";           
			case ExpressionType.LessThanOrEqual:
				return "<=";
			case ExpressionType.AndAlso:
				return "AND";
			case ExpressionType.OrElse:
                return "OR";           	
			case ExpressionType.Add:
                return "+";
			case ExpressionType.Subtract:
                return "-";           		
			case ExpressionType.Multiply:
                return "*";           			
			case ExpressionType.Divide:
			    return "/";           			
			case ExpressionType.Modulo:
                return "MOD";
			case ExpressionType.Coalesce:
				return "COALESCE";
			default:
				return e.ToString();
			}
		}
		
		protected virtual string GetFieldName(string name){
			
			if(useFieldName){
				FieldDefinition fd = modelDef.FieldDefinitions.FirstOrDefault(x=>x.Name==name);
				string fn = fd!=default(FieldDefinition)? fd.FieldName:name;
				return OrmLiteConfig.DialectProvider.GetNameDelimited(fn);
			}
			else{
				return name;
			}   
		}
		
		public virtual string ToDeleteRowStatement(){
			return string.Format("DELETE FROM {0} {1}", 
			                     OrmLiteConfig.DialectProvider.GetTableNameDelimited(modelDef),
			                     (! string.IsNullOrEmpty(WhereExpression)?
			                     WhereExpression:
			                     ""));
		}
		
		public virtual string ToSelectStatement(){
			StringBuilder sql = new StringBuilder();
			
			sql.Append(string.IsNullOrEmpty(SelectExpression)? 
				OrmLiteConfig.DialectProvider.ToSelectStatement(typeof(T), string.Empty):
				SelectExpression );
			sql.Append( string.IsNullOrEmpty(WhereExpression)?
			           "":
			           "\n"+WhereExpression);
			sql.Append( string.IsNullOrEmpty(GroupByExpression)?
			           "":
			           "\n"+GroupByExpression);
			sql.Append( string.IsNullOrEmpty(HavingExpression)?
			           "":
			           "\n"+HavingExpression);
			sql.Append( string.IsNullOrEmpty(OrderByExpression)?
			           "":
			           "\n"+OrderByExpression);
			sql.Append( string.IsNullOrEmpty(LimitExpression)?
			           "":
			           "\n"+LimitExpression);
			return sql.ToString();
			
		}
		
		
		protected string RemoveQuote(string exp){
			
			if (exp.StartsWith("'") )
				exp=exp.Remove(0,1);
			if( exp.EndsWith("'") )
				exp =exp.Remove(exp.Length-1,1);
			
			return exp;				
		}
		
		
		private bool IsFieldName( string quotedExp){
			FieldDefinition fd =
				modelDef.FieldDefinitions.
					FirstOrDefault(x=>
						OrmLiteConfig.DialectProvider.
						GetNameDelimited(x.FieldName)==quotedExp);
			return (fd!=default(FieldDefinition));
		}
		
		private string GetTrueExpression(){
			object o = GetQuotedTrueValue();
			return string.Format("({0}={1})", o,o);
		}
		
		private string GetFalseExpression(){
			
			return string.Format("({0}={1})",
				GetQuotedTrueValue(),
				GetQuotedFalseValue());
		}
		
		private bool IsTrueExpression(string exp){
			return ( exp== GetTrueExpression());
		}
		
		private bool IsFalseExpression(string exp){
			return ( exp== GetFalseExpression());
		}
		
		
		private string GetQuotedTrueValue(){
			return OrmLiteConfig.DialectProvider.GetQuotedValue(true,typeof(bool));
		}
			
		private string GetQuotedFalseValue(){
			return OrmLiteConfig.DialectProvider.GetQuotedValue(false,typeof(bool));
		}
		
	}
	
}

