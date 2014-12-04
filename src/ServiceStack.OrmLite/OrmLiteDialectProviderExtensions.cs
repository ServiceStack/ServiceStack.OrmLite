using System;
using System.Data;

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

        private const int NotFound = -1;

        public static ModelDefinition GetModelDefinition(Type modelType)
        {
            return modelType.GetModelDefinition();
        }

        public static bool HandledDbNullValue(FieldDefinition fieldDef, IDataReader dataReader, int colIndex, object instance)
        {
            if (fieldDef == null || fieldDef.SetValueFn == null || colIndex == NotFound) return true;
            if (dataReader.IsDBNull(colIndex))
            {
                if (fieldDef.IsNullable)
                {
                    fieldDef.SetValueFn(instance, null);
                }
                else
                {
                    fieldDef.SetValueFn(instance, fieldDef.FieldType.GetDefaultValue());
                }
                return true;
            }
            return false;
        }

        public static ulong ConvertToULong(byte[] bytes)
        {
            Array.Reverse(bytes); //Correct Endianness
            var ulongValue = BitConverter.ToUInt64(bytes, 0);
            return ulongValue;
        }
    }
}