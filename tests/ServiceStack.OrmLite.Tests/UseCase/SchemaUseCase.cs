﻿using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.SqlServer;
using ServiceStack.OrmLite.Sqlite;

namespace ServiceStack.OrmLite.Tests.UseCase
{
    [TestFixture]
    public class SchemaUseCase
    {
        [Alias("Users")]
        [Schema("Security")]
        public class User
        {
            [AutoIncrement]
            public int Id { get; set; }

            [DataAnnotations.Index]
            public string Name { get; set; }

            public DateTime CreatedDate { get; set; }
        }

        [Test]
        public void Can_Create_Tables_With_Schema_In_Sqlite()
        {
            OrmLiteConfig.DialectProvider = new SqliteOrmLiteDialectProvider();

            using (IDbConnection db = ":memory:".OpenDbConnection())
            using (IDbCommand dbCmd = db.CreateCommand())
            {
                dbCmd.CreateTable<User>(true);

                var tables =
                    dbCmd.GetFirstColumn<string>
                        (@"SELECT name FROM sqlite_master WHERE type='table';");

                //sqlite dialect should just concatenate the schema and table name to create a unique table name
                Assert.That(tables.Contains("Security_Users"));
            }
        }

        private void CreateSchemaIfNotExists(IDbCommand dbCmd)
        {
            //in Sql2008, CREATE SCHEMA must be the first statement in a batch
            const string createSchemaSQL = @"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Security')
                                        BEGIN
                                        EXEC( 'CREATE SCHEMA Security' );
                                        END";
            dbCmd.CommandText = createSchemaSQL;
            dbCmd.ExecuteNonQuery();
        }

        [Test]
        public void Can_Create_Tables_With_Schema_in_SqlServer()
        {
            var dbFactory = new OrmLiteConnectionFactory(
                @"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\App_Data\Database1.mdf;Integrated Security=True;User Instance=True",
                SqlServerOrmLiteDialectProvider.Instance);
            using (IDbConnection db = dbFactory.OpenDbConnection())
            using (IDbCommand dbCmd = db.CreateCommand())
            {
                CreateSchemaIfNotExists(dbCmd);
                dbCmd.CreateTable<User>(true);

                var tables = dbCmd.GetFirstColumn<string>
                    (@"SELECT '['+SCHEMA_NAME(schema_id)+'].['+name+']' AS SchemaTable FROM sys.tables");

                //sql server dialect should create the table in the schema
                Assert.That(tables.Contains("[Security].[Users]"));
            }
        }

        [Test]
        public void Can_Perform_CRUD_Operations_On_Table_With_Schema()
        {
            var dbFactory = new OrmLiteConnectionFactory(
                @"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\App_Data\Database1.mdf;Integrated Security=True;User Instance=True",
                SqlServerOrmLiteDialectProvider.Instance);
            using (IDbConnection db = dbFactory.OpenDbConnection())
            using (IDbCommand dbCmd = db.CreateCommand())
            {
                CreateSchemaIfNotExists(dbCmd);
                dbCmd.CreateTable<User>(true);

                dbCmd.Insert(new User { Id = 1, Name = "A", CreatedDate = DateTime.Now });
                dbCmd.Insert(new User { Id = 2, Name = "B", CreatedDate = DateTime.Now });
                dbCmd.Insert(new User { Id = 3, Name = "B", CreatedDate = DateTime.Now });

                var lastInsertId = dbCmd.GetLastInsertId();
                Assert.That(lastInsertId, Is.GreaterThan(0));

                var rowsB = dbCmd.Select<User>("Name = {0}", "B");
                Assert.That(rowsB, Has.Count.EqualTo(2));

                var rowIds = rowsB.ConvertAll(x => x.Id);
                Assert.That(rowIds, Is.EquivalentTo(new List<long> { 2, 3 }));

                rowsB.ForEach(x => dbCmd.Delete(x));

                rowsB = dbCmd.Select<User>("Name = {0}", "B");
                Assert.That(rowsB, Has.Count.EqualTo(0));

                var rowsLeft = dbCmd.Select<User>();
                Assert.That(rowsLeft, Has.Count.EqualTo(1));

                Assert.That(rowsLeft[0].Name, Is.EqualTo("A"));
            }
        }
    }
}