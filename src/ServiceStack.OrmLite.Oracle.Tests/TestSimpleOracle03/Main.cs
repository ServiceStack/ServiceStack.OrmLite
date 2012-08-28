
using System;
using System.Data;
using System.Collections.Generic;

using ServiceStack.Common.Utils;
using ServiceStack.DataAnnotations;
using ServiceStack.Common.Extensions;

using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Firebird;

namespace TestLiteFirebir03
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			OrmLiteConfig.DialectProvider = new FirebirdOrmLiteDialectProvider();
			
			
			using (IDbConnection db =
			       "User=SYSDBA;Password=masterkey;Database=employee.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;".OpenDbConnection())
			{
			
				Schema fbd= new Schema(){
					Connection = db
				};
		
				Console.WriteLine("--------TABLES-------------");
				
				var tables = fbd.Tables;
				
				foreach(Table t in tables){
					Console.WriteLine(t.Name);
				}
								
				Console.WriteLine("-------users's owner--------------");
				
				
				Table t1 = fbd.GetTable("USERS");
				Console.WriteLine(t1.Owner);
				
				Console.WriteLine ("---------user's columns ----------------");
				
				var Columns = fbd.GetColumns("USERS");
					
				foreach(Column cl in Columns){
					Console.WriteLine("{0}--{1}--{2}--{3} -- {4}-- {5}--{6} ",
					                  cl.Name,cl.Position, cl.Nullable, cl.Length, cl.DbType, cl.NetType, cl.Sequence);	
				}
				
				Console.WriteLine("--------------------------------------------");
				
				Console.WriteLine("EMPLOYEE's Columns ");
				
				Columns = fbd.GetColumns("EMPLOYEE");
				
				foreach(Column cl in Columns){
					Console.WriteLine("{0}--{1}--{2}--{3} -- {4}-- {5}--{6} -- Computed {7} ",
					                  cl.Name,cl.Position, cl.Nullable, cl.Length, cl.DbType,
					                  cl.NetType, cl.Sequence,cl.IsComputed);	
				}
				
				Columns = fbd.GetColumns("COMPANY");
				
				foreach(Column cl in Columns){
					Console.WriteLine("{0}--{1}--{2}--{3} -- {4}-- {5}--{6} ",
					                  cl.Name,cl.Position, cl.Nullable, cl.Length, cl.DbType, cl.NetType, cl.Sequence);	
				}
				
				Console.WriteLine("--------------------------------------------");
				
				Console.WriteLine("-----------------Procedure---------------------------");
				
				Console.WriteLine ("----- ADD_EMP_PROJ ----");
				
				Procedure p = fbd.GetProcedure("ADD_EMP_PROJ");
				Console.WriteLine("p.Name {0} p.Owner {1} p.Inputs {2} p.Outputs {3} p.Type {4}",
				                  p.Name, p.Owner, p.Inputs, p.Outputs, p.Type);
				
				var parameters = fbd.GetParameters(p);
				foreach( var par in parameters){
					Console.WriteLine("p.ProcedureName {0} p.Name {1} p.Position {2} p.ParameterType {3} p.DbType {4} p.NetType {5}",
					                  par.ProcedureName, par.Name, par.Position, par.Direction, par.DbType, par.NetType);
				}
				
				
				Console.WriteLine ("----- ALL_LANGS ----");
				p = fbd.GetProcedure("ALL_LANGS");
				Console.WriteLine("p.Name {0} p.Owner {1} p.Inputs {2} p.Outputs {3} p.Type {4}",
				                  p.Name, p.Owner, p.Inputs, p.Outputs, p.Type);
				
				
				parameters = fbd.GetParameters(p);
				
				parameters = fbd.GetParameters(p);
				foreach( var par in parameters){
					Console.WriteLine("p.ProcedureName {0} p.Name {1} p.Position {2} p.ParameterType {3} p.DbType {4} p.NetType {5}",
					                  par.ProcedureName, par.Name, par.Position, par.Direction, par.DbType, par.NetType);
				}
				
				
				ClassWriter cw = new ClassWriter(){
					Schema=fbd,
				};
				
				cw.WriteClass( new Table(){Name="EMPLOYEE"} );
				
				
				Console.WriteLine("This is The End my friend");
			
			//DTOGenerator
				
			}
			
			
			
			
		}
	}
}

