using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStack.OrmLite
{
    public class UpperCaseNamingStrategy: OrmLiteNamingStrategyBase
	{
		public virtual string GetTableName(string name)
		{
			return name.ToUpper();
		}
 
		public virtual string GetColumnName(string name)
		{
			return name.ToUpper();
		}
	}
}
