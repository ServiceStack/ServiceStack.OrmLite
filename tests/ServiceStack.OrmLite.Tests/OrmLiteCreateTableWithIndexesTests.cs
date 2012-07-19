using NUnit.Framework;
using ServiceStack.Common.Extensions;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.OrmLite.Tests
{
	[TestFixture]
	public class OrmLiteCreateTableWithIndexesTests 
		: OrmLiteTestBase
	{

		[Test]
		public void Can_create_ModelWithIndexFields_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			{
				db.CreateTable<ModelWithIndexFields>(true);

				var sql =OrmLiteConfig.DialectProvider.ToCreateIndexStatements( typeof (ModelWithIndexFields) ).Join();

				Assert.IsTrue(sql.Contains("idx_modelwithindexfields_name"));
				Assert.IsTrue(sql.Contains("uidx_modelwithindexfields_uniquename"));
			}
		}

		[Test]
		public void Can_create_ModelWithCompositeIndexFields_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			{
				db.CreateTable<ModelWithCompositeIndexFields>(true);

				var sql = OrmLiteConfig.DialectProvider.ToCreateIndexStatements(typeof(ModelWithCompositeIndexFields)).Join();

				Assert.IsTrue(sql.Contains("idx_modelwithcompositeindexfields_name"));
				Assert.IsTrue(sql.Contains("idx_modelwithcompositeindexfields_composite1_composite2"));
			}
		}


	}
}