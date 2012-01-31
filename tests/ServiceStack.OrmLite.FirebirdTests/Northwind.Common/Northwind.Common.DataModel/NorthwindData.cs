using System;
using System.Collections.Generic;
using Northwind.Common;

namespace Northwind.Common.DataModel{
	
	public static class NorthwindData
	{
		
		public static List<Category> Categories
		{
			get;set;
		}
	
		public static List<CustomerCustomerDemo> CustomerCustomerDemos
		{
			get;set;
		}
	
		public static List<Customer> Customers
		{
			get;set;
		}
	
		public static List<Employee> Employees
		{
			get;set;
		}
	
		public static List<EmployeeTerritory> EmployeeTerritories
		{
			get;set;
		}
	
		public static List<OrderDetail> OrderDetails
		{
			get;set;
		}
	
		public static List<Order> Orders
		{
			get;set;
		}
	
		public static List<Product> Products
		{
			get;set;
		}
	
		public static List<Region> Regions
		{
			get;set;
		}
	
		public static List<Shipper> Shippers
		{
			get;set;
		}
	
		public static List<Supplier> Suppliers
		{
			get;set;
		}
	
		public static List<Territory> Territories
		{
			get;set;
		}
	
		
		public static DateTime ToDateTime(string dateTime)
		{			
			string[] strArrays = dateTime.Split(new char[] { '/' });
			return new DateTime(int.Parse(strArrays[2]), int.Parse(strArrays[0]), int.Parse(strArrays[1]));
		}
	}
}