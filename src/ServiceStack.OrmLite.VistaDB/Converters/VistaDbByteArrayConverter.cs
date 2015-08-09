using System;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.VistaDB.Converters
{
    public class VistaDbByteArrayConverter : ByteArrayConverter
    {
        public override string ColumnDefinition
        {
            get { return "VARBINARY(MAX)"; }
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            return "0x" + BitConverter.ToString((byte[])value).Replace("-", "");
        }
    }
}