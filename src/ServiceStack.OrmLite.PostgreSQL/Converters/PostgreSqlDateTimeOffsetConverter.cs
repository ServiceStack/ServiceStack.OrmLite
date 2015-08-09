using System;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.PostgreSQL.Converters
{
    public class PostgreSqlDateTimeOffsetConverter : DateTimeOffsetConverter
    {
        public override string ColumnDefinition
        {
            get { return "timestamp with time zone"; }
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            var dateValue = (DateTimeOffset) value;
            const string iso8601Format = "yyyy-MM-dd HH:mm:ss.fff zzz";
            return base.DialectProvider.GetQuotedValue(dateValue.ToString(iso8601Format), typeof (string));
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            return value;
        }
    }
}
