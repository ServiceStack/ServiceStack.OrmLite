using System;
using System.Data;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    public class SqlServerDateTime2Converter : SqlServerDateTimeConverter
    {
        public override string ColumnDefinition
        {
            get { return "DATETIME2"; }
        }

        public override DbType DbType
        {
            get { return DbType.DateTime2; }
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            return DateTimeFmt((DateTime)value, "yyyyMMdd HH:mm:ss.fffffff");
        }
    }
}