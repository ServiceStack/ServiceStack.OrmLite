using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using ServiceStack.Host.Handlers;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Tests
{
    /// <summary>
    /// Use this base class in conjunction with one or more <seealso cref="TestFixtureAttribute"/>
    /// attributes to repeat tests for each db dialect.
    /// Alternatively, specify <seealso cref="TestFixtureSourceAttribute"/>
    /// to repeat tests for pre-defined sets of <seealso cref="FixtureDataProvider" /> 
    /// </summary>
    /// <example>
    /// <code>
    /// // example
    /// [TestFixtureOrmLite] // all configured dialects
    /// [TestFixtureDialects(TestDialect.Supported)] // all base versions of supported dialects
    /// public TestClass : OrmLiteProvidersTestBase {
    ///   public TestClass(Dialect dialect) : base(dialect) {}
    ///
    ///   // Test runs once per specified providers
    ///   [Test]
    ///   public void SomeTestMethod() {
    ///     // current dialect 
    ///     var dialect = Dialect;
    ///     // current dialect provider instance
    ///     var dp = DialectProvider;
    ///     // get connection for provider and dialect
    ///     using(var db = OpenDbConnection()) {
    ///       // your db agnostic test code
    ///     }
    ///   }
    /// }
    /// </code>
    /// </example>
    public abstract class OrmLiteProvidersTestBase 
    {
        /// <summary>
        /// The current db dialect
        /// </summary>
        public readonly Dialect Dialect;

        /// <summary>
        /// The current DialogProvider instance
        /// </summary>
        protected readonly IOrmLiteDialectProvider DialectProvider;
        
        // The Database Factory
        protected OrmLiteConnectionFactory DbFactory { get; } = DbFactorySetup.Instance.CreateCopy();

        protected TestLogFactory Log => LogSetup.Instance; 

        /// <summary>
        /// The test logs
        /// TODO can scoped logs be created per provider?
        /// </summary>
        public IList<KeyValuePair<TestLogger.Levels, string>> Logs => TestLogger.GetLogs(); 
        
        public OrmLiteProvidersTestBase(Dialect dialect)
        {
            Dialect = dialect;
            DialectProvider = DbFactory.Open(dialect.ToString()).GetDialectProvider();

            if (OrmLiteConfig.DialectProvider == null) OrmLiteConfig.DialectProvider = DialectProvider;
        }

        public virtual IDbConnection OpenDbConnection() => DbFactory.OpenDbConnection(Dialect.ToString());
        public virtual Task<IDbConnection> OpenDbConnectionAsync() => DbFactory.OpenDbConnectionAsync(Dialect.ToString());
    }

    [SetUpFixture]
    public class LogSetup
    {
        public static TestLogFactory Instance => new TestLogFactory();
        
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            // init logging, for use in tests, filter by type?
            LogManager.LogFactory = Instance;
        }
    }

    [SetUpFixture]
    public class DbFactorySetup
    {
        public static OrmLiteConnectionFactory Instance { get; private set; }

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            var dbFactory = InitDbFactory();
            Instance = InitDbScripts(dbFactory);
        }

        private OrmLiteConnectionFactory InitDbFactory()
        {
            // init DbFactory, should be mainly ignored in tests as they should always ask for a provider specific named connection
            var dbFactory = new OrmLiteConnectionFactory(TestConfig.DefaultConnection, TestConfig.DefaultProvider);

            dbFactory.RegisterConnection(Dialect.PostgreSql.ToString(), TestConfig.PostgresDb_9, PostgreSqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.PostgreSql9.ToString(), TestConfig.PostgresDb_9, PostgreSqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.PostgreSql10.ToString(), TestConfig.PostgresDb_10, PostgreSqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.PostgreSql11.ToString(), TestConfig.PostgresDb_11, PostgreSqlDialect.Provider);

            dbFactory.RegisterConnection(Dialect.MySql.ToString(), TestConfig.MySqlDb_5_5, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql5_5.ToString(), TestConfig.MySqlDb_5_5, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_1.ToString(), TestConfig.MySqlDb_10_1, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_2.ToString(), TestConfig.MySqlDb_10_2, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_3.ToString(), TestConfig.MySqlDb_10_3, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_4.ToString(), TestConfig.MySqlDb_10_4, MySqlDialect.Provider);

            dbFactory.RegisterConnection(Dialect.Sqlite.ToString(), TestConfig.SqliteMemoryDb, SqliteDialect.Provider);

            dbFactory.RegisterConnection(Dialect.SqlServer.ToString(), TestConfig.SqlServerBuildDb, SqlServerDialect.Provider);

            dbFactory.RegisterConnection(Dialect.Oracle.ToString(), TestConfig.OracleDb, OracleDialect.Provider);
            dbFactory.RegisterConnection(Dialect.Oracle10.ToString(), TestConfig.OracleDb, OracleDialect.Provider);
            dbFactory.RegisterConnection(Dialect.Oracle11.ToString(), TestConfig.OracleDb, OracleDialect.Provider);
            dbFactory.RegisterConnection(Dialect.Oracle12.ToString(), TestConfig.OracleDb, OracleDialect.Provider);
            dbFactory.RegisterConnection(Dialect.Oracle18.ToString(), TestConfig.OracleDb, OracleDialect.Provider);

            dbFactory.RegisterConnection(Dialect.Firebird.ToString(), TestConfig.FirebirdDb_3, FirebirdDialect.Provider);
            
#if !NETCORE                    
                    VistaDbDialect.Instance.UseLibraryFromGac = true;
                    var connectionString = TestConfig.VistaDb;
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

        private OrmLiteConnectionFactory InitDbScripts(OrmLiteConnectionFactory dbFactory)
        {
            var pgInit = @"CREATE EXTENSION IF NOT EXISTS ""uuid-ossp""";
            using (var pg9 = dbFactory.OpenDbConnection(Dialect.PostgreSql9.ToString()))
            using (var pg10 = dbFactory.OpenDbConnection(Dialect.PostgreSql10.ToString()))
            using (var pg11 = dbFactory.OpenDbConnection(Dialect.PostgreSql11.ToString()))
            {
                pg9.ExecuteSql(pgInit);
                pg10.ExecuteSql(pgInit);
                pg11.ExecuteSql(pgInit);

                var schemaDboIfNotExists = "CREATE SCHEMA IF NOT EXISTS dbo";
                pg9.ExecuteSql(schemaDboIfNotExists);
                pg10.ExecuteSql(schemaDboIfNotExists);
                pg11.ExecuteSql(schemaDboIfNotExists);
            }

            // TODO for sql create unique db per fixture to avoid conflicts when testing dialects
            using (var db = dbFactory.OpenDbConnection(Dialect.SqlServer.ToString()))
            {
                var versions = new (string DbName, IOrmLiteDialectProvider Provider, int CompatabilityLevel)[]
                {
                    (Dialect.SqlServer2008.ToString(), SqlServer2008Dialect.Provider, 100),
                    (Dialect.SqlServer2012.ToString(), SqlServer2012Dialect.Provider, 110),
                    (Dialect.SqlServer2014.ToString(), SqlServer2014Dialect.Provider, 120),
                    (Dialect.SqlServer2016.ToString(), SqlServer2016Dialect.Provider, 130),
                    (Dialect.SqlServer2017.ToString(), SqlServer2017Dialect.Provider, 140),
                };

                var connStr = new SqlConnectionStringBuilder(TestConfig.SqlServerBuildDb);
                foreach (var version in versions)
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
            }

            return dbFactory;
        }
    }

    /// <summary>
    /// Repeats tests for all dialect versions from <see cref="TestConfig"/>.DefaultDialects.
    /// To restrict tests to specific dialects use <see cref="TestFixtureOrmLiteDialectsAttribute"/>
    /// To filter tests for specific dialects use <see cref="IgnoreProviderAttribute"/>
    /// </summary>
    /// <inheritdoc cref="TestFixtureOrmLiteDialectsAttribute"/>
    public class TestFixtureOrmLiteAttribute : TestFixtureOrmLiteDialectsAttribute
    {
        public TestFixtureOrmLiteAttribute() : base(TestConfig.DefaultDialects)
        {
            // loads the dialects from TestConfig.DefaultDialects
            // which can be overridden using an environment variable
        }
    }

    /// <summary>
    /// Repeats tests for all Dialect specified.
    /// Also sets nunit categories for each dialect flag which 
    /// enables skipping of tests by using Dialect enum flag values
    /// as exclude category names in the test runner
    /// </summary>
    /// <example>
    /// Use Dialect flags enum values to filter out one or more dialects from test runs
    /// <code>
    /// dotnet test --filter TestCategory=SqlServer // filters SqlServer tests for all dialects/db versions
    /// dotnet test --filter TestCategory=MySql5_5 // filters MySql tests for db version v5.5 
    /// </code>
    /// </example>
    public class TestFixtureOrmLiteDialectsAttribute : TestFixtureSourceAttribute, IApplyToTest
    {
        public static IEnumerable<Dialect> DialectValues = EnumUtils.GetValues<Dialect>();
        public static IEnumerable<Dialect> GetAllFlags(Dialect dialect)  
        {
            return DialectValues.Where(x => x.HasFlag(dialect)).ToArray();
        }
        
        public TestFixtureOrmLiteDialectsAttribute(TestDialect testDialect) : base(typeof(FixtureDataProvider), testDialect.ToString())
        {
        }

        public void ApplyToTest(Test test)
        {
            // Each dialect argument will set the category
            // this enables filtering of tests using nunit category filters
            // or dotnet test --filter category=<dialect> enum flag values
            var cats = test.Arguments.OfType<Dialect>().SelectMany(GetAllFlags).Distinct();
            cats.Each(e => test.Properties.Add(PropertyNames.Category, e.ToString()));
        }
    }

    /// <summary>
    /// Can be applied to a test to skip for specific dialects
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class IgnoreProviderAttribute : NUnitAttribute, IApplyToContext
    {
        private readonly Dialect _dialect;
        private readonly string _reason;

        public IgnoreProviderAttribute(Dialect dialect, string reason)
        {
            _dialect = dialect;
            _reason = reason;
        }

        public void ApplyToContext(TestExecutionContext context)
        {
            if (context.TestObject?.GetType()?.GetField("Dialect")?.GetValue(context.TestObject) is Dialect currentDialect)
            {
                if (_dialect.HasFlag(currentDialect) && context.CurrentTest.RunState != RunState.NotRunnable)
                {
                    // dialect match, skip test
                    var message = $"Ignoring for {currentDialect}: {_reason}";
                    context.CurrentTest.RunState = RunState.Skipped;
                    context.CurrentTest.Properties.Set(PropertyNames.SkipReason, message);
                    Assert.Ignore(message);

                    return;
                }

                return;
            }
            
            //context.OutWriter.WriteLine("IgnoreProviderAttribute usage invalid, will only work if class has 'Dialect' field set");
        }
    }

    /// <summary>
    /// Each enum value represents a property in <see cref="FixtureDataProvider"/>
    /// </summary>
    public enum TestDialect
    {
        Supported = 0,
        SupportedAllVersions = 1,
        Community = 2,
        CommunityAllVersions = 3,
        All = 4,
        Sqlite = 5,
        SqlServer = 6,
        MySql = 7,
        PostgreSql = 8,
        Firebird = 9,
        Oracle = 10,
        VistaDb
    }

    /// <summary>
    /// Predefined sets of dialects and versions for db integration testing, for use with <seealso cref="OrmLiteProvidersTestBase"/>
    /// Can be used with <seealso cref="TestFixtureOrmLiteDialectsAttribute"/>
    /// To set the default for <seealso cref="TestFixtureOrmLiteAttribute"/> change <see cref="TestConfig" />.DefaultDialects 
    /// </summary>
    public class FixtureDataProvider
    {
        /// <summary>
        /// All versions of all providers
        /// </summary>
        public static IEnumerable All
        {
            get
            {
                yield return new TestFixtureData(Dialect.Sqlite);
                yield return new TestFixtureData(Dialect.MySql5_5);
                yield return new TestFixtureData(Dialect.MySql10_1);
                yield return new TestFixtureData(Dialect.MySql10_2);
                yield return new TestFixtureData(Dialect.MySql10_3);
                yield return new TestFixtureData(Dialect.MySql10_4);
                yield return new TestFixtureData(Dialect.PostgreSql9);
                yield return new TestFixtureData(Dialect.PostgreSql10);
                yield return new TestFixtureData(Dialect.PostgreSql11);
                yield return new TestFixtureData(Dialect.SqlServer);
                yield return new TestFixtureData(Dialect.SqlServer2008);
                yield return new TestFixtureData(Dialect.SqlServer2012);
                yield return new TestFixtureData(Dialect.SqlServer2014);
                yield return new TestFixtureData(Dialect.SqlServer2016);
                yield return new TestFixtureData(Dialect.SqlServer2017);
                yield return new TestFixtureData(Dialect.Firebird);
                yield return new TestFixtureData(Dialect.Oracle10);
                yield return new TestFixtureData(Dialect.Oracle11);
                yield return new TestFixtureData(Dialect.Oracle12);
                yield return new TestFixtureData(Dialect.Oracle18);
                yield return new TestFixtureData(Dialect.VistaDb);
            }
        }

        public static IEnumerable VistaDb
        {
            get { yield return new TestFixtureData(Dialect.VistaDb); }
        }
        
        public static IEnumerable Sqlite
        {
            get { yield return new TestFixtureData(Dialect.Sqlite); }
        }

        /// <summary>
        /// All versions of all providers
        /// </summary>
        public static IEnumerable Oracle
        {
            get
            {
                yield return new TestFixtureData(Dialect.Oracle10);
                yield return new TestFixtureData(Dialect.Oracle11);
                yield return new TestFixtureData(Dialect.Oracle12);
                yield return new TestFixtureData(Dialect.Oracle18);
            }
        }

        /// <summary>
        /// All versions of progres providers
        /// </summary>
        public static IEnumerable PostgreSql
        {
            get
            {
                yield return new TestFixtureData(Dialect.PostgreSql9);
                yield return new TestFixtureData(Dialect.PostgreSql10);
                yield return new TestFixtureData(Dialect.PostgreSql11);
            }
        }

        /// <summary>
        /// All versions of mysql providers
        /// </summary>
        public static IEnumerable MySql
        {
            get
            {
                yield return new TestFixtureData(Dialect.MySql5_5);
                yield return new TestFixtureData(Dialect.MySql10_1);
                yield return new TestFixtureData(Dialect.MySql10_2);
                yield return new TestFixtureData(Dialect.MySql10_3);
                yield return new TestFixtureData(Dialect.MySql10_4);
            }
        }

        /// <summary>
        /// All versions of sqlserver providers
        /// </summary>
        public static IEnumerable SqlServer
        {
            get
            {
                yield return new TestFixtureData(Dialect.SqlServer);
                yield return new TestFixtureData(Dialect.SqlServer2008);
                yield return new TestFixtureData(Dialect.SqlServer2012);
                yield return new TestFixtureData(Dialect.SqlServer2014);
                yield return new TestFixtureData(Dialect.SqlServer2016);
                yield return new TestFixtureData(Dialect.SqlServer2017);
            }
        }
        
        // <summary>
        /// All versions of all providers
        /// </summary>
        public static IEnumerable Firebird
        {
            get { yield return new TestFixtureData(Dialect.Firebird); }
        }

        /// <summary>
        /// base versions of support providers
        /// </summary>
        public static IEnumerable Supported
        {
            get
            {
                yield return new TestFixtureData(Dialect.Sqlite);
                yield return new TestFixtureData(Dialect.MySql5_5);
                yield return new TestFixtureData(Dialect.PostgreSql9);
                yield return new TestFixtureData(Dialect.SqlServer);
            }
        }

        /// <summary>
        /// All versions of supported OrmLite providers
        /// </summary>
        public static IEnumerable SupportedAllVersions
        {
            get
            {
                yield return new TestFixtureData(Dialect.Sqlite);
                yield return new TestFixtureData(Dialect.MySql5_5);
                yield return new TestFixtureData(Dialect.MySql10_1);
                yield return new TestFixtureData(Dialect.MySql10_2);
                yield return new TestFixtureData(Dialect.MySql10_3);
                yield return new TestFixtureData(Dialect.MySql10_4);
                yield return new TestFixtureData(Dialect.PostgreSql9);
                yield return new TestFixtureData(Dialect.PostgreSql10);
                yield return new TestFixtureData(Dialect.PostgreSql11);
                yield return new TestFixtureData(Dialect.SqlServer2008);
                yield return new TestFixtureData(Dialect.SqlServer2012);
                yield return new TestFixtureData(Dialect.SqlServer2014);
                yield return new TestFixtureData(Dialect.SqlServer2016);
                yield return new TestFixtureData(Dialect.SqlServer2017);
            }
        }

        /// <summary>
        /// All base versions of community OrmLite providers
        /// </summary>
        public static IEnumerable Community
        {
            get
            {
                yield return new TestFixtureData(Dialect.Firebird);
                yield return new TestFixtureData(Dialect.Oracle10);
                yield return new TestFixtureData(Dialect.VistaDb);
            }
        }

        /// <summary>
        /// All versions of community OrmLite providers
        /// </summary>
        public static IEnumerable CommunityAllVersions
        {
            get
            {
                yield return new TestFixtureData(Dialect.Firebird);
                yield return new TestFixtureData(Dialect.Oracle10);
                yield return new TestFixtureData(Dialect.Oracle11);
                yield return new TestFixtureData(Dialect.Oracle12);
                yield return new TestFixtureData(Dialect.Oracle18);
                yield return new TestFixtureData(Dialect.VistaDb);
            }
        }

        
    }
}