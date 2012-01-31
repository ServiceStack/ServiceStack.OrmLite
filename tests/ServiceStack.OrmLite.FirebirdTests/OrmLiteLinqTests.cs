using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.FirebirdTests
{
	[TestFixture]
	public class OrmLiteLinqTests
		: OrmLiteTestBase
	{
		public class User
		{
			public long Id { get; set; }

			[Index]
			public string Name { get; set; }

			public DateTime CreatedDate { get; set; }
		}

		[NUnit.Framework.Ignore]
		[Test]
		public void Can_execute_simple_query()
		{
			using (var dbConn = base.ConnectionString.OpenDbConnection())
			using (var dbCmd = dbConn.CreateCommand())
			{
				dbCmd.CreateTable<User>(false);
				dbCmd.DeleteAll<User>();
				
				
				dbCmd.Insert(new User { Id = 1, Name = "A", CreatedDate = DateTime.Now });
				dbCmd.Insert(new User { Id = 2, Name = "B", CreatedDate = DateTime.Now });
				dbCmd.Insert(new User { Id = 3, Name = "B", CreatedDate = DateTime.Now });

				var rowsB = dbCmd.From<User>().Where(x => x.Name == "B")
					.ToList();

				Assert.That(rowsB, Has.Count.EqualTo(2));

				var rowIds = rowsB.ConvertAll(x => x.Id);
				Assert.That(rowIds, Is.EquivalentTo(new List<long> { 2, 3 }));

				rowsB.ForEach(x => dbCmd.Delete(x));

				rowsB = dbCmd.From<User>().Where(x => x.Name == "B")
					.ToList();

				Assert.That(rowsB, Has.Count.EqualTo(0));

				var rowsLeft = dbCmd.Select<User>();
				Assert.That(rowsLeft, Has.Count.EqualTo(1));

				Assert.That(rowsLeft[0].Name, Is.EqualTo("A"));
			}
		}

	}

}