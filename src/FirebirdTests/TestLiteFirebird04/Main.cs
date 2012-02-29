using System;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Collections.Generic;

using ServiceStack.Common.Utils;
using ServiceStack.DataAnnotations;
using ServiceStack.Common.Extensions;

using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Firebird;

using Database.Records;

namespace TestLiteFirebird04
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");
			
			OrmLiteConfig.DialectProvider = new FirebirdOrmLiteDialectProvider();
			
			ServiceStack.OrmLite.SqlExpressionVisitor<Company> sql= 
				new  FirebirdSqlExpressionVisitor<Company>();
			
			List<Object> names = new List<Object>();
			names.Add("SOME COMPANY");
			names.Add("XYZ");
			

			List<Object> ids = new List<Object>();
			ids.Add(1);
			ids.Add(2);
						
			Company company = new Company(){ Id=1, Name ="XYZ"};
			Console.WriteLine( company.Id.In(ids) );
			Console.WriteLine( company.Name.In(names) );
							
					
			sql.Where( cp => cp.Name == "On more Company");
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => cp.Name != "On more Company");
			Console.WriteLine(sql.WhereExpression);
			
			
			sql.Where( cp => cp.Name == null );
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => cp.Name != null);
			Console.WriteLine(sql.WhereExpression);
			
			
			
			sql.Where( cp => cp.SomeBoolean);   // TODO : fix 
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => !cp.SomeBoolean && 1==1); //TODO : fix
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => cp.SomeBoolean && 1==1); //TODO : fix
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => 1 == 1 );  // TODO : fix ?
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => "1"== "1"); // TODO : fix  ?
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => "1"== "0"); // TODO : fix  ?
			Console.WriteLine(sql.WhereExpression);
			
			
			sql.Where( cp => 1 != 1);    //ok 
			Console.WriteLine(sql.WhereExpression);
						
			sql.Where( cp => cp.SomeBoolean==true); //OK    
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => cp.SomeBoolean==false);   //OK
			Console.WriteLine(sql.WhereExpression);
						
			
			sql.Where( cp => !cp.SomeBoolean);    // OK
			Console.WriteLine(sql.WhereExpression);
											

			sql.Where( cp => (cp.Name==cp.Name) );
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => (cp.Name=="On more Company" || cp.Id>30) );
			Console.WriteLine(sql.WhereExpression);
			
			
			sql.Where( cp => (cp.CreatedDate== DateTime.Today ));
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => (cp.CreatedDate==DateTime.Today && (cp.Name=="On more Company" || cp.Id>30)) );
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => ( cp.Name.ToUpper()=="ONE MORE COMPANY" ));
			Console.WriteLine(sql.WhereExpression);
						
			sql.Where( cp => ( cp.Name.ToLower()=="ONE MORE COMPANY".ToLower() ) );
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => ( cp.Name.ToLower().StartsWith("one") ) );
			Console.WriteLine(sql.WhereExpression);
		
			sql.Where( cp => ( cp.Name.ToUpper().EndsWith("COMPANY") ) );
			Console.WriteLine(sql.WhereExpression);
			sql.Where( cp => ( cp.Name.ToUpper().Contains("MORE") ) );
			Console.WriteLine(sql.WhereExpression);
			sql.Where( cp => ( cp.Name.Substring(0)=="ONE MORE COMPANY") );
			Console.WriteLine(sql.WhereExpression);
			sql.Where( cp => ( cp.Name.ToUpper().Substring(0,7)=="ONE MOR" ));
			Console.WriteLine(sql.WhereExpression);
			
						
			sql.Where( cp => (cp.CreatedDate>= new DateTime(2000,1,1 ) ) );
			Console.WriteLine(sql.WhereExpression);
					
			sql.Where( cp => (cp.Employees/2 > 10.0 ) );
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => (cp.Employees*2 > 10.0/5 ));
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => ((cp.Employees+3) > (10.0+5) ) );
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => ((cp.Employees-3) > (10.0+5) ) );
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => (( cp.Employees%3) > (10.0+5)) );
			Console.WriteLine(sql.WhereExpression);
			
			
			
			sql.Where( cp => ( Math.Round( cp.SomeDouble) > (10.0+5) ) );
			Console.WriteLine(sql.WhereExpression);
			
			
			sql.Where( cp => ( Math.Round( cp.SomeDouble,3) > (10.0+5) ) );
			Console.WriteLine(sql.WhereExpression);
			
			
			sql.Where( cp => ( Math.Floor( cp.SomeDouble) > (10.0+5) ) );
			Console.WriteLine(sql.WhereExpression);
			
			
			sql.Where( cp => ( Math.Ceiling( cp.SomeDouble) > (10.0+5) ) );
			Console.WriteLine(sql.WhereExpression);
			
			
			
			sql.Where( cp => ( string.Concat( cp.SomeDouble, "XYZ")  =="SOME COMPANY XYZ") );
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => ( string.Concat( cp.SomeDouble, "X", "Y","Z") =="SOME COMPANY XYZ"));
			Console.WriteLine(sql.WhereExpression);
			
			
			sql.Where( cp => ( string.Concat( cp.Name, "X", "Y","Z") =="SOME COMPANY XYZ"));
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => ( string.Concat( cp.SomeDouble.ToString(), "X", "Y","Z") =="SOME COMPANY XYZ"));
			Console.WriteLine(sql.WhereExpression);
			
			
			sql.Where( cp => (  (cp.CreatedDate ?? DateTime.Today) == DateTime.Today));
			Console.WriteLine(sql.WhereExpression);
			
						
			sql.Where( cp => (  (cp.Turnover ?? 0 )>  15));
			Console.WriteLine(sql.WhereExpression);
			
			sql.Where( cp => (  Math.Abs(cp.Turnover ?? 0 )>  15));
			Console.WriteLine(sql.WhereExpression);
			
						
			sql.Where( cp => (  Sql.In( cp.Name, names ) ));
			Console.WriteLine(sql.WhereExpression);
						
			
			sql.Where( cp => (  Sql.In( cp.Id, ids ) ));
			Console.WriteLine(sql.WhereExpression);
			
			
			sql.OrderBy(cp=> cp.Name);
			Console.WriteLine("{0}",sql.OrderByExpression);
			
			sql.OrderBy(cp=> new{cp.Name, cp.Id} );
			Console.WriteLine("{0}", sql.OrderByExpression);
			
			sql.OrderBy(cp=> new{cp.Name, Id=cp.Id*-1} );
			Console.WriteLine("{0}", sql.OrderByExpression);
			
			sql.OrderByDescending(cp=> cp.Name);
			Console.WriteLine("{0}",sql.OrderByExpression);
			
			sql.OrderBy(cp=> new{cp.Name, X=cp.Id.Desc() } );
			Console.WriteLine("{0}", sql.OrderByExpression);
			
			
			sql.Limit(1,5);
			Console.WriteLine(sql.LimitExpression);
			
			sql.Limit(1);
			Console.WriteLine(sql.LimitExpression);
			
			
			sql.Where( cp => ( string.Concat( cp.Name, "_", cp.Employees) =="SOME COMPANY XYZ_2"));
			Console.WriteLine(sql.WhereExpression);
								
					
			sql.Where( cp => cp.Id != 1);
			Console.WriteLine(sql.WhereExpression);		
			
			
			sql.Select(cp=>new{cp.Employees, cp.Name});
			Console.WriteLine("To Select:'{0}' ",sql.SelectExpression);
			
			sql.Select(cp=>new{cp.Employees, cp.Name, Some=(cp.Id*4).As("SomeExpression")});
			Console.WriteLine("To Select:'{0}' ",sql.SelectExpression);
			
			sql.Select(cp=>new{cp.Employees, cp.Name, Some=cp.Turnover.Sum().As("SomeExpression")});
			Console.WriteLine("To Select:'{0}' ",sql.SelectExpression);
						
			sql.Select(cp=>new{cp.Employees, cp.Name, Some=DbMethods.Sum(cp.Turnover ?? 0).As("SomeExpression")});
			Console.WriteLine("To Select:'{0}' ",sql.SelectExpression);
			
			sql.Update(cp=>  new{cp.Employees,cp.Name} );
			Console.WriteLine("To Update:'{0}' ",  string.Join(",", sql.UpdateFields.ToArray() ) );
						
			sql.Insert(cp=>  new{cp.Id, cp.Employees,cp.Name} );
			Console.WriteLine("To Insert:'{0}' ", string.Join(",", sql.InsertFields.ToArray()) );
			
			
			Console.WriteLine ("This is The End my friend!");
			
			
		}
	}
}

