using System;
using System.Text;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.PostgreSQL.Converters
{
    public class PostgreSqlStringConverter : StringConverter
    {
        public override string ColumnDefinition
        {
            get { return "TEXT"; }
        }
    }

    public class PostgreSqlCharConverter : CharConverter
    {
        public override string ColumnDefinition
        {
            get { return "CHAR(1)"; }
        }
    }

    public class PostgreSqlCharArrayConverter : CharArrayConverter
    {
        public override string ColumnDefinition
        {
            get { return "TEXT"; }
        }
    }
}