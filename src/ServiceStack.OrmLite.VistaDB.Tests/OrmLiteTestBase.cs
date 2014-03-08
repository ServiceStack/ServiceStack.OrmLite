using System;
using System.Configuration;
using System.Data;
using NUnit.Framework;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.VistaDB.Tests
{
    public class OrmLiteTestBase
    {
        protected virtual string ConnectionString { get; set; }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogManager.LogFactory = new ConsoleLogFactory();

            OrmLiteConfig.DialectProvider = VistaDB4Dialect.Provider;
            ConnectionString = "Data Source=ci.vdb4;";
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
