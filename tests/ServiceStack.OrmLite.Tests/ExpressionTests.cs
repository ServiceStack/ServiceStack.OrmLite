using System.Linq;
using NUnit.Framework;
using ServiceStack.OrmLite.Sqlite;
using ServiceStack.OrmLite.Tests.Shared;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class ExpressionTests
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            OrmLiteConfig.DialectProvider = new SqliteOrmLiteDialectProvider();
        }

        public static SqlExpression<Person> expr()
        {
            return OrmLiteConfig.DialectProvider.SqlExpression<Person>();
        }

        [Test]
        public void Does_support_Sql_In_on_int_collections()
        {
            var ids = new[] { 1, 2, 3 };

            Assert.That(expr().Where(q => Sql.In(q.Id, 1, 2, 3)).WhereExpression,
                Is.EqualTo("WHERE \"Id\" IN (@0,@1,@2)"));

            Assert.That(expr().Where(q => Sql.In(q.Id, ids)).WhereExpression,
                Is.EqualTo("WHERE \"Id\" IN (@0,@1,@2)"));

            Assert.That(expr().Where(q => Sql.In(q.Id, ids.ToList())).WhereExpression,
                Is.EqualTo("WHERE \"Id\" IN (@0,@1,@2)"));

            Assert.That(expr().Where(q => Sql.In(q.Id, ids.ToList().Cast<object>())).WhereExpression,
                Is.EqualTo("WHERE \"Id\" IN (@0,@1,@2)"));
        }

        [Test]
        public void Does_support_Sql_In_on_string_collections()
        {
            var ids = new[] { "A", "B", "C" };

            Assert.That(expr().Where(q => Sql.In(q.FirstName, "A", "B", "C")).WhereExpression,
                Is.EqualTo("WHERE \"FirstName\" IN (@0,@1,@2)"));

            Assert.That(expr().Where(q => Sql.In(q.FirstName, ids)).WhereExpression,
                Is.EqualTo("WHERE \"FirstName\" IN (@0,@1,@2)"));

            Assert.That(expr().Where(q => Sql.In(q.FirstName, ids.ToList())).WhereExpression,
                Is.EqualTo("WHERE \"FirstName\" IN (@0,@1,@2)"));

            Assert.That(expr().Where(q => Sql.In(q.FirstName, ids.ToList().Cast<object>())).WhereExpression,
                Is.EqualTo("WHERE \"FirstName\" IN (@0,@1,@2)"));
        }
    }
}