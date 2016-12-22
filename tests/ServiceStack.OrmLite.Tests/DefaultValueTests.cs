using System;
using System.Data;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class DefaultValues
    {
        public int Id { get; set; }

        [Default(1)]
        public int DefaultInt { get; set; }

        public int DefaultIntNoDefault { get; set; }

        [Default(1)]
        public int? NDefaultInt { get; set; }

        [Default(1.1)]
        public double DefaultDouble { get; set; }

        [Default(1.1)]
        public double? NDefaultDouble { get; set; }

        [Default("'String'")]
        public string DefaultString { get; set; }

        [Default(OrmLiteVariables.SystemUtc)]
        public DateTime CreatedDateUtc { get; set; }

        [Default(OrmLiteVariables.SystemUtc)]
        public DateTime? NCreatedDateUtc { get; set; }
    }

    [TestFixture]
    public class DefaultValueTests : OrmLiteTestBase
    {
        [Test]
        public void Can_create_table_with_DefaultValues()
        {
            using (var db = OpenDbConnection())
            {
                var row = CreateAndInitialize(db);

                var expectedDate = Dialect != Dialect.MySql && Dialect != Dialect.Firebird
                    ? DateTime.UtcNow.Date
                    : DateTime.Now.Date; //MySql CURRENT_TIMESTAMP == LOCAL_TIME

                Assert.That(row.CreatedDateUtc, Is.GreaterThan(expectedDate));
                Assert.That(row.NCreatedDateUtc, Is.GreaterThan(expectedDate));
            }
        }

        private DefaultValues CreateAndInitialize(IDbConnection db)
        {
            db.DropAndCreateTable<DefaultValues>();

            db.GetLastSql().Print();

            db.Insert(new DefaultValues { Id = 1 });


            var row = db.SingleById<DefaultValues>(1);

            row.PrintDump();
            Assert.That(row.DefaultInt, Is.EqualTo(1));
            Assert.That(row.DefaultIntNoDefault, Is.EqualTo(0));
            Assert.That(row.NDefaultInt, Is.EqualTo(1));
            Assert.That(row.DefaultDouble, Is.EqualTo(1.1).Within(.1d));
            Assert.That(row.NDefaultDouble, Is.EqualTo(1.1).Within(.1d));
            Assert.That(row.DefaultString, Is.EqualTo("String"));

            return row;
        }

        [Test]
        public void Can_use_ToUpdateStatement_to_generate_inline_SQL()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db);

                var row = db.SingleById<DefaultValues>(1);
                row.DefaultIntNoDefault = 42;

                var sql = db.ToUpdateStatement(row);
                sql.Print();
                db.ExecuteSql(sql);

                row = db.SingleById<DefaultValues>(1);

                Assert.That(row.DefaultInt, Is.EqualTo(1));
                Assert.That(row.DefaultIntNoDefault, Is.EqualTo(42));
                Assert.That(row.NDefaultInt, Is.EqualTo(1));
                Assert.That(row.DefaultDouble, Is.EqualTo(1.1).Within(.1d));
                Assert.That(row.NDefaultDouble, Is.EqualTo(1.1).Within(.1d));
                Assert.That(row.DefaultString, Is.EqualTo("String"));
            }
        }
    }
}
