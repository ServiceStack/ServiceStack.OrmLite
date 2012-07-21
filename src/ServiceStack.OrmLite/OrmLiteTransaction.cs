using System;
using System.Data;

namespace ServiceStack.OrmLite
{
    public class OrmLiteTransaction : IDbTransaction
    {
        private readonly IDbTransaction prevTrans;
        private readonly IDbTransaction trans;

        public OrmLiteTransaction(IDbTransaction trans)
        {
            prevTrans = OrmLiteConfig.CurrentTransaction;
            OrmLiteConfig.CurrentTransaction = this.trans = trans;
        }

        public void Dispose()
        {
            try
            {
                trans.Dispose();                
            }
            finally 
            {
                OrmLiteConfig.CurrentTransaction = prevTrans;
            }
        }

        public void Commit()
        {
            trans.Commit();
        }

        public void Rollback()
        {
            trans.Rollback();
        }

        public IDbConnection Connection
        {
            get { return trans.Connection; }
        }

        public IsolationLevel IsolationLevel
        {
            get { return trans.IsolationLevel; }
        }
    }
}