using System;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace ServiceStack.OrmLite.Tests
{
    [Flags]
    public enum Dialect
    {
        Sqlite         = 1,
        
        SqlServer = 1 << 1,
        SqlServerMdf = 1 << 2,
        SqlServer2008 = 1 << 3,
        SqlServer2012 = 1 << 4,
        SqlServer2014 = 1 << 5,
        SqlServer2016 = 1 << 6,
        SqlServer2017 = 1 << 7,
        
        PostgreSql9 = 1 << 8,
        PostgreSql10 = 1 << 9,
        PostgreSql11 = 1 << 10,
        
        MySql5_5 = 1 << 11,
        MySql10_1 = 1 << 12,
        MySql10_2 = 1 << 13,
        MySql10_3 = 1 << 14,
        MySql10_4 = 1 << 15,
        MySqlConnector = 1 << 16,
        
        Oracle10 = 1 << 17,
        Oracle11 = 1 << 18,
        Oracle12 = 1 << 19,
        Oracle18 = 1 << 20,
        
        Firebird = 1 << 21,
        
        VistaDb = 1 << 22,
        
        // any versions
        AnyPostgreSql = PostgreSql9 | PostgreSql10 | PostgreSql11,
        AnyMySql = MySql5_5 | MySql10_1 | MySql10_2 | MySql10_3 | MySql10_4 | MySqlConnector, 
        AnySqlServer = SqlServer | SqlServer2008 | SqlServer2012 | SqlServer2014 | SqlServer2016 | SqlServer2017 | SqlServerMdf,
        AnyOracle = Oracle10 | Oracle11 | Oracle12 | Oracle18,
        
        // db groups
        BaseSupported = Sqlite | MySql5_5 | MySqlConnector | PostgreSql9 | SqlServer,
        Supported = Sqlite | AnyMySql | AnyPostgreSql | AnySqlServer,
        BaseCommunity = Firebird | Oracle10 | VistaDb,
        Community = Firebird | AnyOracle | VistaDb,
        DockerDb =  AnyMySql | AnyPostgreSql | AnySqlServer | Community,
        
        // all
        All = Supported | Community
    }

    /// <summary>
    /// Primary config for all tests 
    /// </summary>
    public class TestConfig
    {
        /// <summary>
        /// This value controls which providers are tested for all <see cref="TestFixtureOrmLiteAttribute"/> tests where dialects are not explicitly set
        /// </summary>
        public static Dialect DefaultDialects = EnvironmentVariable("ORMLITE_DIALECT", Dialect.Sqlite);
        
        public static string SqliteMemoryDb = EnvironmentVariable("SQLITE_CONNECTION", ":memory:");
        public static string SqlServerBuildDb = EnvironmentVariable("MSSQL_CONNECTION", "Data Source=tcp:localhost,48501\\SQLExpress;Initial Catalog=master;User Id=sa;Password=Test!tesT;Connect Timeout=120;MultipleActiveResultSets=True;");
        public static string OracleDb = EnvironmentVariable("ORACLE_CONNECTION", "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=48401))(CONNECT_DATA=(SID=XE)));User Id=system;Password=test;");
        
        // TODO should dates be handled by reader instead of having to set config options?
        public static string MariaDb_5_5 = EnvironmentVariable(new[]{ "MYSQL55_CONNECTION", "MYSQL_CONNECTION" }, "Server=localhost;Port=48201;Database=test;UID=root;Password=test;SslMode=none;Convert Zero Datetime=True;");
        public static string MariaDb_10_1 = EnvironmentVariable(new[]{ "MYSQL101_CONNECTION", "MYSQL_CONNECTION" }, "Server=localhost;Port=48202;Database=test;UID=root;Password=test;SslMode=none");
        public static string MariaDb_10_2 = EnvironmentVariable(new[]{ "MYSQL102_CONNECTION", "MYSQL_CONNECTION" }, "Server=localhost;Port=48203;Database=test;UID=root;Password=test;SslMode=none");
        public static string MariaDb_10_3 = EnvironmentVariable(new[]{ "MYSQL103_CONNECTION", "MYSQL_CONNECTION" }, "Server=localhost;Port=48204;Database=test;UID=root;Password=test;SslMode=none");
        public static string MariaDb_10_4 = EnvironmentVariable(new[]{ "MYSQL104_CONNECTION", "MYSQL_CONNECTION" }, "Server=localhost;Port=48205;Database=test;UID=root;Password=test;SslMode=none");
        
        public static string MySqlDb_10_1 = EnvironmentVariable(new[]{ "MYSQL101_CONNECTION", "MYSQL_CONNECTION" }, "Server=localhost;Port=48202;Database=testMySql;UID=root;Password=test;SslMode=none;Convert Zero Datetime=True;");
        
        public static string PostgresDb_9 = EnvironmentVariable(new[]{ "PGSQL9_CONNECTION", "PGSQL_CONNECTION" }, "Server=localhost;Port=48301;User Id=test;Password=test;Database=test;Pooling=true;MinPoolSize=0;MaxPoolSize=200");
        public static string PostgresDb_10 = EnvironmentVariable(new[]{ "PGSQL10_CONNECTION", "PGSQL_CONNECTION" }, "Server=localhost;Port=48302;User Id=test;Password=test;Database=test;Pooling=true;MinPoolSize=0;MaxPoolSize=200");
        public static string PostgresDb_11 = EnvironmentVariable(new[]{ "PGSQL11_CONNECTION", "PGSQL_CONNECTION" }, "Server=localhost;Port=48303;User Id=test;Password=test;Database=test;Pooling=true;MinPoolSize=0;MaxPoolSize=200");
        
        public static string FirebirdDb_3 = EnvironmentVariable("FIREBIRD_CONNECTION", @"User=SYSDBA;Password=masterkey;Database=/firebird/data/test.gdb;DataSource=localhost;Port=48101;Dialect=3;charset=ISO8859_1;MinPoolSize=0;MaxPoolSize=100;");
        public static string VistaDb = EnvironmentVariable("VISTADB_CONNECTION", @"Data Source='|DataDirectory|\Database.vdb5'");
        public static string SqliteFileDb = "~/App_Data/db.sqlite".MapAbsolutePath();
        
        // The default provider and connection the DbFactory is initialised with
        public static IOrmLiteDialectProvider DefaultProvider = SqliteDialect.Provider;
        public static string DefaultConnection = SqliteMemoryDb;

        private static string EnvironmentVariable(string[] variables, string defaultValue) => 
            variables.FirstOrDefault(x => Environment.GetEnvironmentVariable(x) != null) ?? defaultValue;

        private static T EnvironmentVariable<T>(string variable, T defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(variable);
            return string.IsNullOrEmpty(value) ? defaultValue : Convert<T>(value);
        }
        
        private static T Convert<T>(string value)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)converter.ConvertFromInvariantString(value);
        }


        public static OrmLiteConnectionFactory InitDbFactory()
        {
            // init DbFactory, should be mainly ignored in tests as they should always ask for a provider specific named connection
            var dbFactory = new OrmLiteConnectionFactory(DefaultConnection, DefaultProvider);

            dbFactory.RegisterConnection(Dialect.PostgreSql9.ToString(), PostgresDb_9, PostgreSqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.PostgreSql10.ToString(), PostgresDb_10, PostgreSqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.PostgreSql11.ToString(), PostgresDb_11, PostgreSqlDialect.Provider);

            dbFactory.RegisterConnection(Dialect.MySql5_5.ToString(), MariaDb_5_5, MySql55Dialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_1.ToString(), MariaDb_10_1, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_2.ToString(), MariaDb_10_2, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_3.ToString(), MariaDb_10_3, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_4.ToString(), MariaDb_10_4, MySqlDialect.Provider);

            dbFactory.RegisterConnection(Dialect.Sqlite.ToString(), SqliteMemoryDb, SqliteDialect.Provider);

            dbFactory.RegisterConnection(Dialect.SqlServer.ToString(), SqlServerBuildDb, SqlServerDialect.Provider);

            dbFactory.RegisterConnection(Dialect.Oracle10.ToString(), OracleDb, OracleDialect.Provider);
            dbFactory.RegisterConnection(Dialect.Oracle11.ToString(), OracleDb, OracleDialect.Provider);
            dbFactory.RegisterConnection(Dialect.Oracle12.ToString(), OracleDb, OracleDialect.Provider);
            dbFactory.RegisterConnection(Dialect.Oracle18.ToString(), OracleDb, OracleDialect.Provider);

            dbFactory.RegisterConnection(Dialect.Firebird.ToString(), FirebirdDb_3, FirebirdDialect.Provider);
            
#if !NETCORE                    
                    VistaDbDialect.Instance.UseLibraryFromGac = true;
                    var connectionString = VistaDb;
                    try
                    {
                        var factory = DbProviderFactories.GetFactory("System.Data.VistaDB5;");
                            using (var db = factory.CreateConnection())
                            using (var cmd = db.CreateCommand())
                            {
                                db.ConnectionString = connectionString;
                                var tmpFile = Path.GetTempPath().CombineWith($"{Guid.NewGuid():n}.vb5");
                                cmd.CommandText =
                                    $"CREATE DATABASE '|DataDirectory|{tmpFile}', PAGE SIZE 4, LCID 1033, CASE SENSITIVE FALSE;";
                                cmd.ExecuteNonQuery();
                                dbFactory.RegisterConnection(Dialect.VistaDb.ToString(), tmpFile,
                                    VistaDbDialect.Provider);
                            }
                    }
                    catch
                    {
                        // vista not installed.
                    }
#endif
            return dbFactory;
        }

        public static void InitDbScripts(OrmLiteConnectionFactory dbFactory)
        {
            // POSTGRESQL specific init
            // enable postgres uuid extension for all test db's
            var pgInits = new[] {
                "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";",
                "CREATE EXTENSION IF NOT EXISTS hstore;",
            };
            

            OrmLiteConnectionFactory getFactory(Dialect dialect) => OrmLiteConnectionFactory.NamedConnections[dialect.ToString()];
            try
            {
                using (var pg9 = getFactory(Dialect.PostgreSql9).OpenDbConnectionString($"{PostgresDb_9};Timeout=1"))
                    pgInits.Map(pg9.ExecuteSql);
            }
            catch
            {
                // no db available
            }

            try
            {
                using (var pg10 = getFactory(Dialect.PostgreSql10).OpenDbConnectionString($"{PostgresDb_10};Timeout=1"))
                    pgInits.Map(pg10.ExecuteSql);
            }
            catch
            {
                // no db available
            }

            try
            {
                using (var pg11 = getFactory(Dialect.PostgreSql11).OpenDbConnectionString($"{PostgresDb_11};Timeout=1"))
                    pgInits.Map(pg11.ExecuteSql);
            }
            catch
            {
                // no db available
            }

            try
            {
                // Create separate Db's for MySqlConnector
                using (var db = getFactory(Dialect.MySql10_1).OpenDbConnectionString($"{MariaDb_10_1};Connection Timeout=1"))
                {
                    db.ExecuteSql("CREATE DATABASE IF NOT EXISTS `testMySql`");
                    dbFactory.RegisterConnection(Dialect.MySqlConnector.ToString(), MySqlDb_10_1,
                        MySqlConnectorDialect.Provider);
                }
            }
            catch
            {
                // no db available
            }

            // SQLSERVER specific init
            // for sql create unique db per fixture to avoid conflicts when testing dialects
            // uses COMPATIBILITY_LEVEL set to each version 
            try
            {
                using (var db = getFactory(Dialect.SqlServer)
                    .OpenDbConnectionString($"{SqlServerBuildDb};Connection Timeout=1"))
                {
                    var versions = new (string DbName, IOrmLiteDialectProvider Provider, int CompatabilityLevel)[]
                    {
                        (Dialect.SqlServer2008.ToString(), SqlServer2008Dialect.Provider, 100),
                        (Dialect.SqlServer2012.ToString(), SqlServer2012Dialect.Provider, 110),
                        (Dialect.SqlServer2014.ToString(), SqlServer2014Dialect.Provider, 120),
                        (Dialect.SqlServer2016.ToString(), SqlServer2016Dialect.Provider, 130),
                        (Dialect.SqlServer2017.ToString(), SqlServer2017Dialect.Provider, 140),
                    };

                    var connStr = new SqlConnectionStringBuilder($"{SqlServerBuildDb};Connection Timeout=1");
                    foreach (var version in versions)
                    {
                        try
                        {
                            var createSqlDb = $@"If(db_id(N'{version.DbName}') IS NULL)
  BEGIN
  CREATE DATABASE {version.DbName};
  ALTER DATABASE {version.DbName} SET COMPATIBILITY_LEVEL = {version.CompatabilityLevel};
  END";
                            connStr.InitialCatalog = version.DbName;
                            db.ExecuteSql(createSqlDb);

                            dbFactory.RegisterConnection(version.DbName, connStr.ToString(), version.Provider);
                        }
                        catch
                        {
                            // no db available
                        }
                    }
                }
            }
            catch
            {
                // no db available
            }
        }
        
    }
}
