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
using System.Text.RegularExpressions;
using ServiceStack.Logging;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteUtils
    {
        internal const string AsyncRequiresNet45Error = "Async support is only available in .NET 4.5 builds";

        public static void DebugCommand(this ILog log, IDbCommand cmd)
        {
            var sb = new StringBuilder();

            sb.Append("SQL: ").Append(cmd.CommandText);

            if (cmd.Parameters.Count > 0)
            {
                sb.AppendLine()
                  .Append("PARAMS: ");

                for (int i = 0; i < cmd.Parameters.Count; i++)
                {
                    var p = (IDataParameter)cmd.Parameters[i];
                    if (i > 0)
                        sb.Append(", ");
                    sb.AppendFormat("{0}={1}", p.ParameterName, p.Value);
                }
            }

            log.Debug(sb.ToString());
        }

        public static T CreateInstance<T>()
        {
            return (T)ReflectionExtensions.CreateInstance<T>();
        }

        public static bool IsScalar<T>()
        {
            return typeof(T).IsValueType || typeof(T) == typeof(string);
        }

        public static T ConvertTo<T>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider)
        {
            using (reader)
            {
                if (reader.Read())
                {
                    if (typeof(T) == typeof (List<object>))
                        return (T)(object)reader.ConvertToListObjects();

                    if (typeof(T) == typeof(Dictionary<string, object>))
                        return (T)(object)reader.ConvertToDictionaryObjects();
                    
                    var row = CreateInstance<T>();
                    var indexCache = reader.GetIndexFieldsCache(ModelDefinition<T>.Definition, dialectProvider);
                    var values = new object[reader.FieldCount];
                    row.PopulateWithSqlReader(dialectProvider, reader, indexCache, values);
                    return row;
                }
                return default(T);
            }
        }

        public static List<object> ConvertToListObjects(this IDataReader dataReader)
        {
            var row = new List<object>();
            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                row.Add(dataReader.GetValue(i));
            }
            return row;
        }

        public static Dictionary<string, object> ConvertToDictionaryObjects(this IDataReader dataReader)
        {
            var row = new Dictionary<string, object>();
            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                row[dataReader.GetName(i).Trim()] = dataReader.GetValue(i);
            }
            return row;
        }

        public static List<T> ConvertToList<T>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider)
        {
            if (typeof(T) == typeof(List<object>))
            {
                var to = new List<List<object>>();
                using (reader)
                {
                    while (reader.Read())
                    {
                        var row = reader.ConvertToListObjects();
                        to.Add(row);
                    }
                }
                return (List<T>)(object)to;
            }
            if (typeof(T) == typeof(Dictionary<string, object>))
            {
                var to = new List<Dictionary<string,object>>();
                using (reader)
                {
                    while (reader.Read())
                    {
                        var row = reader.ConvertToDictionaryObjects();
                        to.Add(row);
                    }
                }
                return (List<T>)(object)to;
            }
            else
            {
                var to = new List<T>();
                using (reader)
                {
                    var indexCache = reader.GetIndexFieldsCache(ModelDefinition<T>.Definition, dialectProvider);
                    var values = new object[reader.FieldCount];
                    while (reader.Read())
                    {
                        var row = CreateInstance<T>();
                        row.PopulateWithSqlReader(dialectProvider, reader, indexCache, values);
                        to.Add(row);
                    }
                }
                return to;
            }
        }

        public static object ConvertTo(this IDataReader reader, IOrmLiteDialectProvider dialectProvider, Type type)
        {
            var modelDef = type.GetModelDefinition();
            using (reader)
            {
                if (reader.Read())
                {
                    var row = type.CreateInstance();
                    var indexCache = reader.GetIndexFieldsCache(modelDef, dialectProvider);
                    var values = new object[reader.FieldCount];
                    row.PopulateWithSqlReader(dialectProvider, reader, indexCache, values);
                    return row;
                }
                return type.GetDefaultValue();
            }
        }

        public static IList ConvertToList(this IDataReader reader, IOrmLiteDialectProvider dialectProvider, Type type)
        {
            var modelDef = type.GetModelDefinition();
            var listInstance = typeof(List<>).MakeGenericType(type).CreateInstance();
            var to = (IList)listInstance;
            using (reader)
            {
                var indexCache = reader.GetIndexFieldsCache(modelDef, dialectProvider);
                var values = new object[reader.FieldCount];
                while (reader.Read())
                {
                    var row = type.CreateInstance();
                    row.PopulateWithSqlReader(dialectProvider, reader, indexCache, values);
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
            return string.Format(sqlText, escapedParams.ToArray());
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

        public static Tuple<FieldDefinition, int, IOrmLiteConverter>[] GetIndexFieldsCache(this IDataReader reader, ModelDefinition modelDefinition, IOrmLiteDialectProvider dialect)
        {
            var cache = new List<Tuple<FieldDefinition, int, IOrmLiteConverter>>();
            var ignoredFields = modelDefinition.IgnoredFieldDefinitions;
            var remainingFieldDefs = modelDefinition.FieldDefinitionsArray
                .Where(x => !ignoredFields.Contains(x) && x.SetValueFn != null).ToList();

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);                
                var fieldDef = modelDefinition.GetFieldDefinition(columnName);

                if (fieldDef != null && !ignoredFields.Contains(fieldDef) && fieldDef.SetValueFn != null)
                {
                    remainingFieldDefs.Remove(fieldDef);
                    cache.Add(Tuple.Create(fieldDef, i, dialect.GetConverterBestMatch(fieldDef)));
                }
            }

            foreach (var fieldDef in remainingFieldDefs)
            {
                var index = FindColumnIndex(reader, dialect, fieldDef);
                if (index != NotFound)
                {
                    cache.Add(Tuple.Create(fieldDef, index, dialect.GetConverterBestMatch(fieldDef)));
                }
            }

            return cache.ToArray();
        }

        private const int NotFound = -1;
        internal static int FindColumnIndex(this IDataReader reader, IOrmLiteDialectProvider dialectProvider, FieldDefinition fieldDef)
        {
            var index = NotFound;
            index = reader.GetColumnIndex(dialectProvider, fieldDef.FieldName);
            if (index == NotFound)
            {
                index = TryGuessColumnIndex(fieldDef.FieldName, reader);
            }
            // Try fallback to original field name when overriden by alias
            if (index == NotFound && fieldDef.Alias != null && !OrmLiteConfig.DisableColumnGuessFallback)
            {
                index = reader.GetColumnIndex(dialectProvider, fieldDef.Name);
                if (index == NotFound)
                {
                    index = TryGuessColumnIndex(fieldDef.Name, reader);
                }
            }

            return index;
        }

        private static readonly Regex AllowedPropertyCharsRegex = new Regex(@"[^0-9a-zA-Z_]",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static int TryGuessColumnIndex(string fieldName, IDataReader dataReader)
        {
            if (OrmLiteConfig.DisableColumnGuessFallback)
                return NotFound;

            var fieldCount = dataReader.FieldCount;
            for (var i = 0; i < fieldCount; i++)
            {
                var dbFieldName = dataReader.GetName(i);

                // First guess: Maybe the DB field has underscores? (most common)
                // e.g. CustomerId (C#) vs customer_id (DB)
                var dbFieldNameWithNoUnderscores = dbFieldName.Replace("_", "");
                if (string.Compare(fieldName, dbFieldNameWithNoUnderscores, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return i;
                }

                // Next guess: Maybe the DB field has special characters?
                // e.g. Quantity (C#) vs quantity% (DB)
                var dbFieldNameSanitized = AllowedPropertyCharsRegex.Replace(dbFieldName, string.Empty);
                if (string.Compare(fieldName, dbFieldNameSanitized, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return i;
                }

                // Next guess: Maybe the DB field has special characters *and* has underscores?
                // e.g. Quantity (C#) vs quantity_% (DB)
                if (string.Compare(fieldName, dbFieldNameSanitized.Replace("_", string.Empty), StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return i;
                }

                // Next guess: Maybe the DB field has some prefix that we don't have in our C# field?
                // e.g. CustomerId (C#) vs t130CustomerId (DB)
                if (dbFieldName.EndsWith(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }

                // Next guess: Maybe the DB field has some prefix that we don't have in our C# field *and* has underscores?
                // e.g. CustomerId (C#) vs t130_CustomerId (DB)
                if (dbFieldNameWithNoUnderscores.EndsWith(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }

                // Next guess: Maybe the DB field has some prefix that we don't have in our C# field *and* has special characters?
                // e.g. CustomerId (C#) vs t130#CustomerId (DB)
                if (dbFieldNameSanitized.EndsWith(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }

                // Next guess: Maybe the DB field has some prefix that we don't have in our C# field *and* has underscores *and* has special characters?
                // e.g. CustomerId (C#) vs t130#Customer_I#d (DB)
                if (dbFieldNameSanitized.Replace("_", "").EndsWith(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }

                // Cater for Naming Strategies like PostgreSQL that has lower_underscore names
                if (dbFieldNameSanitized.Replace("_", "").EndsWith(fieldName.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return NotFound;
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


        public static ModelDefinition GetModelDefinition(Type modelType)
        {
            return modelType.GetModelDefinition();
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

        public static List<string> GetNonDefaultValueInsertFields<T>(T obj)
        {
            var insertFields = new List<string>();
            var modelDef = typeof(T).GetModelDefinition();
            foreach (var fieldDef in modelDef.FieldDefinitionsArray)
            {
                if (!string.IsNullOrEmpty(fieldDef.DefaultValue))
                {
                    var value = fieldDef.GetValue(obj);
                    if (value == null || value.Equals(fieldDef.FieldTypeDefaultValue))
                        continue;
                }
                insertFields.Add(fieldDef.Name);
            }

            return insertFields.Count == modelDef.FieldDefinitionsArray.Length 
                ? null 
                : insertFields;
        }

        public static List<string> ParseTokens(this string expr)
        {
            var to = new List<string>();

            if (string.IsNullOrEmpty(expr))
                return to;

            var inDoubleQuotes = false;
            var inSingleQuotes = false;
            var inBraces = false;

            var pos = 0;

            for (var i = 0; i < expr.Length; i++)
            {
                var c = expr[i];
                if (inDoubleQuotes)
                {
                    if (c == '"')
                        inDoubleQuotes = false;
                    continue;
                }
                if (inSingleQuotes)
                {
                    if (c == '\'')
                        inSingleQuotes = false;
                    continue;
                }
                if (c == '"')
                {
                    inDoubleQuotes = true;
                    continue;
                }
                if (c == '\'')
                {
                    inSingleQuotes = true;
                    continue;
                }
                if (c == '(')
                {
                    inBraces = true;
                    continue;
                }
                if (c == ')')
                {
                    inBraces = false;

                    var endPos = expr.IndexOf(',', i);
                    if (endPos == -1)
                        endPos = expr.Length;

                    to.Add(expr.Substring(pos, endPos - pos).Trim());

                    pos = endPos;
                    continue;
                }

                if (c == ',')
                {
                    if (!inBraces)
                    {
                        var arg = expr.Substring(pos, i - pos).Trim();
                        if (!string.IsNullOrEmpty(arg))
                            to.Add(arg);
                        pos = i + 1;
                    }
                }
            }

            var remaining = expr.Substring(pos, expr.Length - pos);
            if (!string.IsNullOrEmpty(remaining))
                remaining = remaining.Trim();

            if (!string.IsNullOrEmpty(remaining))
                to.Add(remaining);

            return to;
        }

        public static string[] AllAnonFields(this Type type)
        {
            return type.GetPublicProperties().Select(x => x.Name).ToArray();
        }
    }
}