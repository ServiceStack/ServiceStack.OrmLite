using System.Collections;

namespace ServiceStack.OrmLite
{
    public class SqlInValues
    {
        private readonly IEnumerable values;
        private readonly IOrmLiteDialectProvider dialectProvider;

        public int Count { get; private set; }

        public SqlInValues(IEnumerable values, IOrmLiteDialectProvider dialectProvider=null)
        {
            this.values = values;
            this.dialectProvider = dialectProvider ?? OrmLiteConfig.DialectProvider;

            if (values != null)
                foreach (var value in values)
                    ++Count;
        }

        public string ToSqlInString()
        {
            if (Count == 0)
                return "NULL";

            return OrmLiteUtils.SqlJoin(values, dialectProvider);
        }
    }
}