using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Tests
{
    public class ModelWithRowVersion
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ulong RowVersion { get; set; }
    }

    //SqlServer
    //RowVersion NOT NULL

    //Sqlite
    //Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP

    public class RowVersionTests : OrmLiteTestBase
    {
        public RowVersionTests()
        {
            Dialect = Dialect.Sqlite;
        }

        [Test]
        public void Can_create_table_with_RowVersion()
        {
            LogManager.LogFactory = new ConsoleLogFactory();
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithRowVersion>();

                db.Insert(new ModelWithRowVersion { Id = 1, Name = "Name 1" });
                db.Insert(new ModelWithRowVersion { Id = 2, Name = "Name 2" });

                db.Update(new ModelWithRowVersion { Id = 1, Name = "Name 1 Updated" });
                db.Update(new ModelWithRowVersion { Id = 2, Name = "Name 2 Updated" });

                var map = db.Select<ModelWithRowVersion>().ToSafeDictionary(x => x.Id);

                Assert.That(map[1].Name, Is.EqualTo("Name 1 Updated"));
                Assert.That(map[1].RowVersion, Is.GreaterThan(0));
                Assert.That(map[2].Name, Is.EqualTo("Name 2 Updated"));
                Assert.That(map[2].RowVersion, Is.GreaterThan(0));
            }
        }
    }
}