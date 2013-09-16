using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using ServiceStack.DataAccess;

namespace ServiceStack.OrmLite
{
    /// <summary>
    /// Wrapper IDbConnection class to manage db connection events
    /// </summary>
    public class OrmLiteConnectionWrapper
        : IDbConnection, IHasDbConnection
    {
        public IDbTransaction Transaction { get; internal set; }
        public bool AutoDisposeConnection { get; set; }
        public Action<OrmLiteConnectionWrapper> OnDispose { get; set; }
        public IDbTransaction AlwaysReturnTransaction { get; set; }
        public IDbCommand AlwaysReturnCommand { get; set; }
        public Func<IDbConnection, IDbConnection> ConnectionFilter { get; set; }
        public IOrmLiteDialectProvider DialectProvider { get; set; }

        private bool isOpen;

        public OrmLiteConnectionWrapper(IDbConnection dbConn)
        {
            this.DbConnection = dbConn;
        }

        public IDbConnection DbConnection { get; set; }

        public void Dispose()
        {
            if (OnDispose != null) OnDispose(this);
            if (!AutoDisposeConnection) return;

            DbConnection.Dispose();
            DbConnection = null;
            isOpen = false;
        }

        public IDbTransaction BeginTransaction()
        {
            if (AlwaysReturnTransaction != null)
                return AlwaysReturnTransaction;

            Transaction = DbConnection.BeginTransaction();
            return Transaction;
        }

        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (AlwaysReturnTransaction != null)
                return AlwaysReturnTransaction;

            Transaction = DbConnection.BeginTransaction(isolationLevel);
            return Transaction;
        }

        public void Close()
        {
            DbConnection.Close();
        }

        public void ChangeDatabase(string databaseName)
        {
            DbConnection.ChangeDatabase(databaseName);
        }

        public IDbCommand CreateCommand()
        {
            if (AlwaysReturnCommand != null)
                return AlwaysReturnCommand;

            var cmd = DbConnection.CreateCommand();
            if(Transaction != null) { cmd.Transaction = Transaction; }
            cmd.CommandTimeout = OrmLiteConfig.CommandTimeout;
            return cmd;
        }

        public void Open()
        {
            if (isOpen) return;
			
            DbConnection.Open();
            //so the internal connection is wrapped for example by miniprofiler
            if (ConnectionFilter != null) { DbConnection = ConnectionFilter(DbConnection); }
            isOpen = true;
        }

        public string ConnectionString
        {
            get { return DbConnection.ConnectionString; }
            set { DbConnection.ConnectionString = value; }
        }

        public int ConnectionTimeout
        {
            get { return DbConnection.ConnectionTimeout; }
        }

        public string Database
        {
            get { return DbConnection.Database; }
        }

        public ConnectionState State
        {
            get { return DbConnection.State; }
        }

        public static explicit operator SqlConnection(OrmLiteConnectionWrapper dbConn)
        {
            return (SqlConnection)dbConn.DbConnection;
        }

        public static explicit operator DbConnection(OrmLiteConnectionWrapper dbConn)
        {
            return (DbConnection)dbConn.DbConnection;
        }
    }
}