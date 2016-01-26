using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.MySql.Converters
{
    public abstract class MySqlIntegerConverter : IntegerConverter
    {
        public override string ColumnDefinition
        {
            get { return "INT(11)"; }
        }
    }

    public class MySqlByteConverter : MySqlIntegerConverter
    {
        public override DbType DbType
        {
            get { return DbType.Byte; }
        }
    }

    public class MySqlSByteConverter : MySqlIntegerConverter
    {
        public override DbType DbType
        {
            get { return DbType.SByte; }
        }
    }

    public class MySqlInt16Converter : MySqlIntegerConverter
    {
        public override DbType DbType
        {
            get { return DbType.Int16; }
        }
    }

    public class MySqlUInt16Converter : MySqlIntegerConverter
    {
        public override DbType DbType
        {
            get { return DbType.UInt16; }
        }
    }

    public class MySqlInt32Converter : MySqlIntegerConverter
    {
    }

    public class MySqlUInt32Converter : MySqlIntegerConverter
    {
        public override DbType DbType
        {
            get { return DbType.UInt32; }
        }
    }
}