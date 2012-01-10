using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

using ServiceStack.Common.Utils;
using ServiceStack.DataAnnotations;
using ServiceStack.Common.Extensions;

using System.Data;

using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Firebird;

namespace TestLiteFirebird2
{
	[Alias("COMPANY")]
	public class Company
	{
		
		[Alias("ID")]
		[Sequence("COMPANY_ID_GEN")]   
		[PrimaryKey]
		public long Id { get; set; }
		
		[Alias("NAME")]
    	public string Name { get; set; }
		
		[Alias("TURNOVER")]
    	public Nullable<float>  TurnOver { get; set; }
		
		[Alias("STARTED")]
    	public Nullable<DateTime> Started { get; set; }
		
		[Alias("EMPLOYEES")]
    	public Nullable<int> Employees { get; set; }
			
		[Alias("CREATED_DATE")]
    	public Nullable<DateTime> CreatedDate { get; set; }
		
		[Ignore]
		public string UpperName{
			get { return Name.ToUpper();}
		}
	}
	
	class MainClass
	{
		public static void Main (string[] args)
		{
									
			//Set one before use (i.e. in a static constructor).
			OrmLiteConfig.DialectProvider = new FirebirdOrmLiteDialectProvider();
			
			
			using (IDbConnection db =
			       "User=SYSDBA;Password=masterkey;Database=employee.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;".OpenDbConnection())
			using ( IDbCommand dbConn = db.CreateCommand())
			{
				try{
					
					Console.WriteLine( dbConn.HasChildren<Company>( new Company(){Id=100} ) );
					
					Console.WriteLine(dbConn.HasChildren<Company>( new Company(){Id=5} )) ;
					
					Console.WriteLine( dbConn.Exists<Company>( "Id='{0}'",5 ) );
					
					Console.WriteLine(dbConn.Exists<Company>( "Id='{0}'",100 )) ;
					
					
					var rows = dbConn.Select<Company>();
					Console.WriteLine("Company: rows before insert :{0}", rows.Count);
					
					Company cp = new Company{ 
						Name="One More Company", Employees=10,
						Started= DateTime.Today, TurnOver= 12525,
						CreatedDate= DateTime.Now
					};
			
					
					dbConn.Insert( cp);
					
    				rows = dbConn.Select<Company>();
					Console.WriteLine("Company: rows after  insert :{0}", rows.Count);
					foreach(Company u in rows){
						Console.WriteLine("{0} -- {1}" , u.Id, u.Name);
					}
					Console.WriteLine("----------------------");
					
					rows = dbConn.Select<Company>("ID>={0} order by ID descending  rows 5", 10); 
					Console.WriteLine(rows.Count);
					foreach(Company u in rows){
						Console.WriteLine("{0} -- {1} -- {2} -- {3} -- {4} -- {5} --{6}", u.Id, u.Name,
						                  (u.Employees.HasValue)?u.Employees.Value.ToString():"",
						                   u.Started.HasValue?u.Started.Value.ToString():"",
						                  u.TurnOver.HasValue?u.TurnOver.Value.ToString():"",
						                  u.CreatedDate.HasValue?u.CreatedDate.Value.ToString():"",
						                  u.UpperName
						                  );
					}
					
					
					
				}	
				
				catch(Exception e){
					Console.WriteLine(e);
				}

    //Assert.That(rows, Has.Count(1));
    //Assert.That(rows[0].Id, Is.EqualTo(1));
			}	
		}
		
		
	}
	

	//s.Substring(7, s.IndexOf("FROM")-8)  --> columns
	//s.Substring( s.IndexOf("FROM")+5)
	
}
