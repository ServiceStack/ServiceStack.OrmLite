using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
    public class SqlInValues
    {
        private readonly IEnumerable values;

        public int Count { get; private set; }

        public SqlInValues(IEnumerable values)
        {
            this.values = values;

            if (values != null)
                foreach (var value in values)
                    ++Count;
        }

        public string ToSqlInString()
        {
            if (Count == 0)
                return "NULL";

            return OrmLiteUtilExtensions.SqlJoin(values);
        }
    }
}