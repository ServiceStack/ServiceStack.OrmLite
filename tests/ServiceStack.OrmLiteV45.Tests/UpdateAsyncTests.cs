using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class UpdateAsyncTests
        : OrmLiteTestBase
    {
        public class Poco
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

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
    }
}
