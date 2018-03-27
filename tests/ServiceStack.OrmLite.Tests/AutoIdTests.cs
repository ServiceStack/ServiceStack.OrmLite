using System;
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

    public class AutoIdTests : OrmLiteTestBase
    {
        //PostgreSQL / psql on db run: CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
//        public AutoIdTests() : base(Dialect.SqlServer) {}
        

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
                
                Assert.That(guidA.Id, Is.Not.EqualTo(new Guid()));
                Assert.That(guidB.Id, Is.Not.EqualTo(new Guid()));
                Assert.That(guidA.Id, Is.Not.EqualTo(guidB));

                var dbA = db.SingleById<GuidAutoId>(guidA.Id);
                Assert.That(dbA.Name, Is.EqualTo(guidA.Name));

                var dbB = db.SingleById<GuidAutoId>(guidB.Id);
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
                
                Assert.That(guidA.Id, Is.Not.EqualTo(new Guid()));
                Assert.That(guidB.Id, Is.Not.EqualTo(new Guid()));
                Assert.That(guidA.Id, Is.Not.EqualTo(guidB));
 
                var dbA = db.SingleById<GuidAutoId>(guidA.Id);
                Assert.That(dbA.Name, Is.EqualTo(guidA.Name));

                var dbB = db.SingleById<GuidAutoId>(guidB.Id);
                Assert.That(dbB.Name, Is.EqualTo(guidB.Name));
            }
        }
    }
    
}