using System;
using System.Configuration;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;

namespace ServiceStack.OrmLite.MySql.Tests
{
	[TestFixture]
	public class OrmLiteComplexTypesTests : OrmLiteTestBase
	{
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogManager.LogFactory = new ConsoleLogFactory();

            OrmLiteConfig.DialectProvider = MySqlDialectProvider.Instance;
            ConnectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;
        }

		[Test]
		public void Can_insert_into_ModelWithComplexTypes_table()
		{
            using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithComplexTypes>(true);

				var row = ModelWithComplexTypes.Create(1);

				db.Insert(row);
			}
		}

		[Test]
		public void Can_insert_and_select_from_ModelWithComplexTypes_table()
		{
            using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithComplexTypes>(true);

				var row = ModelWithComplexTypes.Create(1);

				db.Insert(row);

				var rows = db.Select<ModelWithComplexTypes>();

				Assert.That(rows, Has.Count.EqualTo(1));

				ModelWithComplexTypes.AssertIsEqual(rows[0], row);
			}
		}

		[Test]
		public void Can_insert_and_select_from_OrderLineData()
		{
            using (var db = OpenDbConnection())
			{
				db.CreateTable<SampleOrderLine>(true);

				var orderIds = new[] { 1, 2, 3, 4, 5 }.ToList();

				orderIds.ForEach(x => db.Insert(
					SampleOrderLine.Create(Guid.NewGuid(), x, 1)));

				var rows = db.Select<SampleOrderLine>();
				Assert.That(rows, Has.Count.EqualTo(orderIds.Count));
			}
		}
	}
}