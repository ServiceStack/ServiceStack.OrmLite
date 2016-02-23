using System;
using System.Configuration;
using System.Data;
using NUnit.Framework;
using ServiceStack.Logging;
using ServiceStack.OrmLite.SqlServer.Converters;

namespace ServiceStack.OrmLite.SqlServerTests.Converters
{
    public class SqlServerConvertersOrmLiteTestBase
    {
        protected virtual string ConnectionString { get; set; }

        public IDbConnection Db { get; set; }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            try
            {
                LogManager.LogFactory = new ConsoleLogFactory();

                // Appending the Sql Server Type System Version to use SqlServerSpatial110.dll (2012) assembly
                // Sql Server defaults to SqlServerSpatial100.dll (2008 R2) even for versions greater
                // https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlconnection.connectionstring.aspx
                ConnectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString + ";Type System Version=SQL Server 2012;";

                var dialectProvider = SqlServerConverters.Configure(SqlServer2012Dialect.Provider);

                Db = new OrmLiteConnectionFactory(ConnectionString, dialectProvider).OpenDbConnection();                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Log(string text)
        {
            Console.WriteLine(text);
        }

        public virtual IDbConnection OpenDbConnection(string connString = null)
        {
            connString = connString ?? ConnectionString;
            return connString.OpenDbConnection();
        }
    }
}
