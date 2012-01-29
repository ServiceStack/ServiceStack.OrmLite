using System;
using System.Configuration;
using System.IO;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using ServiceStack.Common.Utils;
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;
using ServiceStack.OrmLite.MySql;

namespace ServiceStack.OrmLite.Tests
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
	}
}