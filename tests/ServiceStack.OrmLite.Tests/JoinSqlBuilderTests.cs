using NUnit.Framework;
using System;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Tests
{
	[TestFixture ()]
	public class JoinSqlBuilderTests
	{
		[Alias("Users")]
		public class WithAliasUser 
		{
			[AutoIncrement]
			public int Id { get; set; }

			[Alias("Nickname")]
			public string Name { get; set; }

			[Alias("Agealias")]
			public int Age { get; set; }
		}

		[Alias("Addresses")]
		public class WithAliasAddress
		{
			[AutoIncrement]
			public int Id { get; set; }
			public int UserId { get; set; }
			public string City { get; set; }

			[Alias("Countryalias")]
			public string Country { get; set; }
		}

		public class User 
		{
			[AutoIncrement]
			public int Id { get; set; }
			public string Name { get; set; }
			public int Age { get; set; }
		}

		public class Address
		{
			[AutoIncrement]
			public int Id { get; set; }
			public int UserId { get; set; }
			public string City { get; set; }
			public string Country { get; set; }
		}


		[Test ()]
		public void FieldNameLeftJoinTest ()
		{
			var joinQuery = new JoinSqlBuilder<User, User> ().LeftJoin<User, Address> (x => x.Id, x => x.UserId).ToSql ();
			var expected = "SELECT \"User\".\"Id\",\"User\".\"Name\",\"User\".\"Age\" \nFROM \"User\" \n LEFT OUTER JOIN  \"Address\" ON \"User\".\"Id\" = \"Address\".\"UserId\"  \n";

			Assert.AreEqual (expected, joinQuery);


			joinQuery = new JoinSqlBuilder<WithAliasUser, WithAliasUser> ().LeftJoin<WithAliasUser, WithAliasAddress> (x => x.Id, x => x.UserId).ToSql ();
			expected = "SELECT \"Users\".\"Id\",\"Users\".\"Nickname\",\"Users\".\"Agealias\" \nFROM \"Users\" \n LEFT OUTER JOIN  \"Addresses\" ON \"Users\".\"Id\" = \"Addresses\".\"UserId\"  \n";

			Assert.AreEqual (expected, joinQuery);


			joinQuery = new JoinSqlBuilder<User, User> ().LeftJoin<User, WithAliasAddress> (x => x.Id, x => x.UserId).ToSql ();
			expected = "SELECT \"User\".\"Id\",\"User\".\"Name\",\"User\".\"Age\" \nFROM \"User\" \n LEFT OUTER JOIN  \"Addresses\" ON \"User\".\"Id\" = \"Addresses\".\"UserId\"  \n";

			Assert.AreEqual (expected, joinQuery);
		}

		[Test ()]
		public void DoubleWhereLeftJoinTest ()
		{
			var joinQuery = new JoinSqlBuilder<User, User> ().LeftJoin<User, WithAliasAddress> (x => x.Id, x => x.UserId
			                                                                           , sourceWhere: x => x.Age > 18
			                                                                           , destinationWhere: x => x.Country == "Italy").ToSql ();
			var expected = "SELECT \"User\".\"Id\",\"User\".\"Name\",\"User\".\"Age\" \nFROM \"User\" \n LEFT OUTER JOIN  \"Addresses\" ON \"User\".\"Id\" = \"Addresses\".\"UserId\"  \nWHERE (\"User\".\"Age\" > 18) AND (\"Addresses\".\"Countryalias\" = 'Italy') \n";

			Assert.AreEqual (expected, joinQuery);
		}
	}
}