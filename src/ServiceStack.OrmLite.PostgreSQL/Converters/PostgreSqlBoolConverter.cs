using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.PostgreSQL.Converters
{
    public class PostgreSqlBoolConverter : BoolConverter
    {
        public override string ColumnDefinition
        {
            get { return "BOOLEAN"; }
        }

        public override object FromDbValue(FieldDefinition fieldDef, object value)
        {
            return base.FromDbValue(fieldDef, value);
        }
    }
}