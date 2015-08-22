using System;
using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.Firebird.Converters
{
    public class FirebirdDateTimeOffsetConverter : DateTimeOffsetConverter
    {
        public override string ColumnDefinition
        {
            get { return "TIMESTAMP"; }
        }

        public override DbType DbType
        {
            get { return DbType.DateTime; }
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            var dateTimeOffset = (DateTimeOffset)value;
            var dateTime = dateTimeOffset.DateTime;
            var iso8601Format = dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
                .EndsWith("00:00:00.000")
                ? "yyyy-MM-dd"
                : "yyyy-MM-dd HH:mm:ss.fff";

            var dateConverter = DialectProvider.GetDateTimeConverter();
            return dateConverter.DateTimeFmt(dateTime, iso8601Format);
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            return base.ToDbValue(fieldType, value);
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            return base.FromDbValue(fieldType, value);
        }
    }
}