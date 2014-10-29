using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.Text;

using ServiceStack.OrmLite.Tests.Shared;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class AsyncTests
        : OrmLiteTestBase
    {
        [Test]
        public async Task Can_Insert_and_SelectAsync()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Poco>();

                for (var i = 0; i < 3; i++)
                {
                    await db.InsertAsync(new Poco { Name = ((char)('A' + i)).ToString() });
                }

                var results = (await db.SelectAsync<Poco>()).Map(x => x.Name);
                Assert.That(results, Is.EqualTo(new[] { "A", "B", "C" }));

                results = (await db.SelectAsync<Poco>(x => x.Name == "A")).Map(x => x.Name);
                Assert.That(results, Is.EqualTo(new[] { "A" }));

                results = (await db.SelectAsync<Poco>(q => q.Where(x => x.Name == "A"))).Map(x => x.Name);
                Assert.That(results, Is.EqualTo(new[] { "A" }));
            }
        }

        [Test]
        public async Task Test_Thread_Affinity()
        {
            var delayMs = 100;
            var db = OpenDbConnection();

            "Root: {0}".Print(Thread.CurrentThread.ManagedThreadId);
            var task = Task.Factory.StartNew(() => 
            {
                "Before Delay: {0}".Print(Thread.CurrentThread.ManagedThreadId);
                return Task.Delay(delayMs);
            })
            .Then(async t =>
            {
                "After Delay: {0}".Print(Thread.CurrentThread.ManagedThreadId);
                await Task.Delay(delayMs);
            })
            .Then(async t =>
            {
                "Before SQL: {0}".Print(Thread.CurrentThread.ManagedThreadId);
                await db.ExistsAsync<Person>(x => x.Age < 50)
                    .Then(t1 => {
                        "After SQL: {0}".Print(Thread.CurrentThread.ManagedThreadId);
                        return Task.Delay(delayMs);
                    });
            })
            .Then(async inner =>
            {
                "Before Inner: {0}".Print(Thread.CurrentThread.ManagedThreadId);
                await Task.Delay(delayMs);
                "After Inner: {0}".Print(Thread.CurrentThread.ManagedThreadId);
            });

            await task;
            "Await t: {0}".Print(Thread.CurrentThread.ManagedThreadId);
        }
    }
}
