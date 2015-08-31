using System;
using Microsoft.SqlServer.Types;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    /// <summary>
    /// SqlServer Database Converter for the Geometry data type
    /// https://msdn.microsoft.com/en-us/library/microsoft.sqlserver.types.sqlgeometry.aspx
    /// </summary>
    public class SqlServerGeometryTypeConverter : SqlServerSpatialTypeConverter
    {
        public override string ColumnDefinition
        {
            get { return "GEOMETRY"; }
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            var geo = value as SqlGeometry;
            if (geo == null || geo.IsNull)
            {
                return null;
            }

            return base.FromDbValue(fieldType, value);
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            if (value == null)
            {
                return SqlGeometry.Null;
            }

            if (value is SqlGeometry)
            {
                return value;
            }

            if (value is string)
            {
                var str = value as string;
                return SqlGeometry.Parse(str);
            }

            if (value is byte[])
            {
                var bin = value as byte[];
                var sqlBin = new System.Data.SqlTypes.SqlBytes(bin);

                return SqlGeometry.Deserialize(sqlBin);
            }

            return base.ToDbValue(fieldType, value);
        }
    }
}
