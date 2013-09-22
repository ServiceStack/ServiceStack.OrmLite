using System;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Common.DataModel{
	
	[Alias("Territories")]
	public class Territory : IHasStringId, IHasId<string>
	{
		[StringLength(20)]
		[Alias("TerritoryID")]
		public string Id
		{
			get;
			set;
		}
	
		[References(typeof(Region))]
		[Alias("RegionID")]
		public int RegionId
		{
			get;
			set;
		}
	
		[StringLength(50)]
		[Required]
		public string TerritoryDescription
		{
			get;
			set;
		}
	
		public Territory()
		{
		}
	}
}