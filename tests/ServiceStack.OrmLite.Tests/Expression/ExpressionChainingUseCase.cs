using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
namespace ServiceStack.OrmLite.Tests.Expression
{
    [TestFixture]
    public class ExpressionChainingUseCase: OrmLiteTestBase
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            db = OpenDbConnection();
            db.CreateTable<Person>(overwrite: true);

            //People.ToList().ForEach(x => dbCmd.Insert(x));
        }

        [TearDown]
        public void TearDown()
        {
            db.Dispose();
        }

        #endregion

        public class Person
        {
            public Person()
            {
            }

            public Person(int id, string firstName, string lastName, int age)
            {
                Id = id;
                FirstName = firstName;
                LastName = lastName;
                Age = age;
            }

            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int? Age { get; set; }

            public override string ToString()
            {
                return string.Format("Id: {0}, FirstName: {1}, LastName: {2}, Age: {3}", Id, FirstName, LastName, Age);
            }
        }

        private IDbConnection db;

        public Person[] People = new[]
                                     {
                                         new Person(1, "Jimi", "Hendrix", 27),
                                         new Person(2, "Janis", "Joplin", 27),
                                         new Person(3, "Jim", "Morrisson", 27),
                                         new Person(4, "Kurt", "Cobain", 27),
                                         new Person(5, "Elvis", "Presley", 42),
                                         new Person(6, "Michael", "Jackson", 50)
                                     };

        [Test]
        public void Can_Chain_Expressions_Using_And()
        {
            db.InsertAll(People);

            var visitor = ReadExtensions.CreateExpression<Person>();

            visitor.Where(x => x.FirstName.StartsWith("Jim")).And(x => x.LastName.StartsWith("Hen"));
            var results = db.Select<Person>(visitor);

            Assert.AreEqual(1,results.Count);

            visitor.Where(); //clears underlying expression

            visitor.Where(x => x.LastName.StartsWith("J")).And(x => x.Age > 40);
            results = db.Select(visitor);
            Assert.AreEqual(results[0].FirstName,"Michael");
        }

        [Test]
        public void Can_Chain_expressions_Using_Or()
        {
            db.InsertAll(People);

            var visitor = db.CreateExpression<Person>();

            visitor.Where(x => x.FirstName.StartsWith("Jim")).Or(x => x.LastName.StartsWith("Cob"));
            
            var results = db.Select<Person>(visitor);
            Assert.AreEqual(3,results.Count);

            visitor.Where(); //clear the underlying expression

            visitor.Where(x => x.Age < 30).Or(x => x.Age > 45);
            results = db.Select<Person>(visitor);
            Assert.AreEqual(5, results.Count);
            Assert.IsFalse(results.Any(x => x.FirstName == "Elvis"));
        }

        [Test]
        public void When_chaining_expressions_using_Where_it_behaves_like_And()
        {
            db.InsertAll(People);

            var visitor = ReadExtensions.CreateExpression<Person>();

            visitor.Where(x => x.FirstName.StartsWith("Jim"));
            visitor.Where(x => x.LastName.StartsWith("Hen"));
            //WHERE (upper("FirstName") like 'JIM%'  AND upper("LastName") like 'HEN%' )
            var results = db.Select<Person>(visitor); 
            Assert.AreEqual(1, results.Count);

            visitor.Or(x => x.FirstName.StartsWith("M"));
            //WHERE ((upper("FirstName") like 'JIM%'  AND upper("LastName") like 'HEN%' ) OR upper("FirstName") like 'M%' )
            results = db.Select(visitor);
            Assert.AreEqual(2,results.Count);

            visitor.Where(x => x.FirstName.StartsWith("M"));
            //WHERE (((upper("FirstName") like 'JIM%'  AND upper("LastName") like 'HEN%' ) OR upper("FirstName") like 'M%' ) AND upper("FirstName") like 'M%' )
            results = db.Select(visitor);
            Assert.AreEqual(1, results.Count);
        }

        [Test]
        public void Can_Chain_Order_Expressions_using_ThenBy()
        {
            db.InsertAll(People);

            var visitor = ReadExtensions.CreateExpression<Person>();
            visitor.OrderBy(x => x.Age);
            visitor.ThenBy(x => x.FirstName);

            var results = db.Select(visitor);

            Console.WriteLine("Sorting using Linq");
            var expected = People.OrderBy(x => x.Age).ThenBy(x => x.FirstName).ToList();
            foreach (var e in expected)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("Retrieved from DB");
            foreach (var r in results)
            {
                Console.WriteLine(r.ToString());
            }

            for (int i = 0; i < expected.Count();i++)
            {
                if (results[i].Id != expected[i].Id)
                {
                    Assert.Fail("Expected person with id {0}, got {1}",expected[i].Id,results[i].Id);
                }
            }

            visitor.OrderBy(); //clears orderBy Expression

            visitor.OrderBy(x => x.Age);
            visitor.ThenByDescending(x => x.FirstName);
            results = db.Select(visitor);

            Console.WriteLine("Sorting using Linq");
            expected = People.OrderBy(x => x.Age).ThenByDescending(x => x.FirstName).ToList();
            foreach (var e in expected)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("Retrieved from DB");
            foreach (var r in results)
            {
                Console.WriteLine(r.ToString());
            }

            for (int i = 0; i < expected.Count(); i++)
            {
                if (results[i].Id != expected[i].Id)
                {
                    Assert.Fail("Expected person with id {0}, got {1}", expected[i].Id, results[i].Id);
                }
            }
        }
    }
}