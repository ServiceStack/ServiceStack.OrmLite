using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Threading;
using NUnit.Framework;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.VistaDB5.Tests
{
    public class OrmLiteTestBase
    {
        protected virtual string ConnectionString { get; set; }

        protected virtual string DataFileName { get; private set; }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogManager.LogFactory = new ConsoleLogFactory();

            OrmLiteConfig.DialectProvider = VistaDB5Dialect.Provider;

            var dataFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".vdb5");
            var sourceStream = typeof(OrmLiteTestBase).Assembly
                .GetManifestResourceStream("ServiceStack.OrmLite.VistaDB.Tests.Resources.test.vdb5");

            using (sourceStream)
            using (var destStream = File.Create(dataFileName))
                sourceStream.CopyTo(destStream);

            DataFileName = dataFileName;
            ConnectionString = "Data Source=" + dataFileName + ";";
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
