using NUnit.Framework;
using ServiceStack.OrmLite.Tests.UseCase;

namespace ServiceStack.OrmLite.Tests
{
    [Ignore, Explicit("OneOff Tasks")]
    public class AdminTasks
        : OrmLiteTestBase
    {
        [Test]
        public void Clean_Database()
        {
            using (var db = OpenDbConnection())
            {
                //db.DropTable<JoinSqlBuilderTests.User>();
                //db.DropTable<SqlBuilderTests.User>();
                //db.DropTable<AliasedFieldUseCase.User>();
                //db.DropTable<SchemaUseCase.User>();
                //db.DropTable<SimpleUseCase.User>();
            }
        }
    }
}