using System;
using System.Data;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    public class SqlServerTimeConverter : OrmLiteConverter
    {
        private static readonly DateTime timeSpanOffset = new DateTime(1900, 01, 01);

        public override string ColumnDefinition
        {
            get { return "TIME"; }
        }

        public override DbType DbType
        {
            get { return DbType.DateTime; }
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            var timeSpan = (TimeSpan)value;
            return timeSpanOffset + timeSpan;
        }
    }
}