using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.Shared;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class DynamicDbNullIssue : OrmLiteTestBase
    {
        public class Test
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        
        [Test]
        public async Task Does_convert_DbNull_Values_in_Async_Results()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Test>();

                db.Insert(new Test {Id = 1, Name = "A"},
                    new Test {Id = 2, Name = null});

                var rows = db.Select<dynamic>("select Id, Name from Test");
                Assert.That(rows.Any(x => x.Name == null));

                rows = await db.SelectAsync<dynamic>("select Id, Name from Test");
                Assert.That(rows.Any(x => x.Name == null));
            }
        }
    }
}