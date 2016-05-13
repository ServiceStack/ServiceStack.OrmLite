using NUnit.Framework;

namespace ServiceStack.OrmLite.SqlServerTests
{
    [TestFixture]
    public class SchemaTests : OrmLiteTestBase
    {
        public class SchemaTest
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Test]
        public void Drop_add_column()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<SchemaTest>();

                Assert.That(db.ColumnExists<SchemaTest>(x => x.Id));
                Assert.That(db.ColumnExists<SchemaTest>(x => x.Name));

                db.DropColumn<SchemaTest>(nameof(SchemaTest.Name));
                Assert.That(!db.ColumnExists<SchemaTest>(x => x.Name));
                db.DropColumn<SchemaTest>(nameof(SchemaTest.Name)); // Doesn't throw, even though column doesn't exist

                db.AddColumn<SchemaTest>(x => x.Name);
                Assert.That(db.ColumnExists<SchemaTest>(x => x.Name));
                db.AddColumn<SchemaTest>(x => x.Name); // Doesn't throw, even though column already exists
            }
        }
    }
}