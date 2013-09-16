using System;
using System.Linq;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class ExpressionVisitorTests : OrmLiteTestBase
    {
        [SetUp]
        public void Setup()
        {
            using (var con = OpenDbConnection())
            {
                con.DropAndCreateTable<TestType>();
                con.Insert(new TestType { Id = 1, BoolCol = true, DateCol = new DateTime(2012, 1, 1), TextCol = "asdf", EnumCol = TestEnum.Val0 });
                con.Insert(new TestType { Id = 2, BoolCol = true, DateCol = new DateTime(2012, 2, 1), TextCol = "asdf123", EnumCol = TestEnum.Val1 });
                con.Insert(new TestType { Id = 3, BoolCol = false, DateCol = new DateTime(2012, 3, 1), TextCol = "qwer", EnumCol = TestEnum.Val2 });
                con.Insert(new TestType { Id = 4, BoolCol = false, DateCol = new DateTime(2012, 4, 1), TextCol = "qwer123", EnumCol = TestEnum.Val3 });
            }
        }

        [Test]
        public void Can_Select_by_const_int()
        {
            var target = OpenDbConnection().Select<TestType>(q => q.Id == 1);
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_by_value_returned_by_method_without_params()
        {
            var target = OpenDbConnection().Select<TestType>(q => q.Id == MethodReturningInt());
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_by_value_returned_by_method_with_param()
        {
            var target = OpenDbConnection().Select<TestType>(q => q.Id == MethodReturningInt(1));
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_by_const_enum()
        {
            var target = OpenDbConnection().Select<TestType>(q => q.EnumCol == TestEnum.Val0);
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_by_enum_returned_by_method()
        {
            var target = OpenDbConnection().Select<TestType>(q => q.EnumCol == MethodReturningEnum());
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_using_ToUpper_on_string_property_of_T()
        {
            var target =
                OpenDbConnection().Select<TestType>(q => q.TextCol.ToUpper() == "ASDF");
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_using_ToLower_on_string_property_of_field()
        {
            var obj = new TestType {TextCol = "ASDF"};

            var target =
                OpenDbConnection().Select<TestType>(q => q.TextCol == obj.TextCol.ToLower());
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_using_Constant_Bool_Value()
        {
            var target =
                OpenDbConnection().Select<TestType>(q => q.BoolCol == true);
            Assert.AreEqual(2, target.Count);
        }

        [Test]
        public void Can_Select_using_new()
        {
            using (var con = OpenDbConnection())
            {
                con.Insert(new TestType
                               {
                                   Id = 5,
                                   BoolCol = false,
                                   DateCol = new DateTime(2012, 5, 1),
                                   TextCol = "uiop",
                                   EnumCol = TestEnum.Val3,
                                   ComplexObjCol = new TestType {TextCol = "poiu"}
                               });

                var target =
                    OpenDbConnection().Select<TestType>(
                        q => q.ComplexObjCol == new TestType() {TextCol = "poiu"});
                Assert.AreEqual(1, target.Count);
            }
        }

        [Test]
        public void Can_Select_using_IN()
        {
            var visitor = OrmLiteConfig.DialectProvider.ExpressionVisitor<TestType>();
            visitor.Where(q => Sql.In(q.TextCol, "asdf", "qwer"));
            var target = OpenDbConnection().Select(visitor);
            Assert.AreEqual(2, target.Count);
        }

        [Test]
        public void Can_Select_using_IN_using_params()
        {
            var visitor = OrmLiteConfig.DialectProvider.ExpressionVisitor<TestType>();
            visitor.Where(q => Sql.In(q.Id, 1, 2, 3));
            var target = OpenDbConnection().Select(visitor);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Select_using_IN_using_int_array()
        {
            var visitor = OrmLiteConfig.DialectProvider.ExpressionVisitor<TestType>();
            visitor.Where(q => Sql.In(q.Id, new[] {1, 2, 3}));
            var target = OpenDbConnection().Select(visitor);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Select_using_IN_using_object_array()
        {
            var visitor = OrmLiteConfig.DialectProvider.ExpressionVisitor<TestType>();
            visitor.Where(q => Sql.In(q.Id, new object[] { 1, 2, 3 }));
            var target = OpenDbConnection().Select(visitor);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Select_using_Startswith()
        {
            var target = OpenDbConnection().Select<TestType>(q => q.TextCol.StartsWith("asdf"));
            Assert.AreEqual(2, target.Count);
        }

        [Test]
        public void Can_Selelct_using_chained_string_operations()
        {
            var value = "ASDF";
            var visitor = OrmLiteConfig.DialectProvider.ExpressionVisitor<TestType>();
            visitor.Where(q => q.TextCol.ToUpper().StartsWith(value));
            var target = OpenDbConnection().Select(visitor);
            Assert.AreEqual(2, target.Count);
        }

        [Test]
        public void Can_Select_using_object_Array_Contains()
        {
            var vals = new object[]{ TestEnum.Val0, TestEnum.Val1 };

            var visitor1 = OrmLiteConfig.DialectProvider.ExpressionVisitor<TestType>();
            visitor1.Where(q => vals.Contains(q.EnumCol) || vals.Contains(q.EnumCol));
            var sql1 = visitor1.ToSelectStatement();

            var visitor2 = OrmLiteConfig.DialectProvider.ExpressionVisitor<TestType>();
            visitor2.Where(q => Sql.In(q.EnumCol, vals) || Sql.In(q.EnumCol, vals));
            var sql2 = visitor2.ToSelectStatement();

            Assert.AreEqual(sql1, sql2);
        }

        [Test]
        public void Can_Select_using_int_Array_Contains()
        {
            var vals = new int[] { (int)TestEnum.Val0, (int)TestEnum.Val1 };

            var visitor1 = OrmLiteConfig.DialectProvider.ExpressionVisitor<TestType>();
            visitor1.Where(q => vals.Contains((int)q.EnumCol) || vals.Contains((int)q.EnumCol));
            var sql1 = visitor1.ToSelectStatement();

            var visitor2 = OrmLiteConfig.DialectProvider.ExpressionVisitor<TestType>();
            visitor2.Where(q => Sql.In(q.EnumCol, vals) || Sql.In(q.EnumCol, vals));
            var sql2 = visitor2.ToSelectStatement();

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
    }
}
