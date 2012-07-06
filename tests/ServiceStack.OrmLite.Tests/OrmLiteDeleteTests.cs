using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
	[TestFixture]
	public class OrmLiteDeleteTests
		: OrmLiteTestBase
	{
        private IDbConnection dbConn;
        private IDbCommand dbCmd;

        [SetUp]
        public void SetUp()
        {
            CreateNewDatabase();
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
		public void Can_Delete_from_ModelWithFieldsOfDifferentTypes_table()
		{
            var rowIds = new List<int>(new[] { 1, 2, 3 });
            rowIds.ForEach(x => dbCmd.Insert(ModelWithFieldsOfDifferentTypes.Create(x)));

            var rows = dbCmd.Select<ModelWithFieldsOfDifferentTypes>();
            var row2 = rows.First(x => x.Id == 2);

            dbCmd.Delete(row2);

            rows = dbCmd.GetByIds<ModelWithFieldsOfDifferentTypes>(rowIds);
            var dbRowIds = rows.ConvertAll(x => x.Id);

            Assert.That(dbRowIds, Is.EquivalentTo(new[] { 1, 3 }));
        }

		[Test]
		public void Can_DeleteById_from_ModelWithFieldsOfDifferentTypes_table()
		{
            var rowIds = new List<int>(new[] { 1, 2, 3 });
            rowIds.ForEach(x => dbCmd.Insert(ModelWithFieldsOfDifferentTypes.Create(x)));

            dbCmd.DeleteById<ModelWithFieldsOfDifferentTypes>(2);

            var rows = dbCmd.GetByIds<ModelWithFieldsOfDifferentTypes>(rowIds);
            var dbRowIds = rows.ConvertAll(x => x.Id);

            Assert.That(dbRowIds, Is.EquivalentTo(new[] { 1, 3 }));
        }

		[Test]
		public void Can_DeleteByIds_from_ModelWithFieldsOfDifferentTypes_table()
		{
            dbCmd.CreateTable<ModelWithFieldsOfDifferentTypes>(true);

            var rowIds = new List<int>(new[] { 1, 2, 3 });
            rowIds.ForEach(x => dbCmd.Insert(ModelWithFieldsOfDifferentTypes.Create(x)));

            dbCmd.DeleteByIds<ModelWithFieldsOfDifferentTypes>(new[] { 1, 3 });

            var rows = dbCmd.GetByIds<ModelWithFieldsOfDifferentTypes>(rowIds);
            var dbRowIds = rows.ConvertAll(x => x.Id);

            Assert.That(dbRowIds, Is.EquivalentTo(new[] { 2 }));
        }
        
        [Test]
        public void Can_delete_ModelWithFieldsOfDifferentTypes_table_with_filter()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            dbCmd.Insert(row);

            dbCmd.Delete<ModelWithFieldsOfDifferentTypes>(x => x.LongId <= row.LongId);

            var dbRow = dbCmd.GetByIdOrDefault<ModelWithFieldsOfDifferentTypes>(row.Id);

            Assert.That(dbRow, Is.Null);
        }

        [Test]
        public void Can_delete_with_optional_string_params()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            dbCmd.Insert(row);

            dbCmd.Delete<ModelWithFieldsOfDifferentTypes>(where: "LongId <= {0}".SqlFormat(row.LongId));

            var dbRow = dbCmd.GetByIdOrDefault<ModelWithFieldsOfDifferentTypes>(row.Id);

            Assert.That(dbRow, Is.Null);
        }

        [Test]
        public void Can_delete_with_tableName_and_optional_string_params()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            dbCmd.Insert(row);

            dbCmd.Delete(table: "ModelWithFieldsOfDifferentTypes", where: "LongId <= {0}".SqlFormat(row.LongId));

            var dbRow = dbCmd.GetByIdOrDefault<ModelWithFieldsOfDifferentTypes>(row.Id);

            Assert.That(dbRow, Is.Null);
        }

	}
}