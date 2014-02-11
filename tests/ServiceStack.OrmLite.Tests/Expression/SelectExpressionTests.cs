using System.Linq;
using NUnit.Framework;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Expression
{
    public class SelectExpressionTests : ExpressionsTestBase
    {
        [Test]
        public void Can_select_where_and_limit_expression()
        {
            EstablishContext(20);

            using (var db = OpenDbConnection())
            {
                var rows = db.Select<TestType>(q => q.Where(x => x.BoolColumn).Limit(5));
                db.GetLastSql().Print();

                Assert.That(rows.Count, Is.EqualTo(5));
                Assert.That(rows.All(x => x.BoolColumn), Is.True);
            }
        }
    }

}