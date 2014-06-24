using System.Data;
using System.Data.Common;
using NUnit.Framework;

namespace ServiceStack.OrmLite.VistaDB.Tests
{
    [TestFixture]
    public class TestVistaDb
    {
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
            // Loads the factory
            DbProviderFactory factory = DbProviderFactories.GetFactory(connectionString.ProviderName);
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