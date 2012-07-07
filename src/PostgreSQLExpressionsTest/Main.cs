using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data;

using ServiceStack.Common.Utils;
using ServiceStack.DataAnnotations;
using ServiceStack.Common.Extensions;
using System.Reflection;

using ServiceStack.OrmLite;
using ServiceStack.OrmLite.PostgreSQL;


namespace PostgreSQLExpressionsTest
{
	public class Author{
		public Author(){}
		[AutoIncrement]
		[Alias("AuthorID")]
		public Int32 Id { get; set;}
		[Index(Unique = true)]
		[StringLength(40)]
		public string Name { get; set;}
		public DateTime Birthday { get; set;}
		public DateTime ? LastActivity  { get; set;}
		public Decimal? Earnings { get; set;}  
		public bool Active { get; set; } 
		[StringLength(80)]
		[Alias("JobCity")]
		public string City { get; set;}
		[StringLength(80)]
		[Alias("Comment")]
		public string Comments { get; set;}
		public Int16 Rate{ get; set;}
	}
	
	
	
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");
			
			OrmLiteConfig.DialectProvider = PostgreSQLDialectProvider.Instance;
			SqlExpressionVisitor<Author> ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<Author>();
									
			using (IDbConnection db =
			       "Server=localhost;Port=5432;User Id=postgres; Password=postgres; Database=ormlite".OpenDbConnection())
			using ( IDbCommand dbCmd = db.CreateCommand())
			{
				dbCmd.DropTable<Author>();
				dbCmd.CreateTable<Author>();
				dbCmd.DeleteAll<Author>();
				
				List<Author> authors = new List<Author>();
				authors.Add(new Author(){Name="Demis Bellot",Birthday= DateTime.Today.AddYears(-20),Active=true,Earnings= 99.9m,Comments="CSharp books", Rate=10, City="London"});
				authors.Add(new Author(){Name="Angel Colmenares",Birthday= DateTime.Today.AddYears(-25),Active=true,Earnings= 50.0m,Comments="CSharp books", Rate=5, City="Bogota"});
				authors.Add(new Author(){Name="Adam Witco",Birthday= DateTime.Today.AddYears(-20),Active=true,Earnings= 80.0m,Comments="Math Books", Rate=9, City="London"});
				authors.Add(new Author(){Name="Claudia Espinel",Birthday= DateTime.Today.AddYears(-23),Active=true,Earnings= 60.0m,Comments="Cooking books", Rate=10, City="Bogota"});
				authors.Add(new Author(){Name="Libardo Pajaro",Birthday= DateTime.Today.AddYears(-25),Active=true,Earnings= 80.0m,Comments="CSharp books", Rate=9, City="Bogota"});
				authors.Add(new Author(){Name="Jorge Garzon",Birthday= DateTime.Today.AddYears(-28),Active=true,Earnings= 70.0m,Comments="CSharp books", Rate=9, City="Bogota"});
				authors.Add(new Author(){Name="Alejandro Isaza",Birthday= DateTime.Today.AddYears(-20),Active=true,Earnings= 70.0m,Comments="Java books", Rate=0, City="Bogota"});
				authors.Add(new Author(){Name="Wilmer Agamez",Birthday= DateTime.Today.AddYears(-20),Active=true,Earnings= 30.0m,Comments="Java books", Rate=0, City="Cartagena"});
				authors.Add(new Author(){Name="Rodger Contreras",Birthday= DateTime.Today.AddYears(-25),Active=true,Earnings= 90.0m,Comments="CSharp books", Rate=8, City="Cartagena"});
				authors.Add(new Author(){Name="Chuck Benedict",Birthday= DateTime.Today.AddYears(-22),Active=true,Earnings= 85.5m,Comments="CSharp books", Rate=8, City="London"});
				authors.Add(new Author(){Name="James Benedict II",Birthday= DateTime.Today.AddYears(-22),Active=true,Earnings= 85.5m,Comments="Java books", Rate=5, City="Berlin"});
				authors.Add(new Author(){Name="Ethan Brown",Birthday= DateTime.Today.AddYears(-20),Active=true,Earnings= 45.0m,Comments="CSharp books", Rate=5, City="Madrid"});
				authors.Add(new Author(){Name="Xavi Garzon",Birthday= DateTime.Today.AddYears(-22),Active=true,Earnings= 75.0m,Comments="CSharp books", Rate=9, City="Madrid"});
				authors.Add(new Author(){Name="Luis garzon",Birthday= DateTime.Today.AddYears(-22),Active=true,Earnings= 85.0m,Comments="CSharp books", Rate=10, City="Mexico"});
				
				dbCmd.InsertAll(authors);
				
				
				// lets start !
				
				// select authors born 20 year ago
				int year = DateTime.Today.AddYears(-20).Year;
				int expected=5;
				
				ev.Where(rn=> rn.Birthday>=new DateTime(year, 1,1) && rn.Birthday<=new DateTime(year, 12,31));
				List<Author> result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected==result.Count);
				result = dbCmd.Select<Author>(qry => qry.Where(rn => rn.Birthday >= new DateTime(year, 1, 1) && rn.Birthday <= new DateTime(year, 12, 31)));
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
				result = dbCmd.Select<Author>(rn => rn.Birthday >= new DateTime(year, 1, 1) && rn.Birthday <= new DateTime(year, 12, 31));
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
				
				// select authors from London, Berlin and Madrid : 6
				expected=6;
				ev.Where(rn=> Sql.In( rn.City, new object[]{"London", "Madrid", "Berlin"}) );
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected==result.Count);
				
				// select authors from Bogota and Cartagena : 7
				expected=7;
				ev.Where(rn => Sql.In(rn.City, new object[] { "Bogota", "Cartagena" }));
				result = dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected==result.Count);
				result = dbCmd.Select<Author>(rn => Sql.In(rn.City, "Bogota", "Cartagena"));
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
				
				
				// select authors which name starts with A
				expected=3;
				ev.Where(rn=>  rn.Name.StartsWith("A") );
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected==result.Count);
				
				// select authors which name ends with Garzon o GARZON o garzon ( no case sensitive )
				expected=3;
				ev.Where(rn=>  rn.Name.ToUpper().EndsWith("GARZON") );
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected==result.Count);
				
				// select authors which name ends with garzon ( no case sensitive )
				expected=3;
				ev.Where(rn=>  rn.Name.EndsWith("garzon") );
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected==result.Count);
				
				
				// select authors which name contains  Benedict 
				expected=2;
				ev.Where(rn=>  rn.Name.Contains("Benedict") );
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected==result.Count);
				
				
				// select authors with Earnings <= 50 
				expected=3;
				ev.Where(rn=>  rn.Earnings<=50 );
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected==result.Count);
				
				// select authors with Rate = 10 and city=Mexio 
				expected=1;
				ev.Where(rn=>  rn.Rate==10 && rn.City=="Mexico");
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected==result.Count);
			
				//  enough selecting, lets update;
				// set Active=false where rate =0
				expected=2;
				ev.Where(rn=>  rn.Rate==0 ).Update(rn=> rn.Active);
				var rows = dbCmd.UpdateOnly( new Author(){ Active=false }, ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, rows, expected==rows);
			
				// insert values  only in Id, Name, Birthday, Rate and Active fields 
				expected=4;
				ev.Insert(rn =>new { rn.Id, rn.Name, rn.Birthday, rn.Active, rn.Rate} );
				dbCmd.InsertOnly( new Author(){Active=false, Rate=0, Name="Victor Grozny", Birthday=DateTime.Today.AddYears(-18)   }, ev);
				dbCmd.InsertOnly( new Author(){Active=false, Rate=0, Name="Ivan Chorny", Birthday=DateTime.Today.AddYears(-19)   }, ev);
				ev.Where(rn=> !rn.Active);
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected==result.Count);
				
				//update comment for City == null 
				expected=2;
				ev.Where( rn => rn.City==null ).Update(rn=> rn.Comments);
				rows=dbCmd.UpdateOnly(new Author(){Comments="No comments"}, ev);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, rows, expected==rows);
				
				// delete where City is null 
				expected=2;
				rows = dbCmd.Delete( ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, rows, expected==rows);
			
				
				//   lets select  all records ordered by Rate Descending and Name Ascending
				expected=14;
				ev.Where().OrderBy(rn=> new{ at=Sql.Desc(rn.Rate), rn.Name }); // clear where condition
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected==result.Count);
				Console.WriteLine(ev.OrderByExpression);
				var author = result.FirstOrDefault();
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Claudia Espinel", author.Name, "Claudia Espinel"==author.Name);
				
				// select  only first 5 rows ....
				
				expected=5;
				ev.Limit(5); // note: order is the same as in the last sentence
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected==result.Count);
					
					
				// lets select only Name and City (name will be "UPPERCASED" )
			
				ev.Select(rn=> new { at= Sql.As( rn.Name.ToUpper(), "Name" ), rn.City} );
				Console.WriteLine(ev.SelectExpression);
				result=dbCmd.Select(ev);
				author = result.FirstOrDefault();
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Claudia Espinel".ToUpper(), author.Name, "Claudia Espinel".ToUpper()==author.Name);
				
				//paging :
				ev.Limit(0,4);// first page, page size=4;
				result=dbCmd.Select(ev);
				author = result.FirstOrDefault();
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Claudia Espinel".ToUpper(), author.Name, "Claudia Espinel".ToUpper()==author.Name);
				
				ev.Limit(4,4);// second page
				result=dbCmd.Select(ev);
				author = result.FirstOrDefault();
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Jorge Garzon".ToUpper(), author.Name, "Jorge Garzon".ToUpper()==author.Name);
				
				ev.Limit(8,4);// third page
				result=dbCmd.Select(ev);
				author = result.FirstOrDefault();
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Rodger Contreras".ToUpper(), author.Name, "Rodger Contreras".ToUpper()==author.Name);
				
			
				// select distinct..
				ev.Limit().OrderBy(); // clear limit  and order for postgres
				ev.SelectDistinct(r=>r.City);
				expected=6;
				result=dbCmd.Select(ev);	
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected==result.Count);

				Console.WriteLine();
				// Tests for predicate overloads that make use of the expression visitor
				Console.WriteLine("First author by name (exists)");
				author = dbCmd.First<Author>(a => a.Name == "Jorge Garzon");
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Jorge Garzon", author.Name, "Jorge Garzon" == author.Name);

				try
				{
					Console.WriteLine("First author by name (does not exist)");
					author = dbCmd.First<Author>(a => a.Name == "Does not exist");

					Console.WriteLine("Expected exception thrown, OK? False");
				}
				catch
				{
					Console.WriteLine("Expected exception thrown, OK? True");
				}

				Console.WriteLine("First author or default (does not exist)");
				author = dbCmd.FirstOrDefault<Author>(a => a.Name == "Does not exist");
				Console.WriteLine("Expected:null ; OK? {0}", author == null);

				Console.WriteLine("First author or default by city (multiple matches)");
				author = dbCmd.FirstOrDefault<Author>(a => a.City == "Bogota");
				Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Angel Colmenares", author.Name, "Angel Colmenares" == author.Name);
				
				Console.ReadLine();
				Console.WriteLine("Press Enter to continue");
				
			}
			
			Console.WriteLine ("This is The End my friend!");
		}
	}
}