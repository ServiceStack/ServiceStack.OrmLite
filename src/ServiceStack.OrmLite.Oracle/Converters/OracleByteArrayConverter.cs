using System;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.Oracle.Converters
{
    public class OracleByteArrayConverter : ByteArrayConverter
    {
        public override object ToDbParamValue(Type fieldType, object value)
        {
            return "hextoraw('" + BitConverter.ToString((byte[])value).Replace("-", "") + "')";
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            return "hextoraw('" + BitConverter.ToString((byte[])value).Replace("-", "") + "')";
        }
    }
}