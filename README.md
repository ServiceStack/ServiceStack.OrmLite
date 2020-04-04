Follow [@ServiceStack](https://twitter.com/servicestack) or [view the docs](https://docs.servicestack.net), use [StackOverflow](http://stackoverflow.com/questions/ask) or the [Customer Forums](https://forums.servicestack.net/) for support.

# Fast, Simple, Typed ORM for .NET

OrmLite's goal is to provide a convenient, DRY, config-free, RDBMS-agnostic typed wrapper that retains 
a high affinity with SQL, exposing intuitive APIs that generate predictable SQL and maps cleanly to 
(DTO-friendly) disconnected POCO's. This approach makes easier to reason-about your data access making 
it obvious what SQL is getting executed at what time, whilst mitigating unexpected behavior, 
implicit N+1 queries and leaky data access prevalent in Heavy ORMs.

OrmLite was designed with a focus on the core objectives:

  * Provide a set of light-weight C# extension methods around .NET's impl-agnostic `System.Data.*` interfaces
  * Map a POCO class 1:1 to an RDBMS table, cleanly by conventions, without any attributes required.
  * Create/Drop DB Table schemas using nothing but POCO class definitions (IOTW a true code-first ORM)
  * Simplicity - typed, wrist friendly API for common data access patterns.
  * High performance - with support for indexes, text blobs, etc.
    * Amongst the [fastest Micro ORMs](https://servicestackv3.github.io/Mono/src/Mono/benchmarks/default.htm) for .NET.
  * Expressive power and flexibility - with access to IDbCommand and raw SQL
  * Cross platform - supports multiple dbs (currently: Sql Server, Sqlite, MySql, PostgreSQL, Firebird) running on both .NET and Mono platforms.

In OrmLite: **1 Class = 1 Table**. There should be no surprising or hidden behaviour, the Typed API
that produces the Query 
[doesn't impact how results get intuitively mapped](http://stackoverflow.com/a/37443162/85785)
to the returned POCO's which could be different to the POCO used to create the query, e.g. containing only 
a subset of the fields you want populated.

Any non-scalar properties (i.e. complex types) are text blobbed by default in a schema-less text field 
using any of the [available pluggable text serializers](#pluggable-complex-type-serializers). 
Support for [POCO-friendly references](#reference-support-poco-style) is also available to provide 
a convenient API to persist related models. Effectively this allows you to create a table from any 
POCO type and it should persist as expected in a DB Table with columns for each of the classes 1st 
level public properties.

# Download 

[![Download on NuGet](https://raw.githubusercontent.com/ServiceStack/Assets/master/img/release-notes/install-ormlite.png)](https://www.nuget.org/packages?q=servicestack+ormlite)

### 8 flavours of OrmLite is on NuGet: 

  - [ServiceStack.OrmLite.SqlServer](http://nuget.org/List/Packages/ServiceStack.OrmLite.SqlServer)
  - [ServiceStack.OrmLite.SqlServer.Data](http://nuget.org/List/Packages/ServiceStack.OrmLite.SqlServer.Data) (uses [Microsoft.Data.SqlClient](https://devblogs.microsoft.com/dotnet/introducing-the-new-microsoftdatasqlclient/))
  - [ServiceStack.OrmLite.Sqlite](http://nuget.org/packages/ServiceStack.OrmLite.Sqlite)
  - [ServiceStack.OrmLite.Sqlite.Data](http://nuget.org/packages/ServiceStack.OrmLite.Sqlite.Data) (uses [Microsoft.Data.SQLite](https://stackoverflow.com/a/52025556/85785))
  - [ServiceStack.OrmLite.Sqlite.Windows](http://nuget.org/packages/ServiceStack.OrmLite.Sqlite.Windows) (Windows / .NET Framework only)
  - [ServiceStack.OrmLite.PostgreSQL](http://nuget.org/List/Packages/ServiceStack.OrmLite.PostgreSQL)
  - [ServiceStack.OrmLite.MySql](http://nuget.org/List/Packages/ServiceStack.OrmLite.MySql)
  - [ServiceStack.OrmLite.MySqlConnector](http://nuget.org/List/Packages/ServiceStack.OrmLite.MySqlConnector) (uses [MySqlConnector](https://github.com/mysql-net/MySqlConnector))

These packages contain both **.NET Framework v4.5** and **.NET Standard 2.0** versions and supports both .NET Framework and .NET Core projects.

The `.Core` packages contains only **.NET Standard 2.0** versions which can be used in ASP.NET Core Apps running on the .NET Framework:

  - [ServiceStack.OrmLite.SqlServer.Core](http://nuget.org/List/Packages/ServiceStack.OrmLite.SqlServer.Core)
  - [ServiceStack.OrmLite.PostgreSQL.Core](http://nuget.org/List/Packages/ServiceStack.OrmLite.PostgreSQL.Core)
  - [ServiceStack.OrmLite.MySql.Core](http://nuget.org/List/Packages/ServiceStack.OrmLite.MySql.Core)
  - [ServiceStack.OrmLite.Sqlite.Core](http://nuget.org/packages/ServiceStack.OrmLite.Sqlite.Core) 

Unofficial Releases maintained by ServiceStack Community:

  - [ServiceStack.OrmLite.Oracle](http://nuget.org/packages/ServiceStack.OrmLite.Oracle)
  - [ServiceStack.OrmLite.Firebird](http://nuget.org/List/Packages/ServiceStack.OrmLite.Firebird)
  - [ServiceStack.OrmLite.VistaDb](http://nuget.org/List/Packages/ServiceStack.OrmLite.VistaDb)

_Latest v4+ on NuGet is a [commercial release](https://servicestack.net/ormlite) with [free quotas](https://servicestack.net/download#free-quotas)._

### [Getting Started with OrmLite and AWS RDS](https://github.com/ServiceStackApps/AwsGettingStarted)

OrmLite has great support AWS's managed RDS Databases, follow these getting started guides to help getting up and running quickly:

- [PostgreSQL](https://github.com/ServiceStackApps/AwsGettingStarted#getting-started-with-aws-rds-postgresql-and-ormlite)
- [Aurora](https://github.com/ServiceStackApps/AwsGettingStarted#getting-started-with-aws-rds-aurora-and-ormlite)
- [MySQL](https://github.com/ServiceStackApps/AwsGettingStarted#getting-started-with-aws-rds-mysql-and-ormlite)
- [MariaDB](https://github.com/ServiceStackApps/AwsGettingStarted#getting-started-with-aws-rds-mariadb-and-ormlite)
- [SQL Server](https://github.com/ServiceStackApps/AwsGettingStarted#getting-started-with-aws-rds-sql-server-and-ormlite)

#### [Docs and Downloads for older v3 BSD releases](https://github.com/ServiceStackV3/ServiceStackV3)

### Copying

Since September 2013, ServiceStack source code is available under GNU Affero General Public 
License/FOSS License Exception, see license.txt in the source. 
Alternative [commercial licensing](https://servicestack.net/ormlite) is also available.

### Contributing

Contributors need to approve the [Contributor License Agreement](https://docs.google.com/forms/d/16Op0fmKaqYtxGL4sg7w_g-cXXyCoWjzppgkuqzOeKyk/viewform) 
before submitting pull-requests, see the [Contributing wiki](https://github.com/ServiceStack/ServiceStack/wiki/Contributing) for more details. 

***

## Usage

First Install the NuGet package of the RDBMS you want to use, e.g:

    PM> Install-Package ServiceStack.OrmLite.SqlServer

Each RDBMS includes a specialized dialect provider that encapsulated the differences in each RDBMS 
to support OrmLite features. The available Dialect Providers for each RDBMS is listed below:

    SqlServerDialect.Provider      // SQL Server Version 2012+
    SqliteDialect.Provider         // Sqlite
    PostgreSqlDialect.Provider     // PostgreSQL 
    MySqlDialect.Provider          // MySql
    OracleDialect.Provider         // Oracle
    FirebirdDialect.Provider       // Firebird
    VistaDbDialect.Provider        // Vista DB

#### SQL Server Versions

There are a number of different SQL Server dialects to take advantage of features available in each version. For any version before SQL Server 2008 please use `SqlServer2008Dialect.Provider`, for any other version please use the best matching version:

    SqlServer2008Dialect.Provider  // SQL Server <= 2008
    SqlServer2012Dialect.Provider  // SQL Server 2012
    SqlServer2014Dialect.Provider  // SQL Server 2014
    SqlServer2016Dialect.Provider  // SQL Server 2016
    SqlServer2017Dialect.Provider  // SQL Server 2017+

### Configure OrmLiteConnectionFactory

To configure OrmLite you need the DB Connection string along the Dialect Provider of the RDBMS you're
connecting to, e.g: 

```csharp
var dbFactory = new OrmLiteConnectionFactory(
    connectionString,  
    SqlServerDialect.Provider);
```

If you're using an IOC you can register `OrmLiteConnectionFactory` as a **singleton**, e.g:

```csharp
container.Register<IDbConnectionFactory>(c => 
    new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider)); //InMemory Sqlite DB
```

You can then use the `dbFactory` to open ADO.NET DB Connections to your database. 
If connecting to an empty database you can use OrmLite's Create Table API's to create any tables
you need based solely on the Schema definition of your POCO and populate it with any initial 
seed data you need, e.g:

```csharp
using (var db = dbFactory.Open())
{
    if (db.CreateTableIfNotExists<Poco>())
    {
        db.Insert(new Poco { Id = 1, Name = "Seed Data"});
    }

    var result = db.SingleById<Poco>(1);
    result.PrintDump(); //= {Id: 1, Name:Seed Data}
}
```

## [OrmLite Interactive Tour](http://gistlyn.com/ormlite)

The best way to learn about OrmLite is to take the [OrmLite Interactive Tour](http://gistlyn.com/ormlite)
which lets you try out and explore different OrmLite features immediately from the comfort of your own 
browser without needing to install anything:

[![](https://raw.githubusercontent.com/ServiceStack/Assets/master/img/ormlite/ormlite-tour.png)](http://gistlyn.com/ormlite)

## [Type Converters](https://github.com/ServiceStack/ServiceStack.OrmLite/wiki/OrmLite-Type-Converters)

You can customize, enhance or replace how OrmLite handles specific .NET Types with the new 
[OrmLite Type Converters](https://github.com/ServiceStack/ServiceStack.OrmLite/wiki/OrmLite-Type-Converters).

There's also support for SQL Server-specific `SqlGeography`, `SqlGeometry` and `SqlHierarchyId` Types,
See [docs on SQL Server Types](https://github.com/ServiceStack/ServiceStack.OrmLite/wiki/SQL-Server-Types) 
for instructions on how to enable them.

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
[ApiSqlServerTestsAsync.cs](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/Async/ApiSqlServerTestsAsync.cs).

### Async RDBMS Providers

Currently only a limited number of RDBMS providers offer async API's, which at this time are only:

  - [SQL Server .NET 4.5+](https://www.nuget.org/packages/ServiceStack.OrmLite.SqlServer)
  - [PostgreSQL .NET 4.5+](https://www.nuget.org/packages/ServiceStack.OrmLite.PostgreSQL)
  - [MySQL .NET 4.5+](https://www.nuget.org/packages/ServiceStack.OrmLite.MySql)

We've also added a 
[.NET 4.5 build for Sqlite](https://www.nuget.org/packages/ServiceStack.OrmLite.Sqlite) 
as it's a common use-case to swapout to use Sqlite's in-memory provider for faster tests. 
But as Sqlite doesn't provide async API's under-the-hood we fallback to *pseudo async* support where we just wrap its synchronous responses in `Task` results. 

# API Examples

OrmLite provides terse and intuitive typed API's for database querying from simple
lambda expressions to more complex LINQ-Like Typed SQL Expressions which you can use to
construct more complex queries. To give you a flavour here are some examples: 

### Querying with SELECT

```csharp
int agesAgo = DateTime.Today.AddYears(-20).Year;
db.Select<Author>(x => x.Birthday >= new DateTime(agesAgo, 1, 1) 
                    && x.Birthday <= new DateTime(agesAgo, 12, 31));
```

```csharp
db.Select<Author>(x => Sql.In(x.City, "London", "Madrid", "Berlin"));
```

```csharp
db.Select<Author>(x => x.Earnings <= 50);
```

```csharp
db.Select<Author>(x => x.Name.StartsWith("A"));
```

```csharp
db.Select<Author>(x => x.Name.EndsWith("garzon"));
```

```csharp
db.Select<Author>(x => x.Name.Contains("Benedict"));
```

```csharp
db.Select<Author>(x => x.Rate == 10 && x.City == "Mexico");
```

```csharp
db.Select<Author>(x => x.Rate.ToString() == "10"); //impicit string casting
```

```csharp
db.Select<Author>(x => "Rate " + x.Rate == "Rate 10"); //server string concatenation
```

### Convenient common usage data access patterns 

OrmLite also includes a number of convenient API's providing DRY, typed data access for common queries:

```csharp
Person person = db.SingleById<Person>(1);
```

```csharp
Person person = db.Single<Person>(x => x.Age == 42);
```

```csharp
var q = db.From<Person>()
          .Where(x => x.Age > 40)
          .Select(Sql.Count("*"));

int peopleOver40 = db.Scalar<int>(q);
```

```csharp
int peopleUnder50 = db.Count<Person>(x => x.Age < 50);
```

```csharp
bool has42YearOlds = db.Exists<Person>(new { Age = 42 });
```

```csharp
int maxAgeUnder50 = db.Scalar<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
```

```csharp
var q = db.From<Person>()
    .Where(x => x.Age == 27)
    .Select(x => x.LastName);
    
List<string> results = db.Column<string>(q);
```

```csharp
var q = db.From<Person>()
          .Where(x => x.Age < 50)
          .Select(x => x.Age);

HashSet<int> results = db.ColumnDistinct<int>(q);
```

```csharp
var q = db.From<Person>()
          .Where(x => x.Age < 50)
          .Select(x => new { x.Id, x.LastName });

Dictionary<int,string> results = db.Dictionary<int, string>(q);
```

```csharp
var q = db.From<Person>()
          .Where(x => x.Age < 50)
          .Select(x => new { x.Age, x.LastName });

Dictionary<int, List<string>> results = db.Lookup<int, string>(q);
```

The new `db.KeyValuePair<K,V>` API is similar to `db.Dictionary<K,V>` where it uses the **first 2 columns** for its Key/Value Pairs to 
create a Dictionary but is more appropriate when the results can contain duplicate Keys or when ordering needs to be preserved:

```csharp
var q = db.From<StatsLog>()
    .GroupBy(x => x.Name)
    .Select(x => new { x.Name, Count = Sql.Count("*") })
    .OrderByDescending("Count");

var results = db.KeyValuePairs<string, int>(q);
```

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

If you supply your own where expression, it updates every field (inc. Id) but uses your filter instead:

```csharp
db.Update(new Person { Id = 1, FirstName = "JJ" }, p => p.LastName == "Hendrix");
```

One way to limit the fields which gets updated is to use an **Anonymous Type**:

```csharp
db.Update<Person>(new { FirstName = "JJ" }, p => p.LastName == "Hendrix");
```

Or by using `UpdateNonDefaults` which only updates the non-default values in your model using the filter specified:

```csharp
db.UpdateNonDefaults(new Person { FirstName = "JJ" }, p => p.LastName == "Hendrix");
```

#### UpdateOnly

As updating a partial row is a common use-case in Db's, we've added a number of methods for just 
this purpose, named **UpdateOnly**. 

The lambda syntax lets you update only the fields listed in property initializers, e.g:

```csharp
db.UpdateOnly(() => new Person { FirstName = "JJ" });
```

The second argument lets you specify a filter for updates: 

```csharp
db.UpdateOnly(() => new Person { FirstName = "JJ" }, where: p => p.LastName == "Hendrix");
```

Alternatively you can pass in a POCO directly, in which case the first expression in an `UpdateOnly` 
statement is used to specify which fields should be updated:

```csharp
db.UpdateOnly(new Person { FirstName = "JJ" }, onlyFields: p => p.FirstName);

db.UpdateOnly(new Person { FirstName = "JJ", Age = 12 }, 
    onlyFields: p => new { p.FirstName, p.Age });

db.UpdateOnly(new Person { FirstName = "JJ", Age = 12 }, 
    onlyFields: p => new[] { "Name", "Age" });
```

When present, the second expression is used as the where filter:

```csharp
db.UpdateOnly(new Person { FirstName = "JJ" }, 
    onlyFields: p => p.FirstName, 
    where: p => p.LastName == "Hendrix");
```
Instead of using the expression filters above you can choose to use an SqlExpression builder which provides more flexibility when you want to programatically construct the update statement:

```csharp
var q = db.From<Person>()
    .Update(p => p.FirstName);

db.UpdateOnly(new Person { FirstName = "JJ", LastName = "Hendo" }, onlyFields: q);
```

Using an Object Dictionary:

```csharp
var updateFields = new Dictionary<string,object> {
    [nameof(Person.FirstName)] = "JJ",
};

db.UpdateOnly<Person>(updateFields, p => p.LastName == "Hendrix");
```

Using a typed SQL Expression:

```csharp
var q = db.From<Person>()
    .Where(x => x.FirstName == "Jimi")
    .Update(p => p.FirstName);
          
db.UpdateOnly(new Person { FirstName = "JJ" }, onlyFields: q);
```

### Updating existing values

The `UpdateAdd` API provides several Typed API's for updating existing values:

```csharp
//Increase everyone's Score by 3 points
db.UpdateAdd(() => new Person { Score = 3 }); 

//Remove 5 points from Jackson Score
db.UpdateAdd(() => new Person { Score = -5 }, where: x => x.LastName == "Jackson");

//Graduate everyone and increase everyone's Score by 2 points 
db.UpdateAdd(() => new Person { Points = 2, Graduated = true });

//Add 10 points to Michael's score
var q = db.From<Person>()
    .Where(x => x.FirstName == "Michael");
db.UpdateAdd(() => new Person { Points = 10 }, q);
```

> Note: Any non-numeric values in an `UpdateAdd` statement (e.g. strings) are replaced as normal.

### INSERT

Insert's are pretty straight forward since in most cases you want to insert every field:

```csharp
db.Insert(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });
```

#### Partial Inserts

You can use `InsertOnly` for the rare cases you don't want to insert every field

```csharp
db.InsertOnly(() => new Person { FirstName = "Amy" });
```

Alternative API using an SqlExpression

```csharp
var q = db.From<Person>()
    .Insert(p => new { p.FirstName });

db.InsertOnly(new Person { FirstName = "Amy" }, onlyFields: q)
```

### DELETE

Like updates for DELETE's we also provide APIs that take a where Expression:
```csharp
db.Delete<Person>(p => p.Age == 27);
```

Or an SqlExpression:

```csharp
var q = db.From<Person>()
    .Where(p => p.Age == 27);

db.Delete<Person>(q);
```

As well as un-typed, string-based expressions:
```csharp
db.Delete<Person>(where: "Age = @age", new { age = 27 });
```

#### Delete from Table JOIN

Using a SqlExpression to delete rows by querying from a joined table:

```csharp
var q = db.From<Person>()
    .Join<PersonJoin>((x, y) => x.Id == y.PersonId)
    .Where<PersonJoin>(x => x.Id == 2);

db.Delete(q);
```

> Not supported in MySql

# API Overview

The API is minimal, providing basic shortcuts for the primitive SQL statements:

[![OrmLite API](https://raw.githubusercontent.com/ServiceStack/Assets/master/img/ormlite/OrmLiteApi.png)](https://raw.githubusercontent.com/ServiceStack/Assets/master/img/ormlite/OrmLiteApi.png)

OrmLite makes available most of its functionality via extension methods to add enhancments over ADO.NET's `IDbConnection`, providing 
a Typed RDBMS-agnostic API that transparently handles differences in each supported RDBMS provider.

## Create Tables Schemas

OrmLite is able to **CREATE**, **DROP** and **ALTER** RDBMS Tables from your code-first Data Models with rich annotations for 
controlling how the underlying RDBMS Tables are constructed. 

The Example below utilizes several annotations to customize the definition and behavior of RDBMS tables based on a POCOs 
**public properties**:

```csharp
public class Player
{
    public int Id { get; set; }                     // 'Id' is PrimaryKey by convention

    [Required]
    public string FirstName { get; set; }           // Creates NOT NULL Column

    [Alias("Surname")]                              // Maps to [Surname] RDBMS column
    public string LastName { get; set; }

    [Index(Unique = true)]                          // Creates Unique Index
    public string Email { get; set; }

    public List<Phone> PhoneNumbers { get; set; }   // Complex Types blobbed by default

    [Reference]
    public List<GameItem> GameItems { get; set; }   // 1:M Reference Type saved separately

    [Reference]
    public Profile Profile { get; set; }            // 1:1 Reference Type saved separately
    public int ProfileId { get; set; }              // 1:1 Self Ref Id on Parent Table

    [ForeignKey(typeof(Level), OnDelete="CASCADE")] // Creates ON DELETE CASCADE Constraint
    public Guid SavedLevelId { get; set; }          // Creates Foreign Key Reference

    public ulong RowVersion { get; set; }           // Optimistic Concurrency Updates
}

public class Phone                                  // Blobbed Type only
{
    public PhoneKind Kind { get; set; }
    public string Number { get; set; }
    public string Ext { get; set; }
}

public enum PhoneKind
{
    Home,
    Mobile,
    Work,
}

[Alias("PlayerProfile")]                            // Maps to [PlayerProfile] RDBMS Table
[CompositeIndex(nameof(Username), nameof(Region))]  // Creates Composite Index
public class Profile
{
    [AutoIncrement]                                 // Auto Insert Id assigned by RDBMS
    public int Id { get; set; }

    public PlayerRole Role { get; set; }            // Native support for Enums
    public Region Region { get; set; }
    public string Username { get; set; }
    public long HighScore { get; set; }

    [Default(1)]                                    // Created in RDBMS with DEFAULT (1)
    public long GamesPlayed { get; set; }

    [CheckConstraint("Energy BETWEEN 0 AND 100")]   // Creates RDBMS Check Constraint
    public short Energy { get; set; }

    public string ProfileUrl { get; set; }
    public Dictionary<string, string> Meta { get; set; }
}

public enum PlayerRole                              // Enums saved as strings by default
{
    Leader,
    Player,
    NonPlayer,
}

[EnumAsInt]                                         // Enum Saved as int
public enum Region
{
    Africa = 1,
    Americas = 2,
    Asia = 3,
    Australasia = 4,
    Europe = 5,
}

public class GameItem
{
    [PrimaryKey]                                    // Specify field to use as Primary Key
    [StringLength(50)]                              // Creates VARCHAR COLUMN
    public string Name { get; set; }

    public int PlayerId { get; set; }               // Foreign Table Reference Id

    [StringLength(StringLengthAttribute.MaxText)]   // Creates "TEXT" RDBMS Column 
    public string Description { get; set; }

    [Default(OrmLiteVariables.SystemUtc)]           // Populated with UTC Date by RDBMS
    public DateTime DateAdded { get; set; }
}

public class Level
{
    public Guid Id { get; set; }                    // Unique Identifer/GUID Primary Key
    public byte[] Data { get; set; }                // Saved as BLOB/Binary where possible
}
```

We can drop the existing tables and re-create the above table definitions with:

```csharp
using (var db = dbFactory.Open())
{
    if (db.TableExists<Level>())
        db.DeleteAll<Level>();                      // Delete ForeignKey data if exists

    //DROP and CREATE ForeignKey Tables in dependent order
    db.DropTable<Player>();
    db.DropTable<Level>();
    db.CreateTable<Level>();
    db.CreateTable<Player>();

    //DROP and CREATE tables without Foreign Keys in any order
    db.DropAndCreateTable<Profile>();
    db.DropAndCreateTable<GameItem>();

    var savedLevel = new Level
    {
        Id = Guid.NewGuid(),
        Data = new byte[]{ 1, 2, 3, 4, 5 },
    };
    db.Insert(savedLevel);

    var player = new Player
    {
        Id = 1,
        FirstName = "North",
        LastName = "West",
        Email = "north@west.com",
        PhoneNumbers = new List<Phone>
        {
            new Phone { Kind = PhoneKind.Mobile, Number = "123-555-5555"},
            new Phone { Kind = PhoneKind.Home,   Number = "555-555-5555", Ext = "123"},
        },
        GameItems = new List<GameItem>
        {
            new GameItem { Name = "WAND", Description = "Golden Wand of Odyssey"},
            new GameItem { Name = "STAFF", Description = "Staff of the Magi"},
        },
        Profile = new Profile
        {
            Username = "north",
            Role = PlayerRole.Leader,
            Region = Region.Australasia,
            HighScore = 100,
            GamesPlayed = 10,
            ProfileUrl = "https://www.gravatar.com/avatar/205e460b479e2e5b48aec07710c08d50.jpg",
            Meta = new Dictionary<string, string>
            {
                {"Quote", "I am gamer"}
            },
        },
        SavedLevelId = savedLevel.Id,
    };
    db.Save(player, references: true);
}
```

This will add a record in all the above tables with all the Reference data properties automatically populated which we can quickly see
by selecting the inserted `Player` record and all its referenced data by using [OrmLite's Load APIs](#querying-pocos-with-references), e.g:

```csharp
var dbPlayer = db.LoadSingleById<Player>(player.Id);

dbPlayer.PrintDump();
```

Which uses the [Dump Utils](http://docs.servicestack.net/dump-utils) to quickly display the populated data to the console:

    {
        Id: 1,
        FirstName: North,
        LastName: West,
        Email: north@west.com,
        PhoneNumbers: 
        [
            {
                Kind: Mobile,
                Number: 123-555-5555
            },
            {
                Kind: Home,
                Number: 555-555-5555,
                Ext: 123
            }
        ],
        GameItems: 
        [
            {
                Name: WAND,
                PlayerId: 1,
                Description: Golden Wand of Odyssey,
                DateAdded: 2018-01-17T07:53:45-05:00
            },
            {
                Name: STAFF,
                PlayerId: 1,
                Description: Staff of the Magi,
                DateAdded: 2018-01-17T07:53:45-05:00
            }
        ],
        Profile: 
        {
            Id: 1,
            Role: Leader,
            Region: Australasia,
            Username: north,
            HighScore: 100,
            GamesPlayed: 10,
            Energy: 0,
            ProfileUrl: "https://www.gravatar.com/avatar/205e460b479e2e5b48aec07710c08d50.jpg",
            Meta: 
            {
                Quote: I am gamer
            }
        },
        ProfileId: 1,
        SavedLevelId: 7690dfa4d31949ab9bce628c34d1c549,
        RowVersion: 2
    }

Feel free to continue expirementing with [this Example Live on Gistlyn](https://gistlyn.com/?gist=840bc7f09292ad5753d07cef6063893e&collection=991db51e44674ad01d3d318b24cf0934).

## Select APIs

If your SQL doesn't start with a **SELECT** statement, it is assumed a WHERE clause is being provided, e.g:

```csharp
var tracks = db.Select<Track>("Artist = @artist AND Album = @album",
    new { artist = "Nirvana", album = "Heart Shaped Box" });
```

Which is equivalent to:

```csharp
var tracks = db.Select<Track>("SELECT * FROM track WHERE Artist = @artist AND Album = @album", 
    new { artist = "Nirvana", album = "Heart Shaped Box" });
```

Use `Sql*` APIs for when you want to query custom SQL that is not a SELECT statement, e.g:

```csharp
var tracks = db.SqlList<Track>("EXEC GetArtistTracks @artist, @album",
    new { artist = "Nirvana", album = "Heart Shaped Box" });
```

**Select** returns multiple records: 

```csharp
List<Track> tracks = db.Select<Track>()
```

**Single** returns a single record:

```csharp
Track track = db.Single<Track>(x => x.RefId == refId)
```

**Dictionary** returns a Dictionary made from the first two columns:

```csharp
Dictionary<int, string> trackIdNamesMap = db.Dictionary<int, string>(
    db.From<Track>().Select(x => new { x.Id, x.Name }))

Dictionary<int, string> trackIdNamesMap = db.Dictionary<int, string>(
    "select Id, Name from Track")
```

**Lookup** returns an `Dictionary<K, List<V>>` made from the first two columns:

```csharp
Dictionary<int, List<string>> albumTrackNames = db.Lookup<int, string>(
    db.From<Track>().Select(x => new { x.AlbumId, x.Name }))

Dictionary<int, List<string>> albumTrackNames = db.Lookup<int, string>(
    "select AlbumId, Name from Track")
```

**Column** returns a List of first column values:

```csharp
List<string> trackNames = db.Column<string>(db.From<Track>().Select(x => x.Name))

List<string> trackNames = db.Column<string>("select Name from Track")
```

**HashSet** returns a HashSet of distinct first column values:

```csharp    
HashSet<string> uniqueTrackNames = db.ColumnDistinct<string>(
    db.From<Track>().Select(x => x.Name))

HashSet<string> uniqueTrackNames = db.ColumnDistinct<string>("select Name from Track")
```

**Scalar** returns a single scalar value:

```csharp
var trackCount = db.Scalar<int>(db.From<Track>().Select(Sql.Count("*")))

var trackCount = db.Scalar<int>("select count(*) from Track")
```

Anonymous types passed into **Where** are treated like an **AND** filter:

```csharp
var track3 = db.Where<Track>(new { AlbumName = "Throwing Copper", TrackNo = 3 })
```

SingleById(s), SelectById(s), etc provide strong-typed convenience methods to fetch by a Table's **Id** primary key field.

```csharp
var track = db.SingleById<Track>(1);
var tracks = db.SelectByIds<Track>(new[]{ 1,2,3 });
```

## Nested Typed Sub SqlExpressions

The `Sql.In()` API supports nesting and combining of multiple Typed SQL Expressions together 
in a single SQL Query, e.g:
  
```csharp
var usaCustomerIds = db.From<Customer>(c => c.Country == "USA").Select(c => c.Id);
var usaCustomerOrders = db.Select(db.From<Order>()
    .Where(x => Sql.In(x.CustomerId, usaCustomerIds)));
``` 

### SQL In Expressions

```csharp
db.Select<Author>(x => Sql.In(x.City, "London", "Madrid", "Berlin"));

var cities = new[] { "London", "Madrid", "Berlin" };
db.Select<Author>(x => Sql.In(x.City, cities));
```

### Parametrized IN Values

OrmLite also supports providing collection of values which is automatically split into multiple DB parameters to simplify executing parameterized SQL with multiple IN Values, e.g:

```csharp
var ids = new[]{ 1, 2, 3};
var results = db.Select<Table>("Id in (@ids)", new { ids });

var names = new List<string>{ "foo", "bar", "qux" };
var results = db.SqlList<Table>("SELECT * FROM Table WHERE Name IN (@names)", new { names });
```

### Custom SQL using PostgreSQL Arrays

If using PostgreSQL you can take advantage of its complex Array Types and utilize its [Array Functions and Operators](https://www.postgresql.org/docs/9.6/functions-array.html), e.g:

```csharp
var ids = new[]{ 1, 2, 3};
var q = Db.From<Table>()
    .Where("ARRAY[{0}] && ref_ids", ids.Join(","))
var results = db.Select(q);
```

When comparing a string collection you can use `SqlInValues` to create a quoted SQL IN list, e.g:

```csharp
var q = Db.From<Table>()
    .Where($"ARRAY[{new SqlInValues(cities).ToSqlInString()}] && cities");
var results = db.Select(q);
```

### Lazy Queries

API's ending with `Lazy` yield an IEnumerable sequence letting you stream the results without having to map the entire resultset into a disconnected List of POCO's first, e.g:

```csharp
var lazyQuery = db.SelectLazy<Person>("Age > @age", new { age = 40 });
// Iterate over a lazy sequence 
foreach (var person in lazyQuery) {
   //...  
}
```
 
### Save Methods
 
`Save` and `SaveAll` will Insert if no record with **Id** exists, otherwise it Updates. 

`Save` will populate any `[AutoIncrement]` or `[AutoId]` Primary Keys, e.g:

```csharp
db.Save(item);
item.Id // RDBMS populated Auto Id 
```

Alternatively you can also manually Select and Retrieve the Inserted RDBMS Auto Id in a single query with `Insert` APIs by specifying `selectIdentity:true`:

```csharp
item.Id = db.Insert(item, selectIdentity:true);
```

#### Other examples

```csharp
var topVIPs = db.WhereLazy<Person>(new { Age = 27 }).Where(p => IsVip(p)).Take(5)
```

### Other Notes

 - All **Insert**, **Update**, and **Delete** methods take multiple params, while `InsertAll`, `UpdateAll` and `DeleteAll` take IEnumerables.
 - Methods containing the word **Each** return an IEnumerable<T> and are lazily loaded (i.e. non-buffered).

# Features

Whilst OrmLite aims to provide a light-weight typed wrapper around SQL, it offers a number of convenient features that makes working with RDBMS's a clean and enjoyable experience:

## Typed SqlExpression support for JOIN's

Starting with the most basic example you can simply specify the table you want to join with:

```csharp
var q = db.From<Customer>()
          .Join<CustomerAddress>();

var dbCustomers = db.Select<Customer>(q);
```

This query rougly maps to the following SQL:

```sql
SELECT Customer.* 
  FROM Customer 
       INNER JOIN 
       CustomerAddress ON (Customer.Id == CustomerAddress.CustomerId)
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

var c = db.LoadSelect<Customer>(x => x.Name == "The Customer");
c.WorkAddress.Address.Print(); // 2 Work Road

var ukAddress = db.Single<CustomerAddress>(x => x.Country == "UK");
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

The `SelectMulti` API lets you select from multiple joined tables into a typed tuple

```csharp
var q = db.From<Customer>()
    .Join<Customer, CustomerAddress>()
    .Join<Customer, Order>()
    .Where(x => x.CreatedDate >= new DateTime(2016,01,01))
    .And<CustomerAddress>(x => x.Country == "Australia");

var results = db.SelectMulti<Customer, CustomerAddress, Order>(q);

foreach (var tuple in results)
{
    Customer customer = tuple.Item1;
    CustomerAddress custAddress = tuple.Item2;
    Order custOrder = tuple.Item3;
}
```

Thanks to Micro ORM's lightweight abstractions over ADO.NET that maps to clean POCOs, we can also use 
OrmLite's embedded version of [Dapper's QueryMultiple](http://stackoverflow.com/a/37420341/85785):

```csharp
var q = db.From<Customer>()
    .Join<Customer, CustomerAddress>()
    .Join<Customer, Order>()
    .Select("*");

using (var multi = db.QueryMultiple(q.ToSelectStatement()))
{
    var results = multi.Read<Customer, CustomerAddress, Order, 
        Tuple<Customer,CustomerAddress,Order>>(Tuple.Create).ToList();

    foreach (var tuple in results)
    {
        Customer customer = tuple.Item1;
        CustomerAddress custAddress = tuple.Item2;
        Order custOrder = tuple.Item3;
    }
}
```

### SELECT DISTINCT in SelectMulti

[SelectMulti](https://github.com/ServiceStack/ServiceStack.OrmLite#selecting-multiple-columns-across-joined-tables) APIs for populating
multiple tables now supports **SELECT DISTINCT** with:

```csharp
var tuples = db.SelectMulti<Customer, CustomerAddress>(q.SelectDistinct());
```

### Select data from multiple tables into a Custom POCO

Another implicit behaviour when selecting from a typed SqlExpression is that results are mapped to the 
`Customer` POCO. To change this default we just need to explicitly specify what POCO it should map to instead:

```csharp
List<FullCustomerInfo> customers = db.Select<FullCustomerInfo>(
    db.From<Customer>().Join<CustomerAddress>());
```

Where `FullCustomerInfo` is any POCO that contains a combination of properties matching any of the joined 
tables in the query. 

The above example is also equivalent to the shorthand `db.Select<Into,From>()` API:

```csharp
var q = db.From<Customer>()
          .Join<CustomerAddress>();

var customers = db.Select<FullCustomerInfo,Customer>(q);
```

Rules for how results are mapped is simply each property on `FullCustomerInfo` is mapped to the first matching property in any of the tables in the order they were added to the SqlExpression.

The mapping also includes a fallback for referencing fully-qualified names in the format: `{TableName}{FieldName}` allowing you to reference ambiguous fields, e.g:

  - `CustomerId` => "Customer"."Id"
  - `OrderId` => "Order"."Id"
  - `CustomerName` => "Customer"."Name"
  - `OrderCost` => "Order"."Cost"

## Dynamic Result Sets

In addition to populating Typed POCOs, OrmLite has a number of flexible options for accessing dynamic resultsets with adhoc schemas:

### C# 7 Value Tuples
 
The C# 7 Value Tuple support enables a terse, clean and typed API for accessing the Dynamic Result Sets returned when using a custom Select expression:

```csharp
var query = db.From<Employee>()
    .Join<Department>()
    .OrderBy(e => e.Id)
    .Select<Employee, Department>(
        (e, d) => new { e.Id, e.LastName, d.Name });
 
var results = db.Select<(int id, string lastName, string deptName)>(query);
 
var row = results[i];
$"row: ${row.id}, ${row.lastName}, ${row.deptName}".Print();
```
 
Full Custom SQL Example:
 
```csharp
var results = db.SqlList<(int count, string min, string max, int sum)>(
    "SELECT COUNT(*), MIN(Word), MAX(Word), Sum(Total) FROM Table");
```
 
Partial Custom SQL Select Example:
 
```csharp
var query = db.From<Table>()
    .Select("COUNT(*), MIN(Word), MAX(Word), Sum(Total)");
 
var result = db.Single<(int count, string min, string max, int sum)>(query);
```
 
Same as above, but using Typed APIs:
 
```csharp
var result = db.Single<(int count, string min, string max, int sum)>(
    db.From<Table>()
        .Select(x => new {
            Count = Sql.Count("*"),
            Min = Sql.Min(x.Word),
            Max = Sql.Max(x.Word),
            Sum = Sql.Sum(x.Total)
        }));
```

There's also support for returning unstructured resultsets in `List<object>`, e.g:

```csharp
var results = db.Select<List<object>>(db.From<Poco>()
  .Select("COUNT(*), MIN(Id), MAX(Id)"));

results[0].PrintDump();
```

Output of objects in the returned `List<object>`:

    [
        10,
        1,
        10
    ]

You can also Select `Dictionary<string,object>` to return a dictionary of column names mapped with their values, e.g:

```csharp
var results = db.Select<Dictionary<string,object>>(db.From<Poco>()
  .Select("COUNT(*) Total, MIN(Id) MinId, MAX(Id) MaxId"));

results[0].PrintDump();
```

Output of objects in the returned `Dictionary<string,object>`:

    {
        Total: 10,
        MinId: 1,
        MaxId: 10
    }

and can be used for API's returning a **Single** row result:

```csharp
var result = db.Single<List<object>>(db.From<Poco>()
  .Select("COUNT(*) Total, MIN(Id) MinId, MAX(Id) MaxId"));
```

or use `object` to fetch an unknown **Scalar** value:

```csharp
object result = db.Scalar<object>(db.From<Poco>().Select(x => x.Id));
```

### Select data from multiple tables into Dynamic ResultSets

You can also select data from multiple tables into 
[dynamic result sets](https://github.com/ServiceStack/ServiceStack.OrmLite#dynamic-result-sets)
which provide [several Convenience APIs](http://stackoverflow.com/a/37443162/85785)
for accessing data from an unstructured queries.

Using dynamic:

```csharp
var q = db.From<Employee>()
    .Join<Department>()
    .Select<Employee, Department>((e, d) => new { e.FirstName, d.Name });
    
List<dynamic> results = db.Select<dynamic>(q);
foreach (dynamic result in results)
{
    string firstName = result.FirstName;
    string deptName = result.Name;
}
```

Dictionary of Objects:

```csharp
List<Dictionary<string,object>> rows = db.Select<Dictionary<string,object>>(q);
```

List of Objects:

```csharp
List<List<object>> rows = db.Select<Dictionary<string,object>>(q);
```

Custom Key/Value Dictionary:

```csharp
Dictionary<string,string> rows = db.Dictionary<string,string>(q);
```

### BelongsTo Attribute

The `[BelongTo]` attribute can be used for specifying how Custom POCO results are mapped when the resultset is ambiguous, e.g:

```csharp
class A { 
    public int Id { get; set; }
}
class B {
    public int Id { get; set; }
    public int AId { get; set; }
}
class C {
    public int Id { get; set; }
    public int BId { get; set; }
}
class Combined {
    public int Id { get; set; }
    [BelongTo(typeof(B))]
    public int BId { get; set; }
}

var q = db.From<A>()
    .Join<B>()
    .LeftJoin<B,C>();

var results = db.Select<Combined>(q); //Combined.BId = B.Id
```

### Advanced Example

Seeing how the SqlExpression is constructed, joined and mapped, we can take a look at a more advanced example to showcase more of the new API's available:

```csharp
List<FullCustomerInfo> rows = db.Select<FullCustomerInfo>(  // Map results to FullCustomerInfo POCO
  db.From<Customer>()                                       // Create typed Customer SqlExpression
    .LeftJoin<CustomerAddress>()                            // Implicit left join with base table
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
var customers = db.LoadSelect<Customer>(x => x.Name == "Customer 1");
```

More examples available in [LoadReferencesTests.cs](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/tests/ServiceStack.OrmLite.Tests/LoadReferencesTests.cs)

Unlike normal complex properties, references:

  - Doesn't persist as complex type blobs
  - Doesn't impact normal querying
  - Saves and loads references independently from itself
  - Are serializable with Text serializers (only populated are visible).
  - Loads related data only 1-reference-level deep
 
Basically they provides a better story when dealing with referential data that doesn't impact the POCO's ability to be used as DTO's. 

### Merge Disconnected POCO Result Sets

The `Merge` extension method can stitch disconnected POCO collections together as per their relationships defined in OrmLite's POCO References.

For example you can select a collection of Customers who've made an order with quantities of 10 or more and in a separate query select their filtered Orders and then merge the results of these 2 distinct queries together with:

```csharp
//Select Customers who've had orders with Quantities of 10 or more
var q = db.From<Customer>()
          .Join<Order>()
          .Where<Order>(o => o.Qty >= 10)
          .SelectDistinct();

List<Customer> customers = db.Select<Customer>(q);

//Select Orders with Quantities of 10 or more
List<Order> orders = db.Select<Order>(o => o.Qty >= 10);

customers.Merge(orders); // Merge disconnected Orders with their related Customers

customers.PrintDump();   // Print merged customers and orders datasets
```

### Custom Load References

You can selectively specifying which references you want to load using the `include` parameter, e.g:

```csharp
var customerWithAddress = db.LoadSingleById<Customer>(customer.Id, include: new[] { "PrimaryAddress" });

//Alternative
var customerWithAddress = db.LoadSingleById<Customer>(customer.Id, include: x => new { x.PrimaryAddress });
```

### Custom Select with JOIN

You can specify SQL Aliases for ambiguous columns using anonymous properties, e.g:

```csharp
var q = db.From<Table>()
    .Join<JoinedTable>()
    .Select<Table, JoinedTable>((a, b) => new { a, JoinId = b.Id, JoinName = b.Name });
```

Which is roughly equivalent to:

    SELECT a.*, b.Id AS JoinId, b.Name AS JoinName

Where it selects all columns from the primary `Table` as well as `Id` and `Name` columns from `JoinedTable,` 
returning them in the `JoinId` and `JoinName` custom aliases.

### Nested JOIN Table Expressions

You can also query POCO References on JOIN tables, e.g:

```csharp
var q = db.From<Table>()
    .Join<Join1>()
    .Join<Join1, Join2>()
    .Where(x => !x.IsValid.HasValue && 
        x.Join1.IsValid &&
        x.Join1.Join2.Name == theName &&
        x.Join1.Join2.IntValue == intValue)
    .GroupBy(x => x.Join1.Join2.IntValue)
    .Having(x => Sql.Max(x.Join1.Join2.IntValue) != 10)
    .Select(x => x.Join1.Join2.IntValue);
```

### Table aliases

The `TableAlias` APIs lets you specify table aliases when joining same table multiple times together to differentiate from any 
ambiguous columns in Queries with multiple self-reference joins, e.g:

```csharp
var q = db.From<Page>(db.TableAlias("p1"))
    .Join<Page>((p1, p2) => 
        p1.PageId == p2.PageId && 
        p2.ActivityId == activityId, db.TableAlias("p2"))
    .Join<Page,Category>((p2,c) => Sql.TableAlias(p2.Category) == c.Id)
    .Join<Page,Page>((p1,p2) => Sql.TableAlias(p1.Rank,"p1") < Sql.TableAlias(p2.Rank,"p2"))
    .Select<Page>(p => new {
        ActivityId = Sql.TableAlias(p.ActivityId, "p2")
    });

var rows = db.Select(q);
```

### Unique Constraints

In addition to creating an Index with unique constraints using `[Index(Unique=true)]` you can now use `[Unique]` to enforce a single column should only contain unique values or annotate the class with `[UniqueConstraint]` to specify a composite unique constraint, e.g:

```csharp
[UniqueConstraint(nameof(PartialUnique1), nameof(PartialUnique2), nameof(PartialUnique3))]
public class UniqueTest
{
    [AutoIncrement]
    public int Id { get; set; }

    [Unique]
    public string UniqueField { get; set; }

    public string PartialUnique1 { get; set; }
    public string PartialUnique2 { get; set; }
    public string PartialUnique3 { get; set; }
}
```

### Auto populated Guid Ids

Support for Auto populating `Guid` Primary Keys is available using the `[AutoId]` attribute, e.g:

```csharp
public class Table
{
    [AutoId]
    public Guid Id { get; set; }
}
```

In SQL Server it will populate `Id` primary key with `newid()`, in `PostgreSQL` it uses `uuid_generate_v4()` which requires installing the the **uuid-ossp** extension by running the SQL below on each PostgreSQL RDBMS it's used on:

    CREATE EXTENSION IF NOT EXISTS "uuid-ossp"

For all other RDBMS's OrmLite will populate the `Id` with `Guid.NewGuid()`. In all RDBMS's it will populate the `Id` property on `db.Insert()` or `db.Save()` with the new value, e.g:

```csharp
var row = new Table { ... };
db.Insert(row);
row.Id //= Auto populated with new Guid
```

### SQL Server 2012 Sequences

The `[Sequence]` attribute can be used as an alternative to `[AutoIncrement]` for inserting rows with an auto incrementing integer value populated by SQL Server, but instead of needing an `IDENTITY` column it can populate a normal `INT` column from a user-defined Sequence, e.g:

```csharp
public class SequenceTest
{
    [Sequence("Seq_SequenceTest_Id"), ReturnOnInsert]
    public int Id { get; set; }

    public string Name { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }

    [Sequence("Seq_Counter")]
    public int Counter { get; set; }
}

var user = new SequenceTest { Name = "me", Email = "me@mydomain.com" };
db.Insert(user);

user.Id //= Populated by next value in "Seq_SequenceTest_Id" SQL Server Sequence
```

The new `[ReturnOnInsert]` attribute tells OrmLite which columns to return the values of, in this case it returns the new Sequence value the row was inserted with. Sequences offer more flexibility than `IDENTITY` columns where you can use multiple sequences in a table or have the same sequence shared across multiple tables.

When creating tables, OrmLite will also create any missing Sequences automatically so you can continue to have reproducible tests and consistent Startups states that's unreliant on external state. But it doesn't drop sequences when OrmLite drops the table as they could have other external dependents.

To be able to use the new sequence support you'll need to use an SQL Server dialect greater than SQL Server 2012+, e.g:

```csharp
var dbFactory = new OrmLiteConnectionFactory(connString, SqlServer2012Dialect.Provider);
```

### SQL Server Table Hints

Using the same JOIN Filter feature OrmLite also lets you add SQL Server Hints on JOIN Table expressions, e.g:

```csharp
var q = db.From<Car>()
    .Join<Car, CarType>((c, t) => c.CarId == t.CarId, SqlServerTableHint.ReadUncommitted);
```

Which emits the appropriate SQL Server hints:

```sql
SELECT "Car"."CarId", "CarType"."CarTypeName" 
FROM "Car" INNER JOIN "CarType" WITH (READUNCOMMITTED) ON ("Car"."CarId" = "CarType"."CarId")
```

### Custom SqlExpression Filter

The generated SQL from a Typed `SqlExpression` can also be customized using `.WithSqlFilter()`, e.g:

```csharp
var q = db.From<Table>()
    .Where(x => x.Age == 27)
    .WithSqlFilter(sql => sql + " option (recompile)");

var q = db.From<Table>()
    .Where(x => x.Age == 27)
    .WithSqlFilter(sql => sql + " WITH UPDLOCK");

var results = db.Select(q);
```

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

### RowVersion Byte Array

To improve reuse of OrmLite's Data Models in Dapper, OrmLite also supports `byte[] RowVersion` which lets you use OrmLite Data Models with `byte[] RowVersion` properties in Dapper queries.

## Conflict Resolution using commandFilter

An optional `Func<IDbCommand> commandFilter` is available in all `INSERT` and `UPDATE` APIs to allow customization and inspection of the populated `IDbCommand` before it's run. 
This feature is utilized in the [Conflict Resolution Extension methods](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/master/src/ServiceStack.OrmLite/OrmLiteConflictResolutions.cs) 
where you can specify the conflict resolution strategy when a Primary Key or Unique constraint violation occurs:

```csharp
db.InsertAll(rows, dbCmd => dbCmd.OnConflictIgnore());

//Equivalent to: 
db.InsertAll(rows, dbCmd => dbCmd.OnConflict(ConflictResolution.Ignore));
```

In this case it will ignore any conflicts that occurs and continue inserting the remaining rows in SQLite, MySql and PostgreSQL, whilst in SQL Server it's a NOOP.

SQLite offers [additional fine-grained behavior](https://sqlite.org/lang_conflict.html) that can be specified for when a conflict occurs:

 - ROLLBACK
 - ABORT
 - FAIL
 - IGNORE
 - REPLACE

### GetTableNames and GetTableNamesWithRowCounts APIs

As the queries for retrieving table names can vary amongst different RDBMS's, we've abstracted their implementations behind uniform APIs
where you can now get a list of table names and their row counts for all supported RDBMS's with:

```csharp
List<string> tableNames = db.GetTableNames();

List<KeyValuePair<string,long>> tableNamesWithRowCounts = db.GetTableNamesWithRowCounts();
```

> `*Async` variants also available

Both APIs can be called with an optional `schema` if you only want the tables for a specific schema.
It defaults to using the more efficient RDBMS APIs, which if offered typically returns an approximate estimate of rowcounts in each table. 

If you need exact table row counts, you can specify `live:true`:

```csharp
var tablesWithRowCounts = db.GetTableNamesWithRowCounts(live:true);
```

### Modify Custom Schema

OrmLite provides Typed APIs for modifying Table Schemas that makes it easy to inspect the state of an 
RDBMS Table which can be used to determine what modifications you want on it, e.g:

```csharp
class Poco 
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Ssn { get; set; }
}

db.DropTable<Poco>();
db.TableExists<Poco>(); //= false

db.CreateTable<Poco>(); 
db.TableExists<Poco>(); //= true

db.ColumnExists<Poco>(x => x.Ssn); //= true
db.DropColumn<Poco>(x => x.Ssn);
db.ColumnExists<Poco>(x => x.Ssn); //= false
```

In a future version of your Table POCO you can use `ColumnExists` to detect which columns haven't been 
added yet, then use `AddColumn` to add it, e.g:

```csharp
class Poco 
{
    public int Id { get; set; }
    public string Name { get; set; }

    [Default(0)]
    public int Age { get; set; }
}

if (!db.ColumnExists<Poco>(x => x.Age)) //= false
    db.AddColumn<Poco>(x => x.Age);

db.ColumnExists<Poco>(x => x.Age); //= true
```

#### Modify Schema APIs

Additional Modify Schema APIs available in OrmLite include:

 - `AlterTable`
 - `AddColumn`
 - `AlterColumn`
 - `ChangeColumnName`
 - `DropColumn`
 - `AddForeignKey`
 - `DropForeignKey`
 - `CreateIndex`
 - `DropIndex`

### Typed `Sql.Cast()` SQL Modifier

The `Sql.Cast()` provides a cross-database abstraction for casting columns or expressions in SQL queries, e.g:

```csharp
db.Insert(new SqlTest { Value = 123.456 });

var results = db.Select<(int id, string text)>(db.From<SqlTest>()
    .Select(x => new {
        x.Id,
        text = Sql.Cast(x.Id, Sql.VARCHAR) + " : " + Sql.Cast(x.Value, Sql.VARCHAR) + " : " 
             + Sql.Cast("1 + 2", Sql.VARCHAR) + " string"
    }));

results[0].text //= 1 : 123.456 : 3 string
```

### Typed `Column<T>` and `Table<T>` APIs

You can use the `Column<T>` and `Table<T>()` methods to resolve the quoted names of a Column or Table within SQL Fragments (taking into account any configured aliases or naming strategies). 

Usage Example of the new APIs inside a `CustomJoin()` expression used to join on a custom SELECT expression:

```csharp
q.CustomJoin($"LEFT JOIN (SELECT {q.Column<Job>(x => x.Id)} ...")
q.CustomJoin($"LEFT JOIN (SELECT {q.Column<Job>(nameof(Job.Id))} ...")

q.CustomJoin($"LEFT JOIN (SELECT {q.Column<Job>(x => x.Id, tablePrefix:true)} ...")
//Equivalent to:
q.CustomJoin($"LEFT JOIN (SELECT {q.Table<Job>()}.{q.Column<Job>(x => x.Id)} ...")

q.Select($"{q.Column<Job>(x => x.Id)} as JobId, {q.Column<Task>(x => x.Id)} as TaskId")
//Equivalent to:
q.Select<Job,Task>((j,t) => new { JobId = j.Id, TaskId = t.Id })
```

### DB Parameter API's

To enable even finer-grained control of parameterized queries we've added new overloads that take a collection of IDbDataParameter's:

```csharp
List<T> Select<T>(string sql, IEnumerable<IDbDataParameter> sqlParams)
T Single<T>(string sql, IEnumerable<IDbDataParameter> sqlParams)
T Scalar<T>(string sql, IEnumerable<IDbDataParameter> sqlParams)
List<T> Column<T>(string sql, IEnumerable<IDbDataParameter> sqlParams)
IEnumerable<T> ColumnLazy<T>(string sql, IEnumerable<IDbDataParameter> sqlParams)
HashSet<T> ColumnDistinct<T>(string sql, IEnumerable<IDbDataParameter> sqlParams)
Dictionary<K, List<V>> Lookup<K, V>(string sql, IEnumerable<IDbDataParameter> sqlParams)
List<T> SqlList<T>(string sql, IEnumerable<IDbDataParameter> sqlParams)
List<T> SqlColumn<T>(string sql, IEnumerable<IDbDataParameter> sqlParams)
T SqlScalar<T>(string sql, IEnumerable<IDbDataParameter> sqlParams)
```

> Including Async equivalents for each of the above Sync API's.

The new API's let you execute parameterized SQL with finer-grained control over the `IDbDataParameter` used, e.g:

```csharp
IDbDataParameter pAge = db.CreateParam("age", 40, dbType:DbType.Int16);
db.Select<Person>("SELECT * FROM Person WHERE Age > @pAge", new[] { pAge });
```

The new `CreateParam()` extension method above is a useful helper for creating custom IDbDataParameter's.

### Customize null values

The new `OrmLiteConfig.OnDbNullFilter` lets you to replace DBNull values with a custom value, so you could convert all `null` strings to be populated with `"NULL"` using:

```csharp
OrmLiteConfig.OnDbNullFilter = fieldDef => 
    fieldDef.FieldType == typeof(string)
        ? "NULL"
        : null;
```

### Logging an Introspection

One way to see what queries OrmLite generates is to enable a **debug** enabled logger, e.g:

```csharp
LogManager.LogFactory = new ConsoleLogFactory(debugEnabled:true);
```

Where it will log the generated SQL and Params OrmLite executes to the Console.

### BeforeExecFilter and AfterExecFilter filters

An alternative to debug logging which can easily get lost in the noisy stream of other debug messages is to use the `BeforeExecFilter` and `AfterExecFilter` filters where you can inspect executed commands with a custom lambda expression before and after each query is executed. So if one of your a queries are failing you can put a breakpoint in `BeforeExecFilter` to inspect the populated `IDbCommand` object before it's executed or use the `.GetDebugString()` extension method for an easy way to print the Generated SQL and DB Params to the Console:

```csharp
OrmLiteConfig.BeforeExecFilter = dbCmd => Console.WriteLine(dbCmd.GetDebugString());

//OrmLiteConfig.AfterExecFilter = dbCmd => Console.WriteLine(dbCmd.GetDebugString());
```

#### Output Generated SQL

You can use `OrmLiteUtils.PrintSql()` for the common debugging task of viewing the generated SQL OrmLite executes:

```csharp
OrmLiteUtils.PrintSql();
```

To later disable logging use: 

```csharp
OrmLiteUtils.UnPrintSql();
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

### CaptureSqlFilter

Results filters makes it trivial to implement the `CaptureSqlFilter` which allows you to capture SQL Statements without running them.
[CaptureSqlFilter](https://github.com/ServiceStack/ServiceStack.OrmLite/blob/4c56bde197d07cfc78a80be06dd557732ecf68fa/src/ServiceStack.OrmLite/OrmLiteResultsFilter.cs#L321) 
is just a simple Results Filter which can be used to quickly found out what SQL your DB calls generate by surrounding DB access in a using scope, e.g:

```csharp
using (var captured = new CaptureSqlFilter())
using (var db = OpenDbConnection())
{
    db.Where<Person>(new { Age = 27 });

    captured.SqlStatements[0].PrintDump();
}
```

Emits the Executed SQL along with any DB Parameters: 

    {
        Sql: "SELECT ""Id"", ""FirstName"", ""LastName"", ""Age"" FROM ""Person"" WHERE ""Age"" = @Age",
        Parameters: 
        {
            Age: 27
        }
    }

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

    var rowsInserted = db.Count<PocoTable>(x => x.Name == "Multiplicity"); //3
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
    if (row is IAudit auditRow)
        auditRow.CreatedDate = auditRow.ModifiedDate = DateTime.UtcNow;
};

OrmLiteConfig.UpdateFilter = (dbCmd, row) => {
    if (row is IAudit auditRow)
        auditRow.ModifiedDate = DateTime.UtcNow;
};
```

Which will ensure that the `CreatedDate` and `ModifiedDate` fields are populated on every insert and update.

### Validation Example

The filters can also be used for validation where throwing an exception will prevent the operation and bubble the exception, e.g:

```csharp
OrmLiteConfig.InsertFilter = OrmLiteConfig.UpdateFilter = (dbCmd, row) => {
    if (row is IAudit auditRow && auditRow.ModifiedBy == null)
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

### CustomSelect Attribute

The new `[CustomSelect]` can be used to define properties you want populated from a Custom SQL Function or 
Expression instead of a normal persisted column, e.g:

```csharp
public class Block
{
    public int Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    [CustomSelect("Width * Height")]
    public int Area { get; set; }

    [Default(OrmLiteVariables.SystemUtc)]
    public DateTime CreatedDate { get; set; }

    [CustomSelect("FORMAT(CreatedDate, 'yyyy-MM-dd')")]
    public string DateFormat { get; set; }
}

db.Insert(new Block { Id = 1, Width = 10, Height = 5 });

var block = db.SingleById<Block>(1);

block.Area.Print(); //= 50

block.DateFormat.Print(); //= 2016-06-08 (SQL Server)
```

### Order by dynamic expressions

The `[CustomSelect]` attribute can be used to populate a property with a dynamic SQL Expression instead of an existing column, e.g:

```csharp
public class FeatureRequest
{
    public int Id { get; set; }
    public int Up { get; set; }
    public int Down { get; set; }

    [CustomSelect("1 + Up - Down")]
    public int Points { get; set; }
}
```

You can also order by the SQL Expression by referencing the property as you would a normal column. By extension this feature now also works in AutoQuery where you can [select it in a partial result set](http://docs.servicestack.net/autoquery-rdbms#custom-fields) and order the results by using its property name, e.g:

    /features?fields=id,points&orderBy=points

### Custom SQL Fragments

The `Sql.Custom()` API lets you use raw SQL Fragments in Custom `.Select()` expressions, e.g:

```csharp
var q = db.From<Table>()
    .Select(x => new {
        FirstName = x.FirstName,
        LastName = x.LastName,
        Initials = Sql.Custom("CONCAT(LEFT(FirstName,1), LEFT(LastName,1))")
    });
```

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

    [CustomField(OrmLiteVariables.MaxText)]        //= {MAX_TEXT}
    public string MaxText { get; set; }

    [CustomField(OrmLiteVariables.MaxTextUnicode)] //= {NMAX_TEXT}
    public string MaxUnicodeText { get; set; }
}

db.CreateTable<PocoTable>(); 
```

Generates and executes the following SQL in SQL Server:

```sql
CREATE TABLE "PocoTable" 
(
  "Id" INTEGER PRIMARY KEY, 
  "CharColumn" CHAR(20) NULL, 
  "DecimalColumn" DECIMAL(18,4) NULL, 
  "MaxText" VARCHAR(MAX) NULL, 
  "MaxUnicodeText" NVARCHAR(MAX) NULL 
); 
```

> OrmLite replaces any variable placeholders with the value in each RDBMS DialectProvider's `Variables` Dictionary.

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

OrmLite's Expression support satisfies the most common RDBMS queries with a strong-typed API. 
For more complex queries you can easily fall back to raw SQL where the Custom SQL API's 
let you to map custom SqlExpressions into different responses:

```csharp
var q = db.From<Person>()
          .Where(x => x.Age < 50)
          .Select("*");
List<Person> results = db.SqlList<Person>(q);

List<Person> results = db.SqlList<Person>(
    "SELECT * FROM Person WHERE Age < @age", new { age=50});

List<string> results = db.SqlColumn<string>(db.From<Person>().Select(x => x.LastName));
List<string> results = db.SqlColumn<string>("SELECT LastName FROM Person");

HashSet<int> results = db.ColumnDistinct<int>(db.From<Person>().Select(x => x.Age));
HashSet<int> results = db.ColumnDistinct<int>("SELECT Age FROM Person");

var q = db.From<Person>()
          .Where(x => x.Age < 50)
          .Select(Sql.Count("*"));
int result = db.SqlScalar<int>(q);
int result = db.SqlScalar<int>("SELCT COUNT(*) FROM Person WHERE Age < 50");
```

### Custom Insert and Updates

```csharp
Db.ExecuteSql("INSERT INTO page_stats (ref_id, fav_count) VALUES (@refId, @favCount)",
              new { refId, favCount })

//Async:
Db.ExecuteSqlAsync("UPDATE page_stats SET view_count = view_count + 1 WHERE id = @id", new { id })
```

### INSERT INTO SELECT

You can use OrmLite's Typed `SqlExpression` to create a subselect expression that you can use to create and execute a 
typed **INSERT INTO SELECT** `SqlExpression` with:

```csharp
var q = db.From<User>()
    .Where(x => x.UserName == "UserName")
    .Select(x => new {
        x.UserName, 
        x.Email, 
        GivenName = x.FirstName, 
        Surname = x.LastName, 
        FullName = x.FirstName + " " + x.LastName
    });

var id = db.InsertIntoSelect<CustomUser>(q)
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

## Foreign Key attribute for referential actions on Update/Deletes

Creating a foreign key in OrmLite can be done by adding `[References(typeof(ForeignKeyTable))]` on the relation property,
which will result in OrmLite creating the Foreign Key relationship when it creates the DB table with `db.CreateTable<Poco>`.

Additional fine-grain options and behaviour are available in the `[ForeignKey]` attribute which will let you specify the desired behaviour when deleting or updating related rows in Foreign Key tables. 

An example of a table with the different available options:

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

### System Variables and Default Values

To provide richer support for non-standard default values, each RDBMS Dialect Provider contains a 
`OrmLiteDialectProvider.Variables` placeholder dictionary for storing common, but non-standard RDBMS functionality. 
We can use this to declaratively define non-standard default values that works across all supported RDBMS's 
like automatically populating a column with the RDBMS UTC Date when Inserted with a `default(T)` Value: 

```csharp
public class Poco
{
    [Default(OrmLiteVariables.SystemUtc)]  //= {SYSTEM_UTC}
    public DateTime CreatedTimeUtc { get; set; }
}
```

OrmLite variables need to be surrounded with `{}` braces to identify that it's a placeholder variable, e.g `{SYSTEM_UTC}`.

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
        $"~/App_Data/{shardId}.sqlite".MapAbsolutePath(),                //Connection String
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

![Data dump of Robot Shard #1](https://raw.githubusercontent.com/ServiceStack/docs/master/docs/images/ormlite/robots-shard0.sqlite.jpg)

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

[![SQL Server Management Studio results](https://raw.githubusercontent.com/ServiceStack/docs/master/docs/images/ormlite/ormlite-example.png)](https://raw.githubusercontent.com/ServiceStack/docs/master/docs/images/ormlite/ormlite-example.png)

Notice the POCO types are stored in the [very fast](https://github.com/ServiceStackV3/mythz_blog/blob/master/pages/176.md) 
and [Versatile](https://github.com/ServiceStackV3/mythz_blog/blob/master/pages/314.md) 
[JSV Format](https://docs.servicestack.net/jsv-format) which although hard to do - 
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

  var trainsAreUs = db.Single<Shipper>("ShipperTypeId = @Id", new { trainsType.Id });
  Assert.That(trainsAreUs.CompanyName, Is.EqualTo("Trains R Us"));
  Assert.That(db.Select<Shipper>("CompanyName = @company OR Phone = @phone", 
        new { company = "Trains R Us", phone = "555-UNICORNS" }), Has.Count.EqualTo(2));
  Assert.That(db.Select<Shipper>("ShipperTypeId = @Id", new { planesType.Id }), Has.Count.EqualTo(2));

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
var partialColumns = db.Select<SubsetOfShipper>(typeof(Shipper), 
    "ShipperTypeId = @Id", new { planesType.Id });
Assert.That(partialColumns, Has.Count.EqualTo(2));

//Select into another POCO class that matches sql
var rows = db.Select<ShipperTypeCount>(
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

### Soft Deletes

Select Filters let you specify a custom `SelectFilter` that lets you modify queries that use `SqlExpression<T>` before they're executed. This could be used to make working with "Soft Deletes" Tables easier where it can be made to apply a custom `x.IsDeleted != true` condition on every `SqlExpression`.

By either using a `SelectFilter` on concrete POCO Table Types, e.g:

```csharp
SqlExpression<Table1>.SelectFilter = q => q.Where(x => x.IsDeleted != true);
SqlExpression<Table2>.SelectFilter = q => q.Where(x => x.IsDeleted != true);
```

Or alternatively using generic delegate that applies to all SqlExpressions, but you'll only have access to a 
`IUntypedSqlExpression` which offers a limited API surface area but will still let you execute a custom filter 
for all `SqlExpression<T>` that could be used to add a condition for all tables implementing a custom 
`ISoftDelete` interface with:

```csharp
OrmLiteConfig.SqlExpressionSelectFilter = q =>
{
    if (q.ModelDef.ModelType.HasInterface(typeof(ISoftDelete)))
    {
        q.Where<ISoftDelete>(x => x.IsDeleted != true);
    }
};
```

Both solutions above will transparently add the `x.IsDeleted != true` to all `SqlExpression<T>` based queries
so it only returns results which aren't `IsDeleted` from any of queries below:

```csharp
var results = db.Select(db.From<Table>());
var result = db.Single(db.From<Table>().Where(x => x.Name == "foo"));
var result = db.Single(x => x.Name == "foo");
```

### Check Constraints

OrmLite includes support for [SQL Check Constraints](https://en.wikipedia.org/wiki/Check_constraint) which will create your Table schema with the `[CheckConstraint]` specified, e.g:

```csharp
public class Table
{
    [AutoIncrement]
    public int Id { get; set; }

    [Required]
    [CheckConstraint("Age > 1")]
    public int Age { get; set; }

    [CheckConstraint("Name IS NOT NULL")]
    public string Name { get; set; }
}
```

### Bitwise operators

The Typed SqlExpression bitwise operations support depends on the RDBMS used.

E.g. all RDBMS's support Bitwise `And` and `Or` operators:

```csharp
db.Select<Table>(x => (x.Flags | 2) == 3);
db.Select<Table>(x => (x.Flags & 2) == 2);
```

All RDBMS Except for SQL Server support bit shift operators:

```csharp
db.Select<Table>(x => (x.Flags << 1) == 4);
db.Select<Table>(x => (x.Flags >> 1) == 1);
```

Whilst only SQL Server and MySQL Support Exclusive Or:

```csharp
db.Select<Table>(x => (x.Flags ^ 2) == 3);
```

## SQL Server Features

### Memory Optimized Tables

OrmLite allows access to many advanced SQL Server features including 
[Memory-Optimized Tables](https://msdn.microsoft.com/en-us/library/dn133165.aspx) where you can tell 
SQL Server to maintain specific tables in Memory using the `[SqlServerMemoryOptimized]` attribute, e.g:

```csharp
[SqlServerMemoryOptimized(SqlServerDurability.SchemaOnly)]
public class SqlServerMemoryOptimizedCacheEntry : ICacheEntry
{
    [PrimaryKey]
    [StringLength(StringLengthAttribute.MaxText)]
    [SqlServerBucketCount(10000000)]
    public string Id { get; set; }
    [StringLength(StringLengthAttribute.MaxText)]
    public string Data { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
```

The `[SqlServerBucketCount]` attribute can be used to 
[configure the bucket count for a hash index](https://msdn.microsoft.com/en-us/library/mt706517.aspx#configuring_bucket_count)
whilst the new `[SqlServerCollate]` attribute can be used to specify an SQL Server collation.

## PostgreSQL Features

### PostgreSQL Rich Data Types

The `[PgSql*]` specific attributes lets you use attributes to define PostgreSQL rich data types, e.g:

```csharp
public class MyPostgreSqlTable
{
    [PgSqlJson]
    public List<Poco> AsJson { get; set; }

    [PgSqlJsonB]
    public List<Poco> AsJsonB { get; set; }

    [PgSqlTextArray]
    public string[] AsTextArray { get; set; }

    [PgSqlIntArray]
    public int[] AsIntArray { get; set; }

    [PgSqlBigIntArray]
    public long[] AsLongArray { get; set; }
}
```

By default all arrays of .NET's built-in **numeric**, **string** and **DateTime** types will be stored in PostgreSQL array types:

```csharp
public class Table
{
    public Guid Id { get; set; }

    public int[] Ints { get; set; }
    public long[] Longs { get; set; }
    public float[] Floats { get; set; }
    public double[] Doubles { get; set; }
    public decimal[] Decimals { get; set; }
    public string[] Strings { get; set; }
    public DateTime[] DateTimes { get; set; }
    public DateTimeOffset[] DateTimeOffsets { get; set; }
}
```

You can opt-in to annotate other collections like `List<T>` to also be stored in array types by annotating them with `[Pgsql*]` attributes, e.g:

```csharp
public class Table
{
    public Guid Id { get; set; }

    [PgSqlIntArray]
    public List<int> ListInts { get; set; }
    [PgSqlBigIntArray]
    public List<long> ListLongs { get; set; }
    [PgSqlFloatArray]
    public List<float> ListFloats { get; set; }
    [PgSqlDoubleArray]
    public List<double> ListDoubles { get; set; }
    [PgSqlDecimalArray]
    public List<decimal> ListDecimals { get; set; }
    [PgSqlTextArray]
    public List<string> ListStrings { get; set; }
    [PgSqlTimestamp]
    public List<DateTime> ListDateTimes { get; set; }
    [PgSqlTimestampTz]
    public List<DateTimeOffset> ListDateTimeOffsets { get; set; }
}
```

Alternatively if you **always** want `List<T>` stored in Array types, you can register them in the `PostgreSqlDialect.Provider`:

```csharp
PostgreSqlDialect.Provider.RegisterConverter<List<string>>(new PostgreSqlStringArrayConverter());
PostgreSqlDialect.Provider.RegisterConverter<List<int>>(new PostgreSqlIntArrayConverter());
PostgreSqlDialect.Provider.RegisterConverter<List<long>>(new PostgreSqlLongArrayConverter());
PostgreSqlDialect.Provider.RegisterConverter<List<float>>(new PostgreSqlFloatArrayConverter());
PostgreSqlDialect.Provider.RegisterConverter<List<double>>(new PostgreSqlDoubleArrayConverter());
PostgreSqlDialect.Provider.RegisterConverter<List<decimal>>(new PostgreSqlDecimalArrayConverter());
PostgreSqlDialect.Provider.RegisterConverter<List<DateTime>>(new PostgreSqlDateTimeTimeStampArrayConverter());
PostgreSqlDialect.Provider.RegisterConverter<List<DateTimeOffset>>(new PostgreSqlDateTimeOffsetTimeStampTzArrayConverter());
```

### Hstore support

To use `hstore`, its extension needs to be enabled in your PostgreSQL RDBMS by running:

    CREATE EXTENSION hstore;

Which can then be enabled in OrmLite with:

```csharp
PostgreSqlDialect.Instance.UseHstore = true;
```

Where it will now store **string Dictionaries** in `Hstore` columns:

```csharp
public class TableHstore
{
    public int Id { get; set; }

    public Dictionary<string,string> Dictionary { get; set; }
    public IDictionary<string,string> IDictionary { get; set; }
}

db.DropAndCreateTable<TableHstore>();

db.Insert(new TableHstore
{
    Id = 1,
    Dictionary = new Dictionary<string, string> { {"A", "1"} },
    IDictionary = new Dictionary<string, string> { {"B", "2"} },
});
```

Where they can than be queried in postgres using [Hstore SQL Syntax](https://www.postgresql.org/docs/9.0/hstore.html):

```csharp
db.Single(db.From<PostgreSqlTypes>().Where("dictionary -> 'A' = '1'")).Id //= 1
```

Thanks to [@cthames](https://forums.servicestack.net/users/cthames/activity) for this feature.

### JSON data types

If you instead wanted to store arbitrary complex types in PostgreSQL's rich column types to enable deep querying in postgres, 
you'd instead annotate them with `[PgSqlJson]` or `[PgSqlJsonB]`, e.g:

```csharp
public class TableJson
{
    public int Id { get; set; }

    [PgSqlJson]
    public ComplexType ComplexTypeJson { get; set; }

    [PgSqlJsonB]
    public ComplexType ComplexTypeJsonb { get; set; }
}

db.Insert(new TableJson
{
    Id = 1,
    ComplexTypeJson = new ComplexType {
        Id = 2, SubType = new SubType { Name = "JSON" }
    },
    ComplexTypeJsonb = new ComplexType {
        Id = 3, SubType = new SubType { Name = "JSONB" }
    },
});
```

Where they can then be queried on the server with [JSON SQL Syntax and functions](https://www.postgresql.org/docs/9.3/functions-json.html):

```csharp
var result = db.Single<TableJson>("table_json->'SubType'->>'Name' = 'JSON'");
```

# Limitations 

### Single Primary Key

For simplicity, and to be able to have the same POCO class persisted in db4o, memcached, redis or on the filesystem (i.e. providers included in ServiceStack), each model must have a single primary key, by convention OrmLite expects it
to be `Id` although you use `[Alias("DbFieldName")]` attribute it map it to a column with a different name or use 
the `[PrimaryKey]` attribute to tell OrmLite to use a different property for the primary key.

You can still `SELECT` from these tables, you will just be unable to make use of APIs that rely on it, e.g. 
`Update` or `Delete` where the filter is implied (i.e. not specified), all the APIs that end with `ById`, etc.

### Optimize LIKE Searches

One of the primary goals of OrmLite is to expose and RDBMS agnostic Typed API Surface which will allow you
to easily switch databases, or access multiple databases at the same time with the same behavior.

One instance where this can have an impact is needing to use `UPPER()` in **LIKE** searches to enable 
case-insensitive **LIKE** queries across all RDBMS. The drawback of this is that LIKE Queries are not able 
to use any existing RDBMS indexes. We can disable this feature and return to the default RDBMS behavior with:

```csharp
OrmLiteConfig.StripUpperInLike = true;
```

Allowing all **LIKE** Searches in OrmLite or AutoQuery to use any available RDBMS Index.

## Oracle Provider Notes

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
