using ServiceStack.OrmLite.Converters;

namespace ServiceStack.OrmLite.VistaDB.Converters
{
    public class VistaDbStringConverter : StringConverter
    {
        public override string MaxColumnDefinition
        {
            get { return UseUnicode ? "NVARCHAR(MAX)" : "VARCHAR(MAX)"; }
        }
    }
}