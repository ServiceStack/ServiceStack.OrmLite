namespace ServiceStack.OrmLite.Tests.UseCase
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using NUnit.Framework;
    using DataAnnotations;
    using Sqlite;

    [TestFixture]
    public class PasswordUseCase {
        [TestFixtureSetUp]
        public void TestFixtureSetUp() {
            //Inject your database provider here
            //OrmLiteConfig.DialectProvider = new SqliteOrmLiteDialectProvider();
        }

        public class User {
            public long Id { get; set; }

            [Index]
            public string Name { get; set; }

            public DateTime CreatedDate { get; set; }
        }

        [Alias("Users")]
        public class User2 {
            [AutoIncrement]
            public long Id { get; set; }

            public long Value { get; set; }
        }

        [Test]
        public void Simple_CRUD_example() {
            var path = Config.SqliteFileDb;
            if (File.Exists(path))
                File.Delete(path);
            var connectionFactory = new OrmLiteConnectionFactory(path, SqliteOrmLiteDialectProvider.Instance.WithPassword("bob"));
            using (var db = connectionFactory.OpenDbConnection())
            {
                db.CreateTable<User>(true);

                db.InsertAll(new User { Id = 1, Name = "A", CreatedDate = DateTime.Now });
                db.InsertAll(new User { Id = 2, Name = "B", CreatedDate = DateTime.Now });
                db.InsertAll(new User { Id = 3, Name = "B", CreatedDate = DateTime.Now });

                var rowsB = db.SelectFmt<User>("Name = {0}", "B");
                var rowsB1 = db.Select<User>(user => user.Name == "B");

                Assert.That(rowsB, Has.Count.EqualTo(2));
                Assert.That(rowsB1, Has.Count.EqualTo(2));

                var rowIds = rowsB.ConvertAll(x => x.Id);
                Assert.That(rowIds, Is.EquivalentTo(new List<long> { 2, 3 }));

                rowsB.ForEach(x => db.Delete(x));

                rowsB = db.SelectFmt<User>("Name = {0}", "B");
                Assert.That(rowsB, Has.Count.EqualTo(0));

                var rowsLeft = db.Select<User>();
                Assert.That(rowsLeft, Has.Count.EqualTo(1));

                Assert.That(rowsLeft[0].Name, Is.EqualTo("A"));
            }
            File.Delete(path);

        }
    }
}