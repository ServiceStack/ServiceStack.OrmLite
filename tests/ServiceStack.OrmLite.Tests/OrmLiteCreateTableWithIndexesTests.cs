using NUnit.Framework;
using ServiceStack.Text;
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
            using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIndexFields>(true);

				var sql = OrmLiteConfig.DialectProvider.ToCreateIndexStatements( typeof (ModelWithIndexFields) ).Join();

                SuppressIfOracle("Assert comparisons don't work with Oracle provider because it had to squash names to satisfy length restrictions");

                Assert.IsTrue(sql.Contains("idx_modelwithindexfields_name"));
				Assert.IsTrue(sql.Contains("uidx_modelwithindexfields_uniquename"));
			}
		}

		[Test]
		public void Can_create_ModelWithCompositeIndexFields_table()
		{
            using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithCompositeIndexFields>(true);

				var sql = OrmLiteConfig.DialectProvider.ToCreateIndexStatements(typeof(ModelWithCompositeIndexFields)).Join();

                SuppressIfOracle("Assert comparisons don't work with Oracle provider because it had to squash names to satisfy length restrictions");

                Assert.IsTrue(sql.Contains("idx_modelwithcompositeindexfields_name"));
				Assert.IsTrue(sql.Contains("idx_modelwithcompositeindexfields_composite1_composite2"));
			}
		}

        [Test]
        public void Can_create_ModelWithNamedCompositeIndex_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithNamedCompositeIndex>(true);

                var sql = OrmLiteConfig.DialectProvider.ToCreateIndexStatements(typeof(ModelWithNamedCompositeIndex)).Join();

                SuppressIfOracle("Assert comparisons don't work with Oracle provider because it had to squash names to satisfy length restrictions");

                Assert.IsTrue(sql.Contains("idx_modelwithnamedcompositeindex_name"));
                Assert.IsTrue(sql.Contains("custom_index_name"));
                Assert.IsFalse(sql.Contains("uidx_modelwithnamedcompositeindexfields_composite1_composite2"));
            }
        }

	}
}