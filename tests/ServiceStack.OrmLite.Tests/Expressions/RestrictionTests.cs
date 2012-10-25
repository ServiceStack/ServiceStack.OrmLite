using System;
using System.Data;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests.Expressions
{
    [TestFixture]
    public class ExpressionTests : OrmLiteTestBase
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            db = ConnectionString.OpenDbConnection();
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
        }

        private IDbConnection db;

        public Person[] People = new[]
                                     {
                                         new Person(1, "Jimi", "Hendrix", 27),
                                         new Person(2, "Janis", "Joplin", 27),
                                         new Person(3, "Jim", "Morrisson", 27),
                                         new Person(4, "Kurt", "Cobain", 27),
                                         new Person(5, "Elvis", "Presley", 42),
                                         new Person(6, "Michael", "Jackson", 50),
                                     };

        [Test]
        public void Can_Chain_Expressions_Using_Or_and_And()
        {
            db.InsertAll(People);

            SqlExpressionVisitor<Person> visitor = db.CreateExpression<Person>();

            visitor.Where(x => x.FirstName.StartsWith("Jim")).Or(x => x.LastName.StartsWith("Cob"));
            
            var results = db.Select<Person>(visitor);
            Assert.AreEqual(3,results.Count);

            visitor.Where(); //clear where expression

            visitor.Where(x => x.FirstName.StartsWith("Jim")).And(x => x.LastName.StartsWith("Hen"));
            results = db.Select<Person>(visitor);

            Assert.AreEqual(1,results.Count);
        }

        [Test]
        public void Can_get_rowcount_from_expression_visitor()
        {
            db.InsertAll(People);

            SqlExpressionVisitor<Person> visitor = db.CreateExpression<Person>();

            visitor.Where(x => x.FirstName.StartsWith("Jim")).Or(x => x.LastName.StartsWith("Cob"));

            visitor.Count();

            var count = db.GetScalar<int>(visitor.CountExpression);

            var results = db.Select<Person>(visitor);
            Assert.AreEqual(count, results.Count);
        }
    }
}