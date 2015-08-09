using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.Oracle.Converters
{
    public class OracleTimeSpanConverter : TimeSpanConverter
    {
        public override string ColumnDefinition
        {
            get { return "NUMERIC(18)"; }
        }
    }
}