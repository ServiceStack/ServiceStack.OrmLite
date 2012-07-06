using System;
using System.Data;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class OrmLiteUpdateTests
        : OrmLiteTestBase
    {
        private IDbConnection dbConn;
        private IDbCommand dbCmd;

        [SetUp]
        public void SetUp()
        {
            dbConn = ConnectionString.OpenDbConnection();
            dbCmd = dbConn.CreateCommand();
            dbCmd.CreateTable<ModelWithFieldsOfDifferentTypes>(true);
        }

        [TearDown]
        public void TearDown()
        {
            dbCmd.Dispose();
            dbConn.Dispose();
        }

        [Test]
        public void Can_update_ModelWithFieldsOfDifferentTypes_table()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            dbCmd.Insert(row);

            row.Name = "UpdatedName";

            dbCmd.Update(row);

            var dbRow = dbCmd.GetById<ModelWithFieldsOfDifferentTypes>(1);

            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

        [Test]
        public void Can_update_ModelWithFieldsOfDifferentTypes_table_with_filter()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            dbCmd.Insert(row);

            row.Name = "UpdatedName";

            dbCmd.Update(row, x => x.LongId <= row.LongId);

            var dbRow = dbCmd.GetById<ModelWithFieldsOfDifferentTypes>(1);

            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

        [Test]
        public void Can_update_with_anonymousType_and_expr_filter()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            dbCmd.Insert(row);
            row.DateTime = DateTime.Now;
            row.Name = "UpdatedName";

            dbCmd.Update<ModelWithFieldsOfDifferentTypes>(new { row.Name, row.DateTime }, 
                x => x.LongId >= row.LongId && x.LongId <= row.LongId);

            var dbRow = dbCmd.GetById<ModelWithFieldsOfDifferentTypes>(row.Id);
            Console.WriteLine(dbRow.Dump());
            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

        [Test]
        public void Can_update_with_optional_string_params()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            dbCmd.Insert(row);
            row.Name = "UpdatedName";

            dbCmd.Update<ModelWithFieldsOfDifferentTypes>(set: "NAME = {0}".SqlFormat(row.Name), where: "LongId <= {0}".SqlFormat(row.LongId));

            var dbRow = dbCmd.GetById<ModelWithFieldsOfDifferentTypes>(row.Id);
            Console.WriteLine(dbRow.Dump());
            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

        [Test]
        public void Can_update_with_tableName_and_optional_string_params()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            dbCmd.Insert(row);
            row.Name = "UpdatedName";

            dbCmd.Update(table: "ModelWithFieldsOfDifferentTypes", 
                set: "NAME = {0}".SqlFormat(row.Name), where: "LongId <= {0}".SqlFormat(row.LongId));

            var dbRow = dbCmd.GetById<ModelWithFieldsOfDifferentTypes>(row.Id);
            Console.WriteLine(dbRow.Dump());
            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

    }
}