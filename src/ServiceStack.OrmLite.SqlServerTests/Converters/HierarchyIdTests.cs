using System;
using System.Linq;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServerTests.Converters
{
    [TestFixture]
    public class HierarchyIdTests : SqlServerConvertersOrmLiteTestBase
    {
        [Test]
        public void Can_insert_and_retrieve_HierarchyId()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<HierarchyTestTable>();

                var stringValue = "/1/1/3/";  // 0x5ADE is hex

                var treeId = SqlHierarchyId.Parse(stringValue);

                db.Insert(new HierarchyTestTable() { TreeId = treeId, NullTreeId = SqlHierarchyId.Null });

                var result = db.Select<HierarchyTestTable>();
                Assert.AreEqual(null, result[0].NullTreeId);
                Assert.AreEqual(treeId, result[0].TreeId);

                var parent = db.Column<SqlHierarchyId>("SELECT TreeId.GetAncestor(1) from HierarchyTestTable").First();
                var str = parent.ToString();
                Assert.AreEqual("/1/1/", str);
            }
        }
    }

    public class HierarchyTestTable
    {
        [AutoIncrement]
        public long Id { get; set; }

        public SqlHierarchyId TreeId { get; set; }

        public Nullable<SqlHierarchyId> NullTreeId { get; set; }
    }
}
