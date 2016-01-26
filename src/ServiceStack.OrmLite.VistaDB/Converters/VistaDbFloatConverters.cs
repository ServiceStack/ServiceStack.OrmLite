using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.VistaDB.Converters
{
    public class VistaDbFloatConverter : FloatConverter
    {
        public override string ColumnDefinition
        {
            get { return "FLOAT"; }
        }
    }

    public class VistaDbDoubleConverter : DoubleConverter
    {
        public override string ColumnDefinition
        {
            get { return "FLOAT"; }
        }
    }

    public class VistaDbDecimalConverter : DecimalConverter
    {
    }
}