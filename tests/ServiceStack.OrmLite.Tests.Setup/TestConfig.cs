using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [Flags]
    public enum Dialect
    {
        Sqlite = 1,
        
        SqlServer = 1 << 1,
        SqlServer2008 = 1 << 2,
        SqlServer2012 = 1 << 3,
        SqlServer2014 = 1 << 4,
        SqlServer2016 = 1 << 5,
        SqlServer2017 = 1 << 6,
        
        PostgreSql = 1 << 7,
        
        MySql = 1 << 8,
        MySqlConnector = 1 << 9,
        
        Oracle = 1 << 10,
        
        Firebird = 1 << 11,
        
        VistaDb = 1 << 12,
        
        // any versions
        AnyPostgreSql = PostgreSql,
        AnyMySql = MySql | MySqlConnector, 
        AnySqlServer = SqlServer | SqlServer2008 | SqlServer2012 | SqlServer2014 | SqlServer2016 | SqlServer2017,
        AnyOracle = Oracle,
        
        // db groups
        BaseSupported = Sqlite | SqlServer | PostgreSql | MySql | MySqlConnector,
        Supported = Sqlite | AnySqlServer | AnyMySql | AnyPostgreSql,
        Community = Firebird | Oracle | VistaDb,
        
        // all
        All = Supported | Community
    }

    public struct DialectContext
    {
        public Dialect Dialect;
        public int Version;
        public DialectContext(Dialect dialect, int version)
        {
            Dialect = dialect;
            Version = version;
        }
        
        public Tuple<Dialect, int> Tuple => System.Tuple.Create(Dialect, Version);
        public static string Key(Tuple<Dialect, int> tuple) => Key(tuple.Item1, tuple.Item2);
        public static string Key(Dialect dialect, int version) => dialect + "-" + version;

        public override string ToString()
        {
            var defaultLabel = Dialect + " " + Version;
            switch (Dialect)
            {
                case Dialect.Sqlite:
                    return SqliteDb.VersionString(Version); 
                case Dialect.SqlServer:
                case Dialect.SqlServer2008:
                case Dialect.SqlServer2012:
                case Dialect.SqlServer2014:
                case Dialect.SqlServer2016:
                case Dialect.SqlServer2017:
                    return SqlServerDb.VersionString(Dialect, Version); 
                case Dialect.MySql:
                    return MySqlDb.VersionString(Version); 
                case Dialect.PostgreSql:
                    return PostgreSqlDb.VersionString(Version); 
                case Dialect.Oracle:
                    return OracleDb.VersionString(Version); 
                case Dialect.Firebird:
                    return FirebirdDb.VersionString(Version); 
                case Dialect.VistaDb:
                    return VistaDb.VersionString(Version); 
            }

            return defaultLabel;
        }

        public OrmLiteConnectionFactory NamedConnection => OrmLiteConnectionFactory.NamedConnections[Key(Dialect, Version)];
    }

    public static class SqliteDb
    {
        public const int Memory = 1;
        public const int File = 100;
        public static int[] Versions => TestConfig.EnvironmentVariable("SQLITE_VERSION", new[]{ Memory });
        public static string DefaultConnection => MemoryConnection;
        public static string MemoryConnection => TestConfig.DialectConnections[Tuple.Create(Dialect.Sqlite, Memory)];
        public static string FileConnection => TestConfig.DialectConnections[Tuple.Create(Dialect.Sqlite, File)];
        public static string VersionString(int version) => "SQLite " + (version == Memory
            ? "Memory"
            : version == File
            ? "File"
            : version.ToString());
    }
    public static class SqlServerDb
    {
        public const int V2008 = 2008;
        public const int V2012 = 2012;
        public const int V2014 = 2014;
        public const int V2016 = 2016;
        public const int V2017 = 2017;
        public static int[] Versions = TestConfig.EnvironmentVariable("MSSQL_VERSION", new[]{ V2008, V2012, V2014, V2016, V2017 });
        public static int[] V2008Versions = Versions.Where(x => x == V2008).ToArray();
        public static int[] V2012Versions = Versions.Where(x => x == V2012).ToArray();
        public static int[] V2014Versions = Versions.Where(x => x == V2014).ToArray();
        public static int[] V2016Versions = Versions.Where(x => x == V2016).ToArray();
        public static int[] V2017Versions = Versions.Where(x => x == V2017).ToArray();
        public static string DefaultConnection => TestConfig.DialectConnections[Tuple.Create(Dialect.SqlServer2016, V2016)];
        public static string VersionString(Dialect dialect, int version) => "SQL Server " + version;
        
        public static Dictionary<Dialect, int> CompatibilityLevels = new Dictionary<Dialect, int> 
        {
            [Dialect.SqlServer2008] = 100,
            [Dialect.SqlServer2012] = 110,
            [Dialect.SqlServer2014] = 120,
            [Dialect.SqlServer2016] = 130,
            [Dialect.SqlServer2017] = 140,
        };
    }
    public static class MySqlDb
    {
        public const int V5_5 = 55;
        public const int V10_1 = 101;
        public const int V10_2 = 102;
        public const int V10_3 = 103;
        public const int V10_4 = 104;
        public static readonly int[] Versions = TestConfig.EnvironmentVariable("MYSQL_VERSION", new[]{ V5_5, V10_1, V10_2, V10_3, V10_4 });
        public static int[] MySqlConnectorVersions = Versions.Where(x => x == V10_4).ToArray();
        public static readonly string DefaultConnection = TestConfig.DialectConnections[Tuple.Create(Dialect.MySql, V10_4)];

        public static string VersionString(int version) => "MySQL " + (version == V5_5
            ? "v5_5"
            : version == V10_1
            ? "v10_1"
            : version == V10_2
            ? "v10_2"
            : version == V10_3
            ? "v10_3"
            : version == V10_4
            ? "v10_4"
            : version.ToString());

    }
    public static class PostgreSqlDb
    {
        public const int V9 = 9;
        public const int V10 = 10;
        public const int V11 = 11;
        public static readonly int[] Versions = TestConfig.EnvironmentVariable("PGSQL_VERSION", new[]{ V9, V10, V11 });
        public static readonly string DefaultConnection = TestConfig.GetConnection(Dialect.PostgreSql, V11);
        public static string VersionString(int version) => "PostgreSQL " + (version == V9
            ? "v9"
            : version == V10
            ? "v10"
            : version == V11
            ? "v11"
            : version.ToString());
    }
    public static class OracleDb
    {
        public const int V11 = 11;
        public static readonly int[] Versions = TestConfig.EnvironmentVariable("ORACLE_VERSION", new[]{ V11 });
        public static readonly string DefaultConnection = TestConfig.GetConnection(Dialect.Oracle, V11);
        public static string VersionString(int version) => "Oracle " + (version == V11
            ? "v11"
            : version.ToString());
    }
    public static class FirebirdDb
    {
        public const int V3 = 3;
        public static readonly int[] Versions = TestConfig.EnvironmentVariable("FIREBIRD_VERSION", new[]{ V3 });
        public static readonly string DefaultConnection = TestConfig.GetConnection(Dialect.Firebird, V3);
        public static string VersionString(int version) => "Firebird " + (version == V3
            ? "v3"
            : version.ToString());
    }
    public static class VistaDb
    {
        public const int V5 = 5;
        public static readonly int[] Versions = TestConfig.EnvironmentVariable("VISTADB_VERSION", new[]{ V5 });
        public static readonly string DefaultConnection = TestConfig.GetConnection(Dialect.VistaDb, V5);
        public static string VersionString(int version) => "VistaDB " + (version == V5
            ? "v5"
            : version.ToString());
    }

    /// <summary>
    /// Primary config for all tests 
    /// </summary>
    public class TestConfig
    {
        /// <summary>
        /// This value controls which providers are tested for all <see cref="TestFixtureOrmLiteAttribute"/> tests where dialects are not explicitly set
        /// </summary>
        public static Dialect Dialects = EnvironmentVariable("ORMLITE_DIALECT", Dialect.Sqlite);
        public const bool EnableDebugLogging = false;

        public static Dictionary<Dialect, IOrmLiteDialectProvider> DialectProviders = new Dictionary<Dialect, IOrmLiteDialectProvider> 
        {
            [Dialect.Sqlite] = SqliteDialect.Provider,
            [Dialect.SqlServer] = SqlServerDialect.Provider,
            [Dialect.SqlServer2008] = SqlServer2008Dialect.Provider,
            [Dialect.SqlServer2012] = SqlServer2012Dialect.Provider,
            [Dialect.SqlServer2014] = SqlServer2014Dialect.Provider,
            [Dialect.SqlServer2016] = SqlServer2016Dialect.Provider,
            [Dialect.SqlServer2017] = SqlServer2017Dialect.Provider,
            [Dialect.PostgreSql] = PostgreSqlDialect.Provider,
            [Dialect.MySql] = MySqlDialect.Provider,
            [Dialect.MySqlConnector] = MySqlConnectorDialect.Provider,
            [Dialect.Oracle] = OracleDialect.Provider,
            [Dialect.Firebird] = FirebirdDialect.Provider,
#if !NETCORE
            [Dialect.VistaDb] = VistaDbDialect.Provider,
#endif
        };

        public static string GetConnection(Dialect dialect, int version)
        {
            if (DialectConnections.TryGetValue(Tuple.Create(dialect, version), out var connString))
                return connString;

            return null;
        }

        private static Dictionary<Tuple<Dialect, int>, string> dialectConnections;
        public static Dictionary<Tuple<Dialect,int>, string> DialectConnections => dialectConnections ?? (dialectConnections = new Dictionary<Tuple<Dialect,int>, string> 
        {
            [Tuple.Create(Dialect.Sqlite, SqliteDb.Memory)] = EnvironmentVariable(new[]{ "SQLITE_MEMORY_CONNECTION", "SQLITE_CONNECTION" }, ":memory:"),
            [Tuple.Create(Dialect.Sqlite, SqliteDb.File)] = EnvironmentVariable(new[]{ "SQLITE_FILE_CONNECTION", "SQLITE_CONNECTION" }, "~/App_Data/db.sqlite".MapAbsolutePath()),

            [Tuple.Create(Dialect.SqlServer, SqlServerDb.V2008)] = EnvironmentVariable(new[]{ "MSSQL2008_CONNECTION", "MSSQL_CONNECTION" }, "Data Source=tcp:localhost,48501\\SQLExpress;Initial Catalog=master;User Id=sa;Password=Test!tesT;Connect Timeout=120;MultipleActiveResultSets=True;"),
            [Tuple.Create(Dialect.SqlServer2016, SqlServerDb.V2016)] = EnvironmentVariable(new[]{ "MSSQL2016_CONNECTION", "MSSQL_CONNECTION" }, "Data Source=tcp:localhost,48501\\SQLExpress;Initial Catalog=master;User Id=sa;Password=Test!tesT;Connect Timeout=120;MultipleActiveResultSets=True;"),
            [Tuple.Create(Dialect.SqlServer2017, SqlServerDb.V2017)] = EnvironmentVariable(new[]{ "MSSQL2017_CONNECTION", "MSSQL_CONNECTION" }, "Data Source=tcp:localhost,48501\\SQLExpress;Initial Catalog=master;User Id=sa;Password=Test!tesT;Connect Timeout=120;MultipleActiveResultSets=True;"),

            [Tuple.Create(Dialect.PostgreSql, PostgreSqlDb.V9)]  = EnvironmentVariable(new[]{ "PGSQL9_CONNECTION",  "PGSQL_CONNECTION" }, "Server=localhost;Port=48301;User Id=test;Password=test;Database=test;Pooling=true;MinPoolSize=0;MaxPoolSize=200"),
            [Tuple.Create(Dialect.PostgreSql, PostgreSqlDb.V10)] = EnvironmentVariable(new[]{ "PGSQL10_CONNECTION", "PGSQL_CONNECTION" }, "Server=localhost;Port=48302;User Id=test;Password=test;Database=test;Pooling=true;MinPoolSize=0;MaxPoolSize=200"),
            [Tuple.Create(Dialect.PostgreSql, PostgreSqlDb.V11)] = EnvironmentVariable(new[]{ "PGSQL11_CONNECTION", "PGSQL_CONNECTION" }, "Server=localhost;Port=48303;User Id=test;Password=test;Database=test;Pooling=true;MinPoolSize=0;MaxPoolSize=200"),
            
            [Tuple.Create(Dialect.MySql, MySqlDb.V5_5)]  = EnvironmentVariable(new[]{ "MYSQL55_CONNECTION",  "MYSQL_CONNECTION" }, "Server=localhost;Port=48201;Database=test;UID=root;Password=test;SslMode=none;Convert Zero Datetime=True;"),
            [Tuple.Create(Dialect.MySql, MySqlDb.V10_1)] = EnvironmentVariable(new[]{ "MYSQL101_CONNECTION", "MYSQL_CONNECTION" }, "Server=localhost;Port=48202;Database=test;UID=root;Password=test;SslMode=none"),
            [Tuple.Create(Dialect.MySql, MySqlDb.V10_2)] = EnvironmentVariable(new[]{ "MYSQL102_CONNECTION", "MYSQL_CONNECTION" }, "Server=localhost;Port=48203;Database=test;UID=root;Password=test;SslMode=none"),
            [Tuple.Create(Dialect.MySql, MySqlDb.V10_3)] = EnvironmentVariable(new[]{ "MYSQL103_CONNECTION", "MYSQL_CONNECTION" }, "Server=localhost;Port=48204;Database=test;UID=root;Password=test;SslMode=none"),
            [Tuple.Create(Dialect.MySql, MySqlDb.V10_4)] = EnvironmentVariable(new[]{ "MYSQL104_CONNECTION", "MYSQL_CONNECTION" }, "Server=localhost;Port=48205;Database=test;UID=root;Password=test;SslMode=none"),

            [Tuple.Create(Dialect.MySqlConnector, MySqlDb.V10_4)] = EnvironmentVariable(new[]{ "MYSQL104_CONNECTION", "MYSQL_CONNECTION" }, "Server=localhost;Port=48205;Database=test;UID=root;Password=test;SslMode=none"),
            
            [Tuple.Create(Dialect.Oracle, OracleDb.V11)] = EnvironmentVariable(new[]{ "ORACLE11_CONNECTION", "ORACLE_CONNECTION" }, "Data Source=tcp:localhost,48501\\SQLExpress;Initial Catalog=master;User Id=sa;Password=Test!tesT;Connect Timeout=120;MultipleActiveResultSets=True;"),
            
            [Tuple.Create(Dialect.Firebird, FirebirdDb.V3)] = EnvironmentVariable(new[]{ "FIREBIRD3_CONNECTION", "FIREBIRD_CONNECTION" }, @"User=SYSDBA;Password=masterkey;Database=/firebird/data/test.gdb;DataSource=localhost;Port=48101;Dialect=3;charset=ISO8859_1;MinPoolSize=0;MaxPoolSize=100;"),
            
            [Tuple.Create(Dialect.VistaDb, VistaDb.V5)] = EnvironmentVariable(new[]{ "VISTADB5_CONNECTION", "VISTADB_CONNECTION" }, @"Data Source='|DataDirectory|\Database.vdb5'"),
        });
        
        public static Dictionary<Dialect, int[]> DialectVersions = new Dictionary<Dialect, int[]> 
        {
            [Dialect.Sqlite] = SqliteDb.Versions,
            [Dialect.SqlServer] = SqlServerDb.V2008Versions,
            [Dialect.SqlServer2008] = SqlServerDb.V2008Versions,
            [Dialect.SqlServer2012] = SqlServerDb.V2012Versions,
            [Dialect.SqlServer2014] = SqlServerDb.V2014Versions,
            [Dialect.SqlServer2016] = SqlServerDb.V2016Versions,
            [Dialect.SqlServer2017] = SqlServerDb.V2017Versions,
            [Dialect.PostgreSql] = PostgreSqlDb.Versions,
            [Dialect.MySql] = MySqlDb.Versions,
            [Dialect.MySqlConnector] = MySqlDb.MySqlConnectorVersions,
            
            [Dialect.Oracle] = OracleDb.Versions,
            [Dialect.Firebird] = FirebirdDb.Versions,
            [Dialect.VistaDb] = VistaDb.Versions,
        };
        
        public static IOrmLiteDialectProvider DefaultProvider = SqliteDialect.Provider;
        public static string DefaultConnection = SqliteDb.DefaultConnection;

        public static string EnvironmentVariable(string[] variables, string defaultValue) => 
            variables.Map(Environment.GetEnvironmentVariable).FirstOrDefault(x => x != null) ?? defaultValue;

        public static T EnvironmentVariable<T>(string variable, T defaultValue)
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

            foreach (var dialectConnection in DialectConnections)
            {
                var dialect = dialectConnection.Key.Item1;
                if (!DialectProviders.TryGetValue(dialect, out var dialectProvider))
                    continue;

                if (dialect != Dialect.VistaDb)
                {
                    
                    dbFactory.RegisterConnection(DialectContext.Key(dialectConnection.Key), dialectConnection.Value, dialectProvider);
                    continue;
                }
                
#if !NETCORE
                var version = dialectConnection.Key.Item2;
                VistaDbDialect.Instance.UseLibraryFromGac = true;
                var connectionString = VistaDb.DefaultConnection;
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
                        dbFactory.RegisterConnection(DialectContext.Key(dialect,version),tmpFile,VistaDbDialect.Provider);
                    }
                }
                catch
                {
                    // vista not installed.
                }
#endif
            }

            foreach (var provider in DialectProviders)
            {
                dbFactory.RegisterDialectProvider(provider.Key.ToString(), provider.Value);
            }
            
            return dbFactory;
        }

        public static void InitDbScripts(OrmLiteConnectionFactory dbFactory)
        {
            if ((Dialects & Dialect.AnyPostgreSql) != 0)
            {
                foreach (var version in DialectVersions[Dialect.PostgreSql])
                {
                    try
                    {
                        using (var db = dbFactory.OpenDbConnectionString(
                            DialectConnections[Tuple.Create(Dialect.PostgreSql, version)] + ";Timeout=1",
                            Dialect.PostgreSql.ToString()))
                        {
                            InitPostgres(Dialect.PostgreSql, db);
                        }
                    }
                    catch {}
                }
            }

            if ((Dialects & Dialect.MySqlConnector) != 0)
            {
                try
                {
                    foreach (var version in DialectVersions[Dialect.MySqlConnector])
                    {
                        using (var db = dbFactory.OpenDbConnectionString(
                            DialectConnections[Tuple.Create(Dialect.MySqlConnector, version)] + ";Timeout=1",
                            Dialect.MySqlConnector.ToString()))
                        {
                            InitMySqlConnector(Dialect.PostgreSql, db);
                        }
                    }
                }
                catch {}
            }

            if ((Dialects & Dialect.AnySqlServer) != 0)
            {
                void SetupSqlServer(Dialect dialect, int version)
                {
                    if ((Dialects & dialect) != 0)
                    {
                        if (DialectConnections.TryGetValue(Tuple.Create(dialect, version), out var connString))
                        {
                            using (var db = dbFactory.OpenDbConnectionString(connString + ";Timeout=1", dialect.ToString()))
                            {
                                InitSqlServer(dialect, db);
                            }
                        }
                    }
                }
                SetupSqlServer(Dialect.SqlServer, SqlServerDb.V2008);
                SetupSqlServer(Dialect.SqlServer2012, SqlServerDb.V2012);
                SetupSqlServer(Dialect.SqlServer2014, SqlServerDb.V2014);
                SetupSqlServer(Dialect.SqlServer2016, SqlServerDb.V2016);
                SetupSqlServer(Dialect.SqlServer2017, SqlServerDb.V2017);
            }
        }
        
        public static void InitPostgres(Dialect dialect, IDbConnection db)
        {
            db.ExecuteSql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");
            db.ExecuteSql("CREATE EXTENSION IF NOT EXISTS hstore");

            var dialectProvider = db.GetDialectProvider();
            var schemaName = dialectProvider.NamingStrategy.GetSchemaName("Schema");
            db.ExecuteSql($"CREATE SCHEMA IF NOT EXISTS {dialectProvider.GetQuotedName(schemaName)}");
        }

        public static void InitMySqlConnector(Dialect dialect, IDbConnection db)
        {
            db.ExecuteSql("CREATE DATABASE IF NOT EXISTS `testMySql`");
        }
        
        public static void InitSqlServer(Dialect dialect, IDbConnection db)
        {
            // Create unique db per fixture to avoid conflicts when testing dialects
            // uses COMPATIBILITY_LEVEL set to each version 

            var dbName = dialect.ToString();
            var compatibilityLevel = SqlServerDb.CompatibilityLevels[dialect];
            var createSqlDb = $@"If(db_id(N'{dbName}') IS NULL)
  BEGIN
  CREATE DATABASE {dbName};
  ALTER DATABASE {dbName} SET COMPATIBILITY_LEVEL = {compatibilityLevel};
  END";
            db.ExecuteSql(createSqlDb);
        }

    }
}
