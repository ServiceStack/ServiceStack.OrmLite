using System;
using System.Data;
using System.IO;
using NUnit.Framework;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.VistaDB.Tests
{
    public class OrmLiteTestBase
    {
        protected virtual string ConnectionString { get; set; }

        protected virtual string DataFileName { get; private set; }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogManager.LogFactory = new ConsoleLogFactory();

            VistaDbDialect.Provider.UseLibraryFromGac = true;
            OrmLiteConfig.DialectProvider = VistaDbDialect.Provider;

            DataFileName = TestVistaDb.ExtractTestDatabaseFileToTempFile();

            ConnectionString = "Data Source=" + DataFileName + ";";
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (File.Exists(DataFileName))
                File.Delete(DataFileName);
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
