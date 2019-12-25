using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class FilterTests : OrmLiteTestBase
    {
        [Test]
        public async Task Does_fire_correct_filters()
        {
            var sbInsert = new List<string>(); 
            var sbUpdate = new List<string>();
            OrmLiteConfig.InsertFilter = (cmd, o) => sbInsert.Add(cmd.CommandText);
            OrmLiteConfig.UpdateFilter = (cmd, o) => sbUpdate.Add(cmd.CommandText);

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Poco>();
                await db.SaveAllAsync(new[] {
                    new Poco {Id = 1, Name = "A1"},
                    new Poco {Id = 2, Name = "B1"},
                });
                
                Assert.That(sbInsert[0], Does.StartWith("INSERT"));
                Assert.That(sbInsert[1], Does.StartWith("INSERT"));

                await db.SaveAllAsync(new[] {
                    new Poco {Id = 1, Name = "A2"},
                    new Poco {Id = 2, Name = "B2"},
                });
                
                Assert.That(sbUpdate[0], Does.StartWith("UPDATE"));
                Assert.That(sbUpdate[1], Does.StartWith("UPDATE"));
            }
        }
    }
}