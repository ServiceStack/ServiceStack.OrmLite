using System;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.SqlServer.Converters;
using ServiceStack.OrmLite.Tests.Expression;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class PocoWithTime
    {
        [AutoIncrement]
        public int Id { get; set; }

        public TimeSpan TimeSpan { get; set; }
    }

    [TestFixture]
    public class CustomConverterTests : OrmLiteTestBase
    {
        [Test]
        public void Can_override_SqlServer_Time_Converter()
        {
            if (Dialect != Dialect.SqlServer && Dialect != Dialect.SqlServer2012)
                return;

            using (var db = OpenDbConnection())
            {
                var hold = db.GetDialectProvider().GetConverter<TimeSpan>();
                db.GetDialectProvider().RegisterConverter<TimeSpan>(
                    new SqlServerTimeConverter());

                db.DropAndCreateTable<PocoWithTime>();

                var sql = db.GetLastSql();
                Assert.That(sql, Does.Contain("\"TimeSpan\" TIME NOT NULL"));
                sql.Print();

                //SQL Server can't do < 1 day and only 3ms precision
                var oneTime = new TimeSpan(0, 1, 1, 1, 3); 

                db.Insert(new PocoWithTime { TimeSpan = oneTime });

                Assert.That(db.Single<PocoWithTime>(x => x.TimeSpan == oneTime).TimeSpan, 
                    Is.EqualTo(oneTime).Within(TimeSpan.FromSeconds(1)));

                sql = db.GetLastSql();
                sql.Print();

                Assert.That(sql, Does.Contain("\"TimeSpan\" = '01:01:01.0030000'").
                                 Or.Contain("\"TimeSpan\" = CAST(@0 AS TIME))"));

                db.GetDialectProvider().RegisterConverter<TimeSpan>(hold);
            }
        }
    }
}