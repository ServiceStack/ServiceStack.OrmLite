using System;
using System.Configuration;
using System.Data;
using NUnit.Framework;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.SqlServerTests
{
    public class OrmLiteTestBase
    {
        protected virtual string ConnectionString { get; set; }

        public IDbConnection Db { get; set; }

        [OneTimeSetUp]
        public virtual void TestFixtureSetUp()
        {
            LogManager.LogFactory = new ConsoleLogFactory();
            ConnectionString = GetConnectionString();
            OrmLiteConfig.DialectProvider = SqlServerDialect.Provider;
        }

        public void Log(string text)
        {
            Console.WriteLine(text);
        }

        public string GetConnectionString() => ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;

        public virtual IDbConnection OpenDbConnection(string connString = null, IOrmLiteDialectProvider dialectProvider = null)
        {
            dialectProvider = dialectProvider ?? OrmLiteConfig.DialectProvider;
            connString = connString ?? ConnectionString;
            return new OrmLiteConnectionFactory(connString, dialectProvider).OpenDbConnection();
        }
    }
}
