using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServerTests
{
    public class UpdateTests : OrmLiteTestBase
    {
        [Test]
        public void Can_execute_update_using_expression()
        {
            using (var con = ConnectionString.OpenDbConnection())
            {
                con.CreateTable<SimpleType>(true);
                var obj = new SimpleType { Name = "Somename" };
                con.Save(obj);
                var storedObj = con.GetById<SimpleType>(con.GetLastInsertId());

                Assert.AreEqual(obj.Name, storedObj.Name);

                obj.Id = storedObj.Id;
                obj.Name = "Someothername";
                con.Update(obj, q => q.Id == storedObj.Id);

                var target = con.GetById<SimpleType>(storedObj.Id);

                Assert.AreEqual(obj.Name, target.Name);
            }
        }


        [Test]
        public void Can_execute_update()
        {
            using (var con = ConnectionString.OpenDbConnection())
            {
                con.CreateTable<SimpleType>(true);
                var obj = new SimpleType { Name = "Somename" };
                con.Save(obj);
                var storedObj = con.GetById<SimpleType>(con.GetLastInsertId());

                Assert.AreEqual(obj.Name, storedObj.Name);

                obj.Id = storedObj.Id;
                obj.Name = "Someothername";
                con.Update(obj);

                var target = con.GetById<SimpleType>(storedObj.Id);

                Assert.AreEqual(obj.Name, target.Name);
            }
        }
    }

    public class SimpleType
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
