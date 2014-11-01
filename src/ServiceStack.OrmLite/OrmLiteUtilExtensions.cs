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
using System.Text;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteUtilExtensions
    {
        internal const string AsyncRequiresNet45Error = "Async support is only available in .NET 4.5 builds";

        public static T CreateInstance<T>()
        {
            return (T)ReflectionExtensions.CreateInstance<T>();
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

        public static string SqlTableRaw(this string tableName, IOrmLiteDialectProvider dialect=null)
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

        public static SqlInValues SqlInValues<T>(this T[] values, IOrmLiteDialectProvider dialect=null)
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
    }
}