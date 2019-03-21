using System;
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
using NUnit.Framework.Internal.Builders;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Tests
{
    /// <summary>
    /// Use this base class in conjunction with one or more <seealso cref="TestFixtureOrmLiteAttribute"/>
    /// attributes to repeat tests for each db dialect.
    /// Alternatively, specify <seealso cref="TestFixtureOrmLiteDialectsAttribute"/>
    /// to repeat tests for each flag of <seealso cref="Dialect" /> 
    /// </summary>
    /// <example>
    /// <code>
    /// // example
    /// [TestFixtureOrmLite] // all configured dialects
    /// [TestFixtureOrmLiteDialects(Dialect.Supported)] // all base versions of supported dialects
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

        public readonly DialectFeatures DialectFeatures;

        /// <summary>
        /// The current DialogProvider instance
        /// </summary>
        protected IOrmLiteDialectProvider DialectProvider;
        
        // The Database Factory
        protected OrmLiteConnectionFactory DbFactory { get; }

        protected TestLogFactory Log => LogSetup.Instance; 

        /// <summary>
        /// The test logs
        /// TODO can scoped logs be created per provider?
        /// </summary>
        public IList<KeyValuePair<TestLogger.Levels, string>> Logs => TestLogger.GetLogs(); 
        
        public OrmLiteProvidersTestBase(Dialect dialect)
        {
            Dialect = dialect;
            DialectFeatures = new DialectFeatures(Dialect);
            DbFactory = OrmLiteConnectionFactory.NamedConnections[Dialect.ToString()].CreateCopy();
            DialectProvider = DbFactory.DialectProvider;

            if (OrmLiteConfig.DialectProvider == null) OrmLiteConfig.DialectProvider = DialectProvider;
        }

        public virtual IDbConnection OpenDbConnection() => DbFactory.OpenDbConnection();
        public virtual Task<IDbConnection> OpenDbConnectionAsync() => DbFactory.OpenDbConnectionAsync();
    }

    /// <summary>
    /// Holds dialect flags applicable to specific SQL language features
    /// </summary>
    public class DialectFeatures
    {
        public readonly bool RowOffset;
        
        public DialectFeatures(Dialect dialect)
        {
            RowOffset = (Dialect.SqlServer2012 | Dialect.SqlServer2014 | Dialect.SqlServer2016 | Dialect.SqlServer2017).HasFlag(dialect);
        }
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
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            var dbFactory = InitDbFactory();
            InitDbScripts(dbFactory);
        }

        private OrmLiteConnectionFactory InitDbFactory()
        {
            // init DbFactory, should be mainly ignored in tests as they should always ask for a provider specific named connection
            var dbFactory = new OrmLiteConnectionFactory(TestConfig.DefaultConnection, TestConfig.DefaultProvider);

            dbFactory.RegisterConnection(Dialect.PostgreSql9.ToString(), TestConfig.PostgresDb_9, PostgreSqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.PostgreSql10.ToString(), TestConfig.PostgresDb_10, PostgreSqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.PostgreSql11.ToString(), TestConfig.PostgresDb_11, PostgreSqlDialect.Provider);

            dbFactory.RegisterConnection(Dialect.MySqlConnector.ToString(), TestConfig.MySqlDb_5_5, MySqlConnectorDialect.Provider);
            
            dbFactory.RegisterConnection(Dialect.MySql5_5.ToString(), TestConfig.MySqlDb_5_5, MySql55Dialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_1.ToString(), TestConfig.MySqlDb_10_1, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_2.ToString(), TestConfig.MySqlDb_10_2, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_3.ToString(), TestConfig.MySqlDb_10_3, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_4.ToString(), TestConfig.MySqlDb_10_4, MySqlDialect.Provider);

            dbFactory.RegisterConnection(Dialect.Sqlite.ToString(), TestConfig.SqliteMemoryDb, SqliteDialect.Provider);

            dbFactory.RegisterConnection(Dialect.SqlServer.ToString(), TestConfig.SqlServerBuildDb, SqlServerDialect.Provider);

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

        private void InitDbScripts(OrmLiteConnectionFactory dbFactory)
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
        }
    }

    /// <summary>
    /// Repeats tests for all dialect versions from <see cref="TestConfig.DefaultDialects"/>
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
    /// Repeats tests for all Dialect flags specified.
    /// Also sets nunit categories for each dialect flag which 
    /// enables adhoc filtering of tests by using Dialect enum flag values
    /// as category names in the test runner
    /// </summary>
    /// <example>
    /// Use Dialect flags enum values to filter out one or more dialects from test runs
    /// <code>
    /// dotnet test --filter TestCategory=SqlServer // filters SqlServer tests for all dialects/db versions
    /// dotnet test --filter TestCategory=MySql5_5 // filters MySql tests for db version v5.5 
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class TestFixtureOrmLiteDialectsAttribute : NUnitAttribute, IFixtureBuilder2
    {
        private static readonly IEnumerable<Dialect> DialectValues = EnumUtils.GetValues<Dialect>();
 
        public static IEnumerable<Dialect> GetAllFlags(Dialect dialect)
        {
            return DialectValues.Where(x => x.HasFlag(dialect)).ToArray();
        }
        
        private readonly Dialect _dialect;
        private readonly NUnitTestFixtureBuilder _builder = new NUnitTestFixtureBuilder();

        public TestFixtureOrmLiteDialectsAttribute(Dialect dialect)
        {
            _dialect = dialect;
        }

        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            return BuildFrom(typeInfo, null);
        }
        
        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo, IPreFilter filter)
        {
            var dialectArgs = new List<TestFixtureData>();
            
            if(_dialect.HasFlag(Dialect.Sqlite)) dialectArgs.Add(new TestFixtureData(Dialect.Sqlite));
            if(_dialect.HasFlag(Dialect.MySql5_5)) dialectArgs.Add(new TestFixtureData(Dialect.MySql5_5));
            if(_dialect.HasFlag(Dialect.MySql10_1)) dialectArgs.Add(new TestFixtureData(Dialect.MySql10_1));
            if(_dialect.HasFlag(Dialect.MySql10_2)) dialectArgs.Add(new TestFixtureData(Dialect.MySql10_2));
            if(_dialect.HasFlag(Dialect.MySql10_3)) dialectArgs.Add(new TestFixtureData(Dialect.MySql10_3));
            if(_dialect.HasFlag(Dialect.MySql10_4)) dialectArgs.Add(new TestFixtureData(Dialect.MySql10_4));
            if(_dialect.HasFlag(Dialect.PostgreSql9)) dialectArgs.Add(new TestFixtureData(Dialect.PostgreSql9));
            if(_dialect.HasFlag(Dialect.PostgreSql10)) dialectArgs.Add(new TestFixtureData(Dialect.PostgreSql10));
            if(_dialect.HasFlag(Dialect.PostgreSql11)) dialectArgs.Add(new TestFixtureData(Dialect.PostgreSql11));
            if(_dialect.HasFlag(Dialect.SqlServer)) dialectArgs.Add(new TestFixtureData(Dialect.SqlServer));
            if(_dialect.HasFlag(Dialect.SqlServer2008)) dialectArgs.Add(new TestFixtureData(Dialect.SqlServer2008));
            if(_dialect.HasFlag(Dialect.SqlServer2012)) dialectArgs.Add(new TestFixtureData(Dialect.SqlServer2012));
            if(_dialect.HasFlag(Dialect.SqlServer2014)) dialectArgs.Add(new TestFixtureData(Dialect.SqlServer2014));
            if(_dialect.HasFlag(Dialect.SqlServer2016)) dialectArgs.Add(new TestFixtureData(Dialect.SqlServer2016));
            if(_dialect.HasFlag(Dialect.SqlServer2017)) dialectArgs.Add(new TestFixtureData(Dialect.SqlServer2017));
            if(_dialect.HasFlag(Dialect.Firebird)) dialectArgs.Add(new TestFixtureData(Dialect.Firebird));
            if(_dialect.HasFlag(Dialect.Oracle10)) dialectArgs.Add(new TestFixtureData(Dialect.Oracle10));
            if(_dialect.HasFlag(Dialect.Oracle11)) dialectArgs.Add(new TestFixtureData(Dialect.Oracle11));
            if(_dialect.HasFlag(Dialect.Oracle12)) dialectArgs.Add(new TestFixtureData(Dialect.Oracle12));
            if(_dialect.HasFlag(Dialect.Oracle18)) dialectArgs.Add(new TestFixtureData(Dialect.Oracle18));
            if(_dialect.HasFlag(Dialect.VistaDb)) dialectArgs.Add(new TestFixtureData(Dialect.VistaDb));

            foreach (var parms in dialectArgs)
            {
                // ignore test if not in TestConfig but add as ignored to explain why
                if (!TestConfig.DefaultDialects.HasFlag((Dialect)parms.Arguments[0]))
                    parms.Ignore($"Dialect not included in TestConfig.DefaultDialects value {TestConfig.DefaultDialects}");

                parms.Properties.Add(PropertyNames.Category, parms.Arguments[0].ToString());
                yield return _builder.BuildFrom(typeInfo, filter, parms);
            }
        }
    }

    /// <summary>
    /// Can be applied to a test to skip for specific dialects
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class IgnoreProviderAttribute : NUnitAttribute, ITestAction
    {
        private readonly Dialect _dialect;
        private readonly string _reason;

        public IgnoreProviderAttribute(Dialect dialect, string reason)
        {
            _dialect = dialect;
            _reason = reason;
        }

        public void BeforeTest(ITest test)
        {
            // get the dialect from either the class or method parent
            // and if dialect matches, ignore test
            var dialects = test.TestType == "TestMethod" ? test.Parent.Arguments.OfType<Dialect>() : test.Arguments.OfType<Dialect>();
            foreach (var dialect in dialects)
            {
                if (_dialect.HasFlag(dialect) && test.RunState != RunState.NotRunnable)
                {
                    Assert.Ignore($"Ignoring for {dialect}: {_reason}");
                }
            }
        }

        public void AfterTest(ITest test)
        {
        }

        public ActionTargets Targets { get; }
    }
}