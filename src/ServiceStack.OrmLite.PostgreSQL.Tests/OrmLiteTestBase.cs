using System;
using System.Configuration;
using System.Data;
using System.IO;
using NUnit.Framework;
using ServiceStack.Common.Utils;
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;
using ServiceStack.OrmLite.PostgreSQL;


namespace ServiceStack.OrmLite.Tests
{
	public class OrmLiteTestBase
	{
		protected virtual string ConnectionString { get; set; }

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			LogManager.LogFactory = new ConsoleLogFactory();

		    OrmLiteConfig.DialectProvider =  PostgreSQLDialectProvider.Instance;
			OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();
			OrmLiteConfig.ClearCache();
		    ConnectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;
		}

		public void Log(string text)
		{
			Console.WriteLine(text);
		}
    
        public IDbConnection OpenDbConnection(string connString = null)
        {
            connString = connString ?? ConnectionString;
            return connString.OpenDbConnection();
        }
    }
}