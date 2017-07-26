using System;
using System.Data;
using System.Linq;
using NUnit.Framework;

namespace ServiceStack.OrmLite.SqlServerTests.Expressions
{
    public class JsonExpressionsTest : ExpressionsTestBase
    {
        [Test]
        public void Can_select_json_scalar_value()
        {
            OrmLiteConfig.DialectProvider = SqlServer2016Dialect.Provider;

            OpenDbConnection().CreateTableIfNotExists<TestType>();
            OpenDbConnection().DeleteAll<TestType>();

            var obj = new { Address = new Address { Line1 = "1234 Main Street", Line2 = "Apt. 404", City = "Las Vegas", State = "NV" } };
            var stringValue = obj.ToJson(); //"{ \"Address\": { \"Line1\": \"1234 Main Street\", \"Line2\": \"Apt. 404\", \"City\": \"Las Vegas\", \"State\": \"NV\" } }"

            OpenDbConnection().Insert(new TestType() { StringColumn = stringValue });

            var actual = OpenDbConnection().Select<TestType>(q => 
                Sql2016.JsonValue(q.StringColumn, "$.address.state") == "NV");

            Assert.AreEqual(obj.Address.State, actual);
        }

        [Test]
        public void Can_select_json_object_value()
        {
            OrmLiteConfig.DialectProvider = SqlServer2016Dialect.Provider;

            OpenDbConnection().CreateTableIfNotExists<TestType>();
            OpenDbConnection().DeleteAll<TestType>();

            var expected = new Address { Line1 = "1234 Main Street", Line2 = "Apt. 404", City = "Las Vegas", State = "NV" };
            var obj = new { Address = expected };
            var stringValue = obj.ToJson(); //"{ \"Address\": { \"Line1\": \"1234 Main Street\", \"Line2\": \"Apt. 404\", \"City\": \"Las Vegas\", \"State\": \"NV\" } }"

            OpenDbConnection().Insert(new TestType() { StringColumn = stringValue });

            var address = OpenDbConnection().From<TestType>().Select(q =>
                Sql2016.JsonQuery<Address>(q.StringColumn, "$.address")).ConvertTo<Address>();

            Assert.AreEqual(obj.Address, address);
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