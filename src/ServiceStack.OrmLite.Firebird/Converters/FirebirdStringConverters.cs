using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.Firebird.Converters
{
    public class FirebirdStringConverter : StringConverter
    {
        public FirebirdStringConverter() : base(128) {}

        public override string MaxColumnDefinition
        {
            get
            {
                //Max field is 32767 but using lower limit to avoid 64kb max row limit
                return maxColumnDefinition ?? GetColumnDefinition(10000); 
            } 
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