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
        public void Can_delete_all_rows()
        {
            var row1 = ModelWithFieldsOfDifferentTypes.Create(1);
            var row2 = ModelWithFieldsOfDifferentTypes.Create(2);
            var row3 = ModelWithFieldsOfDifferentTypes.Create(3);

            db.Save(row1);
            db.Save(row2);
            db.Save(row3);

            db.DeleteAll(new[] { row1, row3 });

            var remaining = db.Select<ModelWithFieldsOfDifferentTypes>();

            Assert.That(remaining.Count, Is.EqualTo(1));
            Assert.That(remaining[0].Id, Is.EqualTo(row2.Id));
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

            db.Delete<ModelWithFieldsOfDifferentTypes>(x => x.Long <= row.Long);

            var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);

            Assert.That(dbRow, Is.Null);
        }

        [Test]
        public void Can_Delete_entity_with_nullable_DateTime()
        {
            db.DropAndCreateTable<ModelWithFieldsOfNullableTypes>();

            var row = ModelWithFieldsOfNullableTypes.Create(1);
            row.NDateTime = null;

            db.Save(row);

            row = db.SingleById<ModelWithFieldsOfNullableTypes>(row.Id);

            var rowsAffected = db.Delete(row);

            Assert.That(rowsAffected, Is.EqualTo(1));

            row = db.SingleById<ModelWithFieldsOfNullableTypes>(row.Id);
            Assert.That(row, Is.Null);
        }

        [Test]
        public void Can_DeleteAll_entity_with_nullable_DateTime()
        {
            db.DropAndCreateTable<ModelWithFieldsOfNullableTypes>();

            var rows = 3.Times(i => ModelWithFieldsOfNullableTypes.Create(i));
            rows.Each(x => x.NDateTime = null);

            db.SaveAll(rows);
            db.Save(ModelWithFieldsOfNullableTypes.Create(3)); // extra row shouldn't be deleted

            rows = db.SelectByIds<ModelWithFieldsOfNullableTypes>(rows.Map(x => x.Id));

            var rowsAffected = db.Delete(rows.ToArray());

            Assert.That(rowsAffected, Is.EqualTo(rows.Count));

            rows = db.SelectByIds<ModelWithFieldsOfNullableTypes>(rows.Map(x => x.Id));
            Assert.That(rows.Count, Is.EqualTo(0));
        }

        [Test]
        public void Can_DeleteNonDefaults_entity_with_nullable_DateTime()
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

        [Test]
        public void Can_DeleteNonDefaultsAll_entity_with_nullable_DateTime()
        {
            db.DropAndCreateTable<ModelWithFieldsOfNullableTypes>();

            var rows = 3.Times(i => ModelWithFieldsOfNullableTypes.Create(i));
            rows.Each(x => x.NDateTime = null);

            db.SaveAll(rows);
            db.Save(ModelWithFieldsOfNullableTypes.Create(3)); // extra row shouldn't be deleted

            rows = db.SelectByIds<ModelWithFieldsOfNullableTypes>(rows.Map(x => x.Id));

            var rowsAffected = db.DeleteNonDefaults(rows.ToArray());

            Assert.That(rowsAffected, Is.EqualTo(rows.Count));

            rows = db.SelectByIds<ModelWithFieldsOfNullableTypes>(rows.Map(x => x.Id));
            Assert.That(rows.Count, Is.EqualTo(0));
        }

    }
}