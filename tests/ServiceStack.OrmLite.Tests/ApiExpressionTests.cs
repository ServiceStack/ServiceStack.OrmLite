using System;
using System.Data;
using System.Diagnostics;
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
        public int? Age { get; set; }

        public Person() { }
        public Person(int id, string firstName, string lastName, int age)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
        }
    }

    public class AutoIdPerson
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Age { get; set; }
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
            db.DropAndCreateTable<AutoIdPerson>();

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
            db.InsertAll(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });
            db.GetLastSql().Print();


            db.UpdateAll(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27});
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'Jimi',\"LastName\" = 'Hendrix',\"Age\" = 27 WHERE \"Id\" = 1"));
            db.GetLastSql().Print();

            db.Update(new Person { Id = 1, FirstName = "JJ" }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"Id\" = 1,\"FirstName\" = 'JJ',\"LastName\" = NULL,\"Age\" = NULL WHERE (\"LastName\" = 'Hendrix')"));
            db.GetLastSql().Print();

            db.Update<Person>(new { FirstName = "JJ" }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'JJ' WHERE (\"LastName\" = 'Hendrix')"));
            db.GetLastSql().Print();

            db.UpdateNonDefaults(new Person { FirstName = "JJ" }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'JJ' WHERE (\"LastName\" = 'Hendrix')"));
            db.GetLastSql().Print();


            db.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName);
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'JJ'"));
            db.GetLastSql().Print();

            db.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'JJ' WHERE (\"LastName\" = 'Hendrix')"));
            db.GetLastSql().Print();


            db.UpdateOnly(new Person { FirstName = "JJ", LastName = "Hendo" }, ev => ev.Update(p => p.FirstName));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'JJ'"));
            db.GetLastSql().Print();

            db.UpdateOnly(new Person { FirstName = "JJ" }, ev => ev.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'JJ' WHERE (\"FirstName\" = 'Jimi')"));
            db.GetLastSql().Print();


            db.UpdateFmt<Person>(set: "FirstName = {0}".Params("JJ"), where: "LastName = {0}".Params("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'"));
            db.GetLastSql().Print();

            db.UpdateFmt(table: "Person", set: "FirstName = {0}".Params("JJ"), where: "LastName = {0}".Params("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'"));
            db.GetLastSql().Print();


            db.InsertOnly(new AutoIdPerson { FirstName = "Amy" }, ev => ev.Insert(p => new { p.FirstName }));
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"AutoIdPerson\" (\"FirstName\") VALUES ('Amy')"));
            db.GetLastSql().Print();


            db.Delete<Person>(p => p.Age == 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));
            db.GetLastSql().Print();

            db.Delete<Person>(ev => ev.Where(p => p.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));
            db.GetLastSql().Print();

            db.Delete(OrmLiteConfig.DialectProvider.SqlExpression<Person>().Where(p => p.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));
            db.GetLastSql().Print();

            db.DeleteFmt<Person>(where: "Age = {0}".Params(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));
            db.GetLastSql().Print();

            db.DeleteFmt(table: "Person", where: "Age = {0}".Params(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));
            db.GetLastSql().Print();
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
}