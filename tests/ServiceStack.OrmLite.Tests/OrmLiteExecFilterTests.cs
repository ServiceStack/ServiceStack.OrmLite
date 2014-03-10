using System.Data;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.OrmLite.Tests
{
    public class ReplayOrmLiteExecFilter : OrmLiteExecFilter
    {
        public int ReplayTimes { get; set; }

        public override T Exec<T>(IDbConnection dbConn, System.Func<IDbCommand, T> filter)
        {
            var holdProvider = OrmLiteConfig.DialectProvider;
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
                DisposeCommand(dbCmd);
                OrmLiteConfig.DialectProvider = holdProvider;
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
            var holdExecFilter = OrmLiteConfig.ExecFilter;
            OrmLiteConfig.ExecFilter = new ReplayOrmLiteExecFilter { ReplayTimes = 3 };

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithIdAndName>();
                db.Insert(new ModelWithIdAndName { Name = "Multiplicity" });

                var rowsInserted = db.Count<ModelWithIdAndName>(q => q.Name == "Multiplicity");
                Assert.That(rowsInserted, Is.EqualTo(3));
            }

            OrmLiteConfig.ExecFilter = holdExecFilter;
        }
    }
}