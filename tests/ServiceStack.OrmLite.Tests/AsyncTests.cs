using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.OrmLite.Async;
using ServiceStack.Text;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading.Tasks;


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
                    db.Insert(new Poco { Name = ((char)('A' + x)).ToString() }));

                var results = (await db.SelectAsync<Poco>()).Map(x => x.Name);
                Assert.That(results, Is.EqualTo(new[] {"A", "B", "C"}));

                results = (await db.SelectAsync<Poco>(x => x.Name == "A")).Map(x => x.Name);
                Assert.That(results, Is.EqualTo(new[] { "A" }));

                results = (await db.SelectAsync<Poco>(q => q.Where(x => x.Name == "A"))).Map(x => x.Name);
                Assert.That(results, Is.EqualTo(new[] { "A" }));
            }
        }
    }
}
