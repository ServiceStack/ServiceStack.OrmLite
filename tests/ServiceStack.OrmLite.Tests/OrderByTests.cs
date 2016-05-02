using System;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Logging;
using ServiceStack.OrmLite.Tests.Expression;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class OrderByTests : OrmLiteTestBase
    {
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