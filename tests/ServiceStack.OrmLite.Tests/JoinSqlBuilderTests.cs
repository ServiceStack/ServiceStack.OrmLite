using System.Text.RegularExpressions;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
	[TestFixture]
	public class JoinSqlBuilderTests : OrmLiteTestBase
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


		[Test]
		public void FieldNameLeftJoinTest ()
		{
			var joinQuery = new JoinSqlBuilder<User, User> ().LeftJoin<User, Address> (x => x.Id, x => x.UserId).ToSql ();
            var expected = "SELECT \"User\".\"Id\",\"User\".\"Name\",\"User\".\"Age\" \nFROM \"User\" \n LEFT OUTER JOIN  \"Address\" ON \"User\".\"Id\" = \"Address\".\"UserId\"  \n".NormalizeSql();
            var expectedNq = "SELECT \"User\".Id,\"User\".Name,\"User\".Age \nFROM \"User\" \n LEFT OUTER JOIN  Address ON \"User\".Id = Address.UserId  \n".NormalizeSql();

            Assert.That(joinQuery.NormalizeSql(), Is.EqualTo(expected).Or.EqualTo(expectedNq));

			joinQuery = new JoinSqlBuilder<WithAliasUser, WithAliasUser> ().LeftJoin<WithAliasUser, WithAliasAddress> (x => x.Id, x => x.UserId).ToSql ();
            expected = "SELECT \"Users\".\"Id\",\"Users\".\"Nickname\",\"Users\".\"Agealias\" \nFROM \"Users\" \n LEFT OUTER JOIN  \"Addresses\" ON \"Users\".\"Id\" = \"Addresses\".\"UserId\"  \n".NormalizeSql();
            expectedNq = "SELECT Users.Id,Users.Nickname,Users.Agealias \nFROM Users \n LEFT OUTER JOIN  Addresses ON Users.Id = Addresses.UserId  \n".NormalizeSql();

            Assert.That(joinQuery.NormalizeSql(), Is.EqualTo(expected).Or.EqualTo(expectedNq));
            
			joinQuery = new JoinSqlBuilder<User, User> ().LeftJoin<User, WithAliasAddress> (x => x.Id, x => x.UserId).ToSql ();
            expected = "SELECT \"User\".\"Id\",\"User\".\"Name\",\"User\".\"Age\" \nFROM \"User\" \n LEFT OUTER JOIN  \"Addresses\" ON \"User\".\"Id\" = \"Addresses\".\"UserId\"  \n".NormalizeSql();
            expectedNq = "SELECT \"User\".Id,\"User\".Name,\"User\".Age \nFROM \"User\" \n LEFT OUTER JOIN  Addresses ON \"User\".Id = Addresses.UserId  \n".NormalizeSql();

            Assert.That(joinQuery.NormalizeSql(), Is.EqualTo(expected).Or.EqualTo(expectedNq));
        }

		[Test]
		public void DoubleWhereLeftJoinTest ()
		{
            var joinQuery = new JoinSqlBuilder<User, User>().LeftJoin<User, WithAliasAddress>(x => x.Id, x => x.UserId
			                                                                                    , sourceWhere: x => x.Age > 18
			                                                                                    , destinationWhere: x => x.Country == "Italy").ToSql ();
			var expected = "SELECT \"User\".\"Id\",\"User\".\"Name\",\"User\".\"Age\" \nFROM \"User\" \n LEFT OUTER JOIN  \"Addresses\" ON \"User\".\"Id\" = \"Addresses\".\"UserId\"  \nWHERE (\"User\".\"Age\" > 18) AND (\"Addresses\".\"Countryalias\" = 'Italy') \n".NormalizeSql();
            var expectedNq = "SELECT \"User\".Id,\"User\".Name,\"User\".Age \nFROM \"User\" \n LEFT OUTER JOIN  Addresses ON \"User\".Id = Addresses.UserId  \nWHERE (\"User\".Age > 18) AND (Addresses.Countryalias = 'Italy') \n".NormalizeSql();

            Assert.That(joinQuery.NormalizeSql(), Is.EqualTo(expected).Or.EqualTo(expectedNq));

            var stmt = OrmLiteConfig.DialectProvider.ToSelectStatement(typeof(User), joinQuery);
            Assert.That(Regex.Matches(stmt, @"(\b|\n)FROM(\b|\n)", RegexOptions.IgnoreCase).Count, Is.EqualTo(1));
        }

	    [Test]
	    public void Can_execute_JoinSqlBuilder_as_SqlExpression()
	    {
            var joinQuery = new JoinSqlBuilder<User, User>()
                .LeftJoin<User, WithAliasAddress>(x => x.Id, x => x.UserId
                    , sourceWhere: x => x.Age > 18
                    , destinationWhere: x => x.Country == "Italy");

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<User>();
                db.DropAndCreateTable<WithAliasAddress>();

                var userId = db.Insert(new User { Age = 27, Name = "Foo" }, selectIdentity:true);
                db.Insert(new WithAliasAddress { City = "Rome", Country = "Italy", UserId = (int)userId });

                var results = db.Select<User>(joinQuery);
                Assert.That(results.Count, Is.EqualTo(1));
            }
	    }

	    [Test]
	    public void Can_execute_SqlBuilder_templates_as_SqlExpression()
	    {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<User>();
                db.DropAndCreateTable<WithAliasAddress>();

                var sb = new SqlBuilder();

                var tmpl = sb.AddTemplate("SELECT * FROM {0} u INNER JOIN {1} a on a.{2} = u.Id /**where**/"
                    .Fmt("User".SqlTable(), "Addresses".SqlTable(), "UserId".SqlColumn()));

                sb.Where("Age > @age", new { age = 18 });
                sb.Where("Countryalias = @country", new { country = "Italy" });

                var userId = db.Insert(new User { Age = 27, Name = "Foo" }, selectIdentity: true);
                db.Insert(new WithAliasAddress { City = "Rome", Country = "Italy", UserId = (int)userId });

                var results = db.Select<User>(tmpl, tmpl.Parameters);

                Assert.That(results.Count, Is.EqualTo(1));
            }
	    }
    }
}