﻿using System.Collections.Generic;
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
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithEnum>();

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
                db.DropAndCreateTable<TypeWithEnum>();
                db.Save(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var results = db.Select<TypeWithEnum>(q => q.EnumValue == SomeEnum.Value1);
                Assert.That(results.Count, Is.EqualTo(2));
                results = db.Select<TypeWithEnum>(q => q.EnumValue == SomeEnum.Value2);
                Assert.That(results.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_select_with_string()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithEnum>();
                db.Save(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var target = db.SelectFmt<TypeWithEnum>(
                    "EnumValue".SqlColumn() + " = {0}", SomeEnum.Value1);

                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_where_with_AnonType()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithEnum>();
                db.Save(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithEnum { Id = 2, EnumValue = SomeEnum.Value1 });
                db.Save(new TypeWithEnum { Id = 3, EnumValue = SomeEnum.Value2 });

                var target = db.Where<TypeWithEnum>(new { EnumValue = SomeEnum.Value1 });

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
                    Assert.AreEqual(db.Scalar<int>("SELECT Flags FROM {0} WHERE Id = 1"
                        .Fmt("TypeWithFlagsEnum".SqlColumn())), expectedFlags);
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

                var createTableSql = db.GetLastSql().NormalizeSql();
                createTableSql.Print();

                Assert.That(createTableSql, Is.StringContaining("flags int"));
            }
        }

        [Test]
        public void Updates_enum_flags_with_int_value()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithFlagsEnum>();

                db.Insert(new TypeWithFlagsEnum { Id = 1, Flags = FlagsEnum.FlagOne });
                db.Insert(new TypeWithFlagsEnum { Id = 2, Flags = FlagsEnum.FlagTwo });
                db.Insert(new TypeWithFlagsEnum { Id = 3, Flags = FlagsEnum.FlagOne | FlagsEnum.FlagTwo });

                db.Update(new TypeWithFlagsEnum { Id = 1, Flags = FlagsEnum.FlagThree });
                Assert.That(db.GetLastSql(), Is.StringContaining("=@Flags").Or.StringContaining("=:Flags"));
                db.GetLastSql().Print();

                db.UpdateOnly(new TypeWithFlagsEnum { Id = 1, Flags = FlagsEnum.FlagThree }, q => q.Flags);
                Assert.That(db.GetLastSql(), Is.StringContaining("=" + (int)FlagsEnum.FlagThree));
                db.GetLastSql().Print();
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_select_with_expression_enum_flags()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithFlagsEnum>();
                db.Save(new TypeWithFlagsEnum { Id = 1, Flags = FlagsEnum.FlagOne });
                db.Save(new TypeWithFlagsEnum { Id = 2, Flags = FlagsEnum.FlagOne });
                db.Save(new TypeWithFlagsEnum { Id = 3, Flags = FlagsEnum.FlagTwo });

                var results = db.Select<TypeWithFlagsEnum>(q => q.Flags == FlagsEnum.FlagOne);
                db.GetLastSql().Print();
                Assert.That(results.Count, Is.EqualTo(2));
                results = db.Select<TypeWithFlagsEnum>(q => q.Flags == FlagsEnum.FlagTwo);
                db.GetLastSql().Print();
                Assert.That(results.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_select_with_string_enum_flags()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithFlagsEnum>();
                db.Save(new TypeWithFlagsEnum { Id = 1, Flags = FlagsEnum.FlagOne });
                db.Save(new TypeWithFlagsEnum { Id = 2, Flags = FlagsEnum.FlagOne });
                db.Save(new TypeWithFlagsEnum { Id = 3, Flags = FlagsEnum.FlagTwo });

                var target = db.SelectFmt<TypeWithFlagsEnum>(
                    "Flags".SqlColumn() + " = {0}", FlagsEnum.FlagOne);
                db.GetLastSql().Print();
                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void CanQueryByEnumValue_using_where_with_AnonType_enum_flags()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithFlagsEnum>();
                db.Save(new TypeWithFlagsEnum { Id = 1, Flags = FlagsEnum.FlagOne });
                db.Save(new TypeWithFlagsEnum { Id = 2, Flags = FlagsEnum.FlagOne });
                db.Save(new TypeWithFlagsEnum { Id = 3, Flags = FlagsEnum.FlagTwo });

                var target = db.Where<TypeWithFlagsEnum>(new { Flags = FlagsEnum.FlagOne });
                db.GetLastSql().Print();
                Assert.AreEqual(2, target.Count());
            }
        }

        [Test]
        public void Does_save_Enum_with_label_by_default()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithEnum>();

                db.Insert(new TypeWithEnum { Id = 1, EnumValue = SomeEnum.Value1 });
                db.Insert(new TypeWithEnum { Id = 2, EnumValue = SomeEnum.Value2 });

                var row = db.SingleFmt<TypeWithEnum>(
                    "EnumValue".SqlColumn() + " = {0}", "Value2");

                Assert.That(row.Id, Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_save_Enum_as_Integers()
        {
            using (JsConfig.With(treatEnumAsInteger: true))
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithEnumAsInt>();

                db.Insert(new TypeWithEnumAsInt { Id = 1, EnumValue = SomeEnumAsInt.Value1 });
                db.Insert(new TypeWithEnumAsInt { Id = 2, EnumValue = SomeEnumAsInt.Value2 });

                var row = db.SingleFmt<TypeWithEnumAsInt>(
                    "EnumValue".SqlColumn() + " = {0}", "2");

                Assert.That(row.Id, Is.EqualTo(2));
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
        Value1 = 1,
        Value2 = 2,
        Value3 = 3
    }

    public class TypeWithEnum
    {
        public int Id { get; set; }
        public SomeEnum EnumValue { get; set; }
    }

    public enum SomeEnumAsInt
    {
        Value1 = 1,
        Value2 = 2,
        Value3 = 3
    }

    public class TypeWithEnumAsInt
    {
        public int Id { get; set; }
        public SomeEnumAsInt EnumValue { get; set; }
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
