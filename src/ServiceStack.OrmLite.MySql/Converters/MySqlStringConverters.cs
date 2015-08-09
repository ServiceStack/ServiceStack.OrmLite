using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.MySql.Converters
{
    public class MySqlStringConverter : StringConverter
    {
        public MySqlStringConverter() : base(255) {}

        public override string MaxColumnDefinition
        {
            get { return "TEXT"; }
        }
    }

    public class MySqlCharConverter : CharConverter
    {
        public override string ColumnDefinition
        {
            get { return "CHAR(1)"; }
        }
    }

    public class MySqlCharArrayConverter : CharArrayConverter
    {
        public MySqlCharArrayConverter() : base(255) { }

        public override string MaxColumnDefinition
        {
            get { return "TEXT"; }
        }
    }
}