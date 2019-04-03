using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Tests
{
    public class Config
    {
        public static Dialect DefaultDialect = Dialect.Sqlite;
        public const bool EnableDebugLogging = false;

        public static string SqliteMemoryDb = Environment.GetEnvironmentVariable("SQLITE_CONNECTION") ?? ":memory:";
        public static string SqliteFileDir = "~/App_Data/".MapAbsolutePath();
        public static string SqliteFileDb = "~/App_Data/db.sqlite".MapAbsolutePath();
        public static string SqlServerDb = "~/App_Data/Database1.mdf".MapAbsolutePath();
        public static string SqlServerBuildDb = Environment.GetEnvironmentVariable("MSSQL_CONNECTION") ?? "Data Source=tcp:localhost,48501\\SQLExpress;Initial Catalog=master;User Id=sa;Password=Test!tesT;Connect Timeout=120;MultipleActiveResultSets=True;";
        //public static string SqlServerBuildDb = "Data Source=localhost;Initial Catalog=TestDb;Integrated Security=SSPI;Connect Timeout=120;MultipleActiveResultSets=True";

        public static string OracleDb = Environment.GetEnvironmentVariable("ORACLE_CONNECTION") ?? "Data Source=localhost:48401/XE;User ID=system;Password=test";
        public static string MySqlDb_5_5 = Environment.GetEnvironmentVariable("MYSQL_CONNECTION") ?? "Server=localhost;Port=48201;Database=test;UID=root;Password=test;SslMode=none";
        public static string MySqlDb_10_1 = "Server=localhost;Port=48202;Database=test;UID=root;Password=test;SslMode=none";
        public static string MySqlDb_10_2 = "Server=localhost;Port=48203;Database=test;UID=root;Password=test;SslMode=none";
        public static string MySqlDb_10_3 = "Server=localhost;Port=48204;Database=test;UID=root;Password=test;SslMode=none";
        public static string MySqlDb_10_4 = "Server=localhost;Port=48205;Database=test;UID=root;Password=test;SslMode=none";
        public static string PostgresDb_9 = Environment.GetEnvironmentVariable("PGSQL_CONNECTION") ?? "Server=localhost;Port=48301;User Id=test;Password=test;Database=test;Pooling=true;MinPoolSize=0;MaxPoolSize=200";
        public static string PostgresDb_10 = "Server=localhost;Port=48302;User Id=test;Password=test;Database=test;Pooling=true;MinPoolSize=0;MaxPoolSize=200";
        public static string PostgresDb_11 = "Server=localhost;Port=48303;User Id=test;Password=test;Database=test;Pooling=true;MinPoolSize=0;MaxPoolSize=200";
        public static string FirebirdDb_3 = Environment.GetEnvironmentVariable("FIREBIRD_CONNECTION") ?? @"User=SYSDBA;Password=masterkey;Database=/firebird/data/test.gdb;DataSource=localhost;Port=48101;Dialect=3;charset=ISO8859_1;MinPoolSize=0;MaxPoolSize=100;";

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

        public OrmLiteTestBase() { }

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
            var dbFactory = new OrmLiteConnectionFactory(Config.MySqlDb_5_5, MySqlDialect.Provider);
            return dbFactory;
        }

        public static OrmLiteConnectionFactory CreatePostgreSqlDbFactory()
        {
            var dbFactory = new OrmLiteConnectionFactory(Config.PostgresDb_9, PostgreSqlDialect.Provider);
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

        public Dialect Dialect = Config.DefaultDialect;
        protected OrmLiteConnectionFactory DbFactory;

        OrmLiteConnectionFactory Init(string connStr, IOrmLiteDialectProvider dialectProvider)
        {
            ConnectionString = connStr;
            OrmLiteConfig.DialectProvider = dialectProvider;
            DbFactory = new OrmLiteConnectionFactory(ConnectionString, dialectProvider);
            return DbFactory;
        }

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            Init();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            OrmLiteContext.Instance.ClearItems();
        }

        private OrmLiteConnectionFactory Init()
        {
            //OrmLiteConfig.UseParameterizeSqlExpressions = false;

            //OrmLiteConfig.DeoptimizeReader = true;
            LogManager.LogFactory = new ConsoleLogFactory(debugEnabled: Config.EnableDebugLogging);
            
            switch (Dialect)
            {
                case Dialect.Sqlite:
                    var dbFactory = Init(Config.SqliteMemoryDb, SqliteDialect.Provider);
                    dbFactory.AutoDisposeConnection = false;
                    return dbFactory;
                case Dialect.SqlServer:
                    return Init(Config.SqlServerBuildDb, SqlServerDialect.Provider);
                case Dialect.SqlServer2008:
                    return Init(Config.SqlServerBuildDb, SqlServer2008Dialect.Provider);
                case Dialect.SqlServer2012:
                    return Init(Config.SqlServerBuildDb, SqlServer2012Dialect.Provider);
                case Dialect.SqlServer2014:
                    return Init(Config.SqlServerBuildDb, SqlServer2014Dialect.Provider);
                case Dialect.SqlServer2016:
                    return Init(Config.SqlServerBuildDb, SqlServer2016Dialect.Provider);
                case Dialect.SqlServer2017:
                    return Init(Config.SqlServerBuildDb, SqlServer2017Dialect.Provider);
                case Dialect.MySql5_5:
                    return Init(Config.MySqlDb_5_5, MySqlDialect.Provider);
                case Dialect.PostgreSql9:
                    return Init(Config.PostgresDb_9, PostgreSqlDialect.Provider);
                case Dialect.SqlServerMdf:
                    return Init(Config.SqlServerDb, SqlServerDialect.Provider);
                case Dialect.Oracle10:
                    return Init(Config.OracleDb, OracleDialect.Provider);
                case Dialect.Firebird:
                    return Init(Config.FirebirdDb_3, FirebirdDialect.Provider);

#if !NETCORE                    
                case Dialect.VistaDb:
                    VistaDbDialect.Instance.UseLibraryFromGac = true;
                    var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["myVDBConnection"];
                    var factory = DbProviderFactories.GetFactory(connectionString.ProviderName);
                    using (var db = factory.CreateConnection())
                    using (var cmd = db.CreateCommand())
                    {
                        var tmpFile = Path.GetTempPath().CombineWith(Guid.NewGuid().ToString("n") + ".vb5");
                        cmd.CommandText = @"CREATE DATABASE '|DataDirectory|{0}', PAGE SIZE 4, LCID 1033, CASE SENSITIVE FALSE;"
                            .Fmt(tmpFile);
                        cmd.ExecuteNonQuery();
                        return Init("Data Source={0};".Fmt(tmpFile), VistaDbDialect.Provider);
                    }
#endif
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

        public virtual Task<IDbConnection> OpenDbConnectionAsync()
        {
            if (ConnectionString == ":memory:")
            {
                if (InMemoryDbConnection == null || DbFactory.AutoDisposeConnection)
                {
                    InMemoryDbConnection = new OrmLiteConnection(DbFactory);
                    InMemoryDbConnection.Open();
                }
                return Task.FromResult(InMemoryDbConnection);
            }

            return DbFactory.OpenDbConnectionAsync();
        }

        protected void SuppressIfOracle(string reason, params object[] args)
        {
            // Not Oracle if this base class used
        }
    }
}
