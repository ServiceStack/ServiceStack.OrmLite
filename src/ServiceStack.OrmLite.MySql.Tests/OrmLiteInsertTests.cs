using Northwind.Common.DataModel;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.MySql.Tests
{
	[TestFixture]
	public class OrmLiteInsertTests
		: OrmLiteTestBase
	{

		[Test]
		public void Can_insert_into_ModelWithFieldsOfDifferentTypes_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithFieldsOfDifferentTypes>(true);

				var row = ModelWithFieldsOfDifferentTypes.Create(1);

				db.Insert(row);
			}
		}

		[Test]
		public void Can_insert_and_select_from_ModelWithFieldsOfDifferentTypes_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithFieldsOfDifferentTypes>(true);

				var row = ModelWithFieldsOfDifferentTypes.Create(1);

				db.Insert(row);

				var rows = db.Select<ModelWithFieldsOfDifferentTypes>();

				Assert.That(rows, Has.Count.EqualTo(1));

				ModelWithFieldsOfDifferentTypes.AssertIsEqual(rows[0], row);
			}
		}

		[Test]
		public void Can_insert_and_select_from_ModelWithFieldsOfNullableTypes_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithFieldsOfNullableTypes>(true);

				var row = ModelWithFieldsOfNullableTypes.Create(1);

				db.Insert(row);

				var rows = db.Select<ModelWithFieldsOfNullableTypes>();

				Assert.That(rows, Has.Count.EqualTo(1));

				ModelWithFieldsOfNullableTypes.AssertIsEqual(rows[0], row);
			}
		}

		[Test]
		public void Can_insert_and_select_from_ModelWithFieldsOfDifferentAndNullableTypes_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithFieldsOfDifferentAndNullableTypes>(true);

				var row = ModelWithFieldsOfDifferentAndNullableTypes.Create(1);

				db.Insert(row);

				var rows = db.Select<ModelWithFieldsOfDifferentAndNullableTypes>();

				Assert.That(rows, Has.Count.EqualTo(1));

				ModelWithFieldsOfDifferentAndNullableTypes.AssertIsEqual(rows[0], row);
			}
		}

		[Test]
		public void Can_insert_table_with_null_fields()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);

				var row = ModelWithIdAndName.Create(1);
				row.Name = null;

				db.Insert(row);

				var rows = db.Select<ModelWithIdAndName>();

				Assert.That(rows, Has.Count.EqualTo(1));

				ModelWithIdAndName.AssertIsEqual(rows[0], row);
			}
		}

		[Test]
		public void Can_retrieve_LastInsertId_from_inserted_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName1>(true);

                var row1 = new ModelWithIdAndName1() { Name = "A", Id = 4 };
                var row2 = new ModelWithIdAndName1() { Name = "B", Id = 5 };

				db.Insert(row1);
				var row1LastInsertId = db.GetLastInsertId();

				db.Insert(row2);
				var row2LastInsertId = db.GetLastInsertId();

                var insertedRow1 = db.GetById<ModelWithIdAndName1>(row1LastInsertId);
                var insertedRow2 = db.GetById<ModelWithIdAndName1>(row2LastInsertId);

				Assert.That(insertedRow1.Name, Is.EqualTo(row1.Name));
				Assert.That(insertedRow2.Name, Is.EqualTo(row2.Name));
			}
		}

		[Test]
		public void Can_insert_TaskQueue_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<TaskQueue>(true);

				var row = TaskQueue.Create(1);

				db.Insert(row);

				var rows = db.Select<TaskQueue>();

				Assert.That(rows, Has.Count.EqualTo(1));

				//Update the auto-increment id
				row.Id = rows[0].Id;

				TaskQueue.AssertIsEqual(rows[0], row);
			}
		}

        [NUnit.Framework.Ignore("Don't know how to realize in MySql")]
		[Test]
		public void Can_insert_table_with_blobs()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<OrderBlob>(true);

				var row = OrderBlob.Create(1);

				db.Insert(row);

				var rows = db.Select<OrderBlob>();

				Assert.That(rows, Has.Count.EqualTo(1));

				var newRow = rows[0];

				Assert.That(newRow.Id, Is.EqualTo(row.Id));
				Assert.That(newRow.Customer.Id, Is.EqualTo(row.Customer.Id));
				Assert.That(newRow.Employee.Id, Is.EqualTo(row.Employee.Id));
				Assert.That(newRow.IntIds, Is.EquivalentTo(row.IntIds));
				Assert.That(newRow.CharMap, Is.EquivalentTo(row.CharMap));
				Assert.That(newRow.OrderDetails.Count, Is.EqualTo(row.OrderDetails.Count));
				Assert.That(newRow.OrderDetails[0].ProductId, Is.EqualTo(row.OrderDetails[0].ProductId));
				Assert.That(newRow.OrderDetails[1].ProductId, Is.EqualTo(row.OrderDetails[1].ProductId));
				Assert.That(newRow.OrderDetails[2].ProductId, Is.EqualTo(row.OrderDetails[2].ProductId));
			}
		}

	}

    class ModelWithIdAndName1
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
    }

}