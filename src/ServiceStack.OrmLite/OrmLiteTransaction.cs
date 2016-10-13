using System;
using System.Data;
using ServiceStack.Data;

namespace ServiceStack.OrmLite
{
    public class OrmLiteTransaction : IDbTransaction, IHasDbTransaction
    {
        public IDbTransaction Transaction { get; set; }
        public IDbTransaction DbTransaction => Transaction;

        private readonly IDbConnection db;

        public OrmLiteTransaction(IDbConnection db, IDbTransaction transaction)
        {
            this.db = db;
            this.Transaction = transaction;

            //If OrmLite managed connection assign to connection, otherwise use OrmLiteContext
            var ormLiteConn = this.db as ISetDbTransaction;
            if (ormLiteConn != null)
            {
                ormLiteConn.Transaction = this.Transaction = transaction;
            }
            else
            {
                OrmLiteContext.TSTransaction = this.Transaction = transaction;
            }
        }

        public void Dispose()
        {
            try
            {
                Transaction.Dispose();
            }
            finally
            {
                var ormLiteConn = this.db as ISetDbTransaction;
                if (ormLiteConn != null)
                {
                    ormLiteConn.Transaction = null;
                }
                else
                {
                    OrmLiteContext.TSTransaction = null;
                }
            }
        }

        public void Commit()
        {
            Transaction.Commit();
        }

        public void Rollback()
        {
            Transaction.Rollback();
        }

        public IDbConnection Connection => Transaction.Connection;

        public IsolationLevel IsolationLevel => Transaction.IsolationLevel;
    }
}