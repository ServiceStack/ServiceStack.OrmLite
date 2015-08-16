using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.PostgreSQL.Converters
{
    public class PostgreSqlTimeSpanAsIntConverter : TimeSpanAsIntConverter
    {
        public override string ColumnDefinition
        {
            get { return "INTERVAL"; }
        }

        public override DbType DbType
        {
            get { return DbType.Time; }
        }
    }
}