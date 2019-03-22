using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.Models;
using ServiceStack.OrmLite.Tests.Shared;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Async
{
    [TestFixtureOrmLite]
    public class OrmLiteUpdateTestsAsync : OrmLiteProvidersTestBase
    {
        public OrmLiteUpdateTestsAsync(Dialect dialect) : base(dialect)
        {
        }

        [Test]
        public async Task Supports_different_ways_to_UpdateOnly()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Person>();
                await db.InsertAsync(new Person { Id = 1, FirstName = "OriginalFirst", LastName = "OriginalLast", Age = 100 });

                await db.UpdateOnlyAsync(() => new Person { FirstName = "UpdatedFirst", Age = 27 });
                var row = (await db.SelectAsync<Person>()).First();
                Assert.That(row, Is.EqualTo(new Person(1, "UpdatedFirst", "OriginalLast", 27)));

                await db.DeleteAllAsync<Person>();
                await db.InsertAsync(new Person { Id = 1, FirstName = "OriginalFirst", LastName = "OriginalLast", Age = 100 });

                await db.UpdateOnlyAsync(new Person { FirstName = "UpdatedFirst", Age = 27 }, p => p.FirstName);
                row = (await db.SelectAsync<Person>()).First();
                Assert.That(row, Is.EqualTo(new Person(1, "UpdatedFirst", "OriginalLast", 100)));

                await db.DeleteAllAsync<Person>();
                await db.InsertAsync(new Person { Id = 1, FirstName = "OriginalFirst", LastName = "OriginalLast", Age = 100 });

                await db.UpdateOnlyAsync(new Person { FirstName = "UpdatedFirst", Age = 27 }, p => new { p.FirstName, p.Age });
                row = (await db.SelectAsync<Person>()).First();
                Assert.That(row, Is.EqualTo(new Person(1, "UpdatedFirst", "OriginalLast", 27)));

                await db.DeleteAllAsync<Person>();
                await db.InsertAsync(new Person { Id = 1, FirstName = "OriginalFirst", LastName = "OriginalLast", Age = 100 });

                await db.UpdateOnlyAsync(new Person { FirstName = "UpdatedFirst", Age = 27 }, new[] { "FirstName", "Age" });
                row = (await db.SelectAsync<Person>()).First();
                Assert.That(row, Is.EqualTo(new Person(1, "UpdatedFirst", "OriginalLast", 27)));
            }
        }

        [Test]
        [IgnoreProvider(Dialect.AnyMySql, "Default UtcDateTime value not supported")]
        public async Task Can_filter_update_method1_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                await CreateAndInitializeAsync(db, 2);

                await ResetUpdateDateAsync(db);
                await db.UpdateAsync(
                    cmd => cmd.SetUpdateDate<DefaultValues>(nameof(DefaultValues.UpdatedDateUtc), DialectProvider),
                    new DefaultValues {Id = 1, DefaultInt = 45, CreatedDateUtc = DateTime.UtcNow}, new DefaultValues {Id = 2, DefaultInt = 72, CreatedDateUtc = DateTime.UtcNow});
                await VerifyUpdateDateAsync(db);
                await VerifyUpdateDateAsync(db, 2);
            }
        }

        private async Task<DefaultValues> CreateAndInitializeAsync(IDbConnection db, int count = 1)
        {
            db.DropAndCreateTable<DefaultValues>();
            db.GetLastSql().Print();

            DefaultValues firstRow = null;
            for (var i = 1; i <= count; i++)
            {
                var defaultValues = new DefaultValues { Id = i };
                await db.InsertAsync(defaultValues);

                var row = await db.SingleByIdAsync<DefaultValues>(1);
                row.PrintDump();
                Assert.That(row.DefaultInt, Is.EqualTo(1));
                Assert.That(row.DefaultIntNoDefault, Is.EqualTo(0));
                Assert.That(row.NDefaultInt, Is.EqualTo(1));
                Assert.That(row.DefaultDouble, Is.EqualTo(1.1).Within(.1d));
                Assert.That(row.NDefaultDouble, Is.EqualTo(1.1).Within(.1d));
                Assert.That(row.DefaultString, Is.EqualTo("String"));

                if (firstRow == null)
                    firstRow = row;
            }

            return firstRow;
        }

        private async Task ResetUpdateDateAsync(IDbConnection db)
        {
            var updateTime = new DateTime(2011, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            await db.UpdateAsync<DefaultValues>(new { UpdatedDateUtc = updateTime }, p => p.Id == 1);
        }

        private async Task VerifyUpdateDateAsync(IDbConnection db, int id = 1)
        {
            var row = await db.SingleByIdAsync<DefaultValues>(id);
            row.PrintDump();
            Assert.That(row.UpdatedDateUtc, Is.GreaterThan(DateTime.UtcNow - TimeSpan.FromMinutes(5)));
        }

        [Test]
        public async Task Can_filter_update_method2_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                await CreateAndInitializeAsync(db);

                await ResetUpdateDateAsync(db);
                await db.UpdateAsync(new DefaultValues { Id = 1, DefaultInt = 2342, CreatedDateUtc = DateTime.UtcNow}, p => p.Id == 1,
                    cmd => cmd.SetUpdateDate<DefaultValues>(nameof(DefaultValues.UpdatedDateUtc), DialectProvider));
                await VerifyUpdateDateAsync(db);
            }
        }

        [Test]
        [IgnoreProvider(Dialect.AnyMySql, "Default UtcDateTime value not supported")]
        public async Task Can_filter_update_method3_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                await CreateAndInitializeAsync(db);

                await ResetUpdateDateAsync(db);
                var row = await db.SingleByIdAsync<DefaultValues>(1);
                row.DefaultInt = 3245;
                row.DefaultDouble = 978.423;
                await db.UpdateAsync(row, cmd => cmd.SetUpdateDate<DefaultValues>(nameof(DefaultValues.UpdatedDateUtc), DialectProvider));
                await VerifyUpdateDateAsync(db);
            }
        }

        [Test]
        [IgnoreProvider(Dialect.AnyMySql, "Default UtcDateTime value not supported")]
        public async Task Can_filter_update_method4_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                await CreateAndInitializeAsync(db);

                await ResetUpdateDateAsync(db);
                await db.UpdateAsync<DefaultValues>(new { DefaultInt = 765 }, p => p.Id == 1,
                    cmd => cmd.SetUpdateDate<DefaultValues>(nameof(DefaultValues.UpdatedDateUtc), DialectProvider));
                await VerifyUpdateDateAsync(db);
            }
        }

        [Test]
        [IgnoreProvider(Dialect.AnyMySql, "Default UtcDateTime value not supported")]
        public async Task Can_filter_updateAll_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                await CreateAndInitializeAsync(db, 2);

                await ResetUpdateDateAsync(db);
                db.UpdateAll(new[] { new DefaultValues { Id = 1, DefaultInt = 45, CreatedDateUtc = DateTime.UtcNow }, new DefaultValues { Id = 2, DefaultInt = 72, CreatedDateUtc = DateTime.UtcNow } },
                    cmd => cmd.SetUpdateDate<DefaultValues>(nameof(DefaultValues.UpdatedDateUtc), DialectProvider));
                await VerifyUpdateDateAsync(db);
                await VerifyUpdateDateAsync(db, 2);
            }
        }

        [Test]
        [IgnoreProvider(Dialect.AnyMySql, "Default UtcDateTime value not supported")]
        public async Task Can_filter_updateOnly_method1_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                await CreateAndInitializeAsync(db);

                await ResetUpdateDateAsync(db);
                db.UpdateOnly(() => new DefaultValues { DefaultInt = 345 }, p => p.Id == 1,
                    cmd => cmd.SetUpdateDate<DefaultValues>(nameof(DefaultValues.UpdatedDateUtc), DialectProvider));
                await VerifyUpdateDateAsync(db);
            }
        }

        [Test]
        [IgnoreProvider(Dialect.AnyMySql, "Default UtcDateTime value not supported")]
        public async Task Can_filter_updateOnly_method2_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                await CreateAndInitializeAsync(db);

                await ResetUpdateDateAsync(db);
                await db.UpdateOnlyAsync(() => new DefaultValues { DefaultInt = 345 }, db.From<DefaultValues>().Where(p => p.Id == 1),
                    cmd => cmd.SetUpdateDate<DefaultValues>(nameof(DefaultValues.UpdatedDateUtc), DialectProvider));
                await VerifyUpdateDateAsync(db);
            }
        }

        [Test]
        [IgnoreProvider(Dialect.AnyMySql, "Default UtcDateTime value not supported")]
        public async Task Can_filter_updateOnly_method3_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                await CreateAndInitializeAsync(db);

                await ResetUpdateDateAsync(db);
                var row = await db.SingleByIdAsync<DefaultValues>(1);
                row.DefaultDouble = 978.423;
                await db.UpdateOnlyAsync(row, db.From<DefaultValues>().Update(p => p.DefaultDouble),
                    cmd => cmd.SetUpdateDate<DefaultValues>(nameof(DefaultValues.UpdatedDateUtc), DialectProvider));
                await VerifyUpdateDateAsync(db);
            }
        }

        [Test]
        [IgnoreProvider(Dialect.AnyMySql, "Default UtcDateTime value not supported")]
        public async Task Can_filter_updateOnly_method4_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                await CreateAndInitializeAsync(db);

                await ResetUpdateDateAsync(db);
                var row = await db.SingleByIdAsync<DefaultValues>(1);
                row.DefaultDouble = 978.423;
                await db.UpdateOnlyAsync(row, p => p.DefaultDouble, p => p.Id == 1,
                    cmd => cmd.SetUpdateDate<DefaultValues>(nameof(DefaultValues.UpdatedDateUtc), DialectProvider));
                await VerifyUpdateDateAsync(db);
            }
        }

        [Test]
        [IgnoreProvider(Dialect.AnyMySql, "Default UtcDateTime value not supported")]
        public async Task Can_filter_updateOnly_method5_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                await CreateAndInitializeAsync(db);

                await ResetUpdateDateAsync(db);
                var row = await db.SingleByIdAsync<DefaultValues>(1);
                row.DefaultDouble = 978.423;
                await db.UpdateOnlyAsync(row, new[] { nameof(DefaultValues.DefaultDouble) }, p => p.Id == 1,
                    cmd => cmd.SetUpdateDate<DefaultValues>(nameof(DefaultValues.UpdatedDateUtc), DialectProvider));
                await VerifyUpdateDateAsync(db);
            }
        }

        [Test]
        [IgnoreProvider(Dialect.AnyMySql, "Default UtcDateTime value not supported")]
        public async Task Can_filter_updateAdd_expression_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                await CreateAndInitializeAsync(db);

                await ResetUpdateDateAsync(db);

                var count = await db.UpdateAddAsync(() => new DefaultValues { DefaultInt = 5, DefaultDouble = 7.2 }, p => p.Id == 1,
                    cmd => cmd.SetUpdateDate<DefaultValues>(nameof(DefaultValues.UpdatedDateUtc), DialectProvider));

                Assert.That(count, Is.EqualTo(1));
                var row = await db.SingleByIdAsync<DefaultValues>(1);
                Assert.That(row.DefaultInt, Is.EqualTo(6));
                Assert.That(row.DefaultDouble, Is.EqualTo(8.3).Within(0.1));
                await VerifyUpdateDateAsync(db);
            }
        }

        [Test]
        [IgnoreProvider(Dialect.AnyMySql, "Default UtcDateTime value not supported")]
        public async Task Can_filter_updateAdd_SqlExpression_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                await CreateAndInitializeAsync(db);

                await ResetUpdateDateAsync(db);

                var where = db.From<DefaultValues>().Where(p => p.Id == 1);
                var count = await db.UpdateAddAsync(() => new DefaultValues { DefaultInt = 5, DefaultDouble = 7.2 }, where,
                    cmd => cmd.SetUpdateDate<DefaultValues>(nameof(DefaultValues.UpdatedDateUtc), DialectProvider));

                Assert.That(count, Is.EqualTo(1));
                var row = await db.SingleByIdAsync<DefaultValues>(1);
                Assert.That(row.DefaultInt, Is.EqualTo(6));
                Assert.That(row.DefaultDouble, Is.EqualTo(8.3).Within(0.1));
                await VerifyUpdateDateAsync(db);
            }
        }
    }
}