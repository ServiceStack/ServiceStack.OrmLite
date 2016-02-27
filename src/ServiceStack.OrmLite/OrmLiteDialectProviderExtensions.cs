using System;
using System.Data;
using ServiceStack.OrmLite.Converters;

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
            ModelDefinition tableDef, FieldDefinition fieldDef)
        {
            return dialect.GetQuotedTableName(tableDef) +
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

        public static object FromDbValue(this IOrmLiteDialectProvider dialect, 
            IDataReader reader, int columnIndex, Type type)
        {
            return dialect.FromDbValue(dialect.GetValue(reader, columnIndex, type), type);
        }

        public static IOrmLiteConverter GetConverter<T>(this IOrmLiteDialectProvider dialect)
        {
            return dialect.GetConverter(typeof(T));
        }

        public static bool HasConverter(this IOrmLiteDialectProvider dialect, Type type)
        {
            return dialect.GetConverter(type) != null;
        }

        public static StringConverter GetStringConverter(this IOrmLiteDialectProvider dialect)
        {
            return (StringConverter)dialect.GetConverter(typeof(string));
        }

        public static DecimalConverter GetDecimalConverter(this IOrmLiteDialectProvider dialect)
        {
            return (DecimalConverter)dialect.GetConverter(typeof(decimal));
        }

        public static DateTimeConverter GetDateTimeConverter(this IOrmLiteDialectProvider dialect)
        {
            return (DateTimeConverter)dialect.GetConverter(typeof(DateTime));
        }
    }
}