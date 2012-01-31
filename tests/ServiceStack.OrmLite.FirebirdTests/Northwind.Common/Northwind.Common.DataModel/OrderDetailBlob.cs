using System;

namespace Northwind.Common.DataModel{
	
	public class OrderDetailBlob
	{
		public double Discount
		{
			get;
			set;
		}
	
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
	
		public OrderDetailBlob()
		{
		}
	}
}