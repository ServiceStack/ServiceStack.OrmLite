using System.Threading.Tasks;
using AppDb;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class SelectAsyncTests
        : OrmLiteTestBase
    {
        [Test]
        public async Task Can_SELECT_SingleAsync()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Poco>();

                await db.InsertAsync(new Poco { Id = "foo" });

                var row = await db.SingleAsync(db.From<Poco>().Where(x => x.Id == "foo"));

                Assert.That(row.Id, Is.EqualTo("foo"));
            }
        }
    }
}