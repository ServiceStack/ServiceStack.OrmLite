using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
    public static class ReadConnectionExtensions
    {
        [ThreadStatic]
        internal static string LastCommandText;

        public static SqlExpression<T> SqlExpression<T>(this IDbConnection dbConn) 
        {
            return dbConn.GetDialectProvider().SqlExpression<T>();
        }

        public static T Exec<T>(this IDbConnection dbConn, Func<IDbCommand, T> filter)
        {
            var holdProvider = OrmLiteConfig.TSDialectProvider;
            try
            {
                var ormLiteDbConn = dbConn as OrmLiteConnection;
                if (ormLiteDbConn != null)
                    OrmLiteConfig.TSDialectProvider = ormLiteDbConn.Factory.DialectProvider;

                using (var dbCmd = dbConn.CreateCommand())
                {
                    dbCmd.Transaction = (ormLiteDbConn != null) ? ormLiteDbConn.Transaction : OrmLiteConfig.TSTransaction;
                    dbCmd.CommandTimeout = OrmLiteConfig.CommandTimeout;
                    var ret = filter(dbCmd);
                    LastCommandText = dbCmd.CommandText;
                    return ret;
                }
            }
            finally
            {
                OrmLiteConfig.TSDialectProvider = holdProvider;
            }
        }

        public static void Exec(this IDbConnection dbConn, Action<IDbCommand> filter)
        {
            var dialectProvider = OrmLiteConfig.DialectProvider;
            try
            {
                var ormLiteDbConn = dbConn as OrmLiteConnection;
                if (ormLiteDbConn != null)
                    OrmLiteConfig.DialectProvider = ormLiteDbConn.Factory.DialectProvider;

                using (var dbCmd = dbConn.CreateCommand())
                {
                    dbCmd.Transaction = (ormLiteDbConn != null) ? ormLiteDbConn.Transaction : OrmLiteConfig.TSTransaction;
                    dbCmd.CommandTimeout = OrmLiteConfig.CommandTimeout;

                    filter(dbCmd);
                    LastCommandText = dbCmd.CommandText;
                }
            }
            finally
            {
                OrmLiteConfig.DialectProvider = dialectProvider;
            }
        }

        public static IEnumerable<T> ExecLazy<T>(this IDbConnection dbConn, Func<IDbCommand, IEnumerable<T>> filter)
        {
            var dialectProvider = OrmLiteConfig.DialectProvider;
            IDbCommand dbCmd = null;
            try
            {
                var ormLiteDbConn = dbConn as OrmLiteConnection;
                if (ormLiteDbConn != null)
                    OrmLiteConfig.DialectProvider = ormLiteDbConn.Factory.DialectProvider;

                dbCmd = dbConn.CreateCommand();
                dbCmd.Transaction = (ormLiteDbConn != null) ? ormLiteDbConn.Transaction : OrmLiteConfig.TSTransaction;
                dbCmd.CommandTimeout = OrmLiteConfig.CommandTimeout;

                LastCommandText = null;
                var results = filter(dbCmd);

                foreach (var item in results)
                {
                    yield return item;
                }
            }
            finally
            {
                if (dbCmd != null)
                {
                    LastCommandText = dbCmd.CommandText;
                    dbCmd.Dispose();
                }
                OrmLiteConfig.DialectProvider = dialectProvider;
            }
        }

        public static IDbTransaction OpenTransaction(this IDbConnection dbConn)
        {
            return new OrmLiteTransaction(dbConn, dbConn.BeginTransaction());
        }

        public static IDbTransaction OpenTransaction(this IDbConnection dbConn, IsolationLevel isolationLevel)
        {
            return new OrmLiteTransaction(dbConn, dbConn.BeginTransaction(isolationLevel));
        }

        public static IOrmLiteDialectProvider GetDialectProvider(this IDbConnection dbConn)
        {
            var ormLiteDbConn = dbConn as OrmLiteConnection;
            return ormLiteDbConn != null 
                ? ormLiteDbConn.Factory.DialectProvider 
                : OrmLiteConfig.DialectProvider;
        }

        public static SqlExpression<T> SqlExpression<T>()
        {
            return OrmLiteConfig.DialectProvider.SqlExpression<T>();
        }

        public static List<T> Select<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(expression));
        }

        public static List<T> Select<T>(this IDbConnection dbConn, SqlExpression<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(expression));
        }

        public static List<T> Select<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(predicate));
        }

        public static List<T> SelectFmt<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(predicate));
        }

        public static T Single<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single(predicate));
        }

        public static T Single<T>(this IDbConnection dbConn, SqlExpression<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single(expression));
        }

        public static TKey Scalar<T, TKey>(this IDbConnection dbConn, Expression<Func<T, TKey>> field)
        {
            return dbConn.Exec(dbCmd => dbCmd.Scalar(field));
        }

        public static TKey Scalar<T, TKey>(this IDbConnection dbConn, Expression<Func<T, TKey>> field,
                                             Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.Scalar(field, predicate));
        }

        public static long Count<T>(this IDbConnection dbConn, SqlExpression<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Count(expression));
        }

        public static long Count<T>(this IDbConnection dbConn, Expression<Func<T, bool>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Count(expression));
        }

        public static long Count<T>(this IDbConnection dbConn)
        {
            var expression = OrmLiteConfig.DialectProvider.SqlExpression<T>();
            return dbConn.Exec(dbCmd => dbCmd.Count(expression));
        }
    }
}