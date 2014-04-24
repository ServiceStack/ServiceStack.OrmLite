using System;
using System.Data;
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

    public enum Dialect
    {
        Sqlite,
        SqlServer,
        PostgreSql,
        MySql,
        SqlServerMdf,
        Oracle,
    }

	public class OrmLiteTestBase
	{
	    protected virtual string ConnectionString { get; set; }

		protected string GetConnectionString()
		{
			return GetFileConnectionString();
		}

	    public static OrmLiteConnectionFactory CreateSqlServerDbFactory()
	    {
            var dbFactory = new OrmLiteConnectionFactory(Config.SqlServerBuildDb, SqlServerDialect.Provider);
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
	    public Dialect Dialect = Dialect.Sqlite;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			LogManager.LogFactory = new ConsoleLogFactory();

		    switch (Dialect)
		    {
		        case Dialect.Sqlite:
                    OrmLiteConfig.DialectProvider = SqliteDialect.Provider;
                    ConnectionString = GetFileConnectionString();
                    ConnectionString = ":memory:";
                    return;
                case Dialect.SqlServer:
                    OrmLiteConfig.DialectProvider = SqlServerDialect.Provider;
                    ConnectionString = Config.SqlServerBuildDb;
                    return;
                case Dialect.MySql:
                    OrmLiteConfig.DialectProvider = MySqlDialect.Provider;
                    ConnectionString = "Server=localhost;Database=test;UID=root;Password=test";
                    return;
                case Dialect.PostgreSql:
                    OrmLiteConfig.DialectProvider = PostgreSqlDialect.Provider;
                    ConnectionString = "Server=localhost;Port=5432;User Id=test;Password=test;Database=test;Pooling=true;MinPoolSize=0;MaxPoolSize=200";
                    return;
                case Dialect.SqlServerMdf:
                    ConnectionString = "~/App_Data/Database1.mdf".MapAbsolutePath();			
                    ConnectionString = Config.GetDefaultConnection();
                    return;
            }
		}

		public void Log(string text)
		{
			Console.WriteLine(text);
		}

        public IDbConnection InMemoryDbConnection { get; set; }

        public virtual IDbConnection OpenDbConnection(string connString = null)
        {
            connString = connString ?? ConnectionString;
            if (connString == ":memory:")
            {
                if (InMemoryDbConnection == null)
                {
                    var dbConn = connString.OpenDbConnection();
                    InMemoryDbConnection = new OrmLiteConnectionWrapper(dbConn)
                    {
                        DialectProvider = OrmLiteConfig.DialectProvider,
                        AutoDisposeConnection = false,
                    };                    
                }

                return InMemoryDbConnection;
            }

            return connString.OpenDbConnection();            
        }

        protected void SuppressIfOracle(string reason, params object[] args)
        {
            // Not Oracle if this base class used
        }
	}

    public static class TestHelpers
    {
        public static string NormalizeSql(this string sql)
        {
            return sql.ToLower().Replace("\"", "").Replace("`", "").Replace("_","");
        }
    }
}