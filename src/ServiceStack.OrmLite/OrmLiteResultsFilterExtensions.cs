// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt


using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite
{
    public static class OrmLiteResultsFilterExtensions
    {
        internal static int ExecNonQuery(this IDbCommand dbCmd, string sql, object anonType = null)
        {
            if (anonType != null)
                dbCmd.SetParameters(anonType, (bool)false);

            dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.ExecuteSql(dbCmd);
            }

            return dbCmd.ExecuteNonQuery();
        }

        internal static int ExecNonQuery(this IDbCommand dbCmd, string sql, IDictionary<string, object> dict)
        {
            if (dict != null)
                dbCmd.SetParameters(dict, (bool)false);

            dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.ExecuteSql(dbCmd);
            }

            return dbCmd.ExecuteNonQuery();
        }

        internal static int ExecNonQuery(this IDbCommand dbCmd)
        {
            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.ExecuteSql(dbCmd);
            }

            return dbCmd.ExecuteNonQuery();
        }

        public static List<T> ConvertToList<T>(this IDbCommand dbCmd, string sql = null)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetList<T>(dbCmd);
            }

            using (var reader = dbCmd.ExecReader(dbCmd.CommandText))
            {
                return reader.ConvertToList<T>(dbCmd.GetDialectProvider());
            }
        }

        public static IList ConvertToList(this IDbCommand dbCmd, Type refType, string sql = null)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetRefList(dbCmd, refType);
            }

            using (var reader = dbCmd.ExecReader(dbCmd.CommandText))
            {
                return reader.ConvertToList(dbCmd.GetDialectProvider(), refType);
            }
        }

        internal static List<T> ExprConvertToList<T>(this IDbCommand dbCmd, string sql = null)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetList<T>(dbCmd);
            }

            using (var reader = dbCmd.ExecReader(dbCmd.CommandText))
            {
                return reader.ConvertToList<T>(dbCmd.GetDialectProvider());
            }
        }

        public static T ConvertTo<T>(this IDbCommand dbCmd, string sql = null)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetSingle<T>(dbCmd);
            }

            using (var reader = dbCmd.ExecReader(dbCmd.CommandText))
            {
                return reader.ConvertTo<T>(dbCmd.GetDialectProvider());
            }
        }

        internal static object ConvertTo(this IDbCommand dbCmd, Type refType, string sql = null)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetRefSingle(dbCmd, refType);
            }

            using (var reader = dbCmd.ExecReader(dbCmd.CommandText))
            {
                return reader.ConvertTo(dbCmd.GetDialectProvider(), refType);
            }
        }

        public static T Scalar<T>(this IDbCommand dbCmd, string sql = null)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetScalar<T>(dbCmd);
            }

            using (var reader = dbCmd.ExecReader(dbCmd.CommandText))
            {
                return reader.Scalar<T>(dbCmd.GetDialectProvider());
            }
        }

        public static object Scalar(this IDbCommand dbCmd, string sql = null)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetScalar(dbCmd);
            }

            return dbCmd.ExecuteScalar();
        }

        internal static long ExecLongScalar(this IDbCommand dbCmd, string sql = null)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetLongScalar(dbCmd);
            }

            return dbCmd.LongScalar();
        }

        internal static T ExprConvertTo<T>(this IDbCommand dbCmd, string sql = null)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetSingle<T>(dbCmd);
            }

            using (var reader = dbCmd.ExecReader(dbCmd.CommandText))
            {
                return reader.ConvertTo<T>(dbCmd.GetDialectProvider());
            }
        }

        internal static List<T> Column<T>(this IDbCommand dbCmd, string sql = null)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetColumn<T>(dbCmd);
            }

            using (var reader = dbCmd.ExecReader(dbCmd.CommandText))
            {
                return reader.Column<T>(dbCmd.GetDialectProvider());
            }
        }

        internal static HashSet<T> ColumnDistinct<T>(this IDbCommand dbCmd, string sql = null)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetColumnDistinct<T>(dbCmd);
            }

            using (var reader = dbCmd.ExecReader(dbCmd.CommandText))
            {
                return reader.ColumnDistinct<T>(dbCmd.GetDialectProvider());
            }
        }

        internal static Dictionary<K, V> Dictionary<K, V>(this IDbCommand dbCmd, string sql = null)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetDictionary<K, V>(dbCmd);
            }

            using (var reader = dbCmd.ExecReader(dbCmd.CommandText))
            {
                return reader.Dictionary<K, V>(dbCmd.GetDialectProvider());
            }
        }

        internal static Dictionary<K, List<V>> Lookup<K, V>(this IDbCommand dbCmd, string sql = null)
        {
            if (sql != null)
                dbCmd.CommandText = sql;

            if (OrmLiteConfig.ResultsFilter != null)
            {
                return OrmLiteConfig.ResultsFilter.GetLookup<K, V>(dbCmd);
            }

            using (var reader = dbCmd.ExecReader(dbCmd.CommandText))
            {
                return reader.Lookup<K, V>(dbCmd.GetDialectProvider());
            }
        }

    }
}