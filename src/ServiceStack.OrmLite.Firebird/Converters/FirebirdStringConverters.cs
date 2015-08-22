using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.Firebird.Converters
{
    public class FirebirdStringConverter : StringConverter
    {
        public FirebirdStringConverter() : base(128) {}

        public override string MaxColumnDefinition
        {
            get { return maxColumnDefinition ?? GetColumnDefinition(32767); }
        }

        public override string GetColumnDefinition(int? stringLength)
        {
            if (stringLength.GetValueOrDefault() == StringLengthAttribute.MaxText)
                return MaxColumnDefinition;

            return "VARCHAR({0})".Fmt(stringLength.GetValueOrDefault(StringLength));
        }
    }

    public class FirebirdCharArrayConverter : CharArrayConverter
    {
        public override string MaxColumnDefinition
        {
            get { return DialectProvider.GetStringConverter().MaxColumnDefinition; }
        }

        public override string GetColumnDefinition(int? stringLength)
        {
            return MaxColumnDefinition;
        }
    }

}