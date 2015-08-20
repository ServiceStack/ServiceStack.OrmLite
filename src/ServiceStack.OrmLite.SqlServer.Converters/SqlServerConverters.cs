using Microsoft.SqlServer.Types;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    public static class SqlServerConverters
    {
        public static IOrmLiteDialectProvider Configure(IOrmLiteDialectProvider dialectProvider)
        {
            dialectProvider.RegisterConverter<SqlGeography>(new SqlServerGeographyTypeConverter());
            dialectProvider.RegisterConverter<SqlGeometry>(new SqlServerGeometryTypeConverter());
            dialectProvider.RegisterConverter<SqlHierarchyId>(new SqlServerHierarchyIdTypeConverter());
            return dialectProvider;
        }
    }
}