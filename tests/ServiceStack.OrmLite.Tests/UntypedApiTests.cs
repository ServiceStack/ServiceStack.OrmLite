using NUnit.Framework;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class BaseClass
    {
        public int Id { get; set; }
    }

    public class Target : BaseClass
    {
        public string Name { get; set; }
    }

    [TestFixture]
    public class UntypedApiTests : OrmLiteTestBase
    {
        [Test]
        public void Can_create_table_and_insert_with_untyped_Api()
        {
            using (var db = OpenDbConnection())
            {
                var row = (BaseClass)new Target { Id = 1, Name = "Foo" };

                var useType = row.GetType();
                var typedApi = db.CreateTypedApi(useType);

                db.DropAndCreateTables(useType);

                db.GetLastSql().Print();

                typedApi.Save(row);

                var typedRow = db.SingleById<Target>(1);

                Assert.That(typedRow.Name, Is.EqualTo("Foo"));

                var updateRow = (BaseClass)new Target { Id = 1, Name = "Bar" };

                typedApi.Update(updateRow);

                typedRow = db.SingleById<Target>(1);

                Assert.That(typedRow.Name, Is.EqualTo("Bar"));

                typedApi.Delete(typedRow, new { Id = 1 });

                typedRow = db.SingleById<Target>(1);

                Assert.That(typedRow, Is.Null);

                typedApi.Insert(row);

                typedRow = db.SingleById<Target>(1);

                Assert.That(typedRow.Name, Is.EqualTo("Foo"));
            }
        }
    }
}