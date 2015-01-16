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

            for (var i = 0; i < rowIds.Count; i++)
                rowIds[i] = (int)db.Insert(ModelWithFieldsOfDifferentTypes.Create(rowIds[i]), selectIdentity: true);

            var rows = db.Select<ModelWithFieldsOfDifferentTypes>();

            var row2 = rows.First(x => x.Id == rowIds[1]);

            db.Delete(row2);

            rows = db.SelectByIds<ModelWithFieldsOfDifferentTypes>(rowIds);
            var dbRowIds = rows.ConvertAll(x => x.Id);

            Assert.That(dbRowIds, Is.EquivalentTo(new[] { rowIds[0], rowIds[2] }));
        }

		[Test]
		public void Can_DeleteById_from_ModelWithFieldsOfDifferentTypes_table()
		{
            var rowIds = new List<int>(new[] { 1, 2, 3 });

            for (var i = 0; i < rowIds.Count; i++)
                rowIds[i] = (int)db.Insert(ModelWithFieldsOfDifferentTypes.Create(rowIds[i]), selectIdentity: true);

            db.DeleteById<ModelWithFieldsOfDifferentTypes>(rowIds[1]);

            var rows = db.SelectByIds<ModelWithFieldsOfDifferentTypes>(rowIds);
            var dbRowIds = rows.ConvertAll(x => x.Id);

            Assert.That(dbRowIds, Is.EquivalentTo(new[] { rowIds[0], rowIds[2] }));
        }

		[Test]
		public void Can_DeleteByIds_from_ModelWithFieldsOfDifferentTypes_table()
		{
            db.DropAndCreateTable<ModelWithFieldsOfDifferentTypes>();

            var rowIds = new List<int>(new[] { 1, 2, 3 });

            for (var i = 0; i < rowIds.Count; i++)
                rowIds[i] = (int)db.Insert(ModelWithFieldsOfDifferentTypes.Create(rowIds[i]), selectIdentity: true);

            db.DeleteByIds<ModelWithFieldsOfDifferentTypes>(new[] { rowIds[0], rowIds[2] });

            var rows = db.SelectByIds<ModelWithFieldsOfDifferentTypes>(rowIds);
            var dbRowIds = rows.ConvertAll(x => x.Id);

            Assert.That(dbRowIds, Is.EquivalentTo(new[] { rowIds[1] }));
        }
        
        [Test]
        public void Can_delete_ModelWithFieldsOfDifferentTypes_table_with_filter()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            db.Insert(row);

            db.Delete<ModelWithFieldsOfDifferentTypes>(x => x.LongId <= row.LongId);

            var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);

            Assert.That(dbRow, Is.Null);
        }

        [Test]
        public void Can_delete_with_optional_string_params()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            db.Insert(row);

            db.DeleteFmt<ModelWithFieldsOfDifferentTypes>(where: "LongId".SqlColumn() + " <= {0}".SqlFmt(row.LongId));

            var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);

            Assert.That(dbRow, Is.Null);
        }

        [Test]
        public void Can_delete_with_tableName_and_optional_string_params()
        {
            var row = ModelWithFieldsOfDifferentTypes.Create(1);

            db.Insert(row);

            db.DeleteFmt(table: "ModelWithFieldsOfDifferentTypes", where: "LongId".SqlColumn() + " <= {0}".SqlFmt(row.LongId));

            var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);

            Assert.That(dbRow, Is.Null);
        }

	    [Test]
	    public void Can_delete_entity_with_nullable_DateTime()
	    {
            db.DropAndCreateTable<ModelWithFieldsOfNullableTypes>();

            var row = ModelWithFieldsOfNullableTypes.Create(1);
            row.NDateTime = null;

            db.Save(row);

            row = db.SingleById<ModelWithFieldsOfNullableTypes>(row.Id);

            var rowsAffected = db.DeleteNonDefaults(row);

            Assert.That(rowsAffected, Is.EqualTo(1));

            row = db.SingleById<ModelWithFieldsOfNullableTypes>(row.Id);
            Assert.That(row, Is.Null);
        }

	}
}