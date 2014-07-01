using System;
using System.Data;

namespace ServiceStack.OrmLite.VistaDB
{
    public class OrmLiteVistaDbConnection : IDbConnection, ICloneable
    {
        public IDbConnection VistaDbConnection { get; private set; }

        public OrmLiteVistaDbConnection(IDbConnection conn)
        {
            if (conn == null)
                throw new ArgumentNullException("conn");

            VistaDbConnection = conn;
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            //VistaDB 4 supports only ReadCommited isolation level
            if (il != IsolationLevel.Unspecified && il != IsolationLevel.ReadCommitted)
                il = IsolationLevel.ReadCommitted;

            var tn = VistaDbConnection.BeginTransaction(il);
            return new OrmLiteVistaDbTransaction(this, tn);
        }

        public IDbTransaction BeginTransaction()
        {
            var tn = this.VistaDbConnection.BeginTransaction();

            return new OrmLiteVistaDbTransaction(this, tn);
        }

        public void ChangeDatabase(string databaseName)
        {
            VistaDbConnection.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            VistaDbConnection.Close();
        }

        public string ConnectionString
        {
            get { return VistaDbConnection.ConnectionString; }
            set { VistaDbConnection.ConnectionString = value; }
        }

        public int ConnectionTimeout
        {
            get { return VistaDbConnection.ConnectionTimeout; }
        }

        public IDbCommand CreateCommand()
        {
            var cmd = VistaDbConnection.CreateCommand();

            return new OrmLiteVistaDbCommand(this, cmd);
        }

        public string Database
        {
            get { return VistaDbConnection.Database; }
        }

        public void Open()
        {
            VistaDbConnection.Open();
        }

        public ConnectionState State
        {
            get { return VistaDbConnection.State; }
        }

        public void Dispose()
        {
            VistaDbConnection.Dispose();
        }

        public object Clone()
        {
            var cloneable = (ICloneable) VistaDbConnection;
            var conn = (IDbConnection) cloneable.Clone();

            return new OrmLiteVistaDbConnection(conn);
        }
    }
}
