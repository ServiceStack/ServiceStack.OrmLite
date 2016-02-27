﻿using System;
using System.Data;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Converters;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class _TypeDescriptorMetadataTests //Needs to be run first
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
            if (Dialect == Dialect.PostgreSql) return; //Uses 'text' for strings by default

            typeof(DynamicCacheEntry)
                .GetProperty("Data")
                .AddAttributes(new StringLengthAttribute(7000));

            db.DropAndCreateTable<DynamicCacheEntry>();

            Assert.That(db.GetLastSql().NormalizeSql(), 
                Is.StringContaining("Data VARCHAR(7000)".NormalizeSql()));
            db.GetLastSql().Print();
        }

        [Test]
        public void Can_Create_Table_with_MaxText_column()
        {
            try
            {
                db.DropAndCreateTable<CacheEntry>();
            }
            catch (Exception)
            {
                db.DropAndCreateTable<CacheEntry>();
            }

            var sql = db.GetLastSql();
            sql.Print();

            if (Dialect == Dialect.Sqlite)
            {
                Assert.That(sql, Is.StringContaining(" VARCHAR(1000000)"));
            }
            else if (Dialect == Dialect.PostgreSql)
            {
                Assert.That(sql, Is.StringContaining(" TEXT"));
            }
            else if (Dialect == Dialect.MySql)
            {
                Assert.That(sql, Is.StringContaining(" LONGTEXT"));
            }
            else if (Dialect == Dialect.Oracle)
            {
                Assert.That(sql, Is.StringContaining(" VARCHAR2(4000)"));
            }
            else if (Dialect == Dialect.SqlServer)
            {
                Assert.That(sql, Is.StringContaining(" VARCHAR(MAX)"));
            }
        }

        [Test]
        public void Can_Create_Table_with_MaxText_column_Unicode()
        {
            var stringConverter = db.GetDialectProvider().GetStringConverter();
            var hold = stringConverter.UseUnicode;
            stringConverter.UseUnicode = true;

            try
            {
                db.DropAndCreateTable<CacheEntry>();
            }
            catch (Exception)
            {
                db.DropAndCreateTable<CacheEntry>();
            }
            finally
            {
                stringConverter.UseUnicode = hold;
            }

            var sql = db.GetLastSql();
            sql.Print();

            if (Dialect == Dialect.Sqlite)
            {
                Assert.That(sql, Is.StringContaining(" NVARCHAR(1000000)"));
            }
            else if (Dialect == Dialect.PostgreSql)
            {
                Assert.That(sql, Is.StringContaining(" TEXT"));
            }
            else if (Dialect == Dialect.MySql)
            {
                Assert.That(sql, Is.StringContaining(" LONGTEXT"));
            }
            else if (Dialect == Dialect.Oracle)
            {
                Assert.That(sql, Is.StringContaining(" NVARCHAR2(4000)"));
            }
            else if (Dialect == Dialect.Firebird)
            {
                Assert.That(sql, Is.StringContaining(" VARCHAR(10000)"));
            }
            else
            {
                Assert.That(sql, Is.StringContaining(" NVARCHAR(MAX)"));
            }
        }
    }
}