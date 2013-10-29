using System;
using System.Collections.Generic;
using Northwind.Common.DataModel;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
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
                db.DropAndCreateTable<ModelWithIdAndName>();

				var row1 = ModelWithIdAndName.Create(5);
				var row2 = ModelWithIdAndName.Create(6);

				db.Insert(row1);
				var row1LastInsertId = db.GetLastInsertId();

				db.Insert(row2);
				var row2LastInsertId = db.GetLastInsertId();

				var insertedRow1 = db.GetById<ModelWithIdAndName>(row1LastInsertId);
				var insertedRow2 = db.GetById<ModelWithIdAndName>(row2LastInsertId);

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

		[Test]
		public void Can_insert_table_with_blobs()
		{
			using (var db = OpenDbConnection())
			{
				db.DropAndCreateTable<OrderBlob>();

				var row = OrderBlob.Create(1);

				db.InsertParam(row);

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
			using (var db = OpenDbConnection())
			{
				db.CreateTable<UserAuth>(true);

				//var userAuth = new UserAuth {
				//    Id = 1,
				//    UserName = "UserName",
				//    Email = "a@b.com",
				//    PrimaryEmail = "c@d.com",
				//    FirstName = "FirstName",
				//    LastName = "LastName",
				//    DisplayName = "DisplayName",
				//    Salt = "Salt",
				//    PasswordHash = "PasswordHash",
				//    CreatedDate = DateTime.Now,
				//    ModifiedDate = DateTime.UtcNow,
				//};

				var jsv = "{Id:0,UserName:UserName,Email:as@if.com,PrimaryEmail:as@if.com,FirstName:FirstName,LastName:LastName,DisplayName:DisplayName,Salt:WMQi/g==,PasswordHash:oGdE40yKOprIgbXQzEMSYZe3vRCRlKGuqX2i045vx50=,Roles:[],Permissions:[],CreatedDate:2012-03-20T07:53:48.8720739Z,ModifiedDate:2012-03-20T07:53:48.8720739Z}";
				var userAuth = jsv.To<UserAuth>();

				db.Insert(userAuth);

				var rows = db.Select<UserAuth>(q => q.UserName == "UserName");

				Console.WriteLine(rows[0].Dump());

				Assert.That(rows[0].UserName, Is.EqualTo(userAuth.UserName));
			}
		}

        [Test]
        public void Can_GetLastInsertedId_using_InsertParam()
        {
            var testObject = new UserAuth { UserName = "test" };

            //verify that "normal" Insert works as expected
            using (var con = OpenDbConnection())
            {
                con.CreateTable<UserAuth>(true);

                con.Insert(testObject);
                var normalLastInsertedId = con.GetLastInsertId();
                Assert.Greater(normalLastInsertedId, 0, "normal Insert");
            }

            //test with InsertParam
            using (var con = OpenDbConnection())
            {
                con.CreateTable<UserAuth>(true);

                var lastInsertId = con.InsertParam(testObject, selectIdentity: true);
                Assert.Greater(lastInsertId, 0, "with InsertParam");
            }
        }
    }
}