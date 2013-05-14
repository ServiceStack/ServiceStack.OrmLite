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
        public class TypeWithDefaultPrimaryKeyName
        {
            [AutoIncrement]
            public int Id { get; set; }
        }
    }
}