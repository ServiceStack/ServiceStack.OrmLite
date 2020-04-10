using NpgsqlTypes;
using NUnit.Framework;

namespace ServiceStack.OrmLite.PostgreSQL.Tests
{
    public class PgSqlTests
    {
        [Test]
        public void Can_create_NpgsqlParameter()
        {
            Assert.That(PgSql.Param("p", 1).NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Integer));
            Assert.That(PgSql.Param("p", "s").NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Text));
            Assert.That(PgSql.Param("p", 'c').NpgsqlDbType, Is.EqualTo(NpgsqlDbType.Char));
            Assert.That(PgSql.Param("p", new [] { 1 }).NpgsqlDbType, 
                Is.EqualTo(NpgsqlDbType.Integer | NpgsqlDbType.Array));
        }
    }
}