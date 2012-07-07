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
using ServiceStack.OrmLite.Firebird;


namespace TestLiteFirebird00
{
	
	public class Author{
		
		public Author(){
		}
		
		[AutoIncrement]
		//[Sequence("Author_Id_GEN")]
		public Int32 Id { get; set;}
		
		[Required]
		[Index(Unique = true)]
		[StringLength(40)]
		public string Name { get; set;}
		
		[Required]
		public DateTime Birthday { get; set;}
		
		public DateTime ? LastActivity  { get; set;}
		
		public Decimal? Earnings { get; set;}  // Precision=18, scale=12 default values
		
		//[Alias("Active")] // Active Firebird Reserved  ?
		public bool Active { get; set; } 
		
		[StringLength(80)]
		public string City { get; set;}
		
		[StringLength(80)]
		public string Comments { get; set;}
		
		public Int16 Rate{ get; set;}
		
		
	}
	
	public class Book{
		
		public Book(){
		}
		
		[AutoIncrement]
		//[Sequence("Book_Id_GEN")]
		public Int32 Id { get; set;}
		
		[References(typeof(Author))]
		public Int32 IdAuthor {get; set;}
		
		[StringLength(80)]
		public string Title {get; set;}
		[DecimalLength(15,2)]
		public Decimal Price { get; set;}  // Precision= 15, Scale=2
		
	}
	
	
	class MainClass
	{
		public static void Main (string[] args)
		{
			OrmLiteConfig.DialectProvider = new FirebirdOrmLiteDialectProvider();
									
			using (IDbConnection db =
			       "User=SYSDBA;Password=masterkey;Database=employee.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;".OpenDbConnection())
			using ( IDbCommand dbCmd = db.CreateCommand())
			{
			//try{
				// due to firebirdslq features, we have to drop book first  and then author
				dbCmd.DropTable<Book>();
				dbCmd.DropTable<Author>();
				
				dbCmd.CreateTable<Author>();
				dbCmd.CreateTable<Book>();
				
				dbCmd.Insert( new Author(){
					Name="Demis Bellot",
					Birthday= DateTime.Today.AddYears(20),
					Active=true,
					Earnings= 99.9m,
					Comments="ServiceStack.Net ...",
					City="London",
					Rate=10
				});
				
				dbCmd.Insert( new Author(){
					Name="Angel Colmenares",
					Birthday= DateTime.Today.AddYears(30),
					Active=true,
					Earnings= 50.25m,
					Comments="OrmLite.Firebird",
					City="Bogota",
					Rate=9
				});
				
				dbCmd.Insert( new Author(){
					Name="Adam Witco",
					Birthday= DateTime.Today.AddYears(25),
					Active=true,
					Comments="other books...",
					City="London",
					Rate=8
				});
			
				
				dbCmd.Insert( new Author(){
					Name="Claudia Espinel",
					Birthday= DateTime.Today.AddYears(28),
					Active=false,
					Comments="other books...",
					City="Bogota",
					Rate=10
				});
				
				//-------------------------------------------------------------------
				SqlExpressionVisitor<Author> ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<Author>();
				
				ev.Insert(r=> new {r.Id, r.Name, r.Birthday, r.Active, r.Rate}); // fields to insert
				
				var author = new Author(){
					Name="William",
					Birthday= DateTime.Today.AddYears(250),
					Active=false,
					City="London",
					Rate= 0,
					Comments="this will not be inserted" // null in db
				};
			
				dbCmd.InsertOnly(author, ev);
				
				author.Comments="this will be updated";
				
				ev.Update(rn=> rn.Comments).Where(r=>r.Id==author.Id);
				dbCmd.UpdateOnly(author, ev);
				
				
				// update comment for all authors from london...
				
				author.Comments="update from london";
				ev.Where(rn=> rn.City=="London");
				dbCmd.UpdateOnly(author, ev);
				
				// select author from Bogota
				ev.Where(rn=> rn.City=="Bogota");
				var authors = dbCmd.Select(ev);
				Console.WriteLine(authors.Count);
				
				// select author from Bogota and Active=true;
				
				ev.Where(rn=> rn.City=="Bogota" && rn.Active==true); // sorry for firebird must write ==true !
				authors = dbCmd.Select(ev);
				Console.WriteLine(authors.Count);
				
				//-------------------------------------------------------------------
				authors = dbCmd.Select<Author>();
				
				Console.WriteLine("Rows in  Author : '{0}'",authors.Count);
				
				foreach(Author a in authors){
					Console.WriteLine("Id :{0} - Name : {1} -- Earnings {2}", a.Id,
						a.Name,
						a.Earnings.HasValue? a.Earnings.Value: 0.0m);
				}
				
				author= authors.FirstOrDefault<Author>(r=>r.Name=="Angel Colmenares");
				if( author != default(Author) ){
					
					dbCmd.Insert( new Book(){
						IdAuthor= author.Id,
						Title= "The big book",
						Price= 18.55m,
					});
					Console.WriteLine("{0} == {1}", dbCmd.HasChildren<Book>( author), true ) ;
				}
				else{
					Console.WriteLine("Something wrong ");
				}
				
				
				author= authors.FirstOrDefault<Author>(r=>r.Name=="Adam Witco");
				if( author != default(Author) ){
					
					
					Console.WriteLine("{0} == {1}", dbCmd.HasChildren<Book>( author), false ) ;
				}
				else{
					Console.WriteLine("Something wrong ");
				}
				
				var books = dbCmd.Select<Book>();
				
				foreach(var b in books){
					Console.WriteLine("Title {0}  Price {1}",b.Title, b.Price);
				}
				
				ev.Select(r=>new { r.Name, r.Active}).Where(); // only Name and Active fields will be retrived
								
				authors = dbCmd.Select(ev);
				Console.WriteLine(ev.SelectExpression);
				
				foreach(Author r in authors){
					Console.WriteLine("'{0}' '{1}' '{2}'", r.Name, r.Active, r.Id);
				}
				
				
				dbCmd.DeleteAll<Book>();
				dbCmd.DeleteAll<Author>();
				
				
				//}
				
				//catch(Exception e){
				//	Console.WriteLine("Error : " + e.Message);
				//	return;
				//}
				Console.WriteLine("This is The End my friend !");
				
			}
		}
	}
}