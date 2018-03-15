using System;
using ServiceStack.OrmLite.Converters;
using MySql.Data.Types;

namespace ServiceStack.OrmLite.MySql.Converters
{
    public class MySqlDateTimeConverter : MySqlDateTimeConverterBase
    {
        public override object FromDbValue(object value)
        {
            if (value is MySqlDateTime)
            {
                var time = (MySqlDateTime)value;
                return time.GetDateTime();
            }
            return base.FromDbValue(value);
        }
    }
}