using System;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.OrmLite.PostgreSQL;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{

	[TestFixture]
	public class OrmLiteCreateTableWithNamigStrategyTests 
		: OrmLiteTestBase
	{

		[Test]
		public void Can_create_TableWithNamigStrategy_table_prefix()
		{
			OrmLiteConfig.DialectProvider.NamingStrategy = new PrefixNamingStrategy
			{
				 TablePrefix ="tab_",
				 ColumnPrefix = "col_",
			};
			
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);
			}
			
			OrmLiteConfig.DialectProvider.NamingStrategy= new OrmLiteNamingStrategyBase();
		}

		[Test]
		public void Can_create_TableWithNamigStrategy_table_lowered()
		{
			OrmLiteConfig.DialectProvider.NamingStrategy = new LowercaseNamingStrategy();

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);
			}
			
			OrmLiteConfig.DialectProvider.NamingStrategy= new OrmLiteNamingStrategyBase();
		}


		[Test]
		public void Can_create_TableWithNamigStrategy_table_nameUnderscoreCoumpound()
		{
			OrmLiteConfig.DialectProvider.NamingStrategy = new UnderscoreSeparatedCompoundNamingStrategy();

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);
			}
			
			OrmLiteConfig.DialectProvider.NamingStrategy= new OrmLiteNamingStrategyBase();
		}

        [Test]
        public void Can_create_TableWithNamigStrategy_table_aliases()
        {
	        var aliasNamingStrategy = new AliasNamingStrategy
	        {
                TableAliases = { { "ModelWithOnlyStringFields", "TableAlias" } },
                ColumnAliases = { { "Name", "ColumnAlias" } },
            };
	        OrmLiteConfig.DialectProvider.NamingStrategy = aliasNamingStrategy;

            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithOnlyStringFields>(true);

                var sql = db.GetLastSql();
                Assert.That(sql, Is.StringContaining("CREATE TABLE \"TableAlias\""));
                Assert.That(sql, Is.StringContaining("\"ColumnAlias\""));

                var result = db.SqlList<ModelWithIdAndName>(
                    "SELECT * FROM {0} WHERE {1} = {2}"
                        .Fmt("ModelWithOnlyStringFields".SqlTable(),
                             "Name".SqlColumn(),
                             "foo".SqlValue()));

                Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM \"TableAlias\" WHERE \"ColumnAlias\" = 'foo'"));

                db.DropTable<ModelWithOnlyStringFields>();

                aliasNamingStrategy.UseNamingStrategy = new PostgreSqlNamingStrategy();

                db.CreateTable<ModelWithOnlyStringFields>(true);
                sql = db.GetLastSql();
                Assert.That(sql, Is.StringContaining("CREATE TABLE \"table_alias\""));
                Assert.That(sql, Is.StringContaining("\"column_alias\""));
            }

            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();
        }

		[Test]
		public void Can_get_data_from_TableWithNamigStrategy_with_GetById()
		{
			OrmLiteConfig.DialectProvider.NamingStrategy = OrmLiteConfig.DialectProvider.NamingStrategy = new PrefixNamingStrategy
			{
				TablePrefix = "tab_",
				ColumnPrefix = "col_",
			};

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);
				var m = new ModelWithOnlyStringFields { Id= "999", AlbumId = "112", AlbumName="ElectroShip", Name = "MyNameIsBatman"};

				db.Save(m);
                var modelFromDb = db.SingleById<ModelWithOnlyStringFields>("999");

				Assert.AreEqual(m.Name, modelFromDb.Name);
			}
			
			OrmLiteConfig.DialectProvider.NamingStrategy= new OrmLiteNamingStrategyBase();
		}


		[Test]
		public void Can_get_data_from_TableWithNamigStrategy_with_query_by_example()
		{
			OrmLiteConfig.DialectProvider.NamingStrategy = OrmLiteConfig.DialectProvider.NamingStrategy = new PrefixNamingStrategy
			{
				TablePrefix = "tab_",
				ColumnPrefix = "col_",
			};

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);
				var m = new ModelWithOnlyStringFields { Id = "998", AlbumId = "112", AlbumName = "ElectroShip", Name = "QueryByExample" };

				db.Save(m);
				var modelFromDb = db.Where<ModelWithOnlyStringFields>(new { Name = "QueryByExample" })[0];

				Assert.AreEqual(m.Name, modelFromDb.Name);
			}
			
			OrmLiteConfig.DialectProvider.NamingStrategy= new OrmLiteNamingStrategyBase();
		}
		
		[Test]
		public void Can_get_data_from_TableWithUnderscoreSeparatedCompoundNamingStrategy_with_ReadConnectionExtension()
		{
			OrmLiteConfig.DialectProvider.NamingStrategy = new UnderscoreSeparatedCompoundNamingStrategy();

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);
				var m = new ModelWithOnlyStringFields
							{
								Id = "997",
								AlbumId = "112",
								AlbumName = "ElectroShip",
								Name = "ReadConnectionExtensionFirst"
							};
				db.Save(m);
                var modelFromDb = db.Single<ModelWithOnlyStringFields>(x => x.Name == "ReadConnectionExtensionFirst");
				Assert.AreEqual(m.AlbumName, modelFromDb.AlbumName);
			}

            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();
        }
		
		[Test]
		public void Can_get_data_from_TableWithNamigStrategy_AfterChangingNamingStrategy()
		{			
			OrmLiteConfig.DialectProvider.NamingStrategy = OrmLiteConfig.DialectProvider.NamingStrategy = new PrefixNamingStrategy
			{
				TablePrefix = "tab_",
				ColumnPrefix = "col_",
			};

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);
				var m = new ModelWithOnlyStringFields() { Id = "998", AlbumId = "112", AlbumName = "ElectroShip", Name = "QueryByExample" };

				db.Save(m);
				var modelFromDb = db.Where<ModelWithOnlyStringFields>(new { Name = "QueryByExample" })[0];

				Assert.AreEqual(m.Name, modelFromDb.Name);

                modelFromDb = db.SingleById<ModelWithOnlyStringFields>("998");
				Assert.AreEqual(m.Name, modelFromDb.Name);
				
			}
			
			OrmLiteConfig.DialectProvider.NamingStrategy= new OrmLiteNamingStrategyBase();
			
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);
				var m = new ModelWithOnlyStringFields() { Id = "998", AlbumId = "112", AlbumName = "ElectroShip", Name = "QueryByExample" };

				db.Save(m);
				var modelFromDb = db.Where<ModelWithOnlyStringFields>(new { Name = "QueryByExample" })[0];

				Assert.AreEqual(m.Name, modelFromDb.Name);

                modelFromDb = db.SingleById<ModelWithOnlyStringFields>("998");
				Assert.AreEqual(m.Name, modelFromDb.Name);	
			}
			
			OrmLiteConfig.DialectProvider.NamingStrategy = OrmLiteConfig.DialectProvider.NamingStrategy = new PrefixNamingStrategy
			{
				TablePrefix = "tab_",
				ColumnPrefix = "col_",
			};

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);
				var m = new ModelWithOnlyStringFields() { Id = "998", AlbumId = "112", AlbumName = "ElectroShip", Name = "QueryByExample" };

				db.Save(m);
				var modelFromDb = db.Where<ModelWithOnlyStringFields>(new { Name = "QueryByExample" })[0];

				Assert.AreEqual(m.Name, modelFromDb.Name);

                modelFromDb = db.SingleById<ModelWithOnlyStringFields>("998");
				Assert.AreEqual(m.Name, modelFromDb.Name);
			}
			
			OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();
		}

	}

	public class PrefixNamingStrategy : OrmLiteNamingStrategyBase
	{

		public string TablePrefix { get; set; }

		public string ColumnPrefix { get; set; }

		public override string GetTableName(string name)
		{
			return TablePrefix + name;
		}

		public override string GetColumnName(string name)
		{
			return ColumnPrefix + name;
		}

	}

	public class LowercaseNamingStrategy : OrmLiteNamingStrategyBase
	{

		public override string GetTableName(string name)
		{
			return name.ToLower();
		}

		public override string GetColumnName(string name)
		{
			return name.ToLower();
		}

	}

	public class UnderscoreSeparatedCompoundNamingStrategy : OrmLiteNamingStrategyBase
	{

		public override string GetTableName(string name)
		{
			return toUnderscoreSeparatedCompound(name);
		}

		public override string GetColumnName(string name)
		{
			return toUnderscoreSeparatedCompound(name);
		}


		string toUnderscoreSeparatedCompound(string name)
		{

			string r = char.ToLower(name[0]).ToString();

			for (int i = 1; i < name.Length; i++)
			{
				char c = name[i];
				if (char.IsUpper(name[i]))
				{
					r += "_";
					r += char.ToLower(name[i]);
				}
				else
				{
					r += name[i];
				}
			}
			return r;
		}

	}

}