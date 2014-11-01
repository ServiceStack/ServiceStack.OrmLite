using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.OrmLite.Tests.Shared;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class ReplayOrmLiteExecFilter : OrmLiteExecFilter
    {
        public int ReplayTimes { get; set; }

        public override T Exec<T>(IDbConnection dbConn, System.Func<IDbCommand, T> filter)
        {
            var dbCmd = CreateCommand(dbConn);
            try
            {
                var ret = default(T);
                for (var i = 0; i < ReplayTimes; i++)
                {
                    ret = filter(dbCmd);
                }
                return ret;
            }
            finally
            {
                DisposeCommand(dbCmd, dbConn);
            }
        }
    }

    public class MockStoredProcExecFilter : OrmLiteExecFilter
    {
        public override T Exec<T>(IDbConnection dbConn, System.Func<IDbCommand, T> filter)
        {
            try
            {
                return base.Exec(dbConn, filter);
            }
            catch (Exception ex)
            {
                var sql = dbConn.GetLastSql();
                if (sql == "exec sp_name @firstName, @age")
                {
                    return (T)(object)new Person { FirstName = "Mocked" };
                }
                throw;
            }
        }
    }

    [TestFixture]
    public class OrmLiteExecFilterTests
        : OrmLiteTestBase
    {
        [Test]
        public void Can_add_replay_logic()
        {
            SuppressIfOracle("Can't run this with Oracle until use trigger for AutoIncrement primary key insertion");

            var holdExecFilter = OrmLiteConfig.DialectProvider.ExecFilter;
            OrmLiteConfig.DialectProvider.ExecFilter = new ReplayOrmLiteExecFilter { ReplayTimes = 3 };

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithIdAndName>();
                db.Insert(new ModelWithIdAndName { Name = "Multiplicity" });

                var rowsInserted = db.Count<ModelWithIdAndName>(q => q.Name == "Multiplicity");
                Assert.That(rowsInserted, Is.EqualTo(3));
            }

            OrmLiteConfig.DialectProvider.ExecFilter = holdExecFilter;
        }

        [Test]
        public void Can_mock_store_procedure()
        {
            var holdExecFilter = OrmLiteConfig.DialectProvider.ExecFilter;
            OrmLiteConfig.DialectProvider.ExecFilter = new MockStoredProcExecFilter();

            using (var db = OpenDbConnection())
            {
                var person = db.SqlScalar<Person>("exec sp_name @firstName, @age",
                    new {firstName = "aName", age = 1});

                Assert.That(person.FirstName, Is.EqualTo("Mocked"));
            }

            OrmLiteConfig.DialectProvider.ExecFilter = holdExecFilter;
        }

        [Test]
        public void Does_use_StringFilter()
        {
            OrmLiteConfig.StringFilter = s => s.TrimEnd();

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Poco>();

                db.Insert(new Poco { Name = "Value with trailing   " });
                var row = db.Select<Poco>().First();

                Assert.That(row.Name, Is.EqualTo("Value with trailing"));
            }

            OrmLiteConfig.StringFilter = null;
        }
    }
}