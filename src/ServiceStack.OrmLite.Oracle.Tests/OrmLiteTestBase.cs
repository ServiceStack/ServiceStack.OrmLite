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
            var testBase = new OrmLiteTestBase
            {
                DbFactory = new OrmLiteConnectionFactory(
                    ConfigurationManager.ConnectionStrings["testDb"].ConnectionString,
                    OracleDialect.Provider)
            };
            return testBase.OpenDbConnection();
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

            OrmLiteConfig.DialectProvider = OracleDialect.Provider;
            OrmLiteConfig.ClearCache();
			ConnectionString = GetFileConnectionString();
            DbFactory = new OrmLiteConnectionFactory(ConnectionString, OracleDialect.Provider);
		}

		public void Log(string text)
		{
			Console.WriteLine(text);
		}

        public Dialect Dialect = Dialect.Oracle;
        public OrmLiteConnectionFactory DbFactory;

        public virtual IDbConnection OpenDbConnection()
        {
            return DbFactory.OpenDbConnection();
        }

        protected void SuppressIfOracle(string reason, params object[] args)
        {
            Assert.Ignore(reason, args);
        }
    }
}
