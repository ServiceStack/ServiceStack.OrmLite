using System;
using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.VistaDB.Converters
{
    public class VistaDbBoolConverter : BoolConverter
    {
        public override string ColumnDefinition
        {
            get { return "BIT"; }
        }

        public override DbType DbType
        {
            get { return DbType.Boolean; }
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            var boolValue = (bool)value;
            return base.DialectProvider.GetQuotedValue(boolValue ? 1 : 0, typeof(int));
        }
    }
}