using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Interfaces;

namespace ServiceStack.OrmLite.Tests
{
    public class EnumAsIntTests : OrmLiteTestBase
    {
        [Test]
        public void CanCreateTable()
        {
            OpenDbConnection().CreateTable<TypeWithIntEnum>(true);
        }

        [Test]
        public void CanStoreEnumValue()
        {
            using (var con = OpenDbConnection())
            {
                con.CreateTable<TypeWithIntEnum>(true);
                con.Save(new TypeWithIntEnum { Id = 1, EnumValue = SomeEnum.Value1 });
            }
        }

        [Test]
        public void CanGetEnumValue()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithIntEnum>();

                var obj = new TypeWithIntEnum { Id = 1, EnumValue = SomeEnum.Value1 };
                db.Save(obj);
                var target = db.SingleById<TypeWithIntEnum>(obj.Id);
                Assert.AreEqual(obj.Id, target.Id);
                Assert.AreEqual(obj.EnumValue, target.EnumValue);
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_select_with_expression()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithIntEnum>();
                db.Save(new TypeWithIntEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithIntEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithIntEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var results = db.Select<TypeWithIntEnum>(q => q.EnumValue == SomeEnum.Value1);
                Assert.That(results.Count, Is.EqualTo(2));
                results = db.Select<TypeWithIntEnum>(q => q.EnumValue == SomeEnum.Value2);
                Assert.That(results.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_select_with_string()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithIntEnum>();
                db.Save(new TypeWithIntEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithIntEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithIntEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var target = db.SelectFmt<TypeWithIntEnum>("EnumValue".SqlColumn() + " = {0}", (int)SomeEnum.Value1);

                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_where_with_AnonType()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithIntEnum>();
                db.Save(new TypeWithIntEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithIntEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithIntEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var target = db.Where<TypeWithIntEnum>(new { EnumValue = SomeEnum.Value1 });

                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void can_select_enum_equals_other_enum()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<DoubleStateEnumAsInt>();
                db.Insert(new DoubleStateEnumAsInt { Id = "1", State1 = DoubleStateEnumAsInt.State.OK, State2 = DoubleStateEnumAsInt.State.KO });
                db.Insert(new DoubleStateEnumAsInt { Id = "2", State1 = DoubleStateEnumAsInt.State.OK, State2 = DoubleStateEnumAsInt.State.OK });
                IEnumerable<DoubleStateEnumAsInt> doubleStates = db.Select<DoubleStateEnumAsInt>(x => x.State1 != x.State2);
                Assert.AreEqual(1, doubleStates.Count());
            }
        }
        
        [Test]
        public void Can_Select_Type_with_Nullable_Enum()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithNullableIntEnum>();

                db.Insert(new TypeWithNullableIntEnum { Id = 1, EnumValue = SomeEnum.Value1, NullableEnumValue = SomeEnum.Value2 });
                db.Insert(new TypeWithNullableIntEnum { Id = 2, EnumValue = SomeEnum.Value1 });

                var rows = db.Select<TypeWithNullableIntEnum>();
                Assert.That(rows.Count, Is.EqualTo(2));

                var row = rows.First(x => x.NullableEnumValue == null);
                Assert.That(row.Id, Is.EqualTo(2));

                rows = db.SqlList<TypeWithNullableIntEnum>("SELECT * FROM {0}"
                    .Fmt(typeof(TypeWithNullableIntEnum).Name.SqlTable()));

                row = rows.First(x => x.NullableEnumValue == null);
                Assert.That(row.Id, Is.EqualTo(2));

                rows = db.SqlList<TypeWithNullableIntEnum>("SELECT * FROM {0}"
                    .Fmt(typeof(TypeWithNullableIntEnum).Name.SqlTable()), new { Id = 2 });

                row = rows.First(x => x.NullableEnumValue == null);
                Assert.That(row.Id, Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_get_Scalar_Enum()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithIntEnum>();

                var row = new TypeWithIntEnum { Id = 1, EnumValue = SomeEnum.Value2 };
                db.Insert(row);

                var someEnum = db.Scalar<SomeEnum>(db.From<TypeWithIntEnum>()
                    .Where(o => o.Id == row.Id)
                    .Select(o => o.EnumValue));

                Assert.That(someEnum, Is.EqualTo(SomeEnum.Value2));
            }
        }
    }

    public class DoubleStateEnumAsInt
    {
        public enum State
        {
            OK,
            KO
        }

        public string Id { get; set; }
        [EnumAsInt]
        public State State1 { get; set; }
        [EnumAsInt]
        public State State2 { get; set; }
    }

    public class TypeWithIntEnum
    {
        public int Id { get; set; }
        [EnumAsInt]
        public SomeEnum EnumValue { get; set; }
    }

    public class TypeWithNullableIntEnum
    {
        public int Id { get; set; }
        [EnumAsInt]
        public SomeEnum EnumValue { get; set; }
        [EnumAsInt]
        public SomeEnum? NullableEnumValue { get; set; }
    }


}
