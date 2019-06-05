using System.Linq;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.Expression;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixtureOrmLite]
    public class OrderByTests : OrmLiteProvidersTestBase
    {
        public OrderByTests(DialectContext context) : base(context) {}

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

        [Test]
        public void Can_OrderBy_and_ThenBy()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<LetterFrequency>();

                db.Insert(new LetterFrequency {Letter = "C" });
                db.Insert(new LetterFrequency {Letter = "C" });
                db.Insert(new LetterFrequency {Letter = "B" });
                db.Insert(new LetterFrequency {Letter = "A" });

                var q = db.From<LetterFrequency>();
                q.OrderBy(nameof(LetterFrequency.Letter))
                    .ThenBy(nameof(LetterFrequency.Id));

                var tracks = db.Select(q);
                
                Assert.That(tracks.First().Letter, Is.EqualTo("A"));
                Assert.That(tracks.Last().Letter, Is.EqualTo("C"));
            }
        }        
    }
}