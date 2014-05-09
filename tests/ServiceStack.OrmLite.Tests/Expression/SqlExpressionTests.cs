using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Expression
{
    public class LetterFrequency
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Letter { get; set; }
    }

    public class SqlExpressionTests : ExpressionsTestBase
    {
        private static void InitLetters(IDbConnection db)
        {
            db.DropAndCreateTable<LetterFrequency>();

            db.Insert(new LetterFrequency { Letter = "A" });
            db.Insert(new LetterFrequency { Letter = "B" });
            db.Insert(new LetterFrequency { Letter = "B" });
            db.Insert(new LetterFrequency { Letter = "C" });
            db.Insert(new LetterFrequency { Letter = "C" });
            db.Insert(new LetterFrequency { Letter = "C" });
            db.Insert(new LetterFrequency { Letter = "D" });
            db.Insert(new LetterFrequency { Letter = "D" });
            db.Insert(new LetterFrequency { Letter = "D" });
            db.Insert(new LetterFrequency { Letter = "D" });
        }

        [Test]
        public void Can_select_Dictionary_with_SqlExpression()
        {
            using (var db = OpenDbConnection())
            {
                InitLetters(db);

                var query = db.From<LetterFrequency>()
                  .Select(x => new { x.Letter, count = Sql.Count("*") })
                  .Where(q => q.Letter != "D")
                  .GroupBy(x => x.Letter);

                query.ToSelectStatement().Print();

                var map = new SortedDictionary<string, int>(db.Dictionary<string, int>(query));
                Assert.That(map.EquivalentTo(new Dictionary<string, int> {
                    { "A", 1 }, { "B", 2 }, { "C", 3 },
                }));
            }
        }

        [Test]
        public void Can_select_ColumnDistinct_with_SqlExpression()
        {
            using (var db = OpenDbConnection())
            {
                InitLetters(db);

                var query = db.From<LetterFrequency>()
                  .Where(q => q.Letter != "D")
                  .Select(x => x.Letter);

                query.ToSelectStatement().Print();

                var uniqueLetters = db.ColumnDistinct<string>(query);
                Assert.That(uniqueLetters.EquivalentTo(new[] { "A", "B", "C" }));
            }
        }
    }
}