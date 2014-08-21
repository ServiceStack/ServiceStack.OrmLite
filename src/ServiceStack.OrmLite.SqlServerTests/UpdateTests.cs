using NUnit.Framework;
using ServiceStack.Data;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServerTests
{
    public class UpdateTests : OrmLiteTestBase
    {
        [Test]
        public void Can_execute_update_using_expression()
        {
            using (var con = OpenDbConnection())
            {
                con.CreateTable<SimpleType>(true);
                var obj = new SimpleType { Name = "Somename" };
                con.Save(obj);
                var storedObj = con.SingleById<SimpleType>(obj.Id);

                Assert.AreEqual(obj.Name, storedObj.Name);

                obj.Id = storedObj.Id;
                obj.Name = "Someothername";
                con.Update(obj, q => q.Id == storedObj.Id);

                var target = con.SingleById<SimpleType>(storedObj.Id);

                Assert.AreEqual(obj.Name, target.Name);
            }
        }

        [Test]
        public void Can_execute_update_only()
        {
            using (var con = OpenDbConnection())
            {
                con.CreateTable<SimpleType>(true);
                var obj = new SimpleType { Name = "Somename" };
                con.Save(obj);
                var storedObj = con.SingleById<SimpleType>(obj.Id);

                Assert.AreEqual(obj.Name, storedObj.Name);

                var ev = OrmLiteConfig.DialectProvider.SqlExpression<SimpleType>();
                ev.Update();
                ev.Where(q => q.Id == storedObj.Id); 
                storedObj.Name = "Someothername";

                con.UpdateOnly(storedObj, ev);

                var target = con.SingleById<SimpleType>(storedObj.Id);

                Assert.AreEqual("Someothername", target.Name);
            }
        }


        [Test]
        public void Can_execute_update()
        {
            using (var con = OpenDbConnection())
            {
                con.CreateTable<SimpleType>(true);
                var obj = new SimpleType { Name = "Somename" };
                con.Save(obj);
                var storedObj = con.SingleById<SimpleType>(obj.Id);

                Assert.AreEqual(obj.Name, storedObj.Name);

                obj.Id = storedObj.Id;
                obj.Name = "Someothername";
                con.Update(obj);

                var target = con.SingleById<SimpleType>(storedObj.Id);

                Assert.AreEqual(obj.Name, target.Name);
            }
        }

        [Test]
        public void Can_execute_update_using_aliased_columns()
        {
            using (var con = OpenDbConnection())
            {
                con.CreateTable<SimpleAliasedType>(true);
                var obj = new SimpleAliasedType { Name = "Somename" };
                con.Save(obj);
                var storedObj = con.SingleById<SimpleAliasedType>(obj.Id);

                Assert.AreEqual(obj.Name, storedObj.Name);

                obj.Id = storedObj.Id;
                obj.Name = "Someothername";
                con.Update(obj);

                var target = con.SingleById<SimpleAliasedType>(storedObj.Id);

                Assert.AreEqual(obj.Name, target.Name);
            }
        }

        [Test]
        public void Can_execute_updateParam()
        {
            using (var con = OpenDbConnection())
            {
                con.CreateTable<SimpleType>(true);
                var obj = new SimpleType { Name = "Somename" };
                con.Save(obj);
                var storedObj = con.SingleById<SimpleType>(obj.Id);

                Assert.AreEqual(obj.Name, storedObj.Name);

                obj.Id = storedObj.Id;
                obj.Name = "Someothername";
                con.Update(obj);

                var target = con.SingleById<SimpleType>(storedObj.Id);

                Assert.AreEqual(obj.Name, target.Name);
            }
        }

        [Test]
        public void Can_execute_updateParam_using_aliased_columns()
        {
            using (var con = OpenDbConnection())
            {
                con.CreateTable<SimpleAliasedType>(true);
                var obj = new SimpleAliasedType { Name = "Somename" };
                con.Save(obj);
                var storedObj = con.SingleById<SimpleAliasedType>(obj.Id);

                Assert.AreEqual(obj.Name, storedObj.Name);

                obj.Id = storedObj.Id;
                obj.Name = "Someothername";
                con.Update(obj);

                var target = con.SingleById<SimpleAliasedType>(storedObj.Id);

                Assert.AreEqual(obj.Name, target.Name);
            }
        }
            [Test]
        public void Can_update_versioned()
        {
            using (var con = OpenDbConnection())
            {
                con.CreateTable<SqlVersionedType>(true);
                var rowId = con.Insert(new SqlVersionedType { Text = "Text" }, selectIdentity: true);

                var row = con.SingleById<SqlVersionedType>(rowId);

                row.Text += " Updated";

                con.Update(row);

                var updatedRow = con.SingleById<SqlVersionedType>(rowId);

                Assert.That(updatedRow.Text, Is.EqualTo("Text Updated"));
              
                row.Text += " Again";

                //Can't update old record
                Assert.Throws<OptimisticConcurrencyException>(() =>
                    con.Update(row));

                //Can update latest version
                updatedRow.Text += " Again";
                con.Update(updatedRow);
            }
        }
    }

    public class SqlVersionedType
    {
        [AutoIncrement]
        public int Id { get; set; }
        [RowVersion]
        public byte[] Timestamp { get; set; }
        public string Text { get; set; }
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
