using System;
using System.Data;

namespace ServiceStack.OrmLite.Converters
{
    public class GuidConverter : OrmLiteConverter
    {
        public override string ColumnDefinition
        {
            get { return "GUID"; }
        }

        public override DbType DbType
        {
            get { return DbType.Guid; }
        }

        public override object GetValue(IDataReader reader, int columnIndex)
        {
            return reader.GetGuid(columnIndex);
        }
    }
}