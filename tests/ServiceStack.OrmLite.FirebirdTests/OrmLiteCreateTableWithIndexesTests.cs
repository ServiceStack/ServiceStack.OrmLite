using System;
using NUnit.Framework;
using ServiceStack.Common.Extensions;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.OrmLite.FirebirdTests
{
	[TestFixture]
	public class OrmLiteCreateTableWithIndexesTests 
		: OrmLiteTestBase
	{

		[Test]
		public void Can_create_ModelWithIndexFields_table()
		{
			OrmLiteConfig.DialectProvider.DefaultStringLength=128;
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbConn = db.CreateCommand())
			{
				dbConn.CreateTable<ModelWithIndexFields>(true);

				var sql = OrmLiteConfig.DialectProvider.ToCreateIndexStatements( typeof (ModelWithIndexFields)).Join();

				Assert.IsTrue(sql.Contains("idx_modelwif_name"));
				Assert.IsTrue(sql.Contains("uidx_modelwif_uniquename"));
			}
		}

		[Test]
		public void Can_create_ModelWithCompositeIndexFields_table()
		{
			OrmLiteConfig.DialectProvider.DefaultStringLength=128;
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbConn = db.CreateCommand())
			{
				dbConn.CreateTable<ModelWithCompositeIndexFields>(true);

				var sql = OrmLiteConfig.DialectProvider.ToCreateIndexStatements(typeof(ModelWithCompositeIndexFields)).Join();

				Assert.IsTrue(sql.Contains("idx_modelwcif_name"));
				Assert.IsTrue(sql.Contains("idx_modelwcif_comp1_comp2"));
			}
		}


	}
}