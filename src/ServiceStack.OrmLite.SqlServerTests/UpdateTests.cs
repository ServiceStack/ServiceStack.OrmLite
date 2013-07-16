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
        public void Can_execute_update_only()
        {
            using (var con = ConnectionString.OpenDbConnection())
            {
                con.CreateTable<SimpleType>(true);
                var obj = new SimpleType { Name = "Somename" };
                con.Save(obj);
                var storedObj = con.GetById<SimpleType>(con.GetLastInsertId());

                Assert.AreEqual(obj.Name, storedObj.Name);

                var ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<SimpleType>();
                ev.Update();
                ev.Where(q => q.Id == storedObj.Id); 
                storedObj.Name = "Someothername";

                con.UpdateOnly(storedObj, ev);

                var target = con.GetById<SimpleType>(storedObj.Id);

                Assert.AreEqual("Someothername", target.Name);
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

        [Test]
        public void Can_execute_update_using_aliased_columns()
        {
            using (var con = ConnectionString.OpenDbConnection())
            {
                con.CreateTable<SimpleAliasedType>(true);
                var obj = new SimpleAliasedType { Name = "Somename" };
                con.Save(obj);
                var storedObj = con.GetById<SimpleAliasedType>(con.GetLastInsertId());

                Assert.AreEqual(obj.Name, storedObj.Name);

                obj.Id = storedObj.Id;
                obj.Name = "Someothername";
                con.Update(obj);

                var target = con.GetById<SimpleAliasedType>(storedObj.Id);

                Assert.AreEqual(obj.Name, target.Name);
            }
        }

        [Test]
        public void Can_execute_updateParam()
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
                con.UpdateParam(obj);

                var target = con.GetById<SimpleType>(storedObj.Id);

                Assert.AreEqual(obj.Name, target.Name);
            }
        }

        [Test]
        public void Can_execute_updateParam_using_aliased_columns()
        {
            using (var con = ConnectionString.OpenDbConnection())
            {
                con.CreateTable<SimpleAliasedType>(true);
                var obj = new SimpleAliasedType { Name = "Somename" };
                con.Save(obj);
                var storedObj = con.GetById<SimpleAliasedType>(con.GetLastInsertId());

                Assert.AreEqual(obj.Name, storedObj.Name);

                obj.Id = storedObj.Id;
                obj.Name = "Someothername";
                con.UpdateParam(obj);

                var target = con.GetById<SimpleAliasedType>(storedObj.Id);

                Assert.AreEqual(obj.Name, target.Name);
            }
        }
        [Test]
        public void Can_execute_updateParam_using_versioned_row()
        {
            using (var con = ConnectionString.OpenDbConnection())
            {
                con.CreateTable<SimpleVersionedType>(true);
                var obj = new SimpleVersionedType { Name = "Somename" };
                con.Save(obj);
                var storedObj = con.GetById<SimpleVersionedType>(con.GetLastInsertId());

                Assert.IsNotNull(storedObj, "could not retrive the object from databse");

                int nrRowsChangedFirst = con.UpdateParam(storedObj);

                int nrRowsChangedSecond = con.UpdateParam(storedObj);

                Assert.AreEqual(1, nrRowsChangedFirst, "first update must change exectly one row");
                Assert.AreEqual(0, nrRowsChangedSecond, "second update must not change any rows");


            }
        }
    }



    public class SimpleVersionedType
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

    }

    public class SimpleType
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SimpleAliasedType
    {
        [AutoIncrement]
        public int Id { get; set; }
        [Alias("NewName")]
        public string Name { get; set; }
      
    }
}
