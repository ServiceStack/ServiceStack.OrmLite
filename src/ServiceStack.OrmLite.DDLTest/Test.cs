using System;
using NUnit.Framework;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Firebird;
using ServiceStack.OrmLite.MySql;
using ServiceStack.OrmLite.SqlServer;
using System.Collections.Generic;
using ServiceStack.Logging;
using ServiceStack.Logging.Support.Logging;
using System.ComponentModel.DataAnnotations;

namespace ServiceStack.OrmLite.DDLTest
{


	[TestFixture()]
	public class Test
	{
		List<Dialect> dialects = new List<Dialect>();

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			dialects.Add ( new Dialect{
				Provider= FirebirdOrmLiteDialectProvider.Instance, 
				AddColumnString="ALTER TABLE Model ADD Column1 VARCHAR(128) ;",
				AlterColumnString="ALTER TABLE Model ALTER Column2 VARCHAR(50) ;",
				ChangeColumnNameString="ALTER TABLE Model ALTER OldColumn3 TO Column3 ;",
				AddFKString="ALTER TABLE Child ADD CONSTRAINT JustOneFK FOREIGN KEY (IdModel) REFERENCES Model (Id) ON DELETE CASCADE ON UPDATE NO ACTION;",
				AddFKRestrictString="ALTER TABLE Child ADD CONSTRAINT JustOneMoreFK FOREIGN KEY (IdModel) REFERENCES Model (Id) ON UPDATE NO ACTION;",
				CreateIndexString="CREATE UNIQUE INDEX JustIndexOnColumn3 ON Model(Column3);"

			});
			dialects.Add ( new Dialect{
				Provider= MySqlDialectProvider.Instance,
				AddColumnString="ALTER TABLE `Model` ADD COLUMN `Column1` VARCHAR(255) NULL;",
				AlterColumnString="ALTER TABLE `Model` MODIFY COLUMN `Column2` VARCHAR(50) NULL;",
				ChangeColumnNameString="ALTER TABLE `Model` CHANGE COLUMN `OldColumn3` `Column3` VARCHAR(255) NULL;",
				AddFKString="ALTER TABLE `Child` ADD CONSTRAINT `JustOneFK` FOREIGN KEY (`IdModel`) REFERENCES `Model` (`Id`) ON DELETE CASCADE ON UPDATE NO ACTION;",
				AddFKRestrictString="ALTER TABLE `Child` ADD CONSTRAINT `JustOneMoreFK` FOREIGN KEY (`IdModel`) REFERENCES `Model` (`Id`) ON DELETE RESTRICT ON UPDATE NO ACTION;",
				CreateIndexString="CREATE UNIQUE INDEX `JustIndexOnColumn3` ON `Model`(`Column3`);"
			});

			dialects.Add ( new Dialect{
				Provider= SqlServerOrmLiteDialectProvider.Instance,
				AddColumnString = @"ALTER TABLE ""Model"" ADD ""Column1"" VARCHAR(8000) NULL;",
				AlterColumnString = @"ALTER TABLE ""Model"" ALTER COLUMN ""Column2"" VARCHAR(50) NULL;",
				ChangeColumnNameString = @"EXEC sp_rename 'Model.OldColumn3', 'Column3', 'COLUMN';",
				AddFKString = @"ALTER TABLE ""Child"" ADD CONSTRAINT ""JustOneFK"" FOREIGN KEY (""IdModel"") REFERENCES ""Model"" (""Id"") ON DELETE CASCADE ON UPDATE NO ACTION;",
				AddFKRestrictString = @"ALTER TABLE ""Child"" ADD CONSTRAINT ""JustOneMoreFK"" FOREIGN KEY (""IdModel"") REFERENCES ""Model"" (""Id"") ON UPDATE NO ACTION;",
				CreateIndexString = @"CREATE UNIQUE INDEX ""JustIndexOnColumn3"" ON ""Model""(""Column3"");"
			});

			LogManager.LogFactory = new ConsoleLogFactory();
		}

		[Test()]
		public void CanAddColumn ()
		{
			var model = typeof(Model);

			foreach (var d in dialects) 
			{
				OrmLiteConfig.DialectProvider=d.Provider;
				var fielDef = ModelDefinition<Model>.Definition.GetFieldDefinition<Model> (f => f.Column1);
				Assert.AreEqual(d.AddColumnString, (d.Provider.ToAddColumnStatement(model, fielDef)));
			}
		}

		[Test()]
		public void CanAAlterColumn ()
		{
			var model = typeof(Model);
			
			foreach (var d in dialects) 
			{
				OrmLiteConfig.DialectProvider=d.Provider;
				var fielDef = ModelDefinition<Model>.Definition.GetFieldDefinition<Model> (f => f.Column2);
				Assert.AreEqual(d.AlterColumnString, (d.Provider.ToAlterColumnStatement(model, fielDef)));
			}
			
		}


		[Test()]
		public void CanChangeColumnName ()
		{
			var model = typeof(Model);
			
			foreach (var d in dialects) 
			{
				OrmLiteConfig.DialectProvider=d.Provider;
				var fielDef = ModelDefinition<Model>.Definition.GetFieldDefinition<Model> (f => f.Column3);
				Assert.AreEqual(d.ChangeColumnNameString,(d.Provider.ToChangeColumnNameStatement(model, fielDef,"OldColumn3")));

			}
			
		}

		[Test()]
		public void CanAddForeignKey ()
		{
			
			foreach (var d in dialects) 
			{
				OrmLiteConfig.DialectProvider=d.Provider;		
				Assert.AreEqual(d.AddFKString,
				                d.Provider.ToAddForeignKeyStatement<Child,Model>(f=>f.IdModel,
				                                                 fk=>fk.Id,OnFkOption.NoAction,OnFkOption.Cascade, "JustOneFK"));
			}
		}

		[Test()]
		public void CanAddForeignKeyRestrict ()
		{
			
			foreach (var d in dialects) 
			{
				OrmLiteConfig.DialectProvider=d.Provider;		
				Assert.AreEqual(d.AddFKRestrictString,
				                d.Provider.ToAddForeignKeyStatement<Child,Model>(f=>f.IdModel,
				                                                 fk=>fk.Id,OnFkOption.NoAction,OnFkOption.Restrict, "JustOneMoreFK"));
			}
		}


		[Test()]
		public void CanCreateIndex ()
		{

			foreach (var d in dialects) 
			{
				OrmLiteConfig.DialectProvider=d.Provider;
				Assert.AreEqual(d.CreateIndexString, d.Provider.ToCreateIndexStatement<Model>(f=>f.Column3, "JustIndexOnColumn3", true) );
				
			}
			
		}


	}

	public class Model
	{
		public Model()
		{
		}
		public int Id { get; set; }
		public string Column1{ get; set; }
		[StringLength(50)]
		public string Column2{ get; set; }
		public string Column3{ get; set; }

	}

	public class Child
	{
		public Child(){}
		public int Id{ get; set;}
		public int IdModel{ get; set;}

	}

	public class Dialect
	{
		public IOrmLiteDialectProvider Provider { get; set; }
		public string AddColumnString { get; set; }
		public string AlterColumnString { get; set; }
		public string ChangeColumnNameString { get; set; }
		public string AddFKString { get; set; }
		public string AddFKRestrictString { get; set; }
		public string CreateIndexString { get; set; }

	}
}