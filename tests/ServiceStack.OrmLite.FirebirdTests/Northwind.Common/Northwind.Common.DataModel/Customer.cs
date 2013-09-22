using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using System.ComponentModel.DataAnnotations;
using System;

namespace Northwind.Common.DataModel{
	[Alias("Customers")]
	public class Customer : IHasStringId, IHasId<string>
	{
		[StringLength(60)]
		public string Address
		{
			get;
			set;
		}
	
		[Index]
		[StringLength(15)]
		public string City
		{
			get;
			set;
		}
	
		[Index]
		[Required]
		[StringLength(40)]
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
	
		[Alias("CustomerID")]
		[Required]
		[StringLength(5)]
		public string Id
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
	
		public byte[] Picture
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
		[Index]
		public string Region
		{
			get;
			set;
		}
	
		public Customer()
		{
		}
	}
}