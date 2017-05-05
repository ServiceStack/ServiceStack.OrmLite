using NUnit.Framework;
using ServiceStack.OrmLite.Tests.Shared;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Expression
{
    public class SqlExpressionWhereTests : OrmLiteTestBase
    {
        [Test]
        public void Can_use_Where_on_multiple_tables()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Table1>();
                db.DropAndCreateTable<Table2>();
                db.DropAndCreateTable<Table3>();
                db.DropAndCreateTable<Table4>();
                db.DropAndCreateTable<Table5>();

                db.Insert(new Table1 { Id = 1, String = "A" });
                db.Insert(new Table2 { Id = 1, String = "A" });
                db.Insert(new Table3 { Id = 1, String = "A" });
                db.Insert(new Table4 { Id = 1, String = "A" });
                db.Insert(new Table5 { Id = 1, String = "A" });

                var q = db.From<Table1>()
                    .Join<Table2>((t1, t2) => t1.Id == t2.Id)
                    .Join<Table3>((t1, t3) => t1.Id == t3.Id)
                    .Join<Table4>((t1, t4) => t1.Id == t4.Id)
                    .Join<Table5>((t1, t5) => t1.Id == t5.Id)
                    .Where<Table1, Table2, Table3, Table4, Table5>((t1, t2, t3, t4, t5) =>
                        t1.String == t2.String
                        && t2.String == t3.String
                        && t4.String == t5.String);

                var results = db.Select(q);

                db.GetLastSql().Print();

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].String, Is.EqualTo("A"));
            }
        }
    }
}