using System;
using System.Text;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.PostgreSQL.Converters
{
    public class PostgreSqlStringConverter : StringConverter
    {
        public override string ColumnDefinition => "TEXT";

        public override string GetColumnDefinition(int? stringLength)
        {
            //PostgreSQL doesn't support NVARCHAR when UseUnicode = true so just use TEXT
            if (stringLength == null || stringLength == StringLengthAttribute.MaxText)
                return ColumnDefinition;

            return $"VARCHAR({stringLength.Value})";
        }
    }

    public class PostgreSqlCharArrayConverter : CharArrayConverter
    {
        public override string ColumnDefinition => "TEXT";

        public override string GetColumnDefinition(int? stringLength)
        {
            return ColumnDefinition;
        }
    }
}