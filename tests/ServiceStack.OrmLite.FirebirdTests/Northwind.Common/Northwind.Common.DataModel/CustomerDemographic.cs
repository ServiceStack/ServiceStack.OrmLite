using System;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Common.DataModel{

	[Alias("Demographics")]
	public class CustomerDemographic : IHasStringId, IHasId<string>
	{
		public string CustomerDesc
		{
			get;
			set;
		}
	
		[StringLength(10)]
		[Alias("TypeID")]
		public string Id
		{
			get;
			set;
		}
	
		public CustomerDemographic()
		{
		}
	}
}