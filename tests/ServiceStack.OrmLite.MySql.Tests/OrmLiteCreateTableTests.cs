using System;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.MySql.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.MySql.Tests
{
    public class MaxStringTest
    {
        public int Id { get; set; }

        [StringLength(int.MaxValue)]
        public string MaxText { get; set; }

        [Text]
        public string Text { get; set; }

        [CustomField("MEDIUMTEXT")]
        public string MediumText { get; set; }
    }

    [TestFixture]
    public class OrmLiteCreateTableTests
        : OrmLiteTestBase
    {
        [Test]
        public void Can_create_table_with_MaxString_and_Custom_MediumText()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<MaxStringTest>();

                //var sql = db.GetLastSql();
                //Assert.That(sql, Is.StringContaining("`MaxText` LONGTEXT NULL"));
                //Assert.That(sql, Is.StringContaining("`MediumText` MEDIUMTEXT NULL"));
            }
        }

        [Test]
        public void Can_create_ModelWithIdOnly_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithIdOnly>(true);
            }
        }

        [Test]
        public void Can_create_ModelWithOnlyStringFields_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithOnlyStringFields>(true);
            }
        }

        [Test]
        public void Can_create_ModelWithLongIdAndStringFields_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithLongIdAndStringFields>(true);
            }
        }

        [Test]
        public void Can_create_ModelWithFieldsOfDifferentTypes_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithFieldsOfDifferentTypes>(true);
            }
        }

        [Test]
        public void Can_preserve_ModelWithIdOnly_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithIdOnly>(true);

                db.Insert(new ModelWithIdOnly(1));
                db.Insert(new ModelWithIdOnly(2));

                db.CreateTable<ModelWithIdOnly>(false);

                var rows = db.Select<ModelWithIdOnly>();

                Assert.That(rows, Has.Count.EqualTo(2));
            }
        }

        [Test]
        public void Can_preserve_ModelWithIdAndName_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithIdAndName>(true);

                db.Insert(new ModelWithIdAndName(1));
                db.Insert(new ModelWithIdAndName(2));

                db.CreateTable<ModelWithIdAndName>(false);

                var rows = db.Select<ModelWithIdAndName>();

                Assert.That(rows, Has.Count.EqualTo(2));
            }
        }

        [Test]
        public void Can_overwrite_ModelWithIdOnly_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithIdOnly>(true);

                db.Insert(new ModelWithIdOnly(1));
                db.Insert(new ModelWithIdOnly(2));

                db.CreateTable<ModelWithIdOnly>(true);

                var rows = db.Select<ModelWithIdOnly>();

                Assert.That(rows, Has.Count.EqualTo(0));
            }
        }

        [Test]
        public void Can_create_multiple_tables()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTables(true, typeof(ModelWithIdOnly), typeof(ModelWithIdAndName));

                db.Insert(new ModelWithIdOnly(1));
                db.Insert(new ModelWithIdOnly(2));

                db.Insert(new ModelWithIdAndName(1));
                db.Insert(new ModelWithIdAndName(2));

                var rows1 = db.Select<ModelWithIdOnly>();
                var rows2 = db.Select<ModelWithIdOnly>();

                Assert.That(rows1, Has.Count.EqualTo(2));
                Assert.That(rows2, Has.Count.EqualTo(2));
            }
        }


        [Test]
        public void Can_create_ModelWithIdAndName_table_with_specified_DefaultStringLength()
        {
            OrmLiteConfig.DialectProvider.GetStringConverter().StringLength = 255;
            var createTableSql = OrmLiteConfig.DialectProvider.ToCreateTableStatement(typeof(ModelWithIdAndName));

            Console.WriteLine("createTableSql: " + createTableSql);
            Assert.That(createTableSql.Contains("VARCHAR(255)"), Is.True);
        }

    }
}