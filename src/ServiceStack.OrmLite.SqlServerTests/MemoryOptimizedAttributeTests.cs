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
    
    [SqlServerMemoryOptimized]
    public class TypeWithMemTableNoDurability
        {
        [PrimaryKey]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [SqlServerMemoryOptimized(SqlServerDurability.SchemaOnly)]
    public class TypeWithMemTableSchemaOnlyDurability
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [SqlServerMemoryOptimized(SqlServerDurability.SchemaAndData)]
    public class TypeWithMemTableSchemaAndDataDurability
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}