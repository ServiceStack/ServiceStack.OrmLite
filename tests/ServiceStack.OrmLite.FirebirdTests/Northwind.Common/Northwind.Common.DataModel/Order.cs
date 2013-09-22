using System;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Common.DataModel{

	[Alias("Orders")]
	public class Order : IHasIntId, IHasId<int>
	{
		[Alias("CustomerID")]
		[Index]
		[StringLength(5)]
		[References(typeof(Customer))]
		public string CustomerId
		{
			get;
			set;
		}
	
		[References(typeof(Customer))]
		[Alias("EmployeeID")]
		[Index]
		public int EmployeeId
		{
			get;
			set;
		}
	
		public decimal Freight
		{
			get;
			set;
		}
	
		[AutoIncrement]
		[Alias("OrderID")]
		public int Id
		{
			get;
			set;
		}
	
		[Index]
		public DateTime? OrderDate
		{
			get;
			set;
		}
	
		public DateTime? RequiredDate
		{
			get;
			set;
		}
	
		[StringLength(60)]
		public string ShipAddress
		{
			get;
			set;
		}
	
		[StringLength(15)]
		public string ShipCity
		{
			get;
			set;
		}
	
		[StringLength(15)]
		public string ShipCountry
		{
			get;
			set;
		}
	
		[StringLength(40)]
		public string ShipName
		{
			get;
			set;
		}
	
		[Index]
		public DateTime? ShippedDate
		{
			get;
			set;
		}
	
		[StringLength(10)]
		[Index]
		public string ShipPostalCode
		{
			get;
			set;
		}
	
		[StringLength(15)]
		public string ShipRegion
		{
			get;
			set;
		}
	
		[Index]
		[References(typeof(Shipper))]
		public int? ShipVia
		{
			get;
			set;
		}
	
		public Order()
		{
		}
	}
}