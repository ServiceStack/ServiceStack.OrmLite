using System.Collections.Generic;
using Northwind.Common.DataModel;
using NUnit.Framework;
using ServiceStack.Common.Utils;
using ServiceStack.OrmLite.Connection;
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
            OrmLiteConfig.DialectProvider = SqliteOrmLiteDialectProvider.Instance;
            var factory = new OrmLiteConnectionFactory(":memory:", true);

            using (var dbConn = factory.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<Shipper>(false);
                dbCmd.Insert(new Shipper { CompanyName = "I am shipper" });
            }

            using (var dbConn = factory.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<Shipper>(false);
                Assert.That(dbCmd.Select<Shipper>(), Has.Count.EqualTo(0));
            }
        }

        [Test]
        public void NonAutoDispose_ConnectionFactory_reuses_connection()
        {
            OrmLiteConfig.DialectProvider = SqliteOrmLiteDialectProvider.Instance;
            var factory = new OrmLiteConnectionFactory(":memory:", false);

            using (var dbConn = factory.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<Shipper>(false);
                dbCmd.Insert(new Shipper { CompanyName = "I am shipper" });
            }

            using (var dbConn = factory.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<Shipper>(false);
                Assert.That(dbCmd.Select<Shipper>(), Has.Count.EqualTo(1));
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
            var factory = new OrmLiteConnectionFactory(":memory:", false, SqliteOrmLiteDialectProvider.Instance);
            factory.RegisterConnection("sqlserver", "~/App_Data/Database1.mdf".MapAbsolutePath(), SqlServerOrmLiteDialectProvider.Instance);
            factory.RegisterConnection("sqlite-file", "~/App_Data/db.sqlite".MapAbsolutePath(), SqliteOrmLiteDialectProvider.Instance);

            var results = new List<Person>();
            using (var dbConn = factory.OpenDbConnection())
            {
                dbConn.CreateTable<Person>(true);
                dbConn.Insert(new Person { Id = 1, Name = "1) :memory:" });
                dbConn.Insert(new Person { Id = 2, Name = "2) :memory:" });

                using (var dbConn2 = factory.OpenDbConnection("sqlserver"))
                {
                    dbConn2.CreateTable<Person>(true);
                    dbConn2.Insert(new Person { Id = 3, Name = "3) Database1.mdf" });
                    dbConn2.Insert(new Person { Id = 4, Name = "4) Database1.mdf" });

                    using (var dbConn3 = factory.OpenDbConnection("sqlite-file"))
                    {
                        dbConn3.CreateTable<Person>(true);
                        dbConn3.Insert(new Person { Id = 5, Name = "5) db.sqlite" });
                        dbConn3.Insert(new Person { Id = 6, Name = "6) db.sqlite" });

                        results.AddRange(dbConn.Select<Person>());
                        results.AddRange(dbConn2.Select<Person>());
                        results.AddRange(dbConn3.Select<Person>());
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
            var factory = new OrmLiteConnectionFactory(":memory:", false, SqliteOrmLiteDialectProvider.Instance);
            factory.RegisterConnection("sqlserver", "~/App_Data/Database1.mdf".MapAbsolutePath(), SqlServerOrmLiteDialectProvider.Instance);
            factory.RegisterConnection("sqlite-file", "~/App_Data/db.sqlite".MapAbsolutePath(), SqliteOrmLiteDialectProvider.Instance);

            var results = new List<Person>();
            using (var dbConn = factory.OpenDbConnection())
            {
                dbConn.CreateTable<Person>(true);
                dbConn.Insert(new Person { Id = 1, Name = "1) :memory:" });

                using (var dbConn2 = factory.OpenDbConnection("sqlserver"))
                {
                    dbConn2.CreateTable<Person>(true);
                    dbConn.Insert(new Person { Id = 2, Name = "2) :memory:" });
                    dbConn2.Insert(new Person { Id = 3, Name = "3) Database1.mdf" });

                    using (var dbConn3 = factory.OpenDbConnection("sqlite-file"))
                    {
                        dbConn3.CreateTable<Person>(true);
                        dbConn2.Insert(new Person { Id = 4, Name = "4) Database1.mdf" });
                        dbConn3.Insert(new Person { Id = 5, Name = "5) db.sqlite" });

                        results.AddRange(dbConn2.Select<Person>());

                        dbConn3.Insert(new Person { Id = 6, Name = "6) db.sqlite" });
                        results.AddRange(dbConn3.Select<Person>());
                    }
                    results.AddRange(dbConn.Select<Person>());
                }
            }

            results.PrintDump();
            var ids = results.ConvertAll(x => x.Id);
            Assert.AreEqual(new[] { 3, 4, 5, 6, 1, 2 }, ids);
        }

    }
}