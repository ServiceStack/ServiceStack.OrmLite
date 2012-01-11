using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data;

using ServiceStack.DataAnnotations;
using System.Reflection;

using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Firebird;


namespace TestLiteFirebirdProcedures
{
	
	[Alias("EMPLOYEE")]
	public class Employee{
		
		[Alias("EMP_NO")]
		[Sequence("EMP_NO_GEN")]
		public Int16 Id{
			get; set;
		}
		
		[Alias("FIRST_NAME")]
		[Required]
		public string  FirstName{
			get; set;
		}
		
		[Alias("LAST_NAME")]
		[Required]
		public string  LastName{
			get; set;
		}
		
		[Alias("PHONE_EXT")]
		public string  PhoneExtension{
			get; set;
		}
		
		[Alias("HIRE_DATE")]
		[Required]
		public DateTime  HireDate{
			get; set;
		}
		
		[Alias("DEPT_NO")]
		[Required]
		public string  DepartamentNumber{
			get; set;
		}
		
		[Alias("JOB_CODE")]
		[Required]
		public string  JobCode{
			get; set;
		}
		
		[Alias("JOB_GRADE")]
		public Int16 JobGrade{
			get; set;
		}
		
		
		[Alias("JOB_COUNTRY")]
		[Required]
		public string  JobCountry{
			get; set;
		}
		
		[Alias("SALARY")]
		[Required]
		public Decimal Salary{
			get; set;
		}
		
	}
	
	[Alias("DELETE_EMPLOYEE")]	
	public class ProcedureDeleteEmployee
	{
		[Alias("EMP_NUM")]
		public Int16 EmployeeNumber{
			get ; set;
		}
		
	}
	
	[Alias("SUB_TOT_BUDGET")]
	public class ProcedureSubTotalBudgetParameters{
		
		[Alias("HEAD_DEPT")]
		public string HeadDepartament{
			get; set;			
		}
		
	}
	
	public class ProcedureSubTotalBudgetResult{
		
		[Alias("TOT_BUDGET")]
		public decimal Total {
			get; set;
		}	
		
		[Alias("AVG_BUDGET")]
		public decimal Average {
			get; set;
		}
		
		[Alias("MAX_BUDGET")]
		public decimal Max {
			get; set;
		}
		
		[Alias("MIN_BUDGET")]
		public decimal Min {
			get; set;
		}
	}
	
	
	[Alias("SHOW_LANGS")]
		public class ProcedureShowLangsParameters{

			[Alias("CODE")]
			public string Code{
				get; set;
			}
			
			[Alias("GRADE")]
			public Int16 Grade{
				get; set;
			}
		
			[Alias("CTY")]
			public string Country{
				get; set;
			}
			
		}
		
		
		public class ProcedureShowLangsResult{
			
			[Alias("LANGUAGES")]
			public string Language{
				get; set;
			}
		}
	
	[Alias("ALL_LANGS")]
	public class ProcedureAllLangs{
		
		public List<ProcedureAllLangsResult> Execute(IDbCommand dbConn){
			return dbConn.SelectFromProcedure<ProcedureAllLangsResult>(this);			
		}
		
		//public List<ProcedureAllLangsResult> Results{
		//	get; set;
		//}
		
		public class ProcedureAllLangsResult{
		
		[Alias("CODE")]
		public string Code{
			get; set;
		}
			
		[Alias("GRADE")]
		public string Grade{
			get; set;
		}
	
		[Alias("COUNTRY")]
		public string Country{
			get; set;
		}
		
		[Alias("LANG")]
		public string Language{
			get; set;
		}
		
	}
		
	}
	
	
	
	
	
	
	
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");
			
			OrmLiteConfig.DialectProvider = new FirebirdOrmLiteDialectProvider();
			using (IDbConnection db =
			       "User=SYSDBA;Password=masterkey;Database=employee.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;".OpenDbConnection())
			using ( IDbCommand dbConn = db.CreateCommand())
			{
				try{
					
					var employees = dbConn.Select<Employee>();
					Console.WriteLine("Total Employees '{0}'", employees.Count);
					
					Employee employee = new Employee(){
						FirstName="LILA",
						LastName= "FUTURAMA",
						PhoneExtension="0002",
						HireDate= DateTime.Now,
						DepartamentNumber= "900",
						JobCode="Eng",
						JobGrade=2,
						JobCountry="USA",
						Salary=75000
					};
					int count = employees.Count;
					
					dbConn.Insert(employee);
					Console.WriteLine ("Id for new employee : '{0}'", employee.Id);
					
					employees = dbConn.Select<Employee>();
					Console.WriteLine("Total Employees '{0}' = '{1}'", employees.Count, count+1);
					
					Console.WriteLine("Executing 'DELETE_EMPLOYEE' for  '{0}' - {1}", employee.Id, employee.LastName );
					ProcedureDeleteEmployee de = new ProcedureDeleteEmployee();
					de.EmployeeNumber= employee.Id;
					dbConn.ExecuteProcedure(de);
					
					employees = dbConn.Select<Employee>();
					Console.WriteLine("Total Employees '{0}'= '{1}' ", employees.Count, count);
					
				}	
				catch(Exception e){
					Console.WriteLine(e.Message);
				}
				
				
				try{
					
					ProcedureSubTotalBudgetParameters p = new ProcedureSubTotalBudgetParameters()
					{
						HeadDepartament="000"
					};
					
					var results = dbConn.SelectFromProcedure<ProcedureSubTotalBudgetResult>(p, "");
					
					
					foreach(var r in results){
						Console.WriteLine("r.Total:{0} r.Average:{1} r.Max:{2} r.Min:{3}", r.Total, r.Average, r.Max, r.Min);
					}
				}
				catch(Exception e){
					Console.WriteLine(e.Message);
				}
				
				try{
					
					ProcedureShowLangsParameters l = new ProcedureShowLangsParameters()
					{
						Code ="Sales",
						Grade =3,
						Country ="England"
					};
					
					var ls = dbConn.SelectFromProcedure<ProcedureShowLangsResult>(l,"");
					
					foreach(var lr in ls){
						Console.WriteLine(lr.Language);
					}
				}
				catch(Exception e){
					Console.WriteLine(e.Message);
				}
				
				try{
					
					ProcedureAllLangs l = new ProcedureAllLangs();
					
					//var ls = dbConn.SelectFromProcedure<ProcedureAllLangsResult>(l);
					//dbConn.SelectFromProcedure(l);
					
				
					var ls = l.Execute(dbConn);  // better ?
					
					foreach(var lr in ls){
						Console.WriteLine("lr.Code:{0} lr.Country:{1} lr.Grade:{2}  lr.Language:{3}",
						                  lr.Code, lr.Country, lr.Grade, lr.Language);
					}
				}
				catch(Exception e){
					Console.WriteLine(e.Message);
				}
				
				
				Console.WriteLine ("This is The End my friend!");
			}
			
		}
	}
}

