using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class ExpressionVisitorTests : OrmLiteTestBase
    {
        private IDbConnection Db;

        [SetUp]
        public void Setup()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TestType>();
                db.Insert(new TestType { Id = 1, BoolCol = true, DateCol = new DateTime(2012, 1, 1), TextCol = "asdf", EnumCol = TestEnum.Val0, NullableIntCol = 10 });
                db.Insert(new TestType { Id = 2, BoolCol = true, DateCol = new DateTime(2012, 2, 1), TextCol = "asdf123", EnumCol = TestEnum.Val1, NullableIntCol = null });
                db.Insert(new TestType { Id = 3, BoolCol = false, DateCol = new DateTime(2012, 3, 1), TextCol = "qwer", EnumCol = TestEnum.Val2, NullableIntCol = 30 });
                db.Insert(new TestType { Id = 4, BoolCol = false, DateCol = new DateTime(2012, 4, 1), TextCol = "qwer123", EnumCol = TestEnum.Val3, NullableIntCol = 40 });
            }
            Db = OpenDbConnection();
        }

        [TearDown]
        public void TearDown()
        {
            Db.Dispose();
        }

        [Test]
        public void Can_Select_by_const_int()
        {
            var target = Db.Select<TestType>(q => q.Id == 1);
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_by_value_returned_by_method_without_params()
        {
            var target = Db.Select<TestType>(q => q.Id == MethodReturningInt());
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_by_value_returned_by_method_with_param()
        {
            var target = Db.Select<TestType>(q => q.Id == MethodReturningInt(1));
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_by_const_enum()
        {
            var target = Db.Select<TestType>(q => q.EnumCol == TestEnum.Val0);
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_by_enum_returned_by_method()
        {
            var target = Db.Select<TestType>(q => q.EnumCol == MethodReturningEnum());
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_using_ToUpper_on_string_property_of_T()
        {
            var target =
                Db.Select<TestType>(q => q.TextCol.ToUpper() == "ASDF");
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_using_ToLower_on_string_property_of_field()
        {
            var obj = new TestType {TextCol = "ASDF"};

            var target =
                Db.Select<TestType>(q => q.TextCol == obj.TextCol.ToLower());
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_using_Constant_Bool_Value()
        {
            var target =
                Db.Select<TestType>(q => q.BoolCol == true);
            Assert.AreEqual(2, target.Count);
        }

        [Test]
        public void Can_Select_using_new_ComplexType()
        {
            Db.Insert(new TestType
            {
                Id = 5,
                BoolCol = false,
                DateCol = new DateTime(2012, 5, 1),
                TextCol = "uiop",
                EnumCol = TestEnum.Val3,
                ComplexObjCol = new TestType { TextCol = "poiu" }
            });

            var target = Db.Select<TestType>(
                    q => q.ComplexObjCol == new TestType { TextCol = "poiu" });
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_using_IN()
        {
            var q = Db.From<TestType>();
            q.Where(x => Sql.In(x.TextCol, "asdf", "qwer"));
            var target = Db.Select(q);
            Assert.AreEqual(2, target.Count);
        }

        [Test]
        public void Can_Select_using_IN_using_params()
        {
            var q = Db.From<TestType>();
            q.Where(x => Sql.In(x.Id, 1, 2, 3));
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Select_using_IN_using_int_array()
        {
            var q = Db.From<TestType>();
            q.Where(x => Sql.In(x.Id, new[] {1, 2, 3}));
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Select_using_IN_using_object_array()
        {
            var q = Db.From<TestType>();
            q.Where(x => Sql.In(x.Id, new object[] { 1, 2, 3 }));
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Select_using_int_array_Contains()
        {
            var ids = new[] { 1, 2 };
            var q = Db.From<TestType>().Where(x => ids.Contains(x.Id));
            var target = Db.Select(q);
            CollectionAssert.AreEquivalent(ids, target.Select(t => t.Id).ToArray());
        }

        [Test]
        public void Can_Select_using_int_list_Contains()
        {
            var ids = new List<int> { 1, 2 };
            var q = Db.From<TestType>().Where(x => ids.Contains(x.Id));
            var target = Db.Select(q);
            CollectionAssert.AreEquivalent(ids, target.Select(t => t.Id).ToArray());
        }

        [Test]
        public void Can_Select_using_int_array_Contains_Value()
        {
            var ints = new[] { 10, 40 };
            var q = Db.From<TestType>().Where(x => ints.Contains(x.NullableIntCol.Value)); // Doesn't compile without ".Value" here - "ints" is not nullable
            var target = Db.Select(q);
            CollectionAssert.AreEquivalent(new[] { 1, 4 }, target.Select(t => t.Id).ToArray());
        }

        [Test]
        public void Can_Select_using_Startswith()
        {
            var target = Db.Select<TestType>(q => q.TextCol.StartsWith("asdf"));
            Assert.AreEqual(2, target.Count);
        }

        [Test]
        public void Can_Select_using_Endswith()
        {
            var target = Db.Select<TestType>(q => q.TextCol.EndsWith("123"));
            Assert.AreEqual(2, target.Count);
        }

        [Test]
        public void Can_Select_using_Contains()
        {
            var target = Db.Select<TestType>(q => q.TextCol.Contains("df"));
            Assert.AreEqual(2, target.Count);
        }

        [Test]
        public void Can_Selelct_using_chained_string_operations()
        {
            var value = "ASDF";
            var q = Db.From<TestType>();
            q.Where(x => x.TextCol.ToUpper().StartsWith(value));
            var target = Db.Select(q);
            Assert.AreEqual(2, target.Count);
        }

        [Test]
        public void Can_Select_using_object_Array_Contains()
        {
            var vals = new object[]{ TestEnum.Val0, TestEnum.Val1 };

            var q1 = Db.From<TestType>();
            q1.Where(q => vals.Contains(q.EnumCol) || vals.Contains(q.EnumCol));
            var sql1 = q1.ToSelectStatement();

            var q2 = Db.From<TestType>();
            q2.Where(q => Sql.In(q.EnumCol, vals) || Sql.In(q.EnumCol, vals));
            var sql2 = q2.ToSelectStatement();

            Assert.AreEqual(sql1, sql2);
        }

        [Test]
        public void Can_Select_using_int_Array_Contains()
        {
            var vals = new int[] { (int)TestEnum.Val0, (int)TestEnum.Val1 };

            var q1 = Db.From<TestType>();
            q1.Where(q => vals.Contains((int)q.EnumCol) || vals.Contains((int)q.EnumCol));
            var sql1 = q1.ToSelectStatement();

            var q2 = Db.From<TestType>();
            q2.Where(q => Sql.In(q.EnumCol, vals) || Sql.In(q.EnumCol, vals));
            var sql2 = q2.ToSelectStatement();

            Assert.AreEqual(sql1, sql2);
        }

        private int MethodReturningInt(int val)
        {
            return val;
        }

        private int MethodReturningInt()
        {
            return 1;
        }

        private TestEnum MethodReturningEnum()
        {
            return TestEnum.Val0;
        }
    }

    public enum TestEnum
    {
        Val0 = 0,
        Val1,
        Val2,
        Val3
    }

    public class TestType
    {
        public int Id { get; set; }
        public string TextCol { get; set; }
        public bool BoolCol { get; set; }
        public DateTime DateCol { get; set; }
        public TestEnum EnumCol { get; set; }
        public TestType ComplexObjCol { get; set; }
        public int? NullableIntCol { get; set; }
    }
}
