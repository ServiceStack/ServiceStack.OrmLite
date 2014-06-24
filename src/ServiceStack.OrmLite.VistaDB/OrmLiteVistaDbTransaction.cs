using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ServiceStack.OrmLite.VistaDB
{
    public class OrmLiteVistaDbTransaction : IDbTransaction
    {
        public IDbTransaction VistaDbTransaction { get; private set; }

        private bool _finalized, _disposed;

        public OrmLiteVistaDbTransaction(OrmLiteVistaDbConnection connectionWrapper, IDbTransaction transaction)
        {
            if (connectionWrapper == null)
                throw new ArgumentNullException("connectionWrapper");

            if (transaction == null)
                throw new ArgumentNullException("transaction");

            _finalized = _disposed = false;

            this.Connection = connectionWrapper;
            this.VistaDbTransaction = transaction;
        }

        #region IDbTransaction Members

        public void Commit()
        {
            this.VistaDbTransaction.Commit();
            _finalized = true;
        }

        public IDbConnection Connection { get; private set; }

        public IsolationLevel IsolationLevel { get { return this.VistaDbTransaction.IsolationLevel; } }

        public void Rollback()
        {
            this.VistaDbTransaction.Rollback();
            _finalized = true;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {            
            if (!_finalized && !_disposed)
            {
                try
                {
                    this.Rollback();
                    this.Connection = null;
                    _disposed = true;
                }
                catch { }
            }

            this.VistaDbTransaction.Dispose();
        }

        #endregion
    }
}
