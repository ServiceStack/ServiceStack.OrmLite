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
					GetFileConnectionString(),
					OracleDialect.Provider)
			};
			return testBase.OpenDbConnection();
		}

		public static string GetFileConnectionString()
		{
#if NETCORE
			return ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location).ConnectionStrings.ConnectionStrings["testDb"].ConnectionString;
#else
			return ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;
#endif
		}
	}

	public class OrmLiteTestBase
	{		
		protected virtual string ConnectionString { get; set; }



		protected void CreateNewDatabase()
		{
			ConnectionString = Config.GetFileConnectionString();
		}

		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			LogManager.LogFactory = new ConsoleLogFactory();

			OrmLiteConfig.DialectProvider = OracleDialect.Provider;
			OrmLiteConfig.ClearCache();
			ConnectionString = Config.GetFileConnectionString();
			DbFactory = new OrmLiteConnectionFactory(ConnectionString, OracleDialect.Provider);
		}

		public void Log(string text)
		{
			Console.WriteLine(text);
		}

		//public Dialect Dialect = Dialect.Oracle;
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
