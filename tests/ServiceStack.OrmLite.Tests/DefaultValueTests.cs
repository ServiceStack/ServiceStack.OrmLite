using System;
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
                db.DropAndCreateTable<DefaultValues>();

                db.GetLastSql().Print();

                db.Insert(new DefaultValues { Id = 1 });

                var row = db.SingleById<DefaultValues>(1);

                row.PrintDump();
                Assert.That(row.DefaultInt, Is.EqualTo(1));
                Assert.That(row.NDefaultInt, Is.EqualTo(1));
                Assert.That(row.DefaultDouble, Is.EqualTo(1.1));
                Assert.That(row.NDefaultDouble, Is.EqualTo(1.1));
                Assert.That(row.DefaultString, Is.EqualTo("String"));
                Assert.That(row.CreatedDateUtc, Is.GreaterThan(DateTime.UtcNow.Date));
                Assert.That(row.NCreatedDateUtc, Is.GreaterThan(DateTime.UtcNow.Date));
            }
        }
    }
}