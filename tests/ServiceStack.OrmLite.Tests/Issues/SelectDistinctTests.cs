using NUnit.Framework;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class DistinctColumn
    {
        public int Id { get; set; }
        public int Foo { get; set; }
        public int Bar { get; set; }
    }

    public class DistinctJoinColumn
    {
        public int Id { get; set; }
        public int DistinctColumnId { get; set; }
        public string Name { get; set; }
    }

    [TestFixture]
    public class SelectDistinctTests
        : OrmLiteTestBase
    {
        [Test]
        public void Can_Select_Multiple_Distinct_Columns()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<DistinctColumn>();
                db.DropAndCreateTable<DistinctJoinColumn>();

                db.InsertAll(new[] {
                    new DistinctColumn { Id = 1, Foo = 1, Bar = 42 },
                    new DistinctColumn { Id = 2, Foo = 2, Bar = 55 },
                });

                db.InsertAll(new[] {
                    new DistinctJoinColumn { DistinctColumnId = 1, Name = "Foo", Id = 1 },
                    new DistinctJoinColumn { DistinctColumnId = 1, Name = "Foo", Id = 2 },
                    new DistinctJoinColumn { DistinctColumnId = 2, Name = "Bar", Id = 3 },
                });

                var q = db.From<DistinctColumn>()
                    .Join<DistinctJoinColumn>()
                    .SelectDistinct(dt => new { dt.Bar, dt.Foo });
                
                var result = db.Select(q);
                db.GetLastSql().Print();

                Assert.That(result.Count, Is.EqualTo(2));
            }
        }
    }
}