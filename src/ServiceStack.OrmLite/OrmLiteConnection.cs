using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Data;

namespace ServiceStack.OrmLite
{
    /// <summary>
    /// Wrapper IDbConnection class to allow for connection sharing, mocking, etc.
    /// </summary>
    public class OrmLiteConnection
        : IDbConnection, IHasDbConnection, IHasDbTransaction, ISetDbTransaction, IHasDialectProvider
    {
        public readonly OrmLiteConnectionFactory Factory;
        public IDbTransaction Transaction { get; set; }
        public IDbTransaction DbTransaction => Transaction;
        private IDbConnection dbConnection;

        public IOrmLiteDialectProvider DialectProvider { get; set; }
        public string LastCommandText { get; set; }
        public int? CommandTimeout { get; set; }

        public OrmLiteConnection(OrmLiteConnectionFactory factory)
        {
            this.Factory = factory;
            this.DialectProvider = factory.DialectProvider;
        }

        public IDbConnection DbConnection => dbConnection ?? (dbConnection = ConnectionString.ToDbConnection(Factory.DialectProvider));

        public void Dispose()
        {
            Factory.OnDispose?.Invoke(this);
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

                DialectProvider.OnOpenConnection?.Invoke(dbConnection);
            }
        }

        public async Task OpenAsync(CancellationToken token = default(CancellationToken))
        {
            if (DbConnection.State == ConnectionState.Broken)
                DbConnection.Close();

            if (DbConnection.State == ConnectionState.Closed)
            {
                await DialectProvider.OpenAsync(DbConnection, token);
                //so the internal connection is wrapped for example by miniprofiler
                if (Factory.ConnectionFilter != null)
                    dbConnection = Factory.ConnectionFilter(dbConnection);

                DialectProvider.OnOpenConnection?.Invoke(dbConnection);
            }
        }

        private string connectionString;
        public string ConnectionString
        {
            get => connectionString ?? Factory.ConnectionString;
            set => connectionString = value;
        }

        public int ConnectionTimeout => DbConnection.ConnectionTimeout;

        public string Database => DbConnection.Database;

        public ConnectionState State => DbConnection.State;

        public bool AutoDisposeConnection { get; set; }

        public static explicit operator DbConnection(OrmLiteConnection dbConn)
        {
            return (DbConnection)dbConn.DbConnection;
        }
    }

    internal interface ISetDbTransaction
    {
        IDbTransaction Transaction { get; set; }
    }
}