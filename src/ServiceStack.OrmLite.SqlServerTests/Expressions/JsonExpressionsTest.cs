using System;
using System.Collections.Generic;
using System.Configuration;

using ServiceStack.Logging;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer.Converters;

using NUnit.Framework;

namespace ServiceStack.OrmLite.SqlServerTests.Expressions
{
	public class JsonExpressionsTest : OrmLiteTestBase
	{
		[OneTimeSetUp]
		public override void TestFixtureSetUp()
		{
			LogManager.LogFactory = new ConsoleLogFactory();

			OrmLiteConfig.DialectProvider = SqlServer2016Dialect.Provider;
			OrmLiteConfig.DialectProvider.RegisterConverter<Address>(new SqlServerJsonToObjectConverter());

			ConnectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;
		}


		[Test]
		public void Can_test_if_string_field_contains_json()
		{
			using (var db = OpenDbConnection())
			{
				db.DropAndCreateTable<TestType>();

				// test if string field is not JSON with Sql.IsJson
				db.Insert(new TestType { Id = 1, StringColumn = "not json" });

				var j = db.From<TestType>()
					.Select(x => Sql.IsJson(x.StringColumn))
					.Where(x => x.Id == 1);
				var isJson = db.Scalar<bool>(j);

				Assert.IsFalse(isJson);

				// test if string field is JSON with Sql.IsJson
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

				db.Insert(new TestType { Id = 2, StringColumn = stringValue });

				j = db.From<TestType>()
					.Select(x => Sql.IsJson(x.StringColumn))
					.Where(x => x.Id == 2);
				isJson = db.Scalar<bool>(j);

				Assert.IsTrue(isJson);
			}
		}

		[Test]
		public void Can_select_using_a_json_scalar_filter()
		{
			using (var db = OpenDbConnection())
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

				// retrieve records where City in Address is NV (1 record)
				var actual = db.Select<TestType>(q =>
					Sql.JsonValue(q.StringColumn, "$.Address.State") == "NV");

				Assert.IsNotEmpty(actual);

				// retrieve records where City in Address is FL (0 records)
				actual = db.Select<TestType>(q =>
					Sql.JsonValue(q.StringColumn, "$.Address.State") == "FL");

				Assert.IsEmpty(actual);
			}
		}

		[Test]
		public void Can_select_a_json_scalar_value()
		{
			using (var db = OpenDbConnection())
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

				// retrieve only the State in a field that contains a JSON Address
				var state = db.Scalar<string>(
					db.From<TestType>().Select(q =>
						Sql.As(Sql.JsonValue(q.StringColumn, "$.Address.State"), "State")
					)
				);

				Assert.AreEqual(state, obj.Address.State);
			}
		}

		[Test]
		public void Can_select_a_json_object_value()
		{
			using (var db = OpenDbConnection())
			{
				db.DropAndCreateTable<TestType>();

				var expected = new Address
				{
					Line1 = "1234 Main Street",
					Line2 = "Apt. 404",
					City = "Las Vegas",
					State = "NV"
				};

				//{ "Line1": "1234 Main Street", "Line2": "Apt. 404", "City": "Las Vegas", "State": "NV" }
				var stringValue = expected.ToJson();

				db.Insert(new TestType { StringColumn = stringValue });

				// demo how to retrieve inserted JSON string directly to an object
				var address = db.Scalar<Address>(
					db.From<TestType>().Select(q => q.StringColumn)
				);

				var sql = db.GetLastSql();

				Assert.That(expected.Line1, Is.EqualTo(address.Line1));
				Assert.That(expected.Line2, Is.EqualTo(address.Line2));
				Assert.That(expected.City, Is.EqualTo(address.City));
				Assert.That(expected.State, Is.EqualTo(address.State));
			}
		}

		[Ignore("Not functioning properly, issue with converter")]
		[Test]
		public void Can_insert_an_object_directly_to_json()
		{
			using (var db = OpenDbConnection())
			{
				db.DropAndCreateTable<TestType>();

				var expected = new Address
				{
					Line1 = "1234 Main Street",
					Line2 = "Apt. 404",
					City = "Las Vegas",
					State = "NV"
				};

				var tableName = db.GetDialectProvider().GetQuotedTableName(ModelDefinition<TestType>.Definition);

				var sql = $"INSERT {tableName} (StringColumn) VALUES (@StringColumn);";
				db.ExecuteSql(sql, new { StringColumn = expected });

				// demo how to retrieve inserted JSON string directly to an object
				var address = db.Scalar<Address>(
					db.From<TestType>().Select(q => q.StringColumn)
				);

				Assert.That(expected.Line1, Is.EqualTo(address.Line1));
				Assert.That(expected.Line2, Is.EqualTo(address.Line2));
				Assert.That(expected.City, Is.EqualTo(address.City));
				Assert.That(expected.State, Is.EqualTo(address.State));
			}

		}

		public class Address : ISqlJson
		{
			public string Line1 { get; set; }
			public string Line2 { get; set; }
			public string City { get; set; }
			public string State { get; set; }
		}
	}
}