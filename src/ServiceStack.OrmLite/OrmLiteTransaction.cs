using System;
using System.Data;

namespace ServiceStack.OrmLite
{
    public class OrmLiteTransaction : IDbTransaction, IHasDbTransaction
    {
        public IDbTransaction Transaction { get; set; }
        private readonly IDbConnection db;

        public OrmLiteTransaction(IDbConnection db, IDbTransaction transaction)
        {
            this.db = db;
            this.Transaction = transaction;

            //If OrmLite managed connection assign to connection, otherwise use OrmLiteContext
            var ormLiteConn = this.db as IHasDbTransaction;
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
                var ormLiteConn = this.db as IHasDbTransaction;
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

        public IDbConnection Connection
        {
            get { return Transaction.Connection; }
        }

        public IsolationLevel IsolationLevel
        {
            get { return Transaction.IsolationLevel; }
        }
    }
}