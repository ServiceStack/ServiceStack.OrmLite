using System;
using Northwind.Common.DataModel;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.OrmLite.FirebirdTests
{
	[TestFixture]
	public class OrmLiteInsertTests
		: OrmLiteTestBase
	{
		
		[Test]
		public void Can_insert_into_ModelWithFieldsOfDifferentTypes_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbConn = db.CreateCommand())
			{
				dbConn.CreateTable<ModelWithFieldsOfDifferentTypes>(true);

				var row = ModelWithFieldsOfDifferentTypes.Create(1);

				dbConn.Insert(row);
			}
		}

		[Test]
		public void Can_insert_and_select_from_ModelWithFieldsOfDifferentTypes_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbConn = db.CreateCommand())
			{
				dbConn.CreateTable<ModelWithFieldsOfDifferentTypes>(true);

				var row = ModelWithFieldsOfDifferentTypes.Create(1);

				dbConn.Insert(row);

				var rows = dbConn.Select<ModelWithFieldsOfDifferentTypes>();

				Assert.That(rows, Has.Count.EqualTo(1));

				ModelWithFieldsOfDifferentTypes.AssertIsEqual(rows[0], row);
			}
		}

		[Test]
		public void Can_insert_and_select_from_ModelWithFieldsOfNullableTypes_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbConn = db.CreateCommand())
			{
				dbConn.CreateTable<ModelWithFieldsOfNullableTypes>(true);

				var row = ModelWithFieldsOfNullableTypes.Create(1);

				dbConn.Insert(row);

				var rows = dbConn.Select<ModelWithFieldsOfNullableTypes>();

				Assert.That(rows, Has.Count.EqualTo(1));

				ModelWithFieldsOfNullableTypes.AssertIsEqual(rows[0], row);
			}
		}

		[Test]
		public void Can_insert_and_select_from_ModelWithFieldsOfDifferentAndNullableTypes_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbConn = db.CreateCommand())
			{
				dbConn.CreateTable<ModelWithFieldsOfDifferentAndNullableTypes>(true);

				var row = ModelWithFieldsOfDifferentAndNullableTypes.Create(1);
				
				Console.WriteLine(OrmLiteConfig.DialectProvider.ToInsertRowStatement(row, null));
				dbConn.Insert(row);

				var rows = dbConn.Select<ModelWithFieldsOfDifferentAndNullableTypes>();

				Assert.That(rows, Has.Count.EqualTo(1));

				ModelWithFieldsOfDifferentAndNullableTypes.AssertIsEqual(rows[0], row);
			}
		}

		[Test]
		public void Can_insert_table_with_null_fields()
		{
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbConn = db.CreateCommand())
			{
				dbConn.CreateTable<ModelWithIdAndName>(true);
				dbConn.DeleteAll<ModelWithIdAndName>();
				var row = ModelWithIdAndName.Create(0);
				row.Name = null;

				dbConn.Insert(row);

				var rows = dbConn.Select<ModelWithIdAndName>();

				Assert.That(rows, Has.Count.EqualTo(1));

				ModelWithIdAndName.AssertIsEqual(rows[0], row);
			}
		}
		 
		[Test]
		public void Can_retrieve_LastInsertId_from_inserted_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbCmd = db.CreateCommand())
			{
				dbCmd.CreateTable<ModelWithIdAndName>(true);

				var row1 = ModelWithIdAndName.Create(5);
				var row2 = ModelWithIdAndName.Create(6);

				dbCmd.Insert(row1);
				var row1LastInsertId = dbCmd.GetLastInsertId();

				dbCmd.Insert(row2);
				var row2LastInsertId = dbCmd.GetLastInsertId();

				var insertedRow1 = dbCmd.GetById<ModelWithIdAndName>(row1LastInsertId);
				var insertedRow2 = dbCmd.GetById<ModelWithIdAndName>(row2LastInsertId);

				Assert.That(insertedRow1.Name, Is.EqualTo(row1.Name));
				Assert.That(insertedRow2.Name, Is.EqualTo(row2.Name));
			}
		}
		
		[Test]
		public void Can_insert_TaskQueue_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbConn = db.CreateCommand())
			{
				dbConn.CreateTable<TaskQueue>(true);

				var row = TaskQueue.Create(1);

				dbConn.Insert(row);

				var rows = dbConn.Select<TaskQueue>();

				Assert.That(rows, Has.Count.EqualTo(1));

				//Update the auto-increment id
				row.Id = rows[0].Id;

				TaskQueue.AssertIsEqual(rows[0], row);
			}
		}
		 
		[Test][Ignore]
		public void Can_insert_table_with_blobs()
		{
			
		}

	}

}