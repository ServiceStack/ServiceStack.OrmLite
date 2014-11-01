using System;
using System.Data;

namespace ServiceStack.OrmLite
{
    public class OrmLiteTransaction : IDbTransaction
    {
        private readonly IDbTransaction trans;
        private readonly IDbConnection db;

        public OrmLiteTransaction(IDbConnection db, IDbTransaction trans)
        {
            this.db = db;
            this.trans = trans;

            //If OrmLite managed connection assign to connection, otherwise use OrmLiteContext
            var ormLiteConn = this.db as IHasDbTransaction;
            if (ormLiteConn != null)
            {
                ormLiteConn.Transaction = this.trans = trans;
            }
            else
            {
                OrmLiteContext.TSTransaction = this.trans = trans;
            }
        }

        public void Dispose()
        {
            try
            {
                trans.Dispose();
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