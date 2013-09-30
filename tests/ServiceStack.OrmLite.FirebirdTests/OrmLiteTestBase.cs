using System;
using System.Data;
using NUnit.Framework;
using ServiceStack.Logging;
using ServiceStack.OrmLite.Firebird;

namespace ServiceStack.OrmLite.FirebirdTests
{
	public class OrmLiteTestBase
	{		
		protected virtual string ConnectionString { get; set; }

		protected string GetFileConnectionString()
		{
			// add ormlite-tests.fdb = {PATH TO FDB FILE} D:\\ormlite-tests.fdb to your firebird  alias.conf 
			return "User=SYSDBA;Password=masterkey;Database=ormlite-tests.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;MinPoolSize=0;MaxPoolSize=100;";
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

        public IDbConnection OpenDbConnection(string connString = null)
        {
            connString = connString ?? ConnectionString;
            return connString.OpenDbConnection();
        }
    }
}
