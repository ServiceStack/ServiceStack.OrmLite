using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.OrmLite.Tests
{
    public class OrmLiteDropTableWithNamingStrategyTests
        : OrmLiteTestBase
    {
        private INamingStrategy PreviousNamingStrategy { get; set; }

        [SetUp]
        public void SetUp()
        {
            PreviousNamingStrategy = OrmLiteConfig.DialectProvider.NamingStrategy;
        }

        [TearDown]
        public void TearDown()
        {
            OrmLiteConfig.DialectProvider.NamingStrategy = PreviousNamingStrategy;
        }

        [Test]
        public void Can_drop_TableWithNamigStrategy_table_prefix()
        {
            OrmLiteConfig.DialectProvider.NamingStrategy = new PrefixNamingStrategy
            {
                TablePrefix = "tab_",
                ColumnPrefix = "col_"
            };

            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithOnlyStringFields>(true);

                db.DropTable<ModelWithOnlyStringFields>();

                Assert.False(db.TableExists("tab_ModelWithOnlyStringFields"));
            }
        }

        [Test]
        public void Can_drop_TableWithNamigStrategy_table_lowered()
        {
            OrmLiteConfig.DialectProvider.NamingStrategy = new LowercaseNamingStrategy();

            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithOnlyStringFields>(true);

                db.DropTable<ModelWithOnlyStringFields>();

                Assert.False(db.TableExists("modelwithonlystringfields"));
            }
        }


        [Test]
        public void Can_drop_TableWithNamigStrategy_table_nameUnderscoreCompound()
        {
            OrmLiteConfig.DialectProvider.NamingStrategy = new UnderscoreSeparatedCompoundNamingStrategy();

            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithOnlyStringFields>(true);

                db.DropTable<ModelWithOnlyStringFields>();

                Assert.False(db.TableExists("model_with_only_string_fields"));
            }
        }
    }
}