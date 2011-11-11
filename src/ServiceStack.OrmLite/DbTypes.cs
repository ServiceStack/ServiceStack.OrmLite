using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite
{
    public static class DbTypes
    {
        public static Dictionary<Type, string> ColumnTypeMap = new Dictionary<Type, string>();
        public static Dictionary<Type, DbType> ColumnDbTypeMap = new Dictionary<Type, DbType>();
    }

    public static class DbTypes<T>
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

            DbTypes.ColumnTypeMap[typeof(T)] = fieldDefinition;
            DbTypes.ColumnDbTypeMap[typeof(T)] = dbType;
        }
    }
}