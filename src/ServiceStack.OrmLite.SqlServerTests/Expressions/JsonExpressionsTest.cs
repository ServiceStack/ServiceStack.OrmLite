using System.Collections.Generic;
using NUnit.Framework;

namespace ServiceStack.OrmLite.SqlServerTests.Expressions
{
    public class JsonExpressionsTest : ExpressionsTestBase
    {
        [Test]
        public void Can_select_json_scalar_value()
        {
            using (var db = OpenDbConnection(dialectProvider: SqlServer2016Dialect.Provider))
            {
                db.DropAndCreateTable<TestType>();

                var obj = new
                {
                    Address = new Address
                    {
                        Line1 = "1234 Main Street",
                        Line2 = "Apt. 404",
                        City = "Las Vegas",
                        State = "NV"
                    }
                };

                //{ "Address": { "Line1": "1234 Main Street", "Line2": "Apt. 404", "City": "Las Vegas", "State": "NV" } }
                var stringValue = obj.ToJson();

                db.Insert(new TestType { StringColumn = stringValue });

                List<TestType> actual = db.Select<TestType>(q =>
                    Sql.JsonValue(q.StringColumn, "$.address.state") == "NV");

                Assert.That(actual, Is.EqualTo(obj.Address.State));
            }
        }

        [Test]
        public void Can_select_json_object_value()
        {
            using (var db = OpenDbConnection(dialectProvider: SqlServer2016Dialect.Provider))
            {
                db.DropAndCreateTable<TestType>();

                var expected = new Address
                {
                    Line1 = "1234 Main Street",
                    Line2 = "Apt. 404",
                    City = "Las Vegas",
                    State = "NV"
                };
                var obj = new { Address = expected };

                //{ "Address": { "Line1": "1234 Main Street", "Line2": "Apt. 404", "City": "Las Vegas", "State": "NV" } }
                var stringValue = obj.ToJson(); 

                db.Insert(new TestType { StringColumn = stringValue });

                SqlExpression<TestType> q = db.From<TestType>().Select(x =>
                    Sql.JsonQuery<Address>(x.StringColumn, "$.address"));

                var address = q.ConvertTo<Address>();

                Assert.That(address, Is.EqualTo(obj.Address));
            }
        }

        internal class Address
        {
            public string Line1 { get; set; }
            public string Line2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
        }
    }
}