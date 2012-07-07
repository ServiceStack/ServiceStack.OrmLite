using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data;
using ServiceStack.Common.Utils;
using ServiceStack.DataAnnotations;
using ServiceStack.Common.Extensions;
using ServiceStack.OrmLite;

namespace AllDialectsTest
{
	class MainClass
	{
		private static List<Dialect> dialects;
		private static List<Author> authors;

		public static void Main(string[] args)
		{

			bool exit=false;

			dialects = BuildDialectList();
			authors = BuildAuthorList();

			PaintMenu();

			while (!exit)
			{

				Console.WriteLine("Select your option [{0}-{1}] or q to quit  and press ENTER", 1, dialects.Count);
				string option = Console.ReadLine();
				if (string.IsNullOrEmpty(option))
					Console.WriteLine("NO VALID OPTION");
				else if (option.ToUpper() == "Q")
					exit = true;
				else
				{
					int opt;
					if (int.TryParse(option, out opt))
					{
						if (opt >= 1 && opt <= dialects.Count)
							TestDialect(dialects[opt - 1]);
						else
						{
							Console.WriteLine("NO VALID OPTION");
						}

					}
					else
						Console.WriteLine("NO VALID OPTION");
				}
			}


		}

		private static void PaintMenu()
		{
			Console.Clear();
			int i=0;
			foreach (Dialect d in dialects)
			{
				Console.WriteLine("{0} {1}", ++i, d.Name);
			}
			Console.WriteLine("q quit");
		}

		private static List<Dialect> BuildDialectList()
		{
			List<Dialect> l = new List<Dialect>();
			Dialect d = new Dialect() {
				Name = "Sqlite",
				PathToAssembly = "../../../ServiceStack.OrmLite.Sqlite/bin/Debug",
				AssemblyName = "ServiceStack.OrmLite.Sqlite.dll",
				ClassName = "ServiceStack.OrmLite.Sqlite.SqliteOrmLiteDialectProvider",
				InstanceFieldName = "Instance",
				ConnectionString = "~/db.sqlite".MapAbsolutePath()
			};
			l.Add(d);

			d = new Dialect() {
				Name = "SqlServer",
				PathToAssembly = "../../../ServiceStack.OrmLite.SqlServer/bin/Debug",
				AssemblyName = "ServiceStack.OrmLite.SqlServer.dll",
				ClassName = "ServiceStack.OrmLite.SqlServer.SqlServerOrmLiteDialectProvider",
				InstanceFieldName = "Instance",
				ConnectionString = "~/test.mdf".MapAbsolutePath()
			};
			l.Add(d);

			d = new Dialect() {
				Name = "MySql",
				PathToAssembly = "../../../ServiceStack.OrmLite.MySql/bin/Debug",
				AssemblyName = "ServiceStack.OrmLite.MySql.dll",
				ClassName = "ServiceStack.OrmLite.MySql.MySqlDialectProvider",
				InstanceFieldName = "Instance",
				ConnectionString = "Server = 127.0.0.1; Database = ormlite; Uid = root; Pwd = password"
			};
			l.Add(d);

			d = new Dialect() {
				Name = "PostgreSQL",
				PathToAssembly = "../../../ServiceStack.OrmLite.PostgreSQL/bin/Debug",
				AssemblyName = "ServiceStack.OrmLite.PostgreSQL.dll",
				ClassName = "ServiceStack.OrmLite.PostgreSQL.PostgreSQLDialectProvider",
				InstanceFieldName = "Instance",
				ConnectionString = "Server=localhost;Port=5432;User Id=postgres; Password=postgres; Database=ormlite"
			};
			l.Add(d);

			d = new Dialect() {
				Name = "FirebirdSql",
				PathToAssembly = "../../../ServiceStack.OrmLite.Firebird/bin/Debug",
				AssemblyName = "ServiceStack.OrmLite.Firebird.dll",
				ClassName = "ServiceStack.OrmLite.Firebird.FirebirdOrmLiteDialectProvider",
				InstanceFieldName = "Instance",
				ConnectionString = "User=SYSDBA;Password=masterkey;Database=employee.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;"
			};
			l.Add(d);

			return l;

		}

		private static List<Author> BuildAuthorList()
		{

			List<Author> a = new List<Author>();
			a.Add(new Author() { Name = "Demis Bellot", Birthday = DateTime.Today.AddYears(-20), Active = true, Earnings = 99.9m, Comments = "CSharp books", Rate = 10, City = "London" });
			a.Add(new Author() { Name = "Angel Colmenares", Birthday = DateTime.Today.AddYears(-25), Active = true, Earnings = 50.0m, Comments = "CSharp books", Rate = 5, City = "Bogota" });
			a.Add(new Author() { Name = "Adam Witco", Birthday = DateTime.Today.AddYears(-20), Active = true, Earnings = 80.0m, Comments = "Math Books", Rate = 9, City = "London" });
			a.Add(new Author() { Name = "Claudia Espinel", Birthday = DateTime.Today.AddYears(-23), Active = true, Earnings = 60.0m, Comments = "Cooking books", Rate = 10, City = "Bogota" });
			a.Add(new Author() { Name = "Libardo Pajaro", Birthday = DateTime.Today.AddYears(-25), Active = true, Earnings = 80.0m, Comments = "CSharp books", Rate = 9, City = "Bogota" });
			a.Add(new Author() { Name = "Jorge Garzon", Birthday = DateTime.Today.AddYears(-28), Active = true, Earnings = 70.0m, Comments = "CSharp books", Rate = 9, City = "Bogota" });
			a.Add(new Author() { Name = "Alejandro Isaza", Birthday = DateTime.Today.AddYears(-20), Active = true, Earnings = 70.0m, Comments = "Java books", Rate = 0, City = "Bogota" });
			a.Add(new Author() { Name = "Wilmer Agamez", Birthday = DateTime.Today.AddYears(-20), Active = true, Earnings = 30.0m, Comments = "Java books", Rate = 0, City = "Cartagena" });
			a.Add(new Author() { Name = "Rodger Contreras", Birthday = DateTime.Today.AddYears(-25), Active = true, Earnings = 90.0m, Comments = "CSharp books", Rate = 8, City = "Cartagena" });
			a.Add(new Author() { Name = "Chuck Benedict", Birthday = DateTime.Today.AddYears(-22), Active = true, Earnings = 85.5m, Comments = "CSharp books", Rate = 8, City = "London" });
			a.Add(new Author() { Name = "James Benedict II", Birthday = DateTime.Today.AddYears(-22), Active = true, Earnings = 85.5m, Comments = "Java books", Rate = 5, City = "Berlin" });
			a.Add(new Author() { Name = "Ethan Brown", Birthday = DateTime.Today.AddYears(-20), Active = true, Earnings = 45.0m, Comments = "CSharp books", Rate = 5, City = "Madrid" });
			a.Add(new Author() { Name = "Xavi Garzon", Birthday = DateTime.Today.AddYears(-22), Active = true, Earnings = 75.0m, Comments = "CSharp books", Rate = 9, City = "Madrid" });
			a.Add(new Author() { Name = "Luis garzon", Birthday = DateTime.Today.AddYears(-22), Active = true, Earnings = 85.0m, Comments = "CSharp books", Rate = 10, City = "Mexico", LastActivity= DateTime.Today });
			return a;
		}

		private static void TestDialect(Dialect dialect)
		{
			Console.Clear();
			Console.WriteLine("Testing expressions for Dialect {0}", dialect.Name);

			OrmLiteConfig.ClearCache();
			OrmLiteConfig.DialectProvider = dialect.DialectProvider;
			SqlExpressionVisitor<Author> ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<Author>();

			using (IDbConnection db =
			       dialect.ConnectionString.OpenDbConnection())
			using (IDbCommand dbCmd = db.CreateCommand())
			{
				try
				{
					dbCmd.DropTable<Author>();
					
					var tableExists = OrmLiteConfig.DialectProvider.DoesTableExist(dbCmd, typeof(Author).Name);
					Console.WriteLine("Expected:{0} Selected:{1}  {2}", bool.FalseString, tableExists.ToString(), !tableExists ? "OK" : "**************  FAILED ***************");

					dbCmd.CreateTable<Author>();

					tableExists = OrmLiteConfig.DialectProvider.DoesTableExist(dbCmd, typeof(Author).Name);
					Console.WriteLine("Expected:{0} Selected:{1}  {2}", bool.TrueString, tableExists.ToString(), tableExists ? "OK" : "**************  FAILED ***************");

					dbCmd.DeleteAll<Author>();
					Console.WriteLine("Inserting...");
					DateTime t1= DateTime.Now;
					dbCmd.InsertAll(authors);
					DateTime t2= DateTime.Now;
					Console.WriteLine("Inserted {0} rows in {1}", authors.Count, t2 - t1);

					Console.WriteLine("Selecting.....");

					int year = DateTime.Today.AddYears(-20).Year;
					var lastDay= new DateTime(year, 12, 31);
					int expected=5;

					ev.Where(rn => rn.Birthday >= new DateTime(year, 1, 1) && rn.Birthday <= lastDay);
					Console.WriteLine(ev.ToSelectStatement());
					List<Author> result=dbCmd.Select(ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					result = dbCmd.Select<Author>(qry => qry.Where(rn => rn.Birthday >= new DateTime(year, 1, 1) && rn.Birthday <= lastDay));
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					result = dbCmd.Select<Author>(rn => rn.Birthday >= new DateTime(year, 1, 1) && rn.Birthday <= lastDay);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					Author a = new Author() { Birthday = lastDay };
					result = dbCmd.Select<Author>(rn => rn.Birthday >= new DateTime(year, 1, 1) && rn.Birthday <= a.Birthday);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");

					// select authors from London, Berlin and Madrid : 6
					expected = 6;
					//Sql.In can take params object[]
					var city="Berlin";
					ev.Where(rn => Sql.In(rn.City, "London", "Madrid", city));
					result = dbCmd.Select(ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					result = dbCmd.Select<Author>(rn => Sql.In(rn.City, new[] { "London", "Madrid", "Berlin" }));
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");

					// select authors from Bogota and Cartagena : 7
					expected = 7;
					//... or Sql.In can  take List<Object>
					city = "Bogota";
					List<Object> cities= new List<Object>();
					cities.Add(city);
					cities.Add("Cartagena");
					ev.Where(rn => Sql.In(rn.City, cities));
					result = dbCmd.Select(ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					result = dbCmd.Select<Author>(rn => Sql.In(rn.City, "Bogota", "Cartagena"));
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");


					// select authors which name starts with A
					expected = 3;
					ev.Where(rn => rn.Name.StartsWith("A"));
					result = dbCmd.Select(ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					result = dbCmd.Select<Author>(rn => rn.Name.StartsWith("A"));
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");

					// select authors which name ends with Garzon o GARZON o garzon ( no case sensitive )
					expected = 3;
					var name="GARZON";
					ev.Where(rn => rn.Name.ToUpper().EndsWith(name));
					result = dbCmd.Select(ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					result = dbCmd.Select<Author>(rn => rn.Name.ToUpper().EndsWith(name));
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");

					// select authors which name ends with garzon
					//A percent symbol ("%") in the LIKE pattern matches any sequence of zero or more characters 
					//in the string. 
					//An underscore ("_") in the LIKE pattern matches any single character in the string. 
					//Any other character matches itself or its lower/upper case equivalent (i.e. case-insensitive matching).
					expected = 3;
					ev.Where(rn => rn.Name.EndsWith("garzon"));
					result = dbCmd.Select(ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					result = dbCmd.Select<Author>(rn => rn.Name.EndsWith("garzon"));
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");


					// select authors which name contains  Benedict 
					expected = 2;
					name = "Benedict";
					ev.Where(rn => rn.Name.Contains(name));
					result = dbCmd.Select(ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					result = dbCmd.Select<Author>(rn => rn.Name.Contains("Benedict"));
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					a.Name = name;
					result = dbCmd.Select<Author>(rn => rn.Name.Contains(a.Name));
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");


					// select authors with Earnings <= 50 
					expected = 3;
					var earnings=50;
					ev.Where(rn => rn.Earnings <= earnings);
					result = dbCmd.Select(ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					result = dbCmd.Select<Author>(rn => rn.Earnings <= 50);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");

					// select authors with Rate = 10 and city=Mexio 
					expected = 1;
					city = "Mexico";
					ev.Where(rn => rn.Rate == 10 && rn.City == city);
					result = dbCmd.Select(ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					result = dbCmd.Select<Author>(rn => rn.Rate == 10 && rn.City == "Mexico");
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");

					a.City = city;
					result = dbCmd.Select<Author>(rn => rn.Rate == 10 && rn.City == a.City);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");

					//  enough selecting, lets update;
					// set Active=false where rate =0
					expected = 2;
					var rate=0;
					ev.Where(rn => rn.Rate == rate).Update(rn => rn.Active);
					var rows = dbCmd.UpdateOnly(new Author() { Active = false }, ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, rows, expected == rows ? "OK" : "**************  FAILED ***************");

					// insert values  only in Id, Name, Birthday, Rate and Active fields 
					expected = 4;
					ev.Insert(rn => new { rn.Id, rn.Name, rn.Birthday, rn.Active, rn.Rate });
					dbCmd.InsertOnly(new Author() { Active = false, Rate = 0, Name = "Victor Grozny", Birthday = DateTime.Today.AddYears(-18) }, ev);
					dbCmd.InsertOnly(new Author() { Active = false, Rate = 0, Name = "Ivan Chorny", Birthday = DateTime.Today.AddYears(-19) }, ev);
					ev.Where(rn => !rn.Active);
					result = dbCmd.Select(ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");

					//update comment for City == null 
					expected = 2;
					ev.Where(rn => rn.City == null).Update(rn => rn.Comments);
					rows = dbCmd.UpdateOnly(new Author() { Comments = "No comments" }, ev);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, rows, expected == rows ? "OK" : "**************  FAILED ***************");

					// delete where City is null 
					expected = 2;
					rows = dbCmd.Delete(ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, rows, expected == rows ? "OK" : "**************  FAILED ***************");


					//   lets select  all records ordered by Rate Descending and Name Ascending
					expected = 14;
					ev.Where().OrderBy(rn => new { at = Sql.Desc(rn.Rate), rn.Name }); // clear where condition
					result = dbCmd.Select(ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					Console.WriteLine(ev.OrderByExpression);
					var author = result.FirstOrDefault();
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", "Claudia Espinel", author.Name, "Claudia Espinel" == author.Name ? "OK" : "**************  FAILED ***************");

					// select  only first 5 rows ....

					expected = 5;
					ev.Limit(5); // note: order is the same as in the last sentence
					result = dbCmd.Select(ev);
					Console.WriteLine(ev.WhereExpression);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");


					// and finally lets select only Name and City (name will be "UPPERCASED" )

					ev.Select(rn => new { at = Sql.As(rn.Name.ToUpper(), "Name"), rn.City });
					Console.WriteLine(ev.SelectExpression);
					result = dbCmd.Select(ev);
					author = result.FirstOrDefault();
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", "Claudia Espinel".ToUpper(), author.Name, "Claudia Espinel".ToUpper() == author.Name ? "OK" : "**************  FAILED ***************");
					
					ev.Select(rn => new { at = Sql.As(rn.Name.ToUpper(), rn.Name), rn.City });
					Console.WriteLine(ev.SelectExpression);
					result = dbCmd.Select(ev);
					author = result.FirstOrDefault();
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", "Claudia Espinel".ToUpper(), author.Name, "Claudia Espinel".ToUpper() == author.Name ? "OK" : "**************  FAILED ***************");
					
					//paging :
					ev.Limit(0, 4);// first page, page size=4;
					result = dbCmd.Select(ev);
					author = result.FirstOrDefault();
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", "Claudia Espinel".ToUpper(), author.Name, "Claudia Espinel".ToUpper() == author.Name ? "OK" : "**************  FAILED ***************");

					ev.Limit(4, 4);// second page
					result = dbCmd.Select(ev);
					author = result.FirstOrDefault();
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", "Jorge Garzon".ToUpper(), author.Name, "Jorge Garzon".ToUpper() == author.Name ? "OK" : "**************  FAILED ***************");

					ev.Limit(8, 4);// third page
					result = dbCmd.Select(ev);
					author = result.FirstOrDefault();
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", "Rodger Contreras".ToUpper(), author.Name, "Rodger Contreras".ToUpper() == author.Name ? "OK" : "**************  FAILED ***************");

					// select distinct..
					ev.Limit().OrderBy(); // clear limit, clear order for postres
					ev.SelectDistinct(r => r.City);
					expected = 6;
					result = dbCmd.Select(ev);
					Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count ? "OK" : "**************  FAILED ***************");
					
					ev.Select(r=> Sql.As(Sql.Max(r.Birthday), "Birthday"));
					result = dbCmd.Select(ev);
					var expectedResult  = authors.Max(r=>r.Birthday);
					Console.WriteLine("Expected:{0} Selected {1} {2}",expectedResult, 
					                  result[0].Birthday,
					                  expectedResult == result[0].Birthday ? "OK" : "**************  FAILED ***************");
					
					ev.Select(r=> Sql.As(Sql.Max(r.Birthday), r.Birthday));
					result = dbCmd.Select(ev);
					expectedResult  = authors.Max(r=>r.Birthday);
					Console.WriteLine("Expected:{0} Selected {1} {2}",expectedResult, 
					                  result[0].Birthday,
					                  expectedResult == result[0].Birthday ? "OK" : "**************  FAILED ***************");
					
					
					
					var r1 = dbCmd.FirstOrDefault(ev);
					Console.WriteLine("FOD: Expected:{0} Selected {1} {2}",expectedResult, 
					                  r1.Birthday,
					                  expectedResult == r1.Birthday ? "OK" : "**************  FAILED ***************");
					
					
					var r2 = dbCmd.GetScalar<Author, DateTime>( e => Sql.Max(e.Birthday) );
					Console.WriteLine("GetScalar DateTime: Expected:{0} Selected {1} {2}",expectedResult, 
					                  r2,
					                  expectedResult == r2 ? "OK" : "**************  FAILED ***************");
					
					ev.Select(r=> Sql.As( Sql.Min(r.Birthday), "Birthday"));
					result = dbCmd.Select(ev);
					expectedResult  = authors.Min(r=>r.Birthday);
					Console.WriteLine("Expected:{0} Selected {1} {2}",expectedResult, 
					                  result[0].Birthday,
					                  expectedResult == result[0].Birthday? "OK" : "**************  FAILED ***************");
					
					
					
					ev.Select(r=> Sql.As( Sql.Min(r.Birthday), r.Birthday));
					result = dbCmd.Select(ev);
					expectedResult  = authors.Min(r=>r.Birthday);
					Console.WriteLine("Expected:{0} Selected {1} {2}",expectedResult, 
					                  result[0].Birthday,
					                  expectedResult == result[0].Birthday? "OK" : "**************  FAILED ***************");
					
					
					ev.Select(r=>new{r.City,  MaxResult=Sql.As( Sql.Min(r.Birthday), "Birthday") })
							.GroupBy(r=>r.City)
							.OrderBy(r=>r.City);
					result = dbCmd.Select(ev);
					var expectedStringResult= "Berlin";
					Console.WriteLine("Expected:{0} Selected {1} {2}",expectedResult, 
					                  result[0].City,
					                  expectedStringResult == result[0].City ? "OK" : "**************  FAILED ***************");
					
					
					ev.Select(r=>new{r.City,  MaxResult=Sql.As( Sql.Min(r.Birthday), r.Birthday) })
							.GroupBy(r=>r.City)
							.OrderBy(r=>r.City);
					result = dbCmd.Select(ev);
					expectedStringResult= "Berlin";
					Console.WriteLine("Expected:{0} Selected {1} {2}",expectedResult, 
					                  result[0].City,
					                  expectedStringResult == result[0].City ? "OK" : "**************  FAILED ***************");
					
					r1 = dbCmd.FirstOrDefault(ev);
					Console.WriteLine("FOD: Expected:{0} Selected {1} {2}",expectedResult, 
					                  r1.City,
					                  expectedStringResult == result[0].City ? "OK" : "**************  FAILED ***************");
					
					
					var expectedDecimal= authors.Max(e=>e.Earnings);
					Decimal? r3 = dbCmd.GetScalar<Author,Decimal?>(e=> Sql.Max(e.Earnings));
					Console.WriteLine("GetScalar decimal?: Expected:{0} Selected {1} {2}",expectedDecimal, 
					                  r3.Value,
					                  expectedDecimal == r3.Value ? "OK" : "**************  FAILED ***************");
					
					var expectedString= authors.Max(e=>e.Name);
					string r4 = dbCmd.GetScalar<Author,String>(e=> Sql.Max(e.Name));
					
					Console.WriteLine("GetScalar string?: Expected:{0} Selected {1} {2}",expectedString, 
					                  r4,
					                  expectedString == r4 ? "OK" : "**************  FAILED ***************");
					
					var expectedDate= authors.Max(e=>e.LastActivity);
					DateTime? r5 = dbCmd.GetScalar<Author,DateTime?>(e=> Sql.Max(e.LastActivity));
					Console.WriteLine("GetScalar datetime?: Expected:{0} Selected {1} {2}",
					                  expectedDate, 
					                  r5,
					                  expectedDate == r5 ? "OK" : "**************  FAILED ***************");
					
					
					var expectedDate51= authors.Where(e=> e.City=="Bogota").Max(e=>e.LastActivity);
					DateTime? r51 = dbCmd.GetScalar<Author,DateTime?>(
						e=>  Sql.Max(e.LastActivity),
					 	e=>  e.City=="Bogota" );
					
					Console.WriteLine("GetScalar datetime?: Expected:{0} Selected {1} {2}",
					                  expectedDate51, 
					                  r51,
					                  expectedDate51 == r51 ? "OK" : "**************  FAILED ***************");
					
					try{
						var expectedBool= authors.Max(e=>e.Active);
						bool r6 = dbCmd.GetScalar<Author,bool>(e=> Sql.Max(e.Active));
						Console.WriteLine("GetScalar bool: Expected:{0} Selected {1} {2}",expectedBool, 
					                  r6,
					                  expectedBool == r6 ? "OK" : "**************  FAILED ***************");
					}
					catch(Exception e){
						if(dialect.Name=="PostgreSQL")
							Console.WriteLine("OK PostgreSQL: " + e.Message);
						else
							Console.WriteLine("**************  FAILED *************** " + e.Message);
					}
					
					
					
					// Tests for predicate overloads that make use of the expression visitor
					Console.WriteLine("First author by name (exists)");
					author = dbCmd.First<Author>(q => q.Name == "Jorge Garzon");
					Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Jorge Garzon", author.Name, "Jorge Garzon" == author.Name);

					try
					{
						Console.WriteLine("First author by name (does not exist)");
						author = dbCmd.First<Author>(q => q.Name == "Does not exist");

						Console.WriteLine("Expected exception thrown, OK? False");
					}
					catch
					{
						Console.WriteLine("Expected exception thrown, OK? True");
					}

					Console.WriteLine("First author or default (does not exist)");
					author = dbCmd.FirstOrDefault<Author>(q => q.Name == "Does not exist");
					Console.WriteLine("Expected:null ; OK? {0}", author == null);

					Console.WriteLine("First author or default by city (multiple matches)");
					author = dbCmd.FirstOrDefault<Author>(q => q.City == "Bogota");
					Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Angel Colmenares", author.Name, "Angel Colmenares" == author.Name);

					a.City = "Bogota";
					author = dbCmd.FirstOrDefault<Author>(q => q.City == a.City);
					Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Angel Colmenares", author.Name, "Angel Colmenares" == author.Name);
					
					// count test
					
					var expectedCount= authors.Count();
					long r7 = dbCmd.GetScalar<Author,long>(e=> Sql.Count(e.Id));
					Console.WriteLine("GetScalar long: Expected:{0} Selected {1} {2}",expectedCount, 
					                  r7,
					                  expectedCount == r7 ? "OK" : "**************  FAILED ***************");
					
					expectedCount= authors.Count(e=> e.City=="Bogota");
					r7 = dbCmd.GetScalar<Author,long>(
						e=>  Sql.Count(e.Id),
					 	e=>  e.City=="Bogota" );
					
					Console.WriteLine("GetScalar long: Expected:{0} Selected {1} {2}",expectedCount, 
					                  r7,
					                  expectedCount == r7 ? "OK" : "**************  FAILED ***************");
					

                    // more updates.....
                    Console.WriteLine("more updates.....................");
                     ev.Update();// all fields will be updated
                    // select and update 
                    expected=1;
                    var rr= dbCmd.FirstOrDefault<Author>(rn => rn.Name=="Luis garzon");
                    rr.City="Madrid";
                    rr.Comments="Updated";
                    ev.Where(r=>r.Id==rr.Id); // if omit,  then all records will be updated 
                    rows=dbCmd.UpdateOnly(rr,ev); // == dbCmd.Update(rr) but it returns void
                    Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, rows, expected == rows ? "OK" : "**************  FAILED ***************");

                    expected=0;
                    ev.Where(r=>r.City=="Ciudad Gotica");
                    rows=dbCmd.UpdateOnly(rr, ev);
                    Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, rows, expected == rows ? "OK" : "**************  FAILED ***************");

                    expected= dbCmd.Select<Author>(x=>x.City=="Madrid").Count;
                    author = new Author(){Active=false};
                    rows=dbCmd.UpdateOnly(author, x=>x.Active,  x=>x.City=="Madrid");
                    Console.WriteLine("Expected:{0}  Updated:{1}  {2}", expected, rows, expected == rows ? "OK" : "**************  FAILED ***************");

                    expected= dbCmd.Select<Author>(x=>x.Active==false).Count;
                    rows = dbCmd.Delete<Author>( x=>x.Active==false);
                    Console.WriteLine("Expected:{0}  Deleted:{1}  {2}", expected, rows, expected == rows ? "OK" : "**************  FAILED ***************");

					DateTime t3= DateTime.Now;
					Console.WriteLine("Expressions test in: {0}", t3 - t2);
					Console.WriteLine("All test in :        {0}", t3 - t1);

				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}

			Console.WriteLine("Press enter to return to main menu");
			Console.ReadLine();
			PaintMenu();
		}
	}
}
