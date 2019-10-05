using System;
using System.Data;
using System.Linq;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.Sqlite.Converters
{
    public class SqliteGuidConverter : GuidConverter
    {
        public override string ColumnDefinition => "CHAR(36)";

        public override string ToQuotedString(Type fieldType, object value)
        {
            var guid = (Guid)value;
            var bytes = guid.ToByteArray();
            var fmt = "x'" + BitConverter.ToString(bytes).Replace("-", "") + "'";
            return fmt;
        }

        public override object GetValue(IDataReader reader, int columnIndex, object[] values)
        {
            if (values != null)
            {
                if (values[columnIndex] == DBNull.Value)
                    return null;
            }
            else
            {
                if (reader.IsDBNull(columnIndex))
                    return null;
            }

            return reader.GetGuid(columnIndex);
        }
    }
}