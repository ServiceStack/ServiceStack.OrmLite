using System;
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

        [Test]
        public void Can_select_on_TimeSpans()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TestType>();

                db.Insert(new TestType { TimeSpanColumn = TimeSpan.FromHours(1) });

                var rows = db.Select<TestType>(q =>
                    q.TimeSpanColumn > TimeSpan.FromMinutes(30)
                    && q.TimeSpanColumn < TimeSpan.FromHours(2));

                Assert.That(rows.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_select_on_Dates()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Submission>();

                var dates = new[]
                {
                    new DateTime(2014,1,1),
                    new DateTime(2014,1,1,1,0,0),
                    new DateTime(2014,1,1,2,0,0),
                    new DateTime(2014,1,2),
                    new DateTime(2014,2,1),
                    DateTime.UtcNow,
                    new DateTime(2015,1,1),
                };

                var i = 0;
                dates.Each(x => db.Insert(new Submission {
                    Id = i++,
                    StoryDate = x,
                    Headline = "Headline" + i,
                    Body = "Body" + i,
                }));

                Assert.That(db.Select<Submission>(q => q.StoryDate >= new DateTime(2014, 1, 1)).Count,
                    Is.EqualTo(dates.Length));

                Assert.That(db.Select<Submission>(q => q.StoryDate <= new DateTime(2014, 1, 1, 2, 0, 0)).Count,
                    Is.EqualTo(3));

                var storyDateTime = new DateTime(2014, 1, 1);
                Assert.That(db.Select<Submission>(q => q.StoryDate > storyDateTime - new TimeSpan(1,0,0,0) &&
                                                       q.StoryDate < storyDateTime + new TimeSpan(1,0,0,0)).Count,
                    Is.EqualTo(3));
            }
        }
    }

    public class Submission
    {
        public int Id { get; set; }
        public DateTime StoryDate { get; set; }
        public string Headline { get; set; }
        public string Body { get; set; }
    }

}