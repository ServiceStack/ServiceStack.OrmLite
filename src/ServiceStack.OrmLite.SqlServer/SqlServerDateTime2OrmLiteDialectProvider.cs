using System;
using System.Data;

namespace ServiceStack.OrmLite.SqlServer
{
    /// <summary>SqlServer OrmLiteDialectProvider that supports DbType.DateTime2 by default</summary>
    public class SqlServerDateTime2OrmLiteDialectProvider : SqlServerOrmLiteDialectProvider
    {
        public new static SqlServerDateTime2OrmLiteDialectProvider Instance = new SqlServerDateTime2OrmLiteDialectProvider();

        /// <summary>Column definition for a DateTime2.  Defaults to maximum precision: DATETIME2(7)</summary>
        public string DateTime2ColumnDefinition = "DATETIME2(7)";

        SqlServerDateTime2OrmLiteDialectProvider()
        {
            DbTypes<SqlServerOrmLiteDialectProvider, DateTime>.Set(DbType.DateTime2, DateTime2ColumnDefinition);
            DbTypes<SqlServerOrmLiteDialectProvider, DateTime?>.Set(DbType.DateTime2, DateTime2ColumnDefinition);
        }

        public override string GetQuotedValue(object value, Type fieldType)
        {
            if (fieldType == typeof(DateTime))
            {
                const string dateTime2Format = "yyyyMMdd HH:mm:ss.fffffff";
                var dateValue = (DateTime)value;
                return base.GetQuotedValue(dateValue.ToString(dateTime2Format), typeof(string));
            }
            return base.GetQuotedValue(value, fieldType);
        }
    }
}
