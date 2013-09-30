using System.Configuration;
using System.Data;
using NUnit.Framework;
using ServiceStack.Logging;


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