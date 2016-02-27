using System.Data;

namespace ServiceStack.OrmLite.Converters
{
    public class ByteArrayConverter : OrmLiteConverter
    {
        public override string ColumnDefinition
        {
            get { return "BLOB"; }
        }

        public override DbType DbType
        {
            get { return DbType.Binary; }
        }
    }
}