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

        public static string FmtTable(this string tableName, IOrmLiteDialectProvider dialect = null)
        {
            return (dialect ?? OrmLiteConfig.DialectProvider).NamingStrategy.GetTableName(tableName);
        }

        public static string FmtColumn(this string columnName, IOrmLiteDialectProvider dialect=null)
        {
            return (dialect ?? OrmLiteConfig.DialectProvider).NamingStrategy.GetColumnName(columnName);
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
            ModelDefinition tableDef, string fieldName)
        {
            return dialect.GetQuotedTableName(tableDef) +
                "." +
                dialect.GetQuotedColumnName(fieldName);
        }
    }
}