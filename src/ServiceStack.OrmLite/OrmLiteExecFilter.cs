using System;
using System.Collections.Generic;
using System.Data;

namespace ServiceStack.OrmLite
{
    public interface IOrmLiteExecFilter
    {
        SqlExpression<T> SqlExpression<T>(IDbConnection dbConn);
        IDbCommand CreateCommand(IDbConnection dbConn);
        void DisposeCommand(IDbCommand dbCmd, IOrmLiteDialectProvider holdProvider);
        T Exec<T>(IDbConnection dbConn, Func<IDbCommand, T> filter);
        void Exec(IDbConnection dbConn, Action<IDbCommand> filter);
        IEnumerable<T> ExecLazy<T>(IDbConnection dbConn, Func<IDbCommand, IEnumerable<T>> filter);
    }

    public class OrmLiteExecFilter : IOrmLiteExecFilter
    {
        public virtual SqlExpression<T> SqlExpression<T>(IDbConnection dbConn)
        {
            return dbConn.GetDialectProvider().SqlExpression<T>();
        }

        public virtual IDbCommand CreateCommand(IDbConnection dbConn)
        {
            var ormLiteDbConn = dbConn as OrmLiteConnection;
            if (ormLiteDbConn != null)
                OrmLiteConfig.DialectProvider = ormLiteDbConn.Factory.DialectProvider;

            var dbCmd = dbConn.CreateCommand();
            dbCmd.Transaction = (ormLiteDbConn != null) ? ormLiteDbConn.Transaction : OrmLiteConfig.TSTransaction;
            dbCmd.CommandTimeout = OrmLiteConfig.CommandTimeout;
            ReadConnectionExtensions.LastCommandText = null;
            return dbCmd;
        }

        public virtual void DisposeCommand(IDbCommand dbCmd, IOrmLiteDialectProvider holdProvider)
        {
            if (dbCmd != null)
            {
                ReadConnectionExtensions.LastCommandText = dbCmd.CommandText;
                dbCmd.Dispose();
            }
            OrmLiteConfig.TSDialectProvider = holdProvider;
        }

        public virtual T Exec<T>(IDbConnection dbConn, Func<IDbCommand, T> filter)
        {
            var holdProvider = OrmLiteConfig.TSDialectProvider;
            IDbCommand dbCmd = null;
            try
            {
                dbCmd = CreateCommand(dbConn);

                var ret = filter(dbCmd);
                return ret;
            }
            finally
            {
                DisposeCommand(dbCmd, holdProvider);
            }
        }

        public virtual void Exec(IDbConnection dbConn, Action<IDbCommand> filter)
        {
            var holdProvider = OrmLiteConfig.DialectProvider;
            IDbCommand dbCmd = null;
            try
            {
                dbCmd = CreateCommand(dbConn);

                filter(dbCmd);
            }
            finally
            {
                DisposeCommand(dbCmd, holdProvider);
            }
        }

        public virtual IEnumerable<T> ExecLazy<T>(IDbConnection dbConn, Func<IDbCommand, IEnumerable<T>> filter)
        {
            var holdProvider = OrmLiteConfig.DialectProvider;
            IDbCommand dbCmd = null;
            try
            {
                dbCmd = CreateCommand(dbConn);

                var results = filter(dbCmd);

                foreach (var item in results)
                {
                    yield return item;
                }
            }
            finally
            {
                DisposeCommand(dbCmd, holdProvider);
            }
        }
    }
}