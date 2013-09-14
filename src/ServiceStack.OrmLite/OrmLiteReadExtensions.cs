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
using System.Diagnostics;
using System.Text;
using ServiceStack.Logging;
using ServiceStack.Text;
using System.Linq;

namespace ServiceStack.OrmLite
{
	public delegate string GetQuotedValueDelegate(object value, Type fieldType);
	public delegate object ConvertDbValueDelegate(object value, Type type);
	public delegate void PropertySetterDelegate(object instance, object value);
	public delegate object PropertyGetterDelegate(object instance);

	public delegate object GetValueDelegate(int i);

	public static class OrmLiteReadExtensions
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(OrmLiteReadExtensions));
	    public const string UseDbConnectionExtensions = "Use IDbConnection Extensions instead";

		[Conditional("DEBUG")]
		private static void LogDebug(string fmt, params object[] args)
		{
			if (args.Length > 0)
				Log.DebugFormat(fmt, args);
			else
				Log.Debug(fmt);
		}

		internal static IDataReader ExecReader(this IDbCommand dbCmd, string sql)
		{
			LogDebug(sql);
			dbCmd.CommandTimeout = OrmLiteConfig.CommandTimeout;
			dbCmd.CommandText = sql;
			return dbCmd.ExecuteReader();
		}

        internal static IDataReader ExecReader(this IDbCommand dbCmd, string sql, IEnumerable<IDataParameter> parameters)
        {
            LogDebug(sql);
			dbCmd.CommandTimeout = OrmLiteConfig.CommandTimeout;
			dbCmd.CommandText = sql;
            dbCmd.Parameters.Clear();

            foreach (var param in parameters)
            {
                dbCmd.Parameters.Add(param);
            }

			return dbCmd.ExecuteReader();
        }

		public static GetValueDelegate GetValueFn<T>(IDataRecord reader)
		{
			var type = typeof(T);

			if (type == typeof(string))
				return reader.GetString;

			if (type == typeof(short))
				return i => reader.GetInt16(i);

			if (type == typeof(int))
				return i => reader.GetInt32(i);

			if (type == typeof(long))
				return i => reader.GetInt64(i);

			if (type == typeof(bool))
				return i => reader.GetBoolean(i);

			if (type == typeof(DateTime))
				return i => reader.GetDateTime(i);

			if (type == typeof(Guid))
				return i => reader.GetGuid(i);

			if (type == typeof(float))
				return i => reader.GetFloat(i);

			if (type == typeof(double))
				return i => reader.GetDouble(i);

			if (type == typeof(decimal) || type == typeof(decimal?))
				return i => reader.GetDecimal(i);

			return reader.GetValue;
		}

        public static bool IsScalar<T>()
        {
            return typeof(T).IsValueType || typeof(T) == typeof(string);
        }

	    internal static List<T> Select<T>(this IDbCommand dbCmd)
		{
			return Select<T>(dbCmd, (string)null);
		}

	    internal static List<T> Select<T>(this IDbCommand dbCmd, string sqlFilter, params object[] filterParams)
		{
			using (var reader = dbCmd.ExecReader(
				OrmLiteConfig.DialectProvider.ToSelectStatement(typeof(T), sqlFilter, filterParams)))
			{
				return reader.ConvertToList<T>();
			}
		}

	    internal static List<TModel> Select<TModel>(this IDbCommand dbCmd, Type fromTableType)
		{
			return Select<TModel>(dbCmd, fromTableType, null);
		}

	    internal static List<TModel> Select<TModel>(this IDbCommand dbCmd, Type fromTableType, string sqlFilter, params object[] filterParams)
		{
			var sql = new StringBuilder();
			var modelDef = ModelDefinition<TModel>.Definition;
		    sql.AppendFormat("SELECT {0} FROM {1}", OrmLiteConfig.DialectProvider.GetColumnNames( modelDef),
		                     OrmLiteConfig.DialectProvider.GetQuotedTableName(fromTableType.GetModelDefinition()));
            if (!string.IsNullOrEmpty(sqlFilter))
			{
				sqlFilter = sqlFilter.SqlFormat(filterParams);
				sql.Append(" WHERE ");
				sql.Append(sqlFilter);
			}

			using (var reader = dbCmd.ExecReader(sql.ToString()))
			{
				return reader.ConvertToList<TModel>();
			}
		}

	    internal static IEnumerable<T> Each<T>(this IDbCommand dbCmd)
		{
			return Each<T>(dbCmd, null);
		}

	    internal static IEnumerable<T> Each<T>(this IDbCommand dbCmd, string filter, params object[] filterParams)
		{
			var fieldDefs = ModelDefinition<T>.Definition.FieldDefinitionsArray;
			using (var reader = dbCmd.ExecReader(
				OrmLiteConfig.DialectProvider.ToSelectStatement(typeof(T),  filter, filterParams)))
			{
				var indexCache = reader.GetIndexFieldsCache(ModelDefinition<T>.Definition);
                while (reader.Read())
				{
                    var row = OrmLiteUtilExtensions.CreateInstance<T>();
                    row.PopulateWithSqlReader(reader, fieldDefs, indexCache);
					yield return row;
				}
			}
		}

	    internal static T First<T>(this IDbCommand dbCmd, string filter, params object[] filterParams)
		{
			return First<T>(dbCmd, filter.SqlFormat(filterParams));
		}

	    internal static T First<T>(this IDbCommand dbCmd, string filter)
		{
			var result = FirstOrDefault<T>(dbCmd, filter);
			if (Equals(result, default(T)))
			{
				throw new ArgumentNullException(string.Format(
					"{0}: '{1}' does not exist", typeof(T).Name, filter));
			}
			return result;
		}

	    internal static T FirstOrDefault<T>(this IDbCommand dbCmd, string filter, params object[] filterParams)
		{
			return FirstOrDefault<T>(dbCmd, filter.SqlFormat(filterParams));
		}

	    internal static T FirstOrDefault<T>(this IDbCommand dbCmd, string filter)
		{
			using (var dbReader = dbCmd.ExecReader(
				OrmLiteConfig.DialectProvider.ToSelectStatement(typeof(T),  filter)))
			{
				return dbReader.ConvertTo<T>();
			}
		}

	    internal static T GetById<T>(this IDbCommand dbCmd, object idValue)
		{
			return First<T>(dbCmd, OrmLiteConfig.DialectProvider.GetQuotedColumnName(ModelDefinition<T>.PrimaryKeyName) + " = {0}".SqlFormat(idValue));
		}

		[ThreadStatic]
		private static Type lastQueryType;
		private static void SetFilter<T>(IDbCommand dbCmd, string name, object value)
		{
			dbCmd.Parameters.Clear();
			var p = dbCmd.CreateParameter();
			p.ParameterName = name;
			p.DbType = OrmLiteConfig.DialectProvider.GetColumnDbType(value.GetType());
			p.Direction = ParameterDirection.Input;
			dbCmd.Parameters.Add(p);
			dbCmd.CommandText = GetFilterSql<T>(dbCmd);
			lastQueryType = typeof(T);
		}

		public static void SetFilters<T>(this IDbCommand dbCmd, object anonType, bool excludeNulls)
		{
			SetParameters<T>(dbCmd, anonType, excludeNulls);

			dbCmd.CommandText = GetFilterSql<T>(dbCmd);
		}

		private static void SetParameters<T>(this IDbCommand dbCmd, object anonType, bool excludeNulls)
		{
			dbCmd.Parameters.Clear();
			lastQueryType = null;
			if (anonType == null) return;

            var pis = anonType.GetType().GetSerializableProperties();
            var model = ModelDefinition<T>.Definition;

			foreach (var pi in pis)
			{
				var mi = pi.GetGetMethod();
				if (mi == null) continue;

				var value = mi.Invoke(anonType, new object[0]);
				if (excludeNulls && value == null) continue;


				var p = dbCmd.CreateParameter();

                var targetField = model != null ? model.FieldDefinitions.FirstOrDefault(f => string.Equals(f.Name, pi.Name)) : null;
                if (targetField != null && !string.IsNullOrEmpty(targetField.Alias))
                    p.ParameterName = targetField.Alias;
                else
                    p.ParameterName = pi.Name;

				p.DbType = OrmLiteConfig.DialectProvider.GetColumnDbType(pi.PropertyType);
				p.Direction = ParameterDirection.Input;
                p.Value = value ?? DBNull.Value;
				dbCmd.Parameters.Add(p);
			}
		}

        private static void SetParameters(this IDbCommand dbCmd, object anonType, bool excludeNulls)
        {
            dbCmd.Parameters.Clear();
            lastQueryType = null;
            if (anonType == null)
                return;

            var pis = anonType.GetType().GetSerializableProperties();

            foreach (var pi in pis)
            {
                var mi = pi.GetGetMethod();
                if (mi == null)
                    continue;

                var value = mi.Invoke(anonType, new object[0]);
                if (excludeNulls && value == null)
                    continue;

                var p = dbCmd.CreateParameter();

                p.ParameterName = pi.Name;
                p.DbType = OrmLiteConfig.DialectProvider.GetColumnDbType(pi.PropertyType);
                p.Direction = ParameterDirection.Input;
                p.Value = value ?? DBNull.Value;
                dbCmd.Parameters.Add(p);
            }
        }	

		private static void SetParameters(this IDbCommand dbCmd, Dictionary<string,object> dict, bool excludeNulls)
		{
			dbCmd.Parameters.Clear();
			lastQueryType = null;
			if (dict == null) return;

			foreach (var kvp in dict)
			{
				var value = dict[kvp.Key];
				if (excludeNulls && value == null) continue;
				var p = dbCmd.CreateParameter();
				p.ParameterName = kvp.Key;

                if (value != null)
                {
                    p.DbType = OrmLiteConfig.DialectProvider.GetColumnDbType(value.GetType());
                }
				
				p.Direction = ParameterDirection.Input;
                p.Value = value ?? DBNull.Value;
				dbCmd.Parameters.Add(p);
			}
		}


		public static void SetFilters<T>(this IDbCommand dbCmd, object anonType)
		{
            dbCmd.SetFilters<T>(anonType, excludeNulls: false);
		}

		public static void ClearFilters(this IDbCommand dbCmd)
		{
			dbCmd.Parameters.Clear();
		}

		private static string GetFilterSql<T>(IDbCommand dbCmd)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < dbCmd.Parameters.Count; i++)
			{
				sb.Append(i == 0 ? " " : " AND ");
				var p = (IDbDataParameter)dbCmd.Parameters[i];
				sb.AppendFormat("{0} = {1}{2}",
								OrmLiteConfig.DialectProvider.GetQuotedColumnName(p.ParameterName),
								OrmLiteConfig.DialectProvider.ParamString,
								p.ParameterName);
			}
			return OrmLiteConfig.DialectProvider.ToSelectStatement(typeof(T), sb.ToString());
		}

        internal static T QueryById<T>(this IDbCommand dbCmd, object value)
		{
			if (dbCmd.Parameters.Count != 1
				|| ((IDbDataParameter)dbCmd.Parameters[0]).ParameterName != ModelDefinition<T>.PrimaryKeyName
				|| lastQueryType != typeof(T))
				SetFilter<T>(dbCmd, ModelDefinition<T>.PrimaryKeyName, value);

			((IDbDataParameter)dbCmd.Parameters[0]).Value = value;

			using (var dbReader = dbCmd.ExecuteReader())
				return dbReader.ConvertTo<T>();
		}

        internal static T SingleWhere<T>(this IDbCommand dbCmd, string name, object value)
		{
			if (dbCmd.Parameters.Count != 1 || ((IDbDataParameter)dbCmd.Parameters[0]).ParameterName != name
				|| lastQueryType != typeof(T))
				SetFilter<T>(dbCmd, name, value);

			((IDbDataParameter)dbCmd.Parameters[0]).Value = value;

			using (var dbReader = dbCmd.ExecuteReader())
				return dbReader.ConvertTo<T>();
		}

        internal static T QuerySingle<T>(this IDbCommand dbCmd, object anonType)
		{
			if (IsScalar<T>()) return QueryScalar<T>(dbCmd, anonType);

            dbCmd.SetFilters<T>(anonType, excludeNulls: false);

			using (var dbReader = dbCmd.ExecuteReader())
				return dbReader.ConvertTo<T>();
		}

        internal static T QuerySingle<T>(this IDbCommand dbCmd, string sql, object anonType)
		{
			if (IsScalar<T>()) return QueryScalar<T>(dbCmd, sql, anonType);

            dbCmd.SetParameters<T>(anonType, excludeNulls: false);
            dbCmd.CommandText = OrmLiteConfig.DialectProvider.ToSelectStatement(typeof(T), sql);

			using (var dbReader = dbCmd.ExecuteReader())
				return dbReader.ConvertTo<T>();
		}

        internal static List<T> Where<T>(this IDbCommand dbCmd, string name, object value)
		{
			if (dbCmd.Parameters.Count != 1 || ((IDbDataParameter)dbCmd.Parameters[0]).ParameterName != name
				|| lastQueryType != typeof(T))
				SetFilter<T>(dbCmd, name, value);

			((IDbDataParameter)dbCmd.Parameters[0]).Value = value;

			using (var dbReader = dbCmd.ExecuteReader())
				return dbReader.ConvertToList<T>();
		}

        internal static List<T> Where<T>(this IDbCommand dbCmd, object anonType)
		{
			dbCmd.SetFilters<T>(anonType);

			using (var dbReader = dbCmd.ExecuteReader())
				return IsScalar<T>()
					? dbReader.GetFirstColumn<T>()
					: dbReader.ConvertToList<T>();
		}

        internal static List<T> Query<T>(this IDbCommand dbCmd, string sql, object anonType = null)
		{
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeNulls: false);
            dbCmd.CommandText = OrmLiteConfig.DialectProvider.ToSelectStatement(typeof(T), sql);

			using (var dbReader = dbCmd.ExecuteReader())
				return IsScalar<T>()
					? dbReader.GetFirstColumn<T>()
					: dbReader.ConvertToList<T>();
		}

        internal static List<T> Query<T>(this IDbCommand dbCmd, string sql, Dictionary<string, object> dict)
		{
            if (dict != null) dbCmd.SetParameters(dict, excludeNulls: false);
            dbCmd.CommandText = OrmLiteConfig.DialectProvider.ToSelectStatement(typeof(T), sql);

			using (var dbReader = dbCmd.ExecuteReader())
				return IsScalar<T>()
					? dbReader.GetFirstColumn<T>()
					: dbReader.ConvertToList<T>();
		}

        internal static int ExecuteNonQuery(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            if (anonType != null)
                dbCmd.SetParameters(anonType, excludeNulls: false);
            dbCmd.CommandText = sql;

            return dbCmd.ExecuteNonQuery();
        }

        internal static int ExecuteNonQuery(this IDbCommand dbCmd, string sql, Dictionary<string, object> dict)
        {
            if (dict != null)
                dbCmd.SetParameters(dict, excludeNulls: false);
            dbCmd.CommandText = sql;

            return dbCmd.ExecuteNonQuery();
        }

	    internal static T QueryScalar<T>(this IDbCommand dbCmd, object anonType)
		{
            dbCmd.SetFilters<T>(anonType, excludeNulls: false);

			using (var dbReader = dbCmd.ExecuteReader())
				return GetScalar<T>(dbReader);
		}

	    internal static T QueryScalar<T>(this IDbCommand dbCmd, string sql, object anonType = null)
		{
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeNulls: false);
            dbCmd.CommandText = OrmLiteConfig.DialectProvider.ToSelectStatement(typeof(T), sql);

			using (var dbReader = dbCmd.ExecuteReader())
				return GetScalar<T>(dbReader);
		}

        internal static List<T> SqlList<T>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeNulls: false);
            dbCmd.CommandText = sql;

            using (var dbReader = dbCmd.ExecuteReader())
                return IsScalar<T>()
                    ? dbReader.GetFirstColumn<T>()
                    : dbReader.ConvertToList<T>();
        }

        internal static List<T> SqlList<T>(this IDbCommand dbCmd, string sql, Dictionary<string, object> dict)
        {
            if (dict != null) dbCmd.SetParameters(dict, excludeNulls: false);
            dbCmd.CommandText = sql;

            using (var dbReader = dbCmd.ExecuteReader())
                return IsScalar<T>()
                    ? dbReader.GetFirstColumn<T>()
                    : dbReader.ConvertToList<T>();
        }

	    internal static T SqlScalar<T>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeNulls: false);
            dbCmd.CommandText = sql;

            using (var dbReader = dbCmd.ExecuteReader())
                return GetScalar<T>(dbReader);
        }

        internal static T SqlScalar<T>(this IDbCommand dbCmd, string sql, Dictionary<string, object> dict)
        {
            if (dict != null) dbCmd.SetParameters(dict, excludeNulls: false);
            dbCmd.CommandText = sql;

            using (var dbReader = dbCmd.ExecuteReader())
                return GetScalar<T>(dbReader);
        }

        internal static List<T> ByExampleWhere<T>(this IDbCommand dbCmd, object anonType)
        {
            return ByExampleWhere<T>(dbCmd, anonType, true);
        }

        internal static List<T> ByExampleWhere<T>(this IDbCommand dbCmd, object anonType, bool excludeNulls)
		{
            dbCmd.SetFilters<T>(anonType, excludeNulls);

			using (var dbReader = dbCmd.ExecuteReader())
				return dbReader.ConvertToList<T>();
		}

	    internal static List<T> QueryByExample<T>(this IDbCommand dbCmd, string sql, object anonType = null)
		{
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeNulls: false);
            dbCmd.CommandText = OrmLiteConfig.DialectProvider.ToSelectStatement(typeof(T), sql);

			using (var dbReader = dbCmd.ExecuteReader())
				return dbReader.ConvertToList<T>();
		}

	    internal static IEnumerable<T> QueryEach<T>(this IDbCommand dbCmd, string sql, object anonType = null)
		{
            if (anonType != null) dbCmd.SetFilters<T>(anonType);

			var fieldDefs = ModelDefinition<T>.Definition.FieldDefinitionsArray;
			using (var reader = dbCmd.ExecuteReader())
			{
				var indexCache = reader.GetIndexFieldsCache(ModelDefinition<T>.Definition);
                while (reader.Read())
				{
                    var row = OrmLiteUtilExtensions.CreateInstance<T>();
					row.PopulateWithSqlReader(reader, fieldDefs, indexCache);
					yield return row;
				}
			}
		}

	    internal static IEnumerable<T> EachWhere<T>(this IDbCommand dbCmd, object anonType)
		{
			dbCmd.SetFilters<T>(anonType);

			var fieldDefs = ModelDefinition<T>.Definition.FieldDefinitionsArray;
			using (var reader = dbCmd.ExecuteReader())
			{
				var indexCache = reader.GetIndexFieldsCache(ModelDefinition<T>.Definition);
                while (reader.Read())
				{
                    var row = OrmLiteUtilExtensions.CreateInstance<T>();
                    row.PopulateWithSqlReader(reader, fieldDefs, indexCache);
					yield return row;
				}
			}
		}

	    internal static T GetByIdOrDefault<T>(this IDbCommand dbCmd, object idValue)
		{
			return FirstOrDefault<T>(dbCmd, OrmLiteConfig.DialectProvider.GetQuotedColumnName(ModelDefinition<T>.PrimaryKeyName) + " = {0}".SqlFormat(idValue));
		}

	    internal static List<T> GetByIds<T>(this IDbCommand dbCmd, IEnumerable idValues)
		{
			var sql = idValues.GetIdsInSql();
			return sql == null
				? new List<T>()
				: Select<T>(dbCmd, OrmLiteConfig.DialectProvider.GetQuotedColumnName(ModelDefinition<T>.PrimaryKeyName) + " IN (" + sql + ")");
		}

	    internal static T GetByIdParam<T>(this IDbCommand dbCmd, object id)
        {
            var modelDef = ModelDefinition<T>.Definition;
            var idParamString = OrmLiteConfig.DialectProvider.ParamString + "0";

            var sql = string.Format("{0} WHERE {1} = {2}",
                OrmLiteConfig.DialectProvider.ToSelectStatement(typeof(T), "", null),
                OrmLiteConfig.DialectProvider.GetQuotedColumnName(modelDef.PrimaryKey.FieldName),
                idParamString);

            var idParam = dbCmd.CreateParameter();
            idParam.ParameterName = idParamString;
            idParam.Value = id;
            List<IDataParameter> paramsToInsert = new List<IDataParameter>();
            paramsToInsert.Add(idParam);

            return dbCmd.ExecReader(sql, paramsToInsert).ConvertTo<T>();
        }

	    internal static T GetScalar<T>(this IDbCommand dbCmd, string sql, params object[] sqlParams)
		{
			using (var reader = dbCmd.ExecReader(sql.SqlFormat(sqlParams)))
				return GetScalar<T>(reader);
		}

	    internal static T GetScalar<T>(this IDataReader reader)
		{
			while (reader.Read()){
				Type t = typeof(T);

				object oValue = reader.GetValue(0);
				if (oValue == DBNull.Value) return default(T);
	
				if (t== typeof(DateTime) || t== typeof(DateTime?)) 
					return(T)(object) DateTime.Parse(oValue.ToString(), System.Globalization.CultureInfo.CurrentCulture);	
						
				if (t== typeof(decimal) || t== typeof(decimal?)) 
					return(T)(object)decimal.Parse(oValue.ToString(), System.Globalization.CultureInfo.CurrentCulture);	
						
				if (t== typeof(double) || t== typeof(double?)) 
					return(T)(object)double.Parse(oValue.ToString(), System.Globalization.CultureInfo.CurrentCulture);
						
				if (t== typeof(float) || t== typeof(float?))
					return(T)(object)float.Parse(oValue.ToString(), System.Globalization.CultureInfo.CurrentCulture);
						
				object o = OrmLiteConfig.DialectProvider.ConvertDbValue(oValue, t);
				return o == null ? default(T) : (T)o;
			}
			return default(T);
		}

	    internal static long GetLastInsertId(this IDbCommand dbCmd)
		{
			return OrmLiteConfig.DialectProvider.GetLastInsertId(dbCmd);
		}

	    internal static List<T> GetFirstColumn<T>(this IDbCommand dbCmd, string sql, params object[] sqlParams)
		{
			using (var dbReader = dbCmd.ExecReader(sql.SqlFormat(sqlParams)))
				return GetFirstColumn<T>(dbReader);
		}

	    internal static List<T> GetFirstColumn<T>(this IDataReader reader)
		{
			var columValues = new List<T>();
			var getValueFn = GetValueFn<T>(reader);
			while (reader.Read())
			{
				var value = getValueFn(0);
                if (value == DBNull.Value)
                    value = default(T);

				columValues.Add((T)value);
			}
			return columValues;
		}

        /// <summary>
        /// Alias for GetFirstColumn. Returns the first selected column in a List
        /// </summary>
	    internal static List<T> GetList<T>(this IDbCommand dbCmd, string sql, params object[] sqlParams)
        {
            return dbCmd.GetFirstColumn<T>(sql, sqlParams);
	    }

	    internal static HashSet<T> GetFirstColumnDistinct<T>(this IDbCommand dbCmd, string sql, params object[] sqlParams)
        {
            using (var dbReader = dbCmd.ExecReader(sql.SqlFormat(sqlParams)))
                return GetFirstColumnDistinct<T>(dbReader);
        }

        /// <summary>
        /// Alias for GetFirstColumnDistinct. Returns the first selected column in a HashSet
        /// </summary>
        public static HashSet<T> GetHashSet<T>(this IDbCommand dbCmd, string sql, params object[] sqlParams)
        {
            return dbCmd.GetFirstColumnDistinct<T>(sql, sqlParams);
        }

	    internal static HashSet<T> GetFirstColumnDistinct<T>(this IDataReader reader)
		{
			var columValues = new HashSet<T>();
			var getValueFn = GetValueFn<T>(reader);
			while (reader.Read())
			{
				var value = getValueFn(0);
                if (value == DBNull.Value)
                    value = default(T);

				columValues.Add((T)value);
			}
			return columValues;
		}

	    internal static Dictionary<K, List<V>> GetLookup<K, V>(this IDbCommand dbCmd, string sql, params object[] sqlParams)
		{
			using (var dbReader = dbCmd.ExecReader(sql.SqlFormat(sqlParams)))
				return GetLookup<K, V>(dbReader);
		}

	    internal static Dictionary<K, List<V>> GetLookup<K, V>(this IDataReader reader)
		{
			var lookup = new Dictionary<K, List<V>>();

			var getKeyFn = GetValueFn<K>(reader);
			var getValueFn = GetValueFn<V>(reader);
			while (reader.Read())
			{
				var key = (K)getKeyFn(0);
				var value = (V)getValueFn(1);

				List<V> values;
				if (!lookup.TryGetValue(key, out values))
				{
					values = new List<V>();
					lookup[key] = values;
				}
				values.Add(value);
			}

			return lookup;
		}

	    internal static Dictionary<K, V> GetDictionary<K, V>(this IDbCommand dbCmd, string sql, params object[] sqlParams)
		{
			using (var dbReader = dbCmd.ExecReader(sql.SqlFormat(sqlParams)))
				return GetDictionary<K, V>(dbReader);
		}

	    internal static Dictionary<K, V> GetDictionary<K, V>(this IDataReader reader)
		{
			var map = new Dictionary<K, V>();

			var getKeyFn = GetValueFn<K>(reader);
			var getValueFn = GetValueFn<V>(reader);
			while (reader.Read())
			{
				var key = (K)getKeyFn(0);
				var value = (V)getValueFn(1);

				map.Add(key, value);
			}

			return map;
		}
		
		// somo aditional methods

	    internal static bool HasChildren<T>(this IDbCommand dbCmd, object record)
		{
			return HasChildren<T>(dbCmd, record, string.Empty);
		}
		
		private static bool HasChildren<T>(this IDbCommand dbCmd, object record, string sqlFilter, params object[] filterParams)
		{
			var fromTableType = typeof(T);			
			var sql = OrmLiteConfig.DialectProvider.ToExistStatement(fromTableType, record,sqlFilter, filterParams);
			dbCmd.CommandText = sql;
			var result =  dbCmd.ExecuteScalar();
			return result != null;
		}


	    internal static bool Exists<T>(this IDbCommand dbCmd, string sqlFilter, params object[] filterParams)
		{
			return HasChildren<T>(dbCmd, null, sqlFilter, filterParams);
		}

	    internal static bool Exists<T>(this IDbCommand dbCmd, object record)
		{
			return HasChildren<T>(dbCmd, record, string.Empty);
		}
					
		// procedures ...		
	    internal static List<TOutputModel> SelectFromProcedure<TOutputModel>(this IDbCommand dbCommand,
			object fromObjWithProperties)
		{
			return SelectFromProcedure<TOutputModel>(dbCommand, fromObjWithProperties,string.Empty);
		}

	    internal static List<TOutputModel> SelectFromProcedure<TOutputModel>(this IDbCommand dbCommand,
			object fromObjWithProperties,
			string sqlFilter, 
			params object[] filterParams)
		{
			var modelType = typeof(TOutputModel);	
			
			string sql = OrmLiteConfig.DialectProvider.ToSelectFromProcedureStatement(
				fromObjWithProperties,modelType, sqlFilter, filterParams);
			
			using (var reader = dbCommand.ExecReader(sql))
			{
				return reader.ConvertToList<TOutputModel>();
			}
		}

	    public static long GetLongScalar(this IDbCommand dbCmd)
		{
			var result = dbCmd.ExecuteScalar();
			if (result is DBNull) return default(long);
			if (result is int) return (int)result;
			if (result is decimal) return Convert.ToInt64((decimal)result);
			if (result is ulong) return (long)Convert.ToUInt64(result);
			return (long)result;
		}			
	}
}