using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class LogTest
    {
        [AutoIncrement]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public string Name { get; set; }
    }

    [NUnit.Framework.Ignore("Initializing LogFactory needs to run stand-alone")]
    [TestFixture]
    public class LoggingTests
        : OrmLiteTestBase
    {
        [Test]
        public void Does_log_all_statements()
        {
            var sbLogFactory = new StringBuilderLogFactory();
            LogManager.LogFactory = sbLogFactory;

            using (var db = OpenDbConnection())
            {
                db.DropTable<LogTest>();
                db.CreateTable<LogTest>();

                db.Insert(new LogTest {
                    CustomerId = 2, Name = "Foo"
                });

                var test = db.Single<LogTest>(x => x.CustomerId == 2);

                test.Name = "Bar";

                db.Update(test);

                test = db.Single<LogTest>(x => x.CustomerId == 2);

                db.DeleteById<LogTest>(test.Id);

                var logs = sbLogFactory.GetLogs();
                logs.Print();

                Assert.That(logs, Is.StringContaining("CREATE TABLE"));
                Assert.That(logs, Is.StringContaining("INSERT INTO"));
                Assert.That(logs, Is.StringContaining("SELECT"));
                Assert.That(logs, Is.StringContaining("UPDATE"));
                Assert.That(logs, Is.StringContaining("DELETE FROM"));
            }
        }
    }
}