using System;
using System.Collections.Generic;
using System.Data;
using ServiceStack.Data;
using ServiceStack.Text;

namespace ServiceStack.OrmLite
{
    /// <summary>
    /// Allow for mocking and unit testing by providing non-disposing 
    /// connection factory with injectable IDbCommand and IDbTransaction proxies
    /// </summary>
    public class OrmLiteConnectionFactory : IDbConnectionFactoryExtended
    {
        public OrmLiteConnectionFactory()
            : this(null, null, true) { }

        public OrmLiteConnectionFactory(string connectionString)
            : this(connectionString, null, true) { }

        public OrmLiteConnectionFactory(string connectionString, IOrmLiteDialectProvider dialectProvider)
            : this(connectionString, dialectProvider, true) { }

        public OrmLiteConnectionFactory(string connectionString, IOrmLiteDialectProvider dialectProvider, bool setGlobalDialectProvider)
        {
            ConnectionString = connectionString;
            AutoDisposeConnection = connectionString != ":memory:";
            this.DialectProvider = dialectProvider ?? OrmLiteConfig.DialectProvider;

            if (setGlobalDialectProvider && dialectProvider != null)
            {
                OrmLiteConfig.DialectProvider = dialectProvider;
            }

            this.ConnectionFilter = x => x;

            JsConfig.InitStatics();
        }

        public IOrmLiteDialectProvider DialectProvider { get; set; }

        public string ConnectionString { get; set; }

        public bool AutoDisposeConnection { get; set; }

        public Func<IDbConnection, IDbConnection> ConnectionFilter { get; set; }

        /// <summary>
        /// Force the IDbConnection to always return this IDbCommand
        /// </summary>
        public IDbCommand AlwaysReturnCommand { get; set; }

        /// <summary>
        /// Force the IDbConnection to always return this IDbTransaction
        /// </summary>
        public IDbTransaction AlwaysReturnTransaction { get; set; }

        public Action<OrmLiteConnection> OnDispose { get; set; }

        private OrmLiteConnection ormLiteConnection;
        private OrmLiteConnection OrmLiteConnection
        {
            get
            {
                if (ormLiteConnection == null)
                {
                    ormLiteConnection = new OrmLiteConnection(this);
                }
                return ormLiteConnection;
            }
        }

        public virtual IDbConnection OpenDbConnection()
        {
            var connection = CreateDbConnection();
            connection.Open();

            return connection;
        }

        public virtual IDbConnection CreateDbConnection()
        {
            if (this.ConnectionString == null)
                throw new ArgumentNullException("ConnectionString", "ConnectionString must be set");

            var connection = AutoDisposeConnection
                ? new OrmLiteConnection(this)
                : OrmLiteConnection;

            return connection;
        }

        public virtual IDbConnection OpenDbConnectionString(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");

            var connection = new OrmLiteConnection(this)
            {
                ConnectionString = connectionString
            };

            connection.Open();

            return connection;
        }

        public virtual IDbConnection OpenDbConnectionString(string connectionString, string providerName)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");
            if (providerName == null)
                throw new ArgumentNullException("providerName");

            IOrmLiteDialectProvider dialectProvider;
            if (!DialectProviders.TryGetValue(providerName, out dialectProvider))
                throw new ArgumentException("{0} is not a registered DialectProvider".Fmt(providerName));

            var dbFactory = new OrmLiteConnectionFactory(connectionString, dialectProvider, setGlobalDialectProvider:false);

            return dbFactory.OpenDbConnection();
        }

        public virtual IDbConnection OpenDbConnection(string namedConnection)
        {
            OrmLiteConnectionFactory factory;
            if (!NamedConnections.TryGetValue(namedConnection, out factory))
                throw new KeyNotFoundException("No factory registered is named " + namedConnection);

            IDbConnection connection = factory.AutoDisposeConnection
                ? new OrmLiteConnection(factory)
                : factory.OrmLiteConnection;

            //moved setting up the ConnectionFilter to OrmLiteConnection.Open
            //connection = factory.ConnectionFilter(connection);
            connection.Open();

            return connection;
        }

        private static Dictionary<string, IOrmLiteDialectProvider> dialectProviders;
        public static Dictionary<string, IOrmLiteDialectProvider> DialectProviders
        {
            get
            {
                return dialectProviders ?? (dialectProviders = new Dictionary<string, IOrmLiteDialectProvider>());
            }
        }

        public virtual void RegisterDialectProvider(string providerName, IOrmLiteDialectProvider dialectProvider)
        {
            DialectProviders[providerName] = dialectProvider;
        }

        private static Dictionary<string, OrmLiteConnectionFactory> namedConnections;
        public static Dictionary<string, OrmLiteConnectionFactory> NamedConnections
        {
            get
            {
                return namedConnections ?? (namedConnections = new Dictionary<string, OrmLiteConnectionFactory>());
            }
        }

        public virtual void RegisterConnection(string namedConnection, string connectionString, IOrmLiteDialectProvider dialectProvider)
        {
            RegisterConnection(namedConnection, new OrmLiteConnectionFactory(connectionString, dialectProvider, setGlobalDialectProvider: false));
        }

        public virtual void RegisterConnection(string namedConnection, OrmLiteConnectionFactory connectionFactory)
        {
            NamedConnections[namedConnection] = connectionFactory;
        }
    }

    public static class OrmLiteConnectionFactoryExtensions
    {
        /// <summary>
        /// Alias for OpenDbConnection
        /// </summary>
        public static IDbConnection Open(this IDbConnectionFactory connectionFactory)
        {
            return connectionFactory.OpenDbConnection();
        }

        /// <summary>
        /// Alias for OpenDbConnection
        /// </summary>
        public static IDbConnection Open(this IDbConnectionFactory connectionFactory, string namedConnection)
        {
            return ((OrmLiteConnectionFactory)connectionFactory).OpenDbConnection(namedConnection);
        }

        public static IDbConnection OpenDbConnection(this IDbConnectionFactory connectionFactory, string namedConnection)
        {
            return ((OrmLiteConnectionFactory)connectionFactory).OpenDbConnection(namedConnection);
        }

        public static IDbConnection OpenDbConnectionString(this IDbConnectionFactory connectionFactory, string connectionString)
        {
            return ((OrmLiteConnectionFactory)connectionFactory).OpenDbConnectionString(connectionString);
        }

        public static IDbConnection ToDbConnection(this IDbConnection db)
        {
            var hasDb = db as IHasDbConnection;
            return hasDb != null
                ? hasDb.DbConnection.ToDbConnection()
                : db;
        }

        public static IDbCommand ToDbCommand(this IDbCommand dbCmd)
        {
            var hasDbCmd = dbCmd as IHasDbCommand;
            return hasDbCmd != null
                ? hasDbCmd.DbCommand.ToDbCommand()
                : dbCmd;
        }

        public static IDbTransaction ToDbTransaction(this IDbTransaction dbTrans)
        {
            var hasDbTrans = dbTrans as IHasDbTransaction;
            return hasDbTrans != null
                ? hasDbTrans.Transaction
                : dbTrans;
        }
    }
}