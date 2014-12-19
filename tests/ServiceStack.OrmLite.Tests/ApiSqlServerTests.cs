using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.Shared;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class ApiSqlServerTests
        : OrmLiteTestBase
    {
        private IDbConnection db;

        [SetUp]
        public void SetUp()
        {
            SuppressIfOracle("SQL Server tests");
            db = CreateSqlServerDbFactory().OpenDbConnection();
            db.DropAndCreateTable<Person>();
            db.DropAndCreateTable<PersonWithAutoId>();
        }

        [TearDown]
        public void TearDown()
        {
            db.Dispose();
        }

        [Test]
        public void API_SqlServer_Examples()
        {
            db.Insert(Person.Rockstars);

            db.Select<Person>(x => x.Age > 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" > 40)"));

            db.Select<Person>(q => q.Where(x => x.Age > 40).OrderBy(x => x.Id));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" > 40)\nORDER BY \"Id\""));

            db.Select<Person>(q => q.Where(x => x.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" > 40)"));

            db.Select(db.From<Person>().Where(x => x.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" > 40)"));

            db.Single<Person>(x => x.Age == 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT TOP 1 \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" = 42)"));

            db.Single<Person>(q => q.Where(x => x.Age == 42));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT TOP 1 \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" = 42)"));

            db.Single(db.From<Person>().Where(x => x.Age == 42));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT TOP 1 \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" = 42)"));

            db.Scalar<Person, int>(x => Sql.Max(x.Age));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Max(\"Age\") \nFROM \"Person\""));

            db.Scalar<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Max(\"Age\") \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            db.Count<Person>(x => x.Age < 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            db.Count(db.From<Person>().Where(x => x.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));


            db.Select<Person>("Age > 40");
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > 40"));

            db.Select<Person>("SELECT * FROM Person WHERE Age > 40");
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age > 40"));

            db.Select<Person>("Age > @age", new { age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > @age"));

            db.Select<Person>("SELECT * FROM Person WHERE Age > @age", new { age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age > @age"));

            db.Select<Person>("Age > @age", new Dictionary<string, object> { { "age", 40 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > @age"));

            db.Select<Person>("SELECT * FROM Person WHERE Age > @age", new Dictionary<string, object> { { "age", 40 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age > @age"));

            db.SelectFmt<Person>("Age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > 40"));

            db.SelectFmt<Person>("SELECT * FROM Person WHERE Age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age > 40"));

            db.Select<EntityWithId>(typeof(Person));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\" FROM \"Person\""));

            db.SelectFmt<EntityWithId>(typeof(Person), "Age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\" FROM \"Person\" WHERE Age > 40"));

            db.Where<Person>("Age", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Age\" = @Age"));

            db.Where<Person>(new { Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Age\" = @Age"));

            db.SelectByIds<Person>(new[] { 1, 2, 3 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Id\" IN (1,2,3)"));

            db.SelectNonDefaults(new Person { Id = 1 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Id\" = @Id"));

            db.SelectNonDefaults("Age > @Age", new Person { Age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > @Age"));

            db.SelectLazy<Person>().ToList();
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\""));

            db.SelectLazy<Person>("Age > @age", new { age = 40 }).ToList();
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > @age"));

            db.SelectLazyFmt<Person>("Age > {0}", 40).ToList();
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > 40"));

            db.WhereLazy<Person>(new { Age = 27 }).ToList();
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Age\" = @Age"));

            db.SingleById<Person>(1);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Id\" = @Id"));

            db.Single<Person>(new { Age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Age\" = @Age"));

            db.Single<Person>("Age = @age", new { age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age = @age"));

            db.SingleFmt<Person>("Age = {0}", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age = 42"));

            db.SingleById<Person>(1);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Id\" = @Id"));

            db.SingleWhere<Person>("Age", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Age\" = @Age"));

            db.Scalar<int>(db.From<Person>().Select(Sql.Count("*")).Where(q => q.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"Person\"\nWHERE (\"Age\" > 40)"));
            db.Scalar<int>(db.From<Person>().Select(x => Sql.Count("*")).Where(q => q.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Count(*) \nFROM \"Person\"\nWHERE (\"Age\" > 40)"));

            db.Scalar<int>("SELECT COUNT(*) FROM Person WHERE Age > @age", new { age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM Person WHERE Age > @age"));

            db.ScalarFmt<int>("SELECT COUNT(*) FROM Person WHERE Age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM Person WHERE Age > 40"));

            db.Column<string>(db.From<Person>().Select(x => x.LastName).Where(q => q.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"LastName\" \nFROM \"Person\"\nWHERE (\"Age\" = 27)"));

            db.Column<string>("SELECT LastName FROM Person WHERE Age = @age", new { age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT LastName FROM Person WHERE Age = @age"));

            db.ColumnFmt<string>("SELECT LastName FROM Person WHERE Age = {0}", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT LastName FROM Person WHERE Age = 27"));

            db.ColumnDistinct<int>(db.From<Person>().Select(x => x.Age).Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            db.ColumnDistinct<int>("SELECT Age FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age FROM Person WHERE Age < @age"));

            db.ColumnDistinctFmt<int>("SELECT Age FROM Person WHERE Age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age FROM Person WHERE Age < 50"));

            db.Lookup<int, string>(db.From<Person>().Select(x => new { x.Age, x.LastName }).Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Age\",\"LastName\" \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            db.Lookup<int, string>("SELECT Age, LastName FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age, LastName FROM Person WHERE Age < @age"));

            db.LookupFmt<int, string>("SELECT Age, LastName FROM Person WHERE Age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age, LastName FROM Person WHERE Age < 50"));

            db.Dictionary<int, string>(db.From<Person>().Select(x => new { x.Id, x.LastName }).Where(x => x.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\",\"LastName\" \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            db.Dictionary<int, string>("SELECT Id, LastName FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Id, LastName FROM Person WHERE Age < @age"));

            db.DictionaryFmt<int, string>("SELECT Id, LastName FROM Person WHERE Age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Id, LastName FROM Person WHERE Age < 50"));

            db.Exists<Person>(x => x.Age < 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            db.Exists(db.From<Person>().Where(x => x.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            db.Exists<Person>(new { Age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Age\" = @Age"));

            db.Exists<Person>("Age = @age", new { age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age = @age"));
            db.Exists<Person>("SELECT * FROM Person WHERE Age = @age", new { age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age = @age"));

            db.ExistsFmt<Person>("Age = {0}", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age = 42"));
            db.ExistsFmt<Person>("SELECT * FROM Person WHERE Age = {0}", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age = 42"));

            db.SqlList<Person>(db.From<Person>().Select("*").Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            db.SqlList<Person>("SELECT * FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age < @age"));

            db.SqlList<Person>("SELECT * FROM Person WHERE Age < @age", new Dictionary<string, object> { { "age", 50 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM Person WHERE Age < @age"));

            db.SqlColumn<string>(db.From<Person>().Select(x => x.LastName).Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"LastName\" \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            db.SqlColumn<string>("SELECT LastName FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT LastName FROM Person WHERE Age < @age"));

            db.SqlColumn<string>("SELECT LastName FROM Person WHERE Age < @age", new Dictionary<string, object> { { "age", 50 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT LastName FROM Person WHERE Age < @age"));

            db.SqlScalar<int>(db.From<Person>().Select(Sql.Count("*")).Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            db.SqlScalar<int>("SELECT COUNT(*) FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM Person WHERE Age < @age"));

            db.SqlScalar<int>("SELECT COUNT(*) FROM Person WHERE Age < @age", new Dictionary<string, object> { { "age", 50 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM Person WHERE Age < @age"));

            var rowsAffected = db.ExecuteNonQuery("UPDATE Person SET LastName={0} WHERE Id={1}".SqlFmt("WaterHouse", 7));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE Person SET LastName='WaterHouse' WHERE Id=7"));

            rowsAffected = db.ExecuteNonQuery("UPDATE Person SET LastName=@name WHERE Id=@id", new { name = "WaterHouse", id = 7 });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE Person SET LastName=@name WHERE Id=@id"));


            db.Insert(new Person { Id = 7, FirstName = "Amy", LastName = "Winehouse", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"Person\" (\"Id\",\"FirstName\",\"LastName\",\"Age\") VALUES (@Id,@FirstName,@LastName,@Age)"));

            db.Insert(new Person { Id = 8, FirstName = "Tupac", LastName = "Shakur", Age = 25 },
                      new Person { Id = 9, FirstName = "Tupac", LastName = "Shakur2", Age = 26 });

            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"Person\" (\"Id\",\"FirstName\",\"LastName\",\"Age\") VALUES (@Id,@FirstName,@LastName,@Age)"));


            db.InsertAll(new[] { new Person { Id = 10, FirstName = "Biggie", LastName = "Smalls", Age = 24 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"Person\" (\"Id\",\"FirstName\",\"LastName\",\"Age\") VALUES (@Id,@FirstName,@LastName,@Age)"));

            db.InsertOnly(new PersonWithAutoId { FirstName = "Amy", Age = 27 }, q => q.Insert(p => new { p.FirstName, p.Age }));
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"PersonWithAutoId\" (\"FirstName\",\"Age\") VALUES ('Amy',27)"));

            db.InsertOnly(new PersonWithAutoId { FirstName = "Amy", Age = 27 }, q => db.From<PersonWithAutoId>().Insert(p => new { p.FirstName, p.Age }));
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"PersonWithAutoId\" (\"FirstName\",\"Age\") VALUES ('Amy',27)"));

            db.Update(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"=@FirstName, \"LastName\"=@LastName, \"Age\"=@Age WHERE \"Id\"=@Id"));

            db.Update(new Person { Id = 8, FirstName = "Tupac", LastName = "Shakur3", Age = 27 },
                      new Person { Id = 9, FirstName = "Tupac", LastName = "Shakur4", Age = 28 });

            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"=@FirstName, \"LastName\"=@LastName, \"Age\"=@Age WHERE \"Id\"=@Id"));

            db.Update(new[] { new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"=@FirstName, \"LastName\"=@LastName, \"Age\"=@Age WHERE \"Id\"=@Id"));

            db.UpdateAll(new[] { new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"=@FirstName, \"LastName\"=@LastName, \"Age\"=@Age WHERE \"Id\"=@Id"));

            db.Update(new Person { Id = 1, FirstName = "JJ", Age = 27 }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"Id\"=1, \"FirstName\"='JJ', \"LastName\"=NULL, \"Age\"=27 WHERE (\"LastName\" = 'Hendrix')"));

            db.Update<Person>(new { FirstName = "JJ" }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ' WHERE (\"LastName\" = 'Hendrix')"));

            db.UpdateNonDefaults(new Person { FirstName = "JJ" }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ' WHERE (\"LastName\" = 'Hendrix')"));

            db.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName);
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ'"));

            db.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ' WHERE (\"LastName\" = 'Hendrix')"));

            db.UpdateOnly(new Person { FirstName = "JJ", LastName = "Hendo" }, q => q.Update(p => p.FirstName));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ'"));

            db.UpdateOnly(new Person { FirstName = "JJ" }, q => q.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ' WHERE (\"FirstName\" = 'Jimi')"));

            db.UpdateFmt<Person>(set: "FirstName = {0}".SqlFmt("JJ"), where: "LastName = {0}".SqlFmt("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'"));

            db.UpdateFmt(table: "Person", set: "FirstName = {0}".SqlFmt("JJ"), where: "LastName = {0}".SqlFmt("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'"));

            db.Delete<Person>(new { FirstName = "Jimi", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"FirstName\"=@FirstName AND \"Age\"=@Age"));

            db.Delete<Person>(new { FirstName = "Jimi", Age = 27 },
                              new { FirstName = "Janis", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"FirstName\"=@FirstName AND \"Age\"=@Age"));

            db.Delete(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"Id\"=@Id AND \"FirstName\"=@FirstName AND \"LastName\"=@LastName AND \"Age\"=@Age"));

            db.DeleteNonDefaults(new Person { FirstName = "Jimi", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"FirstName\"=@FirstName AND \"Age\"=@Age"));

            db.DeleteNonDefaults(new Person { FirstName = "Jimi", Age = 27 },
                                 new Person { FirstName = "Janis", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"FirstName\"=@FirstName AND \"Age\"=@Age"));

            db.DeleteById<Person>(1);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"Id\" = @0"));

            db.DeleteByIds<Person>(new[] { 1, 2, 3 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE \"Id\" IN (1,2,3)"));

            db.DeleteFmt<Person>("Age = {0}", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));

            db.DeleteFmt(typeof(Person), "Age = {0}", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));

            db.Delete<Person>(p => p.Age == 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));

            db.Delete<Person>(q => q.Where(p => p.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));

            db.Delete(db.From<Person>().Where(p => p.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));

            db.DeleteFmt<Person>(where: "Age = {0}".SqlFmt(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));

            db.DeleteFmt(table: "Person", where: "Age = {0}".SqlFmt(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));

            db.Save(new Person { Id = 11, FirstName = "Amy", LastName = "Winehouse", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"Person\" (\"Id\",\"FirstName\",\"LastName\",\"Age\") VALUES (@Id,@FirstName,@LastName,@Age)"));
            db.Save(new Person { Id = 11, FirstName = "Amy", LastName = "Winehouse", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"=@FirstName, \"LastName\"=@LastName, \"Age\"=@Age WHERE \"Id\"=@Id"));

            db.Save(new Person { Id = 12, FirstName = "Amy", LastName = "Winehouse", Age = 27 },
                    new Person { Id = 13, FirstName = "Amy", LastName = "Winehouse", Age = 27 });

            db.SaveAll(new[]{ new Person { Id = 14, FirstName = "Amy", LastName = "Winehouse", Age = 27 },
                              new Person { Id = 15, FirstName = "Amy", LastName = "Winehouse", Age = 27 } });
        }

    }
}