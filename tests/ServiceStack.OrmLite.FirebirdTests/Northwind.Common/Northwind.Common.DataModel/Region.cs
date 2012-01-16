using System;
using ServiceStack.DesignPatterns.Model;
using ServiceStack.DataAnnotations;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Common.DataModel{
	
	public class Region : IHasIntId, IHasId<int>
	{
		[Alias("RegionID")]
		public int Id
		{
			get;
			set;
		}
	
		[StringLength(50)]
		[Required]
		public string RegionDescription
		{
			get;
			set;
		}
	
		public Region()
		{
		}
	}
}