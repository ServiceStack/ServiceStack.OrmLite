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

        public override string GetColumnDefinition(int? stringLength)
        {
            //PostgreSQL doesn't support NVARCHAR when UseUnicode = true so just use TEXT
            return ColumnDefinition;
        }
    }

    public class PostgreSqlCharArrayConverter : CharArrayConverter
    {
        public override string ColumnDefinition
        {
            get { return "TEXT"; }
        }

        public override string GetColumnDefinition(int? stringLength)
        {
            return ColumnDefinition;
        }
    }
}