using System;
using System.Data;
using System.Linq.Expressions;
using NUnit.Framework;
using ServiceStack.OrmLite.Sqlite;


namespace ServiceStack.OrmLite.Tests
{
    public class Waybill
    {
        public int Id { get; set; }

        public int Number { get; set; }

        public string Name { get; set; }

        public string VirtProperty => "WaybillVirtPropertyValue";

        public string VirtProperty2 => "WaybillVirtPropertyValue2";

        public bool BoolVirtProperty => false;
    }

    public class CustomSqlServerDialectProvider : SqliteOrmLiteDialectProvider
    {
        public override SqlExpression<T> SqlExpression<T>()
        {
            return new CustomSqlExpression<T>(this);
        }
    }

    public class CustomSqlExpression<T> : SqlExpression<T>
    {

        public CustomSqlExpression(IOrmLiteDialectProvider dialectProvider) : base(dialectProvider)
        {
        }

        protected override Object GetMemberExpression(MemberExpression m)
        {
            if (m.Member.DeclaringType == typeof(Waybill))
            {
                if (m.Member.Name == nameof(Waybill.VirtProperty))
                    return "WaybillVirtPropertyValue";
                if (m.Member.Name == nameof(Waybill.VirtProperty2))
                    return "WaybillVirtPropertyValue2";
                if (m.Member.Name == nameof(Waybill.BoolVirtProperty))
                    return false;
            }

            return base.GetMemberExpression(m);
        }
    }

    [TestFixture]
    public class CustomSqlExpressionTests : OrmLiteTestBase
    {
        private IDbConnection Db;
        
        [OneTimeSetUp]
        public void CustomInit()
        {
            ConnectionString = Config.SqliteMemoryDb;
            DbFactory = new OrmLiteConnectionFactory(ConnectionString, new CustomSqlServerDialectProvider());
        }

        [SetUp]
        public void Setup()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<Waybill>();

                db.CreateTable<Waybill>();
                db.Insert(new Waybill {Id = 1, Number = 100, Name = "first"});
                db.Insert(new Waybill {Id = 2, Number = 200, Name = "second"});
                db.Insert(new Waybill {Id = 3, Number = 300, Name = "third"});
            }
            Db = OpenDbConnection();
        }

        [TearDown]
        public void TearDown()
        {
            Db.Dispose();
        }

        [Test]
        public void Can_Where_using_constant_filter1()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => x.VirtProperty == "WaybillVirtPropertyValue";
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter2()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => x.VirtProperty == "Any";
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter3()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => x.VirtProperty != "WaybillVirtPropertyValue";
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter4()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => x.VirtProperty != "Any";
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter5()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => x.VirtProperty == "WaybillVirtPropertyValue" || x.VirtProperty2 == "WaybillVirtPropertyValue2";
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter6()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => x.VirtProperty == "WaybillVirtPropertyValue" && x.VirtProperty2 == "WaybillVirtPropertyValue2";
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter7()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => x.VirtProperty == "WaybillVirtPropertyValue" || x.VirtProperty2 == "Any";
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter8()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => x.VirtProperty == "WaybillVirtPropertyValue" && x.VirtProperty2 == "Any";
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter9()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => x.BoolVirtProperty;
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter10()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => !x.BoolVirtProperty;
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter11()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => x.BoolVirtProperty && x.VirtProperty == "WaybillVirtPropertyValue";
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter12()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => !x.BoolVirtProperty || x.VirtProperty == "WaybillVirtPropertyValue";
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter13()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => !x.BoolVirtProperty &&
                                                                                  x.VirtProperty == "WaybillVirtPropertyValue" &&
                                                                                  x.Number == 100;
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter14()
        {
            System.Linq.Expressions.Expression<Func<Waybill, bool>> filter = x => x.Number == 100 &&
                                                                                  (x.BoolVirtProperty ||
                                                                                   x.VirtProperty == "WaybillVirtPropertyValue");
            var q = Db.From<Waybill>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(1, target.Count);
        }
    }
}