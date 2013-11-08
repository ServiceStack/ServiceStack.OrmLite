using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class SqliteOrmLiteDialectProviderTests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            OrmLiteConfig.DialectProvider = SqliteDialect.Provider;
        }

        public class HasDateTimeOffsetMemeber
        {
            public DateTimeOffset MomentInTime { get; set; }
        }

        public class HasNullableDateTimeOffsetMemeber
        {
            public DateTimeOffset? MomentInTime { get; set; }
        }

        [Test]
        public void CanPersistAndRetrieveDateTimeOffset()
        {
            using (IDbConnection db = ":memory:".OpenDbConnection())
            {
                var dto = DateTimeOffset.Now;

                db.CreateTable<HasDateTimeOffsetMemeber>(false);
                db.Insert(new HasDateTimeOffsetMemeber() {MomentInTime = dto});

                List<HasDateTimeOffsetMemeber> list = db.Select<HasDateTimeOffsetMemeber>();

                Assert.That(list.Count == 1);
                Assert.That(list.First().MomentInTime.CompareTo(dto) == 0);
            }
        }

        [Test]
        public void CanPersistAndRetrieveNullableDateTimeOffset()
        {
            using (IDbConnection db = ":memory:".OpenDbConnection())
            {
                var dto = DateTimeOffset.Now;

                db.CreateTable<HasNullableDateTimeOffsetMemeber>(false);
                db.Insert(new HasNullableDateTimeOffsetMemeber() { MomentInTime = dto });

                List<HasNullableDateTimeOffsetMemeber> list = db.Select<HasNullableDateTimeOffsetMemeber>();

                Assert.That(list.Count == 1);
                Assert.That(list.First().MomentInTime.HasValue);
                Assert.That(list.First().MomentInTime.Value.CompareTo(dto) == 0);
            }
        }
    }
}
