using System;
using Northwind.Common.DataModel;
using NUnit.Framework;
using ServiceStack.OrmLite.Firebird;

namespace ServiceStack.OrmLite.FirebirdTests
{
	[TestFixture][Ignore]
	public class OrmLiteConnectionFactoryTests
	{
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			OrmLiteConfig.DialectProvider = FirebirdOrmLiteDialectProvider.Instance; //use Pooling=false ?
		}

		[Test]
		public void AutoDispose_ConnectionFactory_disposes_connection()
		{
			var factory = new OrmLiteConnectionFactory("User=SYSDBA;Password=masterkey;Database=ormlite-tests.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;", true);

			using (var dbConn = factory.OpenDbConnection())
			using (var dbCmd = dbConn.CreateCommand())
			{
				dbCmd.CreateTable<Shipper>(true);
				dbCmd.Insert(new Shipper { CompanyName = "I am shipper 1" });
			}

			using (var dbConn = factory.OpenDbConnection())
			using (var dbCmd = dbConn.CreateCommand())
			{
				dbCmd.CreateTable<Shipper>(false);
				Assert.That(dbCmd.Select<Shipper>(), Has.Count.EqualTo(1));
				
				dbCmd.DeleteAll<Shipper>();
				Assert.That(dbCmd.Select<Shipper>(), Has.Count.EqualTo(0));
			}
		}

		[Test]
		public void NonAutoDispose_ConnectionFactory_reuses_connection()
		{
			var factory = new OrmLiteConnectionFactory("User=SYSDBA;Password=masterkey;Database=ormlite-tests.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;", false);

			using (var dbConn = factory.OpenDbConnection())
			using (var dbCmd = dbConn.CreateCommand())
			{
				dbCmd.CreateTable<Shipper>(false);
				dbCmd.Insert(new Shipper { CompanyName = "I am shipper 2" });
			}

			using (var dbConn = factory.OpenDbConnection())
			using (var dbCmd = dbConn.CreateCommand())
			{
				dbCmd.CreateTable<Shipper>(false);
				Assert.That(dbCmd.Select<Shipper>(), Has.Count.EqualTo(1));
				
				dbCmd.DeleteAll<Shipper>();
				Assert.That(dbCmd.Select<Shipper>(), Has.Count.EqualTo(0));
			}
		}
		
		[Test]
		public void NonAutoDispose_ConnectionFactory_delete_and_drop()
		{
			var factory = new OrmLiteConnectionFactory("User=SYSDBA;Password=masterkey;Database=ormlite-tests.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;", false);

			using (var dbConn = factory.OpenDbConnection())
			using (var dbCmd = dbConn.CreateCommand())
			{
				dbCmd.DeleteAll<Shipper>();
				Assert.That(dbCmd.Select<Shipper>(), Has.Count.EqualTo(0));
			}

			using (var dbConn = factory.OpenDbConnection())
			using (var dbCmd = dbConn.CreateCommand())
			{
				dbCmd.DropTable<Shipper>();
				Schema schema = new Schema(){
					Connection= dbConn,
				};
								
				Assert.That(schema.GetTable("Shippers".ToUpper() ) ==null );
			}
		}
		
		
	}
}
