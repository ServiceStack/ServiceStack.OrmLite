using System;
using System.IO;
using NUnit.Framework;
using ServiceStack.Common.Utils;
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;
using ServiceStack.OrmLite.Sqlite;
using ServiceStack.OrmLite.SqlServer;

namespace ServiceStack.OrmLite.Tests
{
    public class Config
    {
        public static string SqliteMemoryDb = ":memory:";
        public static string SqliteFileDb = "~/App_Data/db.sqlite".MapAbsolutePath();
        public static string SqlServerDb = "~/App_Data/Database1.mdf".MapAbsolutePath();
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
	        var dbFactory = new OrmLiteConnectionFactory(Config.SqlServerDb,
	            SqlServerOrmLiteDialectProvider.Instance);
	        return dbFactory;
	    }

	    protected string GetFileConnectionString()
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

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			LogManager.LogFactory = new ConsoleLogFactory();

			OrmLiteConfig.DialectProvider = SqliteOrmLiteDialectProvider.Instance;
			ConnectionString = GetFileConnectionString();
			//ConnectionString = ":memory:";

			//OrmLiteConfig.DialectProvider = SqlServerOrmLiteDialectProvider.Instance;
			//ConnectionString = "~/App_Data/Database1.mdf".MapAbsolutePath();			
		}

		public void Log(string text)
		{
			Console.WriteLine(text);
		}
	}
}