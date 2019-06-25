using System.Data;
using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.MySql.Converters
{
    public class MySqlStringConverter : StringConverter
    {
        public MySqlStringConverter() : base(255) {}
        
        //https://stackoverflow.com/a/37721151/85785
        public override int MaxVarCharLength => UseUnicode ? 16383 : 21844;

        public override string MaxColumnDefinition => "LONGTEXT";
    }

    public class MySqlCharArrayConverter : CharArrayConverter
    {
        public MySqlCharArrayConverter() : base(255) { }

        public override string MaxColumnDefinition => "LONGTEXT";
    }
}