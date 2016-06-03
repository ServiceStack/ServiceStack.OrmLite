using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;

namespace ServiceStack.OrmLite.SqlServer.Converters
{
    /// <summary>
    /// SqlServer Database Converter for the HierarchyId data type
    /// https://msdn.microsoft.com/en-us/library/microsoft.sqlserver.types.sqlhierarchyid.aspx
    /// </summary>
    public class SqlServerHierarchyIdTypeConverter : OrmLiteConverter
    {
        public SqlServerHierarchyIdTypeConverter() : base()
        { }

        public override string ColumnDefinition
        {
            get { return "hierarchyid"; }
        }

        public override DbType DbType
        {
            get { return DbType.Object; }
        }

        public override void InitDbParam(IDbDataParameter p, Type fieldType)
        {
            var sqlParam = (SqlParameter)p;
            sqlParam.IsNullable = (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>));
            sqlParam.SqlDbType = SqlDbType.Udt;
            sqlParam.UdtTypeName = ColumnDefinition;
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            if (((SqlHierarchyId)value).IsNull && fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return null;
            }

            return base.FromDbValue(fieldType, value);
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            if (value is SqlHierarchyId)
            {
                return value;
            }

            if (value == null)
            {
                return SqlHierarchyId.Null;
            }

            if (value is string)
            {
                var str = value as string;
                return SqlHierarchyId.Parse(str);
            }

            return base.ToDbValue(fieldType, value);
        }
    }
}
