using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.MySql.Converters
{
    public class MySqlDateTimeOffsetConverter : DateTimeOffsetConverter
    {
        public override string ColumnDefinition
        {
            get { return "VARCHAR(255)"; }
        }
    }
}