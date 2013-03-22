using System.Collections.Generic;
using Northwind.Common.DataModel;
using NUnit.Framework;
using ServiceStack.OrmLite.SqlServer;
using ServiceStack.OrmLite.Sqlite;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class OrmLiteConnectionFactoryTests
    {
        [Test]
        public void AutoDispose_ConnectionFactory_disposes_connection()
        {
            OrmLiteConfig.DialectProvider = SqliteDialect.Provider;
            var factory = new OrmLiteConnectionFactory(":memory:", true);

            using (var db = factory.OpenDbConnection())
            {
                db.CreateTable<Shipper>(false);
                db.Insert(new Shipper { CompanyName = "I am shipper" });
            }

            using (var db = factory.OpenDbConnection())
            {
                db.CreateTable<Shipper>(false);
                Assert.That(db.Select<Shipper>(), Has.Count.EqualTo(0));
            }
        }

        [Test]
        public void NonAutoDispose_ConnectionFactory_reuses_connection()
        {
            OrmLiteConfig.DialectProvider = SqliteDialect.Provider;
            var factory = new OrmLiteConnectionFactory(":memory:", false);

            using (var db = factory.OpenDbConnection())
            {
                db.CreateTable<Shipper>(false);
                db.Insert(new Shipper { CompanyName = "I am shipper" });
            }

            using (var db = factory.OpenDbConnection())
            {
                db.CreateTable<Shipper>(false);
                Assert.That(db.Select<Shipper>(), Has.Count.EqualTo(1));
            }
        }

        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Test]
        public void Can_open_multiple_nested_connections()
        {
            var factory = new OrmLiteConnectionFactory(Config.SqliteMemoryDb, false, SqliteDialect.Provider);
            factory.RegisterConnection("sqlserver", Config.SqlServerBuildDb, SqlServerDialect.Provider);
            factory.RegisterConnection("sqlite-file", Config.SqliteFileDb, SqliteDialect.Provider);

            var results = new List<Person>();
            using (var db = factory.OpenDbConnection())
            {
                db.DropAndCreateTable<Person>();
                db.Insert(new Person { Id = 1, Name = "1) :memory:" });
                db.Insert(new Person { Id = 2, Name = "2) :memory:" });

                using (var db2 = factory.OpenDbConnection("sqlserver"))
                {
                    db2.CreateTable<Person>(true);
                    db2.Insert(new Person { Id = 3, Name = "3) Database1.mdf" });
                    db2.Insert(new Person { Id = 4, Name = "4) Database1.mdf" });

                    using (var db3 = factory.OpenDbConnection("sqlite-file"))
                    {
                        db3.CreateTable<Person>(true);
                        db3.Insert(new Person { Id = 5, Name = "5) db.sqlite" });
                        db3.Insert(new Person { Id = 6, Name = "6) db.sqlite" });

                        results.AddRange(db.Select<Person>());
                        results.AddRange(db2.Select<Person>());
                        results.AddRange(db3.Select<Person>());
                    }
                }
            }

            results.PrintDump();
            var ids = results.ConvertAll(x => x.Id);
            Assert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, ids);
        }

        [Test]
        public void Can_open_multiple_nested_connections_in_any_order()
        {
            var factory = new OrmLiteConnectionFactory(Config.SqliteMemoryDb, false, SqliteDialect.Provider);
            factory.RegisterConnection("sqlserver", Config.SqlServerBuildDb, SqlServerDialect.Provider);
            factory.RegisterConnection("sqlite-file", Config.SqliteFileDb, SqliteDialect.Provider);

            var results = new List<Person>();
            using (var db = factory.OpenDbConnection())
            {
                db.CreateTable<Person>(true);
                db.Insert(new Person { Id = 1, Name = "1) :memory:" });

                using (var db2 = factory.OpenDbConnection("sqlserver"))
                {
                    db2.CreateTable<Person>(true);
                    db.Insert(new Person { Id = 2, Name = "2) :memory:" });
                    db2.Insert(new Person { Id = 3, Name = "3) Database1.mdf" });

                    using (var db3 = factory.OpenDbConnection("sqlite-file"))
                    {
                        db3.CreateTable<Person>(true);
                        db2.Insert(new Person { Id = 4, Name = "4) Database1.mdf" });
                        db3.Insert(new Person { Id = 5, Name = "5) db.sqlite" });

                        results.AddRange(db2.Select<Person>());

                        db3.Insert(new Person { Id = 6, Name = "6) db.sqlite" });
                        results.AddRange(db3.Select<Person>());
                    }
                    results.AddRange(db.Select<Person>());
                }
            }

            results.PrintDump();
            var ids = results.ConvertAll(x => x.Id);
            Assert.AreEqual(new[] { 3, 4, 5, 6, 1, 2 }, ids);
        }

    }
}