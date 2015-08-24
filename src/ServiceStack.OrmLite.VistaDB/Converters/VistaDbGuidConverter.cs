using System;
using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.VistaDB.Converters
{
    public class VistaDbGuidConverter : GuidConverter
    {
        public override string ColumnDefinition
        {
            get { return "UniqueIdentifier"; }
        }

        public override string ToQuotedString(Type fieldType, object value)
        {
            var guidValue = (Guid)value;
            return string.Format("CAST('{0}' AS UNIQUEIDENTIFIER)", guidValue);
        }
    }
}