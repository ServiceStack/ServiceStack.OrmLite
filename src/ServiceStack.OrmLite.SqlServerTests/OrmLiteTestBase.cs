using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;
using ServiceStack.OrmLite.SqlServer;

namespace ServiceStack.OrmLite.SqlServerTests
{
    public class OrmLiteTestBase
    {
        protected virtual string ConnectionString { get; set; }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogManager.LogFactory = new ConsoleLogFactory();

            OrmLiteConfig.DialectProvider = SqlServerOrmLiteDialectProvider.Instance;
            ConnectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;
        }

        public void Log(string text)
        {
            Console.WriteLine(text);
        }
    }
}
