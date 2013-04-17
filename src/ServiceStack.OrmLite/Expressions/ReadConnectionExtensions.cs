﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace ServiceStack.OrmLite
{
    public static class ReadConnectionExtensions
    {
        [ThreadStatic]
        internal static string LastCommandText;

        public static SqlExpressionVisitor<T> CreateExpression<T>(this IDbConnection dbConn) 
        {
            return dbConn.GetDialectProvider().ExpressionVisitor<T>();
        }

        private static bool LockNeeded(IDbConnection dbConn)
        {
            var currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            return dbConn is OrmLiteConnection &&
                   ((OrmLiteConnection)dbConn).TransactionThreadId != currentThreadId;
        }

        public static T Exec<T>(this IDbConnection dbConn, Func<IDbCommand, T> filter)
        {
            Func<T> func = () =>
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

                            var ret = filter(dbCmd);
                            LastCommandText = dbCmd.CommandText;
                            return ret;
                        }
                    }
                    finally
                    {
                        OrmLiteConfig.TSDialectProvider = holdProvider;
                    }
                };

            if (LockNeeded(dbConn))
                lock (dbConn)
                    return func.Invoke();

            return func.Invoke();
        }

        public static void Exec(this IDbConnection dbConn, Action<IDbCommand> filter)
        {
            Action func = () =>
                {
                    var dialectProvider = OrmLiteConfig.DialectProvider;
                    try
                    {
                        var ormLiteDbConn = dbConn as OrmLiteConnection;
                        if (ormLiteDbConn != null)
                            OrmLiteConfig.DialectProvider = ormLiteDbConn.Factory.DialectProvider;

                        using (var dbCmd = dbConn.CreateCommand())
                        {
                            dbCmd.Transaction = (ormLiteDbConn != null)
                                                    ? ormLiteDbConn.Transaction
                                                    : OrmLiteConfig.TSTransaction;

                            filter(dbCmd);
                            LastCommandText = dbCmd.CommandText;
                        }
                    }
                    finally
                    {
                        OrmLiteConfig.DialectProvider = dialectProvider;
                    }
                };

            if (LockNeeded(dbConn))
                lock (dbConn)
                {
                    func.Invoke();
                    return;
                }
                    
            func.Invoke();
        }

        private static IEnumerable<T> ExecLazyMain<T>(this IDbConnection dbConn, Func<IDbCommand, IEnumerable<T>> filter)
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

                    var results = filter(dbCmd);
                    LastCommandText = dbCmd.CommandText;
                    foreach (var item in results)
                    {
                        yield return item;
                    }
                }
            }
            finally
            {
                OrmLiteConfig.DialectProvider = dialectProvider;
            }
        }

        public static IEnumerable<T> ExecLazy<T>(this IDbConnection dbConn, Func<IDbCommand, IEnumerable<T>> filter)
        {
            if (LockNeeded(dbConn))
                lock (dbConn)
                    return ExecLazyMain<T>(dbConn, filter);

            return ExecLazyMain<T>(dbConn, filter);
        }

        public static void UseTransaction(this IDbConnection dbConn, Action<IDbTransaction> action, IsolationLevel? isolationLevel = null)
        {
            var currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (dbConn is OrmLiteConnection && ((OrmLiteConnection) dbConn).TransactionThreadId != currentThreadId)
            {
                lock (dbConn)
                {
                    using (var trans = (null == isolationLevel ? dbConn.OpenTransaction() : dbConn.OpenTransaction(isolationLevel.Value)))
                    {
                        ((OrmLiteConnection) dbConn).TransactionThreadId = currentThreadId;
                        action.Invoke(trans);
                        ((OrmLiteConnection)dbConn).TransactionThreadId = -1;
                    }   
                }
            }
            else
            {
                using (var trans = (null == isolationLevel ? dbConn.OpenTransaction() : dbConn.OpenTransaction(isolationLevel.Value)))
                {
                    action.Invoke(trans);
                }
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

        public static SqlExpressionVisitor<T> CreateExpression<T>()
        {
            return OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
        }

        public static List<T> Select<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(predicate));
        }

        public static List<T> Select<T>(this IDbConnection dbConn, Func<SqlExpressionVisitor<T>, SqlExpressionVisitor<T>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(expression));
        }

        public static List<T> Select<T>(this IDbConnection dbConn, SqlExpressionVisitor<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(expression));
        }

        /// <summary>
        /// Performs the same function as Select() except arguments are passed as parameters to the generated SQL.
        /// Currently does not support complex SQL.## ,  .StartsWith(), EndsWith() and Contains() operators
        /// </summary>
        public static List<T> SelectParam<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.SelectParam(predicate));
        }

        public static T First<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.First(predicate));
        }

        public static T First<T>(this IDbConnection dbConn, SqlExpressionVisitor<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.First(expression));
        }

        public static T FirstOrDefault<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.FirstOrDefault(predicate));
        }

        public static T FirstOrDefault<T>(this IDbConnection dbConn, SqlExpressionVisitor<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.FirstOrDefault(expression));
        }

        public static TKey GetScalar<T, TKey>(this IDbConnection dbConn, Expression<Func<T, TKey>> field)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetScalar(field));
        }

        public static TKey GetScalar<T, TKey>(this IDbConnection dbConn, Expression<Func<T, TKey>> field,
                                             Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.GetScalar(field, predicate));
        }

        public static long Count<T>(this IDbConnection dbConn, SqlExpressionVisitor<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Count(expression));
        }

        public static long Count<T>(this IDbConnection dbConn, Expression<Func<T, bool>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Count(expression));
        }

        public static long Count<T>(this IDbConnection dbConn)
        {
            SqlExpressionVisitor<T> expression = OrmLiteConfig.DialectProvider.ExpressionVisitor<T>();
            return dbConn.Exec(dbCmd => dbCmd.Count(expression));
        }
    }
}