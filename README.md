[Join the new google group](http://groups.google.com/group/servicestack) or
follow [@demisbellot](http://twitter.com/demisbellot) and [@ServiceStack](http://twitter.com/servicestack)
for twitter updates.

ServiceStack.OrmLite is a convention-based, configuration-free lightweight ORM that uses standard POCO classes and Data Annotation attributes to infer its table schema.
# Introduction

OrmLite is a set of light-weight C# extension methods around System.Data.`*` interfaces which is designed to persist POCO classes with a minimal amount of intrusion and configuration.
Another Orm with similar goals is [sqlite-net](http://code.google.com/p/sqlite-net/).

OrmLite was designed with a focus on these core objectives:

  * Simplicity - with minimal, convention-based attribute configuration
  * High performance - with support for indexes, text blobs, etc.
  * Expressive power and flexibility - with access to IDbCommand and raw SQL
  * Cross platform - supports multiple dbs (currently: Sqlite and Sql Server) running on both .NET and Mono platforms.

# Download 
OrmLite is included with [ServiceStack.zip](https://github.com/downloads/mythz/ServiceStack/ServiceStack.zip) or available to download separately in a standalone 
[ ServiceStack.OrmLite.zip](https://github.com/downloads/mythz/ServiceStack.OrmLite/ServiceStack.OrmLite.zip).

The full source code for OrmLite is also [available online](https://github.com/ServiceStack/ServiceStack.OrmLite).

# Limitations 

For simplicity, and to be able to have the same POCO class persisted in db4o, memcached, redis or on the filesystem (i.e. providers included in ServiceStack), each model must have an '`Id`' property which is its primary key.  

# Code-first Customer and Order example with complex types POCO blobs

Below is a complete stand-alone example. No other config or classes is required for it to run. It's also available as a 
[stand-alone unit test](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/UseCase/CustomerOrdersUseCase.cs).

    public enum PhoneType {
        Home,
        Work,
        Mobile,
    }

    public enum AddressType {
        Home,
        Work,
        Other,
    }

    public class Address {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string ZipCode { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class Customer {
        public Customer() {
            this.PhoneNumbers = new Dictionary<PhoneType, string>();
            this.Addresses = new Dictionary<AddressType, Address>();
        }

        [AutoIncrement] // Creates Auto primary key
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Index(Unique = true)] // Creates Unique Index
        public string Email { get; set; }
        public Dictionary<PhoneType, string> PhoneNumbers { get; private set; }
        public Dictionary<AddressType, Address> Addresses { get; private set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Order {
        [AutoIncrement]
        public int Id { get; set; }
        [References(typeof(Customer))] //Creates Foreign Key
        public int CustomerId { get; set; }
        [References(typeof(Employee))] //Creates Foreign Key
        public int EmployeeId { get; set; }
        public Address ShippingAddress { get; set; } //Blobbed (no Address table)
        public DateTime? OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public int? ShipVia { get; set; }
        public decimal Freight { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderDetail {
        [AutoIncrement]
        public int Id { get; set; }
        [References(typeof(Order))] //Creates Foreign Key
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public short Quantity { get; set; }
        public decimal Discount { get; set; }
    }

    public class Employee {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Product {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
    }

    //Setup SQL Server Connection Factory
    var dbFactory = new OrmLiteConnectionFactory(
	@"Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\App_Data\Database1.mdf;Integrated Security=True;User Instance=True",
	SqlServerOrmLiteDialectProvider.Instance);

    //Use in-memory Sqlite DB instead
    //var dbFactory = new OrmLiteConnectionFactory(
    //    ":memory:", false, SqliteOrmLiteDialectProvider.Instance);

    //Non-intrusive: All extension methods hang off System.Data.* interfaces
    IDbConnection dbConn = dbFactory.OpenDbConnection();
    IDbCommand dbCmd = dbConn.CreateCommand();

    //Re-Create all table schemas:
    dbCmd.DropTable<OrderDetail>();
    dbCmd.DropTable<Order>();
    dbCmd.DropTable<Customer>();
    dbCmd.DropTable<Product>();
    dbCmd.DropTable<Employee>();

    dbCmd.CreateTable<Employee>();
    dbCmd.CreateTable<Product>();
    dbCmd.CreateTable<Customer>();
    dbCmd.CreateTable<Order>();
    dbCmd.CreateTable<OrderDetail>();

    dbCmd.Insert(new Employee { Id = 1, Name = "Employee 1" });
    dbCmd.Insert(new Employee { Id = 2, Name = "Employee 2" });
    var product1 = new Product { Id = 1, Name = "Product 1", UnitPrice = 10 };
    var product2 = new Product { Id = 2, Name = "Product 2", UnitPrice = 20 };
    dbCmd.Save(product1, product2);

    var customer = new Customer
    {
        FirstName = "Orm",
        LastName = "Lite",
        Email = "ormlite@servicestack.net",
        PhoneNumbers =
            {
                { PhoneType.Home, "555-1234" },
                { PhoneType.Work, "1-800-1234" },
                { PhoneType.Mobile, "818-123-4567" },
            },
        Addresses =
            {
                { AddressType.Work, new Address { Line1 = "1 Street", Country = "US", State = "NY", City = "New York", ZipCode = "10101" } },
            },
        CreatedAt = DateTime.UtcNow,
    };
    dbCmd.Insert(customer);

    var customerId = dbCmd.GetLastInsertId(); //Get Auto Inserted Id
    customer = dbCmd.QuerySingle<Customer>(new { customer.Email }); //Query
    Assert.That(customer.Id, Is.EqualTo(customerId));

    //Direct access to System.Data.Transactions:
    using (var trans = dbCmd.BeginTransaction(IsolationLevel.ReadCommitted))
    {
        var order = new Order
        {
            CustomerId = customer.Id,
            EmployeeId = 1,
            OrderDate = DateTime.UtcNow,
            Freight = 10.50m,
            ShippingAddress = new Address { Line1 = "3 Street", Country = "US", State = "NY", City = "New York", ZipCode = "12121" },
        };
        dbCmd.Save(order); //Inserts 1st time

        order.Id = (int)dbCmd.GetLastInsertId(); //Get Auto Inserted Id

        var orderDetails = new[] {
            new OrderDetail
            {
                OrderId = order.Id,
                ProductId = product1.Id,
                Quantity = 2,
                UnitPrice = product1.UnitPrice,
            },
            new OrderDetail
            {
                OrderId = order.Id,
                ProductId = product2.Id,
                Quantity = 2,
                UnitPrice = product2.UnitPrice,
                Discount = .15m,
            }
        };

        dbCmd.Insert(orderDetails);

        order.Total = orderDetails.Sum(x => x.UnitPrice * x.Quantity * x.Discount) + order.Freight;

        dbCmd.Save(order); //Updates 2nd Time

        trans.Commit();
    }

Running this against a SQL Server database will yield the results below:

[![SQL Server Management Studio results](http://www.servicestack.net/files/ormlite-example.png)](http://www.servicestack.net/files/ormlite-example.png)

Notice the POCO types are stored in the [very fast](http://www.servicestack.net/mythz_blog/?p=176) 
and [Versatile](http://www.servicestack.net/mythz_blog/?p=314) 
[JSV Format](https://github.com/ServiceStack/ServiceStack.Text/wiki/JSV-Format) which although hard to do - 
is actually more compact, human and parser-friendly than JSON :)

# More Examples 

In its simplest useage, OrmLite can persist any POCO type without any attributes required:

	public class SimpleExample
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	//Set once before use (i.e. in a static constructor).
	OrmLiteConfig.DialectProvider = new SqliteOrmLiteDialectProvider();

	using (IDbConnection db = "/path/to/db.sqlite".OpenDbConnection())
	using (IDbCommand dbConn = db.CreateCommand())
	{
		dbConn.CreateTable<SimpleExample>(true);
		dbConn.Insert(new SimpleExample { Id=1, Name="Hello, World!"});
		var rows = dbConn.Select<SimpleExample>();

		Assert.That(rows, Has.Count(1));
		Assert.That(rows[0].Id, Is.EqualTo(1));
	}

To get a better idea of the features of OrmLite lets walk through a complete example using sample tables from the Northwind database. 
_ (Full source code for this example is [available here](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/ShippersExample.cs).) _

So with no other configuration using only the classes below:

	[Alias("Shippers")]
	public class Shipper
		: IHasId<int>
	{
		[AutoIncrement]
		[Alias("ShipperID")]
		public int Id { get; set; }

		[Required]
		[Index(Unique = true)]
		[StringLength(40)]
		public string CompanyName { get; set; }

		[StringLength(24)]
		public string Phone { get; set; }

		[References(typeof(ShipperType))]
		public int ShipperTypeId { get; set; }
	}

	[Alias("ShipperTypes")]
	public class ShipperType
		: IHasId<int>
	{
		[AutoIncrement]
		[Alias("ShipperTypeID")]
		public int Id { get; set; }

		[Required]
		[Index(Unique = true)]
		[StringLength(40)]
		public string Name { get; set; }
	}

	public class SubsetOfShipper
	{
		public int ShipperId { get; set; }
		public string CompanyName { get; set; }
	}

	public class ShipperTypeCount
	{
		public int ShipperTypeId { get; set; }
		public int Total { get; set; }
	}


### Creating tables 
Creating tables is a simple 1-liner:

	using (IDbConnection dbConn = ":memory:".OpenDbConnection())
	using (IDbCommand dbCmd = dbConn.CreateCommand())
	{
		const bool overwrite = false;
		dbCmd.CreateTables(overwrite, typeof(Shipper), typeof(ShipperType));
	}

	/* In debug mode the line above prints:
	DEBUG: CREATE TABLE "Shippers" 
	(
	  "ShipperID" INTEGER PRIMARY KEY AUTOINCREMENT, 
	  "CompanyName" VARCHAR(40) NOT NULL, 
	  "Phone" VARCHAR(24) NULL, 
	  "ShipperTypeId" INTEGER NOT NULL, 

	  CONSTRAINT "FK_Shippers_ShipperTypes" FOREIGN KEY ("ShipperTypeId") REFERENCES "ShipperTypes" ("ShipperID") 
	);
	DEBUG: CREATE UNIQUE INDEX uidx_shippers_companyname ON "Shippers" ("CompanyName" ASC);
	DEBUG: CREATE TABLE "ShipperTypes" 
	(
	  "ShipperTypeID" INTEGER PRIMARY KEY AUTOINCREMENT, 
	  "Name" VARCHAR(40) NOT NULL 
	);
	DEBUG: CREATE UNIQUE INDEX uidx_shippertypes_name ON "ShipperTypes" ("Name" ASC);
	*/


### Transaction Support
As we have direct access to IDbCommand and friends - playing with transactions is easy:

	int trainsTypeId, planesTypeId;
	using (IDbTransaction dbTrans = dbCmd.BeginTransaction())
	{
		dbCmd.Insert(new ShipperType { Name = "Trains" });
		trainsTypeId = (int) dbCmd.GetLastInsertId();

		dbCmd.Insert(new ShipperType { Name = "Planes" });
		planesTypeId = (int) dbCmd.GetLastInsertId();

		dbTrans.Commit();
	}
	using (IDbTransaction dbTrans = dbCmd.BeginTransaction(IsolationLevel.ReadCommitted))
	{
		dbCmd.Insert(new ShipperType { Name = "Automobiles" });
		Assert.That(dbCmd.Select<ShipperType>(), Has.Count(3));

		dbTrans.Rollback();
	}
	Assert.That(dbCmd.Select<ShipperType>(), Has.Count(2));


### CRUD Operations 
No ORM is complete without the standard crud operations:

	//Performing standard Insert's and Selects
	dbCmd.Insert(new Shipper { CompanyName = "Trains R Us", Phone = "555-TRAINS", ShipperTypeId = trainsTypeId });
	dbCmd.Insert(new Shipper { CompanyName = "Planes R Us", Phone = "555-PLANES", ShipperTypeId = planesTypeId });
	dbCmd.Insert(new Shipper { CompanyName = "We do everything!", Phone = "555-UNICORNS", ShipperTypeId = planesTypeId });

	var trainsAreUs = dbCmd.First<Shipper>("ShipperTypeId = {0}", trainsTypeId);
	Assert.That(trainsAreUs.CompanyName, Is.EqualTo("Trains R Us"));
	Assert.That(dbCmd.Select<Shipper>("CompanyName = {0} OR Phone = {1}", "Trains R Us", "555-UNICORNS"), Has.Count(2));
	Assert.That(dbCmd.Select<Shipper>("ShipperTypeId = {0}", planesTypeId), Has.Count(2));

	//Lets update a record
	trainsAreUs.Phone = "666-TRAINS";
	dbCmd.Update(trainsAreUs);
	Assert.That(dbCmd.GetById<Shipper>(trainsAreUs.Id).Phone, Is.EqualTo("666-TRAINS"));

	//Then make it disappear
	dbCmd.Delete(trainsAreUs);
	Assert.That(dbCmd.GetByIdOrDefault<Shipper>(trainsAreUs.Id), Is.Null);

	//And bring it back again
	dbCmd.Insert(trainsAreUs);


### Performing custom queries 
And with access to raw sql when you need it - the database is your oyster :)

	//Select only a subset from the table
	var partialColumns = dbCmd.Select<SubsetOfShipper>(typeof (Shipper), "ShipperTypeId = {0}", planesTypeId);
	Assert.That(partialColumns, Has.Count(2));

	//Select into another POCO class that matches the sql results
	var rows = dbCmd.Select<ShipperTypeCount>(
		"SELECT ShipperTypeId, COUNT(*) AS Total FROM Shippers GROUP BY ShipperTypeId ORDER BY COUNT(*)");

	Assert.That(rows, Has.Count(2));
	Assert.That(rows[0].ShipperTypeId, Is.EqualTo(trainsTypeId));
	Assert.That(rows[0].Total, Is.EqualTo(1));
	Assert.That(rows[1].ShipperTypeId, Is.EqualTo(planesTypeId));
	Assert.That(rows[1].Total, Is.EqualTo(2));


	//And finally lets quickly clean up the mess we've made:
	dbCmd.DeleteAll<Shipper>();
	dbCmd.DeleteAll<ShipperType>();

	Assert.That(dbCmd.Select<Shipper>(), Has.Count(0));
	Assert.That(dbCmd.Select<ShipperType>(), Has.Count(0));