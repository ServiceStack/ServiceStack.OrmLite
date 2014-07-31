using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Tests.Issues
{
    [TestFixture]
    public class SchemaTests : OrmLiteTestBase
    {
        public SchemaTests()
        {
            Dialect = Dialect.Sqlite; //Other DB Providers needs creating out-of-band 
        }

        [Schema("Schema")]
        public class SchemaTable1
        {
            public int Id { get; set; }

            public int SchemaTable2Id { get; set; }

            [Reference]
            public SchemaTable2 Child { get; set; }
        }

        [Schema("Schema")]
        public class SchemaTable2
        {
            [AutoIncrement]
            public int Id { get; set; }

            public string Name { get; set; }
        }

        [Test]
        public void Can_join_on_table_with_schemas()
        {
            LogManager.LogFactory = new ConsoleLogFactory(debugEnabled: true);

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<SchemaTable1>();
                db.DropAndCreateTable<SchemaTable2>();

                db.Save(new SchemaTable1
                {
                    Id = 1,
                    Child = new SchemaTable2 { Name = "Foo" }
                }, references:true);

                db.Save(new SchemaTable1
                {
                    Id = 2,
                    Child = new SchemaTable2 { Name = "Bar" }
                }, references: true);

                var rows = db.Select<SchemaTable1>(q => q.Join<SchemaTable2>());
                Assert.That(rows.Count, Is.EqualTo(2));
                rows = db.Select<SchemaTable1>(q => q.Join<SchemaTable2>()
                    .Where<SchemaTable2>(x => x.Name == "Foo"));
                Assert.That(rows.Count, Is.EqualTo(1));

                rows = db.Select<SchemaTable1>(q => q.LeftJoin<SchemaTable2>());
                Assert.That(rows.Count, Is.EqualTo(2));
                rows = db.Select<SchemaTable1>(q => q.LeftJoin<SchemaTable2>()
                    .Where<SchemaTable2>(x => x.Name == "Foo"));
                Assert.That(rows.Count, Is.EqualTo(1));
            }
        }
    }
}