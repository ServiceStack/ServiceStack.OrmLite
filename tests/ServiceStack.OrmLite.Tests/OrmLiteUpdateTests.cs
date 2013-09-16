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
        private IDbConnection db;

        [SetUp]
        public void SetUp()
        {
            db = OpenDbConnection();
        }

        [TearDown]
        public void TearDown()
        {
            db.Dispose();
        }

        private ModelWithFieldsOfDifferentTypes CreateModelWithFieldsOfDifferentTypes()
        {
            db.DropAndCreateTable<ModelWithFieldsOfDifferentTypes>();

            var row = ModelWithFieldsOfDifferentTypes.Create(1);
            return row;
        }

        [Test]
        public void Can_update_ModelWithFieldsOfDifferentTypes_table()
        {
            var row = CreateModelWithFieldsOfDifferentTypes();

            db.Insert(row);

            row.Name = "UpdatedName";

            db.Update(row);

            var dbRow = db.GetById<ModelWithFieldsOfDifferentTypes>(1);

            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

        [Test]
        public void Can_update_ModelWithFieldsOfDifferentTypes_table_with_filter()
        {
            var row = CreateModelWithFieldsOfDifferentTypes();

            db.Insert(row);

            row.Name = "UpdatedName";

            db.Update(row, x => x.LongId <= row.LongId);

            var dbRow = db.GetById<ModelWithFieldsOfDifferentTypes>(1);

            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

        [Test]
        public void Can_update_with_anonymousType_and_expr_filter()
        {
            var row = CreateModelWithFieldsOfDifferentTypes();

            db.Insert(row);
            row.DateTime = DateTime.Now;
            row.Name = "UpdatedName";

            db.Update<ModelWithFieldsOfDifferentTypes>(new { row.Name, row.DateTime },
                x => x.LongId >= row.LongId && x.LongId <= row.LongId);

            var dbRow = db.GetById<ModelWithFieldsOfDifferentTypes>(row.Id);
            Console.WriteLine(dbRow.Dump());
            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

        [Test]
        public void Can_update_with_optional_string_params()
        {
            var row = CreateModelWithFieldsOfDifferentTypes();

            db.Insert(row);
            row.Name = "UpdatedName";

            db.Update<ModelWithFieldsOfDifferentTypes>(set: "NAME = {0}".SqlFormat(row.Name), where: "LongId <= {0}".SqlFormat(row.LongId));

            var dbRow = db.GetById<ModelWithFieldsOfDifferentTypes>(row.Id);
            Console.WriteLine(dbRow.Dump());
            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

        [Test]
        public void Can_update_with_tableName_and_optional_string_params()
        {
            var row = CreateModelWithFieldsOfDifferentTypes();

            db.Insert(row);
            row.Name = "UpdatedName";

            db.Update(table: "ModelWithFieldsOfDifferentTypes",
                set: "NAME = {0}".SqlFormat(row.Name), where: "LongId <= {0}".SqlFormat(row.LongId));

            var dbRow = db.GetById<ModelWithFieldsOfDifferentTypes>(row.Id);
            Console.WriteLine(dbRow.Dump());
            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

    }
}