﻿using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class Rockstar
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Age { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime? DateDied { get; set; }
        public LivingStatus LivingStatus { get; set; }
    }

    public class RockstarAlt
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Age { get; set; }
    }

    public enum LivingStatus
    {
        Alive,
        Dead
    }

    public class AutoQueryTestsAsync : OrmLiteTestBase
    {
        public static Rockstar[] SeedRockstars = new[] {
            new Rockstar { Id = 1, FirstName = "Jimi", LastName = "Hendrix", LivingStatus = LivingStatus.Dead, Age = 27, DateOfBirth = new DateTime(1942, 11, 27), DateDied = new DateTime(1970, 09, 18), },
            new Rockstar { Id = 2, FirstName = "Jim", LastName = "Morrison", Age = 27, LivingStatus = LivingStatus.Dead, DateOfBirth = new DateTime(1943, 12, 08), DateDied = new DateTime(1971, 07, 03),  },
            new Rockstar { Id = 3, FirstName = "Kurt", LastName = "Cobain", Age = 27, LivingStatus = LivingStatus.Dead, DateOfBirth = new DateTime(1967, 02, 20), DateDied = new DateTime(1994, 04, 05), },
            new Rockstar { Id = 4, FirstName = "Elvis", LastName = "Presley", Age = 42, LivingStatus = LivingStatus.Dead, DateOfBirth = new DateTime(1935, 01, 08), DateDied = new DateTime(1977, 08, 16), },
            new Rockstar { Id = 5, FirstName = "David", LastName = "Grohl", Age = 44, LivingStatus = LivingStatus.Alive, DateOfBirth = new DateTime(1969, 01, 14), },
            new Rockstar { Id = 6, FirstName = "Eddie", LastName = "Vedder", Age = 48, LivingStatus = LivingStatus.Alive, DateOfBirth = new DateTime(1964, 12, 23), },
            new Rockstar { Id = 7, FirstName = "Michael", LastName = "Jackson", Age = 50, LivingStatus = LivingStatus.Dead, DateOfBirth = new DateTime(1958, 08, 29), DateDied = new DateTime(2009, 06, 05), },
        };

        [Test]
        public async Task Can_query_Rockstars_Async()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Rockstar>();
                await db.InsertAllAsync(SeedRockstars);

                var q = db.From<Rockstar>()
                    .Where("Id < {0} AND Age = {1}", 3, 27);

                var results = await db.SelectAsync(q);
                db.GetLastSql().Print();
                Assert.That(results.Count, Is.EqualTo(2));
                Assert.That(q.Params.Count, Is.EqualTo(2));

                q = db.From<Rockstar>()
                    .Where("Id < {0}", 3)
                    .Or("Age = {0}", 27);
                results = await db.SelectAsync(q);
                Assert.That(results.Count, Is.EqualTo(3));
                Assert.That(q.Params.Count, Is.EqualTo(2));

                q = db.From<Rockstar>().Where("FirstName = {0}", "Kurt");
                results = await db.SelectAsync(q);
                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(q.Params.Count, Is.EqualTo(1));
                Assert.That(results[0].LastName, Is.EqualTo("Cobain"));
            }
        }

        [Test]
        public async Task Can_query_Rockstars_with_ValueFormat()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Rockstar>();
                await db.InsertAllAsync(SeedRockstars);

                var q = db.From<Rockstar>()
                    .Where("FirstName LIKE {0}", "Jim%");

                var results = await db.SelectAsync(q);
                db.GetLastSql().Print();
                Assert.That(results.Count, Is.EqualTo(2));
                Assert.That(q.Params.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Can_query_Rockstars_with_IN_Query()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Rockstar>();
                await db.InsertAllAsync(SeedRockstars);

                var q = db.From<Rockstar>()
                    .Where("FirstName IN ({0})", new SqlInValues(new[] { "Jimi", "Kurt", "Jim" }));

                var results = await db.SelectAsync(q);
                db.GetLastSql().Print();
                Assert.That(results.Count, Is.EqualTo(3));
                Assert.That(q.Params.Count, Is.EqualTo(3));
            }
        }

        [Test]
        public async Task Does_query_Rockstars_Single_with_anon_SelectInto()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Rockstar>();
                await db.InsertAllAsync(SeedRockstars);

                var q = db.From<Rockstar>()
                    .Where(x => x.FirstName == "Kurt")
                    .Select(x => new { x.Id, x.LastName });

                var result = await db.SingleAsync<RockstarAlt>(q);
                Assert.That(result.LastName, Is.EqualTo("Cobain"));
                Assert.That(q.Params.Count, Is.EqualTo(1));

                var results = await db.SelectAsync<RockstarAlt>(q);
                Assert.That(results[0].LastName, Is.EqualTo("Cobain"));
                Assert.That(q.Params.Count, Is.EqualTo(1));
            }
        }
    }

}