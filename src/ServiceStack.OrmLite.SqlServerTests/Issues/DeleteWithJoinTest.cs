using NUnit.Framework;
using ServiceStack.OrmLite.SqlServerTests.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStack.OrmLite.SqlServerTests.Issues
{
    class MainTable
    {
        public int Id { get; set; }
    }
    class JoinedTable
    {
        public int Id { get; set; }
        public int MainTableId { get; set; }
    }

    [TestFixture]
    public class DeleteWithJoinTest : SqlServerConvertersOrmLiteTestBase
    {
        [Test]
        public void Can_delete_entity_with_join_expression()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable(typeof(MainTable));
                db.DropAndCreateTable(typeof(JoinedTable));

                db.Insert(new MainTable() { Id = 1 });
                db.Insert(new JoinedTable() { Id = 1, MainTableId = 1 });
                db.Insert(new JoinedTable() { Id = 2, MainTableId = 1 });

                var ev = db.From<MainTable>();
                ev.Join<JoinedTable>((x, y) => x.Id == y.MainTableId);
                ev.Where<JoinedTable>(x => x.Id == 2);

                var record = db.Single(ev);

                Assert.That(record.Id == 1);

                db.Delete(ev);

                Assert.That(db.Select<MainTable>().Count, Is.EqualTo(0));                
            }
        }
    }
}
