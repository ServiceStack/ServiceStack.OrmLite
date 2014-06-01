using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Tests
{
    public class ModelWithRowVersion
    {
        [AutoIncrement]
        public long Id { get; set; }

        public string Text { get; set; }

        public ulong RowVersion { get; set; }
    }

    public class RowVersionTests : OrmLiteTestBase
    {
        public RowVersionTests()
        {
            //Dialect = Dialect.Sqlite;
        }

        private IDbConnection db;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            using (var dbConn = OpenDbConnection())
            {
                dbConn.DropAndCreateTable<ModelWithRowVersion>();
            }
        }

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

        [Test]
        public void Can_create_table_with_RowVersion()
        {
            db.DropAndCreateTable<ModelWithRowVersion>();

            db.Insert(new ModelWithRowVersion { Text = "Text" });

            var row = db.SingleById<ModelWithRowVersion>(1);

            row.Text += " Updated";

            db.Update(row);

            var updatedRow = db.SingleById<ModelWithRowVersion>(1);

            Assert.That(updatedRow.Text, Is.EqualTo("Text Updated"));
            Assert.That(updatedRow.RowVersion, Is.GreaterThan(0));

            row.Text += " Again";

            //Can't update old record
            Assert.Throws<RowModifiedException>(() =>
                db.Update(row));

            //Can update latest version
            updatedRow.Text += " Again";
            db.Update(updatedRow);
        }

        [Test]
        public void Select_retrieves_rowversion()
        {
            var rowId = db.Insert(new ModelWithRowVersion { Text = "One" }, selectIdentity: true);
            TouchRow(rowId);

            var row = db.SingleById<ModelWithRowVersion>(rowId);

            Assert.That(row.RowVersion, Is.Not.EqualTo(0));
        }

        [Test]
        public void Can_update_with_current_rowversion()
        {
            var rowId = db.Insert(new ModelWithRowVersion { Text = "Two" }, selectIdentity: true);
            var row = db.SingleById<ModelWithRowVersion>(rowId);

            row.Text = "Three";
            db.Update(row);

            var actual = db.SingleById<ModelWithRowVersion>(rowId);
            Assert.That(actual.Text, Is.EqualTo("Three"));
            Assert.That(actual.RowVersion, Is.Not.EqualTo(row.RowVersion));
        }

        [Test]
        public void Can_update_multiple_with_current_rowversions()
        {
            var rowIds = new[]
            {
                db.Insert(new ModelWithRowVersion { Text = "Eleven" }, selectIdentity: true),
                db.Insert(new ModelWithRowVersion { Text = "Twelve" }, selectIdentity: true)
            };
            var rows = rowIds
                .Select(id => db.SingleById<ModelWithRowVersion>(id))
                .ToArray();

            rows[0].Text = "Thirteen";
            rows[1].Text = "Fourteen";
            db.UpdateAll(rows);

            var actualRows = rowIds
                .Select(id => db.SingleById<ModelWithRowVersion>(id))
                .ToArray();
            Assert.That(actualRows[0].Text, Is.EqualTo("Thirteen"));
            Assert.That(actualRows[1].Text, Is.EqualTo("Fourteen"));
        }

        [Test]
        public void Can_delete_with_current_rowversion()
        {
            var rowId = db.Insert(new ModelWithRowVersion { Text = "Four" }, selectIdentity: true);
            var row = db.SingleById<ModelWithRowVersion>(rowId);

            db.Delete(row);

            var count = db.Count<ModelWithRowVersion>(m => m.Id == rowId);
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void Update_with_outdated_rowversion_throws()
        {
            var rowId = db.Insert(new ModelWithRowVersion { Text = "Five" }, selectIdentity: true);
            var row = db.SingleById<ModelWithRowVersion>(rowId);
            TouchRow(rowId);

            row.Text = "Six";
            Assert.Throws<RowModifiedException>(() => db.Update(row));

            var actual = db.SingleById<ModelWithRowVersion>(rowId);
            Assert.That(actual.Text, Is.Not.EqualTo("Six"));
        }

        [Test]
        public void Update_multiple_with_single_outdated_rowversion_throws_and_all_changes_are_rejected()
        {
            var rowIds = new[]
            {
                db.Insert(new ModelWithRowVersion { Text = "Fifteen" }, selectIdentity: true),
                db.Insert(new ModelWithRowVersion { Text = "Sixteen" }, selectIdentity: true)
            };
            var rows = rowIds
                .Select(id => db.SingleById<ModelWithRowVersion>(id))
                .ToArray();
            TouchRow(rowIds[1]);

            rows[0].Text = "Seventeen";
            rows[1].Text = "Eighteen";
            Assert.Throws<RowModifiedException>(() => 
                db.UpdateAll(rows));

            var actualRows = rowIds
                .Select(id => db.SingleById<ModelWithRowVersion>(id))
                .ToArray();

            Assert.That(actualRows[0].Text, Is.EqualTo("Fifteen"));
            Assert.That(actualRows[1].Text, Is.Not.EqualTo("Eighteen"));
        }

        [Test]
        public void Delete_with_outdated_rowversion_throws()
        {
            var rowId = db.Insert(new ModelWithRowVersion { Text = "Seven" }, selectIdentity: true);
            var row = db.SingleById<ModelWithRowVersion>(rowId);
            TouchRow(rowId);

            Assert.Throws<RowModifiedException>(() => 
                db.Delete(row));

            var count = db.Count<ModelWithRowVersion>(m => m.Id == rowId);
            Assert.That(count, Is.EqualTo(1));
        }

        private void TouchRow(long rowId)
        {
            var row = db.SingleById<ModelWithRowVersion>(rowId);
            row.Text = "Touched";
            db.Update(row);
        }
    }
}