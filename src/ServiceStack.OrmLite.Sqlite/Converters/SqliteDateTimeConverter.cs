using System;
using System.Data;
using System.Globalization;
using ServiceStack.OrmLite.Converters;
using ServiceStack.Text.Common;

namespace ServiceStack.OrmLite.Sqlite.Converters
{
    public class SqliteDateTimeConverter : DateTimeConverter
    {
        public override string ColumnDefinition
        {
            get { return "VARCHAR(8000)"; }
        }

        public override DbType DbType
        {
            get { return DbType.DateTime; }
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            var dateTime = (DateTime)value;
            if (DateStyle == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
            }
            else if (DateStyle == DateTimeKind.Local && dateTime.Kind == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime();
            }
            else if (DateStyle == DateTimeKind.Utc)
            {
                dateTime = dateTime.Kind == DateTimeKind.Local
                    ? DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime()
                    : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

                return DialectProvider.GetQuotedValue(dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture), typeof(string));
            }

            var dateStr = DateTimeSerializer.ToLocalXsdDateTimeString(dateTime);
            dateStr = dateStr.Replace("T", " ");
            const int tzPos = 6; //"-00:00".Length;
            var timeZoneMod = dateStr.Substring(dateStr.Length - tzPos, 1);
            if (timeZoneMod == "+" || timeZoneMod == "-")
            {
                dateStr = dateStr.Substring(0, dateStr.Length - tzPos);
            }

            return DialectProvider.GetQuotedValue(dateStr, typeof(string));
        }

        public override object FromDbValue(FieldDefinition fieldDef, object value)
        {
            var dateTime = (DateTime)value;

            if (DateStyle == DateTimeKind.Unspecified)
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
            return base.FromDbValue(dateTime);
        }

        public override object GetValue(IDataReader reader, int columnIndex)
        {
            try
            {
                return reader.GetDateTime(columnIndex);
            }
            catch (Exception ex)
            {
                var dateStr = reader.GetString(columnIndex);
                Log.Warn("Error reading string as DateTime in Sqlite: " + dateStr, ex);
                return DateTime.Parse(dateStr);
            }
        }
    }
}