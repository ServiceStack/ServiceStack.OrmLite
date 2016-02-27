﻿using System;
using System.Data;
using System.Globalization;
using ServiceStack.Logging;
using ServiceStack.Text.Common;

namespace ServiceStack.OrmLite.Converters
{
    public class DateTimeConverter : OrmLiteConverter
    {
        protected static ILog Log = LogManager.GetLogger(typeof(DateTimeConverter));

        public override string ColumnDefinition
        {
            get { return "DATETIME"; }
        }

        public override DbType DbType
        {
            get { return DbType.DateTime; }
        }

        public DateTimeKind DateStyle { get; set; }

        public override string ToQuotedString(Type fieldType, object value)
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

        public override object ToDbValue(Type fieldType, object value)
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

        public override object FromDbValue(Type fieldType, object value)
        {
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