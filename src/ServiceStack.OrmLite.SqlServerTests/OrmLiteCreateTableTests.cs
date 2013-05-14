using System;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServerTests
{
    [TestFixture]
    public class OrmLiteCreateTableTests : OrmLiteTestBase
    {
        [Test]
        public void Can_create_TypeWithDefaultPrimaryKeyName_table_with_specified_DefaultPrimaryKeyName()
        {
            var createTableSql = OrmLiteConfig.DialectProvider.ToCreateTableStatement(typeof(TypeWithDefaultPrimaryKeyName));

            Log("createTableSql: " + createTableSql);
            Assert.That(createTableSql.Contains("CONSTRAINT [PK_TypeWithDefaultPrimaryKeyName_Id] PRIMARY KEY"), Is.True);
        }

        [Test]
        public void Can_create_TypeWithCustomPrimaryKeyName_table_with_specified_CustomPrimaryKeyName()
        {
            var createTableSql = OrmLiteConfig.DialectProvider.ToCreateTableStatement(typeof(TypeWithCustomPrimaryKeyName));

            Log("createTableSql: " + createTableSql);
            Assert.That(createTableSql.Contains("CONSTRAINT [primary_key_TypeWithCustomPrimaryKeyName_Id_column] PRIMARY KEY"), Is.True);
        }

        public class TypeWithDefaultPrimaryKeyName
        {
            [AutoIncrement]
            public int Id { get; set; }
        }

        public class TypeWithCustomPrimaryKeyName
        {
            [AutoIncrement]
            [PrimaryKeyName("primary_key_{0}_{1}_column")]
            public int Id { get; set; }
        }
    }
}