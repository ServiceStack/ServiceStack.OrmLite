using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

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

        [Test]
        public void Can_query_with_Schema_and_alias_attributes()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Section>();
                db.DropAndCreateTable<Page>();

                db.Save(new Page {
                    ReportSectionId = 1,
                    SectionId = 1,
                }, references: true);
                db.Save(new Page {
                    ReportSectionId = 2,
                    SectionId = 2,
                }, references: true);
                db.Save(new Section {
                    Id = 1,
                    ReportSectionId = 1,
                    Name = "Name1",
                    ReportId = 15,
                }, references: true);

                var query = db.From<Section>()
                    .LeftJoin<Section, Page>((s, p) => s.Id == p.SectionId)
                    .Where<Section>(s => s.ReportId == 15);

                var results = db.Select(query);
                db.GetLastSql().Print();

                results.PrintDump();

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Name, Is.EqualTo("Name1"));
            }
        }
    }

    [Schema("Schema")]
    [Alias("PageAlias")]
    public class Page
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Alias("ReportSectionIdAlias")]
        public int ReportSectionId { get; set; }

        [Alias("SectionIdAlias")]
        public int SectionId { get; set; }
    }

    [Schema("Schema")]
    [Alias("SectionAlias")]
    public class Section
    {
        public int Id { get; set; }

        [Alias("ReportSectionIdAlias")]
        public int ReportSectionId { get; set; }

        [Alias("NameAlias")]
        public string Name { get; set; }
        [Alias("ReportIdAlias")]
        public int ReportId { get; set; }
    }
}