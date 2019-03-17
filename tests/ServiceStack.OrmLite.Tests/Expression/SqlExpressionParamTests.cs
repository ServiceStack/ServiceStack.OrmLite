using NUnit.Framework;
using ServiceStack.Logging;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Expression
{
    [TestFixtureOrmLite]
    public class SqlExpressionParamTests : OrmLiteProvidersTestBase
    {
        public SqlExpressionParamTests(Dialect dialect) : base(dialect)
        {
        }

        [Test]
        public void Can_add_DbParam_to_SqlExpression()
        {
            using (var db = OpenDbConnection())
            {
                SqlExpressionTests.InitLetters(db);

                var q = db.From<LetterFrequency>()
                    .UnsafeWhere("Letter = {0}".Fmt(db.GetDialectProvider().GetParam("p1")));

                q.Params.Add(q.CreateParam("p1", "B"));

                var results = db.Select(q);

                results.PrintDump();

                Assert.That(results.Count, Is.EqualTo(2));
            }
        } 
    }
}