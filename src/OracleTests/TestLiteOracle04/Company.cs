using System;
using System.ComponentModel.DataAnnotations;
using ServiceStack.Common;
using ServiceStack.DataAnnotations;
using ServiceStack.DesignPatterns.Model;
using ServiceStack.OrmLite;

namespace Database.Records
{
	[Alias("COMPANY")]
	public partial class Company:IHasId<System.Int32>{

		public Company(){}

		[Alias("ID")]
		[Sequence("COMPANY_ID_GEN")]
		[PrimaryKey]
		[AutoIncrement]
		public System.Int32 Id { get; set;} 

		[Alias("NAME")]
		public System.String Name { get; set;} 

		[Alias("TURNOVER")]
		public System.Single? Turnover { get; set;} 

		[Alias("STARTED")]
		public System.DateTime? Started { get; set;} 

		[Alias("EMPLOYEES")]
		public System.Int32? Employees { get; set;} 

		[Alias("CREATED_DATE")]
		public System.DateTime? CreatedDate { get; set;} 

		[Alias("GUID")]
		public System.Guid? Guid { get; set;} 

		[Alias("SOMEDOUBLE")]
		public Double SomeDouble { get; set;} 
		
		[Alias("SOMEBOOL")]
		public bool SomeBoolean { get; set;} 
		

		public static class Me {
			
			public static string TableName { get { return "COMPANY"; }}
			public static string Id { get { return "ID"; }}
			public static string Name { get { return "NAME"; }}
			public static string Turnover { get { return "TURNOVER"; }}
			public static string Started { get { return "STARTED"; }}
			public static string Employees { get { return "EMPLOYEES"; }}
			public static string CreatedDate { get { return "CREATED_DATE"; }}
			public static string Guid { get { return "GUID"; }}

		}
	}
}