namespace ServiceStack.OrmLite.PostgreSQL
{
	public class PostgreSQLExpressionVisitor<T>:SqlExpressionVisitor<T>
	{
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