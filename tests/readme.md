# OrmLite Testing

The following describes some typical testing scenarios

## Docker Db instances

For integration testing, there is a docker-compose file that can spin up all versions of supported providers. See the readme in /src/docker for more details

## Test Basics

To create tests to run against one or more providers, inherit from `OrmLiteProvidersTestBase` 
and use the `TestFixtureOrmLiteAttribute` class.

```csharp
[TestFixtureOrmLite]
public class DbFeatures1 : OrmLiteProvidersTestBase 
{
    // Required Ctor, Dialects will be injected by TestFixture
    public DbFeatures1(Dialect dialect) : base(dialect)
    {
    }
    
    // Tests can be repeated for one or more providers
    [Test]
    public void Test1() 
    {
        // Current test dialect
        var dialect = base.Dialect;
    
        // current test dialectprovider
        var dialectProvider = base.DialectProvider;
        
        // current DbFactory
        var dbFactory = base.DbFactory;

        // open the correct db connection based on dialect 
        using(var db = OpenConnection())
        {
            // db agnostic tests
        }
    }
}
```

By default, the tests will run against in-memory instances of Sqlite.
This is set in `Config.DefaultDialects` and can be overridden either by changing
the value assigned or by setting an Environment Variable of the same name.

The accepted values come from the enum `TestDialect` and correspond to properties 
in the `FixtureDataProvider` class which will be injected into the class for each provider.

## Targeting specific providers

To run tests against specific providers, use `TestFixtureOrmLiteDialectsAttribute`.

```csharp
[TestFixtureOrmLiteDialects(TestDialect.SqlServer)]
public class SqlDbFeatures1 : OrmLiteProvidersTestBase 
{
    ...
    
    [Test]
    public void Test1() 
    {
        // Will execute for SqlServer provider and dialect versions

        // Current test dialect
        var dialect = base.Dialect;

        // current test dialectprovider
        var dialectProvider = base.DialectProvider

        // open the correct db connection based on dialect 
        using(var db = OpenConnection())
        {
            // db agnostic tests
        }
    }
}
```

## Excluding specific tests 

To exclude individual tests for one or more db providers, use the `IgnoreProviderAttribute`

```csharp
[Test]
[IgnoreProvider(Dialect.PostgreSql9 | Dialect.PostgreSql10, "ignore message to output")]
[IgnoreProvider(Dialect.AnySqlServer, "diff ignore message to output")]
public void Test1()
{
    // Test will not run for any dialects ignored above but any others 
}
``` 
