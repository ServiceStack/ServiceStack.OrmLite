using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.VistaDB.Converters
{
    public abstract class VistaDbIntegerConverter : IntegerConverter
    {
        public override string ColumnDefinition
        {
            get { return "INT"; }
        }
    }

    public class VistaDbByteConverter : VistaDbIntegerConverter
    {
        public override DbType DbType
        {
            get { return DbType.Byte; }
        }
    }

    public class VistaDbSByteConverter : VistaDbIntegerConverter
    {
        public override DbType DbType
        {
            get { return DbType.SByte; }
        }
    }

    public class VistaDbInt16Converter : VistaDbIntegerConverter
    {
        public override DbType DbType
        {
            get { return DbType.Int16; }
        }
    }

    public class VistaDbUInt16Converter : VistaDbIntegerConverter
    {
        public override DbType DbType
        {
            get { return DbType.UInt16; }
        }
    }

    public class VistaDbInt32Converter : VistaDbIntegerConverter
    {
    }

    public class VistaDbUInt32Converter : VistaDbIntegerConverter
    {
        public override DbType DbType
        {
            get { return DbType.UInt32; }
        }
    }
}