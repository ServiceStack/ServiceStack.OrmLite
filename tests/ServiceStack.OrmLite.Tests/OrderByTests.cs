using System.Linq;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.Expression;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixtureOrmLite]
    public class OrderByTests : OrmLiteProvidersTestBase
    {
        public OrderByTests(Dialect dialect) : base(dialect)
        {
        }

        [Test]
        public void Can_order_by_random()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<LetterFrequency>();

                10.Times(i => db.Insert(new LetterFrequency { Letter = ('A' + i).ToString() }));

                var rowIds1 = db.Select(db.From<LetterFrequency>().OrderBy(x => x.Id)).Map(x => x.Id);
                var rowIds2 = db.Select(db.From<LetterFrequency>().OrderBy(x => x.Id)).Map(x => x.Id);

                Assert.That(rowIds1.SequenceEqual(rowIds2));

                rowIds1 = db.Select(db.From<LetterFrequency>().OrderByRandom()).Map(x => x.Id);
                rowIds2 = db.Select(db.From<LetterFrequency>().OrderByRandom()).Map(x => x.Id);

                Assert.That(!rowIds1.SequenceEqual(rowIds2));
            }
        }
    }
}