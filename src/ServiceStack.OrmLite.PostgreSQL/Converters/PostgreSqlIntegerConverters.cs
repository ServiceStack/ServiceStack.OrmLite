using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.PostgreSQL.Converters
{
    //throws unknown type exceptions in parameterized queries, e.g: p.DbType = DbType.SByte
    public class PostrgreSqlSByteConverter : SByteConverter
    {
        public override DbType DbType
        {
            get { return DbType.Byte; }
        }
    }

    public class PostrgreSqlUInt16Converter : UInt16Converter
    {
        public override DbType DbType
        {
            get { return DbType.Int16; }
        }
    }

    public class PostrgreSqlUInt32Converter : UInt32Converter
    {
        public override DbType DbType
        {
            get { return DbType.Int32; }
        }
    }

    public class PostrgreSqlUInt64Converter : UInt64Converter
    {
        public override DbType DbType
        {
            get { return DbType.Int64; }
        }
    }
}