using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.Oracle.Converters
{
    public class OracleBoolConverter : BoolAsIntConverter
    {
        public override string ColumnDefinition
        {
            get { return "NUMBER(1)"; }
        }
    }
}