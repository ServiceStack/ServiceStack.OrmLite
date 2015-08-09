using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.Sqlite.Converters
{
    public class SqliteStringConverter : StringConverter
    {
        public override string MaxColumnDefinition
        {
            get { return UseUnicode ? "NVARCHAR(1000000)" : "VARCHAR(1000000)"; }
        }
    }
}