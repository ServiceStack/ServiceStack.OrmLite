using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.OrmLite.Tests.Shared;
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

            row.Id = (int)db.Insert(row, selectIdentity: true);

            row.Name = "UpdatedName";

            db.Update(row);

            var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);

            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

        [Test]
        public void Can_update_ModelWithFieldsOfDifferentTypes_table_with_filter()
        {
            var row = CreateModelWithFieldsOfDifferentTypes();

            row.Id = (int)db.Insert(row, selectIdentity: true);

            row.Name = "UpdatedName";

            db.Update(row, x => x.LongId <= row.LongId);

            var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);

            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

        [Test]
        public void Can_update_with_anonymousType_and_expr_filter()
        {
            var row = CreateModelWithFieldsOfDifferentTypes();

            row.Id = (int)db.Insert(row, selectIdentity: true);
            row.DateTime = DateTime.Now;
            row.Name = "UpdatedName";

            db.Update<ModelWithFieldsOfDifferentTypes>(new { row.Name, row.DateTime },
                x => x.LongId >= row.LongId && x.LongId <= row.LongId);

            var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);
            Console.WriteLine(dbRow.Dump());
            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

        [Test]
        public void Can_update_with_optional_string_params()
        {
            var row = CreateModelWithFieldsOfDifferentTypes();

            row.Id = (int)db.Insert(row, selectIdentity: true);
            row.Name = "UpdatedName";

            db.UpdateFmt<ModelWithFieldsOfDifferentTypes>(set: "NAME = {0}".SqlFmt(row.Name), where: "LongId".SqlColumn() + " <= {0}".SqlFmt(row.LongId));

            var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);
            Console.WriteLine(dbRow.Dump());
            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

        [Test]
        public void Can_update_with_tableName_and_optional_string_params()
        {
            var row = CreateModelWithFieldsOfDifferentTypes();

            row.Id = (int)db.Insert(row, selectIdentity: true);
            row.Name = "UpdatedName";

            db.UpdateFmt(table: "ModelWithFieldsOfDifferentTypes".SqlTableRaw(),
                set: "NAME = {0}".SqlFmt(row.Name), where: "LongId".SqlColumn() + " <= {0}".SqlFmt(row.LongId));

            var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);
            Console.WriteLine(dbRow.Dump());
            ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
        }

        [Test]
        public void Can_Update_Into_Table_With_Id_Only()
        {
            db.CreateTable<ModelWithIdOnly>(true);
            var row1 = new ModelWithIdOnly(1);
            db.Insert(row1);

            db.Update(row1);
        }

        [Test]
        public void Can_Update_Many_Into_Table_With_Id_Only()
        {
            db.CreateTable<ModelWithIdOnly>(true);
            var row1 = new ModelWithIdOnly(1);
            var row2 = new ModelWithIdOnly(2);
            db.Insert(row1, row2);

            db.Update(row1, row2);

            var list = new List<ModelWithIdOnly> { row1, row2 };
            db.UpdateAll(list);
        }

        [Test]
        public void Can_UpdateOnly_multiple_columns()
        {
            db.DropAndCreateTable<Person>();

            db.Insert(new Person { FirstName = "FirstName", Age = 100 });

            var existingPerson = db.Select<Person>().First();

            existingPerson.FirstName = "JJ";
            existingPerson.Age = 12;

            db.UpdateOnly(existingPerson,
                onlyFields: p => new { p.FirstName, p.Age });

            var person = db.Select<Person>().First();

            Assert.That(person.FirstName, Is.EqualTo("JJ"));
            Assert.That(person.Age, Is.EqualTo(12));
        }
    }
}