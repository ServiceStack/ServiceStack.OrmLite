using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class Sqltest
    {
        [AutoIncrement]
        public int Id { get; set; }
        public double Value { get; set; }
    }

    public class SqlDialectTests : OrmLiteTestBase
    {
        //public SqlDialectTests() : base(Dialect.PostgreSql) {}

        [Test]
        public void Does_concat_values()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Sqltest>();

                db.Insert(new Sqltest { Value = 123.456 });

                var sqlConcat = db.GetDialectProvider().SqlConcat(new object[]{ "'a'", 2, "'c'" });
                var result = db.Scalar<string>($"SELECT {sqlConcat} from sqltest");
                Assert.That(result, Is.EqualTo("a2c"));

                sqlConcat = db.GetDialectProvider().SqlConcat(new object[] { "'$'", "value" });
                result = db.Scalar<string>($"SELECT {sqlConcat} from sqltest");
                Assert.That(result, Is.EqualTo("$123.456"));
            }
        }

        [Test]
        public void Does_format_currency()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Sqltest>();

                db.Insert(new Sqltest { Value = 12 });

                var sqlCurrency = db.GetDialectProvider().SqlCurrency("12.3456");
                var result = db.Scalar<string>($"SELECT {sqlCurrency} from sqltest");
                Assert.That(result, Is.EqualTo("$12.35"));

                sqlCurrency = db.GetDialectProvider().SqlCurrency("12.3456", "£");
                result = db.Scalar<string>($"SELECT {sqlCurrency} from sqltest");
                Assert.That(result, Is.EqualTo("£12.35"));

                db.Insert(new Sqltest { Value = 12.3 });
                db.Insert(new Sqltest { Value = 12.34 });
                db.Insert(new Sqltest { Value = 12.345 });

                var sqlConcat = db.GetDialectProvider().SqlCurrency("value");
                var results = db.SqlList<string>($"SELECT {sqlConcat} from sqltest");

                Assert.That(results, Is.EquivalentTo(new[]
                {
                    "$12.00",
                    "$12.30",
                    "$12.34",
                    "$12.35",
                }));

                sqlConcat = db.GetDialectProvider().SqlCurrency("value", "£");
                results = db.SqlList<string>($"SELECT {sqlConcat} from sqltest");

                Assert.That(results, Is.EquivalentTo(new[]
                {
                    "£12.00",
                    "£12.30",
                    "£12.34",
                    "£12.35",
                }));
            }
        }

    }
}
