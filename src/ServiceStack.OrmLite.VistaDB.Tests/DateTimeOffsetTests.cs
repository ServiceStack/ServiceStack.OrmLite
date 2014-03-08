using System;
using System.Data;
using System.Linq;
using NUnit.Framework;

namespace ServiceStack.OrmLite.VistaDB.Tests
{
    internal class DateTimeOffsetTests : OrmLiteTestBase
    {
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
        [Ignore("VistaDB does not support DateTimeOffset")]
        public void EnsureDateTimeOffsetSaves()
        {
            var dbFactory = new OrmLiteConnectionFactory(base.ConnectionString, VistaDB4Dialect.Provider);

            using (var db = dbFactory.OpenDbConnection())
            {
                var dateTime = new DateTimeOffset(2012, 1, 30, 1, 1, 1, new TimeSpan(5, 0, 0));
                var x = InsertAndSelectDateTimeOffset<DateTimeOffsetObject, DateTimeOffset>(db, dateTime);
                Assert.AreEqual(x.Test, dateTime);
            }
        }

        [Test]
        [Ignore("VistaDB does not support DateTimeOffset")]
        public void EnsureNullableDateTimeOffsetSaves()
        {
            var dbFactory = new OrmLiteConnectionFactory(base.ConnectionString, VistaDB4Dialect.Provider);

            using (var db = dbFactory.OpenDbConnection())
            {
                DateTimeOffset? dateTime = new DateTimeOffset(2012, 1, 30, 1, 1, 1, new TimeSpan(5, 0, 0));
                var x = InsertAndSelectDateTimeOffset<NullableDateTimeOffsetObject, DateTimeOffset?>(db, dateTime);
                Assert.AreEqual(x.Test, dateTime);
            }
        }

        private class DateTimeOffsetObject : IDateTimeOffsetObject<DateTimeOffset>
        {
            public DateTimeOffset Test { get; set; }
        }

        private class NullableDateTimeOffsetObject : IDateTimeOffsetObject<DateTimeOffset?>
        {
            public DateTimeOffset? Test { get; set; }
        }

        private interface IDateTimeOffsetObject<T>
        {
            T Test { get; set; }
        }
    }
}