using System;
using System.Data.SqlClient;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServerTests.TableOptions
{
    [TestFixture]
    public class SqlServer2014MemoryOptimizedAttributeTests : SqlServer2014TableOptionsOrmLiteTestBase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            base.TestFixtureSetUp();

            if (Db.TableExists<TypeWithMemTableNoDurability>())
                Db.DropTable<TypeWithMemTableNoDurability>();

            if (Db.TableExists<TypeWithMemTableSchemaOnlyDurability>())
                Db.DropTable<TypeWithMemTableSchemaOnlyDurability>();

            if (Db.TableExists<TypeWithMemTableSchemaAndDataDurability>())
                Db.DropTable<TypeWithMemTableSchemaAndDataDurability>();

            if (Db.TableExists<TypeWithMemTableWithCollatedHashIndex>())
                Db.DropTable<TypeWithMemTableWithCollatedHashIndex>();
        }

        [Test]
        public void Can_Create_Memory_Optimized_Table()
        {
            Db.CreateTable<TypeWithMemTableNoDurability>(true);

            var sql = Db.GetLastSql();

            Assert.IsTrue(sql.Contains("MEMORY_OPTIMIZED = ON"));
            Assert.IsFalse(sql.Contains("DURABILITY = SCHEMA_ONLY"));
            Assert.IsFalse(sql.Contains("DURABILITY = SCHEMA_AND_DATA"));

            var name = "Test 1";
            var id = Db.Insert(new TypeWithMemTableNoDurability { Name = name }, selectIdentity: true);
            var fromDb = Db.SingleById<TypeWithMemTableNoDurability>(id);

            Assert.AreEqual(name, fromDb.Name);
        }

        [Test]
        public void Can_Create_Memory_Optimized_Table_With_Schema_Only_Durability()
        {
            Db.CreateTable<TypeWithMemTableSchemaOnlyDurability>(true);

            var sql = Db.GetLastSql();

            Assert.IsTrue(sql.Contains("MEMORY_OPTIMIZED = ON"));
            Assert.IsTrue(sql.Contains("DURABILITY = SCHEMA_ONLY"));
            Assert.IsFalse(sql.Contains("DURABILITY = SCHEMA_AND_DATA"));

            var name = "Test 2";
            var id = Db.Insert(new TypeWithMemTableSchemaOnlyDurability { Name = name }, selectIdentity: true);
            var fromDb = Db.SingleById<TypeWithMemTableSchemaOnlyDurability>(id);

            Assert.AreEqual(name, fromDb.Name);
        }

        [Test]
        public void Can_Create_Memory_Optimized_Table_With_Schema_And_Data_Durability()
        {
            Db.CreateTable<TypeWithMemTableSchemaAndDataDurability>(true);

            var sql = Db.GetLastSql();

            Assert.IsTrue(sql.Contains("MEMORY_OPTIMIZED = ON"));
            Assert.IsFalse(sql.Contains("DURABILITY = SCHEMA_ONLY"));
            Assert.IsTrue(sql.Contains("DURABILITY = SCHEMA_AND_DATA"));

            var name = "Test 3";
            var id = Db.Insert(new TypeWithMemTableSchemaAndDataDurability { Name = name }, selectIdentity: true);
            var fromDb = Db.SingleById<TypeWithMemTableSchemaAndDataDurability>(id);

            Assert.AreEqual(name, fromDb.Name);
        }

        [Test]
        public void Can_not_Create_Memory_Optimized_Table_Without_BIN2_Collate_Hash_Index()
        {
            try
            {
                Db.CreateTable<TypeWithMemTableWithoutBIN2HashIndex>(true);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<SqlException>(ex);
            }
        }

        [Test]
        public void Can_Create_Memory_Optimized_Table_With_Collated_Hash_Index()
        {
            var name = "Test 5";
            var index = "Test Index";

            Db.CreateTable<TypeWithMemTableWithCollatedHashIndex>(true);

            var sql = Db.GetLastSql();

            Assert.IsTrue(sql.Contains("MEMORY_OPTIMIZED = ON"));
            Assert.IsTrue(sql.Contains("COLLATE"));

            var id = Db.Insert(new TypeWithMemTableWithCollatedHashIndex {Name = name, Index = index }, selectIdentity: true);
            var fromDb = Db.SingleById<TypeWithMemTableWithCollatedHashIndex>(id);

            Assert.AreEqual(name, fromDb.Name);
            Assert.AreEqual(index, fromDb.Index);
        }
    }

    public class TypeWithNoMemOptimization
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [SqlServerMemoryOptimized]
    public class TypeWithMemTableNoDurability : TypeWithNoMemOptimization { } 

    [SqlServerMemoryOptimized(SqlServerDurability.SchemaOnly)]
    public class TypeWithMemTableSchemaOnlyDurability : TypeWithNoMemOptimization { }

    [SqlServerMemoryOptimized(SqlServerDurability.SchemaAndData)]
    public class TypeWithMemTableSchemaAndDataDurability : TypeWithNoMemOptimization { }

    [SqlServerMemoryOptimized]
    public class TypeWithMemTableWithoutBIN2HashIndex : TypeWithNoMemOptimization
    {
        [StringLength(25)]
        [SqlServerBucketCount(1000)]
        public string Index { get; set; }
    }

    [SqlServerMemoryOptimized]
    public class TypeWithMemTableWithCollatedHashIndex : TypeWithNoMemOptimization
    {
        [StringLength(25)]
        [SqlServerCollate("Latin1_General_100_BIN2")]
        [SqlServerBucketCount(1000)]
        public string Index { get; set; }
    }
}