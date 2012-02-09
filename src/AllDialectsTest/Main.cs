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
		
		public static void Main (string[] args)
		{
					
			bool exit=false;
			
			dialects= BuildDialectList();
			authors= BuildAuthorList();
			
			PaintMenu();
			
			while(!exit){
								
				Console.WriteLine("Select your option [{0}-{1}] or q to quit  and press ENTER", 1, dialects.Count);
				string option = Console.ReadLine();
				if(string.IsNullOrEmpty( option))
					Console.WriteLine("NO VALID OPTION");
				else if( option.ToUpper()=="Q")
					exit=true;
				else {
					int opt;
					if (int.TryParse(option, out opt)){
						if(opt>=1 && opt<=dialects.Count)
							TestDialect(dialects[opt-1]);
						else{
							Console.WriteLine("NO VALID OPTION");
						}
						
					}
					else
						Console.WriteLine("NO VALID OPTION");
				}	
			}
			
			
		}
		
		private static void PaintMenu(){
			Console.Clear();
			int i=0;
			foreach(Dialect d in dialects){
				Console.WriteLine("{0} {1}", ++i, d.Name); 
			}
			Console.WriteLine("q quit"); 
		}
		
		private static List<Dialect> BuildDialectList(){
			List<Dialect> l = new List<Dialect>();
			Dialect d = new Dialect(){
				Name="Sqlite", 
				PathToAssembly="../../../ServiceStack.OrmLite.Sqlite/bin/Debug",
				AssemblyName="ServiceStack.OrmLite.Sqlite.dll",
				ClassName="ServiceStack.OrmLite.Sqlite.SqliteOrmLiteDialectProvider",
				InstanceFieldName="Instance",
				ConnectionString= "~/db.sqlite".MapAbsolutePath()
			};
			l.Add(d);
			
			d = new Dialect(){
				Name="SqlServer", 
				PathToAssembly="../../../ServiceStack.OrmLite.SqlServer/bin/Debug",
				AssemblyName="ServiceStack.OrmLite.SqlServer.dll",
				ClassName="ServiceStack.OrmLite.SqlServer.SqlServerOrmLiteDialectProvider",
				InstanceFieldName="Instance",
				ConnectionString= "~/test.mdf".MapAbsolutePath()
			};
			l.Add(d);
			
			d = new Dialect()
				{Name="MySql",
				PathToAssembly="../../../ServiceStack.OrmLite.MySql/bin/Debug",
				AssemblyName="ServiceStack.OrmLite.MySql.dll",
				ClassName="ServiceStack.OrmLite.MySql.MySqlDialectProvider",
				InstanceFieldName="Instance",
				ConnectionString= "Server = 127.0.0.1; Database = ormlite; Uid = root; Pwd = password"
			};
			l.Add(d);
			
			d = new Dialect(){
				Name="PostgreSQL", 
				PathToAssembly="../../../ServiceStack.OrmLite.PostgreSQL/bin/Debug", 
				AssemblyName="ServiceStack.OrmLite.PostgreSQL.dll", 
				ClassName="ServiceStack.OrmLite.PostgreSQL.PostgreSQLDialectProvider", 
				InstanceFieldName="Instance",
				ConnectionString="Server=localhost;Port=5432;User Id=postgres; Password=postgres; Database=ormlite"
			};
			l.Add(d);
			
			d = new Dialect()
				{Name="FirebirdSql",
				PathToAssembly="../../../ServiceStack.OrmLite.Firebird/bin/Debug", 
				AssemblyName="ServiceStack.OrmLite.Firebird.dll", 
				ClassName="ServiceStack.OrmLite.Firebird.FirebirdOrmLiteDialectProvider", 
				InstanceFieldName="Instance",
				ConnectionString="User=SYSDBA;Password=masterkey;Database=employee.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;"
			};
			l.Add(d);
			
			return l;
			
		}
		
		private static List<Author> BuildAuthorList(){
			
			List<Author> a = new List<Author>();
			a.Add(new Author(){Name="Demis Bellot",Birthday= DateTime.Today.AddYears(-20),Active=true,Earnings= 99.9m,Comments="CSharp books", Rate=10, City="London"});
			a.Add(new Author(){Name="Angel Colmenares",Birthday= DateTime.Today.AddYears(-25),Active=true,Earnings= 50.0m,Comments="CSharp books", Rate=5, City="Bogota"});
			a.Add(new Author(){Name="Adam Witco",Birthday= DateTime.Today.AddYears(-20),Active=true,Earnings= 80.0m,Comments="Math Books", Rate=9, City="London"});
			a.Add(new Author(){Name="Claudia Espinel",Birthday= DateTime.Today.AddYears(-23),Active=true,Earnings= 60.0m,Comments="Cooking books", Rate=10, City="Bogota"});
			a.Add(new Author(){Name="Libardo Pajaro",Birthday= DateTime.Today.AddYears(-25),Active=true,Earnings= 80.0m,Comments="CSharp books", Rate=9, City="Bogota"});
			a.Add(new Author(){Name="Jorge Garzon",Birthday= DateTime.Today.AddYears(-28),Active=true,Earnings= 70.0m,Comments="CSharp books", Rate=9, City="Bogota"});
			a.Add(new Author(){Name="Alejandro Isaza",Birthday= DateTime.Today.AddYears(-20),Active=true,Earnings= 70.0m,Comments="Java books", Rate=0, City="Bogota"});
			a.Add(new Author(){Name="Wilmer Agamez",Birthday= DateTime.Today.AddYears(-20),Active=true,Earnings= 30.0m,Comments="Java books", Rate=0, City="Cartagena"});
			a.Add(new Author(){Name="Rodger Contreras",Birthday= DateTime.Today.AddYears(-25),Active=true,Earnings= 90.0m,Comments="CSharp books", Rate=8, City="Cartagena"});
			a.Add(new Author(){Name="Chuck Benedict",Birthday= DateTime.Today.AddYears(-22),Active=true,Earnings= 85.5m,Comments="CSharp books", Rate=8, City="London"});
			a.Add(new Author(){Name="James Benedict II",Birthday= DateTime.Today.AddYears(-22),Active=true,Earnings= 85.5m,Comments="Java books", Rate=5, City="Berlin"});
			a.Add(new Author(){Name="Ethan Brown",Birthday= DateTime.Today.AddYears(-20),Active=true,Earnings= 45.0m,Comments="CSharp books", Rate=5, City="Madrid"});
			a.Add(new Author(){Name="Xavi Garzon",Birthday= DateTime.Today.AddYears(-22),Active=true,Earnings= 75.0m,Comments="CSharp books", Rate=9, City="Madrid"});
			a.Add(new Author(){Name="Luis garzon",Birthday= DateTime.Today.AddYears(-22),Active=true,Earnings= 85.0m,Comments="CSharp books", Rate=10, City="Mexico"});
			return a;	
		}
		
		
		
		private static void TestDialect(Dialect dialect){
			Console.Clear();
			Console.WriteLine("Testing expressions for Dialect {0}", dialect.Name);
						
			OrmLiteConfig.ClearCache();
			OrmLiteConfig.DialectProvider=dialect.DialectProvider;
			SqlExpressionVisitor<Author> ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<Author>();
									
			using (IDbConnection db =
			       dialect.ConnectionString.OpenDbConnection())
			using ( IDbCommand dbCmd = db.CreateCommand())
			{
				try{
				
				dbCmd.DropTable<Author>();
				dbCmd.CreateTable<Author>();
				dbCmd.DeleteAll<Author>();
				Console.WriteLine("Inserting...");
				DateTime t1= DateTime.Now;
				dbCmd.InsertAll(authors);
				DateTime t2= DateTime.Now;
				Console.WriteLine("Inserted {0} rows in {1}", authors.Count, t2-t1);
								
				Console.WriteLine("Selecting.....");
				
				int year = DateTime.Today.AddYears(-20).Year;
				int expected=5;
				
				ev.Where(rn=> rn.Birthday>=new DateTime(year, 1,1) && rn.Birthday<=new DateTime(year, 12,31));
				List<Author> result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected==result.Count?"OK":"********* FAILED *********");
				result = dbCmd.Select<Author>(qry => qry.Where(rn => rn.Birthday >= new DateTime(year, 1, 1) && rn.Birthday <= new DateTime(year, 12, 31)));
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count?"OK":"********* FAILED *********");
				result = dbCmd.Select<Author>(rn => rn.Birthday >= new DateTime(year, 1, 1) && rn.Birthday <= new DateTime(year, 12, 31));
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count?"OK":"********* FAILED *********");
				
				// select authors from London, Berlin and Madrid : 6
				expected=6;
				//Sql.In can take params object[]
				ev.Where(rn=> Sql.In( rn.City, new object[]{"London", "Madrid", "Berlin"}) );
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected==result.Count?"OK":"********* FAILED *********");
				result = dbCmd.Select<Author>(rn => Sql.In(rn.City, new[] { "London", "Madrid", "Berlin" }));
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count?"OK":"********* FAILED *********");

				// select authors from Bogota and Cartagena : 7
				expected=7;
				//... or Sql.In can  take List<Object>
				List<Object> cities= new List<Object>();
				cities.Add("Bogota");
				cities.Add("Cartagena");
				ev.Where(rn => Sql.In(rn.City, cities ));
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected==result.Count?"OK":"********* FAILED *********");
				result = dbCmd.Select<Author>(rn => Sql.In(rn.City, "Bogota", "Cartagena"));
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count?"OK":"********* FAILED *********");
				
				
				// select authors which name starts with A
				expected=3;
				ev.Where(rn=>  rn.Name.StartsWith("A") );
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected==result.Count?"OK":"********* FAILED *********");
				result = dbCmd.Select<Author>(rn => rn.Name.StartsWith("A"));
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count?"OK":"********* FAILED *********");
				
				// select authors which name ends with Garzon o GARZON o garzon ( no case sensitive )
				expected=3;
				ev.Where(rn=>  rn.Name.ToUpper().EndsWith("GARZON") );
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected==result.Count?"OK":"********* FAILED *********");
				result = dbCmd.Select<Author>(rn => rn.Name.ToUpper().EndsWith("GARZON"));
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count?"OK":"********* FAILED *********");
				
				// select authors which name ends with garzon
				//A percent symbol ("%") in the LIKE pattern matches any sequence of zero or more characters 
				//in the string. 
				//An underscore ("_") in the LIKE pattern matches any single character in the string. 
				//Any other character matches itself or its lower/upper case equivalent (i.e. case-insensitive matching).
				expected=3;
				ev.Where(rn=>  rn.Name.EndsWith("garzon") );
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected==result.Count?"OK":"********* FAILED *********");
				result = dbCmd.Select<Author>(rn => rn.Name.EndsWith("garzon"));
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count?"OK":"********* FAILED *********");
				
				
				// select authors which name contains  Benedict 
				expected=2;
				ev.Where(rn=>  rn.Name.Contains("Benedict") );
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected==result.Count?"OK":"********* FAILED *********");
				result = dbCmd.Select<Author>(rn => rn.Name.Contains("Benedict"));
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count?"OK":"********* FAILED *********");
				
				
				// select authors with Earnings <= 50 
				expected=3;
				ev.Where(rn=>  rn.Earnings<=50 );
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected==result.Count?"OK":"********* FAILED *********");
				result = dbCmd.Select<Author>(rn => rn.Earnings <= 50);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count?"OK":"********* FAILED *********");
				
				// select authors with Rate = 10 and city=Mexio 
				expected=1;
				ev.Where(rn=>  rn.Rate==10 && rn.City=="Mexico");
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected==result.Count?"OK":"********* FAILED *********");
				result = dbCmd.Select<Author>(rn => rn.Rate == 10 && rn.City == "Mexico");
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected == result.Count?"OK":"********* FAILED *********");
			
				//  enough selecting, lets update;
				// set Active=false where rate =0
				expected=2;
				ev.Where(rn=>  rn.Rate==0 ).Update(rn=> rn.Active);
				var rows = dbCmd.Update( new Author(){ Active=false }, ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, rows, expected==rows?"OK":"********* FAILED *********");
			
				// insert values  only in Id, Name, Birthday, Rate and Active fields 
				expected=4;
				ev.Insert(rn =>new { rn.Id, rn.Name, rn.Birthday, rn.Active, rn.Rate} );
				dbCmd.Insert( new Author(){Active=false, Rate=0, Name="Victor Grozny", Birthday=DateTime.Today.AddYears(-18)   }, ev);
				dbCmd.Insert( new Author(){Active=false, Rate=0, Name="Ivan Chorny", Birthday=DateTime.Today.AddYears(-19)   }, ev);
				ev.Where(rn=> !rn.Active);
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected==result.Count?"OK":"********* FAILED *********");
				
				//update comment for City == null 
				expected=2;
				ev.Where( rn => rn.City==null ).Update(rn=> rn.Comments);
				rows=dbCmd.Update(new Author(){Comments="No comments"}, ev);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, rows, expected==rows?"OK":"********* FAILED *********");
				
				// delete where City is null 
				expected=2;
				rows = dbCmd.Delete( ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, rows, expected==rows?"OK":"********* FAILED *********");
					
				
				//   lets select  all records ordered by Rate Descending and Name Ascending
				expected=14;
				ev.Where().OrderBy(rn=> new{ at=Sql.Desc(rn.Rate), rn.Name }); // clear where condition
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected==result.Count?"OK":"********* FAILED *********");
				Console.WriteLine(ev.OrderByExpression);
				var author = result.FirstOrDefault();
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", "Claudia Espinel", author.Name, "Claudia Espinel"==author.Name?"OK":"********* FAILED *********");
				
				// select  only first 5 rows ....
				
				expected=5;
				ev.Limit(5); // note: order is the same as in the last sentence
				result=dbCmd.Select(ev);
				Console.WriteLine(ev.WhereExpression);
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected==result.Count?"OK":"********* FAILED *********");
					
					
				// and finally lets select only Name and City (name will be "UPPERCASED" )
			
				ev.Select(rn=> new { at= Sql.As( rn.Name.ToUpper(), "Name" ), rn.City} );
				Console.WriteLine(ev.SelectExpression);
				result=dbCmd.Select(ev);
				author = result.FirstOrDefault();
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", "Claudia Espinel".ToUpper(), author.Name, "Claudia Espinel".ToUpper()==author.Name?"OK":"********* FAILED *********");
				
				//paging :
				ev.Limit(0,4);// first page, page size=4;
				result=dbCmd.Select(ev);
				author = result.FirstOrDefault();
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", "Claudia Espinel".ToUpper(), author.Name, "Claudia Espinel".ToUpper()==author.Name?"OK":"********* FAILED *********");
				
				ev.Limit(4,4);// second page
				result=dbCmd.Select(ev);
				author = result.FirstOrDefault();
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", "Jorge Garzon".ToUpper(), author.Name, "Jorge Garzon".ToUpper()==author.Name?"OK":"********* FAILED *********");
				
				ev.Limit(8,4);// third page
				result=dbCmd.Select(ev);
				author = result.FirstOrDefault();
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", "Rodger Contreras".ToUpper(), author.Name, "Rodger Contreras".ToUpper()==author.Name?"OK":"********* FAILED *********");			
				
				// select distinct..
				ev.Limit().OrderBy(); // clear limit, clear order for postres
				ev.SelectDistinct(r=>r.City);
				expected=6;
				result=dbCmd.Select(ev);	
				Console.WriteLine("Expected:{0}  Selected:{1}  {2}", expected, result.Count, expected==result.Count?"OK":"********* FAILED *********");
				
				DateTime t3= DateTime.Now;	
				Console.WriteLine("Expressions test in: {0}", t3-t2); 	
				Console.WriteLine("All test in :        {0}", t3-t1); 	
					
				}
				catch(Exception e){
					Console.WriteLine(e.Message);
				}
				
			
			}
			
			
			
			
			Console.WriteLine("Press enter to return to main menu");
			Console.ReadLine();
			PaintMenu();
		}
		
	}
}
