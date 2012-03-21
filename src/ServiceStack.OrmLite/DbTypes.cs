using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite
{
	public static class DbTypes<TDialect>
		where TDialect : IOrmLiteDialectProvider
	{
		public static Dictionary<Type, string> ColumnTypeMap = new Dictionary<Type, string>();
		public static Dictionary<Type, DbType> ColumnDbTypeMap = new Dictionary<Type, DbType>();
	}

	public static class DbTypes<TDialect, T>
		where TDialect : IOrmLiteDialectProvider
	{
        public static DbType DbType;
        public static string TextDefinition;
        public static bool ShouldQuoteValue;

        public static void Set(DbType dbType, string fieldDefinition)
        {
            DbType = dbType;
            TextDefinition = fieldDefinition;
            ShouldQuoteValue = fieldDefinition != "INTEGER"
                && fieldDefinition != "BIGINT"
                && fieldDefinition != "DOUBLE"
                && fieldDefinition != "DECIMAL"
                && fieldDefinition != "BOOL";

			DbTypes<TDialect>.ColumnTypeMap[typeof(T)] = fieldDefinition;
			DbTypes<TDialect>.ColumnDbTypeMap[typeof(T)] = dbType;
        }
    }
}