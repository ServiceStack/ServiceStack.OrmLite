using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.DesignPatterns.Model;
using ServiceStack.OrmLite.Firebird;

namespace ServiceStack.OrmLite.FirebirdTests
{
	[TestFixture]
	public class ShippersExample
	{
		static ShippersExample()
		{
			OrmLiteConfig.DialectProvider = FirebirdOrmLiteDialectProvider.Instance;
		}


		[Alias("ShippersT")]
		public class Shipper
			: IHasId<int>
		{
			[AutoIncrement]
			[Alias("Id")]
			public int Id { get; set; }

			[Required]
			[Index(Unique = true)]
			[StringLength(40)]
			public string CompanyName { get; set; }

			[StringLength(24)]
			public string Phone { get; set; }

			[References(typeof(ShipperType))]
			[Alias("Type")]
			public int ShipperTypeId { get; set; }
		}

		[Alias("ShipperTypesT")]
		public class ShipperType
			: IHasId<int>
		{
			[AutoIncrement]
			[Alias("Id")]
			public int Id { get; set; }

			[Required]
			[Index(Unique = true)]
			[StringLength(40)]
			public string Name { get; set; }
		}

		public class SubsetOfShipper
		{
			public int Id { get; set; }
			public string CompanyName { get; set; }
		}

		public class ShipperTypeCount
		{
			public int ShipperTypeId { get; set; }
			public int Total { get; set; }
		}


		[Test]
		public void Shippers_UseCase()
		{
			using (IDbConnection dbConn = "User=SYSDBA;Password=masterkey;Database=ormlite-tests.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;".OpenDbConnection())
			using (IDbCommand dbCmd = dbConn.CreateCommand())
			{
				const bool overwrite = false;
				dbCmd.DropTable<Shipper>();
				dbCmd.DropTable<ShipperType>();
				dbCmd.CreateTables(overwrite, typeof(ShipperType),  typeof(Shipper));// ShipperType must be created first!

				int trainsTypeId, planesTypeId;

				//Playing with transactions
				using (IDbTransaction dbTrans = dbCmd.BeginTransaction())
				{
					dbCmd.Insert(new ShipperType { Name = "Trains" });
					trainsTypeId = (int)dbCmd.GetLastInsertId();

					dbCmd.Insert(new ShipperType { Name = "Planes" });
					planesTypeId = (int)dbCmd.GetLastInsertId();

					dbTrans.Commit();
				}
				using (IDbTransaction dbTrans = dbCmd.BeginTransaction(IsolationLevel.ReadCommitted))
				{
					dbCmd.Insert(new ShipperType { Name = "Automobiles" });
					Assert.That(dbCmd.Select<ShipperType>(), Has.Count.EqualTo(3));

					dbTrans.Rollback();
				}
				Assert.That(dbCmd.Select<ShipperType>(), Has.Count.EqualTo(2));


				//Performing standard Insert's and Selects
				dbCmd.Insert(new Shipper { CompanyName = "Trains R Us", Phone = "555-TRAINS", ShipperTypeId = trainsTypeId });
				dbCmd.Insert(new Shipper { CompanyName = "Planes R Us", Phone = "555-PLANES", ShipperTypeId = planesTypeId });
				dbCmd.Insert(new Shipper { CompanyName = "We do everything!", Phone = "555-UNICORNS", ShipperTypeId = planesTypeId });

				var trainsAreUs = dbCmd.First<Shipper>("\"Type\" = {0}", trainsTypeId);
				Assert.That(trainsAreUs.CompanyName, Is.EqualTo("Trains R Us"));
				Assert.That(dbCmd.Select<Shipper>("CompanyName = {0} OR Phone = {1}", "Trains R Us", "555-UNICORNS"), Has.Count.EqualTo(2));
				Assert.That(dbCmd.Select<Shipper>("\"Type\" = {0}", planesTypeId), Has.Count.EqualTo(2));

				//Lets update a record
				trainsAreUs.Phone = "666-TRAINS";
				dbCmd.Update(trainsAreUs);
				Assert.That(dbCmd.GetById<Shipper>(trainsAreUs.Id).Phone, Is.EqualTo("666-TRAINS"));
				
				//Then make it dissappear
				dbCmd.Delete(trainsAreUs);
				Assert.That(dbCmd.GetByIdOrDefault<Shipper>(trainsAreUs.Id), Is.Null);

				//And bring it back again
				dbCmd.Insert(trainsAreUs);


				//Performing custom queries
				//Select only a subset from the table
				var partialColumns = dbCmd.Select<SubsetOfShipper>(typeof (Shipper), "\"Type\" = {0}", planesTypeId);
				Assert.That(partialColumns, Has.Count.EqualTo(2));

				//Select into another POCO class that matches sql
				var rows = dbCmd.Select<ShipperTypeCount>(
					"SELECT \"Type\" as ShipperTypeId, COUNT(*) AS Total FROM ShippersT GROUP BY \"Type\" ORDER BY COUNT(*)");

				Assert.That(rows, Has.Count.EqualTo(2));
				Assert.That(rows[0].ShipperTypeId, Is.EqualTo(trainsTypeId));
				Assert.That(rows[0].Total, Is.EqualTo(1));
				Assert.That(rows[1].ShipperTypeId, Is.EqualTo(planesTypeId));
				Assert.That(rows[1].Total, Is.EqualTo(2));


				//And finally lets quickly clean up the mess we've made:
				dbCmd.DeleteAll<Shipper>();
				dbCmd.DeleteAll<ShipperType>();

				Assert.That(dbCmd.Select<Shipper>(), Has.Count.EqualTo(0));
				Assert.That(dbCmd.Select<ShipperType>(), Has.Count.EqualTo(0));
			}
		}
		
	}

}