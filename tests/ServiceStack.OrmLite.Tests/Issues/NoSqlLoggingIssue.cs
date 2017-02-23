using NUnit.Framework;
using ServiceStack.Logging;
using ServiceStack.OrmLite.Tests.Shared;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class NoSqlLoggingIssue : OrmLiteTestBase
    {
        //NUnit in .NET Core not liking resetting AppDomain after each TestFixture
#if !NETCORE
        [Test]
        public void Does_log_SQL_Insert_for_Saves_with_Auto_Ids()
        {
            var sbLogFactory = new StringBuilderLogFactory();
            LogManager.LogFactory = sbLogFactory;

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<PersonWithAutoId>();

                db.Save(new PersonWithAutoId { Id = 1, FirstName = "first", LastName = "last", Age = 27 });
            }

            var sql = sbLogFactory.GetLogs();

            Assert.That(sql, Does.Contain("INSERT INTO"));
            LogManager.LogFactory = null;
        }
#endif
    }
}