using System;
using System.Threading.Tasks;
using AppDb;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;

namespace ServiceStack.OrmLite.Tests
{
    public class PocoWithBytes : IHasGuidId
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public byte[] Image { get; set; }

        public string ContentType { get; set; }
    }

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

        [Test]
        public async Task Can_SELECT_SingleAsyncForStrangeClass()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<PocoWithBytes>();

                var bar = new PocoWithBytes { Id = Guid.NewGuid(), Image = new byte[1024 * 10], ContentType = "image/jpeg" };
                await db.InsertAsync(bar);

                var blah = await db.SingleAsync(db.From<PocoWithBytes>().Where(x => x.Id == bar.Id));
                Assert.That(blah, Is.Not.Null);
            }
        }
    }
}