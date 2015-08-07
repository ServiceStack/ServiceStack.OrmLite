using System;
using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.PostgreSQL.Converters
{
    public class PostgreSqlDateTimeOffsetConverter : DateTimeOffsetConverter
    {
        public override string ColumnDefinition
        {
            get { return "timestamp with time zone"; }
        }

        public override string ToQuotedString(object value)
        {
            var dateValue = (DateTimeOffset)value;
            const string iso8601Format = "yyyy-MM-dd HH:mm:ss.fff zzz";
            return base.DialectProvider.GetQuotedValue(dateValue.ToString(iso8601Format), typeof(string));
        }

        public override object ToDbValue(FieldDefinition fieldDef, object value)
        {
            return value;
        }

        public override object FromDbValue(FieldDefinition fieldDef, IDataReader reader, int columnIndex)
        {
            var dateTime = reader.GetDateTime(columnIndex);
            var dateTimeOffset = new DateTimeOffset(dateTime);
            return dateTimeOffset;
        }
    }
}