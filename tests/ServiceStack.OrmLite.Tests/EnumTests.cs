using System.Linq;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests
{
    public class EnumTests : OrmLiteTestBase
    {
        [Test]
        public void CanCreateTable()
        {
            ConnectionString.OpenDbConnection().CreateTable<TypeWithEnum>(true);
        }

        [Test]
        public void CanStoreEnumValue()
        {
            using(var con = ConnectionString.OpenDbConnection())
            {
                con.CreateTable<TypeWithEnum>(true);
                con.Save(new TypeWithEnum {Id = 1, EnumValue = SomeEnum.Value1});
            }
        }

        [Test]
        public void CanGetEnumValue()
        {
            using (var con = ConnectionString.OpenDbConnection())
            {
                con.CreateTable<TypeWithEnum>(true);
                var obj = new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 };
                con.Save(obj);
                var target = con.GetById<TypeWithEnum>(obj.Id);
                Assert.AreEqual(obj.Id, target.Id);
                Assert.AreEqual(obj.EnumValue, target.EnumValue);
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_select_with_expression()
        {
            using (var con = ConnectionString.OpenDbConnection())
            {
                con.CreateTable<TypeWithEnum>(true);
                con.Save(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                con.Save(new TypeWithEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                con.Save(new TypeWithEnum { Id = 3, EnumValue = SomeEnum.Value2 });
                con.Save(new TypeWithEnum { Id = 4, EnumValue = SomeEnum.Value3 });

                var target = con.Select<TypeWithEnum>(q => q.EnumValue == SomeEnum.Value1);
                Assert.AreEqual(2, target.Count());

                target = con.Select<TypeWithEnum>(q => SomeEnum.Value1 == q.EnumValue);
                Assert.AreEqual(2, target.Count());

                target = con.Select<TypeWithEnum>(q => q.EnumValue != SomeEnum.Value2 && q.EnumValue != SomeEnum.Value3);
                Assert.AreEqual(2, target.Count());

                target = con.Select<TypeWithEnum>(q => q.EnumValue != SomeEnum.Value2);
                Assert.AreEqual(3, target.Count());
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_select_with_string()
        {
            using (var con = ConnectionString.OpenDbConnection())
            {
                con.CreateTable<TypeWithEnum>(true);
                con.Save(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                con.Save(new TypeWithEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                con.Save(new TypeWithEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var target = con.Select<TypeWithEnum>("EnumValue = {0}", SomeEnum.Value1);

                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_where_with_AnonType()
        {
            using (var con = ConnectionString.OpenDbConnection())
            {
                con.CreateTable<TypeWithEnum>(true);
                con.Save(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                con.Save(new TypeWithEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                con.Save(new TypeWithEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var target = con.Where<TypeWithEnum>(new { EnumValue = SomeEnum.Value1 });

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
