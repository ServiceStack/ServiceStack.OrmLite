using System;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class TestDate
    {
        public string Name { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    [TestFixture]
    public class UtcDateTimeIssueTests : OrmLiteTestBase
    {
        [Test]
        public void Test_DateTime_Select()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TestDate>();

                db.Insert(new TestDate {
                    Name = "Test name", 
                    ExpiryDate = DateTime.UtcNow.AddHours(1)
                });

                var result = db.Select<TestDate>(q => q.ExpiryDate > DateTime.UtcNow);

                Assert.That(result.Count, Is.EqualTo(1));
            }
        }         
    }
}