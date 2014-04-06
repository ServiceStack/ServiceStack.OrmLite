using System;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class PocoTable
    {
        public int Id { get; set; }

        [CustomField("CHAR(20)")]
        public string CharColumn { get; set; }

        [CustomField("DECIMAL(18,4)")]
        public decimal? DecimalColumn { get; set; }
    }

    [PreCreateTable("CREATE INDEX udxNoTable on NonExistingTable (Name);")]
    public class ModelWithPreCreateSql
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [PostCreateTable("INSERT INTO ModelWithSeedDataSql (Name) VALUES ('Foo');" +
                     "INSERT INTO ModelWithSeedDataSql (Name) VALUES ('Bar');")]
    public class ModelWithSeedDataSql
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class DynamicAttributeSeedData
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [PreDropTable("CREATE INDEX udxNoTable on NonExistingTable (Name);")]
    public class ModelWithPreDropSql
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [PostDropTable("CREATE INDEX udxNoTable on NonExistingTable (Name);")]
    public class ModelWithPostDropSql
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [TestFixture]
    public class CustomSqlTests
        : OrmLiteTestBase
    {
        [Test]
        public void Can_create_field_with_custom_sql()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<PocoTable>();

                var createTableSql = db.GetLastSql().Replace("NULL","null");
                createTableSql.Print();

                Assert.That(createTableSql, Is.StringContaining("\"CharColumn\" CHAR(20) null")
                                            .Or.StringContaining("CharColumn CHAR(20) null"));
                Assert.That(createTableSql, Is.StringContaining("\"DecimalColumn\" DECIMAL(18,4) null")
                                            .Or.StringContaining("DecimalColumn DECIMAL(18,4) null"));
            }
        }

        [Test]
        public void Does_execute_CustomSql_before_table_created()
        {
            using (var db = OpenDbConnection())
            {
                try
                {
                    db.CreateTable<ModelWithPreCreateSql>();
                    Assert.Fail("Should throw");
                }
                catch (Exception)
                {
                    Assert.That(!db.TableExists("ModelWithPreCreateSql"));
                }
            }
        }

        [Test]
        public void Does_execute_CustomSql_after_table_created()
        {
            SuppressIfOracle("For Oracle need wrap multiple SQL statements in an anonymous block");

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithSeedDataSql>();

                var seedDataNames = db.Select<ModelWithSeedDataSql>().ConvertAll(x => x.Name);

                Assert.That(seedDataNames, Is.EquivalentTo(new[] { "Foo", "Bar" }));
            }
        }

        [Test]
        public void Does_execute_CustomSql_after_table_created_using_dynamic_attribute()
        {
            SuppressIfOracle("For Oracle need wrap multiple SQL statements in an anonymous block");

            typeof(DynamicAttributeSeedData)
                .AddAttributes(new PostCreateTableAttribute(
                    "INSERT INTO DynamicAttributeSeedData (Name) VALUES ('Foo');" +
                    "INSERT INTO DynamicAttributeSeedData (Name) VALUES ('Bar');"));

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<DynamicAttributeSeedData>();

                var seedDataNames = db.Select<DynamicAttributeSeedData>().ConvertAll(x => x.Name);

                Assert.That(seedDataNames, Is.EquivalentTo(new[] { "Foo", "Bar" }));
            }
        }

        [Test]
        public void Does_execute_CustomSql_before_table_dropped()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithPreDropSql>();
                try
                {
                    db.DropTable<ModelWithPreDropSql>();
                    Assert.Fail("Should throw");
                }
                catch (Exception)
                {
                    Assert.That(db.TableExists("ModelWithPreDropSql"));
                }
            }
        }

        [Test]
        public void Does_execute_CustomSql_after_table_dropped()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithPostDropSql>();
                try
                {
                    db.DropTable<ModelWithPostDropSql>();
                    Assert.Fail("Should throw");
                }
                catch (Exception)
                {
                    Assert.That(!db.TableExists("ModelWithPostDropSql"));
                }
            }
        }

    }
}