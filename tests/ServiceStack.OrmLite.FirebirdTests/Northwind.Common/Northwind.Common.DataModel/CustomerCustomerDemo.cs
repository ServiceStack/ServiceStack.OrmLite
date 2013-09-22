using ServiceStack.Model;
using ServiceStack.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System;

namespace Northwind.Common.DataModel{
	
	[Alias("CustomerDemo")]
	public class CustomerCustomerDemo : IHasStringId, IHasId<string>{
	
		[Alias("CustomerTypeID")]
		[StringLength(10)]
		public string CustomerTypeId
		{
			get;
			set;
		}
		
		[Alias("CustomerID")]
		[StringLength(5)]
		public string Id
		{
			get;
			set;
		}
		
		public CustomerCustomerDemo()
		{
		}
	}
}