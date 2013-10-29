[Join the ServiceStack Google+ group](https://plus.google.com/u/0/communities/112445368900682590445) or
follow [@servicestack](http://twitter.com/servicestack) for updates.

# An Open Source Micro ORM for .NET

_Note: the source code is provided as-is - no direct or commercial support is available for ServiceStack_

# Introduction

OrmLite is a set of light-weight C# extension methods around `System.Data.*` interfaces which is designed to persist POCO classes with a minimal amount of intrusion and configuration.
Another Orm with similar goals is [sqlite-net](https://github.com/praeclarum/sqlite-net) by Frank Krueger.

OrmLite was designed with a focus on the core objectives:

  * Map a POCO class 1:1 to an RDBMS table, cleanly by conventions, without any attributes required.
  * Create/Drop DB Table schemas using nothing but POCO class definitions (IOTW a true code-first ORM)
  * Simplicity - typed, wrist friendly API for common data access patterns.
  * High performance - with support for indexes, text blobs, etc.
    * Amongst the [fastest Micro ORMs](http://servicestack.net/benchmarks/) for .NET (just behind [Dapper](http://code.google.com/p/dapper-dot-net/)).
  * Expressive power and flexibility - with access to IDbCommand and raw SQL
  * Cross platform - supports multiple dbs (currently: Sql Server, Sqlite, MySql, PostgreSQL, Firebird) running on both .NET and Mono platforms.

In OrmLite: **1 Class = 1 Table**. There should be no surprising or hidden behaviour.
Any non-scalar properties (i.e. complex types) are text blobbed in a schema-less text field using [.NET's fastest Text Serializer](http://www.servicestack.net/mythz_blog/?p=176).
Effectively this allows you to create a table from any POCO type and it should persist as expected in a DB Table with columns for each of the classes 1st level public properties.

# Download 

[![Download on NuGet](http://www.servicestack.net/img/nuget-servicestack.ormlite.sqlserver.png)](http://nuget.org/List/Packages/ServiceStack.OrmLite.SqlServer)

### 8 flavours of OrmLite is on NuGet: 

  - [Sql Server](http://nuget.org/List/Packages/ServiceStack.OrmLite.SqlServer)
  - [MySql](http://nuget.org/List/Packages/ServiceStack.OrmLite.MySql)
  - [PostgreSQL](http://nuget.org/List/Packages/ServiceStack.OrmLite.PostgreSQL)
  - [Oracle](http://nuget.org/packages/ServiceStack.OrmLite.Oracle)
  - [Firebird](http://nuget.org/List/Packages/ServiceStack.OrmLite.Firebird) 
  - [Sqlite32](http://nuget.org/List/Packages/ServiceStack.OrmLite.Sqlite32) - 32bit Mixed mode .NET only assembly 
  - [Sqlite64](http://nuget.org/List/Packages/ServiceStack.OrmLite.Sqlite64) - 64bit Mixed mode .NET only assembly 
  - [Sqlite.Mono](http://nuget.org/packages/ServiceStack.OrmLite.Sqlite.Mono) - 32bit unmanaged dll, compatible with .NET / Mono

OrmLite.SqlServer is also included in [ServiceStack](https://github.com/ServiceStack/ServiceStack/downloads).

_Note: the binary packages are provided as-is - there is no commercial support available for ServiceStack_

## Download published NuGet binaries without NuGet

For environments that doesn't have NuGet installed (e.g. OSX/Linux) you can still download the published binaries by 
extracting them from the published NuGet packages. The url to download a nuget package is: 

    http://packages.nuget.org/api/v1/package/{PackageName}/{Version}
    
 So to get the OrmLite MySQL provider in OSX/Linux (or using gnu tools for Windows) you can just do:

    wget -O OrmLite.MySql.zip http://packages.nuget.org/api/v1/package/ServiceStack.OrmLite.MySql/3.9.60
    unzip OrmLite.MySql.zip 'lib/*'

which will download and extract the dlls into your local local `lib/` folder.

## IMPORTANT NOTICE

Please upgrade your client code to use non-deprecated methods as we will be removing access to deprecated APIs in the next release of OrmLite.

***

## T4 Template Support

[Guru Kathiresan](https://github.com/gkathire) continues to enhance [OrmLite's T4 Template support](https://github.com/ServiceStack/ServiceStack.OrmLite/tree/master/src/T4) which are useful when you want to automatically generate POCO's and strong-typed wrappers for executing stored procedures. 


## New Parameterized API's

We've now added API's that use parameterized statements for all SQL operations, these are identified with a **Param** suffix, e.g:

### Parameterized Write operations

```csharp
db.InsertParam(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27})
db.UpdateParam(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27})
db.DeleteByIdParam<Person>(1)
```

### Parameterized Read operations

```csharp
var people = db.SelectParam<Person>(q => q.Age == 27)
var person = db.GetByIdParam<Person>(1)

//Existing parameterized query API's
var people = db.Where<Person>(new { FirstName = "Jimi", Age = 27 })
var people = db.Query<Track>("FirstName = @name and Age = @age", new { name = "Jimi", age = 27 })
```

Apart from a slight performance increase, parameterized API's now lets you insert and update binary blob data. At the same time as these new parameterized API's, we've also added support for querying binary blob data.

## New API's to execute custom SQL

Prior to v3.9.60 the ways to execute custom SQL was with `db.ExecuteSql()` which as it only returned an int code, users were using `db.Query` to read arbitrary sql returning tabular resultsets. However db.Query is only intended for SELECT statements. For this purpose we've introduced new API's for executing custom sql, e.g:

```csharp
List<Poco> results = db.SqlList<Poco>("EXEC GetAnalyticsForWeek 1");
List<Poco> results = db.SqlList<Poco>("EXEC GetAnalyticsForWeek @weekNo", new { weekNo = 1 });

List<int> results = db.SqlList<int>("EXEC GetTotalsForWeek 1");
List<int> results = db.SqlList<int>("EXEC GetTotalsForWeek @weekNo", new { weekNo = 1 });

int result = db.SqlScalar<int>("SELECT 10");
```

Some more examples can be found in [SqlServerProviderTests](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/SqlServerProviderTests.cs).

## New Simplified API
We've streamlined our API, now all OrmLite extensions that used to be on `IDbCommand` now hang off `IDbConnection` 
(just like Dapper), this reduces the boiler-plate when opening a connection to a single line, so now you can 
create a table and insert a record with just:

```csharp
using (IDbConnection db = dbFactory.OpenDbConnection()) {
	db.CreateTable<Employee>();
	db.Insert(new Employee { Id = 1, Name = "Employee 1" });
}
```
    
> The methods off `IDbCommand` have now been deprecated and will one day be removed. Update your library.

## New Foreign Key attribute for referential actions on Update/Deletes

Creating a foreign key in OrmLite can be done by adding `[References(typeof(ForeignKeyTable))]` on the relation property,
which will result in OrmLite creating the Foreign Key relationship when it creates the DB table with `db.CreateTable<Poco>`.
[@brainless83](https://github.com/brainless83) has extended this support further by adding more finer-grain options 
and behaviours with the new `[ForeignKey]` attribute which will now let you specify the desired behaviour when deleting
or updating related rows in Foreign Key tables. 

An example of a table with all the different options:

```csharp
public class TableWithAllCascadeOptions
{
	[AutoIncrement] public int Id { get; set; }
	
	[References(typeof(ForeignKeyTable1))]
	public int SimpleForeignKey { get; set; }
	
	[ForeignKey(typeof(ForeignKeyTable2), OnDelete = "CASCADE", OnUpdate = "CASCADE")]
	public int? CascadeOnUpdateOrDelete { get; set; }
	
	[ForeignKey(typeof(ForeignKeyTable3), OnDelete = "NO ACTION")]
	public int? NoActionOnCascade { get; set; }
	
	[Default(typeof(int), "17")]
	[ForeignKey(typeof(ForeignKeyTable4), OnDelete = "SET DEFAULT")]
	public int SetToDefaultValueOnDelete { get; set; }
	
	[ForeignKey(typeof(ForeignKeyTable5), OnDelete = "SET NULL")]
	public int? SetToNullOnDelete { get; set; }
}
```

The [ForeignKeyTests](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/ForeignKeyAttributeTests.cs)
show the resulting behaviour with each of these configurations in more detail.

> Note: Only supported on RDBMS's with foreign key/referential action support, e.g. 
[Sql Server](http://msdn.microsoft.com/en-us/library/ms174979.aspx), 
[PostgreSQL](http://www.postgresql.org/docs/9.1/static/ddl-constraints.html),
[MySQL](http://dev.mysql.com/doc/refman/5.5/en/innodb-foreign-key-constraints.html). Otherwise they're ignored.

## Multi nested database connections

We now support multiple nested database connections so you can now trivially use OrmLite to access multiple databases
on different connections. The `OrmLiteConnectionFactory` class has been extended to support named connections which 
allows you to conveniently define all your db connections when you register it in your IOC and access them with the 
named property when you use them.

A popular way of scaling RDBMS's is to create a Master / Shard setup where datasets for queries that span entire system
are kept in the master database, whilst context-specific related data can be kept together in an isolated shard.
This feature makes it trivial to maintain multiple separate db shards with a master database in a different RDBMS. 

Here's an (entire source code) sample of the code needed to define, and populate a Master/Shard setup.
Sqlite can create DB shards on the fly so only the blank SqlServer master database needed to be created out-of-band:

### Sharding 1000 Robots into 10 Sqlite DB shards - referencing each in a Master SqlServer RDBMS

```csharp
public class MasterRecord {
    public Guid Id { get; set; }
    public int RobotId { get; set; }
    public string RobotName { get; set; }
    public DateTime? LastActivated { get; set; }
}

public class Robot {
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActivated { get; set; }
    public long CellCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

const int NoOfShards = 10;
const int NoOfRobots = 1000;

var dbFactory = new OrmLiteConnectionFactory(
    "Data Source=host;Initial Catalog=RobotsMaster;Integrated Security=SSPI",  //Connection String
    SqlServerDialect.Provider); 

dbFactory.Run(db => db.CreateTable<MasterRecord>(overwrite:false));

NoOfShards.Times(i => {
    var namedShard = "robots-shard" + i;
    dbFactory.RegisterConnection(namedShard, 
        "~/App_Data/{0}.sqlite".Fmt(shardId).MapAbsolutePath(),                //Connection String
        SqliteDialect.Provider);
	
	dbFactory.OpenDbConnection(namedShard).Run(db => db.CreateTable<Robot>(overwrite:false));
});

var newRobots = NoOfRobots.Times(i => //Create 1000 Robots
    new Robot { Id=i, Name="R2D"+i, CreatedDate=DateTime.UtcNow, CellCount=DateTime.Now.ToUnixTimeMs() % 100000 });

foreach (var newRobot in newRobots) 
{
    using (IDbConnection db = dbFactory.OpenDbConnection()) //Open Connection to Master DB 
    {
        db.Insert(new MasterRecord { Id = Guid.NewGuid(), RobotId = newRobot.Id, RobotName = newRobot.Name });
        using (IDbConnection robotShard = dbFactory.OpenDbConnection("robots-shard"+newRobot.Id % NoOfShards)) //Shard
        {
            robotShard.Insert(newRobot);
        }
    }
}
```

Using the [SQLite Manager](https://addons.mozilla.org/en-US/firefox/addon/sqlite-manager/?src=search) Firefox extension
we can peek at one of the created shards to see 100 Robots in each shard. This is the dump of `robots-shard0.sqlite`:

![Data dump of Robot Shard #1](http://www.servicestack.net/files/robots-shard0.png)

As expected each shard has every 10th robot inside.

## New strong-typed Sql Expression API

We've now added SQL Expression support to bring you even nicer LINQ-liked querying to all our providers. 
To give you a flavour here are some examples with their partial SQL output (done in SQL Server): 

### Querying with SELECT

```csharp
int agesAgo = DateTime.Today.AddYears(-20).Year;
db.Select<Author>(q => q.Birthday >= new DateTime(agesAgo, 1, 1) && q.Birthday <= new DateTime(agesAgo, 12, 31));
```

**WHERE (("Birthday" >= '1992-01-01 00:00:00.000') AND ("Birthday" <= '1992-12-31 00:00:00.000'))**

```csharp
db.Select<Author>(q => Sql.In(q.City, "London", "Madrid", "Berlin"));
```

**WHERE "JobCity" In ('London', 'Madrid', 'Berlin')**

```csharp
db.Select<Author>(q => q.Earnings <= 50);
```

**WHERE ("Earnings" <= 50)**

```csharp
db.Select<Author>(q => q.Name.StartsWith("A"));
```

**WHERE upper("Name") like 'A%'**

```csharp
db.Select<Author>(q => q.Name.EndsWith("garzon"));
```

**WHERE upper("Name") like '%GARZON'**

```csharp
db.Select<Author>(q => q.Name.Contains("Benedict"));
```

**WHERE upper("Name") like '%BENEDICT%'**

```csharp
db.Select<Author>(q => q.Rate == 10 && q.City == "Mexico");
```

**WHERE (("Rate" = 10) AND ("JobCity" = 'Mexico'))**

Right now the Expression support can satisfy most simple queries with a strong-typed API. 
For anything more complex (e.g. queries with table joins) you can still easily fall back to raw SQL queries as seen below. 

### INSERT, UPDATE and DELETEs

To see the behaviour of the different APIs, all examples uses this simple model

```csharp
public class Person
{
	public int Id { get; set; }
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public int? Age { get; set; }
}
```

### UPDATE

In its most simple form, updating any model without any filters will update every field, except the **Id** which 
is used to filter the update to this specific record:

```csharp
db.Update(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27});
```
**UPDATE "Person" SET "FirstName" = 'Jimi',"LastName" = 'Hendrix',"Age" = 27 WHERE "Id" = 1**

If you supply your own where expression, it updates every field (inc. Id) but uses your filter instead:

```csharp
db.Update(new Person { Id = 1, FirstName = "JJ" }, p => p.LastName == "Hendrix");
```
**UPDATE "Person" SET "Id" = 1,"FirstName" = 'JJ',"LastName" = NULL,"Age" = NULL WHERE ("LastName" = 'Hendrix')**

One way to limit the fields which gets updated is to use an **Anonymous Type**:

```csharp
db.Update<Person>(new { FirstName = "JJ" }, p => p.LastName == "Hendrix");
```

Or by using `UpdateNonDefaults` which only updates the non-default values in your model using the filter specified:

```csharp
db.UpdateNonDefaults(new Person { FirstName = "JJ" }, p => p.LastName == "Hendrix");
```

**UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("LastName" = 'Hendrix')**

#### UpdateOnly

As updating a partial row is a common use-case in Db's, we've added a number of methods for just 
this purpose, named **UpdateOnly**.

The first expression in an `UpdateOnly` statement is used to specify which fields should be updated:
```csharp
db.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName);
```
**UPDATE "Person" SET "FirstName" = 'JJ'**

```csharp
db.UpdateOnly(new Person { FirstName = "JJ", Age = 12 }, p => new { p.FirstName, p.Age });
```
**UPDATE "Person" SET "FirstName" = 'JJ', "Age" = 12**

When present, the second expression is used as the where filter:
```csharp
db.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName, p => p.LastName == "Hendrix");
```
**UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("LastName" = 'Hendrix')**

Instead of using the expression filters above you can choose to use an ExpressionVisitor builder which provides more 
flexibility when you want to programatically construct the update statement:

```csharp
db.UpdateOnly(new Person { FirstName = "JJ", LastName = "Hendo" }, ev => ev.Update(p => p.FirstName));
```
**UPDATE "Person" SET "FirstName" = 'JJ'**

```csharp
db.UpdateOnly(new Person { FirstName = "JJ" }, ev => ev.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
```
**UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("LastName" = 'Hendrix')**

For the ultimate flexibility we also provide un-typed, string-based expressions. Use the `.Params()` extension method
escape parameters (inspired by [massive](https://github.com/robconery/massive)):

```csharp
db.Update<Person>(set: "FirstName = {0}".Params("JJ"), where: "LastName = {0}".Params("Hendrix"));
```
Even the Table name can be a string so you perform the same update without requiring the Person model at all:

```csharp
db.Update(table: "Person", set: "FirstName = {0}".Params("JJ"), where: "LastName = {0}".Params("Hendrix"));
```
**UPDATE "Person" SET FirstName = 'JJ' WHERE LastName = 'Hendrix'**

### INSERT

Insert's are pretty straight forward since in most cases you want to insert every field:

```csharp
db.Insert(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });
```
**INSERT INTO "Person" ("Id","FirstName","LastName","Age") VALUES (1,'Jimi','Hendrix',27)**

But do provide an API that takes an Expression Visitor for the rare cases you don't want to insert every field

```csharp
db.InsertOnly(new Person { FirstName = "Amy" }, ev => ev.Insert(p => new { p.FirstName }));
```
**INSERT INTO "Person" ("FirstName") VALUES ('Amy')**

### DELETE

Like updates for DELETE's we also provide APIs that take a where Expression:
```csharp
db.Delete<Person>(p => p.Age == 27);
```

Or an Expression Visitor:
```csharp
db.Delete<Person>(ev => ev.Where(p => p.Age == 27));
```

**DELETE FROM "Person" WHERE ("Age" = 27)**

As well as un-typed, string-based expressions:
```csharp
db.Delete<Person>(where: "Age = {0}".Params(27));
```

Which also can take a table name so works without requiring a typed **Person** model
```csharp
db.Delete(table: "Person", where: "Age = {0}".Params(27));
```

**DELETE FROM "Person" WHERE Age = 27**


## Code-first Customer & Order example with complex types on POCO as text blobs

Below is a complete stand-alone example. No other config or classes is required for it to run. It's also available as a 
[stand-alone unit test](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/UseCase/CustomerOrdersUseCase.cs).

```csharp
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
        
        public Dictionary<PhoneType, string> PhoneNumbers { get; set; }  //Blobbed
        public Dictionary<AddressType, Address> Addresses { get; set; }  //Blobbed
        public DateTime CreatedAt { get; set; }
    }

    public class Order {
        
        [AutoIncrement]
        public int Id { get; set; }
        
        [References(typeof(Customer))]      //Creates Foreign Key
        public int CustomerId { get; set; }
        
        [References(typeof(Employee))]      //Creates Foreign Key
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
    	SqlServerDialect.Provider);

    //Use in-memory Sqlite DB instead
    //var dbFactory = new OrmLiteConnectionFactory(
    //    ":memory:", false, SqliteDialect.Provider);

    //Non-intrusive: All extension methods hang off System.Data.* interfaces
    IDbConnection db = dbFactory.OpenDbConnection();

    //Re-Create all table schemas:
    db.DropTable<OrderDetail>();
    db.DropTable<Order>();
    db.DropTable<Customer>();
    db.DropTable<Product>();
    db.DropTable<Employee>();

    db.CreateTable<Employee>();
    db.CreateTable<Product>();
    db.CreateTable<Customer>();
    db.CreateTable<Order>();
    db.CreateTable<OrderDetail>();

    db.Insert(new Employee { Id = 1, Name = "Employee 1" });
    db.Insert(new Employee { Id = 2, Name = "Employee 2" });
    var product1 = new Product { Id = 1, Name = "Product 1", UnitPrice = 10 };
    var product2 = new Product { Id = 2, Name = "Product 2", UnitPrice = 20 };
    db.Save(product1, product2);

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
    db.Insert(customer);

    var customerId = db.GetLastInsertId(); //Get Auto Inserted Id
    customer = db.QuerySingle<Customer>(new { customer.Email }); //Query
    Assert.That(customer.Id, Is.EqualTo(customerId));

    //Direct access to System.Data.Transactions:
    using (var trans = db.OpenTransaction(IsolationLevel.ReadCommitted))
    {
        var order = new Order
        {
            CustomerId = customer.Id,
            EmployeeId = 1,
            OrderDate = DateTime.UtcNow,
            Freight = 10.50m,
            ShippingAddress = new Address { Line1 = "3 Street", Country = "US", State = "NY", City = "New York", ZipCode = "12121" },
        };
        db.Save(order); //Inserts 1st time

        order.Id = (int)db.GetLastInsertId(); //Get Auto Inserted Id

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

        db.Insert(orderDetails);

        order.Total = orderDetails.Sum(x => x.UnitPrice * x.Quantity * x.Discount) + order.Freight;

        db.Save(order); //Updates 2nd Time

        trans.Commit();
    }
```

Running this against a SQL Server database will yield the results below:

[![SQL Server Management Studio results](http://www.servicestack.net/files/ormlite-example.png)](http://www.servicestack.net/files/ormlite-example.png)

Notice the POCO types are stored in the [very fast](http://www.servicestack.net/mythz_blog/?p=176) 
and [Versatile](http://www.servicestack.net/mythz_blog/?p=314) 
[JSV Format](https://github.com/ServiceStack/ServiceStack.Text/wiki/JSV-Format) which although hard to do - 
is actually more compact, human and parser-friendly than JSON :)

# API Overview

The API is minimal, providing basic shortcuts for the primitive SQL statements:

[![OrmLite API](http://www.servicestack.net/files/ormlite-api.png)](http://www.servicestack.net/files/ormlite-api.png)

Nearly all extension methods hang off the implementation agnostic `IDbCommand`.

`CreateTable<T>` and `DropTable<T>` create and drop tables based on a classes type definition (only public properties used).

For a one-time use of a connection, you can query straight of the `IDbConnectionFactory` with:

```csharp
var customers = dbFactory.Run(db => db.Where<Customer>(new { Age = 30 }));
```

The **Select** methods allow you to construct Sql using C# `string.Format()` syntax.
If your SQL doesn't start with a **SELECT** statement, it is assumed a WHERE clause is being provided, e.g:

```csharp
var tracks = db.Select<Track>("Artist = {0} AND Album = {1}", "Nirvana", "Heart Shaped Box");
```

The same results could also be fetched with:

```csharp
var tracks = db.Select<Track>("select * from track WHERE Artist={0} AND Album={1}", "Nirvana", "Heart Shaped Box");
```

**Select** returns multiple records 

```csharp
List<Track> tracks = db.Select<Track>()
```

**Single** returns a single record. Alias: `First`  

```csharp
Track track = db.Single<Track>("RefId = {0}", refId)
```

**Dictionary** returns a Dictionary made from the first two columns. Alias: `GetDictionary`

```csharp
Dictionary<int, string> trackIdNamesMap = db.Dictionary<int, string>("select Id, Name from Track")
```

**Lookup** returns an `Dictionary<K, List<V>>` made from the first two columns. Alias: `GetLookup`

```csharp
Dictionary<int, List<string>> albumTrackNames = db.Lookup<int, string>("select AlbumId, Name from Track")
```

**List** returns a List of first column values. Alias: `GetList`

```csharp
List<string> trackNames = db.List<string>("select Name from Track")
```

**HashSet** returns a HashSet of distinct first column values. Alias: `GetHashSet`

```csharp    
HashSet<string> uniqueTrackNames = db.HashSet<string>("select Name from Track")
```

**Scalar** returns a single scalar value. Alias: `GetScalar`

```csharp
var trackCount = db.Scalar<int>("select count(*) from Track")
```

All **Insert**, **Update**, and **Delete** methods take multiple params, while `InsertAll`, `UpdateAll` and `DeleteAll` take IEnumerables.
**GetLastInsertId** returns the last inserted records auto incremented primary key.

`Save` and `SaveAll` will Insert if no record with **Id** exists, otherwise it Updates. 
Both take multiple items, optimized to perform a single read to check for existing records and are executed within a sinlge transaction.

Methods containing the word **Each** return an IEnumerable<T> and are lazily loaded (i.e. non-buffered).

Selection methods containing the word **Query** or **Where** use parameterized SQL (other selection methods do not).
Anonymous types passed into **Where** are treated like an **AND** filter.

```csharp
var track3 = db.Where<Track>(new { AlbumName = "Throwing Copper", TrackNo = 3 })
```

**Query** statements take in parameterized SQL using properties from the supplied anonymous type (if any)

```csharp
var track3 = db.Query<Track>("select * from Track Where AlbumName = @album and TrackNo = @trackNo", 
	new { album = "Throwing Copper", trackNo = 3 })
```

GetById(s), QueryById(s), etc provide strong-typed convenience methods to fetch by a Table's **Id** primary key field.

```csharp
var track = db.QueryById<Track>(1);
var track = db.Id<Track>(1);                    //Alias: GetById
var tracks = db.Ids<Track>(new[]{ 1,2,3 });     //Alias: GetByIds
```


# Limitations 

For simplicity, and to be able to have the same POCO class persisted in db4o, memcached, redis or on the filesystem 
(i.e. providers included in ServiceStack), each model must have a single primary key, by convention OrmLite expects it
to be `Id` although you use `[Alias("DbFieldName")]` attribute it map it to a column with a different name or use 
the `[PrimaryKey]` attribute to tell OrmLite to use a different property for the primary key.

You can still `SELECT` from these tables, you will just be unable to make use of APIs that rely on it, e.g. 
`Update` or `Delete` where the filter is implied (i.e. not specified), all the APIs that end with `ById`, etc.

### Workaround single Primary Key limitation
A potential workaround to support tables with multiple primary keys is to create an auto generated `Id` property that 
returns a unique value based on all the primary key fields, e.g:

```csharp
public class OrderDetail
{
	public string Id { get { return this.OrderId + "/" + this.ProductId; } }
	
	public int OrderId { get; set; }
	public int ProductId { get; set; }
	public decimal UnitPrice { get; set; }
	public short Quantity { get; set; }
	public double Discount { get; set; }
}
```

### Ignoring DTO Properties
You may use the [Ignore] attribute to denote DTO properties that are not fields in the table. This will force the SQL generation to ignore that property.

# More Examples 

In its simplest useage, OrmLite can persist any POCO type without any attributes required:

```csharp
public class SimpleExample
{
	public int Id { get; set; }
	public string Name { get; set; }
}

//Set once before use (i.e. in a static constructor).
OrmLiteConfig.DialectProvider = SqliteDialect.Provider;

using (IDbConnection db = "/path/to/db.sqlite".OpenDbConnection())
{
	db.CreateTable<SimpleExample>(true);
	db.Insert(new SimpleExample { Id=1, Name="Hello, World!"});
	var rows = db.Select<SimpleExample>();

	Assert.That(rows, Has.Count(1));
	Assert.That(rows[0].Id, Is.EqualTo(1));
}
```

To get a better idea of the features of OrmLite lets walk through a complete example using sample tables from the Northwind database. 
_ (Full source code for this example is [available here](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/ShippersExample.cs).) _

So with no other configuration using only the classes below:

```csharp
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
```

### Creating tables 
Creating tables is a simple 1-liner:

```csharp
	using (IDbConnection db = ":memory:".OpenDbConnection())
	{
		const bool overwrite = false;
		db.CreateTables(overwrite, typeof(Shipper), typeof(ShipperType));
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
```

### Transaction Support
As we have direct access to IDbCommand and friends - playing with transactions is easy:

```csharp
	int trainsTypeId, planesTypeId;
	using (IDbTransaction dbTrans = db.OpenTransaction())
	{
		db.Insert(new ShipperType { Name = "Trains" });
		trainsTypeId = (int) db.GetLastInsertId();

		db.Insert(new ShipperType { Name = "Planes" });
		planesTypeId = (int) db.GetLastInsertId();

		dbTrans.Commit();
	}
	using (IDbTransaction dbTrans = db.OpenTransaction(IsolationLevel.ReadCommitted))
	{
		db.Insert(new ShipperType { Name = "Automobiles" });
		Assert.That(db.Select<ShipperType>(), Has.Count(3));

		dbTrans.Rollback();
	}
	Assert.That(db.Select<ShipperType>(), Has.Count(2));
```

### CRUD Operations 
No ORM is complete without the standard crud operations:

```csharp
	//Performing standard Insert's and Selects
	db.Insert(new Shipper { CompanyName = "Trains R Us", Phone = "555-TRAINS", ShipperTypeId = trainsTypeId });
	db.Insert(new Shipper { CompanyName = "Planes R Us", Phone = "555-PLANES", ShipperTypeId = planesTypeId });
	db.Insert(new Shipper { CompanyName = "We do everything!", Phone = "555-UNICORNS", ShipperTypeId = planesTypeId });

	var trainsAreUs = db.First<Shipper>("ShipperTypeId = {0}", trainsTypeId);
	Assert.That(trainsAreUs.CompanyName, Is.EqualTo("Trains R Us"));
	Assert.That(db.Select<Shipper>("CompanyName = {0} OR Phone = {1}", "Trains R Us", "555-UNICORNS"), Has.Count(2));
	Assert.That(db.Select<Shipper>("ShipperTypeId = {0}", planesTypeId), Has.Count(2));

	//Lets update a record
	trainsAreUs.Phone = "666-TRAINS";
	db.Update(trainsAreUs);
	Assert.That(db.GetById<Shipper>(trainsAreUs.Id).Phone, Is.EqualTo("666-TRAINS"));

	//Then make it disappear
	db.Delete(trainsAreUs);
	Assert.That(db.GetByIdOrDefault<Shipper>(trainsAreUs.Id), Is.Null);

	//And bring it back again
	db.Insert(trainsAreUs);
```

### Performing custom queries 
And with access to raw sql when you need it - the database is your oyster :)

```csharp
	//Select only a subset from the table
	var partialColumns = db.Select<SubsetOfShipper>(typeof (Shipper), "ShipperTypeId = {0}", planesTypeId);
	Assert.That(partialColumns, Has.Count(2));

	//Select into another POCO class that matches the sql results
	var rows = db.Select<ShipperTypeCount>(
		"SELECT ShipperTypeId, COUNT(*) AS Total FROM Shippers GROUP BY ShipperTypeId ORDER BY COUNT(*)");

	Assert.That(rows, Has.Count(2));
	Assert.That(rows[0].ShipperTypeId, Is.EqualTo(trainsTypeId));
	Assert.That(rows[0].Total, Is.EqualTo(1));
	Assert.That(rows[1].ShipperTypeId, Is.EqualTo(planesTypeId));
	Assert.That(rows[1].Total, Is.EqualTo(2));


	//And finally lets quickly clean up the mess we've made:
	db.DeleteAll<Shipper>();
	db.DeleteAll<ShipperType>();

	Assert.That(db.Select<Shipper>(), Has.Count(0));
	Assert.That(db.Select<ShipperType>(), Has.Count(0));
```

## Other notable Micro ORMs for .NET
Many performance problems can be mitigated and a lot of use-cases can be simplified without the use of a heavyweight ORM, and their config, mappings and infrastructure. 
As [performance is the most important feature](https://github.com/mythz/ScalingDotNET) we can recommend the following list, each with their own unique special blend of features. 

  * **[Dapper](http://code.google.com/p/dapper-dot-net/)** - by [@samsaffron](http://twitter.com/samsaffron) and [@marcgravell](http://twitter.com/marcgravell) 
    - The current performance king, supports both POCO and dynamic access, fits in a single class. Put in production to solve [StackOverflow's DB Perf issues](http://samsaffron.com/archive/2011/03/30/How+I+learned+to+stop+worrying+and+write+my+own+ORM). Requires .NET 4.
  * **[PetaPoco](http://www.toptensoftware.com/petapoco/)** - by [@toptensoftware](http://twitter.com/toptensoftware)
    - Fast, supports dynamics, expandos and typed POCOs, fits in a single class, runs on .NET 3.5 and Mono. Includes optional T4 templates for POCO table generation.
  * **[Massive](https://github.com/robconery/massive)** - by [@robconery](http://twitter.com/robconery)
    - Fast, supports dynamics and expandos, smart use of optional params to provide a wrist-friendly api, fits in a single class. Multiple RDBMS support. Requires .NET 4.
  * **[Simple.Data](https://github.com/markrendle/Simple.Data)** - by [@markrendle](http://twitter.com/markrendle)
    - A little slower than above ORMS, most wrist-friendly courtesy of a dynamic API, multiple RDBMS support inc. Mongo DB. Requires .NET 4.
	

# Community Resources

  - [Code Generation using ServiceStack.OrmLite and T4 Text templates](http://jokecamp.wordpress.com/2013/09/07/code-generation-using-servicestack-ormlite-and-t4-text-templates/) by [@jokecamp](https://twitter.com/jokecamp)
  - [Simple ServiceStack OrmLite Example](http://www.curlette.com/?p=1068) by [@robrtc](https://twitter.com/robrtc)
  - [OrmLite Blobbing done with NHibernate and Serialized JSON](http://www.philliphaydon.com/2012/03/ormlite-blobbing-done-with-nhibernate-and-serialized-json/) by [@philliphaydon](https://twitter.com/philliphaydon)
  - [Creating An ASP.NET MVC Blog With ServiceStack.OrmLite](http://www.eggheadcafe.com/tutorials/asp-net/285cbe96-9922-406a-b193-3a0b40e31c40/creating-an-aspnet-mvc-blog-with-servicestackormlite.aspx) by [@peterbromberg](https://twitter.com/peterbromberg)
