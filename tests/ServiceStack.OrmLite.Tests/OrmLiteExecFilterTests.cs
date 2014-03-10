using System.Data;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.OrmLite.Tests
{
    public class ReplyOrmLiteExecFilter : OrmLiteExecFilter
    {
        public int ReplyTimes { get; set; }

        public override T Exec<T>(IDbConnection dbConn, System.Func<IDbCommand, T> filter)
        {
            var holdProvider = OrmLiteConfig.TSDialectProvider;
            IDbCommand dbCmd = null;
            try
            {
                dbCmd = CreateCommand(dbConn);

                var ret = default(T);
                for (var i = 0; i < ReplyTimes; i++)
                {
                    ret = filter(dbCmd);
                }
                return ret;
            }
            finally
            {
                DisposeCommand(dbCmd, holdProvider);
            }
        }
    }

    [TestFixture]
    public class OrmLiteExecFilterTests
        : OrmLiteTestBase
    {
        [Test]
        public void Can_add_retry_logic()
        {
            var holdExecFilter = OrmLiteConfig.ExecFilter;
            OrmLiteConfig.ExecFilter = new ReplyOrmLiteExecFilter { ReplyTimes = 3 };

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