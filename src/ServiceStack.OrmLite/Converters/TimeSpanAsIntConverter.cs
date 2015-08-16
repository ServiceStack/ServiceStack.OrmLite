using System;
using System.Data;
using System.Globalization;

namespace ServiceStack.OrmLite.Converters
{
    public class TimeSpanAsIntConverter : OrmLiteConverter
    {
        public override string ColumnDefinition
        {
            get { return "BIGINT"; }
        }

        public override DbType DbType
        {
            get { return DbType.Int64; }
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            return ((TimeSpan)value).Ticks.ToString(CultureInfo.InvariantCulture);
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            var timespan = (TimeSpan)value;
            return timespan.Ticks;
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            var ticks = (long)this.ConvertNumber(typeof(long), value);
            return TimeSpan.FromTicks(ticks);
        }
    }
}