using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Common;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class DummyTable
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [TestFixture]
    public class SqlServerProviderTests
    {
        private IDbConnection db;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            db = Config.OpenDbConnection();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            db.Dispose();
        }

        [Test]
        public void Can_SqlList_StoredProc_returning_Table()
        {
            var sql = @"CREATE PROCEDURE dbo.DummyTable
    @Times integer
AS
BEGIN
    SET NOCOUNT ON;
 
    CREATE TABLE #Temp
    (
        Id   integer NOT NULL,
        Name nvarchar(50) COLLATE DATABASE_DEFAULT NOT NULL
    );
 
	declare @i int
	set @i=1
	WHILE @i < @Times
	BEGIN
	    INSERT INTO #Temp (Id, Name) VALUES (@i, CAST(@i as nvarchar))
		SET @i = @i + 1
	END

	SELECT * FROM #Temp;
	 
    DROP TABLE #Temp;
END;";
            db.ExecuteSql("IF OBJECT_ID('DummyTable') IS NOT NULL DROP PROC DummyTable");
            db.ExecuteSql(sql);

            var expected = 0;
            10.Times(i => expected += i);

            var results = db.SqlList<DummyTable>("EXEC DummyTable @Times", new { Times = 10 });
            results.PrintDump();
            Assert.That(results.Sum(x => x.Id), Is.EqualTo(expected));

            results = db.SqlList<DummyTable>("EXEC DummyTable 10");
            Assert.That(results.Sum(x => x.Id), Is.EqualTo(expected));

            results = db.SqlList<DummyTable>("EXEC DummyTable @Times", new Dictionary<string, object> { {"Times", 10}});
            Assert.That(results.Sum(x => x.Id), Is.EqualTo(expected));
        }

        [Test]
        public void Can_SqlList_StoredProc_returning_Column()
        {
            var sql = @"CREATE PROCEDURE dbo.DummyColumn
    @Times integer
AS
BEGIN
    SET NOCOUNT ON;
 
    CREATE TABLE #Temp
    (
        Id   integer NOT NULL,
    );
 
	declare @i int
	set @i=1
	WHILE @i < @Times
	BEGIN
	    INSERT INTO #Temp (Id) VALUES (@i)
		SET @i = @i + 1
	END

	SELECT * FROM #Temp;
	 
    DROP TABLE #Temp;
END;";
            db.ExecuteSql("IF OBJECT_ID('DummyColumn') IS NOT NULL DROP PROC DummyColumn");
            db.ExecuteSql(sql);

            var expected = 0;
            10.Times(i => expected += i);

            var results = db.SqlList<int>("EXEC DummyColumn @Times", new { Times = 10 });
            results.PrintDump();
            Assert.That(results.Sum(), Is.EqualTo(expected));

            results = db.SqlList<int>("EXEC DummyColumn 10");
            Assert.That(results.Sum(), Is.EqualTo(expected));

            results = db.SqlList<int>("EXEC DummyTable @Times", new Dictionary<string, object> { { "Times", 10 } });
            Assert.That(results.Sum(), Is.EqualTo(expected));
        }

        [Test]
        public void Can_SqlList_StoredProc_returning_Scalar()
        {
            var sql = @"CREATE PROCEDURE dbo.DummyScalar
    @Times integer
AS
BEGIN
    SET NOCOUNT ON;

	SELECT @Times AS Id
END;";
            db.ExecuteSql("IF OBJECT_ID('DummyScalar') IS NOT NULL DROP PROC DummyScalar");
            db.ExecuteSql(sql);

            const int expected = 10;

            var result = db.SqlScalar<int>("EXEC DummyScalar @Times", new { Times = 10 });
            result.PrintDump();
            Assert.That(result, Is.EqualTo(expected));

            result = db.SqlScalar<int>("EXEC DummyScalar 10");
            Assert.That(result, Is.EqualTo(expected));

            result = db.SqlScalar<int>("EXEC DummyScalar @Times", new Dictionary<string, object> { { "Times", 10 } });
            Assert.That(result, Is.EqualTo(expected));

            result = db.SqlScalar<int>("SELECT 10");
            Assert.That(result, Is.EqualTo(expected));
        }

    }
}