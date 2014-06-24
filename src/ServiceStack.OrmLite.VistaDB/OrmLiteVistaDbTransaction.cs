using System;
using System.Data;

namespace ServiceStack.OrmLite.VistaDB
{
    public class OrmLiteVistaDbTransaction : IDbTransaction
    {
        public IDbTransaction VistaDbTransaction { get; private set; }

        private bool finalized, disposed;

        public OrmLiteVistaDbTransaction(OrmLiteVistaDbConnection connectionWrapper, IDbTransaction transaction)
        {
            if (connectionWrapper == null)
                throw new ArgumentNullException("connectionWrapper");

            if (transaction == null)
                throw new ArgumentNullException("transaction");

            finalized = disposed = false;

            this.Connection = connectionWrapper;
            this.VistaDbTransaction = transaction;
        }

        public void Commit()
        {
            this.VistaDbTransaction.Commit();
            finalized = true;
        }

        public IDbConnection Connection { get; private set; }

        public IsolationLevel IsolationLevel { get { return this.VistaDbTransaction.IsolationLevel; } }

        public void Rollback()
        {
            this.VistaDbTransaction.Rollback();
            finalized = true;
        }

        public void Dispose()
        {            
            if (!finalized && !disposed)
            {
                try
                {
                    this.Rollback();
                    this.Connection = null;
                    disposed = true;
                }
                catch { }
            }

            this.VistaDbTransaction.Dispose();
        }
    }
}
