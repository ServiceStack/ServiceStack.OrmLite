using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using NUnit.Framework;

namespace ServiceStack.OrmLite.VistaDB.Tests
{
    [TestFixture]
    public class TestVistaDb
    {
        public static string ExtractTestDatabaseFileToTempFile()
        {
            var dataFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".vdb5");

            ExtractTestDatabaseFile(dataFileName);

            return dataFileName;
        }

        public static void ExtractTestDatabaseFile(string dataFileName)
        {
            var sourceStream = typeof(OrmLiteTestBase).Assembly
                .GetManifestResourceStream("ServiceStack.OrmLite.VistaDB.Tests.Resources.test.vdb5");

            using (sourceStream)
            using (var destStream = File.Create(dataFileName))
                sourceStream.CopyTo(destStream);
        }

        public static void CreateDatabase(DbProviderFactory factory)
        {
            using (var conn = factory.CreateConnection())
            using (var comm = conn.CreateCommand())
            {
                comm.CommandText = @"CREATE DATABASE '|DataDirectory|\Database.vdb5', PAGE SIZE 4, LCID 1033, CASE SENSITIVE FALSE;";
                comm.ExecuteNonQuery();
            }
        }

        [Test]
        public void TestVistaDB()
        {
            string VersionInfo = null;
            for (int i = 0; i < System.Configuration.ConfigurationManager.ConnectionStrings.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine(System.Configuration.ConfigurationManager.ConnectionStrings[i].ToString());
            }

            string connectionName = "myVDBConnection";
            // Find this connection string in the app.config
            System.Configuration.ConnectionStringSettings connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionString == null)
            {
                VersionInfo = "Failed to load connectionString from config file";
                Assert.Fail(VersionInfo);
            }

            DbProviderFactory factory = null;
            // Loads the factory
            try
            {
                factory = DbProviderFactories.GetFactory(connectionString.ProviderName);
            }
            catch
            {
                Assert.Ignore("VistaDB library should be copied locally or installed into GAC.");
            }

            try
            {
                CreateDatabase(factory);
            }
            catch { }

            // After this it looks pretty normal
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString.ConnectionString;
                connection.Open();
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT @@VERSION;";
                    command.CommandType = CommandType.Text;

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string result = reader.GetString(0);
                            if (!reader.IsDBNull(0))
                            {
                                VersionInfo = result;
                                Assert.True(true, "Connected to: " + VersionInfo);
                                return;
                            }
                        }
                    }
                }
            }
            VersionInfo = string.Empty;
            Assert.Fail("Could not connect to VistaDB");
        }
    }

}