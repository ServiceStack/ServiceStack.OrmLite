using System.Data;
using System.Linq;
using NUnit.Framework;

namespace ServiceStack.OrmLite.VistaDB.Tests
{
    public class EnumTests : OrmLiteTestBase
    {
        [Test]
        public void CanCreateTable()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<TypeWithEnum>(true);
            }
        }

        [Test]
        public void CanStoreEnumValue()
        {
            using(var db = OpenDbConnection())
            {
                db.CreateTable<TypeWithEnum>(true);
                db.Save(new TypeWithEnum {Id = 1, EnumValue = SomeEnum.Value1});
            }
        }

        [Test]
        public void CanGetEnumValue()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<TypeWithEnum>(true);
                var obj = new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 };
                db.Save(obj);
                var target = db.SingleById<TypeWithEnum>(obj.Id);
                Assert.AreEqual(obj.Id, target.Id);
                Assert.AreEqual(obj.EnumValue, target.EnumValue);
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_select_with_expression()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<TypeWithEnum>(true);
                db.Save(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var target = db.Select<TypeWithEnum>(q => q.EnumValue == SomeEnum.Value1);

                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_select_with_string()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<TypeWithEnum>(true);
                db.Save(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var target = db.Select<TypeWithEnum>("EnumValue = @value", new { value = SomeEnum.Value1 });

                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_where_with_AnonType()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<TypeWithEnum>(true);
                db.Save(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var target = db.Where<TypeWithEnum>(new { EnumValue = SomeEnum.Value1 });

                Assert.AreEqual(2, target.Count());
            }
        }
    }

    public enum SomeEnum
    {
        Value1,
        Value2,
        Value3
    }

    public class TypeWithEnum
    {
        public int Id { get; set; }
        public SomeEnum EnumValue { get; set; } 
    }
}
