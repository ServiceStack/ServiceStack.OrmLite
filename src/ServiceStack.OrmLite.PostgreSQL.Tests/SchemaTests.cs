using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class SchemaTests : OrmLiteTestBase
    {
        [Alias("Users")]
        [Schema("TestSchema")]
        public class User
        {
            [AutoIncrement]
            public int Id { get; set; }

            [Index]
            public string Name { get; set; }

            public DateTime CreatedDate { get; set; }
        }

        private void CreateSchemaIfNotExists(System.Data.IDbConnection db)
        {
            const string createSchemaSQL = @"DO $$
BEGIN

    IF NOT EXISTS(
        SELECT schema_name
          FROM information_schema.schemata
          WHERE schema_name = 'TestSchema'
      )
    THEN
      EXECUTE 'CREATE SCHEMA ""TestSchema""';
    END IF;

END
$$;";
            db.ExecuteSql(createSchemaSQL);
        }

        [Test]
        public void Can_Create_Tables_With_Schema_in_PostgreSQL()
        {
            using (var db = OpenDbConnection())
            using (var dbCmd = db.CreateCommand())
            {
                CreateSchemaIfNotExists(db);
                db.DropAndCreateTable<User>();

                var tables = db.GetFirstColumn<string>
                    (@"SELECT '[' || n.nspname || '].[' || c.relname ||']' FROM pg_class c LEFT JOIN pg_namespace n ON n.oid = c.relnamespace WHERE c.relname = 'Users' AND n.nspname = 'TestSchema'");
                
                //PostgreSQL dialect should create the table in the schema
                Assert.That(tables.Contains("[TestSchema].[Users]"));
            }
        }

        [Test]
        public void Can_Perform_CRUD_Operations_On_Table_With_Schema()
        {
            using (var db = OpenDbConnection())
            using (var dbCmd = db.CreateCommand())
            {
                CreateSchemaIfNotExists(db);
                db.CreateTable<User>(true);

                db.Insert(new User { Id = 1, Name = "A", CreatedDate = DateTime.Now });
                db.Insert(new User { Id = 2, Name = "B", CreatedDate = DateTime.Now });
                db.Insert(new User { Id = 3, Name = "B", CreatedDate = DateTime.Now });

                var lastInsertId = db.GetLastInsertId();
                Assert.That(lastInsertId, Is.GreaterThan(0));

                var rowsB = db.Select<User>("\"Name\" = {0}", "B");
                Assert.That(rowsB, Has.Count.EqualTo(2));

                var rowIds = rowsB.ConvertAll(x => x.Id);
                Assert.That(rowIds, Is.EquivalentTo(new List<long> { 2, 3 }));

                rowsB.ForEach(x => db.Delete(x));

                rowsB = db.Select<User>("\"Name\" = {0}", "B");
                Assert.That(rowsB, Has.Count.EqualTo(0));

                var rowsLeft = db.Select<User>();
                Assert.That(rowsLeft, Has.Count.EqualTo(1));

                Assert.That(rowsLeft[0].Name, Is.EqualTo("A"));
            }
        }
    }
}
