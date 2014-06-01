using System;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Tests
{
    public class ModelWithRowVersion
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ulong RowVersion { get; set; }
    }

    public class RowVersionTests : OrmLiteTestBase
    {
        public RowVersionTests()
        {
            //Dialect = Dialect.Sqlite;
        }

        [Test]
        public void Can_create_table_with_RowVersion()
        {
            LogManager.LogFactory = new ConsoleLogFactory();
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithRowVersion>();

                db.Insert(new ModelWithRowVersion { Id = 1, Name = "Name" });

                var row = db.SingleById<ModelWithRowVersion>(1);

                row.Name += " Updated";

                db.Update(row);

                var updatedRow = db.SingleById<ModelWithRowVersion>(1);

                Assert.That(updatedRow.Name, Is.EqualTo("Name Updated"));
                Assert.That(updatedRow.RowVersion, Is.GreaterThan(0));

                row.Name += " Again";

                //Can't update old record
                Assert.Throws<RowModifiedException>(() => 
                    db.Update(row));

                //Can update latest version
                updatedRow.Name += " Again";
                db.Update(updatedRow);
            }
        }
    }
}