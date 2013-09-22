using System;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using System.ComponentModel.DataAnnotations;

namespace Northwind.Common.DataModel{
	
	[Alias("OrderDetails")]
	public class OrderDetail : IHasStringId, IHasId<string>
	{
		public double Discount
		{
			get;
			set;
		}
	
		public string Id
		{
			get
			{
				return string.Concat(this.OrderId, "/", this.ProductId);
			}
		}
	
		[Index]
		[References(typeof(Order))]
		[Alias("OrderID")]
		public int OrderId
		{
			get;
			set;
		}
	
		[References(typeof(Product))]
		[Alias("ProductID")]
		[Index]
		public int ProductId
		{
			get;
			set;
		}
	
		public short Quantity
		{
			get;
			set;
		}
	
		public decimal UnitPrice
		{
			get;
			set;
		}
	
		public OrderDetail()
		{
		}
	}
}