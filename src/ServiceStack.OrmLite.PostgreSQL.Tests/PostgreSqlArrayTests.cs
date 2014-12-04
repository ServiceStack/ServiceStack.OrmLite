using System;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Tests;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.PostgreSQL.Tests
{
    public class ModelWithArrayType
    {
        public int Id { get; set; }

        [CustomField("integer[]")]
        public int[] IntegerArray { get; set; }

        [CustomField("bigint[]")]
        public long[] BigIntegerArray { get; set; }
    }

    [TestFixture]
    public class PostgreSqlArrayTests : OrmLiteTestBase
    {
        public PostgreSqlArrayTests() : base(Dialect.PostgreSql) {}

        [Test]
        public void Can_save_integer_array()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithArrayType>();

                db.GetLastSql().Print();

                var row = new ModelWithArrayType
                {
                    Id = 1,
                    IntegerArray = new []{1,2,3}
                };

                try
                {
                    db.Insert(row);
                }
                catch (Exception e)
                {
                    e.Message.Print();
                }

                var result = db.Select<ModelWithArrayType>();

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].IntegerArray.Count(), Is.EqualTo(3));
            }
        }

        [Test]
        public void Can_save_big_integer_array()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithArrayType>();

                db.GetLastSql().Print();

                var row = new ModelWithArrayType
                {
                    Id = 2,
                    BigIntegerArray = new long[] { 1, 2, 3, 4 }
                };

                try
                {
                    db.Insert(row);
                }
                catch (Exception e)
                {
                    e.Message.Print();
                }

                var result = db.Select<ModelWithArrayType>();

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].BigIntegerArray.Count(), Is.EqualTo(4));
            }
        }
    }
}