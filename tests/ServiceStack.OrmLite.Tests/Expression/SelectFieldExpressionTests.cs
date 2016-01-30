using System.Linq;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.Shared;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Expression
{
    public class SelectFieldExpressionTests
         : OrmLiteTestBase
    {
        [Test]
        public void Can_Select_Substring()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Person>();
                db.InsertAll(Person.Rockstars);

                var results = db.Select<Person>(x => x.FirstName.Substring(1, 2) == "im");

                results.PrintDump();

                var expected = Person.Rockstars.Where(x => x.FirstName.Substring(1, 2) == "im").ToList();

                Assert.That(results.Count, Is.EqualTo(expected.Count));
                Assert.That(results, Is.EquivalentTo(expected));

                results = db.Select<Person>(x => x.FirstName.Substring(1) == "im");
                results.PrintDump();

                expected = Person.Rockstars.Where(x => x.FirstName.Substring(1) == "im").ToList();
                Assert.That(results.Count, Is.EqualTo(expected.Count));
                Assert.That(results, Is.EquivalentTo(expected));
            }
        }

        [Test]
        public void Can_select_value_as_null()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Person>();
                db.InsertAll(Person.Rockstars);

                var firstName = "Kurt";
                string lastName = null;

                var results = db.Select<Person>(q =>
                    q.Where(x => firstName == null || x.FirstName == firstName)
                        .And(x => lastName == null || x.LastName == lastName));

                db.GetLastSql().Print();

                results.PrintDump();

                Assert.That(results[0].LastName, Is.EqualTo("Cobain"));
            }
        }
    }
}