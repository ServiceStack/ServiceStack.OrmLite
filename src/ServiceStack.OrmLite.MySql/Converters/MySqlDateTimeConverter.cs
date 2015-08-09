using System;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.MySql.Converters
{
    public class MySqlDateTimeConverter : DateTimeConverter
    {
        public override string ToQuotedString(Type fieldType, object value)
        {
            /*
             * ms not contained in format. MySql ignores ms part anyway
             * for more details see: http://dev.mysql.com/doc/refman/5.1/en/datetime.html
             */
            var dateTime = (DateTime)value;
            return DateTimeFmt(dateTime, "yyyy-MM-dd HH:mm:ss");
        }
    }
}