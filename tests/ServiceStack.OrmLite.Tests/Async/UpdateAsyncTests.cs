using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.OrmLite.Tests.Shared;

namespace ServiceStack.OrmLite.Tests.Async
{
    [TestFixture]
    public class UpdateAsyncTests
        : OrmLiteTestBase
    {
        [Test]
        public async Task Can_updated_with_ExecuteSql_and_db_params_Async()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Poco>();

                db.Insert(new Poco { Id = 1, Name = "A" });
                db.Insert(new Poco { Id = 2, Name = "B" });

                var result = await db.ExecuteSqlAsync("UPDATE poco SET name = @name WHERE id = @id", new { id = 2, name = "UPDATED" });
                Assert.That(result, Is.EqualTo(1));

                var row = await db.SingleByIdAsync<Poco>(2);
                Assert.That(row.Name, Is.EqualTo("UPDATED"));
            }
        }

        [Test]
        public async Task Does_UpdateAdd_using_AssignmentExpression_async()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Person>();
                await db.InsertAllAsync(Person.Rockstars);

                var count = await db.UpdateAddAsync(() => new Person { FirstName = "JJ", Age = 1 }, where: p => p.LastName == "Hendrix");
                Assert.That(count, Is.EqualTo(1));

                var hendrix = Person.Rockstars.First(x => x.LastName == "Hendrix");
                var kurt = Person.Rockstars.First(x => x.LastName == "Cobain");

                var row = await db.SingleAsync<Person>(p => p.LastName == "Hendrix");
                Assert.That(row.FirstName, Is.EqualTo("JJ"));
                Assert.That(row.Age, Is.EqualTo(hendrix.Age + 1));

                count = await db.UpdateAddAsync(() => new Person { FirstName = "KC", Age = hendrix.Age + 1 }, where: p => p.LastName == "Cobain");
                Assert.That(count, Is.EqualTo(1));

                row = await db.SingleAsync<Person>(p => p.LastName == "Cobain");
                Assert.That(row.FirstName, Is.EqualTo("KC"));
                Assert.That(row.Age, Is.EqualTo(kurt.Age + hendrix.Age + 1));
            }
        }
    }
}
