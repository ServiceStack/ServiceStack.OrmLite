using System;
using System.Collections.Generic;
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

    public class ModelWithRowVersionAlias
    {
        [AutoIncrement]
        public long Id { get; set; }

        public string Text { get; set; }

        [Alias("VersionAlias")]
        public ulong RowVersion { get; set; }
    }


    [Alias("TheModelWithAliasedRowVersion")]
    public class ModelWithAliasedRowVersion
    {
        [AutoIncrement]
        public long Id { get; set; }

        public string Text { get; set; }

        [RowVersion]
        [Alias("TheVersion")]
        public long Version { get; set; }
    }

    public class ModelWithOptimisticChildren
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Text { get; set; }

        [Reference]
        public List<ModelWithRowVersionAndParent> Children { get; set; }
    }

    public class ModelWithRowVersionAndParent
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Reference]
        public int ModelWithOptimisticChildrenId { get; set; }

        public string Text { get; set; }

        [RowVersion]
        public long Version { get; set; }
    }

    public class RowVersionTests : OrmLiteTestBase
    {
        private IDbConnection db;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            //Dialect = Dialect.Sqlite;
            LogManager.LogFactory = new ConsoleLogFactory(debugEnabled: true);
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
        public void Can_create_table_with_RowVersion_Alias()
        {
            db.DropAndCreateTable<ModelWithRowVersionAlias>();

            db.Insert(new ModelWithRowVersionAlias { Text = "Text" });

            var row = db.SingleById<ModelWithRowVersionAlias>(1);

            row.Text += " Updated";

            db.Update(row);

            var updatedRow = db.SingleById<ModelWithRowVersionAlias>(1);

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
        public void SingleById_retrieves_rowversion()
        {
            var rowId = db.Insert(new ModelWithRowVersion { Text = "One" }, selectIdentity: true);
            TouchRow(rowId);

            var row = db.SingleById<ModelWithRowVersion>(rowId);

            Assert.That(row.RowVersion, Is.Not.EqualTo(0));
        }

        [Test]
        public void Select_retrieves_rowversion()
        {
            var rowId = db.Insert(new ModelWithRowVersion { Text = "OnePointOne" }, selectIdentity: true);
            TouchRow(rowId);

            var rows = db.Select<ModelWithRowVersion>(x => x.Id == rowId);

            Assert.That(rows.Single().RowVersion, Is.Not.EqualTo(0));
        }

        [Test]
        public void SingleById_with_aliases_retrieves_rowversion()
        {
            db.DropAndCreateTable<ModelWithAliasedRowVersion>();

            var row = new ModelWithAliasedRowVersion { Text = "TheOne" };
            db.Save(row);

            var actualRow = db.SingleById<ModelWithAliasedRowVersion>(row.Id);

            Assert.That(actualRow.Version, Is.EqualTo(row.Version));
        }

        [Test]
        public void Can_Save_new_row_and_retrieve_rowversion()
        {
            var row = new ModelWithRowVersion { Text = "First" };

            bool wasInserted = db.Save(row);

            Assert.That(wasInserted, Is.True);
            var actualRow = db.SingleById<ModelWithRowVersion>(row.Id);
            Assert.That(row.RowVersion, Is.EqualTo(actualRow.RowVersion));
        }

        [Test]
        public void Can_SaveAll_new_rows_and_retrieve_rowversion()
        {
            var rows = new[]
            {
                new ModelWithRowVersion {Text = "Eleventh"},
                new ModelWithRowVersion {Text = "Twelfth"}
            };

            var insertedCount = db.SaveAll(rows);

            Assert.That(insertedCount, Is.EqualTo(2));
            var actualRows = db.SelectByIds<ModelWithRowVersion>(rows.Select(x => x.Id));
            Assert.That(rows[0].RowVersion, Is.EqualTo(actualRows[0].RowVersion));
            Assert.That(rows[1].RowVersion, Is.EqualTo(actualRows[1].RowVersion));
        }

        [Test]
        public void Can_Save_new_row_with_references_and_retrieve_child_rowversions()
        {
            db.DropAndCreateTable<ModelWithOptimisticChildren>();
            db.DropAndCreateTable<ModelWithRowVersionAndParent>();

            var row = new ModelWithOptimisticChildren
            {
                Text = "Twentyfirst",
                Children = new List<ModelWithRowVersionAndParent> {
                    new ModelWithRowVersionAndParent { Text = "Twentysecond" }
                }
            };

            db.Save(row, references: true);

            var actualChildRow = db.SingleById<ModelWithRowVersionAndParent>(row.Children[0].Id);
            Assert.That(row.Children[0].Version, Is.EqualTo(actualChildRow.Version));
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
        public void Can_Save_changed_row_with_current_rowversion_and_retrieve_rowversion()
        {
            var rowId = db.Insert(new ModelWithRowVersion { Text = "Second" }, selectIdentity: true);
            var row = db.SingleById<ModelWithRowVersion>(rowId);

            row.Text = "Third";
            bool wasInserted = db.Save(row);

            Assert.That(wasInserted, Is.False);
            var actualRow = db.SingleById<ModelWithRowVersion>(rowId);
            Assert.That(row.RowVersion, Is.EqualTo(actualRow.RowVersion));
        }

        [Test]
        public void Can_UpdateAll_with_current_rowversions()
        {
            var rowIds = new[]
            {
                db.Insert(new ModelWithRowVersion { Text = "Eleven" }, selectIdentity: true),
                db.Insert(new ModelWithRowVersion { Text = "Twelve" }, selectIdentity: true)
            };
            var rows = db.SelectByIds<ModelWithRowVersion>(rowIds);

            rows[0].Text = "Thirteen";
            rows[1].Text = "Fourteen";
            db.UpdateAll(rows);

            var actualRows = db.SelectByIds<ModelWithRowVersion>(rowIds);
            Assert.That(actualRows[0].Text, Is.EqualTo("Thirteen"));
            Assert.That(actualRows[1].Text, Is.EqualTo("Fourteen"));
        }

        [Test]
        public void Can_SaveAll_changed_rows_with_current_rowversion_and_retrieve_rowversion()
        {
            var rowIds = new[]
            {
                db.Insert(new ModelWithRowVersion { Text = "Thirteenth" }, selectIdentity: true),
                db.Insert(new ModelWithRowVersion { Text = "Fourteenth" }, selectIdentity: true)
            };
            var rows = db.SelectByIds<ModelWithRowVersion>(rowIds);

            rows[0].Text = "Fifteenth";
            rows[1].Text = "Sixteenth";
            var insertedCount = db.SaveAll(rows);

            Assert.That(insertedCount, Is.EqualTo(0));
            var actualRows = db.SelectByIds<ModelWithRowVersion>(rows.Select(x => x.Id));
            Assert.That(actualRows[0].Text, Is.EqualTo("Fifteenth"));
            Assert.That(rows[0].RowVersion, Is.EqualTo(actualRows[0].RowVersion));
            Assert.That(rows[1].RowVersion, Is.EqualTo(actualRows[1].RowVersion));
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
        public void UpdateAll_with_single_outdated_rowversion_throws_and_all_changes_are_rejected()
        {
            var rowIds = new[]
            {
                db.Insert(new ModelWithRowVersion { Text = "Fifteen" }, selectIdentity: true),
                db.Insert(new ModelWithRowVersion { Text = "Sixteen" }, selectIdentity: true)
            };
            var rows = db.SelectByIds<ModelWithRowVersion>(rowIds);
            TouchRow(rowIds[1]);

            rows[0].Text = "Seventeen";
            rows[1].Text = "Eighteen";
            Assert.Throws<RowModifiedException>(() => db.UpdateAll(rows));

            var actualRows = db.SelectByIds<ModelWithRowVersion>(rowIds);
            Assert.That(actualRows[0].Text, Is.EqualTo("Fifteen"));
            Assert.That(actualRows[1].Text, Is.Not.EqualTo("Eighteen"));
        }

        [Test]
        public void SaveAll_with_outdated_rowversion_throws_and_all_changed_are_rejected()
        {
            var rowIds = new[]
            {
                db.Insert(new ModelWithRowVersion { Text = "Seventeenth" }, selectIdentity: true),
                db.Insert(new ModelWithRowVersion { Text = "Eighteenth" }, selectIdentity: true)
            };
            var rows = db.SelectByIds<ModelWithRowVersion>(rowIds);
            TouchRow(rowIds[1]);

            rows[0].Text = "Nineteenth";
            rows[1].Text = "Twentieth";
            Assert.Throws<RowModifiedException>(() => db.SaveAll(rows));

            var actualRows = db.SelectByIds<ModelWithRowVersion>(rows.Select(x => x.Id));
            Assert.That(actualRows[0].Text, Is.EqualTo("Seventeenth"));
            Assert.That(actualRows[1].Text, Is.Not.EqualTo("Twentieth"));
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