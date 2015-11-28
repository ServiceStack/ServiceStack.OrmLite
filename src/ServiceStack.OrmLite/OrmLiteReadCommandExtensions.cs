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
using System.Globalization;
using System.Reflection;
using System.Text;
using ServiceStack.Logging;
using System.Linq;
using ServiceStack.OrmLite.Support;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    public delegate void PropertySetterDelegate(object instance, object value);
    public delegate object PropertyGetterDelegate(object instance);

    public delegate object GetValueDelegate(int i);

    public static class OrmLiteReadCommandExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OrmLiteReadCommandExtensions));
        public const string UseDbConnectionExtensions = "Use IDbConnection Extensions instead";

        internal static IDataReader ExecReader(this IDbCommand dbCmd, string sql)
        {
            dbCmd.CommandTimeout = OrmLiteConfig.CommandTimeout;
            dbCmd.CommandText = sql;

            if (Log.IsDebugEnabled)
                Log.DebugCommand(dbCmd);

            return dbCmd.ExecuteReader();
        }

        internal static IDataReader ExecReader(this IDbCommand dbCmd, string sql, IEnumerable<IDataParameter> parameters)
        {
            dbCmd.CommandTimeout = OrmLiteConfig.CommandTimeout;
            dbCmd.CommandText = sql;
            dbCmd.Parameters.Clear();

            foreach (var param in parameters)
            {
                dbCmd.Parameters.Add(param);
            }

            if (Log.IsDebugEnabled)
                Log.DebugCommand(dbCmd);

            return dbCmd.ExecuteReader();
        }

        internal static List<T> Select<T>(this IDbCommand dbCmd)
        {
            return SelectFmt<T>(dbCmd, (string)null);
        }

        internal static List<T> SelectFmt<T>(this IDbCommand dbCmd, string sqlFilter, params object[] filterParams)
        {
            return dbCmd.ConvertToList<T>(
                dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sqlFilter, filterParams));
        }

        internal static List<TModel> Select<TModel>(this IDbCommand dbCmd, Type fromTableType)
        {
            return SelectFmt<TModel>(dbCmd, fromTableType, null);
        }

        internal static List<TModel> SelectFmt<TModel>(this IDbCommand dbCmd, Type fromTableType, string sqlFilter, params object[] filterParams)
        {
            var sql = ToSelectFmt<TModel>(dbCmd.GetDialectProvider(), fromTableType, sqlFilter, filterParams);

            return dbCmd.ConvertToList<TModel>(sql.ToString());
        }

        internal static StringBuilder ToSelectFmt<TModel>(IOrmLiteDialectProvider dialectProvider, Type fromTableType, string sqlFilter, object[] filterParams)
        {
            var sql = new StringBuilder();
            var modelDef = ModelDefinition<TModel>.Definition;
            sql.AppendFormat("SELECT {0} FROM {1}", dialectProvider.GetColumnNames(modelDef),
                             dialectProvider.GetQuotedTableName(fromTableType.GetModelDefinition()));
            if (!string.IsNullOrEmpty(sqlFilter))
            {
                sqlFilter = sqlFilter.SqlFmt(filterParams);
                sql.Append(" WHERE ");
                sql.Append(sqlFilter);
            }
            return sql;
        }

        internal static T SelectByIdFmt<T>(this IDbCommand dbCmd, object idValue)
        {
            return SingleFmt<T>(dbCmd, dbCmd.GetDialectProvider().GetQuotedColumnName(ModelDefinition<T>.PrimaryKeyName) + " = {0}".SqlFmt(idValue));
        }

        internal static T SingleFmt<T>(this IDbCommand dbCmd, string filter, params object[] filterParams)
        {
            return dbCmd.ConvertTo<T>(dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), filter, filterParams));
        }

        [ThreadStatic]
        private static Type lastQueryType;
        internal static void SetFilter<T>(this IDbCommand dbCmd, string name, object value)
        {
            var dialectProvider = dbCmd.GetDialectProvider();

            dbCmd.Parameters.Clear();
            var p = dbCmd.CreateParameter();
            p.ParameterName = name;
            p.Direction = ParameterDirection.Input;
            dialectProvider.InitDbParam(p, value.GetType());

            dbCmd.Parameters.Add(p);
            dbCmd.CommandText = GetFilterSql<T>(dbCmd);
            lastQueryType = typeof(T);
        }

        internal static IDbCommand SetFilters<T>(this IDbCommand dbCmd, object anonType, bool excludeDefaults)
        {
            dbCmd.SetParameters<T>(anonType, excludeDefaults); //needs to be called first
            dbCmd.CommandText = dbCmd.GetFilterSql<T>();
            return dbCmd;
        }

        internal static IDbCommand SetParameters<T>(this IDbCommand dbCmd, object anonType, bool excludeDefaults)
        {
            if (anonType == null)
                return dbCmd;

            dbCmd.Parameters.Clear();
            lastQueryType = null;

            var dialectProvider = dbCmd.GetDialectProvider();
            var fieldMap = typeof(T).IsUserType() //Ensure T != Scalar<int>()
                ? dialectProvider.GetFieldDefinitionMap(typeof(T).GetModelDefinition())
                : null;

            anonType.ForEachParam<T>(excludeDefaults, (pi, columnName, value) =>
            {
                var p = dbCmd.CreateParameter();
                p.ParameterName = columnName;
                p.Direction = ParameterDirection.Input;
                dialectProvider.InitDbParam(p, pi.PropertyType);

                FieldDefinition fieldDef;
                if (fieldMap != null && fieldMap.TryGetValue(columnName, out fieldDef))
                {
                    value = dialectProvider.GetFieldValue(fieldDef, value);
                    var valueType = value != null ? value.GetType() : null;
                    if (valueType != null && valueType != pi.PropertyType)
                        dialectProvider.InitDbParam(p, valueType);
                }
                else
                {
                    value = dialectProvider.GetFieldValue(pi.PropertyType, value);
                    var valueType = value != null ? value.GetType() : null;
                    if (valueType != null && valueType != pi.PropertyType)
                        dialectProvider.InitDbParam(p, valueType);
                }

                p.Value = value == null ?
                    DBNull.Value
                  : p.DbType == DbType.String ?
                    value.ToString() :
                    value;

                dbCmd.Parameters.Add(p);
            });

            return dbCmd;
        }

        internal delegate void ParamIterDelegate(PropertyInfo pi, string columnName, object value);

        internal static void ForEachParam<T>(this object anonType, bool excludeDefaults, ParamIterDelegate fn)
        {
            if (anonType == null) return;

            var pis = anonType.GetType().GetSerializableProperties();
            var model = ModelDefinition<T>.Definition;

            foreach (var pi in pis)
            {
                var mi = pi.GetGetMethod();
                if (mi == null) continue;

                var value = mi.Invoke(anonType, new object[0]);
                if (excludeDefaults && (value == null || value.Equals(pi.PropertyType.GetDefaultValue()))) continue;

                var targetField = model != null ? model.FieldDefinitions.FirstOrDefault(f => String.Equals(f.Name, pi.Name)) : null;
                var columnName = targetField != null && !String.IsNullOrEmpty(targetField.Alias)
                    ? targetField.Alias
                    : pi.Name;

                fn(pi, columnName, value);
            }
        }

        internal static List<string> AllFields<T>(this object anonType)
        {
            var ret = new List<string>();
            ForEachParam<T>(anonType, excludeDefaults: false, fn: (pi, columnName, value) => ret.Add(pi.Name));
            return ret;
        }

        internal static Dictionary<string, object> AllFieldsMap<T>(this object anonType)
        {
            var ret = new Dictionary<string, object>();
            ForEachParam<T>(anonType, excludeDefaults: false, fn: (pi, columnName, value) => ret[pi.Name] = value);
            return ret;
        }

        internal static Dictionary<string, object> NonDefaultsOnly(this Dictionary<string, object> fieldValues)
        {
            var map = new Dictionary<string, object>();
            foreach (var entry in fieldValues)
            {
                if (entry.Value != null)
                {
                    var type = entry.Value.GetType();
                    if (!type.IsValueType || !entry.Value.Equals(type.GetDefaultValue()))
                    {
                        map[entry.Key] = entry.Value;
                    }
                }
            }
            return map;
        }

        internal static List<string> NonDefaultFields<T>(this object anonType)
        {
            var ret = new List<string>();
            ForEachParam<T>(anonType, excludeDefaults: true, fn: (pi, columnName, value) => ret.Add(pi.Name));
            return ret;
        }

        internal static IDbCommand SetParameters(this IDbCommand dbCmd, object anonType, bool excludeDefaults)
        {
            if (anonType == null)
                return dbCmd;

            dbCmd.Parameters.Clear();
            lastQueryType = null;
            var pis = anonType.GetType().GetSerializableProperties();
            var dialectProvider = dbCmd.GetDialectProvider();

            foreach (var pi in pis)
            {
                var mi = pi.GetGetMethod();
                if (mi == null)
                    continue;

                var value = mi.Invoke(anonType, new object[0]);
                if (excludeDefaults && value == null)
                    continue;

                var p = dbCmd.CreateParameter();

                p.ParameterName = pi.Name;
                p.Direction = ParameterDirection.Input;
                p.Value = value ?? DBNull.Value;
                dialectProvider.InitDbParam(p, pi.PropertyType);

                dbCmd.Parameters.Add(p);
            }
            return dbCmd;
        }

        internal static IDbCommand SetParameters(this IDbCommand dbCmd, IDictionary<string, object> dict, bool excludeDefaults)
        {
            if (dict == null)
                return dbCmd;

            dbCmd.Parameters.Clear();
            lastQueryType = null;
            var dialectProvider = dbCmd.GetDialectProvider();

            foreach (var kvp in dict)
            {
                var value = dict[kvp.Key];
                if (excludeDefaults && value == null) continue;
                var p = dbCmd.CreateParameter();
                p.ParameterName = kvp.Key;

                p.Direction = ParameterDirection.Input;
                if (value != null)
                    dialectProvider.InitDbParam(p, value.GetType());
                p.Value = value ?? DBNull.Value;

                dbCmd.Parameters.Add(p);
            }

            return dbCmd;
        }

        public static IDbCommand SetFilters<T>(this IDbCommand dbCmd, object anonType)
        {
            return dbCmd.SetFilters<T>(anonType, excludeDefaults: false);
        }

        public static void ClearFilters(this IDbCommand dbCmd)
        {
            dbCmd.Parameters.Clear();
        }

        internal static string GetFilterSql<T>(this IDbCommand dbCmd)
        {
            var dialectProvider = dbCmd.GetDialectProvider();

            var sb = new StringBuilder();
            foreach (IDbDataParameter p in dbCmd.Parameters)
            {
                if (sb.Length > 0)
                    sb.Append(" AND ");

                var fieldName = p.ParameterName;
                sb.Append(dialectProvider.GetQuotedColumnName(fieldName));

                p.ParameterName = dialectProvider.SanitizeFieldNameForParamName(fieldName);

                sb.Append(" = ");
                sb.Append(dialectProvider.GetParam(p.ParameterName));
            }

            return dialectProvider.ToSelectStatement(typeof(T), sb.ToString());
        }

        internal static bool CanReuseParam<T>(this IDbCommand dbCmd, string paramName)
        {
            return (dbCmd.Parameters.Count == 1
                    && ((IDbDataParameter)dbCmd.Parameters[0]).ParameterName == paramName
                    && lastQueryType != typeof(T));
        }

        internal static List<T> SelectByIds<T>(this IDbCommand dbCmd, IEnumerable idValues)
        {
            var sql = idValues.GetIdsInSql();
            return sql == null
                ? new List<T>()
                : SelectFmt<T>(dbCmd, dbCmd.GetDialectProvider().GetQuotedColumnName(ModelDefinition<T>.PrimaryKeyName) + " IN (" + sql + ")");
        }

        internal static T SingleById<T>(this IDbCommand dbCmd, object value)
        {
            if (!dbCmd.CanReuseParam<T>(ModelDefinition<T>.PrimaryKeyName))
                SetFilter<T>(dbCmd, ModelDefinition<T>.PrimaryKeyName, value);

            ((IDbDataParameter)dbCmd.Parameters[0]).Value = value;

            return dbCmd.ConvertTo<T>();
        }

        internal static T SingleWhere<T>(this IDbCommand dbCmd, string name, object value)
        {
            if (!dbCmd.CanReuseParam<T>(name))
                SetFilter<T>(dbCmd, name, value);

            ((IDbDataParameter)dbCmd.Parameters[0]).Value = value;

            return dbCmd.ConvertTo<T>();
        }

        internal static T Single<T>(this IDbCommand dbCmd, object anonType)
        {
            dbCmd.SetFilters<T>(anonType, excludeDefaults: false);

            return dbCmd.ConvertTo<T>();
        }

        internal static T Single<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams)
        {
            dbCmd.SetParameters(sqlParams);

            return OrmLiteUtils.IsScalar<T>()
                ? dbCmd.Scalar<T>(sql)
                : dbCmd.ConvertTo<T>(dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql));
        }

        internal static T Single<T>(this IDbCommand dbCmd, string sql, object anonType)
        {
            dbCmd.SetParameters<T>(anonType, excludeDefaults: false);

            return OrmLiteUtils.IsScalar<T>()
                ? dbCmd.Scalar<T>(sql)
                : dbCmd.ConvertTo<T>(dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql));
        }

        internal static List<T> Where<T>(this IDbCommand dbCmd, string name, object value)
        {
            if (!dbCmd.CanReuseParam<T>(name))
                SetFilter<T>(dbCmd, name, value);

            ((IDbDataParameter)dbCmd.Parameters[0]).Value = value;

            return dbCmd.ConvertToList<T>();
        }

        internal static List<T> Where<T>(this IDbCommand dbCmd, object anonType)
        {
            dbCmd.SetFilters<T>(anonType);

            return dbCmd.ConvertToList<T>();
        }

        internal static List<T> Select<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams)
        {
            dbCmd.SetParameters(sqlParams).CommandText = dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql);
            return dbCmd.ConvertToList<T>();
        }

        internal static List<T> Select<T>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeDefaults: false);
            dbCmd.CommandText = dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql);

            return dbCmd.ConvertToList<T>();
        }

        internal static List<T> Select<T>(this IDbCommand dbCmd, string sql, Dictionary<string, object> dict)
        {
            if (dict != null) SetParameters(dbCmd, (IDictionary<string, object>)dict, (bool)false);
            dbCmd.CommandText = dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql);

            return dbCmd.ConvertToList<T>();
        }

        internal static List<T> SqlList<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams)
        {
            dbCmd.CommandText = sql;

            return dbCmd.SetParameters(sqlParams).ConvertToList<T>();
        }

        internal static List<T> SqlList<T>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeDefaults: false);
            dbCmd.CommandText = sql;

            return dbCmd.ConvertToList<T>();
        }

        internal static List<T> SqlList<T>(this IDbCommand dbCmd, string sql, Dictionary<string, object> dict)
        {
            if (dict != null) SetParameters(dbCmd, dict, false);
            dbCmd.CommandText = sql;

            return dbCmd.ConvertToList<T>();
        }

        internal static List<T> SqlList<T>(this IDbCommand dbCmd, string sql, Action<IDbCommand> dbCmdFilter)
        {
            if (dbCmdFilter != null) dbCmdFilter(dbCmd);
            dbCmd.CommandText = sql;

            return dbCmd.ConvertToList<T>();
        }

        internal static List<T> SqlColumn<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams)
        {
            dbCmd.SetParameters(sqlParams).CommandText = sql;
            return dbCmd.ConvertToList<T>();
        }

        internal static List<T> SqlColumn<T>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            dbCmd.SetParameters<T>(anonType, excludeDefaults: false).CommandText = sql;
            return dbCmd.ConvertToList<T>();
        }

        internal static List<T> SqlColumn<T>(this IDbCommand dbCmd, string sql, Dictionary<string, object> dict)
        {
            if (dict != null) SetParameters(dbCmd, dict, false);
            dbCmd.CommandText = sql;

            return dbCmd.ConvertToList<T>();
        }

        internal static T SqlScalar<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams)
        {
            return dbCmd.SetParameters(sqlParams).Scalar<T>(sql);
        }

        internal static T SqlScalar<T>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeDefaults: false);

            return dbCmd.Scalar<T>(sql);
        }

        internal static T SqlScalar<T>(this IDbCommand dbCmd, string sql, Dictionary<string, object> dict)
        {
            if (dict != null) SetParameters(dbCmd, dict, false);

            return dbCmd.Scalar<T>(sql);
        }

        internal static List<T> SelectNonDefaults<T>(this IDbCommand dbCmd, object filter)
        {
            dbCmd.SetFilters<T>(filter, excludeDefaults: true);

            return dbCmd.ConvertToList<T>();
        }

        internal static List<T> SelectNonDefaults<T>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeDefaults: true);

            return dbCmd.ConvertToList<T>(dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql));
        }

        internal static IEnumerable<T> SelectLazy<T>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeDefaults: false);
            var dialectProvider = dbCmd.GetDialectProvider();
            dbCmd.CommandText = dialectProvider.ToSelectStatement(typeof(T), sql);

            var resultsFilter = OrmLiteConfig.ResultsFilter;
            if (resultsFilter != null)
            {
                foreach (var item in resultsFilter.GetList<T>(dbCmd))
                {
                    yield return item;
                }
                yield break;
            }

            using (var reader = dbCmd.ExecuteReader())
            {
                var indexCache = reader.GetIndexFieldsCache(ModelDefinition<T>.Definition, dialectProvider);
                var values = new object[reader.FieldCount];
                while (reader.Read())
                {
                    var row = OrmLiteUtils.CreateInstance<T>();
                    row.PopulateWithSqlReader(dialectProvider, reader, indexCache, values);
                    yield return row;
                }
            }
        }

        internal static IEnumerable<T> ColumnLazy<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams)
        {
            foreach (var p in dbCmd.SetParameters(sqlParams).ColumnLazy<T>(sql)) yield return p;
        }

        internal static IEnumerable<T> ColumnLazy<T>(this IDbCommand dbCmd, string sql, object anonType)
        {
            foreach (var p in dbCmd.SetParameters<T>(anonType, excludeDefaults: false).ColumnLazy<T>(sql)) yield return p;
        }

        private static IEnumerable<T> ColumnLazy<T>(this IDbCommand dbCmd, string sql)
        {
            var dialectProvider = dbCmd.GetDialectProvider();
            dbCmd.CommandText = dialectProvider.ToSelectStatement(typeof(T), sql);

            if (OrmLiteConfig.ResultsFilter != null)
            {
                foreach (var item in OrmLiteConfig.ResultsFilter.GetColumn<T>(dbCmd))
                {
                    yield return item;
                }
                yield break;
            }

            using (var reader = dbCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var value = dialectProvider.FromDbValue(reader, 0, typeof(T));
                    if (value == DBNull.Value)
                        yield return default(T);
                    else
                        yield return (T)value;
                }
            }
        }

        internal static IEnumerable<T> WhereLazy<T>(this IDbCommand dbCmd, object anonType)
        {
            dbCmd.SetFilters<T>(anonType);

            if (OrmLiteConfig.ResultsFilter != null)
            {
                foreach (var item in OrmLiteConfig.ResultsFilter.GetList<T>(dbCmd))
                {
                    yield return item;
                }
                yield break;
            }

            var dialectProvider = dbCmd.GetDialectProvider();
            using (var reader = dbCmd.ExecuteReader())
            {
                var indexCache = reader.GetIndexFieldsCache(ModelDefinition<T>.Definition, dialectProvider);
                var values = new object[reader.FieldCount];
                while (reader.Read())
                {
                    var row = OrmLiteUtils.CreateInstance<T>();
                    row.PopulateWithSqlReader(dialectProvider, reader, indexCache, values);
                    yield return row;
                }
            }
        }

        internal static IEnumerable<T> SelectLazy<T>(this IDbCommand dbCmd)
        {
            return SelectLazyFmt<T>(dbCmd, null);
        }

        internal static IEnumerable<T> SelectLazyFmt<T>(this IDbCommand dbCmd, string filter, params object[] filterParams)
        {
            var dialectProvider = dbCmd.GetDialectProvider();
            dbCmd.CommandText = dialectProvider.ToSelectStatement(typeof(T), filter, filterParams);

            if (OrmLiteConfig.ResultsFilter != null)
            {
                foreach (var item in OrmLiteConfig.ResultsFilter.GetList<T>(dbCmd))
                {
                    yield return item;
                }
                yield break;
            }

            using (var reader = dbCmd.ExecReader(dbCmd.CommandText))
            {
                var indexCache = reader.GetIndexFieldsCache(ModelDefinition<T>.Definition, dialectProvider);
                var values = new object[reader.FieldCount];
                while (reader.Read())
                {
                    var row = OrmLiteUtils.CreateInstance<T>();
                    row.PopulateWithSqlReader(dialectProvider, reader, indexCache, values);
                    yield return row;
                }
            }
        }

        internal static T Scalar<T>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeDefaults: false);

            return dbCmd.Scalar<T>(sql);
        }

        internal static T ScalarFmt<T>(this IDbCommand dbCmd, string sql, params object[] sqlParams)
        {
            return dbCmd.Scalar<T>(sql.SqlFmt(sqlParams));
        }

        internal static T Scalar<T>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider)
        {
            while (reader.Read())
            {
                return ToScalar<T>(dialectProvider, reader);
            }

            return default(T);
        }

        internal static T ToScalar<T>(IOrmLiteDialectProvider dialectProvider, IDataReader reader, int columnIndex = 0)
        {
            var nullableType = Nullable.GetUnderlyingType(typeof(T));
            if (nullableType != null)
            {
                object oValue = reader.GetValue(columnIndex);
                if (oValue == DBNull.Value)
                    return default(T);
            }

            var underlyingType = nullableType ?? typeof(T);
            if (underlyingType == typeof(object))
                return (T)reader.GetValue(0);

            var converter = dialectProvider.GetConverterBestMatch(underlyingType);
            if (converter != null)
            {
                object oValue = converter.GetValue(reader, columnIndex, null);
                if (oValue == null)
                    return default(T);

                var convertedValue = converter.FromDbValue(underlyingType, oValue);
                return convertedValue == null ? default(T) : (T)convertedValue;
            }

            return (T)reader.GetValue(0);
        }

        internal static long LastInsertId(this IDbCommand dbCmd)
        {
            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetLastInsertId(dbCmd);
            }

            return dbCmd.GetDialectProvider().GetLastInsertId(dbCmd);
        }

        internal static List<T> Column<T>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            if (anonType != null) dbCmd.SetParameters<T>(anonType, excludeDefaults: false);

            return dbCmd.Column<T>(dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql));
        }

        internal static List<T> ColumnFmt<T>(this IDbCommand dbCmd, string sql, params object[] sqlParams)
        {
            return dbCmd.Column<T>(sql.SqlFmt(sqlParams));
        }

        internal static List<T> Column<T>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider)
        {
            var columValues = new List<T>();

            while (reader.Read())
            {
                var value = dialectProvider.FromDbValue(reader, 0, typeof(T));
                if (value == DBNull.Value)
                    value = default(T);

                columValues.Add((T)value);
            }
            return columValues;
        }

        internal static HashSet<T> ColumnDistinct<T>(this IDbCommand dbCmd, string sql, IEnumerable<IDbDataParameter> sqlParams)
        {
            return dbCmd.SetParameters(sqlParams).ColumnDistinct<T>(sql);
        }

        internal static HashSet<T> ColumnDistinct<T>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            return dbCmd.SetParameters<T>(anonType, excludeDefaults: false).ColumnDistinct<T>(sql);
        }

        internal static HashSet<T> ColumnDistinctFmt<T>(this IDbCommand dbCmd, string sql, params object[] sqlParams)
        {
            return dbCmd.ColumnDistinct<T>(sql.SqlFmt(sqlParams));
        }

        internal static HashSet<T> ColumnDistinct<T>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider)
        {
            var columValues = new HashSet<T>();
            while (reader.Read())
            {
                var value = dialectProvider.FromDbValue(reader, 0, typeof(T));
                if (value == DBNull.Value)
                    value = default(T);

                columValues.Add((T)value);
            }
            return columValues;
        }

        internal static Dictionary<K, List<V>> Lookup<K, V>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            return dbCmd.SetParameters(anonType, false).Lookup<K, V>(sql);
        }

        internal static Dictionary<K, List<V>> LookupFmt<K, V>(this IDbCommand dbCmd, string sql, params object[] sqlParams)
        {
            return dbCmd.Lookup<K, V>(sql.SqlFmt(sqlParams));
        }

        internal static Dictionary<K, List<V>> Lookup<K, V>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider)
        {
            var lookup = new Dictionary<K, List<V>>();

            while (reader.Read())
            {
                var key = (K)dialectProvider.FromDbValue(reader, 0, typeof(K));
                var value = (V)dialectProvider.FromDbValue(reader, 1, typeof(V));

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

        internal static Dictionary<K, V> Dictionary<K, V>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            if (anonType != null) SetParameters(dbCmd, anonType, excludeDefaults: false);

            return dbCmd.Dictionary<K, V>(sql);
        }

        internal static Dictionary<K, V> DictionaryFmt<K, V>(this IDbCommand dbCmd, string sqlFormat, params object[] sqlParams)
        {
            return dbCmd.Dictionary<K, V>(sqlFormat.SqlFmt(sqlParams));
        }

        internal static Dictionary<K, V> Dictionary<K, V>(this IDataReader reader, IOrmLiteDialectProvider dialectProvider)
        {
            var map = new Dictionary<K, V>();

            while (reader.Read())
            {
                var key = (K)dialectProvider.FromDbValue(reader, 0, typeof(K));
                var value = (V)dialectProvider.FromDbValue(reader, 1, typeof(V));

                map.Add(key, value);
            }

            return map;
        }

        internal static bool Exists<T>(this IDbCommand dbCmd, object anonType)
        {
            if (anonType != null) SetParameters(dbCmd, anonType, excludeDefaults: true);

            var sql = dbCmd.GetDialectProvider().ToExecuteProcedureStatement(anonType)
                ?? GetFilterSql<T>(dbCmd);

            var result = dbCmd.Scalar(sql);
            return result != null;
        }

        internal static bool Exists<T>(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            if (anonType != null) SetParameters(dbCmd, anonType, (bool)false);

            var result = dbCmd.Scalar(dbCmd.GetDialectProvider().ToSelectStatement(typeof(T), sql));
            return result != null;
        }

        internal static bool ExistsFmt<T>(this IDbCommand dbCmd, string sqlFilter, params object[] filterParams)
        {
            var fromTableType = typeof(T);
            var result = dbCmd.Scalar(dbCmd.GetDialectProvider().ToSelectStatement(fromTableType, sqlFilter, filterParams));
            return result != null;
        }

        // procedures ...		
        internal static List<TOutputModel> SqlProcedure<TOutputModel>(this IDbCommand dbCommand, object fromObjWithProperties)
        {
            return SqlProcedureFmt<TOutputModel>(dbCommand, fromObjWithProperties, String.Empty);
        }

        internal static List<TOutputModel> SqlProcedureFmt<TOutputModel>(this IDbCommand dbCmd,
            object fromObjWithProperties,
            string sqlFilter,
            params object[] filterParams)
        {
            var modelType = typeof(TOutputModel);

            string sql = dbCmd.GetDialectProvider().ToSelectFromProcedureStatement(
                fromObjWithProperties, modelType, sqlFilter, filterParams);

            return dbCmd.ConvertToList<TOutputModel>(sql);
        }

        public static long LongScalar(this IDbCommand dbCmd)
        {
            var result = dbCmd.ExecuteScalar();
            return ToLong(result);
        }

        internal static long ToLong(object result)
        {
            if (result is DBNull) return default(long);
            if (result is int) return (int)result;
            if (result is decimal) return Convert.ToInt64((decimal)result);
            if (result is ulong) return (long)Convert.ToUInt64(result);
            return (long)result;
        }

        internal static T LoadSingleById<T>(this IDbCommand dbCmd, object value, string[] include = null)
        {
            var row = dbCmd.SingleById<T>(value);
            if (row == null)
                return default(T);

            dbCmd.LoadReferences(row, include);

            return row;
        }

        public static void LoadReferences<T>(this IDbCommand dbCmd, T instance, string[] include = null)
        {
            var loadRef = new LoadReferencesSync<T>(dbCmd, instance);
            var fieldDefs = loadRef.FieldDefs;

            if (!include.IsEmpty())
            {
                // Check that any include values aren't reference fields of the specified type
                var fields = fieldDefs.Select(q => q.FieldName);
                var invalid = include.Except<string>(fields).ToList();
                if (invalid.Count > 0)
                    throw new ArgumentException("Fields '{0}' are not Reference Properties of Type '{1}'".Fmt(invalid.Join("', '"), typeof(T).Name));

                fieldDefs = fieldDefs.Where(fd => include.Contains(fd.FieldName)).ToList();
            }

            foreach (var fieldDef in fieldDefs)
            {
                dbCmd.Parameters.Clear();
                var listInterface = fieldDef.FieldType.GetTypeWithGenericInterfaceOf(typeof(IList<>));
                if (listInterface != null)
                {
                    loadRef.SetRefFieldList(fieldDef, listInterface.GenericTypeArguments()[0]);
                }
                else
                {
                    loadRef.SetRefField(fieldDef, fieldDef.FieldType);
                }
            }
        }

        internal static List<Into> LoadListWithReferences<Into, From>(this IDbCommand dbCmd, SqlExpression<From> expr = null, string[] include = null)
        {
            var loadList = new LoadListSync<Into, From>(dbCmd, expr);

            var fieldDefs = loadList.FieldDefs;
            if (!include.IsEmpty())
            {
                // Check that any include values aren't reference fields of the specified From type
                var fields = fieldDefs.Select(q => q.FieldName);
                var invalid = include.Except<string>(fields).ToList();
                if (invalid.Count > 0)
                    throw new ArgumentException("Fields '{0}' are not Reference Properties of Type '{1}'".Fmt(invalid.Join("', '"), typeof(From).Name));

                fieldDefs = loadList.FieldDefs.Where(fd => include.Contains(fd.FieldName)).ToList();
            }

            foreach (var fieldDef in fieldDefs)
            {
                var listInterface = fieldDef.FieldType.GetTypeWithGenericInterfaceOf(typeof(IList<>));
                if (listInterface != null)
                {
                    loadList.SetRefFieldList(fieldDef, listInterface.GenericTypeArguments()[0]);
                }
                else
                {
                    loadList.SetRefField(fieldDef, fieldDef.FieldType);
                }
            }

            return loadList.ParentResults;
        }

        public static FieldDefinition GetRefFieldDef(this ModelDefinition modelDef, ModelDefinition refModelDef, Type refType)
        {
            var refField = GetRefFieldDefIfExists(modelDef, refModelDef);
            if (refField == null)
                throw new ArgumentException("Cant find '{0}' Property on Type '{1}'".Fmt(modelDef.ModelName + "Id", refType.Name));
            return refField;
        }

        public static FieldDefinition GetRefFieldDefIfExists(this ModelDefinition modelDef, ModelDefinition refModelDef)
        {
            var refField =
                   refModelDef.FieldDefinitions.FirstOrDefault(x => x.ForeignKey != null && x.ForeignKey.ReferenceType == modelDef.ModelType
                       && modelDef.IsRefField(x))
                ?? refModelDef.FieldDefinitions.FirstOrDefault(x => x.ForeignKey != null && x.ForeignKey.ReferenceType == modelDef.ModelType)
                ?? refModelDef.FieldDefinitions.FirstOrDefault(modelDef.IsRefField);

            return refField;
        }

        public static FieldDefinition GetSelfRefFieldDefIfExists(this ModelDefinition modelDef, ModelDefinition refModelDef, FieldDefinition fieldDef)
        {
            var refField = (fieldDef == null ? null
                 : modelDef.FieldDefinitions.FirstOrDefault(x => x.ForeignKey != null && x.ForeignKey.ReferenceType == refModelDef.ModelType
                     && fieldDef.IsSelfRefField(x)))
                ?? modelDef.FieldDefinitions.FirstOrDefault(x => x.ForeignKey != null && x.ForeignKey.ReferenceType == refModelDef.ModelType)
                ?? modelDef.FieldDefinitions.FirstOrDefault(refModelDef.IsRefField);

            return refField;
        }

        public static IDbDataParameter AddParam(this IDbCommand dbCmd,
            string name,
            object value = null,
            ParameterDirection direction = ParameterDirection.Input,
            DbType? dbType = null)
        {
            var p = dbCmd.CreateParam(name, value, direction, dbType);
            dbCmd.Parameters.Add(p);
            return p;
        }

        public static IDbDataParameter CreateParam(this IDbCommand dbCmd,
            string name,
            object value = null,
            ParameterDirection direction = ParameterDirection.Input,
            DbType? dbType = null)
        {
            var p = dbCmd.CreateParameter();
            var dialectProvider = dbCmd.GetDialectProvider();
            p.ParameterName = dialectProvider.GetParam(name);
            p.Direction = direction;

            if (p.DbType == DbType.String)
                p.Size = dialectProvider.GetStringConverter().StringLength;

            if (value != null)
            {
                p.Value = value;
                dialectProvider.InitDbParam(p, value.GetType());
            }

            if (dbType != null)
                p.DbType = dbType.Value;

            return p;
        }

        internal static IDbCommand SqlProc(this IDbCommand dbCmd, string name, object inParams = null, bool excludeDefaults = false)
        {
            dbCmd.CommandType = CommandType.StoredProcedure;
            dbCmd.CommandText = name;
            dbCmd.CommandTimeout = OrmLiteConfig.CommandTimeout;

            dbCmd.SetParameters(inParams, excludeDefaults);

            return dbCmd;
        }
    }
}