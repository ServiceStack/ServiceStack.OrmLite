using System.Data;
using System.Data.Common;
using ServiceStack.Data;

namespace ServiceStack.OrmLite
{
    /// <summary>
    /// Wrapper IDbConnection class to allow for connection sharing, mocking, etc.
    /// </summary>
    public class OrmLiteConnection
        : IDbConnection, IHasDbConnection, IHasDbTransaction
    {
        public readonly OrmLiteConnectionFactory Factory;
        public IDbTransaction Transaction { get; set; }
        private IDbConnection dbConnection;

        public OrmLiteConnection(OrmLiteConnectionFactory factory)
        {
            this.Factory = factory;
        }

        public IDbConnection DbConnection
        {
            get
            {
                if (dbConnection == null)
                {
                    dbConnection = Factory.ConnectionString.ToDbConnection(Factory.DialectProvider);
                }
                return dbConnection;
            }
        }

        public void Dispose()
        {
            if (Factory.OnDispose != null) Factory.OnDispose(this);
            if (!Factory.AutoDisposeConnection) return;

            DbConnection.Dispose();
            dbConnection = null;
        }

        public IDbTransaction BeginTransaction()
        {
            if (Factory.AlwaysReturnTransaction != null)
                return Factory.AlwaysReturnTransaction;

            Transaction = DbConnection.BeginTransaction();
            return Transaction;
        }

        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (Factory.AlwaysReturnTransaction != null)
                return Factory.AlwaysReturnTransaction;

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
            if (Factory.AlwaysReturnCommand != null)
                return Factory.AlwaysReturnCommand;

            var cmd = DbConnection.CreateCommand();
            if (Transaction != null) { cmd.Transaction = Transaction; }
            cmd.CommandTimeout = OrmLiteConfig.CommandTimeout;
            return cmd;
        }

        public void Open()
        {
            if (DbConnection.State == ConnectionState.Broken)
                DbConnection.Close();

            if (DbConnection.State == ConnectionState.Closed)
            {
                DbConnection.Open();
                //so the internal connection is wrapped for example by miniprofiler
                if (Factory.ConnectionFilter != null) { dbConnection = Factory.ConnectionFilter(dbConnection); }
            }
        }

        public string ConnectionString
        {
            get { return Factory.ConnectionString; }
            set { Factory.ConnectionString = value; }
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

        public static explicit operator DbConnection(OrmLiteConnection dbConn)
        {
            return (DbConnection)dbConn.DbConnection;
        }
    }
}