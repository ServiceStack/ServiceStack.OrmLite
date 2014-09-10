using System;
using System.Data;
using System.Threading;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Oracle;

namespace ServiceStack.OrmLite.Tests.Issues
{
    [TestFixture]
    public class MultiConnectionIdTests : OrmLiteTestBase
    {
        [SetUp]
        public void SetUp()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<MultiConnTable1>();
                db.DropAndCreateTable<MultiConnTable2>();
                // A couple of rows in one table to ensure keys added in tests are different between the two tables
                db.Insert(new MultiConnTable1 { Data = "first" });
                db.Insert(new MultiConnTable1 { Data = "second" });
            }
        }

        [Test]
        public void CanRetrieveIdOnMultipleConnections()
        {
            IDbConnection db1 = null, db2 = null;
            try
            {
                db1 = OpenDbConnection();
                db2 = OpenDbConnection();
                var id1Immediate = db1.Insert(new MultiConnTable1 { Data = "third" }, true);
                var id2Immediate = db2.Insert(new MultiConnTable2 { Data = "third" }, true);
                var id1Later = db1.LastInsertId();
                var id2Later = db2.LastInsertId();
                
                Assert.That(id1Later, Is.EqualTo(id1Immediate));
                Assert.That(id2Later, Is.EqualTo(id2Immediate));
            }
            finally
            {
                if (db1 != null)
                    db1.Close();
                if (db2 != null)
                    db2.Close();
            }
        }

        [Test]
        public void CanRetrieveIdOnMultipleConnectionsInASingleTable()
        {
            IDbConnection db1 = null, db2 = null;
            try
            {
                db1 = OpenDbConnection();
                db2 = OpenDbConnection();
                var id1Immediate = db1.Insert(new MultiConnTable1 { Data = "third" }, true);
                var id2Immediate = db2.Insert(new MultiConnTable1 { Data = "fourth" }, true);
                var id1Later = db1.LastInsertId();
                var id2Later = db2.LastInsertId();

                Assert.That(id1Later, Is.EqualTo(id1Immediate));
                Assert.That(id2Later, Is.EqualTo(id2Immediate));
                Assert.That(id2Later, Is.GreaterThan(id1Later));
            }
            finally
            {
                if (db1 != null)
                    db1.Close();
                if (db2 != null)
                    db2.Close();
            }
        }

        [Test]
        public void PurgingOfIdsWorks()
        {
            var provider = (OracleOrmLiteDialectProvider)OrmLiteConfig.DialectProvider;
            provider.InsertIdCachePurgeInterval = new TimeSpan(0, 0, 0, 0, 100);
            provider.InsertIdCacheKeepTime = new TimeSpan(0, 0, 0, 0, 200);

            IDbConnection db1 = null, db2 = null;
            try
            {
                db1 = OpenDbConnection();
                db2 = OpenDbConnection();
                db1.Insert(new MultiConnTable1 { Data = "third" }, true);

                Thread.Sleep(provider.InsertIdCacheKeepTime);

                var id2Immediate = db2.Insert(new MultiConnTable2 { Data = "third" }, true);
                var id1Later = db1.LastInsertId();

                Thread.Sleep(provider.InsertIdCacheKeepTime);

                var id2Later = db2.LastInsertId();

                Assert.That(id1Later, Is.EqualTo(0));
                Assert.That(id2Later, Is.EqualTo(id2Immediate));
            }
            finally
            {
                if (db1 != null)
                    db1.Close();
                if (db2 != null)
                    db2.Close();
            }
        }

        [Test]
        public void PurgingOfIdsForASingleTableWorks()
        {
            var provider = (OracleOrmLiteDialectProvider)OrmLiteConfig.DialectProvider;
            provider.InsertIdCachePurgeInterval = new TimeSpan(0, 0, 0, 0, 100);
            provider.InsertIdCacheKeepTime = new TimeSpan(0, 0, 0, 0, 200);

            IDbConnection db1 = null, db2 = null;
            try
            {
                db1 = OpenDbConnection();
                db2 = OpenDbConnection();
                db1.Insert(new MultiConnTable1 { Data = "third" }, true);

                Thread.Sleep(provider.InsertIdCacheKeepTime);

                var id2Immediate = db2.Insert(new MultiConnTable1 { Data = "fourth" }, true);
                var id1Later = db1.LastInsertId();

                Thread.Sleep(provider.InsertIdCacheKeepTime);

                var id2Later = db2.LastInsertId();

                Assert.That(id1Later, Is.EqualTo(0));
                Assert.That(id2Later, Is.EqualTo(id2Immediate));
            }
            finally
            {
                if (db1 != null)
                    db1.Close();
                if (db2 != null)
                    db2.Close();
            }
        }
    }

    public class MultiConnTable1
    {
        [AutoIncrement]
        public long Id { get; set; }
        public string Data { get; set; }
    }

    public class MultiConnTable2
    {
        [AutoIncrement]
        public long Id { get; set; }
        public string Data { get; set; }
    }
}
