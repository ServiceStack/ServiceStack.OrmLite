using System;
using System.Collections.Generic;
using System.Data;
using Northwind.Common.DataModel;
using Northwind.Perf;

namespace ServiceStack.OrmLite.TestsPerf.Scenarios.Northwind
{
	public class InsertNorthwindDataScenario
		: DatabaseScenarioBase
{
		static InsertNorthwindDataScenario()
		{
			NorthwindData.LoadData(false);
		}

        protected override void Run(IDbConnection db)
		{
			InsertData(db);
		}

        public void InsertData(IDbConnection db)
		{
			if (this.IsFirstRun)
			{
				db.CreateTables(true, NorthwindFactory.ModelTypes.ToArray());
			}
			else
			{
				NorthwindFactory.ModelTypes.ForEach(x => db.DeleteAll(x));
			}

			NorthwindData.Categories.ForEach(x => db.InsertAll(x));
			NorthwindData.Customers.ForEach(x => db.InsertAll(x));
			NorthwindData.Employees.ForEach(x => db.InsertAll(x));
			NorthwindData.Shippers.ForEach(x => db.InsertAll(x));
			NorthwindData.Orders.ForEach(x => db.InsertAll(x));
			NorthwindData.Products.ForEach(x => db.InsertAll(x));
			NorthwindData.OrderDetails.ForEach(x => db.InsertAll(x));
			NorthwindData.CustomerCustomerDemos.ForEach(x => db.InsertAll(x));
			NorthwindData.Regions.ForEach(x => db.InsertAll(x));
			NorthwindData.Territories.ForEach(x => db.InsertAll(x));
			NorthwindData.EmployeeTerritories.ForEach(x => db.InsertAll(x));
		}
	}
}