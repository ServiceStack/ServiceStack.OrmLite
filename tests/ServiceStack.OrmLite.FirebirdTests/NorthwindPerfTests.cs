using System;
using System.Data;
using System.Diagnostics;
using Northwind.Common.DataModel;
using NUnit.Framework;
using ServiceStack.DataAccess;
using ServiceStack.OrmLite.Firebird;

namespace ServiceStack.OrmLite.FirebirdTests
{
	[Ignore("Perf test")]
	[TestFixture]
	public class NorthwindPerfTests
	{
		[Test]
		public void Load_Northwind_database()
		{
			OrmLiteConfig.DialectProvider = new FirebirdOrmLiteDialectProvider();

			NorthwindData.LoadData(false);
			GC.Collect();

			var stopWatch = new Stopwatch();
			stopWatch.Start();

			using (var dbConn = "User=SYSDBA;Password=masterkey;Database=/var/lib/firebird/2.1/data/NorthWind.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;".OpenDbConnection())
			using (var dbCmd = dbConn.CreateCommand())
			{
				using (var client = new OrmLitePersistenceProvider(dbConn))
				{
					OrmLiteNorthwindTests.CreateNorthwindTables(dbCmd);
					LoadNorthwindData(client);
				}
			}

			Console.WriteLine("stopWatch.ElapsedMilliseconds: " + stopWatch.ElapsedMilliseconds);
		}

		private static void LoadNorthwindData(IBasicPersistenceProvider persistenceProvider)
		{
			persistenceProvider.StoreAll(NorthwindData.Categories);
			persistenceProvider.StoreAll(NorthwindData.Customers);
			persistenceProvider.StoreAll(NorthwindData.Employees);
			persistenceProvider.StoreAll(NorthwindData.Shippers);
			persistenceProvider.StoreAll(NorthwindData.Orders);
			persistenceProvider.StoreAll(NorthwindData.Products);
			persistenceProvider.StoreAll(NorthwindData.OrderDetails);
			persistenceProvider.StoreAll(NorthwindData.CustomerCustomerDemos);
			persistenceProvider.StoreAll(NorthwindData.Regions);
			persistenceProvider.StoreAll(NorthwindData.Territories);
			persistenceProvider.StoreAll(NorthwindData.EmployeeTerritories);
		}
	}
}