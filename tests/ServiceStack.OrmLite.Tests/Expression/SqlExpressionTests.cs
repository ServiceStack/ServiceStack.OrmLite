using System;
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

    public class LetterWeighting
    {
        public long LetterFrequencyId { get; set; }
        public int Weighting { get; set; }
    }

    public class LetterStat
    {
        [AutoIncrement]
        public int Id { get; set; }
        public long LetterFrequencyId { get; set; }
        public string Letter { get; set; }
        public int Weighting { get; set; }
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

        [Test]
        public void Can_select_limit_with_SqlExpression()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<LetterFrequency>();
                db.DropAndCreateTable<LetterWeighting>();

                var letters = "A,B,C,D,E".Split(',');
                var i = 0;
                letters.Each(letter =>
                {
                    var id = db.Insert(new LetterFrequency { Letter = letter }, selectIdentity: true);
                    db.Insert(new LetterWeighting { LetterFrequencyId = id, Weighting = ++i * 10 });
                });

                var results = db.Select(db.From<LetterFrequency>().Limit(3));
                Assert.That(results.Count, Is.EqualTo(3));

                results = db.Select(db.From<LetterFrequency>().Skip(3));
                Assert.That(results.Count, Is.EqualTo(2));

                results = db.Select(db.From<LetterFrequency>().Limit(1, 2));
                Assert.That(results.Count, Is.EqualTo(2));
                Assert.That(results.ConvertAll(x => x.Letter), Is.EquivalentTo(new[] { "B", "C" }));

                results = db.Select(db.From<LetterFrequency>().Skip(1).Take(2));
                Assert.That(results.ConvertAll(x => x.Letter), Is.EquivalentTo(new[] { "B", "C" }));

                results = db.Select(db.From<LetterFrequency>()
                    .OrderByDescending(x => x.Letter)
                    .Skip(1).Take(2));
                Assert.That(results.ConvertAll(x => x.Letter), Is.EquivalentTo(new[] { "D", "C" }));
            }
        }

        [Test]
        public void Can_select_limit_with_JoinSqlBuilder()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<LetterFrequency>();
                db.DropAndCreateTable<LetterWeighting>();

                var letters = "A,B,C,D,E".Split(',');
                var i = 0;
                letters.Each(letter =>
                {
                    var id = db.Insert(new LetterFrequency { Letter = letter }, selectIdentity: true);
                    db.Insert(new LetterWeighting { LetterFrequencyId = id, Weighting = ++i * 10 });
                });

                var joinFn = new Func<JoinSqlBuilder<LetterFrequency, LetterWeighting>>(() =>
                    new JoinSqlBuilder<LetterFrequency, LetterWeighting>()
                        .Join<LetterFrequency, LetterWeighting>(x => x.Id, x => x.LetterFrequencyId)
                    );

                var results = db.Select<LetterFrequency>(joinFn());
                Assert.That(results.Count, Is.EqualTo(5));

                results = db.Select<LetterFrequency>(joinFn().Limit(3));
                Assert.That(results.Count, Is.EqualTo(3));

                results = db.Select<LetterFrequency>(joinFn().Skip(3));
                Assert.That(results.Count, Is.EqualTo(2));

                results = db.Select<LetterFrequency>(joinFn().Limit(1, 2));
                Assert.That(results.Count, Is.EqualTo(2));
                Assert.That(results.ConvertAll(x => x.Letter), Is.EquivalentTo(new[] { "B", "C" }));

                results = db.Select<LetterFrequency>(joinFn().Skip(1).Take(2));
                Assert.That(results.ConvertAll(x => x.Letter), Is.EquivalentTo(new[] { "B", "C" }));

                results = db.Select<LetterFrequency>(joinFn()
                    .OrderByDescending<LetterFrequency>(x => x.Letter)
                    .Skip(1).Take(2));
                Assert.That(results.ConvertAll(x => x.Letter), Is.EquivalentTo(new[] { "D", "C" }));
            }
        }

        [Test]
        public void Can_add_basic_joins_with_SqlExpression()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<LetterFrequency>();
                db.DropAndCreateTable<LetterStat>();

                var letters = "A,B,C,D,E".Split(',');
                var i = 0;
                letters.Each(letter =>
                {
                    var id = db.Insert(new LetterFrequency { Letter = letter }, selectIdentity: true);
                    db.Insert(new LetterStat
                    {
                        LetterFrequencyId = id,
                        Letter = letter,
                        Weighting = ++i * 10
                    });
                });

                db.Insert(new LetterFrequency { Letter = "F" });

                Assert.That(db.Count<LetterFrequency>(), Is.EqualTo(6));

                var results = db.Select(db.From<LetterFrequency, LetterStat>());
                db.GetLastSql().Print();
                Assert.That(results.Count, Is.EqualTo(5));

                results = db.Select(db.From<LetterFrequency, LetterStat>((x, y) => x.Id == y.LetterFrequencyId));
                db.GetLastSql().Print();
                Assert.That(results.Count, Is.EqualTo(5));

                results = db.Select(db.From<LetterFrequency>()
                    .Join<LetterFrequency, LetterStat>((x, y) => x.Id == y.LetterFrequencyId));
                db.GetLastSql().Print();
                Assert.That(results.Count, Is.EqualTo(5));

                results = db.Select<LetterFrequency>(q =>
                    q.Join<LetterFrequency, LetterStat>((x, y) => x.Id == y.LetterFrequencyId));
                db.GetLastSql().Print();
                Assert.That(results.Count, Is.EqualTo(5));
            }
        }

        [Test]
        public void Can_do_ToCountStatement_with_SqlExpression_if_where_expression_refers_to_joined_table()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<LetterFrequency>();
                db.DropAndCreateTable<LetterStat>();

                var letterFrequency = new LetterFrequency { Letter = "A" };
                letterFrequency.Id = (int)db.Insert(letterFrequency, true);

                db.Insert(new LetterStat { Letter = "A", LetterFrequencyId = letterFrequency.Id, Weighting = 1 });

                var expr = db.From<LetterFrequency>()
                    .Join<LetterFrequency, LetterStat>()
                    .Where<LetterStat>(x => x.Id > 0);

                var count = db.SqlScalar<long>(expr.ToCountStatement());

                Assert.That(count, Is.GreaterThan(0));

                count = db.Count<LetterFrequency>(q => q.Join<LetterStat>().Where<LetterStat>(x => x.Id > 0));

                Assert.That(count, Is.GreaterThan(0));

                Assert.That(
                    db.Exists<LetterFrequency>(q => q.Join<LetterStat>().Where<LetterStat>(x => x.Id > 0)));
            }
        }

        [Test]
        public void Can_OrderBy_Fields_with_different_sort_directions()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<LetterFrequency>();
                db.DropAndCreateTable<LetterStat>();

                var i = 0;
                "A,B,B,C,C,C,D,D,E".Split(',').Each(letter => {
                    db.Insert(new LetterFrequency { Letter = letter }, selectIdentity: true);
                });

                var rows = db.Select<LetterFrequency>(q => q.OrderByFields("Letter", "Id"));
                Assert.That(rows.Map(x => x.Letter), Is.EquivalentTo("A,B,B,C,C,C,D,D,E".Split(',')));
                Assert.That(rows.Map(x => x.Id), Is.EquivalentTo("1,2,3,4,5,6,7,8,9".Split(',').Map(int.Parse)));

                rows = db.Select<LetterFrequency>(q => q.OrderByFields("Letter", "-Id"));
                Assert.That(rows.Map(x => x.Letter), Is.EquivalentTo("A,B,B,C,C,C,D,D,E".Split(',')));
                Assert.That(rows.Map(x => x.Id), Is.EquivalentTo("1,3,2,6,5,4,8,7,9".Split(',').Map(int.Parse)));

                rows = db.Select<LetterFrequency>(q => q.OrderByFieldsDescending("Letter", "-Id"));
                Assert.That(rows.Map(x => x.Letter), Is.EquivalentTo("E,D,D,C,C,C,B,B,A".Split(',')));
                Assert.That(rows.Map(x => x.Id), Is.EquivalentTo("9,7,8,4,5,6,2,3,1".Split(',').Map(int.Parse)));
            }

        }
    }
}