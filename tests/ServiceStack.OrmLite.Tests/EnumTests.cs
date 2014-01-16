using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    using System;

    public class EnumTests : OrmLiteTestBase
    {
        [Test]
        public void CanCreateTable()
        {
            OpenDbConnection().CreateTable<TypeWithEnum>(true);
        }

        [Test]
        public void CanStoreEnumValue()
        {
            using (var con = OpenDbConnection())
            {
                con.CreateTable<TypeWithEnum>(true);
                con.Save(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
            }
        }

        [Test]
        public void CanGetEnumValue()
        {
            using (var con = OpenDbConnection())
            {
                con.CreateTable<TypeWithEnum>(true);
                var obj = new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 };
                con.Save(obj);
                var target = con.SingleById<TypeWithEnum>(obj.Id);
                Assert.AreEqual(obj.Id, target.Id);
                Assert.AreEqual(obj.EnumValue, target.EnumValue);
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_select_with_expression()
        {
            using (var con = OpenDbConnection())
            {
                con.CreateTable<TypeWithEnum>(true);
                con.Save(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                con.Save(new TypeWithEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                con.Save(new TypeWithEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var results = con.Select<TypeWithEnum>(q => q.EnumValue == SomeEnum.Value1);
                Assert.That(results.Count, Is.EqualTo(2));
                results = con.Select<TypeWithEnum>(q => q.EnumValue == SomeEnum.Value2);
                Assert.That(results.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_select_with_string()
        {
            using (var con = OpenDbConnection())
            {
                con.CreateTable<TypeWithEnum>(true);
                con.Save(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                con.Save(new TypeWithEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                con.Save(new TypeWithEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var target = con.SelectFmt<TypeWithEnum>("EnumValue = {0}", SomeEnum.Value1);

                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_where_with_AnonType()
        {
            using (var con = OpenDbConnection())
            {
                con.CreateTable<TypeWithEnum>(true);
                con.Save(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                con.Save(new TypeWithEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                con.Save(new TypeWithEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var target = con.Where<TypeWithEnum>(new { EnumValue = SomeEnum.Value1 });

                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void can_select_enum_equals_other_enum()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<DoubleState>();
                db.Insert(new DoubleState { Id = "1", State1 = DoubleState.State.OK, State2 = DoubleState.State.KO });
                db.Insert(new DoubleState { Id = "2", State1 = DoubleState.State.OK, State2 = DoubleState.State.OK });
                IEnumerable<DoubleState> doubleStates = db.Select<DoubleState>(x => x.State1 != x.State2);
                Assert.AreEqual(1, doubleStates.Count());
            }
        }

        [Test]
        public void StoresFlagEnumsAsNumericValues()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithFlagsEnum>();
                db.Insert(
                    new TypeWithFlagsEnum { Id = 1, Flags = FlagsEnum.FlagOne | FlagsEnum.FlagTwo | FlagsEnum.FlagThree });

                try
                {
                    var expectedFlags = (int)(FlagsEnum.FlagOne | FlagsEnum.FlagTwo | FlagsEnum.FlagThree);
                    Assert.AreEqual(db.Scalar<int>("SELECT Flags FROM TypeWithFlagsEnum WHERE Id = 1"), expectedFlags);
                }
                catch (FormatException)
                {
                    // Probably a string then
                    var value = db.Scalar<string>("SELECT Flags FROM TypeWithFlagsEnum WHERE Id = 1");
                    throw new Exception(string.Format("Expected integer value but got string value {0}", value));
                }
            }
        }

        [Test]
        public void Creates_int_field_for_enum_flags()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithFlagsEnum>();

                db.GetLastSql().Print();

                Assert.That(db.GetLastSql(), Is.StringContaining("\"Flags\" INT"));
            }
        }

    }


    public class DoubleState
    {
        public enum State
        {
            OK,
            KO
        }

        public string Id { get; set; }
        public State State1 { get; set; }
        public State State2 { get; set; }
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

    [Flags]
    public enum FlagsEnum
    {
        FlagOne = 0x0,
        FlagTwo = 0x01,
        FlagThree = 0x02
    }

    public class TypeWithFlagsEnum
    {
        public int Id { get; set; }
        public FlagsEnum Flags { get; set; }
    }
}
