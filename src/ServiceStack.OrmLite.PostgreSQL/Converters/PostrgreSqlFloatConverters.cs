using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.PostgreSQL.Converters
{
    public class PostrgreSqlFloatConverter : FloatConverter
    {
        public override string ColumnDefinition
        {
            get { return "DOUBLE PRECISION"; }
        }
    }

    public class PostrgreSqlDoubleConverter : DoubleConverter
    {
        public override string ColumnDefinition
        {
            get { return "DOUBLE PRECISION"; }
        }
    }

    public class PostrgreSqlDecimalConverter : DecimalConverter
    {
        public PostrgreSqlDecimalConverter() 
            : base(38, 6) {}

        public override string GetColumnDefinition(int? precision, int? scale)
        {
            return "NUMERIC({0},{1})".Fmt(
                precision.GetValueOrDefault(Precision),
                scale.GetValueOrDefault(Scale));
        }
    }
}