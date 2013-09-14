using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

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
            db = OpenDbConnection();
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
            db.Insert(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });
            Console.WriteLine(db.GetLastSql());


            db.Update(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27});
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'Jimi',\"LastName\" = 'Hendrix',\"Age\" = 27 WHERE \"Id\" = 1"));
            Console.WriteLine(db.GetLastSql());

            db.Update(new Person { Id = 1, FirstName = "JJ" }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"Id\" = 1,\"FirstName\" = 'JJ',\"LastName\" = NULL,\"Age\" = NULL WHERE (\"LastName\" = 'Hendrix')"));
            Console.WriteLine(db.GetLastSql());

            db.Update<Person>(new { FirstName = "JJ" }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'JJ' WHERE (\"LastName\" = 'Hendrix')"));
            Console.WriteLine(db.GetLastSql());

            db.UpdateNonDefaults(new Person { FirstName = "JJ" }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'JJ' WHERE (\"LastName\" = 'Hendrix')"));
            Console.WriteLine(db.GetLastSql());


            db.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName);
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'JJ'"));
            Console.WriteLine(db.GetLastSql());

            db.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'JJ' WHERE (\"LastName\" = 'Hendrix')"));
            Console.WriteLine(db.GetLastSql());


            db.UpdateOnly(new Person { FirstName = "JJ", LastName = "Hendo" }, ev => ev.Update(p => p.FirstName));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'JJ'"));
            Console.WriteLine(db.GetLastSql());

            db.UpdateOnly(new Person { FirstName = "JJ" }, ev => ev.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET \"FirstName\" = 'JJ' WHERE (\"FirstName\" = 'Jimi')"));
            Console.WriteLine(db.GetLastSql());


            db.Update<Person>(set: "FirstName = {0}".Params("JJ"), where: "LastName = {0}".Params("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'"));
            Console.WriteLine(db.GetLastSql());

            db.Update(table: "Person", set: "FirstName = {0}".Params("JJ"), where: "LastName = {0}".Params("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"Person\" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'"));
            Console.WriteLine(db.GetLastSql());


            db.InsertOnly(new AutoIdPerson { FirstName = "Amy" }, ev => ev.Insert(p => new { p.FirstName }));
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"AutoIdPerson\" (\"FirstName\") VALUES ('Amy')"));
            Console.WriteLine(db.GetLastSql());


            db.Delete<Person>(p => p.Age == 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));
            Console.WriteLine(db.GetLastSql());

            db.Delete<Person>(ev => ev.Where(p => p.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));
            Console.WriteLine(db.GetLastSql());

            db.Delete(OrmLiteConfig.DialectProvider.ExpressionVisitor<Person>().Where(p => p.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE (\"Age\" = 27)"));
            Console.WriteLine(db.GetLastSql());

            db.Delete<Person>(where: "Age = {0}".Params(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));
            Console.WriteLine(db.GetLastSql());

            db.Delete(table: "Person", where: "Age = {0}".Params(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"Person\" WHERE Age = 27"));
            Console.WriteLine(db.GetLastSql());
        }
         
    }
}