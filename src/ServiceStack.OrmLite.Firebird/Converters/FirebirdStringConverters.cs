using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.Firebird.Converters
{
    public class FirebirdStringConverter : StringConverter
    {
        public override string ColumnDefinition
        {
            get { return GetColumnDefinition(128); }
        }

        public override string MaxColumnDefinition
        {
            get { return GetColumnDefinition(32767); }
        }

        public override string GetColumnDefinition(int? stringLength)
        {
            return "VARCHAR({0})".Fmt(stringLength);
        }
    }
}