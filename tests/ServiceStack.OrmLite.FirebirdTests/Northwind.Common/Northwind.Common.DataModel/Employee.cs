using System;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Common.DataModel{
	
	[Alias("Employees")]
	public class Employee : IHasIntId, IHasId<int>
	{
		[StringLength(60)]
		public string Address
		{
			get;
			set;
		}
	
		public DateTime? BirthDate
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
	
		[StringLength(15)]
		public string Country
		{
			get;
			set;
		}
	
		[StringLength(4)]
		public string Extension
		{
			get;
			set;
		}
	
		[Required]
		[StringLength(10)]
		public string FirstName
		{
			get;
			set;
		}
	
		public DateTime? HireDate
		{
			get;
			set;
		}
	
		[StringLength(24)]
		public string HomePhone
		{
			get;
			set;
		}
	
		[AutoIncrement]
		[Alias("EmployeeID")]
		public int Id
		{
			get;
			set;
		}
	
		[Required]
		[StringLength(20)]
		[Index]
		public string LastName
		{
			get;
			set;
		}
	
		public string Notes
		{
			get;
			set;
		}
	
		public byte[] Photo
		{
			get;
			set;
		}
	
		[StringLength(255)]
		public string PhotoPath
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
	
		[References(typeof(Employee))]
		public int? ReportsTo
		{
			get;
			set;
		}
	
		[StringLength(30)]
		public string Title
		{
			get;
			set;
		}
	
		[StringLength(25)]
		public string TitleOfCourtesy
		{
			get;
			set;
		}
	
		public Employee()
		{
		}
	}
}