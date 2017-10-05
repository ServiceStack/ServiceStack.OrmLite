using System;
using System.Configuration;
using System.Data;
using NUnit.Framework;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.MySql.Tests
{
	public class OrmLiteTestBase
	{
		protected virtual string ConnectionString { get; set; }

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			LogManager.LogFactory = new ConsoleLogFactory();

		    OrmLiteConfig.DialectProvider = MySqlDialectProvider.Instance;
		    ConnectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;
		}

		public void Log(string text)
		{
			Console.WriteLine(text);
		}

        public virtual IDbConnection OpenDbConnection(string connString = null)
        {
            connString = connString ?? ConnectionString;
            return connString.OpenDbConnection();
        }
    }
}