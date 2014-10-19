using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.OrmLite.Async;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class AsyncTests
        : OrmLiteTestBase
    {
        [Test]
        public async Task Can_SelectAsync()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Poco>();

                3.Times(x =>
                    db.Insert(new Poco { Name = x.ToString() }));

                var results = await db.SelectAsync<Poco>();
                results.PrintDump();
            }
        }
    }
}
