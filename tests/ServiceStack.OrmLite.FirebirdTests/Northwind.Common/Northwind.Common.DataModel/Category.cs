using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using System.ComponentModel.DataAnnotations;
using System;

namespace Northwind.Common.DataModel{
	[Alias("Categories")]
	public class Category : IHasIntId, IHasId<int>
	{
		[StringLength(15)]
		[Required]
		[Index]
		public string CategoryName
		{
			get;
			set;
		}
	
		[StringLength(10)]
		public string Description
		{
			get;
			set;
		}
	
		[Alias("CategoryID")]
		public int Id
		{
			get;
			set;
		}
	
		public byte[] Picture
		{
			get;
			set;
		}
	
		public Category()
		{
		}
	}
}