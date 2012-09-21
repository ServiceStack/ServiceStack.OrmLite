using System;

namespace ServiceStack.OrmLite.Firebird
{
	public class FirebirdSqlExpressionVisitor<T>:SqlExpressionVisitor<T>
	{
	    public override string LimitExpression{
			get{
				if(!Skip.HasValue) return "";
				int fromRow= Skip.Value+1;
				if(fromRow<=0)
					throw new ArgumentException(
						string.Format("Skip value:'{0}' must be>=0",Skip.Value ));
				string toRow;
				if(Rows.HasValue){
					if( Rows.Value<0) {
						throw new ArgumentException(
							string.Format("Rows value:'{0}' must be>=0", Rows.Value));
					}
					toRow= string.Format("TO {0}", fromRow+Rows.Value-1 );
				}
				else{
					toRow=string.Empty;
				}
				return string.Format("ROWS {0} {1}", fromRow, toRow);                   
			}
		}
				
	}
}

