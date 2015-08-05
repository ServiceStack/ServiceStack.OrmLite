using System;
using System.Data;
using System.Globalization;
using ServiceStack.Text;
using ServiceStack.Text.Common;

namespace ServiceStack.OrmLite.Converters
{
    public class SqliteDateTimeConverter : DateTimeConverter
    {
        public SqliteDateTimeConverter(IOrmLiteDialectProvider dialectProvider) : base(dialectProvider)
        { }

        public override string ToQuotedString(object value)
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

        public override object FromDbValue(FieldDefinition fieldDef, IDataReader reader, int columnIndex)
        {
            DateTime dateTime;
            try
            {
                dateTime = reader.GetDateTime(columnIndex);
            }
            catch (Exception)
            {
                var dateStr = reader.GetString(columnIndex);
                dateTime = DateTimeSerializer.ParseShortestXsdDateTime(dateStr);
            }

            if (DateStyle == DateTimeKind.Unspecified)
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
            return base.FromDbValue(dateTime);
        }
    }

    public class SqlServerDateTimeConverter : DateTimeConverter
    {
        public SqlServerDateTimeConverter(IOrmLiteDialectProvider dialectProvider) : base(dialectProvider)
        { }

        public override string ToQuotedString(object value)
        {
            return DateTimeFmt((DateTime)value, "yyyyMMdd HH:mm:ss.fff");
        }
    }

    public class MySqlDateTimeConverter : DateTimeConverter
    {
        public MySqlDateTimeConverter(IOrmLiteDialectProvider dialectProvider) : base(dialectProvider)
        { }

        public override string ToQuotedString(object value)
        {
            /*
             * ms not contained in format. MySql ignores ms part anyway
             * for more details see: http://dev.mysql.com/doc/refman/5.1/en/datetime.html
             */
            var dateTime = (DateTime)value;
            return DateTimeFmt(dateTime, "yyyy-MM-dd HH:mm:ss");
        }
    }

    public class PostgreSqlDateTimeConverter : DateTimeConverter
    {
        public PostgreSqlDateTimeConverter(IOrmLiteDialectProvider dialectProvider) : base(dialectProvider)
        { }
    }

    public class DateTimeConverter : IOrmLiteConverter
    {
        public IOrmLiteDialectProvider DialectProvider { get; set; }

        public DateTimeKind DateStyle
        {
            get { return DialectProvider.DateStyle; }
        }

        public DateTimeConverter(IOrmLiteDialectProvider dialectProvider)
        {
            DialectProvider = dialectProvider;
        }

        public virtual string ToQuotedString(object value)
        {
            var dateTime = (DateTime)value;
            return DateTimeFmt(dateTime, "yyyy-MM-dd HH:mm:ss.fff");
        }

        public virtual string DateTimeFmt(DateTime dateTime, string dateTimeFormat)
        {
            if (DateStyle == DateTimeKind.Utc && dateTime.Kind == DateTimeKind.Local)
                dateTime = dateTime.ToUniversalTime();

            if (DateStyle == DateTimeKind.Local && dateTime.Kind != DateTimeKind.Local)
                dateTime = dateTime.ToLocalTime();

            return DialectProvider.GetQuotedValue(dateTime.ToString(dateTimeFormat, CultureInfo.InvariantCulture), typeof(string));
        }

        public virtual object ToDbValue(FieldDefinition fieldDef, object value)
        {
            var dateTime = (DateTime)value;
            if (DateStyle == DateTimeKind.Utc && dateTime.Kind == DateTimeKind.Local)
            {
                dateTime = dateTime.ToUniversalTime();
            }
            else if (DateStyle == DateTimeKind.Local && dateTime.Kind != DateTimeKind.Local)
            {
                dateTime = dateTime.Kind == DateTimeKind.Utc
                    ? dateTime.ToLocalTime()
                    : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime();
            }

            return dateTime;
        }

        public virtual object FromDbValue(FieldDefinition fieldDef, IDataReader reader, int columnIndex)
        {
            var value = reader.GetValue(columnIndex);
            var strValue = value as string;
            if (strValue != null)
            {
                value = DateTimeSerializer.ParseShortestXsdDateTime(strValue);
            }

            return FromDbValue(value);
        }

        public virtual object FromDbValue(object value)
        {
            var dateTime = (DateTime)value;
            if (DateStyle == DateTimeKind.Utc)
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

            if (DateStyle == DateTimeKind.Local && dateTime.Kind != DateTimeKind.Local)
            {
                dateTime = dateTime.Kind == DateTimeKind.Utc
                    ? dateTime.ToLocalTime()
                    : DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
            }

            return dateTime;
        }
    }
}