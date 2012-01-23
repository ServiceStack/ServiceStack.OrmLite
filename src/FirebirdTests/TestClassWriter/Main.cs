using System;
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
			

			}


			Console.WriteLine ("This is The End my friend!");
			
		}
	}
}
