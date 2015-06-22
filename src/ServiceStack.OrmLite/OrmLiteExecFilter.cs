using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite
{
    public interface IOrmLiteExecFilter
    {
        SqlExpression<T> SqlExpression<T>(IDbConnection dbConn);
        IDbCommand CreateCommand(IDbConnection dbConn);
        void DisposeCommand(IDbCommand dbCmd, IDbConnection dbConn);
        T Exec<T>(IDbConnection dbConn, Func<IDbCommand, T> filter);
        IDbCommand Exec(IDbConnection dbConn, Func<IDbCommand, IDbCommand> filter);
        Task<T> Exec<T>(IDbConnection dbConn, Func<IDbCommand, Task<T>> filter);
        Task<IDbCommand> Exec(IDbConnection dbConn, Func<IDbCommand, Task<IDbCommand>> filter);
        void Exec(IDbConnection dbConn, Action<IDbCommand> filter);
        Task Exec(IDbConnection dbConn, Func<IDbCommand, Task> filter);
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
            var ormLiteConn = dbConn as OrmLiteConnection;

            var dbCmd = dbConn.CreateCommand();

            dbCmd.Transaction = ormLiteConn != null 
                ? ormLiteConn.Transaction 
                : OrmLiteContext.TSTransaction;

            dbCmd.CommandTimeout = ormLiteConn != null 
                ? (ormLiteConn.CommandTimeout ?? OrmLiteConfig.CommandTimeout) 
                : OrmLiteConfig.CommandTimeout;

            ormLiteConn.SetLastCommandText(null);

            return new OrmLiteCommand(ormLiteConn, dbCmd);
        }

        public virtual void DisposeCommand(IDbCommand dbCmd, IDbConnection dbConn)
        {
            if (dbCmd == null) return;
            dbConn.SetLastCommandText(dbCmd.CommandText);

            dbCmd.Dispose();
        }

        public virtual T Exec<T>(IDbConnection dbConn, Func<IDbCommand, T> filter)
        {
            var dbCmd = CreateCommand(dbConn);
            try
            {
                var ret = filter(dbCmd);
                return ret;
            }
            finally
            {
                DisposeCommand(dbCmd, dbConn);
            }
        }

        public IDbCommand Exec(IDbConnection dbConn, Func<IDbCommand, IDbCommand> filter)
        {
            var dbCmd = CreateCommand(dbConn);
            var ret = filter(dbCmd);
            if (dbCmd != null)
            {
                dbConn.SetLastCommandText(dbCmd.CommandText);
            }
            return ret;
        }

        public virtual void Exec(IDbConnection dbConn, Action<IDbCommand> filter)
        {
            var dbCmd = CreateCommand(dbConn);
            try
            {
                filter(dbCmd);
            }
            finally
            {
                DisposeCommand(dbCmd, dbConn);
            }
        }

        public virtual Task<T> Exec<T>(IDbConnection dbConn, Func<IDbCommand, Task<T>> filter)
        {
            var dbCmd = CreateCommand(dbConn);

            return filter(dbCmd)
                .Then(t =>
                {
                    DisposeCommand(dbCmd, dbConn);
                    return t;
                });
        }

        public Task<IDbCommand> Exec(IDbConnection dbConn, Func<IDbCommand, Task<IDbCommand>> filter)
        {
            var dbCmd = CreateCommand(dbConn);

            return filter(dbCmd).Then(t => t);
        }

        public virtual Task Exec(IDbConnection dbConn, Func<IDbCommand, Task> filter)
        {
            var dbCmd = CreateCommand(dbConn);

            return filter(dbCmd)
                .Then(t =>
                {
                    DisposeCommand(dbCmd, dbConn);
                    return t;
                });
        }

        public virtual IEnumerable<T> ExecLazy<T>(IDbConnection dbConn, Func<IDbCommand, IEnumerable<T>> filter)
        {
            var dbCmd = CreateCommand(dbConn);
            try
            {
                var results = filter(dbCmd);

                foreach (var item in results)
                {
                    yield return item;
                }
            }
            finally
            {
                DisposeCommand(dbCmd, dbConn);
            }
        }
    }
}