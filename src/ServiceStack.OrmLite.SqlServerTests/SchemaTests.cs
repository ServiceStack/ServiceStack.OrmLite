using System;
using NUnit.Framework;
using ServiceStack.OrmLite.SqlServer.Converters;
using ServiceStack.Text;

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
                catch (Exception) {}
            }
        }

        public class TestDecimalConverter
        {
            public int Id { get; set; }
            public decimal Decimal { get; set; }
        }

        [Test]
        public void Can_replace_decimal_column()
        {
            SqlServerDialect.Provider.RegisterConverter<decimal>(new SqlServerFloatConverter());

            //Requires OrmLiteConnection Wrapper to capture last SQL executed
            var dbFactory = new OrmLiteConnectionFactory(ConnectionString, SqlServerDialect.Provider);

            using (var db = dbFactory.OpenDbConnection())
            {
                db.DropAndCreateTable<TestDecimalConverter>();

                Assert.That(db.GetLastSql(), Is.StringContaining("FLOAT"));
            }
        }
    }

}