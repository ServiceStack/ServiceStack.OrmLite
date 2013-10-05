using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.OrmLite.SqlServer;

namespace ServiceStack.OrmLite.SqlServerTests
{
    internal class EnsureUtcTests : OrmLiteTestBase
    {
        [Test]
        public void SaveDateTimeToDatabase()
        {
            var dbFactory = new OrmLiteConnectionFactory(base.ConnectionString, SqlServerOrmLiteDialectProvider.Instance);
            SqlServerOrmLiteDialectProvider.Instance.EnsureUtc(true);

            using (var db = dbFactory.OpenDbConnection())
            {
                var dateTime = new DateTime(2012, 1, 1, 1, 1, 1, DateTimeKind.Local);
                var x = InsertAndSelectDateTime(db, dateTime);
                Assert.AreEqual(DateTimeKind.Utc, x.Test.Kind);
                Assert.AreEqual(x.Test.ToUniversalTime(), dateTime.ToUniversalTime());
                Assert.AreEqual(x.Test.ToLocalTime(), dateTime.ToLocalTime());

                dateTime = new DateTime(2012, 1, 1, 1, 1, 1, DateTimeKind.Utc);
                x = InsertAndSelectDateTime(db, dateTime);
                Assert.AreEqual(DateTimeKind.Utc, x.Test.Kind);
                Assert.AreEqual(x.Test.ToUniversalTime(), dateTime.ToUniversalTime());
                Assert.AreEqual(x.Test.ToLocalTime(), dateTime.ToLocalTime());

                dateTime = new DateTime(2012, 1, 1, 1, 1, 1, DateTimeKind.Unspecified);
                x = InsertAndSelectDateTime(db, dateTime);
                Assert.AreEqual(DateTimeKind.Utc, x.Test.Kind);
                Assert.AreEqual(x.Test.ToUniversalTime(), dateTime);
                Assert.AreEqual(x.Test.ToLocalTime(), dateTime.ToLocalTime());
            }
        }

        private static DateTimeObject InsertAndSelectDateTime(IDbConnection db, DateTime dateTime)
        {
            db.DropAndCreateTable<DateTimeObject>();
            db.InsertAll(new DateTimeObject {Test = dateTime});
            var x = db.Select<DateTimeObject>().First();
            return x;
        }

        private class DateTimeObject
        {
            public DateTime Test { get; set; }
        }
    }
}
