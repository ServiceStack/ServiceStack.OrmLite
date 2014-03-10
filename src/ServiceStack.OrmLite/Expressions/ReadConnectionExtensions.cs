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
            IDbCommand dbCmd = null;
            try
            {
                var ormLiteDbConn = dbConn as OrmLiteConnection;
                if (ormLiteDbConn != null)
                    OrmLiteConfig.TSDialectProvider = ormLiteDbConn.Factory.DialectProvider;

                dbCmd = dbConn.CreateCommand();
                dbCmd.Transaction = (ormLiteDbConn != null) ? ormLiteDbConn.Transaction : OrmLiteConfig.TSTransaction;
                dbCmd.CommandTimeout = OrmLiteConfig.CommandTimeout;

                var ret = filter(dbCmd);
                return ret;
            }
            finally
            {
                if (dbCmd != null)
                {
                    LastCommandText = dbCmd.CommandText;
                    dbCmd.Dispose();
                }
                OrmLiteConfig.TSDialectProvider = holdProvider;
            }
        }

        public static void Exec(this IDbConnection dbConn, Action<IDbCommand> filter)
        {
            var holdProvider = OrmLiteConfig.DialectProvider;
            IDbCommand dbCmd = null;
            try
            {
                var ormLiteDbConn = dbConn as OrmLiteConnection;
                if (ormLiteDbConn != null)
                    OrmLiteConfig.DialectProvider = ormLiteDbConn.Factory.DialectProvider;

                dbCmd = dbConn.CreateCommand();

                dbCmd.Transaction = (ormLiteDbConn != null) ? ormLiteDbConn.Transaction : OrmLiteConfig.TSTransaction;
                dbCmd.CommandTimeout = OrmLiteConfig.CommandTimeout;

                filter(dbCmd);
            }
            finally
            {
                if (dbCmd != null)
                {
                    LastCommandText = dbCmd.CommandText;
                    dbCmd.Dispose();
                }
                OrmLiteConfig.DialectProvider = holdProvider;
            }
        }

        public static IEnumerable<T> ExecLazy<T>(this IDbConnection dbConn, Func<IDbCommand, IEnumerable<T>> filter)
        {
            var holdProvider = OrmLiteConfig.DialectProvider;
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
                OrmLiteConfig.DialectProvider = holdProvider;
            }
        }

        /// <summary>
        /// Open a Transaction in OrmLite
        /// </summary>
        public static IDbTransaction OpenTransaction(this IDbConnection dbConn)
        {
            return new OrmLiteTransaction(dbConn, dbConn.BeginTransaction());
        }

        /// <summary>
        /// Open a Transaction in OrmLite
        /// </summary>
        public static IDbTransaction OpenTransaction(this IDbConnection dbConn, IsolationLevel isolationLevel)
        {
            return new OrmLiteTransaction(dbConn, dbConn.BeginTransaction(isolationLevel));
        }

        /// <summary>
        /// Return the IOrmLiteDialectProvider on this connection.
        /// </summary>
        public static IOrmLiteDialectProvider GetDialectProvider(this IDbConnection dbConn)
        {
            var ormLiteDbConn = dbConn as OrmLiteConnection;
            return ormLiteDbConn != null
                ? ormLiteDbConn.Factory.DialectProvider
                : OrmLiteConfig.DialectProvider;
        }

        /// <summary>
        /// Create a new SqlExpression builder allowing typed LINQ-like queries.
        /// </summary>
        public static SqlExpression<T> SqlExpression<T>()
        {
            return OrmLiteConfig.DialectProvider.SqlExpression<T>();
        }

        /// <summary>
        /// Returns results from using a LINQ Expression. E.g:
        /// <para>db.Select&lt;Person&gt;(x =&gt; x.Age &gt; 40)</para>
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(predicate));
        }

        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select&lt;Person&gt;(q =&gt; q.Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(expression));
        }

        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select(db.SqlExpression&lt;Person&gt;().Where(x =&gt; x.Age &gt; 40))</para>
        /// </summary>
        public static List<T> Select<T>(this IDbConnection dbConn, SqlExpression<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Select(expression));
        }

        /// <summary>
        /// Returns a single result from using a LINQ Expression. E.g:
        /// <para>db.Single&lt;Person&gt;(x =&gt; x.Age == 42)</para>
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single(predicate));
        }

        /// <summary>
        /// Returns a single result from using an SqlExpression lambda. E.g:
        /// <para>db.Single&lt;Person&gt;(q =&gt; q.Where(x =&gt; x.Age == 42))</para>
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single(expression));
        }

        /// <summary>
        /// Returns results from using an SqlExpression lambda. E.g:
        /// <para>db.Select&lt;Person&gt;(x =&gt; x.Age &gt; 40)</para>
        /// </summary>
        public static T Single<T>(this IDbConnection dbConn, SqlExpression<T> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Single(expression));
        }

        /// <summary>
        /// Returns a scalar result from using an SqlExpression lambda. E.g:
        /// <para>db.Scalar&lt;Person, int&gt;(x =&gt; Sql.Max(x.Age))</para>
        /// </summary>
        public static TKey Scalar<T, TKey>(this IDbConnection dbConn, Expression<Func<T, TKey>> field)
        {
            return dbConn.Exec(dbCmd => dbCmd.Scalar(field));
        }

        /// <summary>
        /// Returns a scalar result from using an SqlExpression lambda. E.g:
        /// <para>db.Scalar&lt;Person, int&gt;(x =&gt; Sql.Max(x.Age), , x =&gt; x.Age &lt; 50)</para>
        /// </summary>        
        public static TKey Scalar<T, TKey>(this IDbConnection dbConn,
            Expression<Func<T, TKey>> field, Expression<Func<T, bool>> predicate)
        {
            return dbConn.Exec(dbCmd => dbCmd.Scalar(field, predicate));
        }

        /// <summary>
        /// Returns the count of rows that match the LINQ expression, E.g:
        /// <para>db.Count&lt;Person&gt;(x =&gt; x.Age &lt; 50)</para>
        /// </summary>
        public static long Count<T>(this IDbConnection dbConn, Expression<Func<T, bool>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Count(expression));
        }

        /// <summary>
        /// Returns the count of rows that match the SqlExpression lambda, E.g:
        /// <para>db.Count&lt;Person&gt;(q =&gt; q.Where(x =&gt; x.Age &lt; 50))</para>
        /// </summary>
        public static long Count<T>(this IDbConnection dbConn, Func<SqlExpression<T>, SqlExpression<T>> expression)
        {
            return dbConn.Exec(dbCmd => dbCmd.Count(expression));
        }

        /// <summary>
        /// Returns the count of rows that match the supplied SqlExpression, E.g:
        /// <para>db.Count(db.SqlExpression&lt;Person&gt;().Where(x =&gt; x.Age &lt; 50))</para>
        /// </summary>
        public static long Count<T>(this IDbConnection dbConn, SqlExpression<T> expression)
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