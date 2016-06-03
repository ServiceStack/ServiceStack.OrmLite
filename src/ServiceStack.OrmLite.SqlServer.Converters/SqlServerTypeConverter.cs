using System;
using System.Data;
using System.Data.SqlClient;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    public abstract class SqlServerSpatialTypeConverter : OrmLiteConverter
    {
        public override DbType DbType
        {
            get { return DbType.Object; }
        }

        public override void InitDbParam(IDbDataParameter p, Type fieldType)
        {
            var sqlParam = (SqlParameter)p;
            sqlParam.SqlDbType = SqlDbType.Udt;
            sqlParam.IsNullable = fieldType.IsNullableType();
            sqlParam.UdtTypeName = ColumnDefinition;
        }
    }
}
