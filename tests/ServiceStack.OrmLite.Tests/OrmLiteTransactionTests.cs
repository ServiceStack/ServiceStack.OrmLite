using System;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Sqlite;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class OrmLiteTransactionTests
        : OrmLiteTestBase
    {
        [Test]
        public void Transaction_commit_persists_data_to_the_db()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithIdAndName>();
                db.Insert(new ModelWithIdAndName(1));

                Assert.That(((OrmLiteConnection)db).Transaction, Is.Null);

                using (var dbTrans = db.OpenTransaction())
                {
                    Assert.That(((OrmLiteConnection)db).Transaction, Is.Not.Null);

                    db.Insert(new ModelWithIdAndName(2));
                    db.Insert(new ModelWithIdAndName(3));

                    var rowsInTrans = db.Select<ModelWithIdAndName>();
                    Assert.That(rowsInTrans, Has.Count.EqualTo(3));

                    dbTrans.Commit();
                }

                var rows = db.Select<ModelWithIdAndName>();
                Assert.That(rows, Has.Count.EqualTo(3));
            }
        }

        [Test]
        public void Transaction_rollsback_if_not_committed()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithIdAndName>();
                db.Insert(new ModelWithIdAndName(1));

                using (var dbTrans = db.OpenTransaction())
                {
                    db.Insert(new ModelWithIdAndName(2));
                    db.Insert(new ModelWithIdAndName(3));

                    var rowsInTrans = db.Select<ModelWithIdAndName>();
                    Assert.That(rowsInTrans, Has.Count.EqualTo(3));
                }

                var rows = db.Select<ModelWithIdAndName>();
                Assert.That(rows, Has.Count.EqualTo(1));
            }
        }

        [Test]
        public void Transaction_rollsback_transactions_to_different_tables()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithIdAndName>();
                db.DropAndCreateTable<ModelWithFieldsOfDifferentTypes>();
                db.DropAndCreateTable<ModelWithOnlyStringFields>();

                db.Insert(new ModelWithIdAndName(1));

                using (var dbTrans = db.OpenTransaction())
                {
                    db.Insert(new ModelWithIdAndName(2));
                    db.Insert(ModelWithFieldsOfDifferentTypes.Create(3));
                    db.Insert(ModelWithOnlyStringFields.Create("id3"));

                    Assert.That(db.Select<ModelWithIdAndName>(), Has.Count.EqualTo(2));
                    Assert.That(db.Select<ModelWithFieldsOfDifferentTypes>(), Has.Count.EqualTo(1));
                    Assert.That(db.Select<ModelWithOnlyStringFields>(), Has.Count.EqualTo(1));
                }

                Assert.That(db.Select<ModelWithIdAndName>(), Has.Count.EqualTo(1));
                Assert.That(db.Select<ModelWithFieldsOfDifferentTypes>(), Has.Count.EqualTo(0));
                Assert.That(db.Select<ModelWithOnlyStringFields>(), Has.Count.EqualTo(0));
            }
        }

        [Test]
        public void Transaction_commits_inserts_to_different_tables()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithIdAndName>();
                db.DropAndCreateTable<ModelWithFieldsOfDifferentTypes>();
                db.DropAndCreateTable<ModelWithOnlyStringFields>();

                db.Insert(new ModelWithIdAndName(1));

                using (var dbTrans = db.OpenTransaction())
                {
                    db.Insert(new ModelWithIdAndName(2));
                    db.Insert(ModelWithFieldsOfDifferentTypes.Create(3));
                    db.Insert(ModelWithOnlyStringFields.Create("id3"));

                    Assert.That(db.Select<ModelWithIdAndName>(), Has.Count.EqualTo(2));
                    Assert.That(db.Select<ModelWithFieldsOfDifferentTypes>(), Has.Count.EqualTo(1));
                    Assert.That(db.Select<ModelWithOnlyStringFields>(), Has.Count.EqualTo(1));

                    dbTrans.Commit();
                }

                Assert.That(db.Select<ModelWithIdAndName>(), Has.Count.EqualTo(2));
                Assert.That(db.Select<ModelWithFieldsOfDifferentTypes>(), Has.Count.EqualTo(1));
                Assert.That(db.Select<ModelWithOnlyStringFields>(), Has.Count.EqualTo(1));
            }
        }

        public class MyTable
        {
            [AutoIncrement]
            public int Id { get; set; }
            public string SomeTextField { get; set; }
        }

        [Test]
        public void Does_Sqlite_transactions()
        {
            var factory = new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider);

            // test 1 - no transactions
            try
            {
                using (var conn = factory.OpenDbConnection())
                {
                    conn.CreateTable<MyTable>();

                    conn.Insert(new MyTable { SomeTextField = "Example" });
                    var record = conn.SingleById<MyTable>(1);
                }

                "Test 1 Success".Print();
            }
            catch (Exception e)
            {
                Assert.Fail("Test 1 Failed: {0}".Fmt(e.Message));
            }

            // test 2 - all transactions
            try
            {
                using (var conn = factory.OpenDbConnection())
                {
                    conn.CreateTable<MyTable>();

                    using (var tran = conn.OpenTransaction())
                    {
                        conn.Insert(new MyTable { SomeTextField = "Example" });
                        tran.Commit();
                    }

                    using (var tran = conn.OpenTransaction())
                    {
                        var record = conn.SingleById<MyTable>(1);
                    }
                }

                "Test 2 Success".Print();
            }
            catch (Exception e)
            {
                Assert.Fail("Test 2 Failed: {0}".Fmt(e.Message));
            }

            // test 3 - transaction for insert, not for select
            try
            {
                using (var conn = factory.OpenDbConnection())
                {
                    conn.CreateTable<MyTable>();

                    using (var tran = conn.OpenTransaction())
                    {
                        conn.Insert(new MyTable { SomeTextField = "Example" });
                        tran.Commit();
                    }

                    var record = conn.SingleById<MyTable>(1);
                }

                "Test 3 Success".Print();
            }
            catch (Exception e)
            {
                Assert.Fail("Test 3 Failed: {0}".Fmt(e.Message));
            }
        }

        [Test]
        public void Does_allow_setting_transactions_on_raw_DbCommands()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<MyTable>();

                using (var trans = db.OpenTransaction())
                {
                    db.Insert(new MyTable { SomeTextField = "Example" });

                    using (var dbCmd = db.CreateCommand())
                    {
                        dbCmd.Transaction = trans.ToDbTransaction();

                        dbCmd.CommandText = "INSERT INTO {0} ({1}) VALUES ('From OrmLite DB Command')"
                            .Fmt("MyTable".SqlTable(), "SomeTextField".SqlColumn());
                        dbCmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                }

                Assert.That(db.Count<MyTable>(), Is.EqualTo(2));
            }
        }

        [Test]
        public void Can_use_OpenCommand_in_Transaction()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<MyTable>();

                using (var trans = db.OpenTransaction())
                {
                    db.Insert(new MyTable { SomeTextField = "Example" });

                    using (var dbCmd = db.OpenCommand())
                    {
                        dbCmd.CommandText = "INSERT INTO {0} ({1}) VALUES ('From OrmLite DB Command')"
                            .Fmt("MyTable".SqlTable(), "SomeTextField".SqlColumn());
                        dbCmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                }

                Assert.That(db.Count<MyTable>(), Is.EqualTo(2));
            }
        }

    }
}