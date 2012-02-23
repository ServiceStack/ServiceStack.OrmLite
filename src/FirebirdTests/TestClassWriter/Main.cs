using System;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Collections.Generic;
using System.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Firebird;
using ServiceStack.OrmLite.Firebird.DbSchema;

namespace TestClassWriter
{
	class MainClass
	{
		public static void Main (string[] args)
		{
		
			OrmLiteConfig.DialectProvider = new FirebirdOrmLiteDialectProvider();
			using (IDbConnection db =
				       "User=SYSDBA;Password=masterkey;Database=employee.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;".OpenDbConnection())
			using ( IDbCommand dbConn = db.CreateCommand())
			{
				Schema fbd= new Schema(){
					Connection = db
				};

				ClassWriter cw = new ClassWriter(){
					Schema=fbd,
					GenerateMetadata=true,
					//SpaceName= "your.app.namespace",
					//OutputDirectory="outputpath"
					//Usings="Using System;\nUsing System.Data\n"
				};

				foreach(var t in fbd.Tables){
					Console.Write("Generating POCO Class for table:'{0}'...", t.Name);
					cw.WriteClass( t);	
					Console.WriteLine(" Done.");
				}
				Console.WriteLine("---------------------------------");
				Console.WriteLine("See classes in: '{0}'", cw.OutputDirectory);
				
				//compilar ...
				CompilerParameters cp = new CompilerParameters();
				cp.GenerateExecutable=false;
				cp.GenerateInMemory=false;
				cp.ReferencedAssemblies.AddRange(
					new string[]{
						"System.dll",
						"System.ComponentModel.DataAnnotations.dll",
						Path.Combine( Directory.GetCurrentDirectory(), "ServiceStack.OrmLite.dll"),
						Path.Combine( Directory.GetCurrentDirectory(), "ServiceStack.Common.dll"),
						Path.Combine( Directory.GetCurrentDirectory(),"ServiceStack.Interfaces.dll")
				});
				cp.OutputAssembly= Path.Combine(cw.OutputDirectory, cw.SpaceName+".dll");
				
				var providerOptions = new Dictionary<string,string>();
    			providerOptions.Add("CompilerVersion", "v3.5");
				
				CodeDomProvider cdp =new CSharpCodeProvider(providerOptions);
			
				string [] files = Directory.GetFiles(cw.OutputDirectory,"*.cs");
				CompilerResults cr= cdp.CompileAssemblyFromFile(cp, files);
				
				if( cr.Errors.Count==0){
					Console.WriteLine("Generated file {0}", Path.Combine(cw.OutputDirectory, cw.SpaceName+".dll")); 
				}
            	else{							
            		foreach (CompilerError ce in cr.Errors)
                		Console.WriteLine(ce.ErrorText);
				}
				
						
			

			}


			Console.WriteLine ("This is The End my friend!");
			
		}
	}
}
