using System;
using Microsoft.SqlServer.Types;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    /// <summary>
    /// SqlServer Database Converter for the Geometry data type
    /// https://msdn.microsoft.com/en-us/library/microsoft.sqlserver.types.sqlgeography.aspx
    /// </summary>
    public class SqlServerGeographyTypeConverter : SqlServerSpatialTypeConverter
    {
        public override string ColumnDefinition
        {
            get { return "GEOGRAPHY"; }
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            var geo = value as SqlGeography;
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
                return SqlGeography.Null;
            }

            if (value is SqlGeography)
            {
                return value;
            }

            if (value is string)
            {
                var str = value as string;
                return SqlGeography.Parse(str);
            }

            if (value is byte[])
            {
                var bin = value as byte[];
                var sqlBin = new System.Data.SqlTypes.SqlBytes(bin);

                return SqlGeography.Deserialize(sqlBin);
            }

            return base.ToDbValue(fieldType, value);
        }
    }
}
