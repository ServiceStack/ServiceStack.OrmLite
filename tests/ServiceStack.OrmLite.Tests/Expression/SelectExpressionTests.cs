using System;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
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
                dates.Each(x => db.Insert(new Submission
                {
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
                Assert.That(db.Select<Submission>(q => q.StoryDate > storyDateTime - new TimeSpan(1, 0, 0, 0) &&
                                                       q.StoryDate < storyDateTime + new TimeSpan(1, 0, 0, 0)).Count,
                    Is.EqualTo(3));
            }
        }

        public class Shipper
        {
            [AutoIncrement]
            public int Id { get; set; }

            public string CompanyName { get; set; }

            public string Phone { get; set; }

            public int ShipperTypeId { get; set; }
        }

        public class SubsetOfShipper
        {
            public string Phone { get; set; }
            public string CompanyName { get; set; }
        }

        [Test]
        public void Can_select_Partial_SQL_Statements()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Shipper>();

                db.Insert(new Shipper { CompanyName = "Trains R Us", Phone = "555-TRAINS", ShipperTypeId = 1 });
                db.Insert(new Shipper { CompanyName = "Planes R Us", Phone = "555-PLANES", ShipperTypeId = 2 });
                db.Insert(new Shipper { CompanyName = "We do everything!", Phone = "555-UNICORNS", ShipperTypeId = 2 });

                var partialColumns = db.Select<SubsetOfShipper>(
                    db.From<Shipper>().Where(q => q.ShipperTypeId == 2));

                Assert.That(partialColumns.Map(x => x.Phone),
                    Is.EquivalentTo(new[] { "555-UNICORNS", "555-PLANES" }));
                Assert.That(partialColumns.Map(x => x.CompanyName),
                    Is.EquivalentTo(new[] { "Planes R Us", "We do everything!" }));


                var partialDto = db.Select<Shipper>(q =>
                    q.Select(x => new { x.Phone, x.CompanyName })
                     .Where(x => x.ShipperTypeId == 2));

                Assert.That(partialDto.Map(x => x.Phone),
                    Is.EquivalentTo(new[] { "555-UNICORNS", "555-PLANES" }));
                Assert.That(partialDto.Map(x => x.CompanyName),
                    Is.EquivalentTo(new[] { "Planes R Us", "We do everything!" }));


                partialDto = db.Select<Shipper>(q =>
                    q.Select("Phone, " + "CompanyName".SqlColumn())
                     .Where(x => x.ShipperTypeId == 2));

                Assert.That(partialDto.Map(x => x.Phone),
                    Is.EquivalentTo(new[] { "555-UNICORNS", "555-PLANES" }));
                Assert.That(partialDto.Map(x => x.CompanyName),
                    Is.EquivalentTo(new[] { "Planes R Us", "We do everything!" }));
            }
        }

        [Test]
        public void Can_escape_wildcards()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Shipper>();

                db.Insert(new Shipper { CompanyName = "a" });
                db.Insert(new Shipper { CompanyName = "ab" });
                db.Insert(new Shipper { CompanyName = "a_c" });
                db.Insert(new Shipper { CompanyName = "a_cd" });
                db.Insert(new Shipper { CompanyName = "abcd" });
                db.Insert(new Shipper { CompanyName = "a%" });
                db.Insert(new Shipper { CompanyName = "a%b" });
                db.Insert(new Shipper { CompanyName = "a%bc" });
                db.Insert(new Shipper { CompanyName = "a\\" });
                db.Insert(new Shipper { CompanyName = "a\\b" });
                db.Insert(new Shipper { CompanyName = "a\\bc" });

                Assert.That(db.Count<Shipper>(q => q.CompanyName == "a_"), Is.EqualTo(0));
                Assert.That(db.Count<Shipper>(q => q.CompanyName.StartsWith("a_")), Is.EqualTo(2));
                Assert.That(db.Count<Shipper>(q => q.CompanyName.StartsWith("a%")), Is.EqualTo(3));
                Assert.That(db.Count<Shipper>(q => q.CompanyName.StartsWith("a_c")), Is.EqualTo(2));
                Assert.That(db.Count<Shipper>(q => q.CompanyName.StartsWith(@"a\")), Is.EqualTo(3));
                Assert.That(db.Count<Shipper>(q => q.CompanyName.StartsWith(@"a\b")), Is.EqualTo(2));
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