using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

        public Person() { }
        public Person(int id, string firstName, string lastName, int age)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
        }
    }

    public class PersonWithAutoId
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
    }

    public class EntityWithId
    {
        public int Id { get; set; }
    }

    public class ApiExpressionTests
        : OrmLiteTestBase
    {
        private IDbConnection db;

        public Person[] People = new[] {
            new Person(1, "Jimi", "Hendrix", 27), 
            new Person(2, "Janis", "Joplin", 27), 
            new Person(3, "Jim", "Morrisson", 27), 
            new Person(4, "Kurt", "Cobain", 27),              
            new Person(5, "Elvis", "Presley", 42), 
            new Person(6, "Michael", "Jackson", 50), 
        };

        [SetUp]
        public void SetUp()
        {
            //db = OpenDbConnection();
            db = CreateSqlServerDbFactory().OpenDbConnection();
            db.DropAndCreateTable<Person>();
            db.DropAndCreateTable<PersonWithAutoId>();

            //People.ToList().ForEach(x => dbCmd.Insert(x));
        }

        [TearDown]
        public void TearDown()
        {
            db.Dispose();
        }

        [Test]
        public void API_Expression_Examples()
        {
            db.InsertAll(People);

            db.Select<Person>(x => x.Age > 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" > 40)"));

            db.Select<Person>(q => q.Where(x => x.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" > 40)"));

            db.Select(db.SqlExpression<Person>().Where(x => x.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" > 40)"));

            db.Single<Person>(x => x.Age == 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT TOP 1 \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" = 42)"));

            db.Single<Person>(q => q.Where(x => x.Age == 42));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT TOP 1 \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" = 42)"));

            db.Single(db.SqlExpression<Person>().Where(x => x.Age == 42));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT TOP 1 \"Id\", \"FirstName\", \"LastName\", \"Age\" \nFROM \"Person\"\nWHERE (\"Age\" = 42)"));

            db.Scalar<Person, int>(x => Sql.Max(x.Age));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Max(\"Age\") \nFROM \"Person\""));

            db.Scalar<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Max(\"Age\") \nFROM \"Person\"\nWHERE (\"Age\" < 50)"));

            db.Count<Person>(x => x.Age < 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM \"Person\" WHERE (\"Age\" < 50)"));

            db.Count(db.SqlExpression<Person>().Where(x => x.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM \"Person\" WHERE (\"Age\" < 50)"));


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

            db.SelectByExample(new Person { Id = 1 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Id\" = @Id"));

            db.SelectByExample("Age > @Age", new Person { Age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > @Age"));

            db.SelectLazy<Person>().ToList();
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\""));

            db.SelectLazy<Person>("Age > @age", new { age = 40 }).ToList();
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > @age"));

            db.SelectLazyFmt<Person>("Age > {0}", 40).ToList();
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE Age > 40"));

            db.WhereLazy<Person>(new { Age = 27 }).ToList();
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"FirstName\", \"LastName\", \"Age\" FROM \"Person\" WHERE \"Age\" = @Age"));

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

            db.Scalar<int>("SELECT COUNT(*) FROM Person WHERE Age > @age", new { age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM Person WHERE Age > @age"));

            db.ScalarFmt<int>("SELECT COUNT(*) FROM Person WHERE Age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM Person WHERE Age > 40"));

            db.Column<string>("SELECT LastName FROM Person WHERE Age = @age", new { age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT LastName FROM Person WHERE Age = @age"));

            db.ColumnFmt<string>("SELECT LastName FROM Person WHERE Age = {0}", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT LastName FROM Person WHERE Age = 27"));

            db.ColumnDistinct<int>("SELECT Age FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age FROM Person WHERE Age < @age"));

            db.ColumnDistinctFmt<int>("SELECT Age FROM Person WHERE Age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age FROM Person WHERE Age < 50"));

            db.Lookup<int, string>("SELECT Age, LastName FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age, LastName FROM Person WHERE Age < @age"));

            db.LookupFmt<int, string>("SELECT Age, LastName FROM Person WHERE Age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Age, LastName FROM Person WHERE Age < 50"));

            db.Dictionary<int, string>("SELECT Id, LastName FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Id, LastName FROM Person WHERE Age < @age"));

            db.DictionaryFmt<int, string>("SELECT Id, LastName FROM Person WHERE Age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Id, LastName FROM Person WHERE Age < 50"));

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


            db.SqlColumn<string>("SELECT LastName FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT LastName FROM Person WHERE Age < @age"));

            db.SqlColumn<string>("SELECT LastName FROM Person WHERE Age < @age", new Dictionary<string, object> { { "age", 50 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT LastName FROM Person WHERE Age < @age"));

            db.SqlScalar<int>("SELECT COUNT(*) FROM Person WHERE Age < @age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM Person WHERE Age < @age"));

            db.SqlScalar<int>("SELECT COUNT(*) FROM Person WHERE Age < @age", new Dictionary<string, object> { { "age", 50 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM Person WHERE Age < @age"));

            var rowsAffected = db.ExecuteNonQuery("UPDATE Person SET LastName={0} WHERE Id={1}".SqlFormat("WaterHouse", 7));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE Person SET LastName='WaterHouse' WHERE Id=7"));

            rowsAffected = db.ExecuteNonQuery("UPDATE Person SET LastName=@name WHERE Id=@id", new { name = "WaterHouse", id = 7 });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE Person SET LastName=@name WHERE Id=@id"));


            db.InsertAll(new Person { Id = 7, FirstName = "Amy", LastName = "Winehouse", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"Person\" (\"Id\",\"FirstName\",\"LastName\",\"Age\") VALUES (7,'Amy','Winehouse',27)"));

            db.UpdateAll(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='Jimi', \"LastName\"='Hendrix', \"Age\"=27 WHERE \"Id\"=1"));

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

            db.UpdateOnly(new Person { FirstName = "JJ", LastName = "Hendo" }, ev => ev.Update(p => p.FirstName));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ'"));

            db.UpdateOnly(new Person { FirstName = "JJ" }, ev => ev.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\"='JJ' WHERE (\"FirstName\" = 'Jimi')"));

            db.UpdateFmt<Person>(set: "FirstName = {0}".Params("JJ"), where: "LastName = {0}".Params("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'"));

            db.UpdateFmt(table: "Person", set: "FirstName = {0}".Params("JJ"), where: "LastName = {0}".Params("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'"));

            db.InsertOnly(new PersonWithAutoId { FirstName = "Amy", Age = 27 }, ev => ev.Insert(p => new { p.FirstName, p.Age }));
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"PersonWithAutoId\" (\"FirstName\",\"Age\") VALUES ('Amy',27)"));

            db.Delete<Person>(p => p.Age == 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));

            db.Delete<Person>(ev => ev.Where(p => p.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));

            db.Delete(OrmLiteConfig.DialectProvider.SqlExpression<Person>().Where(p => p.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));

            db.DeleteFmt<Person>(where: "Age = {0}".Params(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));

            db.DeleteFmt(table: "Person", where: "Age = {0}".Params(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));
        }

        [Test]
        public void Parameterized_query_tests()
        {
            db.InsertAll(People);

            //db.Select<Person>().PrintDump();
            //db.GetLastSql().Print();

            //db.Query<Person>(q => q);
            //db.GetLastSql().Print();
            db.SingleById<Person>(1).PrintDump();
            db.GetLastSql().Print();
        }

        [Explicit]
        public void Benchmark()
        {
            Measure(() => db.SingleById<Person>(1), times: 1).ToString().Print("Test 2: {0}");
            Measure(() => db.SingleById<Person>(1), times: 1).ToString().Print("Test 1: {0}");
        }

        static double MeasureFor(Action fn, int timeMinimum)
        {
            int iter = 0;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            long elapsed = 0;
            while (elapsed < timeMinimum)
            {
                fn();
                elapsed = watch.ElapsedMilliseconds;
                iter++;
            }
            return 1000.0 * elapsed / iter;
        }

        static double Measure(Action fn, int times = 10, int runfor = 2000, Action setup = null, Action warmup = null, Action teardown = null)
        {
            if (setup != null)
                setup();

            // Warmup for at least 100ms. Discard result.
            if (warmup == null)
                warmup = fn;

            MeasureFor(() => { warmup(); }, 100);

            // Run the benchmark for at least 2000ms.
            double result = MeasureFor(() =>
            {
                for (var i = 0; i < times; i++)
                    fn();
            }, runfor);

            if (teardown != null)
                teardown();

            return result;
        }
    }

    internal static class StaticExt
    {
        public static string PrintLastSql(this IDbConnection dbConn)
        {
            dbConn.GetLastSql().Print();
            "".Print();
            return dbConn.GetLastSql();
        }

    }
}