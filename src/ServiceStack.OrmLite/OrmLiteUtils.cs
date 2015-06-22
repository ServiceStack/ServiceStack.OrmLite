//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2013 Service Stack LLC. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteUtils
    {
        internal const string AsyncRequiresNet45Error = "Async support is only available in .NET 4.5 builds";

        public static T CreateInstance<T>()
        {
            return (T)ReflectionExtensions.CreateInstance<T>();
        }

        public static bool IsScalar<T>()
        {
            return typeof(T).IsValueType || typeof(T) == typeof(string);
        }

        public static T ConvertTo<T>(this IDataReader dataReader, IOrmLiteDialectProvider dialectProvider)
        {
            var fieldDefs = ModelDefinition<T>.Definition.AllFieldDefinitionsArray;

            using (dataReader)
            {
                if (dataReader.Read())
                {
                    var row = CreateInstance<T>();
                    var indexCache = dataReader.GetIndexFieldsCache(ModelDefinition<T>.Definition);
                    row.PopulateWithSqlReader(dialectProvider, dataReader, fieldDefs, indexCache);
                    return row;
                }
                return default(T);
            }
        }

        public static List<T> ConvertToList<T>(this IDataReader dataReader, IOrmLiteDialectProvider dialectProvider)
        {
            var fieldDefs = ModelDefinition<T>.Definition.AllFieldDefinitionsArray;

            var to = new List<T>();
            using (dataReader)
            {
                var indexCache = dataReader.GetIndexFieldsCache(ModelDefinition<T>.Definition);
                while (dataReader.Read())
                {
                    var row = CreateInstance<T>();
                    row.PopulateWithSqlReader(dialectProvider, dataReader, fieldDefs, indexCache);
                    to.Add(row);
                }
            }
            return to;
        }

        public static object ConvertTo(this IDataReader dataReader, IOrmLiteDialectProvider dialectProvider, Type type)
        {
            var modelDef = type.GetModelDefinition();
            var fieldDefs = modelDef.AllFieldDefinitionsArray;

            using (dataReader)
            {
                if (dataReader.Read())
                {
                    var row = type.CreateInstance();
                    var indexCache = dataReader.GetIndexFieldsCache(modelDef);
                    row.PopulateWithSqlReader(dialectProvider, dataReader, fieldDefs, indexCache);
                    return row;
                }
                return type.GetDefaultValue();
            }
        }

        public static IList ConvertToList(this IDataReader dataReader, IOrmLiteDialectProvider dialectProvider, Type type)
        {
            var modelDef = type.GetModelDefinition();
            var fieldDefs = modelDef.AllFieldDefinitionsArray;

            var listInstance = typeof(List<>).MakeGenericType(type).CreateInstance();
            var to = (IList)listInstance;
            using (dataReader)
            {
                var indexCache = dataReader.GetIndexFieldsCache(modelDef);
                while (dataReader.Read())
                {
                    var row = type.CreateInstance();
                    row.PopulateWithSqlReader(dialectProvider, dataReader, fieldDefs, indexCache);
                    to.Add(row);
                }
            }
            return to;
        }

        internal static string GetColumnNames(this Type tableType, IOrmLiteDialectProvider dialect)
        {
            return GetColumnNames(tableType.GetModelDefinition(), dialect);
        }

        public static string GetColumnNames(this ModelDefinition modelDef, IOrmLiteDialectProvider dialect)
        {
            return dialect.GetColumnNames(modelDef);
        }

        internal static string GetIdsInSql(this IEnumerable idValues)
        {
            var sql = new StringBuilder();
            foreach (var idValue in idValues)
            {
                if (sql.Length > 0) sql.Append(",");
                sql.AppendFormat("{0}".SqlFmt(idValue));
            }
            return sql.Length == 0 ? null : sql.ToString();
        }

        public static string SqlFmt(this string sqlText, params object[] sqlParams)
        {
            return SqlFmt(sqlText, OrmLiteConfig.DialectProvider, sqlParams);
        }

        public static string SqlFmt(this string sqlText, IOrmLiteDialectProvider dialect, params object[] sqlParams)
        {
            var escapedParams = new List<string>();
            foreach (var sqlParam in sqlParams)
            {
                if (sqlParam == null)
                {
                    escapedParams.Add("NULL");
                }
                else
                {
                    var sqlInValues = sqlParam as SqlInValues;
                    if (sqlInValues != null)
                    {
                        escapedParams.Add(sqlInValues.ToSqlInString());
                    }
                    else
                    {
                        escapedParams.Add(dialect.GetQuotedValue(sqlParam, sqlParam.GetType()));
                    }
                }
            }
            return String.Format(sqlText, escapedParams.ToArray());
        }

        public static string SqlColumn(this string columnName, IOrmLiteDialectProvider dialect = null)
        {
            return (dialect ?? OrmLiteConfig.DialectProvider).GetQuotedColumnName(columnName);
        }

        public static string SqlColumnRaw(this string columnName, IOrmLiteDialectProvider dialect = null)
        {
            return (dialect ?? OrmLiteConfig.DialectProvider).NamingStrategy.GetColumnName(columnName);
        }

        public static string SqlTable(this string tableName, IOrmLiteDialectProvider dialect = null)
        {
            return (dialect ?? OrmLiteConfig.DialectProvider).GetQuotedTableName(tableName);
        }

        public static string SqlTableRaw(this string tableName, IOrmLiteDialectProvider dialect = null)
        {
            return (dialect ?? OrmLiteConfig.DialectProvider).NamingStrategy.GetTableName(tableName);
        }

        public static string SqlValue(this object value)
        {
            return "{0}".SqlFmt(value);
        }

        public static string[] IllegalSqlFragmentTokens = { 
            "--", ";--", ";", "%", "/*", "*/", "@@", "@", 
            "char", "nchar", "varchar", "nvarchar",
            "alter", "begin", "cast", "create", "cursor", "declare", "delete",
            "drop", "end", "exec", "execute", "fetch", "insert", "kill",
            "open", "select", "sys", "sysobjects", "syscolumns", "table", "update" };

        public static string SqlVerifyFragment(this string sqlFragment)
        {
            return SqlVerifyFragment(sqlFragment, IllegalSqlFragmentTokens);
        }

        public static string SqlVerifyFragment(this string sqlFragment, IEnumerable<string> illegalFragments)
        {
            var fragmentToVerify = sqlFragment
                .StripQuotedStrings('\'')
                .StripQuotedStrings('"')
                .StripQuotedStrings('`')
                .ToLower();

            foreach (var illegalFragment in illegalFragments)
            {
                if ((fragmentToVerify.IndexOf(illegalFragment, StringComparison.Ordinal) >= 0))
                    throw new ArgumentException("Potential illegal fragment detected: " + sqlFragment);
            }

            return sqlFragment;
        }

        public static string SqlParam(this string paramValue)
        {
            return paramValue.Replace("'", "''");
        }

        public static string StripQuotedStrings(this string text, char quote = '\'')
        {
            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == quote)
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (!inQuotes)
                    sb.Append(c);
            }

            return sb.ToString();
        }

        public static string SqlJoin<T>(this List<T> values, IOrmLiteDialectProvider dialect = null)
        {
            dialect = (dialect ?? OrmLiteConfig.DialectProvider);

            var sb = new StringBuilder();
            foreach (var value in values)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(dialect.GetQuotedValue(value, value.GetType()));
            }

            return sb.ToString();
        }

        public static string SqlJoin(IEnumerable values, IOrmLiteDialectProvider dialect = null)
        {
            dialect = (dialect ?? OrmLiteConfig.DialectProvider);

            var sb = new StringBuilder();
            foreach (var value in values)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(dialect.GetQuotedValue(value, value.GetType()));
            }

            return sb.ToString();
        }

        public static SqlInValues SqlInValues<T>(this T[] values, IOrmLiteDialectProvider dialect = null)
        {
            return new SqlInValues(values, dialect);
        }

        public static Dictionary<string, int> GetIndexFieldsCache(this IDataReader reader, ModelDefinition modelDefinition = null)
        {
            var cache = new Dictionary<string, int>();
            if (modelDefinition != null)
            {
                foreach (var field in modelDefinition.IgnoredFieldDefinitions)
                {
                    cache[field.FieldName] = -1;
                }
            }
            for (var i = 0; i < reader.FieldCount; i++)
            {
                cache[reader.GetName(i)] = i;
            }
            return cache;
        }

        public static bool IsRefType(this Type fieldType)
        {
            return (!fieldType.UnderlyingSystemType.IsValueType
                || JsConfig.TreatValueAsRefTypes.Contains(fieldType.IsGeneric()
                    ? fieldType.GenericTypeDefinition()
                    : fieldType))
                && fieldType != typeof(string);
        }

        public static string StripTablePrefixes(this string selectExpression)
        {
            if (selectExpression.IndexOf('.') < 0)
                return selectExpression;

            var sb = new StringBuilder();
            var tokens = selectExpression.Split(' ');
            foreach (var token in tokens)
            {
                var parts = token.SplitOnLast('.');
                if (parts.Length > 1)
                {
                    sb.Append(" " + parts[parts.Length - 1]);
                }
                else
                {
                    sb.Append(" " + token);
                }
            }

            return sb.ToString().Trim();
        }

        public static char[] QuotedChars = new[] { '"', '`', '[', ']' };

        public static string StripQuotes(this string quotedExpr)
        {
            return quotedExpr.Trim(QuotedChars);
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

        public static List<Parent> Merge<Parent, Child>(this Parent parent, List<Child> children)
        {
            return new List<Parent> { parent }.Merge(children);
        }

        public static List<Parent> Merge<Parent, Child>(this List<Parent> parents, List<Child> children)
        {
            var modelDef = ModelDefinition<Parent>.Definition;

            var hasChildRef = false;

            foreach (var fieldDef in modelDef.AllFieldDefinitionsArray)
            {
                if ((fieldDef.FieldType != typeof (Child) && fieldDef.FieldType != typeof (List<Child>)) || !fieldDef.IsReference) 
                    continue;
                
                hasChildRef = true;

                var listInterface = fieldDef.FieldType.GetTypeWithGenericInterfaceOf(typeof(IList<>));
                if (listInterface != null)
                {
                    var refType = listInterface.GenericTypeArguments()[0];
                    var refModelDef = refType.GetModelDefinition();
                    var refField = modelDef.GetRefFieldDef(refModelDef, refType);

                    SetListChildResults(parents, modelDef, fieldDef, refType, children, refField);
                }
                else
                {
                    var refType = fieldDef.FieldType;

                    var refModelDef = refType.GetModelDefinition();

                    var refSelf = modelDef.GetSelfRefFieldDefIfExists(refModelDef, fieldDef);
                    var refField = refSelf == null
                        ? modelDef.GetRefFieldDef(refModelDef, refType)
                        : modelDef.GetRefFieldDefIfExists(refModelDef);

                    if (refSelf != null)
                    {
                        SetRefSelfChildResults(parents, fieldDef, refModelDef, refSelf, children);
                    }
                    else if (refField != null)
                    {
                        SetRefFieldChildResults(parents, modelDef, fieldDef, refField, children);
                    }
                }
            }

            if (!hasChildRef)
                throw new Exception("Could not find Child Reference for '{0}' on Parent '{1}'".Fmt(typeof(Child).Name, typeof(Parent).Name));

            return parents;
        }

        internal static void SetListChildResults<Parent>(List<Parent> parents, ModelDefinition modelDef,
            FieldDefinition fieldDef, Type refType, IList childResults, FieldDefinition refField)
        {
            var map = new Dictionary<object, List<object>>();
            List<object> refValues;

            foreach (var result in childResults)
            {
                var refValue = refField.GetValue(result);
                if (!map.TryGetValue(refValue, out refValues))
                {
                    map[refValue] = refValues = new List<object>();
                }
                refValues.Add(result);
            }

            var untypedApi = refType.CreateTypedApi();
            foreach (var result in parents)
            {
                var pkValue = modelDef.PrimaryKey.GetValue(result);
                if (map.TryGetValue(pkValue, out refValues))
                {
                    var castResults = untypedApi.Cast(refValues);
                    fieldDef.SetValueFn(result, castResults);
                }
            }
        }

        internal static void SetRefSelfChildResults<Parent>(List<Parent> parents, FieldDefinition fieldDef, ModelDefinition refModelDef, FieldDefinition refSelf, IList childResults)
        {
            var map = new Dictionary<object, object>();
            foreach (var result in childResults)
            {
                var pkValue = refModelDef.PrimaryKey.GetValue(result);
                map[pkValue] = result;
            }

            foreach (var result in parents)
            {
                object childResult;
                var fkValue = refSelf.GetValue(result);
                if (fkValue != null && map.TryGetValue(fkValue, out childResult))
                {
                    fieldDef.SetValueFn(result, childResult);
                }
            }
        }

        internal static void SetRefFieldChildResults<Parent>(List<Parent> parents, ModelDefinition modelDef,
            FieldDefinition fieldDef, FieldDefinition refField, IList childResults)
        {
            var map = new Dictionary<object, object>();

            foreach (var result in childResults)
            {
                var refValue = refField.GetValue(result);
                map[refValue] = result;
            }

            foreach (var result in parents)
            {
                object childResult;
                var pkValue = modelDef.PrimaryKey.GetValue(result);
                if (map.TryGetValue(pkValue, out childResult))
                {
                    fieldDef.SetValueFn(result, childResult);
                }
            }
        }

    }
}