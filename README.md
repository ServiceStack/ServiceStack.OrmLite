[Join the ServiceStack Google+ group](https://plus.google.com/u/0/communities/112445368900682590445) or
follow [@servicestack](http://twitter.com/servicestack) for updates.

# Fast, Simple, Typed ORM for .NET

OrmLite's goal is to provide a convenient, DRY, config-free, RDBMS-agnostic typed wrapper that retains a high affinity with SQL, exposing intuitive APIs that generate predictable SQL and maps cleanly to (DTO-friendly) disconnected POCO's. This approach makes easier to reason-about your data access making it obvious what SQL is getting executed at what time, whilst mitigating unexpected behavior, implicit N+1 queries and leaky data access prevalent in Heavy ORMs.

OrmLite was designed with a focus on the core objectives:

  * Provide a set of light-weight C# extension methods around .NET's impl-agnostic `System.Data.*` interfaces
  * Map a POCO class 1:1 to an RDBMS table, cleanly by conventions, without any attributes required.
  * Create/Drop DB Table schemas using nothing but POCO class definitions (IOTW a true code-first ORM)
  * Simplicity - typed, wrist friendly API for common data access patterns.
  * High performance - with support for indexes, text blobs, etc.
    * Amongst the [fastest Micro ORMs](http://servicestack.net/benchmarks/) for .NET (just behind [Dapper](http://code.google.com/p/dapper-dot-net/)).
  * Expressive power and flexibility - with access to IDbCommand and raw SQL
  * Cross platform - supports multiple dbs (currently: Sql Server, Sqlite, MySql, PostgreSQL, Firebird) running on both .NET and Mono platforms.

In OrmLite: **1 Class = 1 Table**. There should be no surprising or hidden behaviour.
Any non-scalar properties (i.e. complex types) are by default text blobbed in a schema-less text field using any of the [avilable pluggable text serializers](#pluggable-complex-type-serializers). Support for [POCO-friendly references](#reference-support-poco-style) is also available to provide a convenient API to persist related models. Effectively this allows you to create a table from any POCO type and it should persist as expected in a DB Table with columns for each of the classes 1st level public properties.

# Download 

[![Download on NuGet](https://raw.githubusercontent.com/ServiceStack/Assets/master/img/release-notes/install-ormlite.png)](https://www.nuget.org/packages?q=servicestack+ormlite)

### 8 flavours of OrmLite is on NuGet: 

  - [ServiceStack.OrmLite.Sql Server](http://nuget.org/List/Packages/ServiceStack.OrmLite.SqlServer)
  - [ServiceStack.OrmLite.MySql](http://nuget.org/List/Packages/ServiceStack.OrmLite.MySql)
  - [ServiceStack.OrmLite.PostgreSQL](http://nuget.org/List/Packages/ServiceStack.OrmLite.PostgreSQL)
  - [ServiceStack.OrmLite.Sqlite.Mono](http://nuget.org/packages/ServiceStack.OrmLite.Sqlite.Mono) - Compatible with Mono / Windows (x86) 
  - [ServiceStack.OrmLite.Sqlite.Windows](http://nuget.org/List/Packages/ServiceStack.OrmLite.Sqlite.Windows) - 32/64bit Mixed mode .NET for WIndows only 
  - [ServiceStack.OrmLite.Oracle](http://nuget.org/packages/ServiceStack.OrmLite.Oracle) (unofficial)
  - [ServiceStack.OrmLite.Firebird](http://nuget.org/List/Packages/ServiceStack.OrmLite.Firebird)  (unofficial)
  - [ServiceStack.OrmLite.VistaDb](http://nuget.org/List/Packages/ServiceStack.OrmLite.VistaDb)  (unofficial)
   
_Latest v4+ on NuGet is a commercial release with [free quotas](https://servicestack.net/download#free-quotas)._

#### [Docs and Downloads for older v3 BSD releases](https://github.com/ServiceStackV3/ServiceStackV3)

### Copying

Since September 2013, ServiceStack source code is available under GNU Affero General Public License/FOSS License Exception, see license.txt in the source. Alternative [commercial licensing](https://servicestack.net/ormlite) is also available.

### Contributing

Contributors need to approve the [Contributor License Agreement](https://docs.google.com/forms/d/16Op0fmKaqYtxGL4sg7w_g-cXXyCoWjzppgkuqzOeKyk/viewform) before submitting pull-requests, see the [Contributing wiki](https://github.com/ServiceStack/ServiceStack/wiki/Contributing) for more details. 

***

# Async API Overview

A quick overview of Async API's can be seen in the class diagram below:

![OrmLite Async APIs](https://raw.githubusercontent.com/ServiceStack/Assets/master/img/ormlite/OrmLiteApiAsync.png) 

Essentially most of OrmLite public API's now have async equivalents of the same name and an additional conventional `*Async` suffix. 
The Async API's also take an optional `CancellationToken` making converting sync code trivial, where you just need to
add the `Async` suffix and **await** keyword, as can be seen in the 
[Customer Orders UseCase upgrade to Async diff](https://github.com/ServiceStack/ServiceStack.OrmLite/commit/c1ce6f0eac99133fc232b263c26c42379d4c5f48)
, e.g:

Sync:

```csharp
db.Insert(new Employee { Id = 1, Name = "Employee 1" });
db.Save(product1, product2);
var customer = db.Single<Customer>(new { customer.Email }); 
```

Async:

```csharp
await db.InsertAsync(new Employee { Id = 1, Name = "Employee 1" });
await db.SaveAsync(product1, product2);
var customer = await db.SingleAsync<Customer>(new { customer.Email });
```

> Effectively the only Data Access API's that doesn't have async equivalents are `*Lazy` APIs yielding a lazy 
> sequence (incompatible with async) as well as **Schema** DDL API's which are typically not used at runtime.

For a quick preview of many of the new Async API's in action, checkout 
[ApiSqlServerTestsAsync.cs](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLiteV45.Tests/ApiSqlServerTestsAsync.cs).

### Async RDBMS Providers

Currently only a limited number of RDBMS providers offer async API's which are only available in their **.NET 4.5** builds, which at this time are only:

  - [SQL Server .NET 4.5+](https://www.nuget.org/packages/ServiceStack.OrmLite.SqlServer)
  - [MySQL .NET 4.5+](https://www.nuget.org/packages/ServiceStack.OrmLite.MySql)

We've also added a 
[.NET 4.5 build for Sqlite](https://www.nuget.org/packages/ServiceStack.OrmLite.Sqlite.Mono) 
as it's a common use-case to swapout to use Sqlite's in-memory provider for faster tests. 
But as Sqlite doesn't provide async API's under-the-hood we fallback to *pseudo async* support where we just wrap its synchronous responses in `Task` results. 

# API Examples

OrmLite's SQL Expression support lets you use LINQ-liked querying in all our providers. 
To give you a flavour here are some examples with their partial SQL output (using SqlServer dialect): 

### Querying with SELECT

```csharp
int agesAgo = DateTime.Today.AddYears(-20).Year;
db.Select<Author>(q => q.Birthday >= new DateTime(agesAgo, 1, 1) 
                    && q.Birthday <= new DateTime(agesAgo, 12, 31));
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

### Convenient common usage data access patterns 

OrmLite also includes a number of convenient API's providing DRY, typed data access for common queries:

```csharp
Person personById = db.SingleById<Person>(1);
```

**SELECT "Id", "FirstName", "LastName", "Age" FROM "Person" WHERE "Id" = @Id**

```csharp
Person personByAge = db.Single<Person>(x => x.Age == 42);
```

**SELECT TOP 1 "Id", "FirstName", "LastName", "Age"  FROM "Person" WHERE ("Age" = 42)**

```csharp
int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
```

**SELECT Max("Age") FROM "Person" WHERE ("Age" < 50)**

```csharp
int peopleOver40 = db.Scalar<int>(
    db.From<Person>().Select(Sql.Count("*")).Where(q => q.Age > 40));
```

**SELECT COUNT(*) FROM "Person" WHERE ("Age" > 40)**

```csharp
int peopleUnder50 = db.Count<Person>(x => x.Age < 50);
```

**SELECT COUNT(*) FROM "Person" WHERE ("Age" < 50)**

```csharp
bool has42YearOlds = db.Exists<Person>(new { Age = 42 });
```

**WHERE "Age" = @Age**

```csharp
List<string> results = db.Column<string>(db.From<Person>().Select(x => x.LastName)
                         .Where(q => q.Age == 27));
```

**SELECT "LastName" FROM "Person" WHERE ("Age" = 27)**

```csharp
HashSet<int> results = db.ColumnDistinct<int>(db.From<Person>().Select(x => x.Age)
                         .Where(q => q.Age < 50));
```

**SELECT "Age" FROM "Person" WHERE ("Age" < 50)**

```csharp
Dictionary<int,string> results = db.Dictionary<int, string>(
    db.From<Person>().Select(x => new { x.Id, x.LastName }).Where(x => x.Age < 50));
```

**SELECT "Id","LastName" FROM "Person" WHERE ("Age" < 50)**


```csharp
Dictionary<int, List<string>> results = db.Lookup<int, string>(
    db.From<Person>().Select(x => new { x.Age, x.LastName }).Where(q => q.Age < 50));
```

**SELECT "Age","LastName" FROM "Person" WHERE ("Age" < 50)**

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
db.UpdateOnly(new Person { FirstName = "JJ", Age = 12 }, 
    onlyFields: p => new { p.FirstName, p.Age });
```
**UPDATE "Person" SET "FirstName" = 'JJ', "Age" = 12**

When present, the second expression is used as the where filter:
```csharp
db.UpdateOnly(new Person { FirstName = "JJ" }, 
    onlyFields: p => p.FirstName, 
    where: p => p.LastName == "Hendrix");
```
**UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("LastName" = 'Hendrix')**

Instead of using the expression filters above you can choose to use an ExpressionVisitor builder which provides more flexibility when you want to programatically construct the update statement:

```csharp
db.UpdateOnly(new Person { FirstName = "JJ", LastName = "Hendo" }, 
  onlyFields: q => q.Update(p => p.FirstName));
```
**UPDATE "Person" SET "FirstName" = 'JJ'**

```csharp
db.UpdateOnly(new Person { FirstName = "JJ" }, 
  onlyFields: q => q.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
```
**UPDATE "Person" SET "FirstName" = 'JJ' WHERE ("LastName" = 'Hendrix')**

For the ultimate flexibility we also provide un-typed, string-based expressions. Use the `.Params()` extension method escape parameters (inspired by [massive](https://github.com/robconery/massive)):

```csharp
db.Update<Person>(set: "FirstName = {0}".Params("JJ"), 
                where: "LastName = {0}".Params("Hendrix"));
```
Even the Table name can be a string so you perform the same update without requiring the Person model at all:

```csharp
db.Update(table: "Person", set: "FirstName = {0}".Params("JJ"), 
          where: "LastName = {0}".Params("Hendrix"));
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
db.InsertOnly(new Person { FirstName = "Amy" }, q => q.Insert(p => new {p.FirstName}))
```
**INSERT INTO "Person" ("FirstName") VALUES ('Amy')**

### DELETE

Like updates for DELETE's we also provide APIs that take a where Expression:
```csharp
db.Delete<Person>(p => p.Age == 27);
```

Or an Expression Visitor:
```csharp
db.Delete<Person>(q => q.Where(p => p.Age == 27));
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

# API Overview

The API is minimal, providing basic shortcuts for the primitive SQL statements:

[![OrmLite API](https://raw.githubusercontent.com/ServiceStack/Assets/master/img/ormlite/OrmLiteApi.png)](https://raw.githubusercontent.com/ServiceStack/Assets/master/img/ormlite/OrmLiteApi.png)

### Notes

OrmLite Extension methods hang off ADO.NET's `IDbConnection`.

`CreateTable<T>` and `DropTable<T>` create and drop tables based on a classes type definition (only public properties used).

By default the Select API's use parameterized SQL whilst any selection methods ending with **Fmt** allow you to construct Sql using C# `string.Format()` syntax.

If your SQL doesn't start with a **SELECT** statement, it is assumed a WHERE clause is being provided, e.g:

```csharp
var tracks = db.SelectFmt<Track>("Artist = {0} AND Album = {1}", 
  "Nirvana", 
  "Heart Shaped Box");
```

Which is equivalent to:

```csharp
var tracks = db.SelectFmt<Track>("SELECT * FROM track WHERE Artist={0} AND Album={1}", 
    "Nirvana", 
    "Heart Shaped Box");
```

**Select** returns multiple records: 

```csharp
List<Track> tracks = db.Select<Track>()
```

**Single** returns a single record:

```csharp
Track track = db.Single<Track>(q => q.RefId == refId)
```

**Dictionary** returns a Dictionary made from the first two columns:

```csharp
Dictionary<int, string> trackIdNamesMap = db.Dictionary<int, string>(
    "select Id, Name from Track")
```

**Lookup** returns an `Dictionary<K, List<V>>` made from the first two columns:

```csharp
Dictionary<int, List<string>> albumTrackNames = db.Lookup<int, string>(
    "select AlbumId, Name from Track")
```

**Column** returns a List of first column values:

```csharp
List<string> trackNames = db.Column<string>("select Name from Track")
```

**HashSet** returns a HashSet of distinct first column values:

```csharp    
HashSet<string> uniqueTrackNames = db.ColumnDistinct<string>("select Name from Track")
```

**Scalar** returns a single scalar value:

```csharp
var trackCount = db.Scalar<int>("select count(*) from Track")
```

Anonymous types passed into **Where** are treated like an **AND** filter:

```csharp
var track3 = db.Where<Track>(new { AlbumName = "Throwing Copper", TrackNo = 3 })
```

**Select** statements take in parameterized SQL using properties from the supplied anonymous type (if any):

```csharp
var track3 = db.Select<Track>(
  "select * from Track Where AlbumName = @album and TrackNo = @trackNo", 
  new { album = "Throwing Copper", trackNo = 3 })
```

SingleById(s), SelectById(s), etc provide strong-typed convenience methods to fetch by a Table's **Id** primary key field.

```csharp
var track = db.SingleById<Track>(1);
var tracks = db.SelectByIds<Track>(new[]{ 1,2,3 });
```

### Other Notes

 - All **Insert**, **Update**, and **Delete** methods take multiple params, while `InsertAll`, `UpdateAll` and `DeleteAll` take IEnumerables.
 - `Save` and `SaveAll` will Insert if no record with **Id** exists, otherwise it Updates. 
 - Methods containing the word **Each** return an IEnumerable<T> and are lazily loaded (i.e. non-buffered).


# Features

Whilst OrmLite aims to provide a light-weight typed wrapper around SQL, it offers a number of convenient features that makes working with RDBMS's a clean and enjoyable experience:

## Typed SqlExpression support for JOIN's

Starting with the most basic example you can simply specify the table you want to join with:

```csharp
var dbCustomers = db.Select<Customer>(q => q.Join<CustomerAddress>());
```

This query rougly maps to the following SQL:

```sql
SELECT Customer.* 
  FROM Customer 
       INNER JOIN 
       CustomerAddress ON (Customer.Id == CustomerAddress.Id)
```

Just like before `q` is an instance of `SqlExpression<Customer>` which is bounded to the base `Customer` type (and what any subsequent implicit API's apply to). 

To better illustrate the above query, lets expand it to the equivalent explicit query:

```csharp
SqlExpression<Customer> q = db.From<Customer>();
q.Join<Customer,CustomerAddress>((cust,address) => cust.Id == address.CustomerId);

List<Customer> dbCustomers = db.Select(q);
```

### Reference Conventions

The above query implicitly joins together the `Customer` and `CustomerAddress` POCO's using the same `{ParentType}Id` property convention used in [OrmLite's support for References](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/LoadReferencesTests.cs), e.g:

```csharp
class Customer {
    public int Id { get; set; }
    ...
}
class CustomerAddress {
    public int Id { get; set; }
    public int CustomerId { get; set; }  // Reference based on Property name convention
}
```

References based on matching alias names is also supported, e.g:

```csharp
[Alias("LegacyCustomer")]
class Customer {
    public int Id { get; set; }
    ...
}
class CustomerAddress {
    public int Id { get; set; }

    [Alias("LegacyCustomerId")]             // Matches `LegacyCustomer` Alias
    public int RenamedCustomerId { get; set; }  // Reference based on Alias Convention
}
```

### Self References

Self References are also supported for **1:1** relations where the Foreign Key can instead be on the parent table:

```csharp
public class Customer
{
    ...
    public int CustomerAddressId { get; set; }

    [Reference]
    public CustomerAddress PrimaryAddress { get; set; }
}
```

### Foreign Key and References Attributes

References that don't follow the above naming conventions can be declared explicitly using
the `[References]` and `[ForeignKey]` attributes:

```csharp
public class Customer
{
    [References(typeof(CustomerAddress))]
    public int PrimaryAddressId { get; set; }

    [Reference]
    public CustomerAddress PrimaryAddress { get; set; }
}
```

> Reference Attributes take precedence over naming conventions

### Multiple Self References

The example below shows a customer with multiple `CustomerAddress` references which are able to be matched with 
the `{PropertyReference}Id` naming convention, e.g:

```csharp
public class Customer
{
    [AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }

    [References(typeof(CustomerAddress))]
    public int? HomeAddressId { get; set; }

    [References(typeof(CustomerAddress))]
    public int? WorkAddressId { get; set; }

    [Reference]
    public CustomerAddress HomeAddress { get; set; }

    [Reference]
    public CustomerAddress WorkAddress { get; set; }
}
```

Once defined, it can be saved and loaded via OrmLite's normal Reference and Select API's, e.g:

```csharp
var customer = new Customer
{
    Name = "The Customer",
    HomeAddress = new CustomerAddress {
        Address = "1 Home Street",
        Country = "US"
    },
    WorkAddress = new CustomerAddress {
        Address = "2 Work Road",
        Country = "UK"
    },
};

db.Save(customer, references:true);

var c = db.LoadSelect<Customer>(q => q.Name == "The Customer");
c.WorkAddress.Address.Print(); // 2 Work Road

var ukAddress = db.Single<CustomerAddress>(q => q.Country == "UK");
ukAddress.Address.Print();     // 2 Work Road
```

### Implicit Reference Conventions are applied by default

The implicit relationship above allows you to use any of these equilvalent APIs to JOIN tables:

```csharp
q.Join<CustomerAddress>();
q.Join<Customer,CustomerAddress>();
q.Join<Customer,CustomerAddress>((cust,address) => cust.Id == address.CustomerId);
```

### Selecting multiple columns across joined tables

Another implicit behaviour when selecting from a typed SqlExpression is that results are mapped to the `Customer` POCO. To change this default we just need to explicitly specify what POCO it should map to instead:

```csharp
List<FullCustomerInfo> customers = db.Select<FullCustomerInfo>(
    db.From<Customer>().Join<CustomerAddress>());
```

Where `FullCustomerInfo` is any POCO that contains a combination of properties matching any of the joined tables in the query. 

The above example is also equivalent to the shorthand `db.Select<Into,From>()` API:

```csharp
var customers = db.Select<FullCustomerInfo,Customer>(q => q.Join<CustomerAddress>());
```

Rules for how results are mapped is simply each property on `FullCustomerInfo` is mapped to the first matching property in any of the tables in the order they were added to the SqlExpression.

The mapping also includes a fallback for referencing fully-qualified names in the format: `{TableName}{FieldName}` allowing you to reference ambiguous fields, e.g:

  - `CustomerId` => "Customer"."Id"
  - `OrderId` => "Order"."Id"
  - `CustomerName` => "Customer"."Name"
  - `OrderCost` => "Order"."Cost"

### Advanced Example

Seeing how the SqlExpression is constructed, joined and mapped, we can take a look at a more advanced example to showcase more of the new API's available:

```csharp
List<FullCustomerInfo> rows = db.Select<FullCustomerInfo>(  // Map results to FullCustomerInfo POCO
  db.From<Customer>()                                       // Create typed Customer SqlExpression
    .LeftJoin<CustomerAddress>()                            // Implict left join with base table
    .Join<Customer, Order>((c,o) => c.Id == o.CustomerId)   // Explicit join and condition
    .Where(c => c.Name == "Customer 1")                     // Implicit condition on base table
    .And<Order>(o => o.Cost < 2)                            // Explicit condition on joined Table
    .Or<Customer,Order>((c,o) => c.Name == o.LineItem));    // Explicit condition with joined Tables
```

The comments next to each line document each Type of API used. Some of the new API's introduced in this example include:

  - Usage of `LeftJoin` for specifying a LEFT JOIN, `RightJoin` and `FullJoin` also available
  - Usage of `And<Table>()`, to specify an **AND** condition on a Joined table 
  - Usage of `Or<Table1,Table2>`, to specify an **OR** condition against 2 joined tables

More code examples of References and Joined tables are available in:

  - [LoadReferencesTests.cs](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/LoadReferencesTests.cs)
  - [LoadReferencesJoinTests.cs](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/LoadReferencesJoinTests.cs)

### Reference Support, POCO style

OrmLite lets you Store and Load related entities in separate tables using `[Reference]` attributes in primary tables in conjunction with `{Parent}Id` property convention in child tables, e.g: 

```csharp
public class Customer
{
    [AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }

    [Reference] // Save in CustomerAddress table
    public CustomerAddress PrimaryAddress { get; set; }

    [Reference] // Save in Order table
    public List<Order> Orders { get; set; }
}

public class CustomerAddress
{
    [AutoIncrement]
    public int Id { get; set; }
    public int CustomerId { get; set; } //`{Parent}Id` convention to refer to Customer
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
}

public class Order
{
    [AutoIncrement]
    public int Id { get; set; }
    public int CustomerId { get; set; } //`{Parent}Id` convention to refer to Customer
    public string LineItem { get; set; }
    public int Qty { get; set; }
    public decimal Cost { get; set; }
}
```

With the above structure you can save a POCO and all its entity references with `db.Save(T,references:true)`, e.g:

```csharp
var customer =  new Customer {
    Name = "Customer 1",
    PrimaryAddress = new CustomerAddress {
        AddressLine1 = "1 Australia Street",
        Country = "Australia"
    },
    Orders = new[] {
        new Order { LineItem = "Line 1", Qty = 1, Cost = 1.99m },
        new Order { LineItem = "Line 2", Qty = 2, Cost = 2.99m },
    }.ToList(),
};

db.Save(customer, references:true);
```

This saves the root customer POCO in the `Customer` table, its related PrimaryAddress in the `CustomerAddress` table and its 2 Orders in the `Order` table.

### Querying POCO's with References

The `Load*` API's are used to automatically load a POCO and all it's child references, e.g:

```csharp
var customer = db.LoadSingleById<Customer>(customerId);
```

Using Typed SqlExpressions:

```csharp
var customers = db.LoadSelect<Customer>(q => q.Name == "Customer 1");
```

More examples available in [LoadReferencesTests.cs](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/LoadReferencesTests.cs)

Unlike normal complex properties, references:

  - Doesn't persist as complex type blobs
  - Doesn't impact normal querying
  - Saves and loads references independently from itself
  - Are serializable with Text serializers (only populated are visible).
  - Loads related data only 1-reference-level deep
 
Basically they provides a better story when dealing with referential data that doesn't impact the POCO's ability to be used as DTO's. 

## Optimistic Concurrency

Optimistic concurrency can be added to any table by adding the `ulong RowVersion { get; set; }` property, e.g:

```csharp
public class Poco
{
    ...
    public ulong RowVersion { get; set; }
}
```

RowVersion is implemented efficiently in all major RDBMS's, i.e:

 - Uses `rowversion` datatype in SqlServer 
 - Uses PostgreSql's `xmin` system column (no column on table required)
 - Uses UPDATE triggers on MySql, Sqlite and Oracle whose lifetime is attached to Create/Drop tables APIs

Despite their differing implementations each provider works the same way where the `RowVersion` property is populated when the record is selected and only updates the record if the RowVersion matches with what's in the database, e.g:

```csharp
var rowId = db.Insert(new Poco { Text = "Text" }, selectIdentity:true);

var row = db.SingleById<Poco>(rowId);
row.Text += " Updated";
db.Update(row); //success!

row.Text += "Attempting to update stale record";

//Can't update stale record
Assert.Throws<OptimisticConcurrencyException>(() =>
    db.Update(row));

//Can update latest version
var updatedRow = db.SingleById<Poco>(rowId);  // fresh version
updatedRow.Text += "Update Success!";
db.Update(updatedRow);

updatedRow = db.SingleById<Poco>(rowId);
db.Delete(updatedRow);                        // can delete fresh version
```

Optimistic concurrency is only verified on API's that update or delete an entire entity, i.e. it's not enforced in partial updates. There's also an Alternative API available for DELETE's:

```csharp
db.DeleteById<Poco>(id:updatedRow.Id, rowversion:updatedRow.RowVersion)
```

### Exec, Result and String Filters

OrmLite's core Exec filters makes it possible to inject your own behavior, tracing, profiling, etc.

It's useful in situations like wanting to use SqlServer in production but use an `in-memory` Sqlite database in tests and being able to emulate any missing SQL Server Stored Procedures in code:

```csharp
public class MockStoredProcExecFilter : OrmLiteExecFilter
{
    public override T Exec<T>(IDbConnection dbConn, Func<IDbCommand, T> filter)
    {
        try
        {
            return base.Exec(dbConn, filter);
        }
        catch (Exception ex)
        {
            if (dbConn.GetLastSql() == "exec sp_name @firstName, @age")
                return (T)(object)new Person { FirstName = "Mocked" };
            throw;
        }
    }
}

OrmLiteConfig.ExecFilter = new MockStoredProcExecFilter();

using (var db = OpenDbConnection())
{
    var person = db.SqlScalar<Person>("exec sp_name @firstName, @age",
        new { firstName = "aName", age = 1 });

    person.FirstName.Print(); //Mocked
}
```

Results filters makes it trivial to implement the `CaptureSqlFilter` which allows you to capture SQL Statements without running them, e.g:

```csharp
public class CaptureSqlFilter : OrmLiteResultsFilter
{
    public CaptureSqlFilter()
    {
        SqlFilter = CaptureSql;
        SqlStatements = new List<string>();
    }

    private void CaptureSql(string sql)
    {
        SqlStatements.Add(sql);
    }

    public List<string> SqlStatements { get; set; }
}
```

Which can be used to wrap around existing database calls to capture, defer or print generated SQL, e.g:

```csharp
using (var captured = new CaptureSqlFilter())
using (var db = OpenDbConnection())
{
    db.CreateTable<Person>();
    db.Count<Person>(x => x.Age < 50);
    db.Insert(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix" });
    db.Delete<Person>(new { FirstName = "Jimi", Age = 27 });

    var sql = string.Join(";\n", captured.SqlStatements.ToArray());
    sql.Print();
}
```

### Replay Exec Filter

Or if you want to do things like executing each operation multiple times, e.g:

```csharp
public class ReplayOrmLiteExecFilter : OrmLiteExecFilter
{
    public int ReplayTimes { get; set; }

    public override T Exec<T>(IDbConnection dbConn, Func<IDbCommand, T> filter)
    {
        var holdProvider = OrmLiteConfig.DialectProvider;
        var dbCmd = CreateCommand(dbConn);
        try
        {
            var ret = default(T);
            for (var i = 0; i < ReplayTimes; i++)
            {
                ret = filter(dbCmd);
            }
            return ret;
        }
        finally
        {
            DisposeCommand(dbCmd);
            OrmLiteConfig.DialectProvider = holdProvider;
        }
    }
}

OrmLiteConfig.ExecFilter = new ReplayOrmLiteExecFilter { ReplayTimes = 3 };

using (var db = OpenDbConnection())
{
    db.DropAndCreateTable<PocoTable>();
    db.Insert(new PocoTable { Name = "Multiplicity" });

    var rowsInserted = db.Count<PocoTable>(q => q.Name == "Multiplicity"); //3
}
```


## Mockable extension methods

The Result Filters also lets you easily mock results and avoid hitting the database, typically useful in Unit Testing Services to mock OrmLite API's directly instead of using a repository, e.g:

```csharp
using (new OrmLiteResultsFilter {
    PrintSql = true,
    SingleResult = new Person { 
      Id = 1, FirstName = "Mocked", LastName = "Person", Age = 100 
    },
})
{
    db.Single<Person>(x => x.Age == 42).FirstName // Mocked
    db.Single(db.From<Person>().Where(x => x.Age == 42)).FirstName // Mocked
    db.Single<Person>(new { Age = 42 }).FirstName // Mocked
    db.Single<Person>("Age = @age", new { age = 42 }).FirstName // Mocked
}
```

More examples showing how to mock different API's including support for nesting available in [MockAllApiTests.cs](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/MockAllApiTests.cs)

### String Filter

There's also a specific filter for strings available which allows you to apply custom sanitization on String fields, e.g. you can ensure all strings are right trimmed with:

```csharp
OrmLiteConfig.StringFilter = s => s.TrimEnd();

db.Insert(new Poco { Name = "Value with trailing   " });
db.Select<Poco>().First().Name // "Value with trailing"
```

## Pluggable Complex Type Serializers

Pluggable serialization lets you specify different serialization strategies of Complex Types for each available RDBMS provider, e.g:

```csharp
//ServiceStack's JSON and JSV Format
SqliteDialect.Provider.StringSerializer = new JsvStringSerializer();       
PostgreSqlDialect.Provider.StringSerializer = new JsonStringSerializer();
//.NET's XML and JSON DataContract serializers
SqlServerDialect.Provider.StringSerializer = new DataContractSerializer();
MySqlDialect.Provider.StringSerializer = new JsonDataContractSerializer();
//.NET XmlSerializer
OracleDialect.Provider.StringSerializer = new XmlSerializableSerializer();
```
You can also provide a custom serialization strategy by implementing 
[IStringSerializer](https://github.com/ServiceStack/ServiceStack.Text/blob/master/src/ServiceStack.Text/IStringSerializer.cs).

By default all dialects use the existing `JsvStringSerializer`, except for PostgreSQL which due to its built-in support for JSON, uses the JSON format by default.  

## Global Insert / Update Filters

Similar to interceptors in some heavy ORM's, Insert and Update filters get fired just before any **INSERT** or **UPDATE** operation using OrmLite's typed API's (i.e. not dynamic SQL or partial updates using anon types). This functionality can be used for easily auto-maintaining Audit information for your POCO data models, e.g:

```csharp
public interface IAudit 
{
    DateTime CreatedDate { get; set; }
    DateTime ModifiedDate { get; set; }
    string ModifiedBy { get; set; }
}

OrmLiteConfig.InsertFilter = (dbCmd, row) => {
    var auditRow = row as IAudit;
    if (auditRow != null)
        auditRow.CreatedDate = auditRow.ModifiedDate = DateTime.UtcNow;
};

OrmLiteConfig.UpdateFilter = (dbCmd, row) => {
    var auditRow = row as IAudit;
    if (auditRow != null)
        auditRow.ModifiedDate = DateTime.UtcNow;
};
```

Which will ensure that the `CreatedDate` and `ModifiedDate` fields are populated on every insert and update.

### Validation Example

The filters can also be used for validation where throwing an exception will prevent the operation and bubble the exception, e.g:

```csharp
OrmLiteConfig.InsertFilter = OrmLiteConfig.UpdateFilter = (dbCmd, row) => {
    var auditRow = row as IAudit;
    if (auditRow != null && auditRow.ModifiedBy == null)
        throw new ArgumentNullException("ModifiedBy");
};

try
{
    db.Insert(new AuditTable());
}
catch (ArgumentNullException) {
   //throws ArgumentNullException
}

db.Insert(new AuditTable { ModifiedBy = "Me!" }); //succeeds
```

## Custom SQL Customizations

A number of new hooks are available to provide more flexibility when creating and dropping your RDBMS tables.

### Custom Field Declarations

The `[CustomField]` attribute can be used for specifying custom field declarations in the generated Create table DDL statements, e.g:

```csharp
public class PocoTable
{
    public int Id { get; set; }

    [CustomField("CHAR(20)")]
    public string CharColumn { get; set; }

    [CustomField("DECIMAL(18,4)")]
    public decimal? DecimalColumn { get; set; }
}

db.CreateTable<PocoTable>(); 
```

Generates and executes the following SQL:

```sql
CREATE TABLE "PocoTable" 
(
  "Id" INTEGER PRIMARY KEY, 
  "CharColumn" CHAR(20) NULL, 
  "DecimalColumn" DECIMAL(18,4) NULL 
);  
```

#### Pre / Post Custom SQL Hooks when Creating and Dropping tables 

Pre / Post Custom SQL Hooks allow you to inject custom SQL before and after tables are created or dropped, e.g:

```csharp
[PostCreateTable("INSERT INTO TableWithSeedData (Name) VALUES ('Foo');" +
                 "INSERT INTO TableWithSeedData (Name) VALUES ('Bar');")]
public class TableWithSeedData
{
    [AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
}
```

Which like other ServiceStack attributes, can also be added dynamically, e.g:

```csharp
typeof(TableWithSeedData)
    .AddAttributes(new PostCreateTableAttribute(
        "INSERT INTO TableWithSeedData (Name) VALUES ('Foo');" +
        "INSERT INTO TableWithSeedData (Name) VALUES ('Bar');"));
```

Custom SQL Hooks also allow executing custom SQL before and after a table has been created or dropped, i.e:

```csharp
[PreCreateTable(runSqlBeforeTableCreated)]
[PostCreateTable(runSqlAfterTableCreated)]
[PreDropTable(runSqlBeforeTableDropped)]
[PostDropTable(runSqlAfterTableDropped)]
public class Table {}
```

### Untyped API support

The [IUntypedApi](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/src/ServiceStack.OrmLite/IUntypedApi.cs) interface is useful for when you only have access to a late-bound object runtime type which is accessible via `db.CreateTypedApi`, e.g:

```csharp
public class BaseClass
{
    public int Id { get; set; }
}

public class Target : BaseClass
{
    public string Name { get; set; }
}

var row = (BaseClass)new Target { Id = 1, Name = "Foo" };

var useType = row.GetType();
var typedApi = db.CreateTypedApi(useType);

db.DropAndCreateTables(useType);

typedApi.Save(row);

var typedRow = db.SingleById<Target>(1);
typedRow.Name //= Foo

var updateRow = (BaseClass)new Target { Id = 1, Name = "Bar" };

typedApi.Update(updateRow);

typedRow = db.SingleById<Target>(1);
typedRow.Name //= Bar

typedApi.Delete(typedRow, new { Id = 1 });

typedRow = db.SingleById<Target>(1); //= null
```

## T4 Template Support

[OrmLite's T4 Template](https://github.com/ServiceStack/ServiceStack.OrmLite/tree/master/src/T4) 
are useful in database-first development or when wanting to use OrmLite with an existing
RDBMS by automatically generating POCO's and strong-typed wrappers 
for executing stored procedures. 

OrmLite's T4 support can be added via NuGet with:

    PM> Install-Package ServiceStack.OrmLite.T4

## Typed SqlExpressions with Custom SQL APIs

The Custom SQL API's allow you to map custom SqlExpressions into different responses:

```csharp
List<Person> results = db.SqlList<Person>(
    db.From<Person>().Select("*").Where(q => q.Age < 50));
List<Person> results = db.SqlList<Person>(
    "SELECT * FROM Person WHERE Age < @age", new { age=50});

List<string> results = db.SqlColumn<string>(db.From<Person>().Select(x => x.LastName));
List<string> results = db.SqlColumn<string>("SELECT LastName FROM Person");

HashSet<int> results = db.ColumnDistinct<int>(db.From<Person>().Select(x => x.Age));
HashSet<int> results = db.ColumnDistinct<int>("SELECT Age FROM Person");

int result = db.SqlScalar<int>(
    db.From<Person>().Select(Sql.Count("*")).Where(q => q.Age < 50));
int result = db.SqlScalar<int>("SELCT COUNT(*) FROM Person WHERE Age < 50");
```

## Stored Procedures using Custom Raw SQL API's

The Raw SQL API's provide a convenient way for mapping results of any Custom SQL like
executing Stored Procedures:

```csharp
List<Poco> results = db.SqlList<Poco>("EXEC GetAnalyticsForWeek 1");
List<Poco> results = db.SqlList<Poco>(
    "EXEC GetAnalyticsForWeek @weekNo", new { weekNo = 1 });

List<int> results = db.SqlList<int>("EXEC GetTotalsForWeek 1");
List<int> results = db.SqlList<int>(
    "EXEC GetTotalsForWeek @weekNo", new { weekNo = 1 });

int result = db.SqlScalar<int>("SELECT 10");
```

### Stored Procedures with output params

The `SqlProc` API provides even greater customization by letting you modify the underlying
ADO.NET Stored Procedure call by returning a prepared `IDbCommand` allowing for 
advanced customization like setting and retriving OUT parameters, e.g:

```csharp
string spSql = @"DROP PROCEDURE IF EXISTS spSearchLetters;
    CREATE PROCEDURE spSearchLetters (IN pLetter varchar(10), OUT pTotal int)
    BEGIN
        SELECT COUNT(*) FROM LetterFrequency WHERE Letter = pLetter INTO pTotal;
        SELECT * FROM LetterFrequency WHERE Letter = pLetter;
    END";

db.ExecuteSql(spSql);

using (var cmd = db.SqlProc("spSearchLetters", new { pLetter = "C" }))
{
    var pTotal = cmd.AddParam("pTotal", direction: ParameterDirection.Output);

    var results = cmd.ConvertToList<LetterFrequency>();
    var total = pTotal.Value;
}
```

An alternative approach is to use `SqlList` which lets you use a filter to customize a 
Stored Procedure or any other command type, e.g:

```csharp
IDbDataParameter pTotal = null;
var results = db.SqlList<LetterFrequency>("spSearchLetters", cmd => {
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.AddParam("pLetter", "C");
        pTotal = cmd.AddParam("pTotal", direction: ParameterDirection.Output);
    });
var total = pTotal.Value;
```

More examples can be found in [SqlServerProviderTests](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/SqlServerProviderTests.cs).

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

![Data dump of Robot Shard #1](http://mono.servicestack.net/files/robots-shard0.png)

As expected each shard has every 10th robot inside.

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
using (IDbConnection db = Config.OpenDbConnection())
{
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

  var customer = new Customer {
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
          { AddressType.Work, new Address { 
            Line1 = "1 Street", Country = "US", State = "NY", City = "New York", ZipCode = "10101" } 
          },
      },
      CreatedAt = DateTime.UtcNow,
  };

  var customerId = db.Insert(customer, selectIdentity: true); //Get Auto Inserted Id
  customer = db.Single<Customer>(new { customer.Email }); //Query
  Assert.That(customer.Id, Is.EqualTo(customerId));

  //Direct access to System.Data.Transactions:
  using (IDbTransaction trans = db.OpenTransaction(IsolationLevel.ReadCommitted))
  {
      var order = new Order {
          CustomerId = customer.Id,
          EmployeeId = 1,
          OrderDate = DateTime.UtcNow,
          Freight = 10.50m,
          ShippingAddress = new Address { 
            Line1 = "3 Street", Country = "US", State = "NY", City = "New York", ZipCode = "12121" },
      };
      db.Save(order); //Inserts 1st time

      //order.Id populated on Save().

      var orderDetails = new[] {
          new OrderDetail {
              OrderId = order.Id,
              ProductId = product1.Id,
              Quantity = 2,
              UnitPrice = product1.UnitPrice,
          },
          new OrderDetail {
              OrderId = order.Id,
              ProductId = product2.Id,
              Quantity = 2,
              UnitPrice = product2.UnitPrice,
              Discount = .15m,
          }
      };

      db.Save(orderDetails);

      order.Total = orderDetails.Sum(x => x.UnitPrice * x.Quantity * x.Discount) + order.Freight;

      db.Save(order); //Updates 2nd Time

      trans.Commit();
  }
}
```

Running this against a SQL Server database will yield the results below:

[![SQL Server Management Studio results](http://mono.servicestack.net/files/ormlite-example.png)](http://www.servicestack.net/files/ormlite-example.png)

Notice the POCO types are stored in the [very fast](http://mono.servicestack.net/mythz_blog/?p=176) 
and [Versatile](http://mono.servicestack.net/mythz_blog/?p=314) 
[JSV Format](https://github.com/ServiceStack/ServiceStack.Text/wiki/JSV-Format) which although hard to do - 
is actually more compact, human and parser-friendly than JSON :)

### Ignoring DTO Properties

You may use the `[Ignore]` attribute to denote DTO properties that are not fields in the table. This will force the SQL generation to ignore that property.

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
    db.CreateTable<ShipperType>();
    db.CreateTable<Shipper>();
}

/* In debug mode the line above prints:
DEBUG: CREATE TABLE "ShipperTypes" 
(
  "ShipperTypeID" INTEGER PRIMARY KEY AUTOINCREMENT, 
  "Name" VARCHAR(40) NOT NULL 
);
DEBUG: CREATE UNIQUE INDEX uidx_shippertypes_name ON "ShipperTypes" ("Name" ASC);
DEBUG: CREATE TABLE "Shippers" 
(
  "ShipperID" INTEGER PRIMARY KEY AUTOINCREMENT, 
  "CompanyName" VARCHAR(40) NOT NULL, 
  "Phone" VARCHAR(24) NULL, 
  "ShipperTypeId" INTEGER NOT NULL, 

  CONSTRAINT "FK_Shippers_ShipperTypes" FOREIGN KEY ("ShipperTypeId") REFERENCES "ShipperTypes" ("ShipperID") 
);
DEBUG: CREATE UNIQUE INDEX uidx_shippers_companyname ON "Shippers" ("CompanyName" ASC);
*/
```

### Transaction Support
As we have direct access to IDbCommand and friends - playing with transactions is easy:

```csharp
  var trainsType = new ShipperType { Name = "Trains" };
  var planesType = new ShipperType { Name = "Planes" };

  //Playing with transactions
  using (IDbTransaction dbTrans = db.OpenTransaction())
  {
      db.Save(trainsType);
      db.Save(planesType);

      dbTrans.Commit();
  }

  using (IDbTransaction dbTrans = db.OpenTransaction(IsolationLevel.ReadCommitted))
  {
      db.Insert(new ShipperType { Name = "Automobiles" });
      Assert.That(db.Select<ShipperType>(), Has.Count.EqualTo(3));
  }
	Assert.That(db.Select<ShipperType>(), Has.Count(2));
```

### CRUD Operations 
No ORM is complete without the standard crud operations:

```csharp
	//Performing standard Insert's and Selects
  db.Insert(new Shipper { CompanyName = "Trains R Us", Phone = "555-TRAINS", ShipperTypeId = trainsType.Id });
  db.Insert(new Shipper { CompanyName = "Planes R Us", Phone = "555-PLANES", ShipperTypeId = planesType.Id });
  db.Insert(new Shipper { CompanyName = "We do everything!", Phone = "555-UNICORNS", ShipperTypeId = planesType.Id });

  var trainsAreUs = db.SingleFmt<Shipper>("ShipperTypeId = {0}", trainsType.Id);
  Assert.That(trainsAreUs.CompanyName, Is.EqualTo("Trains R Us"));
  Assert.That(db.SelectFmt<Shipper>("CompanyName = {0} OR Phone = {1}", "Trains R Us", "555-UNICORNS"), Has.Count.EqualTo(2));
  Assert.That(db.SelectFmt<Shipper>("ShipperTypeId = {0}", planesType.Id), Has.Count.EqualTo(2));

  //Lets update a record
  trainsAreUs.Phone = "666-TRAINS";
  db.Update(trainsAreUs);
          Assert.That(db.SingleById<Shipper>(trainsAreUs.Id).Phone, Is.EqualTo("666-TRAINS"));
  
  //Then make it dissappear
  db.Delete(trainsAreUs);
          Assert.That(db.SingleById<Shipper>(trainsAreUs.Id), Is.Null);

  //And bring it back again
  db.Insert(trainsAreUs);
```

### Performing custom queries 
And with access to raw sql when you need it - the database is your oyster :)

```csharp
    var partialColumns = db.SelectFmt<SubsetOfShipper>(typeof (Shipper), "ShipperTypeId = {0}", planesType.Id);
    Assert.That(partialColumns, Has.Count.EqualTo(2));

    //Select into another POCO class that matches sql
    var rows = db.SelectFmt<ShipperTypeCount>(
      "SELECT ShipperTypeId, COUNT(*) AS Total FROM Shippers GROUP BY ShipperTypeId ORDER BY COUNT(*)");

    Assert.That(rows, Has.Count.EqualTo(2));
    Assert.That(rows[0].ShipperTypeId, Is.EqualTo(trainsType.Id));
    Assert.That(rows[0].Total, Is.EqualTo(1));
    Assert.That(rows[1].ShipperTypeId, Is.EqualTo(planesType.Id));
    Assert.That(rows[1].Total, Is.EqualTo(2));


    //And finally lets quickly clean up the mess we've made:
    db.DeleteAll<Shipper>();
    db.DeleteAll<ShipperType>();

    Assert.That(db.Select<Shipper>(), Has.Count.EqualTo(0));
    Assert.That(db.Select<ShipperType>(), Has.Count.EqualTo(0));
```

# Limitations 

For simplicity, and to be able to have the same POCO class persisted in db4o, memcached, redis or on the filesystem (i.e. providers included in ServiceStack), each model must have a single primary key, by convention OrmLite expects it
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

## Oracle Provider Notes

The Oracle provider requires an installation of Oracle's ODP.NET. It has been tested with Oracle 11g but should work with 10g and perhaps even older versions. It has not been tested with Oracle 12c and does not support any new 12c features such as AutoIncrement keys. It also does not support the new Oracle fully-managed client.

By default the Oracle provider stores Guids in the database as character strings and when generating SQL it quotes only table and column names that are reserved words in Oracle. That requires that you use the same quoting if you code your own SQL. Both of these options can be overridden, but overriding them will cause problems: the provider can store Guids as raw(16) but it cannot read them.

The Oracle provider uses Oracle sequences to implement AutoIncrement columns and it queries the sequence to get a new value in a separate database call. You can override the automatically generated sequence name with a

  [Sequence("name")]

attribute on a field. The Sequence attribute implies [AutoIncrement], but you can use both on the same field.

Since Oracle has a very restrictive 30 character limit on names, it is strongly suggested that you use short entity class and field names or aliases, remembering that indexes and foreign keys get compound names. If you use long names, the provider will squash them to make them compliant with the restriction. The algorithm used is to remove all vowels ("aeiouy") and if still too long then every fourth letter starting with the third one and finally if still too long to truncate the name. You must apply the same squashing algorithm if you are coding your own SQL.  

The previous version of ServiceStack.OrmLite.Oracle used System.Data.OracleClient to talk to the database. Microsoft has deprecated that client, but it does still mostly work if you construct the Oracle provider like this:

    OracleOrmLiteDialectProvider.Instance = new OracleOrmLiteDialectProvider(
    compactGuid: false,
    quoteNames: false,
    clientProvider: OracleOrmLiteDialectProvider.MicrosoftProvider); 

DateTimeOffset fields and, in locales that use a comma to separate the fractional part of a floating point number, some aspects of using floating point numbers, do not work with System.Data.OracleClient.


# Community Resources

  - [OrmLite and Redis: New alternatives for handling db communication](http://www.abtosoftware.com/blog/servicestack-ormlite-and-redis-new-alternatives-for-handling-db-communication) by [@abtosoftware](https://twitter.com/abtosoftware)
  - [Object Serialization as Step Towards Normalization](http://www.unpluggeddevelopment.com/post/85225892120/object-serialization-as-step-towards-normalization) by [@ 82unpluggd](https://twitter.com/82unpluggd)
  - [Creating a Data Access Layer using OrmLite](http://blogs.askcts.com/2014/05/07/getting-started-with-servicestack-part-2/) by [Lydon Bergin](http://blogs.askcts.com/)
  - [Code Generation using ServiceStack.OrmLite and T4 Text templates](http://jokecamp.wordpress.com/2013/09/07/code-generation-using-servicestack-ormlite-and-t4-text-templates/) by [@jokecamp](https://twitter.com/jokecamp)
  - [Simple ServiceStack OrmLite Example](http://www.curlette.com/?p=1068) by [@robrtc](https://twitter.com/robrtc)
  - [OrmLite Blobbing done with NHibernate and Serialized JSON](http://www.philliphaydon.com/2012/03/ormlite-blobbing-done-with-nhibernate-and-serialized-json/) by [@philliphaydon](https://twitter.com/philliphaydon)
  - [Creating An ASP.NET MVC Blog With ServiceStack.OrmLite](http://www.eggheadcafe.com/tutorials/asp-net/285cbe96-9922-406a-b193-3a0b40e31c40/creating-an-aspnet-mvc-blog-with-servicestackormlite.aspx) by [@peterbromberg](https://twitter.com/peterbromberg)


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
