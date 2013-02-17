using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Common.Utils;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.SqlServer;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class SqlBuilderTests
    {
        [Alias("Users")]
        public class User 
        {
            [AutoIncrement]
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }

        private IDbConnection db;

        [SetUp]
        public void SetUp()
        {
            db = Config.OpenDbConnection();
            db.DropAndCreateTable<User>();
        }

        [TearDown]
        public void TearDown()
        {
        }
        
        [Test]
        public void BuilderSelectClause()
        {
            var rand = new Random(8675309);
            var data = new List<User>();
            for (var i = 0; i < 100; i++)
            {
                var nU = new User { Age = rand.Next(70), Id = i, Name = Guid.NewGuid().ToString() };
                data.Add(nU);
                db.Insert(nU);
                nU.Id = (int)db.GetLastInsertId();
            }

            var builder = new SqlBuilder();
            var justId = builder.AddTemplate("SELECT /**select**/ FROM Users");
            var all = builder.AddTemplate("SELECT /**select**/, Name, Age FROM Users");

            builder.Select("Id");

            var ids = db.Query<int>(justId.RawSql, justId.Parameters);
            var users = db.Query<User>(all.RawSql, all.Parameters);

            foreach (var u in data)
            {
                Assert.That(ids.Any(i => u.Id == i), "Missing ids in select");
                Assert.That(users.Any(a => a.Id == u.Id && a.Name == u.Name && a.Age == u.Age), "Missing users in select");
            }
        }

        [Test]
        public void BuilderTemplateWOComposition()
        {
            var builder = new SqlBuilder();
            var template = builder.AddTemplate("SELECT COUNT(*) FROM Users WHERE Age = @age", new { age = 5 });

            if (template.RawSql == null) throw new Exception("RawSql null");
            if (template.Parameters == null) throw new Exception("Parameters null");

            db.Insert(new User { Age = 5, Name = "Testy McTestington" });

            Assert.That(db.QueryScalar<int>(template.RawSql, template.Parameters), Is.EqualTo(1));
        }         
    }
}