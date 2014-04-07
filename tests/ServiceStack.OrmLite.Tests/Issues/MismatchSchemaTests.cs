using NUnit.Framework;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class Nullable
    {
        public class ModelIntValue
        {
            public int Id { get; set; }
            public int? Value { get; set; }
            public string Text { get; set; }
            public long LongMismatch { get; set; }
            public int? LongMismatch2 { get; set; }
        }
    }

    public class NotNullable
    {
        public class ModelIntValue
        {
            public int Id { get; set; }
            public int Value { get; set; }
            public string Text { get; set; }
            public int? LongMismatch { get; set; }
            public long LongMismatch2 { get; set; }
        }
    }

    [TestFixture]
    public class MismatchSchemaTests
        : OrmLiteTestBase
    {
        [Test]
        public void Does_allow_reading_from_table_with_mismatched_nullable_int_type()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Nullable.ModelIntValue>();

                db.Insert(new Nullable.ModelIntValue { Id = 1, Value = null, Text = "Foo" });

                var row = db.SingleById<NotNullable.ModelIntValue>(1);

                Assert.That(row.Value, Is.EqualTo(0));
                Assert.That(row.Text, Is.EqualTo("Foo"));
            }
        }

        [Test]
        public void Does_allow_reading_from_table_with_mismatched_number_types()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Nullable.ModelIntValue>();

                db.Insert(new Nullable.ModelIntValue { Id = 1, LongMismatch = 1, LongMismatch2 = 2 });

                var row = db.SingleById<NotNullable.ModelIntValue>(1);

                Assert.That(row.LongMismatch, Is.EqualTo(1));
                Assert.That(row.LongMismatch2, Is.EqualTo(2));
            }
        }
    }
}