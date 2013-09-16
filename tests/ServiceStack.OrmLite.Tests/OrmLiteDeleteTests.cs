using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.OrmLite.Tests
{
	[TestFixture]
	public class OrmLiteDeleteTests
		: OrmLiteTestBase
	{
        private IDbConnection db;

        [SetUp]
        public void SetUp()
        {
            CreateNewDatabase();
            db = OpenDbConnection();
            db.DropAndCreateTable<ModelWithFieldsOfDifferentTypes>();
        }

        [TearDown]
        public void TearDown()
        {
            db.Dispose();
        }

		[Test]
		public void Can_Delete_from_ModelWithFieldsOfDifferentTypes_table()
		{
            var rowIds = new List<int>(new[] { 1, 2, 3 });
            rowIds.ForEach(x => db.Insert(ModelWithFieldsOfDifferentTypes.Create(x)));

            var rows = db.Select<ModelWithFieldsOfDifferentTypes>();
            var row2 = rows.First(x => x.Id == 2);

            db.Delete(row2);

            rows = db.GetByIds<ModelWithFieldsOfDifferentTypes>(rowIds);
            var dbRowIds = rows.ConvertAll(x => x.Id);

            Assert.That(dbRowIds, Is.EquivalentTo(new[] { 1, 3 }));
        }

		[Test]
		public void Can_DeleteById_from_ModelWithFieldsOfDifferentTypes_table()
		{
            var rowIds = new List<int>(new[] { 1, 2, 3 });
            rowIds.ForEach(x => db.Insert(ModelWithFieldsOfDifferentTypes.Create(x)));

            db.DeleteById<ModelWithFieldsOfDifferentTypes>(2);

            var rows = db.GetByIds<ModelWithFieldsOfDifferentTypes>(rowIds);
            var dbRowIds = rows.ConvertAll(x => x.Id);

            Assert.That(dbRowIds, Is.EquivalentTo(new[] { 1, 3 }));
        }

		[Test]
		public void Can_DeleteByIds_from_ModelWithFieldsOfDifferentTypes_table()
		{
            db.DropAndCreateTable<ModelWithFieldsOfDifferentTypes>();

            var rowIds = new List<int>(new[] { 1, 2, 3 });
            rowIds.ForEach(x => db.Insert(ModelWithFieldsOfDifferentTypes.Create(x)));

            db.DeleteByIds<ModelWithFieldsOfDifferentTypes>(new[] { 1, 3 });

            var rows = db.GetByIds<ModelWithFieldsOfDifferentTypes>(rowIds);
            var dbRowIds = rows.ConvertAll(x => x.Id);

            Assert.That(dbRowIds, Is.EquivalentTo(new[] { 2 }));
        }
        
        [Test]
        public void Can_delete_ModelWithFieldsOfDifferentTypes_table_with_filter()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            db.Insert(row);

            db.Delete<ModelWithFieldsOfDifferentTypes>(x => x.LongId <= row.LongId);

            var dbRow = db.GetByIdOrDefault<ModelWithFieldsOfDifferentTypes>(row.Id);

            Assert.That(dbRow, Is.Null);
        }

        [Test]
        public void Can_delete_with_optional_string_params()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            db.Insert(row);

            db.Delete<ModelWithFieldsOfDifferentTypes>(where: "LongId <= {0}".SqlFormat(row.LongId));

            var dbRow = db.GetByIdOrDefault<ModelWithFieldsOfDifferentTypes>(row.Id);

            Assert.That(dbRow, Is.Null);
        }

        [Test]
        public void Can_delete_with_tableName_and_optional_string_params()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            db.Insert(row);

            db.Delete(table: "ModelWithFieldsOfDifferentTypes", where: "LongId <= {0}".SqlFormat(row.LongId));

            var dbRow = db.GetByIdOrDefault<ModelWithFieldsOfDifferentTypes>(row.Id);

            Assert.That(dbRow, Is.Null);
        }

	}
}