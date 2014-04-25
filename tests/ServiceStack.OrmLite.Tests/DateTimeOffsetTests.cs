using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests
{
    public class DateTimeOffsetTests : OrmLiteTestBase
    {
        private IDbConnection db;

        [TestFixtureSetUp]
        public new void TestFixtureSetUp()
        {
            db = base.OpenDbConnection();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            db.Dispose();
        }

        /// <summary>
        /// Generic way to create our test tables.
        /// </summary>
        /// <typeparam name="TTable"></typeparam>
        /// <typeparam name="TProp"></typeparam>
        /// <param name="db"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static TTable InsertAndSelectDateTimeOffset<TTable, TProp>(IDbConnection db, TProp value) where TTable : IDateTimeOffsetObject<TProp>, new()
        {
            db.DropAndCreateTable<TTable>();
            db.Insert(new TTable
            {
                Test = value
            });
            var result = db.Select<TTable>().First();
            return result;
        }

        [Test]
        public void EnsureDateTimeOffsetSaves()
        {
            var dateTime = new DateTimeOffset(2012, 1, 30, 1, 1, 1, new TimeSpan(5, 0, 0));
            var x = InsertAndSelectDateTimeOffset<DateTimeOffsetObject, DateTimeOffset>(db, dateTime);
            Assert.That(x.Test, Is.EqualTo(dateTime));
        }

        [Test]
        public void EnsureNullableDateTimeOffsetSaves()
        {
            DateTimeOffset? dateTime = new DateTimeOffset(2012, 1, 30, 1, 1, 1, new TimeSpan(5, 0, 0));
            var x = InsertAndSelectDateTimeOffset<NullableDateTimeOffsetObject, DateTimeOffset?>(db, dateTime);
            Assert.That(x.Test, Is.EqualTo(dateTime));
        }

        private class DateTimeOffsetObject : IDateTimeOffsetObject<DateTimeOffset>
        {
            public int Id { get; set; }
            public DateTimeOffset Test { get; set; }
        }

        private class NullableDateTimeOffsetObject : IDateTimeOffsetObject<DateTimeOffset?>
        {
            public int Id { get; set; }
            public DateTimeOffset? Test { get; set; }
        }

        private interface IDateTimeOffsetObject<T>
        {
            int Id { get; set; }
            T Test { get; set; }
        }


        public class HasDateTimeOffsetMember
        {
            public int Id { get; set; }
            public DateTimeOffset MomentInTime { get; set; }
        }

        public class HasNullableDateTimeOffsetMember
        {
            public int Id { get; set; }
            public DateTimeOffset? MomentInTime { get; set; }
        }

        [Test]
        public void CanPersistAndRetrieveDateTimeOffset()
        {
            var now = DateTimeOffset.Now;

            db.DropAndCreateTable<HasDateTimeOffsetMember>();
            db.Insert(new HasDateTimeOffsetMember { MomentInTime = now });

            List<HasDateTimeOffsetMember> list = db.Select<HasDateTimeOffsetMember>();

            Assert.That(list.Count == 1);
            var actual = list.First().MomentInTime;
            Assert.That(actual.Date, Is.EqualTo(now.Date));
        }

        [Test]
        public void CanPersistAndRetrieveNullableDateTimeOffset()
        {
            var now = DateTimeOffset.Now;

            db.DropAndCreateTable<HasNullableDateTimeOffsetMember>();
            db.Insert(new HasNullableDateTimeOffsetMember { MomentInTime = now });

            List<HasNullableDateTimeOffsetMember> list = db.Select<HasNullableDateTimeOffsetMember>();

            Assert.That(list.Count == 1);
            var actual = list.First().MomentInTime;
            Assert.That(actual.HasValue);
            Assert.That(actual.Value.Date, Is.EqualTo(now.Date));
        }
    }
}