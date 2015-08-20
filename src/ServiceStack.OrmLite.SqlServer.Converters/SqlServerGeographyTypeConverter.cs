using System;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    public class SqlServerGeographyTypeConverter : SqlServerTypeConverter
    {
        public SqlServerGeographyTypeConverter(string libraryPath = null, string msvcrFileName = "msvcr100.dll", string sqlSpatialFileName = "SqlServerSpatial110.dll") 
            : base(libraryPath, msvcrFileName, sqlSpatialFileName) 
        { 
        }

        public override string ColumnDefinition
        {
            get { return "GEOGRAPHY"; }
        }
    }
}
