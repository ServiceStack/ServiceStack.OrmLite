using System.Linq;
using NUnit.Framework;
using ServiceStack.OrmLite.Sqlite;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class ExpressionTests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            OrmLiteConfig.DialectProvider = new SqliteOrmLiteDialectProvider();
        }

        public static SqliteExpressionVisitor<Person> expr()
        {
            return new SqliteExpressionVisitor<Person>();
        }

        [Test]
        public void Does_support_Sql_In_on_int_collections()
        {
            var ids = new[] { 1, 2, 3 };

            Assert.That(expr().Where(q => Sql.In(q.Id, 1, 2, 3)).WhereExpression,
                Is.EqualTo("WHERE \"Id\" In (1,2,3)"));

            Assert.That(expr().Where(q => Sql.In(q.Id, ids)).WhereExpression,
                Is.EqualTo("WHERE \"Id\" In (1,2,3)"));

            Assert.That(expr().Where(q => Sql.In(q.Id, ids.ToList())).WhereExpression,
                Is.EqualTo("WHERE \"Id\" In (1,2,3)"));

            Assert.That(expr().Where(q => Sql.In(q.Id, ids.ToList().Cast<object>())).WhereExpression,
                Is.EqualTo("WHERE \"Id\" In (1,2,3)"));
        }

        [Test]
        public void Does_support_Sql_In_on_string_collections()
        {
            var ids = new[] { "A", "B", "C" };

            Assert.That(expr().Where(q => Sql.In(q.FirstName, "A", "B", "C")).WhereExpression,
                Is.EqualTo("WHERE \"FirstName\" In ('A','B','C')"));

            Assert.That(expr().Where(q => Sql.In(q.FirstName, ids)).WhereExpression,
                Is.EqualTo("WHERE \"FirstName\" In ('A','B','C')"));

            Assert.That(expr().Where(q => Sql.In(q.FirstName, ids.ToList())).WhereExpression,
                Is.EqualTo("WHERE \"FirstName\" In ('A','B','C')"));

            Assert.That(expr().Where(q => Sql.In(q.FirstName, ids.ToList().Cast<object>())).WhereExpression,
                Is.EqualTo("WHERE \"FirstName\" In ('A','B','C')"));
        }

        [Test]
        public void Does_generate_valid_update_statement_when_fields_are_prefixed()
        {
            var sut = expr();
            sut.PrefixFieldWithTableName = true;

            Assert.That(sut.Update(p => p.FirstName).ToUpdateStatement(new Person { FirstName = "Guybrush" }).Trim(),
                Is.EqualTo(@"UPDATE ""Person"" SET ""Person"".""FirstName""='Guybrush'"));
        }
    }
}