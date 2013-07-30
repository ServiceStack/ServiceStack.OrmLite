using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using ServiceStack.Common.Extensions;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;

namespace MsSqlServerTests
{
	[Alias("Authors")]
	public class Author
	{
		public Author()
		{
		}

		[AutoIncrement]
		public Int32 Id { get; set; }

		[Required]
		[Index(Unique = true)]
		[StringLength(40)]
		[Alias("FName")]
		public string FirstName { get; set; }

		[Required]
		[Index(Unique = true)]
		[StringLength(40)]
		[Alias("LName")]
		public string LastName { get; set; }

		[Ignore]
		public int IgnoredField1 { get; set; }

		[Ignore]
		public string IgnoredField2 { get; set; }

	}
	class Program
	{
		static void Main(string[] args)
		{
			OrmLiteConfig.DialectProvider = new SqlServerOrmLiteDialectProvider();

			using (var db =
				   "data source=.;initial catalog=TestOrmLite;Integrated Security=SSPI".OpenDbConnection())
			{
				db.DropTable<Author>();
				db.CreateTable<Author>();
				db.Insert(new Author()
				{
					FirstName = "Demis",
					LastName = "Bellot"
				});
				db.Insert(new Author()
				{
					FirstName = "Facundo",
					LastName = "Voncina"
				});

				var list = db.Select<Author>();
				var listFromSimpleQuery = db.Query<Author>("SELECT * FROM Authors");
				var listFromQueryWithIgnoredFields = db.Query<Author>("SELECT *, 1 as IgnoredField1, 'test' as IgnoredField2 FROM Authors");
				db.Each<Author>().ForEach(author => Console.WriteLine(author.FirstName));


				Console.ReadLine();
			}
		}
	}
}
