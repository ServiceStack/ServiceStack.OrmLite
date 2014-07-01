namespace ServiceStack.OrmLite
{
    public static class OrmLiteDialectProviderExtensions
    {
        public static string GetParam(this IOrmLiteDialectProvider dialect, string name)
        {
            return dialect.ParamString + name;
        }

        public static string GetParam(this IOrmLiteDialectProvider dialect, int indexNo = 0)
        {
            return dialect.ParamString + indexNo;
        }

        public static string ToFieldName(this IOrmLiteDialectProvider dialect, string paramName)
        {
            return paramName.Substring(dialect.ParamString.Length);
        }

        public static string FmtTable(this string tableName)
        {
            return OrmLiteConfig.DialectProvider.NamingStrategy.GetTableName(tableName);
        }

        public static string FmtColumn(this string columnName)
        {
            return OrmLiteConfig.DialectProvider.NamingStrategy.GetColumnName(columnName);
        }

        public static string GetQuotedColumnName(this IOrmLiteDialectProvider dialect, 
            FieldDefinition fieldDef)
        {
            return dialect.GetQuotedColumnName(fieldDef.FieldName);
        }

        public static string GetQuotedColumnName(this IOrmLiteDialectProvider dialect,
            ModelDefinition modelDef, FieldDefinition fieldDef)
        {
            return dialect.GetQuotedTableName(modelDef.ModelName) +
                "." +
                dialect.GetQuotedColumnName(fieldDef.FieldName);
        }

        public static string GetQuotedColumnName(this IOrmLiteDialectProvider dialect,
            string tableName, string fieldName)
        {
            return dialect.GetQuotedTableName(tableName) +
                "." +
                dialect.GetQuotedColumnName(fieldName);
        }
    }
}