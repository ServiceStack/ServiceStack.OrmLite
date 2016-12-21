using System;
using System.Data;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Async
{
    public class DefaultValues
    {
        public int Id { get; set; }

        [Default(1, OnUpdate = true)]
        public int DefaultInt { get; set; }

        [Default(1)]
        public int DefaultIntNonUpdate { get; set; }

        [Default(1, OnUpdate = true)]
        public int? NDefaultInt { get; set; }

        [Default(1.1, OnUpdate = true)]
        public double DefaultDouble { get; set; }

        [Default(1.1, OnUpdate = true)]
        public double? NDefaultDouble { get; set; }

        [Default("'String'", OnUpdate = true)]
        public string DefaultString { get; set; }

        [Default(OrmLiteVariables.SystemUtc)]
        public DateTime CreatedDateUtc { get; set; }

        [Default(OrmLiteVariables.SystemUtc, OnUpdate = true)]
        public DateTime UpdatedDateUtc { get; set; }

        [Default(OrmLiteVariables.SystemUtc, OnUpdate = true)]
        public DateTime? NCreatedDateUtc { get; set; }
    }

    public class DefaultValueOnUpdateTestsAsync : OrmLiteTestBase
    {
        [Test]
        public async Task Can_Update_with_DefaultValues_Async()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitializeToNonDefaultValues(db);

                var count = await db.UpdateAsync(new DefaultValues { Id = 1 });
                Assert.That(count, Is.EqualTo(1));

                var row = await db.SingleByIdAsync<DefaultValues>(1);

                row.PrintDump();
                AssertRowEqualExcludingDates(row, new DefaultValues
                {
                    DefaultInt = 1,
                    DefaultIntNonUpdate = 0,
                    NDefaultInt = 1,
                    DefaultDouble = 1.1,
                    NDefaultDouble = 1.1,
                    DefaultString = "String"
                });

                var expectedDate = Dialect != Dialect.MySql && Dialect != Dialect.Firebird
                    ? DateTime.UtcNow.Date
                    : DateTime.Now.Date; //MySql CURRENT_TIMESTAMP == LOCAL_TIME

                Assert.That(row.CreatedDateUtc, Is.EqualTo(DateTime.MinValue));
                Assert.That(row.NCreatedDateUtc, Is.GreaterThan(expectedDate));
                Assert.That(row.UpdatedDateUtc, Is.GreaterThan(expectedDate));
            }
        }

        private DefaultValues CreateAndInitializeToNonDefaultValues(IDbConnection db)
        {
            db.DropAndCreateTable<DefaultValues>();

            db.GetLastSql().Print();

            return InitializeToNonDefaultValues(db, 1);
        }

        private DefaultValues InitializeToNonDefaultValues(IDbConnection db, int id)
        {
            var insertValues = new DefaultValues
            {
                Id = id,
                DefaultDouble = 44.55,
                DefaultInt = 234,
                DefaultIntNonUpdate = 987,
                DefaultString = "foo",
                CreatedDateUtc = new DateTime(2010, 3, 3, 0, 0, 0, DateTimeKind.Utc),
                NCreatedDateUtc = new DateTime(2010, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                NDefaultDouble = 56.78,
                NDefaultInt = 123,
                UpdatedDateUtc = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
            if (Dialect == Dialect.MySql || Dialect == Dialect.Firebird)
            {
                insertValues.CreatedDateUtc = DateTime.SpecifyKind(insertValues.CreatedDateUtc, DateTimeKind.Local);
                insertValues.NCreatedDateUtc = DateTime.SpecifyKind(insertValues.NCreatedDateUtc ?? DateTime.Now, DateTimeKind.Local);
                insertValues.UpdatedDateUtc = DateTime.SpecifyKind(insertValues.UpdatedDateUtc, DateTimeKind.Local);
            }

            db.Insert(insertValues);

            var row = db.SingleById<DefaultValues>(1);

            row.PrintDump();
            AssertRowEqualExcludingDates(row, insertValues);
            Assert.That(row.CreatedDateUtc, Is.EqualTo(insertValues.CreatedDateUtc));
            Assert.That(row.NCreatedDateUtc, Is.EqualTo(insertValues.NCreatedDateUtc));
            Assert.That(row.UpdatedDateUtc, Is.EqualTo(insertValues.UpdatedDateUtc));

            return insertValues;
        }

        private void AssertRowEqualExcludingDates(DefaultValues actual, DefaultValues expected)
        {
            Assert.That(actual.DefaultInt, Is.EqualTo(expected.DefaultInt));
            Assert.That(actual.DefaultIntNonUpdate, Is.EqualTo(expected.DefaultIntNonUpdate));
            Assert.That(actual.NDefaultInt, Is.EqualTo(expected.NDefaultInt));
            Assert.That(actual.DefaultDouble, Is.EqualTo(expected.DefaultDouble).Within(.1d));
            Assert.That(actual.NDefaultDouble, Is.EqualTo(expected.NDefaultDouble).Within(.1d));
            Assert.That(actual.DefaultString, Is.EqualTo(expected.DefaultString));
        }

        [Test]
        public async Task Can_Update_with_someValuesAndSomeDefaultValues_Async()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitializeToNonDefaultValues(db);

                var createdDate = new DateTime(2005, 11, 27, 15, 1, 1, DateTimeKind.Utc);
                var count = await db.UpdateAsync(new DefaultValues { Id = 1, DefaultIntNonUpdate = 47, CreatedDateUtc = createdDate });
                Assert.That(count, Is.EqualTo(1));

                var row = await db.SingleByIdAsync<DefaultValues>(1);

                row.PrintDump();
                AssertRowEqualExcludingDates(row, new DefaultValues
                {
                    DefaultInt = 1,
                    DefaultIntNonUpdate = 47,
                    NDefaultInt = 1,
                    DefaultDouble = 1.1,
                    NDefaultDouble = 1.1,
                    DefaultString = "String"
                });

                var expectedDate = Dialect != Dialect.MySql && Dialect != Dialect.Firebird
                    ? DateTime.UtcNow.Date
                    : DateTime.Now.Date; //MySql CURRENT_TIMESTAMP == LOCAL_TIME

                Assert.That(row.CreatedDateUtc, Is.EqualTo(createdDate));
                Assert.That(row.NCreatedDateUtc, Is.GreaterThan(expectedDate));
                Assert.That(row.UpdatedDateUtc, Is.GreaterThan(expectedDate));
            }
        }

        [Test]
        public async Task Can_Update_OnlyStyle_with_DefaultValues_Async()
        {
            using (var db = OpenDbConnection())
            {
                var insertValues = CreateAndInitializeToNonDefaultValues(db);

                var count = await db.UpdateAsync<DefaultValues>(new { DefaultDouble = 2.2, DefaultString = "Fred" });
                Assert.That(count, Is.EqualTo(1));

                var row = await db.SingleByIdAsync<DefaultValues>(1);

                row.PrintDump();
                AssertRowEqualExcludingDates(row, new DefaultValues
                {
                    DefaultInt = 1,
                    DefaultIntNonUpdate = insertValues.DefaultIntNonUpdate,
                    NDefaultInt = 1,
                    DefaultDouble = 2.2,
                    NDefaultDouble = 1.1,
                    DefaultString = "Fred"
                });

                var expectedDate = Dialect != Dialect.MySql && Dialect != Dialect.Firebird
                    ? DateTime.UtcNow.Date
                    : DateTime.Now.Date; //MySql CURRENT_TIMESTAMP == LOCAL_TIME

                Assert.That(row.CreatedDateUtc, Is.EqualTo(insertValues.CreatedDateUtc));
                Assert.That(row.NCreatedDateUtc, Is.GreaterThan(expectedDate));
                Assert.That(row.UpdatedDateUtc, Is.GreaterThan(expectedDate));
            }
        }

        [Test]
        public async Task Can_Update_several_with_DefaultValues_Async()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitializeToNonDefaultValues(db);
                InitializeToNonDefaultValues(db, 2);

                var count = await db.UpdateAsync(new DefaultValues { Id = 1 }, new DefaultValues { Id = 2, DefaultIntNonUpdate = 43354 });
                Assert.That(count, Is.EqualTo(2));

                VerifyRow(db, 1, 0, DateTime.MinValue);
                VerifyRow(db, 2, 43354, DateTime.MinValue);
            }
        }

        private async void VerifyRow(IDbConnection db, int id, int nonUpdateInt, DateTime createdDate)
        {
            var row = await db.SingleByIdAsync<DefaultValues>(id);

            row.PrintDump();
            AssertRowEqualExcludingDates(row, new DefaultValues
            {
                DefaultInt = 1,
                DefaultIntNonUpdate = nonUpdateInt,
                NDefaultInt = 1,
                DefaultDouble = 1.1,
                NDefaultDouble = 1.1,
                DefaultString = "String"
            });

            var expectedDate = Dialect != Dialect.MySql && Dialect != Dialect.Firebird
                ? DateTime.UtcNow.Date
                : DateTime.Now.Date; //MySql CURRENT_TIMESTAMP == LOCAL_TIME

            Assert.That(row.CreatedDateUtc, Is.EqualTo(createdDate));
            Assert.That(row.NCreatedDateUtc, Is.GreaterThan(expectedDate));
            Assert.That(row.UpdatedDateUtc, Is.GreaterThan(expectedDate));
        }

        [Test]
        public async Task Can_UpdateAll_with_DefaultValues_Async()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitializeToNonDefaultValues(db);
                InitializeToNonDefaultValues(db, 2);

                var count = await db.UpdateAllAsync(new[] { new DefaultValues { Id = 1 }, new DefaultValues { Id = 2, DefaultIntNonUpdate = 43354 } });
                Assert.That(count, Is.EqualTo(2));

                VerifyRow(db, 1, 0, DateTime.MinValue);
                VerifyRow(db, 2, 43354, DateTime.MinValue);
            }
        }

        [Test]
        public async Task Can_UpdateOnly_SqlExpression_with_DefaultValues_Async()
        {
            using (var db = OpenDbConnection())
            {
                var insertValues = CreateAndInitializeToNonDefaultValues(db);

                var sql = db.From<DefaultValues>();
                var createdDate = new DateTime(2013, 3, 7, 5, 23, 47, DateTimeKind.Utc);
                var count = await db.UpdateOnlyAsync(new DefaultValues { Id = 1, DefaultInt = 8733, CreatedDateUtc = createdDate },
                    sql.Update(p => new { p.DefaultInt, p.CreatedDateUtc }));
                Assert.That(count, Is.EqualTo(1));

                var row = await db.SingleByIdAsync<DefaultValues>(1);

                row.PrintDump();
                AssertRowEqualExcludingDates(row, new DefaultValues
                {
                    DefaultInt = 8733,
                    DefaultIntNonUpdate = insertValues.DefaultIntNonUpdate,
                    NDefaultInt = 1,
                    DefaultDouble = 1.1,
                    NDefaultDouble = 1.1,
                    DefaultString = "String"
                });

                var expectedDate = Dialect != Dialect.MySql && Dialect != Dialect.Firebird
                    ? DateTime.UtcNow.Date
                    : DateTime.Now.Date; //MySql CURRENT_TIMESTAMP == LOCAL_TIME

                Assert.That(row.CreatedDateUtc, Is.EqualTo(createdDate));
                Assert.That(row.NCreatedDateUtc, Is.GreaterThan(expectedDate));
                Assert.That(row.UpdatedDateUtc, Is.GreaterThan(expectedDate));
            }
        }

        [Test]
        public async Task Can_UpdateOnly_OnlyFields_with_DefaultValues_Async()
        {
            using (var db = OpenDbConnection())
            {
                var insertValues = CreateAndInitializeToNonDefaultValues(db);

                var createdDate = new DateTime(2013, 3, 7, 5, 23, 47, DateTimeKind.Utc);
                var count = await db.UpdateOnlyAsync(new DefaultValues { Id = 1, DefaultInt = 8733, CreatedDateUtc = createdDate },
                    onlyFields: p => new { p.DefaultInt, p.CreatedDateUtc });
                Assert.That(count, Is.EqualTo(1));

                var row = await db.SingleByIdAsync<DefaultValues>(1);

                row.PrintDump();
                AssertRowEqualExcludingDates(row, new DefaultValues
                {
                    DefaultInt = 8733,
                    DefaultIntNonUpdate = insertValues.DefaultIntNonUpdate,
                    NDefaultInt = 1,
                    DefaultDouble = 1.1,
                    NDefaultDouble = 1.1,
                    DefaultString = "String"
                });

                var expectedDate = Dialect != Dialect.MySql && Dialect != Dialect.Firebird
                    ? DateTime.UtcNow.Date
                    : DateTime.Now.Date; //MySql CURRENT_TIMESTAMP == LOCAL_TIME

                Assert.That(row.CreatedDateUtc, Is.EqualTo(createdDate));
                Assert.That(row.NCreatedDateUtc, Is.GreaterThan(expectedDate));
                Assert.That(row.UpdatedDateUtc, Is.GreaterThan(expectedDate));
            }
        }

        [Test]
        public async Task Can_UpdateOnly_Expression_with_DefaultValues_Async()
        {
            using (var db = OpenDbConnection())
            {
                var insertValues = CreateAndInitializeToNonDefaultValues(db);

                var createdDate = new DateTime(2013, 3, 7, 5, 23, 47, DateTimeKind.Utc);
                var count = await db.UpdateOnlyAsync(() => new DefaultValues { DefaultInt = 8733, CreatedDateUtc = createdDate }, p => p.Id == 1);
                Assert.That(count, Is.EqualTo(1));

                var row = await db.SingleByIdAsync<DefaultValues>(1);

                row.PrintDump();
                AssertRowEqualExcludingDates(row, new DefaultValues
                {
                    DefaultInt = 8733,
                    DefaultIntNonUpdate = insertValues.DefaultIntNonUpdate,
                    NDefaultInt = 1,
                    DefaultDouble = 1.1,
                    NDefaultDouble = 1.1,
                    DefaultString = "String"
                });

                var expectedDate = Dialect != Dialect.MySql && Dialect != Dialect.Firebird
                    ? DateTime.UtcNow.Date
                    : DateTime.Now.Date; //MySql CURRENT_TIMESTAMP == LOCAL_TIME

                Assert.That(row.CreatedDateUtc, Is.EqualTo(createdDate));
                Assert.That(row.NCreatedDateUtc, Is.GreaterThan(expectedDate));
                Assert.That(row.UpdatedDateUtc, Is.GreaterThan(expectedDate));
            }
        }

        [Test]
        public async Task Can_UpdateOnly_StringFieldList_with_DefaultValues_Async()
        {
            using (var db = OpenDbConnection())
            {
                var insertValues = CreateAndInitializeToNonDefaultValues(db);

                var createdDate = new DateTime(2013, 3, 7, 5, 23, 47, DateTimeKind.Utc);
                var count = await db.UpdateOnlyAsync(new DefaultValues { Id = 1, DefaultInt = 8733, CreatedDateUtc = createdDate },
                    new[] { "DefaultInt", "CreatedDateUtc" });
                Assert.That(count, Is.EqualTo(1));

                var row = await db.SingleByIdAsync<DefaultValues>(1);

                row.PrintDump();
                AssertRowEqualExcludingDates(row, new DefaultValues
                {
                    DefaultInt = 8733,
                    DefaultIntNonUpdate = insertValues.DefaultIntNonUpdate,
                    NDefaultInt = 1,
                    DefaultDouble = 1.1,
                    NDefaultDouble = 1.1,
                    DefaultString = "String"
                });

                var expectedDate = Dialect != Dialect.MySql && Dialect != Dialect.Firebird
                    ? DateTime.UtcNow.Date
                    : DateTime.Now.Date; //MySql CURRENT_TIMESTAMP == LOCAL_TIME

                Assert.That(row.CreatedDateUtc, Is.EqualTo(createdDate));
                Assert.That(row.NCreatedDateUtc, Is.GreaterThan(expectedDate));
                Assert.That(row.UpdatedDateUtc, Is.GreaterThan(expectedDate));
            }
        }

        [Test]
        public async Task Can_UpdateAdd_with_DefaultValues_Async()
        {
            using (var db = OpenDbConnection())
            {
                var insertValues = CreateAndInitializeToNonDefaultValues(db);

                var createdDate = new DateTime(2013, 3, 7, 5, 23, 47, DateTimeKind.Utc);
                var count = await db.UpdateAddAsync(() => new DefaultValues { DefaultInt = 8733, CreatedDateUtc = createdDate }, p => p.Id == 1);
                Assert.That(count, Is.EqualTo(1));

                var row = await db.SingleByIdAsync<DefaultValues>(1);

                row.PrintDump();
                AssertRowEqualExcludingDates(row, new DefaultValues
                {
                    DefaultInt = 8733 + insertValues.DefaultInt,
                    DefaultIntNonUpdate = insertValues.DefaultIntNonUpdate,
                    NDefaultInt = 1,
                    DefaultDouble = 1.1,
                    NDefaultDouble = 1.1,
                    DefaultString = "String"
                });

                var expectedDate = Dialect != Dialect.MySql && Dialect != Dialect.Firebird
                    ? DateTime.UtcNow.Date
                    : DateTime.Now.Date; //MySql CURRENT_TIMESTAMP == LOCAL_TIME

                Assert.That(row.CreatedDateUtc, Is.EqualTo(createdDate));
                Assert.That(row.NCreatedDateUtc, Is.GreaterThan(expectedDate));
                Assert.That(row.UpdatedDateUtc, Is.GreaterThan(expectedDate));
            }
        }

        [Test]
        public async Task Can_UpdateAdd_SqlExpression_with_DefaultValues_Async()
        {
            using (var db = OpenDbConnection())
            {
                var insertValues = CreateAndInitializeToNonDefaultValues(db);

                var createdDate = new DateTime(2013, 3, 7, 5, 23, 47, DateTimeKind.Utc);
                var count = await db.UpdateAddAsync(() => new DefaultValues { DefaultInt = 8733, CreatedDateUtc = createdDate }, db.From<DefaultValues>().Where(p => p.Id == 1));
                Assert.That(count, Is.EqualTo(1));

                var row = await db.SingleByIdAsync<DefaultValues>(1);

                row.PrintDump();
                AssertRowEqualExcludingDates(row, new DefaultValues
                {
                    DefaultInt = 8733 + insertValues.DefaultInt,
                    DefaultIntNonUpdate = insertValues.DefaultIntNonUpdate,
                    NDefaultInt = 1,
                    DefaultDouble = 1.1,
                    NDefaultDouble = 1.1,
                    DefaultString = "String"
                });

                var expectedDate = Dialect != Dialect.MySql && Dialect != Dialect.Firebird
                    ? DateTime.UtcNow.Date
                    : DateTime.Now.Date; //MySql CURRENT_TIMESTAMP == LOCAL_TIME

                Assert.That(row.CreatedDateUtc, Is.EqualTo(createdDate));
                Assert.That(row.NCreatedDateUtc, Is.GreaterThan(expectedDate));
                Assert.That(row.UpdatedDateUtc, Is.GreaterThan(expectedDate));
            }
        }

        [Test]
        public async Task Can_Save_whichDoesUpdate_With_DefaultValues_Async()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitializeToNonDefaultValues(db);

                var newRow = await db.SaveAsync(new DefaultValues { Id = 1 });
                Assert.That(newRow, Is.False);

                var row = await db.SingleByIdAsync<DefaultValues>(1);

                row.PrintDump();
                AssertRowEqualExcludingDates(row, new DefaultValues
                {
                    DefaultInt = 1,
                    DefaultIntNonUpdate = 0,
                    NDefaultInt = 1,
                    DefaultDouble = 1.1,
                    NDefaultDouble = 1.1,
                    DefaultString = "String"
                });

                var expectedDate = Dialect != Dialect.MySql && Dialect != Dialect.Firebird
                    ? DateTime.UtcNow.Date
                    : DateTime.Now.Date; //MySql CURRENT_TIMESTAMP == LOCAL_TIME

                Assert.That(row.CreatedDateUtc, Is.EqualTo(DateTime.MinValue));
                Assert.That(row.NCreatedDateUtc, Is.GreaterThan(expectedDate));
                Assert.That(row.UpdatedDateUtc, Is.GreaterThan(expectedDate));
            }
        }

        [Test]
        public async Task Can_SaveAll_whichDoesUpdate_with_DefaultValues_Async()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitializeToNonDefaultValues(db);
                InitializeToNonDefaultValues(db, 2);

                var count = await db.SaveAllAsync(new[] { new DefaultValues { Id = 1 }, new DefaultValues { Id = 2, DefaultIntNonUpdate = 43354 } });
                Assert.That(count, Is.EqualTo(0));

                VerifyRow(db, 1, 0, DateTime.MinValue);
                VerifyRow(db, 2, 43354, DateTime.MinValue);
            }
        }
    }
}
