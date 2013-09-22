using System;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Common.DataModel{
	
	[Alias("Suppliers")]
	public class Supplier : IHasIntId, IHasId<int>
	{
		[StringLength(60)]
		public string Address
		{
			get;
			set;
		}
		
		[StringLength(15)]
		public string City
		{
			get;
			set;
		}
		
		[StringLength(40)]
		[Required]
		[Index]
		public string CompanyName
		{
			get;
			set;
		}
		
		[StringLength(30)]
		public string ContactName
		{
			get;
			set;
		}
		
		[StringLength(30)]
		public string ContactTitle
		{
			get;
			set;
		}
		
		[StringLength(15)]
		public string Country
		{
			get;
			set;
		}
		
		[StringLength(24)]
		public string Fax
		{
			get;
			set;
		}
		
		public string HomePage
		{
			get;
			set;
		}
		
		[AutoIncrement]
		[Alias("SupplierID")]
		public int Id
		{
			get;
			set;
		}
		
		[StringLength(24)]
		public string Phone
		{
			get;
			set;
		}
		
		[StringLength(10)]
		[Index]
		public string PostalCode
		{
			get;
			set;
		}
		
		[StringLength(15)]
		public string Region
		{
			get;
			set;
		}
		
		public Supplier()
		{
		}
	}
}