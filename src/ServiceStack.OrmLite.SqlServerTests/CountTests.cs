using System.Linq;
using NUnit.Framework;

namespace ServiceStack.OrmLite.SqlServerTests
{
    public class CountTests : OrmLiteTestBase
    {
        [Test]
        public void Can_get_RowCount_if_expression_has_OrderBy()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<LetterFrequency>();

                db.Insert(new LetterFrequency { Letter = "A" });
                db.Insert(new LetterFrequency { Letter = "B" });
                db.Insert(new LetterFrequency { Letter = "B" });

                var query = db.From<LetterFrequency>()
                    .Select(x => x.Letter)
                    .OrderBy(x => x.Id);

                var rowCount = db.RowCount(query);
                Assert.That(rowCount, Is.EqualTo(3));

                rowCount = db.Select(query).Count;
                Assert.That(rowCount, Is.EqualTo(3));
            }
        }
    }
}
