using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServerTests
{
    [TestFixture]
    public class MemoryOptimizedAttributeTests : OrmLiteTestBase
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            OrmLiteConfig.DialectProvider = SqlServer2014Dialect.Provider;

            using (var dbConn = OpenDbConnection())
            {
                dbConn.DropTable<TypeWithMemTableNoDurability>();
                dbConn.DropTable<TypeWithMemTableSchemaOnlyDurability>();
                dbConn.DropTable<TypeWithMemTableSchemaAndDataDurability>();
            }
        }

        [Test]
        public void CanCreateMemoryOptimizedTable()
        {
            using (var dbConn = OpenDbConnection())
            {
                dbConn.CreateTable<TypeWithMemTableNoDurability>(true);
            }
        }

        [Test]
        public void CanCreateMemoryOptimizedTableWithSchemaOnlyDurability()
        {
            using (var dbConn = OpenDbConnection())
            {
                dbConn.CreateTable<TypeWithMemTableSchemaOnlyDurability>(true);
            }
        }

        [Test]
        public void CanCreateMemoryOptimizedTableWithSchemaAndDurability()
        {
            using (var dbConn = OpenDbConnection())
            {
                dbConn.CreateTable<TypeWithMemTableSchemaAndDataDurability>(true);
            }
        }
    }
    
    [MemoryOptimized]
    public class TypeWithMemTableNoDurability
        {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [MemoryOptimized(TableDurability.SCHEMA_ONLY)]
    public class TypeWithMemTableSchemaOnlyDurability
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [MemoryOptimized(TableDurability.SCHEMA_AND_DATA)]
    public class TypeWithMemTableSchemaAndDataDurability
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}