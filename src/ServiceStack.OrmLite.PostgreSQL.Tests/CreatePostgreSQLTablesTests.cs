﻿using System;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Converters;
using ServiceStack.OrmLite.Tests;

namespace ServiceStack.OrmLite.PostgreSQL.Tests
{
    [TestFixture]
    public class CreatePostgreSQLTablesTests : OrmLiteTestBase
    {
        public CreatePostgreSQLTablesTests() : base(Dialect.PostgreSql) { }
        
        [Test]
        public void DropAndCreateTable_DropsTableAndCreatesTable()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<TestData>();
                db.CreateTable<TestData>();
                db.Insert<TestData>(new TestData { Id = Guid.NewGuid() });
                db.DropAndCreateTable<TestData>();
                db.Insert<TestData>(new TestData { Id = Guid.NewGuid() });
            }
        }


        [Test]
        public void can_create_tables_after_UseUnicode_or_DefaultStringLength_changed()
        {
            //first one passes
            _reCreateTheTable();

            //all of these pass now:
            var stringConverter = OrmLiteConfig.DialectProvider.GetStringConverter();
            stringConverter.UseUnicode = true;
            _reCreateTheTable();

            stringConverter.UseUnicode = false;
            _reCreateTheTable();

            stringConverter.StringLength = 98765;

            _reCreateTheTable();
        }

        private void _reCreateTheTable()
        {
            using(var db = OpenDbConnection()) {
                db.CreateTable<CreatePostgreSQLTablesTests_dummy_table>(true);
            }
        }

        private class CreatePostgreSQLTablesTests_dummy_table
        {
            [AutoIncrement]
            public int Id { get; set; }

            public String StringNoExplicitLength { get; set; }

            [StringLength(100)]
            public String String100Characters { get; set; }
        }
          
        [Test]
        public void can_create_same_table_in_multiple_schemas_based_on_conn_string_search_path()
        {
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(ConnectionString);
            var schema1 = "schema_1";
            var schema2 = "schema_2";
            using (var db = OpenDbConnection())
            {
                CreateSchemaIfNotExists(db, schema1);
                CreateSchemaIfNotExists(db, schema2);
            }

            builder.SearchPath = schema1;
            using (var dbS1 = builder.ToString().OpenDbConnection())
            {
                dbS1.DropTable<CreatePostgreSQLTablesTests_dummy_table>();
                dbS1.CreateTable<CreatePostgreSQLTablesTests_dummy_table>();
                Assert.That(dbS1.Count<CreatePostgreSQLTablesTests_dummy_table>(), Is.EqualTo(0));
                dbS1.DropTable<CreatePostgreSQLTablesTests_dummy_table>();
            }
            builder.SearchPath = schema2;

            using (var dbS2 = builder.ToString().OpenDbConnection())
            {
                dbS2.DropTable<CreatePostgreSQLTablesTests_dummy_table>();
                dbS2.CreateTable<CreatePostgreSQLTablesTests_dummy_table>();
                Assert.That(dbS2.Count<CreatePostgreSQLTablesTests_dummy_table>(), Is.EqualTo(0));
                dbS2.DropTable<CreatePostgreSQLTablesTests_dummy_table>();
            }

        }

        public class TestData
        {
            [PrimaryKey]
            public Guid Id { get; set; }

            public string Name { get; set; }

            public string Surname { get; set; }
        }

        private void CreateSchemaIfNotExists(System.Data.IDbConnection db, string name)
        {
            string createSchemaSQL = @"DO $$
BEGIN

    IF NOT EXISTS(
        SELECT 1
          FROM INFORMATION_SCHEMA.SCHEMATA
          WHERE SCHEMA_NAME = '{0}'
      )
    THEN
      EXECUTE 'CREATE SCHEMA ""{0}""';
    END IF;

END
$$;"
                .Fmt(name);
            db.ExecuteSql(createSchemaSQL);
        }
    }
}
