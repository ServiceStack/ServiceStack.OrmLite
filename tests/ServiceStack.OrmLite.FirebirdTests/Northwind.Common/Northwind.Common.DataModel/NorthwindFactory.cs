using System;
using System.Collections.Generic;

namespace Northwind.Common.DataModel{
	
	public static class NorthwindFactory
	{
		
		public readonly static List<Type> ModelTypes;
	
		static NorthwindFactory()
		{
			List<Type> types = new List<Type>();
			types.Add(typeof(Employee));
			types.Add(typeof(Category));
			types.Add(typeof(Customer));
			types.Add(typeof(Shipper));
			types.Add(typeof(Supplier));
			types.Add(typeof(Order));
			types.Add(typeof(Product));
			types.Add(typeof(OrderDetail));
			types.Add(typeof(CustomerCustomerDemo));
			types.Add(typeof(Category));
			types.Add(typeof(CustomerDemographic));
			types.Add(typeof(Region));
			types.Add(typeof(Territory));
			types.Add(typeof(EmployeeTerritory));
			NorthwindFactory.ModelTypes = types;
		}
	
		public static Category Category(int id, string categoryName, string description, byte[] picture)
		{
			Category category = new Category();
			category.Id = id;
			category.CategoryName = categoryName;
			category.Description = description;
			category.Picture = picture;
			return category;
		}
	
		public static Customer Customer(string customerId, string companyName, string contactName, string contactTitle, string address, string city, string region, string postalCode, string country, string phoneNo, string faxNo, byte[] picture)
		{
			Customer customer = new Customer();
			customer.Id = customerId;
			customer.CompanyName = companyName;
			customer.ContactName = contactName;
			customer.ContactTitle = contactTitle;
			customer.Address = address;
			customer.City = city;
			customer.Region = region;
			customer.PostalCode = postalCode;
			customer.Country = country;
			customer.Phone = phoneNo;
			customer.Fax = faxNo;
			customer.Picture = picture;
			return customer;
		}
	
		public static CustomerCustomerDemo CustomerCustomerDemo(string customerId, string customerTypeId)
		{
			CustomerCustomerDemo customerCustomerDemo = new CustomerCustomerDemo();
			customerCustomerDemo.Id = customerId;
			customerCustomerDemo.CustomerTypeId = customerTypeId;
			return customerCustomerDemo;
		}
	
		public static Employee Employee(int employeeId, string lastName, string firstName, string title, string titleOfCourtesy, DateTime? birthDate, DateTime? hireDate, string address, string city, string region, string postalCode, string country, string homePhone, string extension, byte[] photo, string notes, int? reportsTo, string photoPath)
		{
			Employee employee = new Employee();
			employee.Id = employeeId;
			employee.LastName = lastName;
			employee.FirstName = firstName;
			employee.Title = title;
			employee.TitleOfCourtesy = titleOfCourtesy;
			employee.BirthDate = birthDate;
			employee.HireDate = hireDate;
			employee.Address = address;
			employee.City = city;
			employee.Region = region;
			employee.PostalCode = postalCode;
			employee.Country = country;
			employee.HomePhone = homePhone;
			employee.Extension = extension;
			employee.Photo = photo;
			employee.Notes = notes;
			employee.ReportsTo = reportsTo;
			employee.PhotoPath = photoPath;
			return employee;
		}
	
		public static EmployeeTerritory EmployeeTerritory(int employeeId, string territoryId)
		{
			EmployeeTerritory employeeTerritory = new EmployeeTerritory();
			employeeTerritory.EmployeeId = employeeId;
			employeeTerritory.TerritoryId = territoryId;
			return employeeTerritory;
		}
	
		public static Order Order(int orderId, string customerId, int employeeId, DateTime? orderDate, DateTime? requiredDate, DateTime? shippedDate, int shipVia, decimal freight, string shipName, string address, string city, string region, string postalCode, string country)
		{
			Order order = new Order();
			order.Id = orderId;
			order.CustomerId = customerId;
			order.EmployeeId = employeeId;
			order.OrderDate = orderDate;
			order.RequiredDate = requiredDate;
			order.ShippedDate = shippedDate;
			order.ShipVia = new int?(shipVia);
			order.Freight = freight;
			order.ShipName = shipName;
			order.ShipAddress = address;
			order.ShipCity = city;
			order.ShipRegion = region;
			order.ShipPostalCode = postalCode;
			order.ShipCountry = country;
			return order;
		}
	
		public static OrderDetail OrderDetail(int orderId, int productId, decimal unitPrice, short quantity, double discount)
		{
			OrderDetail orderDetail = new OrderDetail();
			orderDetail.OrderId = orderId;
			orderDetail.ProductId = productId;
			orderDetail.UnitPrice = unitPrice;
			orderDetail.Quantity = quantity;
			orderDetail.Discount = discount;
			return orderDetail;
		}
	
		public static Product Product(int productId, string productName, int supplierId, int categoryId, string qtyPerUnit, decimal unitPrice, short unitsInStock, short unitsOnOrder, short reorderLevel, bool discontinued)
		{
			Product product = new Product();
			product.Id = productId;
			product.ProductName = productName;
			product.SupplierId = supplierId;
			product.CategoryId = categoryId;
			product.QuantityPerUnit = qtyPerUnit;
			product.UnitPrice = unitPrice;
			product.UnitsInStock = unitsInStock;
			product.UnitsOnOrder = unitsOnOrder;
			product.ReorderLevel = reorderLevel;
			product.Discontinued = discontinued;
			return product;
		}
	
		public static Region Region(int regionId, string regionDescription)
		{
			Region region = new Region();
			region.Id = regionId;
			region.RegionDescription = regionDescription;
			return region;
		}
	
		public static Shipper Shipper(int id, string companyName, string phoneNo)
		{
			Shipper shipper = new Shipper();
			shipper.Id = id;
			shipper.CompanyName = companyName;
			shipper.Phone = phoneNo;
			return shipper;
		}
	
		public static Supplier Supplier(int supplierId, string companyName, string contactName, string contactTitle, string address, string city, string region, string postalCode, string country, string phoneNo, string faxNo, string homePage)
		{
			Supplier supplier = new Supplier();
			supplier.Id = supplierId;
			supplier.CompanyName = companyName;
			supplier.ContactName = contactName;
			supplier.ContactTitle = contactTitle;
			supplier.Address = address;
			supplier.City = city;
			supplier.Region = region;
			supplier.PostalCode = postalCode;
			supplier.Country = country;
			supplier.Phone = phoneNo;
			supplier.Fax = faxNo;
			supplier.HomePage = homePage;
			return supplier;
		}
	
		public static Territory Territory(string territoryId, string territoryDescription, int regionId)
		{
			Territory territory = new Territory();
			territory.Id = territoryId;
			territory.TerritoryDescription = territoryDescription;
			territory.RegionId = regionId;
			return territory;
		}
	}
}