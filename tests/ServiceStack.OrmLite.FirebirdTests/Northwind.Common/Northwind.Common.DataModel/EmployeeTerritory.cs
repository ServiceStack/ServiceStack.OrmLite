using System;
using ServiceStack.DataAnnotations;
using ServiceStack.DesignPatterns.Model;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Common.DataModel{
	
	[Alias("EmployeeTerritories")]
	public class EmployeeTerritory : IHasStringId, IHasId<string>
	{
		[Alias("EmployeeID")]
		public int EmployeeId
		{
			get;
			set;
		}
	
		public string Id
		{
			get
			{
				return string.Concat(this.EmployeeId, "/", this.TerritoryId);
			}
		}
	
		[StringLength(20)]
		[Required]
		[Alias("TerritoryID")]
		public string TerritoryId
		{
			get;
			set;
		}
	
		public EmployeeTerritory()
		{
		}
	}
	}