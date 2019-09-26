using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Tests.Shared;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixtureOrmLite]
    public class SchemaTests : OrmLiteProvidersTestBase
    {
        public SchemaTests(DialectContext context) : base(context) {}

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            // sqlite doesn't support schemas
            if (DialectFeatures.SchemaSupport)
            {
                using (var db = OpenDbConnection())
                {
                    db.CreateSchema<TestWithSchema>();
                    db.CreateSchema("SchemaTest");
                }
            }
        }

        public class Schematest
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Alias("SchemaTest")]
        public class NewSchematest
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
                db.DropTable<Schematest>();
                Assert.That(!db.TableExists<Schematest>());

                db.CreateTable<Schematest>();
                Assert.That(db.TableExists<Schematest>());

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
                db.DropTable<Schematest>();
                db.DropTable<NewSchematest>();

                Assert.That(!db.ColumnExists<Schematest>(x => x.Id));
                Assert.That(!db.ColumnExists<Schematest>(x => x.Name));
                Assert.That(!db.ColumnExists<NewSchematest>(x => x.Int));
                Assert.That(!db.ColumnExists<NewSchematest>(x => x.NInt));

                db.CreateTable<Schematest>();

                Assert.That(db.ColumnExists<Schematest>(x => x.Id));
                Assert.That(db.ColumnExists<Schematest>(x => x.Name));
                Assert.That(!db.ColumnExists<NewSchematest>(x => x.Int));
                Assert.That(!db.ColumnExists<NewSchematest>(x => x.NInt));

                db.CreateTable<NewSchematest>();
                
                if (!db.ColumnExists<NewSchematest>(x => x.Int))
                    db.AddColumn<NewSchematest>(x => x.Int);
                Assert.That(db.ColumnExists<NewSchematest>(x => x.Int));

                if (!db.ColumnExists<NewSchematest>(x => x.NInt))
                    db.AddColumn<NewSchematest>(x => x.NInt);
                Assert.That(db.ColumnExists<NewSchematest>(x => x.NInt));

                db.DropTable<TestWithSchema>();
                Assert.That(!db.ColumnExists<TestWithSchema>(x => x.Id));
                db.CreateTable<TestWithSchema>();
                Assert.That(db.ColumnExists<TestWithSchema>(x => x.Id));
            }
        }

        [Test]
        [IgnoreDialect(Dialect.Sqlite, "DROP COLUMN Not supported")]
        public void Can_drop_and_add_column()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Schematest>();

                Assert.That(db.ColumnExists<Schematest>(x => x.Id));
                Assert.That(db.ColumnExists<Schematest>(x => x.Name));

                db.DropColumn<Schematest>(x => x.Name);
                Assert.That(!db.ColumnExists<Schematest>(x => x.Name));

                try
                {
                    db.DropColumn<Schematest>(x => x.Name);
                    Assert.Fail("Should throw");
                }
                catch (Exception) { }

                db.AddColumn<Schematest>(x => x.Name);
                Assert.That(db.ColumnExists<Schematest>(x => x.Name));

                try
                {
                    db.AddColumn<Schematest>(x => x.Name);
                    Assert.Fail("Should throw");
                }
                catch (Exception) { }
            }
        }

        [Test]
        public void Can_get_Schema_Table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTableIfNotExists<Person>();

                var columnSchemas = db.GetTableColumns<Person>();
                
                columnSchemas.Each(x => x.ToString().Print());
                
                columnSchemas.Each(x => x.PrintDump());
            }
        }

        [Test]
        public async Task Can_get_Schema_Table_Async()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTableIfNotExists<Person>();

                var columnSchemas = await db.GetTableColumnsAsync<Person>();
                
                columnSchemas.Each(x => x.PrintDump());
            }
        }
    }
}