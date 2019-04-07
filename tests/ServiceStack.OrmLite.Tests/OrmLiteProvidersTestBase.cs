using System;
using System.Collections.Generic;
using System.Data;
#if !NETCORE
using System.Data.Common;
using System.IO;
#endif
using System.Data.SqlClient;
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
        protected OrmLiteConnectionFactory DbFactory { get; set; }

        protected TestLogFactory Log => OrmLiteFixtureSetup.LogFactoryInstance; 

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

            if (OrmLiteConfig.DialectProvider == null) 
                OrmLiteConfig.DialectProvider = DialectProvider;
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
        public readonly bool SchemaSupport;
        
        public DialectFeatures(Dialect dialect)
        {
            // Tag dialects with supported features and use to toggle in tests
            RowOffset = (Dialect.SqlServer2012 | Dialect.SqlServer2014 | Dialect.SqlServer2016 | Dialect.SqlServer2017).HasFlag(dialect);
            SchemaSupport = !(Dialect.Sqlite | Dialect.AnyPostgreSql).HasFlag(dialect);
        }
    }

    [SetUpFixture]
    public class OrmLiteFixtureSetup
    {
        public static TestLogFactory LogFactoryInstance => new TestLogFactory();
        
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            // init logging, for use in tests, filter by type?
            LogManager.LogFactory = LogFactoryInstance;
            
            // setup db factories
            var dbFactory = TestConfig.InitDbFactory();
        }

    }

    /// <summary>
    /// Repeats tests for all dialect versions from <see cref="TestConfig.Dialect"/>
    /// To restrict tests to specific dialects use <see cref="TestFixtureOrmLiteDialectsAttribute"/>
    /// To filter tests for specific dialects use <see cref="IgnoreDialectAttribute"/>
    /// </summary>
    /// <inheritdoc cref="TestFixtureOrmLiteDialectsAttribute"/>
    public class TestFixtureOrmLiteAttribute : TestFixtureOrmLiteDialectsAttribute
    {
        public TestFixtureOrmLiteAttribute() : base(TestConfig.Dialect)
        {
            // loads the dialects from TestConfig.DefaultDialects
            // which can be overridden using an environment variable
        }
    }

    /// <summary>
    /// Repeats tests for all Dialect flags specified.
    /// Also sets NUnit categories for each dialect flag which 
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
        private readonly Dialect dialect;
        private readonly NUnitTestFixtureBuilder _builder = new NUnitTestFixtureBuilder();
        private readonly string reason;

        public TestFixtureOrmLiteDialectsAttribute(Dialect dialect)
        {
            this.dialect = dialect;
            reason = $"Dialect not included in TestConfig.DefaultDialects value {TestConfig.Dialect}";
        }

        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            return BuildFrom(typeInfo, null);
        }
        
        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo, IPreFilter filter)
        {
            var fixtureData = new List<TestFixtureData>();

            if (dialect.HasFlag(Dialect.MySqlConnector)) fixtureData.Add(new TestFixtureData(Dialect.MySqlConnector));
            if (dialect.HasFlag(Dialect.Sqlite)) fixtureData.Add(new TestFixtureData(Dialect.Sqlite));
            if (dialect.HasFlag(Dialect.MySql5_5)) fixtureData.Add(new TestFixtureData(Dialect.MySql5_5));
            if (dialect.HasFlag(Dialect.MySql10_1)) fixtureData.Add(new TestFixtureData(Dialect.MySql10_1));
            if (dialect.HasFlag(Dialect.MySql10_2)) fixtureData.Add(new TestFixtureData(Dialect.MySql10_2));
            if (dialect.HasFlag(Dialect.MySql10_3)) fixtureData.Add(new TestFixtureData(Dialect.MySql10_3));
            if (dialect.HasFlag(Dialect.MySql10_4)) fixtureData.Add(new TestFixtureData(Dialect.MySql10_4));
            if (dialect.HasFlag(Dialect.PostgreSql9)) fixtureData.Add(new TestFixtureData(Dialect.PostgreSql9));
            if (dialect.HasFlag(Dialect.PostgreSql10)) fixtureData.Add(new TestFixtureData(Dialect.PostgreSql10));
            if (dialect.HasFlag(Dialect.PostgreSql11)) fixtureData.Add(new TestFixtureData(Dialect.PostgreSql11));
            if (dialect.HasFlag(Dialect.SqlServer)) fixtureData.Add(new TestFixtureData(Dialect.SqlServer));
            if (dialect.HasFlag(Dialect.SqlServer2008)) fixtureData.Add(new TestFixtureData(Dialect.SqlServer2008));
            if (dialect.HasFlag(Dialect.SqlServer2012)) fixtureData.Add(new TestFixtureData(Dialect.SqlServer2012));
            if (dialect.HasFlag(Dialect.SqlServer2014)) fixtureData.Add(new TestFixtureData(Dialect.SqlServer2014));
            if (dialect.HasFlag(Dialect.SqlServer2016)) fixtureData.Add(new TestFixtureData(Dialect.SqlServer2016));
            if (dialect.HasFlag(Dialect.SqlServer2017)) fixtureData.Add(new TestFixtureData(Dialect.SqlServer2017));
            if (dialect.HasFlag(Dialect.Firebird)) fixtureData.Add(new TestFixtureData(Dialect.Firebird));
            if (dialect.HasFlag(Dialect.Oracle10)) fixtureData.Add(new TestFixtureData(Dialect.Oracle10));
            if (dialect.HasFlag(Dialect.Oracle11)) fixtureData.Add(new TestFixtureData(Dialect.Oracle11));
            if (dialect.HasFlag(Dialect.Oracle12)) fixtureData.Add(new TestFixtureData(Dialect.Oracle12));
            if (dialect.HasFlag(Dialect.Oracle18)) fixtureData.Add(new TestFixtureData(Dialect.Oracle18));
            if (dialect.HasFlag(Dialect.VistaDb)) fixtureData.Add(new TestFixtureData(Dialect.VistaDb));

            foreach (var data in fixtureData)
            {
                // ignore test if not in TestConfig but add as ignored to explain why
                if (!TestConfig.Dialect.HasFlag((Dialect)data.Arguments[0]))
                    data.Ignore(reason);

                data.Properties.Add(PropertyNames.Category, data.Arguments[0].ToString());
                yield return _builder.BuildFrom(typeInfo, filter, data);
            }
        }
    }

    /// <summary>
    /// Can be applied to a test to skip for specific dialects
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class IgnoreDialectAttribute : NUnitAttribute, ITestAction
    {
        private readonly Dialect dialect;
        private readonly string reason;

        /// <summary>
        /// Ignore one or more specific dialects from testing
        /// </summary>
        /// <param name="dialect">The dialect flags to ignore</param>
        /// <param name="reason">The ignore reason that will be output in test results</param>
        public IgnoreDialectAttribute(Dialect dialect, string reason)
        {
            this.dialect = dialect;
            this.reason = reason;
        }

        public void BeforeTest(ITest test)
        {
            // get the dialect from either the class or method parent
            // and if dialect matches, ignore test
            var testDialects = test.TestType == "TestMethod" ? test.Parent.Arguments.OfType<Dialect>() : test.Arguments.OfType<Dialect>();
            foreach (var testDialect in testDialects)
            {
                if (this.dialect.HasFlag(testDialect) && test.RunState != RunState.NotRunnable)
                {
                    Assert.Ignore($"Ignoring for {testDialect}: {reason}");
                }
            }
        }

        public void AfterTest(ITest test)
        {
        }

        public ActionTargets Targets { get; }
    }
}