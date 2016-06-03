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

                var createTableSql = db.GetLastSql().NormalizeSql();

                createTableSql.Print();

                if (Dialect != Dialect.Firebird)
                {
                    Assert.That(createTableSql, Is.StringContaining("charcolumn char(20) null"));
                    Assert.That(createTableSql, Is.StringContaining("decimalcolumn decimal(18,4) null"));
                }
                else
                {
                    Assert.That(createTableSql, Is.StringContaining("charcolumn char(20)"));
                    Assert.That(createTableSql, Is.StringContaining("decimalcolumn decimal(18,4)"));
                }
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
                    Assert.That(!db.TableExists("ModelWithPreCreateSql".SqlColumn()));
                }
            }
        }

        [Test]
        public void Does_execute_CustomSql_after_table_created()
        {
            SuppressIfOracle("For Oracle need wrap multiple SQL statements in an anonymous block");
            if (Dialect == Dialect.PostgreSql || Dialect == Dialect.Oracle || Dialect == Dialect.Firebird) return;

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
            if (Dialect == Dialect.Oracle || Dialect == Dialect.Firebird) return;

            typeof(DynamicAttributeSeedData)
                .AddAttributes(new PostCreateTableAttribute(
                    "INSERT INTO {0} (Name) VALUES ('Foo');".Fmt("DynamicAttributeSeedData".SqlTable()) +
                    "INSERT INTO {0} (Name) VALUES ('Bar');".Fmt("DynamicAttributeSeedData".SqlTable())));

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
                    Assert.That(db.TableExists("ModelWithPreDropSql".SqlTableRaw()));
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

        public class CustomSelectTest
        {
            public int Id { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            [CustomSelect("Width * Height")]
            public int Area { get; set; }
        }

        [Test]
        public void Can_select_custom_field_expressions()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<CustomSelectTest>();

                db.Insert(new CustomSelectTest { Id = 1, Width = 10, Height = 5 });

                var row = db.SingleById<CustomSelectTest>(1);

                Assert.That(row.Area, Is.EqualTo(10 * 5));
            }
        }
    }
}