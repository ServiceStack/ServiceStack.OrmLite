using System;
using System.Data;
using ServiceStack.OrmLite;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    public class SqlServerGeographyTypeConverter : OrmLiteConverter
    {
        public override string ColumnDefinition
        {
            get { return "GEOGRAPHY"; }
        }

        public override DbType DbType
        {
            get { return DbType.Object; }
        }
    }
}
