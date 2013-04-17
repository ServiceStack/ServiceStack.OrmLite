using System;
using System.Data;
using System.Diagnostics;
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
        private bool _firstThreadCompleted = false;
	    private bool _secondThreadFinished = false;

		[Test]
		public void Transaction_commit_persists_data_to_the_db()
		{
			using (var db = Config.OpenDbConnection())
			{
				db.DropAndCreateTable<ModelWithIdAndName>();
				db.Insert(new ModelWithIdAndName(1));

				using (var dbTrans = db.OpenTransaction())
				{
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
            using (var db = Config.OpenDbConnection())
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
            using (var db = Config.OpenDbConnection())
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
            using (var db = Config.OpenDbConnection())
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
        
        class MyTable
        {
            [AutoIncrement]
            public int Id { get; set; }
            public String SomeTextField { get; set; }
        }

	    [Test]
	    public void Does_Sqlite_transactions()
	    {
	        var factory = new OrmLiteConnectionFactory(":memory:", true, SqliteDialect.Provider);

	        // test 1 - no transactions
	        try
	        {
	            using (var conn = factory.OpenDbConnection())
	            {
	                conn.CreateTable<MyTable>();

	                conn.Insert(new MyTable {SomeTextField = "Example"});
	                var record = conn.GetById<MyTable>(1);
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
	                    conn.Insert(new MyTable {SomeTextField = "Example"});
	                    tran.Commit();
	                }

	                using (var tran = conn.OpenTransaction())
	                {
	                    var record = conn.GetById<MyTable>(1);
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
	                    conn.Insert(new MyTable {SomeTextField = "Example"});
	                    tran.Commit();
	                }

	                var record = conn.GetById<MyTable>(1);
	            }

	            "Test 3 Success".Print();
	        }
	        catch (Exception e)
	        {
	            Assert.Fail("Test 3 Failed: {0}".Fmt(e.Message));
	        }

	        // test 4 - use transaction
	        try
	        {
	            using (var conn = factory.OpenDbConnection())
	            {
	                conn.CreateTable<MyTable>();

	                conn.UseTransaction((T) =>
	                    {
	                        conn.Insert(new MyTable {SomeTextField = "Example"});
	                        T.Commit();
	                    });

	                var record = conn.GetById<MyTable>(1);
	            }

	            "Test 4 Success".Print();
	        }
	        catch (Exception e)
	        {
	            Assert.Fail("Test 4 Failed: {0}".Fmt(e.Message));
	        }

            // test 5 - use transaction blocks writes
            try
            {
                var conn = factory.OpenDbConnection();
                conn.CreateTable<MyTable>();

                var thread = new System.Threading.Thread(() =>
                    {
                        conn.Insert(new MyTable { SomeTextField = "Example2" });
                        if (_firstThreadCompleted)
                            conn.Dispose();
                        _secondThreadFinished = true;
                    });

                conn.UseTransaction((T) =>
                {
                    conn.Insert(new MyTable { SomeTextField = "Example" });
                    thread.Start();
                    System.Threading.Thread.Sleep(3000);
                    T.Commit();
                    Assert.That(_secondThreadFinished, Is.EqualTo(false));
                });

                var record = conn.GetById<MyTable>(1);

                _firstThreadCompleted = true;

                if(_secondThreadFinished)
                    conn.Dispose();

                "Test 5 Success".Print();
            }
            catch (Exception e)
            {
                Assert.Fail("Test 5 Failed: {0}".Fmt(e.Message));
            }

            _firstThreadCompleted = false;
	        _secondThreadFinished = false;

            // test 6 - use transaction blocks use transactions from other threads
            try
            {
                var conn = factory.OpenDbConnection();
                conn.CreateTable<MyTable>();

                var thread = new System.Threading.Thread(() =>
                {
                    conn.UseTransaction((T) =>
                        {
                            //no-op;
                        });
                    
                    if (_firstThreadCompleted)
                        conn.Dispose();
                    _secondThreadFinished = true;
                });

                conn.UseTransaction((T) =>
                {
                    conn.Insert(new MyTable { SomeTextField = "Example" });
                    thread.Start();
                    System.Threading.Thread.Sleep(3000);
                    T.Commit();
                    Assert.That(_secondThreadFinished, Is.EqualTo(false));
                });

                var record = conn.GetById<MyTable>(1);

                _firstThreadCompleted = true;

                if (_secondThreadFinished)
                    conn.Dispose();

                "Test 6 Success".Print();
            }
            catch (Exception e)
            {
                Assert.Fail("Test 6 Failed: {0}".Fmt(e.Message));
            }
	    }
	}
}