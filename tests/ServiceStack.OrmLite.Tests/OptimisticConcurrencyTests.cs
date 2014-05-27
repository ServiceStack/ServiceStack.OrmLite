using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class OptimisticConcurrencyTests 
        : OrmLiteTestBase
    {
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
        public void Select_retrieves_rowversion()
        {
            var rowId = db.Insert(new ModelWithRowVersion { Text = "One" }, selectIdentity:true);
            TouchRow(rowId);

            var row = db.SingleById<ModelWithRowVersion>(rowId);

            Assert.That(row.Version, Is.Not.EqualTo(0));
        }

        [Test]
        public void Can_Save_new_row_and_retrieve_rowversion()
        {
            var row = new ModelWithRowVersion { Text = "First" };

            bool wasInserted = db.Save(row);

            Assert.That(wasInserted, Is.True);
            var actualRow = db.SingleById<ModelWithRowVersion>(row.Id);
            Assert.That(row.Version, Is.EqualTo(actualRow.Version));
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
            Assert.That(actual.Version, Is.Not.EqualTo(row.Version));
        }

        [Test]
        public void Can_Save_changed_row_with_current_rowversion_and_retrieve_rowversion()
        {
            var rowId = db.Insert(new ModelWithRowVersion { Text = "Second" }, selectIdentity: true);
            var row = db.SingleById<ModelWithRowVersion>(rowId);

            row.Text = "Third";
            bool wasInserted = db.Save(row);

            Assert.That(wasInserted, Is.False);
            var actualRow = db.SingleById<ModelWithRowVersion>(rowId);
            Assert.That(row.Version, Is.EqualTo(actualRow.Version));
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
        public void Save_changed_row_with_outdated_rowversion_throws()
        {
            var rowId = db.Insert(new ModelWithRowVersion { Text = "Fourth" }, selectIdentity: true);
            var row = db.SingleById<ModelWithRowVersion>(rowId); 
            TouchRow(rowId);

            row.Text = "Fifth";
            Assert.Throws<RowModifiedException>(() => db.Save(row));

            var actualRow = db.SingleById<ModelWithRowVersion>(rowId);
            Assert.That(actualRow.Text, Is.Not.EqualTo("Fourth"));
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
            Assert.Throws<RowModifiedException>(() => db.UpdateAll(rows));

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

            Assert.Throws<RowModifiedException>(() => db.Delete(row));

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

    public class ModelWithRowVersion
    {
        [AutoIncrement]
        public long Id { get; set; }

        public string Text { get; set; }

        [RowVersion]
        public long Version { get; set; }
    }
}
