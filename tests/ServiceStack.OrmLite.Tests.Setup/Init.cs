using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests.Setup
{
    public class Init
    {
        [Test, Ignore("Use to run setup against all RDBMS")]
        public void Run_RDBMS_Setup_on_all_databases()
        {
            var dbFactory = TestConfig.InitDbFactory();
            TestConfig.InitDbScripts(dbFactory);
        }

        [Test]
        public void Run_RDBMS_Setup_for_Active_OrmLite_Dialect()
        {
            var dialect = TestConfig.Dialect;
            var dbFactory = TestConfig.InitDbFactory();
            
            TestConfig.InitPostgres(dbFactory, dialect);
            TestConfig.InitMySqlConnector(dbFactory, dialect);
            TestConfig.InitSqlServer(dbFactory, dialect);
        }
    }
}