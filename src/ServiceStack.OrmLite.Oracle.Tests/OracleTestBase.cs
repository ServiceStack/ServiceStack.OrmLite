using System;
using System.Configuration;
using System.Data;
using System.IO;
using NUnit.Framework;
using ServiceStack.Common.Utils;
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;
using ServiceStack.OrmLite.Oracle;


namespace ServiceStack.OrmLite.Oracle.Tests
{
    public class OracleTestBase
	{
		protected virtual string ConnectionString { get; set; }

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			LogManager.LogFactory = new ConsoleLogFactory();

            OrmLiteConfig.DialectProvider = OracleOrmLiteDialectProvider.Instance;
			OrmLiteConfig.ClearCache();
		    ConnectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;
		}
    
        public IDbConnection OpenDbConnection(string connString = null)
        {
            connString = connString ?? ConnectionString;
            return connString.OpenDbConnection();
        }
    }
}