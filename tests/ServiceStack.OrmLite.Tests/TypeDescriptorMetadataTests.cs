// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt


using System;
using System.Data;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class PersonDescriptor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    [TestFixture]
    public class TypeDescriptorMetadataTests
        : OrmLiteTestBase
    {
        private IDbConnection db;

        [TestFixtureSetUp]
        public new void TestFixtureSetUp()
        {
            db = OpenDbConnection();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            db.Dispose();
        }


        [Test]
        public void Can_add_AutoIncrement_Id_at_runtime()
        {
            var model = new PersonDescriptor { FirstName = "Jimi", LastName = "Hendrix", Age = 27 };

            typeof(PersonDescriptor).GetProperty("Id")
                .AddAttributes(new AutoIncrementAttribute());

            db.DropAndCreateTable<PersonDescriptor>();

            var oldRows = db.Select<PersonDescriptor>();

            db.Insert(model);
            db.Insert(model);
            model.Id = 0; // Oracle provider currently updates the id field so force it back to get an insert operation
            db.Save(model);

            var allRows = db.Select<PersonDescriptor>();
            Assert.That(allRows.Count - oldRows.Count, Is.EqualTo(3));
        }

        [Test]
        public void Can_change_column_definition()
        {
            SuppressIfOracle("Test assert fails with Oracle because Oracle does not allow 64000 character fields and uses VARCHAR2 not VARCHAR");

            typeof(DynamicCacheEntry)
                .GetProperty("Data")
                .AddAttributes(new StringLengthAttribute(64000));

            db.DropAndCreateTable<DynamicCacheEntry>();

            Assert.That(db.GetLastSql(), Is.StringContaining("\"Data\" VARCHAR(64000)"));
            db.GetLastSql().Print();
        }

        [Test]
        public void Can_Create_Table_with_MaxText_column()
        {
            db.DropAndCreateTable<CacheEntry>();

            db.GetLastSql().Print();
        }
    }

    public class DynamicCacheEntry
    {
        public string Id { get; set; }
        public string Data { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public class CacheEntry
    {
        public string Id { get; set; }
        [StringLength(StringLengthAttribute.MaxText)]
        public string Data { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}