using System;
using ServiceStack.DesignPatterns.Model;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace Northwind.Common.DataModel{
	
	public class OrderBlob : IHasIntId, IHasId<int>{
		
		public Dictionary<int, string> CharMap
		{
			get;
			set;
		}
		
		public Customer Customer
		{
			get;
			set;
		}
		
		public Employee Employee
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
		public int Id
		{
			get;
			set;
		}
		
		public List<int> IntIds
		{
			get;
			set;
		}
		
		public DateTime? OrderDate
		{
			get;
			set;
		}
		
		public List<OrderDetailBlob> OrderDetails
		{
			get;
			set;
		}
		
		public DateTime? RequiredDate
		{
			get;
			set;
		}
		
		public string ShipAddress
		{
			get;
			set;
		}
		
		public string ShipCity
		{
			get;
			set;
		}
		
		public string ShipCountry
		{
			get;
			set;
		}
		
		public string ShipName
		{
			get;
			set;
		}
		
		public DateTime? ShippedDate
		{
			get;
			set;
		}
		
		public string ShipPostalCode
		{
			get;
			set;
		}
		
		public string ShipRegion
		{
			get;
			set;
		}
		
		public int? ShipVia
		{
			get;
			set;
		}
		
		public OrderBlob()
		{
			this.OrderDetails = new List<OrderDetailBlob>();
		}
		
		public static OrderBlob Create(int orderId)
		{
			
			OrderBlob orderBlob = new OrderBlob();
			orderBlob.Id = orderId;
			orderBlob.Customer = NorthwindFactory.Customer("ALFKI", "Alfreds Futterkiste", "Maria Anders", "Sales Representative", "Obere Str. 57", "Berlin", null, "12209", "Germany", "030-0074321", "030-0076545", null);
			orderBlob.Employee = NorthwindFactory.Employee(1, "Davolio", "Nancy", "Sales Representative", "Ms.", new DateTime?(NorthwindData.ToDateTime("12/08/1948")), new DateTime?(NorthwindData.ToDateTime("05/01/1992")), "507 - 20th Ave. E. Apt. 2A", "Seattle", "WA", "98122", "USA", "(206) 555-9857", "5467", null, "Education includes a BA in psychology from Colorado State University in 1970.  She also completed 'The Art of the Cold Call.'  Nancy is a member of Toastmasters International.", new int?(2), "http://accweb/emmployees/davolio.bmp");
			orderBlob.OrderDate = new DateTime?(NorthwindData.ToDateTime("7/4/1996"));
			orderBlob.RequiredDate = new DateTime?(NorthwindData.ToDateTime("8/1/1996"));
			orderBlob.ShippedDate = new DateTime?(NorthwindData.ToDateTime("7/16/1996"));
			orderBlob.ShipVia = new int?(5);
			orderBlob.Freight = new decimal(3238, 0, 0, false, 2);
			orderBlob.ShipName = "Vins et alcools Chevalier";
			orderBlob.ShipAddress = "59 rue de l'Abbaye";
			orderBlob.ShipCity = "Reims";
			orderBlob.ShipRegion = null;
			orderBlob.ShipPostalCode = "51100";
			orderBlob.ShipCountry = "France";
			
			orderBlob.OrderDetails = new List<OrderDetailBlob>
            {
                new OrderDetailBlob
                {
                    ProductId = 11, 
                    UnitPrice = 11m, 
                    Quantity = 14, 
                    Discount = 0.0
                }, 
                new OrderDetailBlob
                {
                    ProductId = 42, 
                    UnitPrice = 9.8m, 
                    Quantity = 10, 
                    Discount = 0.0
                }, 
                new OrderDetailBlob
                {
                    ProductId = 72, 
                    UnitPrice = 34.8m, 
                    Quantity = 5, 
                    Discount = 0.0
                }
            }; 
			
            orderBlob.IntIds = new List<int>
            {
                10, 20, 30
            };
			
            orderBlob.CharMap = new Dictionary<int, string>
            {
                
                {
                    1, "A"
                },              
                {
                    2, "B"
                }, 
                {
                    3, "C"
                }
            };
		
			return orderBlob;
		}
	}
}