using System;
using System.Configuration;
using System.Data;
using NUnit.Framework;
using ServiceStack.Logging;
using ServiceStack.OrmLite.Firebird;
using ServiceStack.OrmLite.Tests;

namespace ServiceStack.OrmLite.FirebirdTests
{
	public class OrmLiteTestBase
	{		
		protected virtual string ConnectionString { get; set; }

		protected string GetFileConnectionString()
		{
            return TestConfig.FirebirdDb_3;
        }

		protected void CreateNewDatabase()
		{
			ConnectionString = GetFileConnectionString();
		}

		[OneTimeSetUp]
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

        public IDbConnection OpenDbConnection(string connString = null)
        {
            connString = connString ?? ConnectionString;
            return connString.OpenDbConnection();
        }
    }
}
