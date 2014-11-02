using System;
using System.Data;
using System.Data.Common;
using System.IO;
using NUnit.Framework;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Tests
{
    public class Config
    {
        public static string SqliteMemoryDb = ":memory:";
        public static string SqliteFileDir = "~/App_Data/".MapAbsolutePath();
        public static string SqliteFileDb = "~/App_Data/db.sqlite".MapAbsolutePath();
        public static string SqlServerDb = "~/App_Data/Database1.mdf".MapAbsolutePath();
        public static string SqlServerBuildDb = "Server={0};Database=test;User Id=test;Password=test;".Fmt(Environment.GetEnvironmentVariable("CI_HOST"));
        //public static string SqlServerBuildDb = "Data Source=localhost;Initial Catalog=TestDb;Integrated Security=SSPI;Connect Timeout=120;MultipleActiveResultSets=True";

        public static string MySqlDb = "Server={0};Database=test;UID=root;Password=test".Fmt(Environment.GetEnvironmentVariable("CI_HOST"));

        public static IOrmLiteDialectProvider DefaultProvider = SqlServerDialect.Provider;
        public static string DefaultConnection = SqlServerBuildDb;

        public static string GetDefaultConnection()
        {
            OrmLiteConfig.DialectProvider = DefaultProvider;
            return DefaultConnection;
        }

        public static IDbConnection OpenDbConnection()
        {
            return GetDefaultConnection().OpenDbConnection();
        }
    }

	public class OrmLiteTestBase
	{
	    protected virtual string ConnectionString { get; set; }

	    public OrmLiteTestBase() {}

	    public OrmLiteTestBase(Dialect dialect)
	    {
	        Dialect = dialect;
            Init();
        }

	    protected string GetConnectionString()
		{
			return GetFileConnectionString();
		}

        public static OrmLiteConnectionFactory CreateSqliteMemoryDbFactory()
        {
            var dbFactory = new OrmLiteConnectionFactory(Config.SqliteMemoryDb, SqliteDialect.Provider);
            return dbFactory;
        }

        public static OrmLiteConnectionFactory CreateSqlServerDbFactory()
        {
            var dbFactory = new OrmLiteConnectionFactory(Config.SqlServerBuildDb, SqlServerDialect.Provider);
            return dbFactory;
        }

        public static OrmLiteConnectionFactory CreateMySqlDbFactory()
        {
            var dbFactory = new OrmLiteConnectionFactory(Config.MySqlDb, MySqlDialect.Provider);
            return dbFactory;
        }

	    protected virtual string GetFileConnectionString()
		{
            var connectionString = Config.SqliteFileDb;
			if (File.Exists(connectionString))
				File.Delete(connectionString);

			return connectionString;
		}

		protected void CreateNewDatabase()
		{
			if (ConnectionString.Contains(".sqlite"))
				ConnectionString = GetFileConnectionString();
		}

        public Dialect Dialect = Dialect.SqlServer;
	    protected OrmLiteConnectionFactory DbFactory;

        OrmLiteConnectionFactory Init(string connStr, IOrmLiteDialectProvider dialectProvider)
        {
            ConnectionString = connStr;
            OrmLiteConfig.DialectProvider = dialectProvider;
            DbFactory = new OrmLiteConnectionFactory(ConnectionString, dialectProvider);
            return DbFactory;
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Init();
        }

        private OrmLiteConnectionFactory Init()
        {
	        LogManager.LogFactory = new ConsoleLogFactory(debugEnabled: false);

	        switch (Dialect)
	        {
                case Dialect.Sqlite:
                    var dbFactory = Init(Config.SqliteMemoryDb, SqliteDialect.Provider);
                    dbFactory.AutoDisposeConnection = false;
                    return dbFactory;
                case Dialect.SqlServer:
                    return Init(Config.SqlServerBuildDb, SqlServerDialect.Provider);
                case Dialect.MySql:
                    return Init(Config.MySqlDb, MySqlDialect.Provider);
            }

            throw new NotImplementedException("{0}".Fmt(Dialect));
	    }

	    public void Log(string text)
		{
			Console.WriteLine(text);
		}

        public IDbConnection InMemoryDbConnection { get; set; }

        public virtual IDbConnection OpenDbConnection()
        {
            if (ConnectionString == ":memory:")
            {
                if (InMemoryDbConnection == null || DbFactory.AutoDisposeConnection)
                {
                    InMemoryDbConnection = new OrmLiteConnection(DbFactory);
                    InMemoryDbConnection.Open();
                }
                return InMemoryDbConnection;
            }

            return DbFactory.OpenDbConnection();
        }

        protected void SuppressIfOracle(string reason, params object[] args)
        {
            // Not Oracle if this base class used
        }
	}
}