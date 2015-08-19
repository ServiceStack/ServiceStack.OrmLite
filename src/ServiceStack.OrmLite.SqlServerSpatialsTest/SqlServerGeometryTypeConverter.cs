using System;
using System.Data;
using ServiceStack.OrmLite;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    public class SqlServerGeometryTypeConverter : OrmLiteConverter
    {
        public override string ColumnDefinition
        {
            get { return "GEOMETRY"; }
        }

        public override DbType DbType
        {
            get { return DbType.Object; }
        }
    }
}
