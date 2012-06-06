using System;
using System.Collections.Generic;
using Northwind.Common.DataModel;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

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
        public void Can_insert_and_select_from_ModelWithFieldsOfDifferentAndNullableTypes_table_default_GUID()
        {
            using (var db = ConnectionString.OpenDbConnection())
            using (var dbConn = db.CreateCommand())
                Can_insert_and_select_from_ModelWithFieldsOfDifferentAndNullableTypes_table_impl(dbConn);
        }

        [Test]
		public void Can_insert_and_select_from_ModelWithFieldsOfDifferentAndNullableTypes_table_compact_GUID()
        {
            Firebird.FirebirdOrmLiteDialectProvider dialect = new Firebird.FirebirdOrmLiteDialectProvider(true);
            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(ConnectionString, dialect);
            using (var db = factory.CreateDbConnection())
            {
                db.Open();
                using (var dbConn = db.CreateCommand())
                {
                    Can_insert_and_select_from_ModelWithFieldsOfDifferentAndNullableTypes_table_impl(dbConn);
                }
            }
        }

		private void Can_insert_and_select_from_ModelWithFieldsOfDifferentAndNullableTypes_table_impl(System.Data.IDbCommand dbConn)
		{
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
		 
		[Test]
		public void Can_insert_table_with_blobs()
		{
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbConn = db.CreateCommand())
			{
				var dsl= OrmLiteConfig.DialectProvider.DefaultStringLength;
				OrmLiteConfig.DialectProvider.DefaultStringLength=1024;
				
				dbConn.CreateTable<OrderBlob>(true);
				OrmLiteConfig.DialectProvider.DefaultStringLength=dsl;

				var row = OrderBlob.Create(1);

				dbConn.Insert(row);

				var rows = dbConn.Select<OrderBlob>();

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
		
		public class UserAuth
		{
			public UserAuth()
			{
				this.Roles = new List<string>();
				this.Permissions = new List<string>();
			}

			[AutoIncrement]
			public virtual int Id { get; set; }
			public virtual string UserName { get; set; }
			public virtual string Email { get; set; }
			public virtual string PrimaryEmail { get; set; }
			public virtual string FirstName { get; set; }
			public virtual string LastName { get; set; }
			public virtual string DisplayName { get; set; }
			public virtual string Salt { get; set; }
			public virtual string PasswordHash { get; set; }
			public virtual List<string> Roles { get; set; }
			public virtual List<string> Permissions { get; set; }
			public virtual DateTime CreatedDate { get; set; }
			public virtual DateTime ModifiedDate { get; set; }
			public virtual Dictionary<string, string> Meta { get; set; }
		}

		[Test]
		public void Can_insert_table_with_UserAuth()
		{
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbCmd = db.CreateCommand())
			{
				dbCmd.CreateTable<UserAuth>(true);
				
				var jsv = "{Id:0,UserName:UserName,Email:as@if.com,PrimaryEmail:as@if.com,FirstName:FirstName,LastName:LastName,DisplayName:DisplayName,Salt:WMQi/g==,PasswordHash:oGdE40yKOprIgbXQzEMSYZe3vRCRlKGuqX2i045vx50=,Roles:[],Permissions:[],CreatedDate:2012-03-20T07:53:48.8720739Z,ModifiedDate:2012-03-20T07:53:48.8720739Z}";
				var userAuth = jsv.To<UserAuth>();

				dbCmd.Insert(userAuth);

				var rows = dbCmd.Select<UserAuth>(q => q.UserName == "UserName");

				Console.WriteLine(rows[0].Dump());

				Assert.That(rows[0].UserName, Is.EqualTo(userAuth.UserName));
			}
		}
		
		
	}

}