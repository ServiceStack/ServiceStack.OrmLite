using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServerTests
{
    [TestFixture]
    public class ForeignKeyAttributeTests : OrmLiteTestBase
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            using (var dbConn = ConnectionString.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<ReferencedType>(true);
            }
        }

        [Test]
        public void CanCreateSimpleForeignKey()
        {
            using (var dbConn = ConnectionString.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<TypeWithSimpleForeignKey>(true);
            }
        }

        [Test]
        public void CanCreateForeignWithOnDeleteCascade()
        {
            using (var dbConn = ConnectionString.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<TypeWithOnDeleteCascade>(true);
            }
        }

        [Test]
        public void CascadesOnDelete()
        {
            using (var dbConn = ConnectionString.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<TypeWithOnDeleteCascade>(true);

                dbCmd.Save(new ReferencedType { Id = 1 });
                dbCmd.Save(new TypeWithOnDeleteCascade { RefId = 1 });

                Assert.AreEqual(1, dbCmd.Select<ReferencedType>().Count);
                Assert.AreEqual(1, dbCmd.Select<TypeWithOnDeleteCascade>().Count);

                dbCmd.Delete<ReferencedType>(r => r.Id == 1);

                Assert.AreEqual(0, dbCmd.Select<ReferencedType>().Count);
                Assert.AreEqual(0, dbCmd.Select<TypeWithOnDeleteCascade>().Count);
            }
        }

        [Test]
        public void CanCreateForeignWithOnDeleteCascadeAndOnUpdateCascade()
        {
            using (var dbConn = ConnectionString.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<TypeWithOnDeleteAndUpdateCascade>(true);
            }
        }

        [Test]
        public void CanCreateForeignWithOnDeleteNoAction()
        {
            using (var dbConn = ConnectionString.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<TypeWithOnDeleteNoAction>(true);
            }
        }

        [NUnit.Framework.Ignore("Not supported in SQL Server")]
        [Test]
        public void CanCreateForeignWithOnDeleteRestrict()
        {
            using (var dbConn = ConnectionString.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<TypeWithOnDeleteRestrict>(true);
            }
        }
        
        [Test]
        public void CanCreateForeignWithOnDeleteSetDefault()
        {
            using (var dbConn = ConnectionString.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<TypeWithOnDeleteSetDefault>(true);
            }
        }

        [Test]
        public void CanCreateForeignWithOnDeleteSetNull()
        {
            using (var dbConn = ConnectionString.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<TypeWithOnDeleteSetNull>(true);
            }
        }

        [TestFixtureTearDown]
        public void TearDwon()
        {
            using (var dbConn = ConnectionString.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.DropTable<TypeWithOnDeleteAndUpdateCascade>();
                dbCmd.DropTable<TypeWithOnDeleteSetNull>();
                dbCmd.DropTable<TypeWithOnDeleteSetDefault>();
                dbCmd.DropTable<TypeWithOnDeleteRestrict>();
                dbCmd.DropTable<TypeWithOnDeleteNoAction>();
                dbCmd.DropTable<TypeWithOnDeleteCascade>();
                dbCmd.DropTable<TypeWithSimpleForeignKey>();
                dbCmd.DropTable<ReferencedType>();
            }
        }
    }

    public class ReferencedType
    {
        public int Id { get; set; }
    }


    public class TypeWithSimpleForeignKey
    {
        [AutoIncrement]
        public int Id { get; set; }
        [References(typeof(ReferencedType))]
        public int RefId { get; set; }
    }

    public class TypeWithOnDeleteCascade
    {
        [AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(ReferencedType), OnDelete = "CASCADE")]
        public int? RefId { get; set; }
    }

    public class TypeWithOnDeleteAndUpdateCascade
    {
        [AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(ReferencedType), OnDelete = "CASCADE", OnUpdate = "CASCADE")]
        public int? RefId { get; set; }
    }

    public class TypeWithOnDeleteNoAction
    {
        [AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(ReferencedType), OnDelete = "NO ACTION")]
        public int? RefId { get; set; }
    }

    public class TypeWithOnDeleteRestrict
    {
        [AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(ReferencedType), OnDelete = "RESTRICT")]
        public int? RefId { get; set; }
    }

    public class TypeWithOnDeleteSetDefault
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Default(typeof(int), "17")]
        [ForeignKey(typeof(ReferencedType), OnDelete = "SET DEFAULT")]
        public int RefId { get; set; }
    }

    public class TypeWithOnDeleteSetNull
    {
        [AutoIncrement]
        public int Id { get; set; }

        [ForeignKey(typeof(ReferencedType), OnDelete = "SET NULL")]
        public int? RefId { get; set; }
    }
}