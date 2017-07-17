using System;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Tests.Shared;
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
        public void Can_InsertOnly_aliased_fields()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<PersonWithAliasedAge>();

                db.InsertOnly(() => new PersonWithAliasedAge { Name = "Bob", Age = 30 });
            }
        }

        [Test]
        public void Can_InsertOnly_fields_using_EnumAsInt()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<PersonUsingEnumAsInt>();

                db.InsertOnly(() => new PersonUsingEnumAsInt { Name = "Sarah", Gender = Gender.Female });

                var saved = db.Single<PersonUsingEnumAsInt>(p => p.Name == "Sarah");
                Assert.That(saved.Name, Is.EqualTo("Sarah"));
                Assert.That(saved.Gender, Is.EqualTo(Gender.Female));
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

                rows.PrintDump();

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

                var row1LastInsertId = db.Insert(row1, selectIdentity: true);

                var row2LastInsertId = db.Insert(row2, selectIdentity: true);

                var insertedRow1 = db.SingleById<ModelWithIdAndName>(row1LastInsertId);
                var insertedRow2 = db.SingleById<ModelWithIdAndName>(row2LastInsertId);

                Assert.That(insertedRow1.Name, Is.EqualTo(row1.Name));
                Assert.That(insertedRow2.Name, Is.EqualTo(row2.Name));
            }
        }

        [Test]
        public void Can_retrieve_AutoIdentityId_with_InsertOnly_fieldNames()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithIdAndName>();

                var row1 = ModelWithIdAndName.Create(5);
                var row2 = ModelWithIdAndName.Create(6);

                var row1LastInsertId = db.InsertOnly(row1, new[] { "Id", "Name" }, selectIdentity: true);

                var row2LastInsertId = db.InsertOnly(row2, new[] { "Id", "Name" }, selectIdentity: true);

                var insertedRow1 = db.SingleById<ModelWithIdAndName>(row1LastInsertId);
                var insertedRow2 = db.SingleById<ModelWithIdAndName>(row2LastInsertId);

                Assert.That(insertedRow1.Name, Is.EqualTo(row1.Name));
                Assert.That(insertedRow2.Name, Is.EqualTo(row2.Name));
            }
        }

        [Test]
        public void Can_retrieve_AutoIdentityId_with_InsertOnly_expression()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithIdAndName>();

                var row1LastInsertId = db.InsertOnly(() => new ModelWithIdAndName
                {
                    Id = 5,
                    Name = "Name 5"
                }, selectIdentity: true);

                var row2LastInsertId = db.InsertOnly(() => new ModelWithIdAndName
                {
                    Id = 6,
                    Name = "Name 6"
                }, selectIdentity: true);

                var insertedRow1 = db.SingleById<ModelWithIdAndName>(row1LastInsertId);
                var insertedRow2 = db.SingleById<ModelWithIdAndName>(row2LastInsertId);

                Assert.That(insertedRow1.Name, Is.EqualTo("Name 5"));
                Assert.That(insertedRow2.Name, Is.EqualTo("Name 6"));
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
        public void Can_GetLastInsertedId_using_Insert()
        {
            SuppressIfOracle("Need trigger for autoincrement keys to work in Oracle with caller supplied SQL");
            if (Dialect == Dialect.Firebird) return; //Requires Generator

            var date = new DateTime(2000, 1, 1);
            var testObject = new UserAuth
            {
                UserName = "test",
                CreatedDate = date,
                ModifiedDate = date,
                InvalidLoginAttempts = 0,
            };

            //verify that "normal" Insert works as expected
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<UserAuth>();

                db.ExecuteSql("INSERT INTO {0} ({1},{2},{3},{4}) VALUES ({5},'2000-01-01','2000-01-01',0)"
                    .Fmt("UserAuth".SqlTable(),
                         "UserName".SqlColumn(),
                         "CreatedDate".SqlColumn(),
                         "ModifiedDate".SqlColumn(),
                         "InvalidLoginAttempts".SqlColumn(),
                         testObject.UserName.SqlValue()));
                var normalLastInsertedId = db.LastInsertId();
                Assert.Greater(normalLastInsertedId, 0, "normal Insert");
            }

            //test with InsertParam
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<UserAuth>();

                var lastInsertId = db.Insert(testObject, selectIdentity: true);
                Assert.Greater(lastInsertId, 0, "with InsertParam");
            }
        }

        [Test]
        public void Can_InsertOnly_selected_fields()
        {
            SuppressIfOracle("Need trigger for autoincrement keys to work in Oracle");

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<PersonWithAutoId>();

                db.InsertOnly(new PersonWithAutoId { Age = 27 }, x => x.Age);
                var row = db.Select<PersonWithAutoId>()[0];
                Assert.That(row.Age, Is.EqualTo(27));
                Assert.That(row.FirstName, Is.Null);
                Assert.That(row.LastName, Is.Null);

                db.DeleteAll<PersonWithAutoId>();

                db.InsertOnly(new PersonWithAutoId { FirstName = "Amy", Age = 27 }, x => new { x.FirstName, x.Age });
                row = db.Select<PersonWithAutoId>()[0];
                Assert.That(row.FirstName, Is.EqualTo("Amy"));
                Assert.That(row.Age, Is.EqualTo(27));
                Assert.That(row.LastName, Is.Null);

                db.DeleteAll<PersonWithAutoId>();

                db.InsertOnly(new PersonWithAutoId { FirstName = "Amy", Age = 27 }, new[] { "FirstName", "Age" });
                row = db.Select<PersonWithAutoId>()[0];
                Assert.That(row.FirstName, Is.EqualTo("Amy"));
                Assert.That(row.Age, Is.EqualTo(27));
                Assert.That(row.LastName, Is.Null);
            }
        }

        [Test]
        public void Can_InsertOnly_selected_fields_using_AssignmentExpression()
        {
            SuppressIfOracle("Need trigger for autoincrement keys to work in Oracle");

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<PersonWithAutoId>();

                db.InsertOnly(() => new PersonWithAutoId { FirstName = "Amy", Age = 27 });
                Assert.That(db.GetLastSql().NormalizeSql(), Is.EqualTo("insert into personwithautoid (firstname,age) values (@firstname,@age)"));

                var row = db.Select<PersonWithAutoId>()[0];
                Assert.That(row.FirstName, Is.EqualTo("Amy"));
                Assert.That(row.Age, Is.EqualTo(27));
                Assert.That(row.LastName, Is.Null);
            }
        }

        [Test]
        public void Can_insert_record_with_Computed_column()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Market>();
                var market = new Market
                {
                    Available = 10,
                    AvailableTotal = 0,
                    AvailableSalesEvent = 2,
                    MinCustomerBuy = 10
                };

                var fieldDef = typeof(Market).GetModelMetadata()
                    .GetFieldDefinition<Market>(x => x.AvailableTotal);

                fieldDef.IsComputed = false;

                db.Insert(market);

                fieldDef.IsComputed = true;
            }
        }
    }

    public class Market
    {
        [AutoIncrement]
        public int Id { get; set; }
        [Required]
        public int Available { get; set; }
        [Required]
        public int AvailableSalesEvent { get; set; }
        [Compute]
        [Required]
        public int AvailableTotal { get; set; }
        [Required]
        public int? MinCustomerBuy { get; set; }
    }

    public class UserAuth
    {
        public UserAuth()
        {
            Roles = new List<string>();
            Permissions = new List<string>();
        }

        [AutoIncrement]
        public virtual int Id { get; set; }

        public virtual string UserName { get; set; }
        public virtual string Email { get; set; }
        public virtual string PrimaryEmail { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual string Company { get; set; }
        public virtual DateTime? BirthDate { get; set; }
        public virtual string BirthDateRaw { get; set; }
        public virtual string Address { get; set; }
        public virtual string Address2 { get; set; }
        public virtual string City { get; set; }
        public virtual string State { get; set; }
        public virtual string Country { get; set; }
        public virtual string Culture { get; set; }
        public virtual string FullName { get; set; }
        public virtual string Gender { get; set; }
        public virtual string Language { get; set; }
        public virtual string MailAddress { get; set; }
        public virtual string Nickname { get; set; }
        public virtual string PostalCode { get; set; }
        public virtual string TimeZone { get; set; }
        public virtual string Salt { get; set; }
        public virtual string PasswordHash { get; set; }
        public virtual string DigestHa1Hash { get; set; }
        public virtual List<string> Roles { get; set; }
        public virtual List<string> Permissions { get; set; }
        public virtual DateTime CreatedDate { get; set; }
        public virtual DateTime ModifiedDate { get; set; }
        public virtual int InvalidLoginAttempts { get; set; }
        public virtual DateTime? LastLoginAttempt { get; set; }
        public virtual DateTime? LockedDate { get; set; }
        public virtual string RecoveryToken { get; set; }

        //Custom Reference Data
        public virtual int? RefId { get; set; }
        public virtual string RefIdStr { get; set; }
        public virtual Dictionary<string, string> Meta { get; set; }
    }
    public class UserAuthRole
    {
        [AutoIncrement]
        public virtual int Id { get; set; }
        public virtual int UserAuthId { get; set; }
        public virtual string Role { get; set; }
        public virtual string Permission { get; set; }
        public virtual DateTime CreatedDate { get; set; }
        public virtual DateTime ModifiedDate { get; set; }

        //Custom Reference Data
        public virtual int? RefId { get; set; }
        public virtual string RefIdStr { get; set; }
        public virtual Dictionary<string, string> Meta { get; set; }
    }
}