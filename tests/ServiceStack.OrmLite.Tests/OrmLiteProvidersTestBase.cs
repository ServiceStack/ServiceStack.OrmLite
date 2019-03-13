using System.Collections;
using System.Data;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests
{
    /// <summary>
    /// Predefined sets for db integration testing, for use with <seealso cref="OrmLiteProvidersTestBase"/>
    /// </summary>
    public class ProvidersFixtureData
    {
        public const string SupportedBase = nameof(Supported);
        public const string SupportedAll = nameof(SupportedAllVersions);
        public const  string CommunityBase = nameof(Community);
        public const  string CommunityAll = nameof(CommunityAllVersions);
            
        public static IEnumerable Supported
        {
            get
            {
                yield return new TestFixtureData(Dialect.Sqlite);
                yield return new TestFixtureData(Dialect.MySql);
                yield return new TestFixtureData(Dialect.PostgreSql);
                yield return new TestFixtureData(Dialect.SqlServer);
                yield return new TestFixtureData(Dialect.Oracle);
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
                yield return new TestFixtureData(Dialect.Oracle);
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
                yield return new TestFixtureData(Dialect.Oracle);
                yield return new TestFixtureData(Dialect.VistaDb);
            }
        }
    }
    
    /// <summary>
    /// Use this base class in conjunction with one or more <seealso cref="TestFixtureAttribute"/>
    /// attributes to repeat tests for each db dialect.
    /// Alternatively, specify <seealso cref="TestFixtureSourceAttribute"/>
    /// to repeat tests for pre-defined sets of <seealso cref="ProvidersFixtureData" /> 
    /// </summary>
    /// <code>
    /// // example
    /// [TestFixture(Dialect.Sqllite)]
    /// [TestFixtureSource(typeof(ProvidersFixtureData), ProvidersFixtureData.Supported)]
    /// public TestClass : OrmLiteProvidersTestBase {
    ///   public TestClass(Dialect dialect) : base(dialect) {}
    ///
    ///   // Test runs once per TestFixture and all TestFixtureSource specified providers
    ///   // or 
    ///   [Test]
    ///   public void SomeTestMethod() {
    ///     using(var db = OpenDbConnection()) {
    ///       // your db agnostic test code
    ///     }
    ///   }
    /// }
    /// </code>
    public abstract class OrmLiteProvidersTestBase
    {
        /// <summary>
        /// The current db dialect
        /// </summary>
        public Dialect Dialect;

        public readonly IOrmLiteDialectProvider DialectProvider;
        
        // The Database Factory
        protected readonly OrmLiteConnectionFactory DbFactory;

        public OrmLiteProvidersTestBase(Dialect dialect = default)
        {
            DbFactory = Init();
            
            Dialect = dialect;
            DialectProvider = DbFactory.Open(dialect.ToString()).GetDialectProvider();
            
            InitDbScripts();
        }
        
        public virtual IDbConnection OpenDbConnection()
        {
            return DbFactory.OpenDbConnection(Dialect.ToString());
        }

        public virtual Task<IDbConnection> OpenDbConnectionAsync()
        {
            return DbFactory.OpenDbConnectionAsync(Dialect.ToString());
        }

        private OrmLiteConnectionFactory Init()
        {
            var dbFactory = new OrmLiteConnectionFactory(Config.DefaultConnection, Config.DefaultProvider);

            dbFactory.RegisterConnection(Dialect.PostgreSql.ToString(), Config.PostgresDb_9, PostgreSqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.PostgreSql9.ToString(), Config.PostgresDb_9, PostgreSqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.PostgreSql10.ToString(), Config.PostgresDb_10, PostgreSqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.PostgreSql11.ToString(), Config.PostgresDb_11, PostgreSqlDialect.Provider);

            dbFactory.RegisterConnection(Dialect.MySql.ToString(), Config.MySqlDb_5_5, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql5_5.ToString(), Config.MySqlDb_5_5, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_1.ToString(), Config.MySqlDb_10_1, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_2.ToString(), Config.MySqlDb_10_2, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_3.ToString(), Config.MySqlDb_10_3, MySqlDialect.Provider);
            dbFactory.RegisterConnection(Dialect.MySql10_4.ToString(), Config.MySqlDb_10_4, MySqlDialect.Provider);
            
            dbFactory.RegisterConnection(Dialect.Sqlite.ToString(), Config.SqliteMemoryDb, SqliteDialect.Provider);
            
            dbFactory.RegisterConnection(Dialect.SqlServer.ToString(), Config.SqlServerBuildDb, SqlServerDialect.Provider);
            dbFactory.RegisterConnection(Dialect.SqlServer2017.ToString(), Config.SqlServerBuildDb, SqlServer2017Dialect.Provider);
            dbFactory.RegisterConnection(Dialect.SqlServer2016.ToString(), Config.SqlServerBuildDb, SqlServer2016Dialect.Provider);
            dbFactory.RegisterConnection(Dialect.SqlServer2014.ToString(), Config.SqlServerBuildDb, SqlServer2014Dialect.Provider);
            dbFactory.RegisterConnection(Dialect.SqlServer2012.ToString(), Config.SqlServerBuildDb, SqlServer2012Dialect.Provider);
            dbFactory.RegisterConnection(Dialect.SqlServer2008.ToString(), Config.SqlServerBuildDb, SqlServer2008Dialect.Provider);

            dbFactory.RegisterConnection(Dialect.Oracle.ToString(), Config.OracleDb, OracleDialect.Provider);
            
            dbFactory.RegisterConnection(Dialect.Firebird.ToString(), Config.FirebirdDb_3, FirebirdDialect.Provider);

            return dbFactory;
        }

        private void InitDbScripts()
        {
            var pgInit = @"CREATE EXTENSION IF NOT EXISTS ""uuid-ossp""";
            using (var pg9 = DbFactory.OpenDbConnection(Dialect.PostgreSql9.ToString()))
            using (var pg10 = DbFactory.OpenDbConnection(Dialect.PostgreSql10.ToString()))
            using (var pg11 = DbFactory.OpenDbConnection(Dialect.PostgreSql11.ToString()))
            {
                pg9.ExecuteSql(pgInit);
                pg10.ExecuteSql(pgInit);
                pg11.ExecuteSql(pgInit);
            }
        }
    }
}