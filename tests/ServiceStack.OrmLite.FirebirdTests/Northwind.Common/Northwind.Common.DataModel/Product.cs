using ServiceStack.DataAnnotations;
using ServiceStack.Model;

namespace Northwind.Common.DataModel
{
	[Alias("Products")]
	public class Product : IHasIntId, IHasId<int>
	{
		[References(typeof(Category))]
		[Alias("CategoryID")]
		[Index]
		public int CategoryId
		{
			get;
			set;
		}
	
		public bool Discontinued
		{
			get;
			set;
		}
	
		[AutoIncrement]
		[Alias("ProductID")]
		public int Id
		{
			get;
			set;
		}
	
		[Index]
		[Required]
		[StringLength(40)]
		public string ProductName
		{
			get;
			set;
		}
	
		[StringLength(20)]
		public string QuantityPerUnit
		{
			get;
			set;
		}
	
		public short ReorderLevel
		{
			get;
			set;
		}
	
		[Alias("SupplierID")]
		[References(typeof(Supplier))]
		[Index]
		public int SupplierId
		{
			get;
			set;
		}

		public decimal UnitPrice
		{
			get;
			set;
		}
	
		public short UnitsInStock
		{
			get;
			set;
		}
	
		public short UnitsOnOrder
		{
			get;
			set;
		}
	
		public Product()
		{
		}
	}
}