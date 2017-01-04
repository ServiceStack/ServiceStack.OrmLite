using System;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Tests.Issues
{
    [TestFixture]
    public class CustomTypeTests : OrmLiteTestBase
    {
        public class PocoWithGuid
        {
            [AutoIncrement]
            public int Id { get; set; }

            [Index]
            public Guid Guid { get; set; }
        }

        [Test]
        public void Can_select_Guid()
        {
            LogManager.LogFactory = new ConsoleLogFactory();
            
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<PocoWithGuid>();

                var dto = new PocoWithGuid
                {
                    Guid = Guid.NewGuid()
                };

                long id = db.Insert(dto, selectIdentity: true);
                var row = db.Single<PocoWithGuid>(r => r.Id == id);

                Assert.That(row.Guid, Is.EqualTo(dto.Guid));
            }
        }
    }
}