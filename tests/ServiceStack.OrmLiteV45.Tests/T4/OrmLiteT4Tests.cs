using System.Data;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.Shared;
using ServiceStack.Text;
using StoredProcedures;

namespace ServiceStack.OrmLite.Tests.T4
{
    [TestFixture]
    public class OrmLiteT4Tests
        : OrmLiteTestBase
    {
        private IDbConnection db;

        [SetUp]
        public void SetUp()
        {
            SuppressIfOracle("SQL Server tests");
            db = CreateSqlServerDbFactory().OpenDbConnection();
        }

        [TearDown]
        public void TearDown()
        {
            db.Dispose();
        }

        public class DummyProc
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Test]
        public void Can_call_Dummy_Proc()
        {
            //defined in SqlServerProviderTests.Can_SqlList_StoredProc_passing_null_parameter()
            using (var spResult = db.DummyProc("name"))
            {
                var results = spResult.ConvertToList<DummyProc>();

                Assert.That(results.Count, Is.GreaterThan(0));
                Assert.That(results[0].Id, Is.GreaterThan(0));
                Assert.That(results[0].Name, Is.StringStarting("Name"));
            }
        }

        [Test]
        public void Can_call_Dummy_Proc_in_Transaction()
        {
            using (var dbTrans = db.OpenTransaction())
            using (var spResult = db.DummyProc("name"))
            {
                var results = spResult.ConvertToList<DummyProc>();

                dbTrans.Commit();

                Assert.That(results.Count, Is.GreaterThan(0));
                Assert.That(results[0].Id, Is.GreaterThan(0));
                Assert.That(results[0].Name, Is.StringStarting("Name"));
            }
        }
    }
}