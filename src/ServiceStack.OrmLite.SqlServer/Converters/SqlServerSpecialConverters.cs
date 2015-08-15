using System;
using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    public class SqlServerRowVersionConverter : RowVersionConverter
    {
        public override string ColumnDefinition
        {
            get { return "rowversion"; }
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            var bytes = value as byte[];
            if (bytes != null)
            {
                var ulongValue = OrmLiteUtils.ConvertToULong(bytes);
                return ulongValue;
            }
            return null;
        }

        public override ulong FromDbRowVersion(object value)
        {
            var bytes = value as byte[];
            var ulongValue = OrmLiteUtils.ConvertToULong(bytes);
            return ulongValue;
        }
    }
}