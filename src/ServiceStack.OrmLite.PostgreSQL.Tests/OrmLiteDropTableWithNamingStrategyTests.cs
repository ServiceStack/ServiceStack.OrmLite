using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.OrmLite.Tests;

namespace ServiceStack.OrmLite.PostgreSQL.Tests
{
    public class OrmLiteDropTableWithNamingStrategyTests
        : OrmLiteTestBase
    {
        
        [Test]
        public void Can_drop_TableWithNamigStrategy_table_PostgreSqlNamingStrategy()
        {
            OrmLiteConfig.DialectProvider.NamingStrategy = new PostgreSqlNamingStrategy();

            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithOnlyStringFields>(true);
                db.DropTable<ModelWithOnlyStringFields>();
                Assert.False(db.TableExists("model_with_only_string_fields"));
            }

            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();
        }
    }
}