//
// ServiceStack.OrmLite: Light-weight POCO ORM for .NET and Mono
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2010 Liquidbit Ltd.
//
// Licensed under the same terms of ServiceStack: new BSD license.
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
        public static T CreateInstance<T>()
        {
            return (T)ReflectionExtensions.CreateInstance<T>();
        }

		public static T ConvertTo<T>(this IDataReader dataReader)
        {
			var fieldDefs = ModelDefinition<T>.Definition.AllFieldDefinitionsArray;

			using (dataReader)
			{
				if (dataReader.Read())
				{
                    var row = CreateInstance<T>();
					var indexCache = dataReader.GetIndexFieldsCache(ModelDefinition<T>.Definition);
					row.PopulateWithSqlReader(dataReader, fieldDefs, indexCache);
					return row;
				}
				return default(T);
			}
		}

		public static List<T> ConvertToList<T>(this IDataReader dataReader)
		{
            var fieldDefs = ModelDefinition<T>.Definition.AllFieldDefinitionsArray;

			var to = new List<T>();
			using (dataReader)
			{
				var indexCache = dataReader.GetIndexFieldsCache(ModelDefinition<T>.Definition);
				while (dataReader.Read())
				{
                    var row = CreateInstance<T>();
					row.PopulateWithSqlReader(dataReader, fieldDefs, indexCache);
					to.Add(row);
				}
			}
			return to;
		}

		internal static string GetColumnNames(this Type tableType)
		{
		    var modelDefinition = tableType.GetModelDefinition();
		    return GetColumnNames(modelDefinition);
		}

	    public static string GetColumnNames(this ModelDefinition modelDef)
	    {
            var sqlColumns = new StringBuilder();
	        modelDef.FieldDefinitions.ForEach(x => 
                sqlColumns.AppendFormat("{0}{1} ", sqlColumns.Length > 0 ? "," : "",
                  OrmLiteConfig.DialectProvider.GetQuotedColumnName(x.FieldName)));

	        return sqlColumns.ToString();
	    }

	    internal static string GetIdsInSql(this IEnumerable idValues)
		{
			var sql = new StringBuilder();
			foreach (var idValue in idValues)
			{
				if (sql.Length > 0) sql.Append(",");
				sql.AppendFormat("{0}".SqlFormat(idValue));
			}
			return sql.Length == 0 ? null : sql.ToString();
		}

		public static string Params(this string sqlText, params object[] sqlParams)
		{
		    return SqlFormat(sqlText, sqlParams);
		}

		public static string SqlFormat(this string sqlText, params object[] sqlParams)
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
						escapedParams.Add(OrmLiteConfig.DialectProvider.GetQuotedValue(sqlParam, sqlParam.GetType()));
					}
				}
			}
			return string.Format(sqlText, escapedParams.ToArray());
		}

		public static string SqlJoin<T>(this List<T> values)
		{
			var sb = new StringBuilder();
			foreach (var value in values)
			{
				if (sb.Length > 0) sb.Append(",");
				sb.Append(OrmLiteConfig.DialectProvider.GetQuotedValue(value, value.GetType()));
			}

			return sb.ToString();
		}

		public static string SqlJoin(IEnumerable values)
		{
			var sb = new StringBuilder();
			foreach (var value in values)
			{
				if (sb.Length > 0) sb.Append(",");
				sb.Append(OrmLiteConfig.DialectProvider.GetQuotedValue(value, value.GetType()));
			}

			return sb.ToString();
		}

		public static SqlInValues SqlInValues<T>(this List<T> values)
		{
			return new SqlInValues(values);
		}

		public static SqlInValues SqlInValues<T>(this T[] values)
		{
			return new SqlInValues(values);
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

	}
}