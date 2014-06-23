using System;
using System.Configuration;
using System.Data;
using NUnit.Framework;
using ServiceStack.Logging;
using ServiceStack.OrmLite.Oracle;

namespace ServiceStack.OrmLite.Tests
{
    public class Config
    {
        public static IDbConnection OpenDbConnection()
        {
            var testBase = new OrmLiteTestBase();
            var connectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;
            return testBase.OpenDbConnection(connectionString);
        }
    }

    public class OrmLiteTestBase
	{		
		protected virtual string ConnectionString { get; set; }

		protected virtual string GetFileConnectionString()
		{
            return ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;
        }

		protected void CreateNewDatabase()
		{
			ConnectionString = GetFileConnectionString();
		}

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			LogManager.LogFactory = new ConsoleLogFactory();

            OrmLiteConfig.DialectProvider = OracleOrmLiteDialectProvider.Instance;
            OrmLiteConfig.ClearCache();
			ConnectionString = GetFileConnectionString();
		}

		public void Log(string text)
		{
			Console.WriteLine(text);
		}

        public Dialect Dialect = Dialect.Oracle;

        public virtual IDbConnection OpenDbConnection(string connString = null)
        {
            connString = connString ?? ConnectionString;
            return connString.OpenDbConnection();
        }

        protected void SuppressIfOracle(string reason, params object[] args)
        {
            Assert.Ignore(reason, args);
        }
    }
}
