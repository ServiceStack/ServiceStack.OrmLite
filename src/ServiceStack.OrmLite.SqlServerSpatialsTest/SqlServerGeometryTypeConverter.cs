using System;
using System.Data;
using System.Data.SqlClient;
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

        public override void InitDbParam(IDbDataParameter p, Type fieldType)
        {
            var geogParam = (SqlParameter)p;
            geogParam.SqlDbType = SqlDbType.Udt;
            geogParam.UdtTypeName = ColumnDefinition;
        }
    }
}
