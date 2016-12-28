using System;
using System.Data;
using NUnit.Framework;
using ServiceStack.OrmLite.SqlServer;
using ServiceStack.OrmLite.SqlServer.Converters;
using ServiceStack.OrmLite.Tests.Models;
using ServiceStack.OrmLite.Tests.Shared;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class DefaultValueTests : OrmLiteTestBase
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            if (OrmLiteConfig.DialectProvider == SqlServerOrmLiteDialectProvider.Instance
                || OrmLiteConfig.DialectProvider == SqlServer2012OrmLiteDialectProvider.Instance
                || OrmLiteConfig.DialectProvider == SqlServer2014OrmLiteDialectProvider.Instance
                || OrmLiteConfig.DialectProvider == SqlServer2016OrmLiteDialectProvider.Instance)
                SqlServerDialect.Provider.RegisterConverter<DateTime>(new SqlServerDateTime2Converter());
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (OrmLiteConfig.DialectProvider == SqlServerOrmLiteDialectProvider.Instance
                || OrmLiteConfig.DialectProvider == SqlServer2012OrmLiteDialectProvider.Instance
                || OrmLiteConfig.DialectProvider == SqlServer2014OrmLiteDialectProvider.Instance
                || OrmLiteConfig.DialectProvider == SqlServer2016OrmLiteDialectProvider.Instance)
                SqlServerDialect.Provider.RegisterConverter<DateTime>(new SqlServerDateTimeConverter());
        }

        [Test]
        public void Can_create_table_with_DefaultValues()
        {
            using (var db = OpenDbConnection())
            {
                var row = CreateAndInitialize(db);

                var expectedDate = Dialect != Dialect.MySql && Dialect != Dialect.Firebird
                    ? DateTime.UtcNow.Date
                    : DateTime.Now.Date; //MySql CURRENT_TIMESTAMP == LOCAL_TIME

                Assert.That(row.CreatedDateUtc, Is.GreaterThan(expectedDate));
                Assert.That(row.NCreatedDateUtc, Is.GreaterThan(expectedDate));
            }
        }

        private DefaultValues CreateAndInitialize(IDbConnection db, int count = 1)
        {
            db.DropAndCreateTable<DefaultValues>();
            db.GetLastSql().Print();

            DefaultValues firstRow = null;
            for (var i = 1; i <= count; i++)
            {
                var defaultValues = new DefaultValues { Id = i };
                db.Insert(defaultValues);

                var row = db.SingleById<DefaultValues>(1);
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

        [Test]
        public void Can_use_ToUpdateStatement_to_generate_inline_SQL()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db);

                var row = db.SingleById<DefaultValues>(1);
                row.DefaultIntNoDefault = 42;

                var sql = db.ToUpdateStatement(row);
                sql.Print();
                db.ExecuteSql(sql);

                row = db.SingleById<DefaultValues>(1);

                Assert.That(row.DefaultInt, Is.EqualTo(1));
                Assert.That(row.DefaultIntNoDefault, Is.EqualTo(42));
                Assert.That(row.NDefaultInt, Is.EqualTo(1));
                Assert.That(row.DefaultDouble, Is.EqualTo(1.1).Within(.1d));
                Assert.That(row.NDefaultDouble, Is.EqualTo(1.1).Within(.1d));
                Assert.That(row.DefaultString, Is.EqualTo("String"));
            }
        }

        [Test]
        public void Can_filter_update_method1_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db, 2);

                ResetUpdateDate(db);
                db.Update(cmd => UpdateCommandFilter.SetUpdateDate<DefaultValues>(cmd, nameof(DefaultValues.UpdatedDateUtc)),
                    new DefaultValues { Id = 1, DefaultInt = 45 }, new DefaultValues { Id = 2, DefaultInt = 72 });
                VerifyUpdateDate(db);
                VerifyUpdateDate(db, id: 2);
            }
        }

        private static void ResetUpdateDate(IDbConnection db)
        {
            var updateTime = new DateTime(2011, 1, 1, 1, 1, 1, DateTimeKind.Utc);
            db.Update<DefaultValues>(new { UpdatedDateUtc = updateTime }, p => p.Id == 1);
        }

        private static void VerifyUpdateDate(IDbConnection db, int id = 1)
        {
            var row = db.SingleById<DefaultValues>(id);
            row.PrintDump();
            Assert.That(row.UpdatedDateUtc, Is.GreaterThan(DateTime.UtcNow - TimeSpan.FromMinutes(5)));
        }

        [Test]
        public void Can_filter_update_method2_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db);

                ResetUpdateDate(db);
                db.Update(new DefaultValues { Id = 1, DefaultInt = 2342 }, p => p.Id == 1,
                    cmd => UpdateCommandFilter.SetUpdateDate<DefaultValues>(cmd, nameof(DefaultValues.UpdatedDateUtc)));
                VerifyUpdateDate(db);
            }
        }

        [Test]
        public void Can_filter_update_method3_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db);

                ResetUpdateDate(db);
                var row = db.SingleById<DefaultValues>(1);
                row.DefaultInt = 3245;
                row.DefaultDouble = 978.423;
                db.Update(row, cmd => UpdateCommandFilter.SetUpdateDate<DefaultValues>(cmd, nameof(DefaultValues.UpdatedDateUtc)));
                VerifyUpdateDate(db);
            }
        }

        [Test]
        public void Can_filter_update_method4_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db);

                ResetUpdateDate(db);
                db.Update<DefaultValues>(new { DefaultInt = 765 }, p => p.Id == 1,
                    cmd => UpdateCommandFilter.SetUpdateDate<DefaultValues>(cmd, nameof(DefaultValues.UpdatedDateUtc)));
                VerifyUpdateDate(db);
            }
        }

        [Test]
        public void Can_filter_updateAll_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db, 2);

                ResetUpdateDate(db);
                db.UpdateAll(new [] { new DefaultValues { Id = 1, DefaultInt = 45 }, new DefaultValues { Id = 2, DefaultInt = 72 } },
                    cmd => UpdateCommandFilter.SetUpdateDate<DefaultValues>(cmd, nameof(DefaultValues.UpdatedDateUtc)));
                VerifyUpdateDate(db);
                VerifyUpdateDate(db, id: 2);
            }
        }

        [Test]
        public void Can_filter_updateOnly_method1_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db);

                ResetUpdateDate(db);
                db.UpdateOnly(() => new DefaultValues {DefaultInt = 345}, p => p.Id == 1,
                    cmd => UpdateCommandFilter.SetUpdateDate<DefaultValues>(cmd, nameof(DefaultValues.UpdatedDateUtc)));
                VerifyUpdateDate(db);
            }
        }

        [Test]
        public void Can_filter_updateOnly_method2_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db);

                ResetUpdateDate(db);
                db.UpdateOnly(() => new DefaultValues { DefaultInt = 345 }, db.From<DefaultValues>().Where(p => p.Id == 1),
                    cmd => UpdateCommandFilter.SetUpdateDate<DefaultValues>(cmd, nameof(DefaultValues.UpdatedDateUtc)));
                VerifyUpdateDate(db);
            }
        }

        [Test]
        public void Can_filter_updateOnly_method3_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db);

                ResetUpdateDate(db);
                var row = db.SingleById<DefaultValues>(1);
                row.DefaultDouble = 978.423;
                db.UpdateOnly(row, db.From<DefaultValues>().Update(p => p.DefaultDouble),
                    cmd => UpdateCommandFilter.SetUpdateDate<DefaultValues>(cmd, nameof(DefaultValues.UpdatedDateUtc)));
                VerifyUpdateDate(db);
            }
        }

        [Test]
        public void Can_filter_updateOnly_method4_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db);

                ResetUpdateDate(db);
                var row = db.SingleById<DefaultValues>(1);
                row.DefaultDouble = 978.423;
                db.UpdateOnly(row, p => p.DefaultDouble, p => p.Id == 1,
                    cmd => UpdateCommandFilter.SetUpdateDate<DefaultValues>(cmd, nameof(DefaultValues.UpdatedDateUtc)));
                VerifyUpdateDate(db);
            }
        }

        [Test]
        public void Can_filter_updateOnly_method5_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db);

                ResetUpdateDate(db);
                var row = db.SingleById<DefaultValues>(1);
                row.DefaultDouble = 978.423;
                db.UpdateOnly(row, new[] { nameof(DefaultValues.DefaultDouble) }, p => p.Id == 1,
                    cmd => UpdateCommandFilter.SetUpdateDate<DefaultValues>(cmd, nameof(DefaultValues.UpdatedDateUtc)));
                VerifyUpdateDate(db);
            }
        }

        [Test]
        public void Can_filter_updateAdd_expression_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db);

                ResetUpdateDate(db);

                var count = db.UpdateAdd(() => new DefaultValues { DefaultInt = 5, DefaultDouble = 7.2 }, p => p.Id == 1,
                    cmd => UpdateCommandFilter.SetUpdateDate<DefaultValues>(cmd, nameof(DefaultValues.UpdatedDateUtc)));

                Assert.That(count, Is.EqualTo(1));
                var row = db.SingleById<DefaultValues>(1);
                Assert.That(row.DefaultInt, Is.EqualTo(6));
                Assert.That(row.DefaultDouble, Is.EqualTo(8.3).Within(0.1));
                VerifyUpdateDate(db);
            }
        }

        [Test]
        public void Can_filter_updateAdd_sqlexpression_to_insert_date()
        {
            using (var db = OpenDbConnection())
            {
                CreateAndInitialize(db);

                ResetUpdateDate(db);

                var where = db.From<DefaultValues>().Where(p => p.Id == 1);
                var count = db.UpdateAdd(() => new DefaultValues { DefaultInt = 5, DefaultDouble = 7.2 }, where,
                    cmd => UpdateCommandFilter.SetUpdateDate<DefaultValues>(cmd, nameof(DefaultValues.UpdatedDateUtc)));

                Assert.That(count, Is.EqualTo(1));
                var row = db.SingleById<DefaultValues>(1);
                Assert.That(row.DefaultInt, Is.EqualTo(6));
                Assert.That(row.DefaultDouble, Is.EqualTo(8.3).Within(0.1));
                VerifyUpdateDate(db);
            }
        }
    }
}
