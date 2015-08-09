using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.PostgreSQL.Converters
{
    public class PostgreSqlDecimalConverter : DecimalConverter
    {
        public PostgreSqlDecimalConverter() : base(38, 6) { }

        public override string GetColumnDefinition(int? precision, int? scale)
        {
            return "NUMERIC({0},{1})".Fmt(
                precision.GetValueOrDefault(Precision),
                scale.GetValueOrDefault(Scale));
        }
    }
}