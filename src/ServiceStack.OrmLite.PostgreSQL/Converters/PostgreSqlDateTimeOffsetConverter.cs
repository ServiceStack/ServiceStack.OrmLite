using System;
using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.PostgreSQL.Converters
{
    public class PostgreSqlDateTimeOffsetConverter : DateTimeOffsetConverter
    {
        public override string ColumnDefinition
        {
            //get { return "timestamp"; }
            get { return "timestamp with time zone"; }
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            var dateValue = (DateTimeOffset) value;
            const string iso8601Format = "yyyy-MM-dd HH:mm:ss.fff zzz";
            return base.DialectProvider.GetQuotedValue(dateValue.ToString(iso8601Format), typeof (string));
        }

        public override object ToDbValue(FieldDefinition fieldDef, object value)
        {
            return value;
        }

        //public override object FromDbValue(FieldDefinition fieldDef, object value)
        //{
        //    var dateTime = (DateTime)value;
        //    var dateTimeOffset = new DateTimeOffset(dateTime);
        //    return dateTimeOffset;
        //}

        //public override object GetValue(IDataReader reader, int columnIndex)
        //{
        //    return reader.GetDateTime(columnIndex);
        //}
    }
}
