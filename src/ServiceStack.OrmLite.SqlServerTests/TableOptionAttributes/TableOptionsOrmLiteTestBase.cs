using System;
using System.Configuration;
using NUnit.Framework;
using ServiceStack.Logging;
using ServiceStack.OrmLite.SqlServer.Converters;

namespace ServiceStack.OrmLite.SqlServerTests.TableOptions
{
    public class SqlServer2012TableOptionsOrmLiteTestBase : OrmLiteTestBase
    {
        [OneTimeSetUp]
        public override void TestFixtureSetUp()
        {
            try
            {
                LogManager.LogFactory = new ConsoleLogFactory();

                // Sql Server In-Memory OLTP does not support MARS
                ConnectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString.Replace("MultipleActiveResultSets=True;", "");

                var dialectProvider = SqlServerConverters.Configure(SqlServer2012Dialect.Provider);

                Db = new OrmLiteConnectionFactory(ConnectionString, dialectProvider).OpenDbConnection();                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class SqlServer2014TableOptionsOrmLiteTestBase : OrmLiteTestBase
    {
        [OneTimeSetUp]
        public override void TestFixtureSetUp()
        {
            try
            {
                LogManager.LogFactory = new ConsoleLogFactory();

                // Sql Server In-Memory OLTP does not support MARS
                ConnectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString.Replace("MultipleActiveResultSets=True;", "");

                var dialectProvider = SqlServerConverters.Configure(SqlServer2014Dialect.Provider);

                Db = new OrmLiteConnectionFactory(ConnectionString, dialectProvider).OpenDbConnection();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class SqlServer2016TableOptionsOrmLiteTestBase : OrmLiteTestBase
    {
        [OneTimeSetUp]
        public override void TestFixtureSetUp()
        {
            try
            {
                LogManager.LogFactory = new ConsoleLogFactory();

                // Sql Server In-Memory OLTP does not support MARS
                ConnectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString.Replace("MultipleActiveResultSets=True;", "");

                var dialectProvider = SqlServerConverters.Configure(SqlServer2016Dialect.Provider);

                Db = new OrmLiteConnectionFactory(ConnectionString, dialectProvider).OpenDbConnection();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
