using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Microsoft.SqlServer.Types;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.SqlServer;

namespace ServiceStack.OrmLite.SqlServerTests.Spatials
{
    [TestFixture]
    public class HierarchyIdTests : SqlServerConvertersOrmLiteTestBase
    {
        [SetUp]
        public void Setup()
        {
            OpenDbConnection().CreateTable<HierarchyTestTable>(true);
        }

        // Avoid painful refactor to change all tests to use a using pattern
        private IDbConnection db;

        public override IDbConnection OpenDbConnection(string connString = null)
        {
            if (db != null && db.State != ConnectionState.Open)
                db = null;

            return db ?? (db = base.OpenDbConnection(connString));
        }

        [TearDown]
        public void TearDown()
        {
            if (db == null)
                return;
            db.Dispose();
            db = null;
        }
        [Test]
        public void Can_insert_and_retrieve_HierarchyId()
        {
            using (var db = OpenDbConnection())
            {
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
