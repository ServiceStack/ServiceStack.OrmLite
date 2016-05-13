using System;
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
        public void Can_drop_and_add_column()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<SchemaTest>();

                Assert.That(db.ColumnExists<SchemaTest>(x => x.Id));
                Assert.That(db.ColumnExists<SchemaTest>(x => x.Name));

                db.DropColumn<SchemaTest>(nameof(SchemaTest.Name));
                Assert.That(!db.ColumnExists<SchemaTest>(x => x.Name));

                try
                {
                    db.DropColumn<SchemaTest>(nameof(SchemaTest.Name));
                    Assert.Fail("Should throw");
                }
                catch (Exception) { }

                db.AddColumn<SchemaTest>(x => x.Name);
                Assert.That(db.ColumnExists<SchemaTest>(x => x.Name));

                try
                {
                    db.AddColumn<SchemaTest>(x => x.Name);
                    Assert.Fail("Should throw");
                }
                catch (Exception) {}
            }
        }
    }
}