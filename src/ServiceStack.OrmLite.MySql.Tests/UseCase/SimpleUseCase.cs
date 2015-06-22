using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using NUnit.Framework;

namespace ServiceStack.OrmLite.MySql.Tests.UseCase
{
	[TestFixture]
	public class SimpleUseCase
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			//Inject your database provider here
            OrmLiteConfig.DialectProvider = MySqlDialectProvider.Instance;
		}

		public class UserWithIndex
		{
			public long Id { get; set; }

			[ServiceStack.DataAnnotations.Index]
			public string Name { get; set; }

			public DateTime CreatedDate { get; set; }
		}

		[Test]
		public void Simple_CRUD_example()
		{
            using (IDbConnection db = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString.OpenDbConnection())
			{
				db.CreateTable<UserWithIndex>(true);

				db.Insert(new UserWithIndex { Id = 1, Name = "A", CreatedDate = DateTime.Now });
				db.Insert(new UserWithIndex { Id = 2, Name = "B", CreatedDate = DateTime.Now });
				db.Insert(new UserWithIndex { Id = 3, Name = "B", CreatedDate = DateTime.Now });

				var rowsB = db.SelectFmt<UserWithIndex>("Name = {0}", "B");

				Assert.That(rowsB, Has.Count.EqualTo(2));

				var rowIds = rowsB.ConvertAll(x => x.Id);
				Assert.That(rowIds, Is.EquivalentTo(new List<long> { 2, 3 }));

				rowsB.ForEach(x => db.Delete(x));

				rowsB = db.SelectFmt<UserWithIndex>("Name = {0}", "B");
				Assert.That(rowsB, Has.Count.EqualTo(0));

				var rowsLeft = db.Select<UserWithIndex>();
				Assert.That(rowsLeft, Has.Count.EqualTo(1));

				Assert.That(rowsLeft[0].Name, Is.EqualTo("A"));
			}
		}

	}

}