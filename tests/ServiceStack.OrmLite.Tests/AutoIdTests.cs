using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Tests
{
    public class GuidAutoId
    {
        [AutoId]
        public Guid Id { get; set; }
        
        public string Name { get; set; }
    }

    [TestFixtureOrmLite]
    [NonParallelizable]
    public class AutoIdTests : OrmLiteProvidersTestBase
    {
        public AutoIdTests(DialectContext context) : base(context) {}
        
        [Test]
        public void Does_populate_and_return_new_guid_on_insert()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<GuidAutoId>();

                var guidA = new GuidAutoId { Name = "A" };
                var guidB = new GuidAutoId { Name = "B" };

                db.Insert(guidA);
                db.Insert(guidB);
                
                Assert.That(guidA.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(guidB.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(guidA.Id, Is.Not.EqualTo(guidB));

                var dbA = db.SingleById<GuidAutoId>(guidA.Id);
                Assert.That(dbA.Name, Is.EqualTo(guidA.Name));

                var dbB = db.SingleById<GuidAutoId>(guidB.Id);
                Assert.That(dbB.Name, Is.EqualTo(guidB.Name));
            }
        }

        [Test]
        public async Task Does_populate_and_return_new_guid_on_insert_Async()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<GuidAutoId>();

                var guidA = new GuidAutoId { Name = "A" };
                var guidB = new GuidAutoId { Name = "B" };

                await db.InsertAsync(guidA);
                await db.InsertAsync(guidB);
                
                Assert.That(guidA.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(guidB.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(guidA.Id, Is.Not.EqualTo(guidB));

                var dbA = await db.SingleByIdAsync<GuidAutoId>(guidA.Id);
                Assert.That(dbA.Name, Is.EqualTo(guidA.Name));

                var dbB = await db.SingleByIdAsync<GuidAutoId>(guidB.Id);
                Assert.That(dbB.Name, Is.EqualTo(guidB.Name));
            }
        }

        [Test]
        public void Does_populate_and_return_new_guid_on_save()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<GuidAutoId>();

                var guidA = new GuidAutoId { Name = "A" };
                var guidB = new GuidAutoId { Name = "B" };

                db.Save(guidA);
                db.Save(guidB);
                
                Assert.That(guidA.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(guidB.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(guidA.Id, Is.Not.EqualTo(guidB));
 
                var dbA = db.SingleById<GuidAutoId>(guidA.Id);
                Assert.That(dbA.Name, Is.EqualTo(guidA.Name));

                var dbB = db.SingleById<GuidAutoId>(guidB.Id);
                Assert.That(dbB.Name, Is.EqualTo(guidB.Name));
            }
        }

        [Test]
        public async Task Does_populate_and_return_new_guid_on_save_Async()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<GuidAutoId>();

                var guidA = new GuidAutoId { Name = "A" };
                var guidB = new GuidAutoId { Name = "B" };

                await db.SaveAsync(guidA);
                await db.SaveAsync(guidB);
                
                Assert.That(guidA.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(guidB.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(guidA.Id, Is.Not.EqualTo(guidB));
 
                var dbA = await db.SingleByIdAsync<GuidAutoId>(guidA.Id);
                Assert.That(dbA.Name, Is.EqualTo(guidA.Name));

                var dbB = await db.SingleByIdAsync<GuidAutoId>(guidB.Id);
                Assert.That(dbB.Name, Is.EqualTo(guidB.Name));
            }
        }

        [Test]
        public void Uses_existing_Guid_Id_if_not_Empty()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<GuidAutoId>();

                var existingGuid = Guid.NewGuid();
                var guidA = new GuidAutoId { Id = existingGuid, Name = "A" };

                db.Insert(guidA);
                
                Assert.That(guidA.Id, Is.EqualTo(existingGuid));

                var fromDb = db.SingleById<GuidAutoId>(existingGuid);
                
                Assert.That(fromDb.Id, Is.EqualTo(existingGuid));
            }
        }

        [Test]
        public void Uses_existing_Guid_Id_if_not_Empty_for_row_inserts()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<GuidAutoId>();

                var existingGuid = Guid.NewGuid();
                var guidA = new GuidAutoId { Id = existingGuid, Name = "A" };

                db.Exec(cmd => {
                    cmd.CommandText = DialectProvider.ToInsertRowStatement(cmd, guidA);
                    DialectProvider.SetParameterValues<GuidAutoId>(cmd, guidA);
                    cmd.ExecuteNonQuery();
                });

                var fromDb = db.SingleById<GuidAutoId>(existingGuid);
                Assert.That(fromDb.Id, Is.EqualTo(existingGuid));
            }
        }
    }
    
}
