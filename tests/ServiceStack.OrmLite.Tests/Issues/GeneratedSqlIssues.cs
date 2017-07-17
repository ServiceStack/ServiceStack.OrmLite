using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class GeneratedSqlIssues : OrmLiteTestBase
    {
        public GeneratedSqlIssues() : base(Dialect.SqlServer2012) {}

        [Test]
        public void Does_generate_valid_sql_when_param_contains_dollar_char()
        {
            using (var db = OpenDbConnection())
            {
                var model = new Poco
                {
                    Id = 1,
                    Name = "Guest$"
                };

                var sql = db.ToUpdateStatement(model);
                Assert.That(sql, Is.EqualTo("UPDATE \"Poco\" SET \"Name\"='Guest$' WHERE \"Id\"=1"));
            }

        }

    }
}