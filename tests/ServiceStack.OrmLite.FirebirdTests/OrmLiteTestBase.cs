using System;
using System.IO;
using System.Configuration;
using NUnit.Framework;
using ServiceStack.Common.Utils;
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;
using ServiceStack.OrmLite.Firebird;

namespace ServiceStack.OrmLite.FirebirdTests
{
	public class OrmLiteTestBase
	{		
		protected virtual string ConnectionString { get; set; }

		protected string GetFileConnectionString()
		{
			// add ormlite-tests.fdb = D:\\ormlite-tests.fdb to your firebird  alias.conf 
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

			OrmLiteConfig.DialectProvider = FirebirdOrmLiteDialectProvider.Instance;
			ConnectionString = GetFileConnectionString();
		}

		public void Log(string text)
		{
			Console.WriteLine(text);
		}
	}
}