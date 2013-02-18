using System;
using System.Data;

namespace ServiceStack.OrmLite
{
    public class OrmLiteTransaction : IDbTransaction
    {
        private readonly IDbTransaction prevTrans;
        private readonly IDbTransaction trans;
        private readonly IDbConnection db;

        public OrmLiteTransaction(IDbConnection db, IDbTransaction trans)
        {
            this.db = db;
            prevTrans = OrmLiteConfig.TSTransaction;
            OrmLiteConfig.TSTransaction = this.trans = trans;
        }

        public void Dispose()
        {
            try
            {
                trans.Dispose();                
            }
            finally
            {
                OrmLiteConfig.TSTransaction = prevTrans;
                var ormLiteDbConn = this.db as OrmLiteConnection;
                if (ormLiteDbConn != null)
                {
                    ormLiteDbConn.Transaction = prevTrans;
                }
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