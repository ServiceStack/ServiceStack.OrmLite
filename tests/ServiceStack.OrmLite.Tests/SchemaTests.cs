using System;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class SchemaTests : OrmLiteTestBase
    {
        public class SchemaTest
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Alias("SchemaTest")]
        public class NewSchemaTest
        {
            public int Id { get; set; }
            public string Name { get; set; }

            [Default(0)]
            public int Int { get; set; }

            public int? NInt { get; set; }
        }

        [Schema("Schema")]
        public class TestWithSchema
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }


        [Test]
        public void Does_verify_if_table_exists()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<SchemaTest>();
                Assert.That(!db.TableExists<SchemaTest>());

                db.CreateTable<SchemaTest>();
                Assert.That(db.TableExists<SchemaTest>());

                db.DropTable<TestWithSchema>();
                Assert.That(!db.TableExists<TestWithSchema>());

                db.CreateTable<TestWithSchema>();
                Assert.That(db.TableExists<TestWithSchema>());
            }
        }

        [Test]
        public void Does_verify_if_column_exists()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<SchemaTest>();

                Assert.That(!db.ColumnExists<SchemaTest>(x => x.Id));
                Assert.That(!db.ColumnExists<SchemaTest>(x => x.Name));
                Assert.That(!db.ColumnExists<NewSchemaTest>(x => x.Int));
                Assert.That(!db.ColumnExists<NewSchemaTest>(x => x.NInt));

                db.CreateTable<SchemaTest>();

                Assert.That(db.ColumnExists<SchemaTest>(x => x.Id));
                Assert.That(db.ColumnExists<SchemaTest>(x => x.Name));
                Assert.That(!db.ColumnExists<NewSchemaTest>(x => x.Int));
                Assert.That(!db.ColumnExists<NewSchemaTest>(x => x.NInt));

                if (!db.ColumnExists<NewSchemaTest>(x => x.Int))
                    db.AddColumn<NewSchemaTest>(x => x.Int);
                Assert.That(db.ColumnExists<NewSchemaTest>(x => x.Int));

                if (!db.ColumnExists<NewSchemaTest>(x => x.NInt))
                    db.AddColumn<NewSchemaTest>(x => x.NInt);
                Assert.That(db.ColumnExists<NewSchemaTest>(x => x.NInt));

                db.DropTable<TestWithSchema>();
                Assert.That(!db.ColumnExists<TestWithSchema>(x => x.Id));
                db.CreateTable<TestWithSchema>();
                Assert.That(db.ColumnExists<TestWithSchema>(x => x.Id));
            }
        }

        [Test]
        public void Can_drop_and_add_column()
        {
            if (Dialect == Dialect.Sqlite) return; //DROP COLUMN Not supported

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<SchemaTest>();

                Assert.That(db.ColumnExists<SchemaTest>(x => x.Id));
                Assert.That(db.ColumnExists<SchemaTest>(x => x.Name));

                db.DropColumn<SchemaTest>(x => x.Name);
                Assert.That(!db.ColumnExists<SchemaTest>(x => x.Name));

                try
                {
                    db.DropColumn<SchemaTest>(x => x.Name);
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
                catch (Exception) { }
            }
        }
    }
}