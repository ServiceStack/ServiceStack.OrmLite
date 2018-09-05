using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Sqlite;


namespace ServiceStack.OrmLite.Tests
{
    public class ObjectBase
    {
        [PrimaryKey]
        public int Id { get; set; }
    }

    public class WaybillBase : ObjectBase
    {
        public int Number { get; set; }

        public string Name { get; set; }

        public Guid Id1 { get; set; }

        public Guid Id2 { get; set; }

        public DateTime Date { get; set; }

        public decimal Amount { get; set; }

        [DataAnnotations.Ignore]
        public string VirtProperty => "WaybillVirtPropertyValue";

        [DataAnnotations.Ignore]
        public string VirtProperty2 => "WaybillVirtPropertyValue2";

        [DataAnnotations.Ignore]
        public string VirtPropertyEmpty => String.Empty;

        [DataAnnotations.Ignore]
        public string VirtPropertyNull => null;

        [DataAnnotations.Ignore]
        public bool BoolVirtProperty => false;

        [DataAnnotations.Ignore]
        public bool? NullableTrueBoolVirtProperty => true;

        [DataAnnotations.Ignore]
        public bool? NullableNullBoolVirtProperty => null;

        [DataAnnotations.Ignore]
        public Guid GuidVirtProperty => Guid.Parse("00000000-0000-0000-0000-000000000000");

        [DataAnnotations.Ignore]
        public DateTime DateVirtProperty => DateTime.Parse("2000-01-01");

        [DataAnnotations.Ignore]
        public int IntVirtProperty => 5;

        [DataAnnotations.Ignore]
        public decimal DecimalVirtProperty => 10M;
    }

    public class WaybillIn : WaybillBase
    {
        public DateTime DateBegin { get; set; }

        public DateTime DateEnd { get; set; }

        public string Note { get; set; }
    }

    /// <summary>
    /// Class only for creating the table and population it with data.
    /// </summary>
    [Alias(nameof(WaybillIn))]
    public class SeparateWaybillIn
    {
        public int Id { get; set; }

        public DateTime DateBegin { get; set; }

        public DateTime DateEnd { get; set; }

        public string Note { get; set; }
    }

    public class CustomSqlServerDialectProvider : SqliteOrmLiteDialectProvider
    {
        public override SqlExpression<T> SqlExpression<T>()
        {
            return new CustomSqlExpression<T>(this);
        }
    }

    public class CustomSqlExpression<T> : SqliteExpression<T>
    {

        public CustomSqlExpression(IOrmLiteDialectProvider dialectProvider) : base(dialectProvider)
        {
        }

        protected override Object GetMemberExpression(MemberExpression m)
        {
            if (m.Member.DeclaringType == typeof(WaybillBase))
            {
                if (m.Member.Name == nameof(WaybillBase.VirtProperty))
                    return "WaybillVirtPropertyValue";
                if (m.Member.Name == nameof(WaybillBase.VirtProperty2))
                    return "WaybillVirtPropertyValue2";
                if (m.Member.Name == nameof(WaybillBase.VirtPropertyEmpty))
                    return String.Empty;
                if (m.Member.Name == nameof(WaybillBase.VirtPropertyNull))
                    return null;
                if (m.Member.Name == nameof(WaybillBase.BoolVirtProperty))
                    return false;
                if (m.Member.Name == nameof(WaybillBase.NullableTrueBoolVirtProperty))
                    return true;
                if (m.Member.Name == nameof(WaybillBase.NullableNullBoolVirtProperty))
                    return null;
                if (m.Member.Name == nameof(WaybillBase.GuidVirtProperty))
                    return Guid.Parse("00000000-0000-0000-0000-000000000000");
                if (m.Member.Name == nameof(WaybillBase.DateVirtProperty))
                    return DateTime.Parse("2000-01-01");
                if (m.Member.Name == nameof(WaybillBase.IntVirtProperty))
                    return 5;
                if (m.Member.Name == nameof(WaybillBase.DecimalVirtProperty))
                    return 10M;
            }

            return base.GetMemberExpression(m);
        }

        protected override string GetQuotedColumnName(ModelDefinition tableDef, string memberName)
        {
            if (useFieldName)
            {
                var actualTableDefForMember = GetCurrentTableDef(tableDef, memberName);
                if (tableDef.ModelName != actualTableDefForMember.ModelName)
                {
                    CreateHierarchyJoin(actualTableDefForMember, tableDef);
                }

                return base.GetQuotedColumnName(actualTableDefForMember, memberName);
            }

            return base.GetQuotedColumnName(tableDef, memberName);
        }

        protected virtual void CreateHierarchyJoin(ModelDefinition actualHierarchyTableDef, ModelDefinition mainHierarchyTableDef)
        {
        }

        private ModelDefinition GetCurrentTableDef(ModelDefinition tableDef, string memberName)
        {
            var curType = tableDef.ModelType;
            var nonInheritedProperties = GetCurrentPropertiesWithoutBase(tableDef);
            while (curType != null && !nonInheritedProperties.Contains(memberName))
            {
                curType = curType.BaseType;
                nonInheritedProperties = GetCurrentPropertiesWithoutBase(curType?.GetModelMetadata());
            }

            return curType?.GetModelMetadata() ?? tableDef;
        }

        protected virtual List<string> GetCurrentPropertiesWithoutBase(ModelDefinition currentModelDef)
        {
            if (currentModelDef == null) return null;

            var baseType = currentModelDef.ModelType;
            var res = new List<string> { currentModelDef.PrimaryKey.Name };

            res.AddRange(baseType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly).Select(a => a.Name));

            return res;
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
                db.DropTable<WaybillBase>();

                db.CreateTable<WaybillBase>();
                db.Insert(new WaybillBase
                {
                    Id = 1,
                    Number = 100,
                    Name = "first",
                    Id1 = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Id2 = Guid.Parse("51111111-1111-1111-1111-111111111111"),
                    Date = DateTime.Parse("2001-01-01"),
                    Amount = 20M
                });
                db.Insert(new WaybillBase
                {
                    Id = 2,
                    Number = 200,
                    Name = "second",
                    Id1 = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Id2 = Guid.Parse("52222222-2222-2222-2222-222222222222"),
                    Date = DateTime.Parse("2002-01-01"),
                    Amount = 30M
                });
                db.Insert(new WaybillBase
                {
                    Id = 3,
                    Number = 300,
                    Name = "third",
                    Id1 = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Id2 = Guid.Parse("53333333-3333-3333-3333-333333333333"),
                    Date = DateTime.Parse("2003-01-01"),
                    Amount = 40M
                });

                db.DropTable<SeparateWaybillIn>();

                db.CreateTable<SeparateWaybillIn>();
                db.Insert(new SeparateWaybillIn { Id = 1, DateBegin = DateTime.Parse("2014-01-01"), DateEnd = DateTime.Parse("2014-01-03"), Note = "firstNote"});
                db.Insert(new SeparateWaybillIn { Id = 2, DateBegin = DateTime.Parse("2015-01-01"), DateEnd = DateTime.Parse("2015-01-03"), Note = "secondNote" });
                db.Insert(new SeparateWaybillIn { Id = 3, DateBegin = DateTime.Parse("2016-01-01"), DateEnd = DateTime.Parse("2016-01-03"), Note = "thirdNote" });
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
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty == "WaybillVirtPropertyValue";
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter2()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty == "Any";
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter3()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty != "WaybillVirtPropertyValue";
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter4()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty != "Any";
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter5()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty == "WaybillVirtPropertyValue" || x.VirtProperty2 == "WaybillVirtPropertyValue2";
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter6()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty == "WaybillVirtPropertyValue" && x.VirtProperty2 == "WaybillVirtPropertyValue2";
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter7()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty == "WaybillVirtPropertyValue" || x.VirtProperty2 == "Any";
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter8()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty == "WaybillVirtPropertyValue" && x.VirtProperty2 == "Any";
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter9()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.BoolVirtProperty;
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter10()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => !x.BoolVirtProperty;
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter11()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.BoolVirtProperty && x.VirtProperty == "WaybillVirtPropertyValue";
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter12()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => !x.BoolVirtProperty || x.VirtProperty == "WaybillVirtPropertyValue";
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter13()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => !x.BoolVirtProperty &&
                                                                                  x.VirtProperty == "WaybillVirtPropertyValue" &&
                                                                                  x.Number == 100;
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_filter14()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.Number == 100 &&
                                                                                  (x.BoolVirtProperty ||
                                                                                   x.VirtProperty == "WaybillVirtPropertyValue");
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Select_hierarchy_classes1()
        {
            var q1 = Db.From<WaybillIn>();
            q1.PrefixFieldWithTableName = true;
            q1.Select(x => new {x.Name, x.Number});
            q1.SelectInto<WaybillIn>();
            var sql1 = q1.SelectExpression;

            var q2 = Db.From<WaybillBase>();
            q2.PrefixFieldWithTableName = true;
            q2.Select(x => new {x.Name, x.Number});
            q2.SelectInto<WaybillIn>();
            var sql2 = q2.SelectExpression;

            Assert.AreEqual(sql1, sql2);
        }

        [Test]
        public void Can_Select_hierarchy_classes2()
        {
            var q = Db.From<WaybillIn>();
            q.PrefixFieldWithTableName = true;
            q.Join<WaybillBase>((x, y) => x.Id == y.Id);
            q.Where(x => x.Name == "first" && x.Note == "firstNote");
            var target = Db.Select(q);

            Assert.AreEqual(1, target.Count);

            var obj = target[0];
            Assert.AreEqual(DateTime.Parse("2014-01-01"), obj.DateBegin);
        }

        [Test]
        public void Can_Select_hierarchy_classes3()
        {
            var q = Db.From<WaybillIn>();
            q.PrefixFieldWithTableName = true;
            q.Join<WaybillBase>((x, y) => x.Id == y.Id);
            q.Where(x => x.Name == "first" && x.Note == "firstNote");
            q.Select(new [] {nameof(WaybillBase.Number)});
            var target = Db.Column<int>(q);

            Assert.AreEqual(1, target.Count);

            var obj = target[0];
            Assert.AreEqual(100, obj);
        }

        [Test]
        public void Can_Select_hierarchy_classes4()
        {
            var q = Db.From<WaybillIn>();
            q.PrefixFieldWithTableName = true;
            q.Join<WaybillBase>((x, y) => x.Id == y.Id);
            q.Where(x => x.Name == "first" && x.Note == "firstNote");
            q.OrderByFields(nameof(WaybillBase.Number));
            var target = Db.Select(q);

            Assert.AreEqual(1, target.Count);

            var obj = target[0];
            Assert.AreEqual(DateTime.Parse("2014-01-01"), obj.DateBegin);
        }

        [Test]
        public void Can_Where_using_constant_orderBy1()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => !x.BoolVirtProperty;
            System.Linq.Expressions.Expression<Func<WaybillBase, object>> orderBy = x => x.BoolVirtProperty;
            var q = Db.From<WaybillBase>().Where(filter).OrderBy(orderBy);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_orderBy2()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => !x.BoolVirtProperty &&
                                                                                      x.VirtPropertyEmpty != "WaybillVirtPropertyValue" &&
                                                                                      x.Number == 100;
            System.Linq.Expressions.Expression<Func<WaybillBase, object>> orderBy = x => x.VirtProperty;
            var q = Db.From<WaybillBase>().Where(filter).OrderBy(orderBy);
            var target = Db.Select(q);
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_coniditionalOrderBy()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => !x.BoolVirtProperty &&
                                                                                      x.VirtPropertyEmpty != "WaybillVirtPropertyValue" &&
                                                                                      x.Number == 100;
            System.Linq.Expressions.Expression<Func<WaybillBase, object>> orderBy = x => x.Number > 0 ? x.VirtPropertyEmpty : x.Name;
            var q = Db.From<WaybillBase>().Where(filter).OrderBy(orderBy);
            var target = Db.Select(q);
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Where_using_constant_func_where()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty.StartsWith("Way");
            System.Linq.Expressions.Expression<Func<WaybillBase, object>> orderBy = x => x.Name;
            var q = Db.From<WaybillBase>().Where(filter).OrderByDescending(orderBy);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Select_using_constant()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => !x.BoolVirtProperty &&
                                                                                      x.VirtPropertyEmpty != "WaybillVirtPropertyValue" &&
                                                                                      x.Number == 100;

            System.Linq.Expressions.Expression<Func<WaybillBase, object>> select = x => x.VirtProperty;
            var q = Db.From<WaybillBase>().Where(filter).Select(select);
            var target = Db.Column<string>(q);
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual("WaybillVirtPropertyValue", target[0]);
        }

        [Test]
        public void Can_Where_using_guid_constant_coniditional()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => (x.Number > 0 ? x.GuidVirtProperty : x.Id1) == Guid.Parse("00000000-0000-0000-0000-000000000000");
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_null_constant_coniditional()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => (x.Number == 0 ? x.VirtPropertyNull : x.Name) == "first";
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(1, target.Count);
        }

        [Test]
        public void Can_Where_using_datetime_constant_coniditional()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => (x.Number > 0 ? x.DateVirtProperty : x.Date) == DateTime.Parse("2000-01-01");
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_int_constant_coniditional()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => (x.Number > 0 ? x.IntVirtProperty : x.Number) == 5;
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_decimal_constant_coniditional()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => (x.Number > 0 ? x.DecimalVirtProperty : x.Amount) == 10M;
            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_GroupBy_using_constant()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => (x.Number > 0 ? x.DecimalVirtProperty : x.Amount) == 10M;
            var q = Db.From<WaybillBase>().Where(filter).GroupBy(x => x.VirtProperty);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Having_using_constant()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => (x.Number > 0 ? x.DecimalVirtProperty : x.Amount) == 10M;
            var q = Db.From<WaybillBase>().Where(filter).Select(x=>x.Name).GroupBy(x => x.Name).Having(x=>x.VirtProperty == "WaybillVirtPropertyValue");
            var target = Db.Column<string>(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty1()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty.Length == 0;

            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty2()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty.Length == 24;

            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty3()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty.Length == 0 && x.Id > 0;

            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty4()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtPropertyNull.Length == 0;

            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty5()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtPropertyNull.Length == 0 && x.Id > 0;

            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty6()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.BoolVirtProperty != null;//should be always true

            var q = Db.From<WaybillBase>().Where(filter);
            Assert.That(q.ToSelectStatement().ToLower(), Does.Not.Contain("where null"));

            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty7()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => null != x.BoolVirtProperty;//should be always true

            var q = Db.From<WaybillBase>().Where(filter);
            Assert.That(q.ToSelectStatement().ToLower(), Does.Not.Contain("where null"));

            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty8()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => null != x.BoolVirtProperty && x.Id > 0;

            var q = Db.From<WaybillBase>().Where(filter);
            Assert.That(q.ToSelectStatement().ToLower(), Does.Not.Contain("where null"));

            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty9()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.BoolVirtProperty == null;//should be always false

            var q = Db.From<WaybillBase>().Where(filter);
            Assert.That(q.ToSelectStatement().ToLower(), Does.Not.Contain("where not (null)"));

            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty10()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => null == x.BoolVirtProperty;//should be always false

            var q = Db.From<WaybillBase>().Where(filter);
            Assert.That(q.ToSelectStatement().ToLower(), Does.Not.Contain("where not (null)"));

            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty11()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.NullableTrueBoolVirtProperty == null;

            var q = Db.From<WaybillBase>().Where(filter);
            Assert.That(q.ToSelectStatement().ToLower(), Does.Not.Contain("where null"));

            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty12()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.NullableTrueBoolVirtProperty != null;

            var q = Db.From<WaybillBase>().Where(filter);
            Assert.That(q.ToSelectStatement().ToLower(), Does.Not.Contain("where not (null)"));

            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty13()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.NullableNullBoolVirtProperty == null;

            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty14()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.NullableNullBoolVirtProperty != null;

            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty15()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.NullableNullBoolVirtProperty == true;

            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty16()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.BoolVirtProperty != false;

            var q = Db.From<WaybillBase>().Where(filter);
            Assert.That(q.ToSelectStatement().ToLower(), Does.Not.Contain("where null"));

            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty17()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtPropertyNull == null;

            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty18()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtPropertyNull != null;

            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty19()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtPropertyNull == "null";

            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty20()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty == null;

            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void Can_Where_using_StringLengthVirtualProperty21()
        {
            System.Linq.Expressions.Expression<Func<WaybillBase, bool>> filter = x => x.VirtProperty == "WaybillVirtPropertyValue";

            var q = Db.From<WaybillBase>().Where(filter);
            var target = Db.Select(q);
            Assert.AreEqual(3, target.Count);
        }
    }
}