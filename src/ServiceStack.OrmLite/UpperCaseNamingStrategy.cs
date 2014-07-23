using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceStack.OrmLite
{
    public class UpperCaseNamingStrategy : OrmLiteNamingStrategyBase
    {
        public override string GetTableName(string name)
        {
            return name.ToUpper();
        }

        public override string GetColumnName(string name)
        {
            return name.ToUpper();
        }
    }
}
