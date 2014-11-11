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

        public IOrmLiteDialectProvider DialectProvider { get; set; }
        public string LastCommandText { get; set; }
        public int? CommandTimeout { get; set; }

        public OrmLiteConnection(OrmLiteConnectionFactory factory)
        {
            this.Factory = factory;
            this.DialectProvider = factory.DialectProvider;
        }

        public IDbConnection DbConnection
        {
            get
            {
                if (dbConnection == null)
                {
                    dbConnection = ConnectionString.ToDbConnection(Factory.DialectProvider);
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

            return DbConnection.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (Factory.AlwaysReturnTransaction != null)
                return Factory.AlwaysReturnTransaction;

            return DbConnection.BeginTransaction(isolationLevel);
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
                if (Factory.ConnectionFilter != null)
                    dbConnection = Factory.ConnectionFilter(dbConnection);
            }
        }

        private string connectionString;
        public string ConnectionString
        {
            get { return connectionString ?? Factory.ConnectionString; }
            set { connectionString = value; }
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

        public bool AutoDisposeConnection { get; set; }

        public static explicit operator DbConnection(OrmLiteConnection dbConn)
        {
            return (DbConnection)dbConn.DbConnection;
        }
    }

    internal interface IHasDbTransaction
    {
        IDbTransaction Transaction { get; set; }
    }
}