using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.UseCase;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class LoadReferencesJoinTests
        : OrmLiteTestBase
    {
        private IDbConnection db;

        [TestFixtureSetUp]
        public new void TestFixtureSetUp()
        {
            db = base.OpenDbConnection();
            CustomerOrdersUseCase.DropTables(db); //Has conflicting 'Order' table

            db.DropAndCreateTable<Order>();
            db.DropAndCreateTable<Customer>();
            db.DropAndCreateTable<CustomerAddress>();
            db.DropAndCreateTable<Country>();
        }

        [SetUp]
        public void SetUp()
        {
            db.DeleteAll<Order>();
            db.DeleteAll<CustomerAddress>();
            db.DeleteAll<Customer>();
            db.DeleteAll<Country>();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            db.Dispose();
        }

        private Customer AddCustomerWithOrders()
        {
            var customer = new Customer
            {
                Name = "Customer 1",
                PrimaryAddress = new CustomerAddress
                {
                    AddressLine1 = "1 Australia Street",
                    Country = "Australia"
                },
                Orders = new[]
                {
                    new Order {LineItem = "Line 1", Qty = 1, Cost = 1.99m},
                    new Order {LineItem = "Line 2", Qty = 2, Cost = 2.99m},
                }.ToList(),
            };

            db.Save(customer, references: true);

            return customer;
        }

        public class FullCustomerInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string AddressLine1 { get; set; }
            public string City { get; set; }
            public string LineItem { get; set; }
            public decimal Cost { get; set; }
            public string CountryCode { get; set; }
        }

        [Test]
        public void Can_do_multiple_joins_with_SqlExpression()
        {
            AddCustomerWithOrders();

            var results = db.Select<FullCustomerInfo, Customer>(q => q
                .Join<Customer, CustomerAddress>()
                .Join<Customer, Order>());

            var costs = results.ConvertAll(x => x.Cost);
            Assert.That(costs, Is.EquivalentTo(new[] { 1.99m, 2.99m }));

            var expr = db.From<Customer>()
                .Join<Customer, CustomerAddress>()
                .Join<Customer, Order>();

            results = db.Select<FullCustomerInfo>(expr);

            costs = results.ConvertAll(x => x.Cost);
            Assert.That(costs, Is.EquivalentTo(new[] { 1.99m, 2.99m }));
        }

        [Test]
        public void Can_do_joins_with_wheres_using_SqlExpression()
        {
            AddCustomerWithOrders();

            var results = db.Select<FullCustomerInfo, Customer>(q => q
                .Join<Customer, CustomerAddress>()
                .Join<Customer, Order>((c, o) => c.Id == o.CustomerId && o.Cost < 2));

            var costs = results.ConvertAll(x => x.Cost);
            Assert.That(costs, Is.EquivalentTo(new[] { 1.99m }));

            var orders = db.Select<Order>(q => q
                .Join<Order, Customer>()
                .Join<Customer, CustomerAddress>()
                .Where(o => o.Cost < 2)
                .And<Customer>(c => c.Name == "Customer 1"));

            costs = orders.ConvertAll(x => x.Cost);
            Assert.That(costs, Is.EquivalentTo(new[] { 1.99m }));

            results = db.Select<FullCustomerInfo, Customer>(q => q
                .Join<Customer, CustomerAddress>()
                .Join<Customer, Order>()
                .Where<Order>(o => o.Cost < 2));

            costs = results.ConvertAll(x => x.Cost);
            Assert.That(costs, Is.EquivalentTo(new[] { 1.99m }));

            results = db.Select<FullCustomerInfo, Customer>(q => q
                .Join<Customer, CustomerAddress>()
                .Join<Customer, Order>()
                .Where<Order>(o => o.Cost < 2 || o.LineItem == "Line 2"));

            costs = results.ConvertAll(x => x.Cost);
            Assert.That(costs, Is.EquivalentTo(new[] { 1.99m, 2.99m }));

            var expr = db.From<Customer>()
                .Join<Customer, CustomerAddress>()
                .Join<Customer, Order>()
                .Where<Order>(o => o.Cost < 2 || o.LineItem == "Line 2");
            results = db.Select<FullCustomerInfo>(expr);

            costs = results.ConvertAll(x => x.Cost);
            Assert.That(costs, Is.EquivalentTo(new[] { 1.99m, 2.99m }));
        }

        [Test]
        public void Can_do_joins_with_complex_wheres_using_SqlExpression()
        {
            var customers = new[]
            {
                new Customer
                {
                    Name = "Customer 1",
                    PrimaryAddress = new CustomerAddress {
                        AddressLine1 = "1 Australia Street",
                        Country = "Australia"
                    },
                    Orders = new[] {
                        new Order { LineItem = "Line 1", Qty = 1, Cost = 1.99m },
                        new Order { LineItem = "Line 1", Qty = 2, Cost = 3.98m },
                        new Order { LineItem = "Line 2", Qty = 1, Cost = 1.49m },
                        new Order { LineItem = "Line 2", Qty = 2, Cost = 2.98m },
                        new Order { LineItem = "Australia Flag", Qty = 1, Cost = 9.99m },
                    }.ToList(),
                },
                new Customer
                {
                    Name = "Customer 2",
                    PrimaryAddress = new CustomerAddress {
                        AddressLine1 = "2 Prospect Park",
                        Country = "USA"
                    },
                    Orders = new[] {
                        new Order { LineItem = "USA", Qty = 1, Cost = 20m },
                    }.ToList(),
                },
            };

            customers.Each(c =>
                db.Save(c, references: true));

            db.Insert(
                new Country { CountryName = "Australia", CountryCode = "AU" },
                new Country { CountryName = "USA", CountryCode = "US" });

            var results = db.Select<FullCustomerInfo, Customer>(q => q
                .Join<CustomerAddress>() //implicit
                .Join<Customer, Order>() //explicit
                .Where(c => c.Name == "Customer 1")
                .And<Order>(o => o.Cost < 2)
                .Or<Order>(o => o.LineItem == "Australia Flag"));

            var costs = results.ConvertAll(x => x.Cost);
            Assert.That(costs, Is.EquivalentTo(new[] { 1.99m, 1.49m, 9.99m }));

            //Same as above using using db.From<Customer>()
            results = db.Select<FullCustomerInfo>(db.From<Customer>()
                .Join<CustomerAddress>() //implicit
                .Join<Customer, Order>() //explicit
                .Where(c => c.Name == "Customer 1")
                .And<Order>(o => o.Cost < 2)
                .Or<Order>(o => o.LineItem == "Australia Flag"));

            costs = results.ConvertAll(x => x.Cost);
            Assert.That(costs, Is.EquivalentTo(new[] { 1.99m, 1.49m, 9.99m }));

            results = db.Select<FullCustomerInfo, Customer>(q => q
                .Join<Customer, CustomerAddress>()
                .Join<Customer, Order>()
                .Where(c => c.Name == "Customer 2")
                .And<CustomerAddress, Order>((a, o) => a.Country == o.LineItem));

            costs = results.ConvertAll(x => x.Cost);
            Assert.That(costs, Is.EquivalentTo(new[] { 20m }));

            var countryResults = db.Select<FullCustomerInfo>(db.From<Customer>()
                .Join<CustomerAddress>()                     //implicit join with Customer
                .Join<Order>((c, o) => c.Id == o.CustomerId) //explicit join condition
                .Join<CustomerAddress, Country>((ca, c) => ca.Country == c.CountryName)
                .Where(c => c.Name == "Customer 2")          //implicit condition with Customer
                .And<CustomerAddress, Order>((a, o) => a.Country == o.LineItem));

            db.GetLastSql().Print();

            costs = countryResults.ConvertAll(x => x.Cost);
            Assert.That(costs, Is.EquivalentTo(new[] { 20m }));
            Assert.That(countryResults.ConvertAll(x => x.CountryCode), Is.EquivalentTo(new[] { "US" }));
        }

        [Test]
        public void Can_do_LeftJoins_using_SqlExpression()
        {
            var customers = new[]
            {
                new Customer
                {
                    Name = "Customer 1",
                    PrimaryAddress = new CustomerAddress {
                        AddressLine1 = "1 Australia Street",
                        Country = "Australia"
                    },
                },
                new Customer
                {
                    Name = "Customer 2",
                    PrimaryAddress = new CustomerAddress {
                        AddressLine1 = "2 America Street",
                        Country = "USA"
                    },
                },
                new Customer
                {
                    Name = "Customer 3",
                    PrimaryAddress = new CustomerAddress {
                        AddressLine1 = "3 Canada Street",
                        Country = "Canada"
                    },
                },
            };

            customers.Each(c =>
                db.Save(c, references: true));

            db.Insert(
                new Country { CountryName = "Australia", CountryCode = "AU" },
                new Country { CountryName = "USA", CountryCode = "US" },
                new Country { CountryName = "Italy", CountryCode = "IT" },
                new Country { CountryName = "Spain", CountryCode = "ED" });

            //Normal Join
            var dbCustomers = db.Select<Customer>(q => q
                .Join<CustomerAddress>()
                .Join<CustomerAddress, Country>((ca, c) => ca.Country == c.CountryName));

            Assert.That(dbCustomers.Count, Is.EqualTo(2));

            //Left Join
            dbCustomers = db.Select<Customer>(q => q
                .Join<CustomerAddress>()
                .LeftJoin<CustomerAddress, Country>((ca, c) => ca.Country == c.CountryName));

            Assert.That(dbCustomers.Count, Is.EqualTo(3));

            //Warning: Right and Full Joins are not implemented by Sqlite3. Avoid if possible.
            var dbCountries = db.Select<Country>(q => q
                .LeftJoin<CustomerAddress>((c, ca) => ca.Country == c.CountryName)
                .LeftJoin<CustomerAddress, Customer>());

            Assert.That(dbCountries.Count, Is.EqualTo(4));

            var dbAddresses = db.Select<CustomerAddress>(q => q
                .LeftJoin<Country>((ca, c) => ca.Country == c.CountryName)
                .LeftJoin<CustomerAddress, Customer>());

            Assert.That(dbAddresses.Count, Is.EqualTo(3));
        }

        [Test]
        public void Can_Join_on_matching_Alias_convention()
        {
            db.DropAndCreateTable<AliasedCustomer>();
            db.DropAndCreateTable<AliasedCustomerAddress>();

            var customers = new[]
            {
                new AliasedCustomer
                {
                    Name = "Customer 1",
                    PrimaryAddress = new AliasedCustomerAddress {
                        AddressLine1 = "1 Australia Street",
                        Country = "Australia"
                    },
                },
                new AliasedCustomer
                {
                    Name = "Customer 2",
                    PrimaryAddress = new AliasedCustomerAddress {
                        AddressLine1 = "2 America Street",
                        Country = "USA"
                    },
                },
                new AliasedCustomer
                {
                    Name = "Customer 3",
                    PrimaryAddress = new AliasedCustomerAddress {
                        AddressLine1 = "3 Canada Street",
                        Country = "Canada"
                    },
                },
            };

            customers.Each(c =>
                db.Save(c, references: true));

            db.Insert(
                new Country { CountryName = "Australia", CountryCode = "AU" },
                new Country { CountryName = "USA", CountryCode = "US" },
                new Country { CountryName = "Italy", CountryCode = "IT" },
                new Country { CountryName = "Spain", CountryCode = "ED" });

            //Normal Join
            var dbCustomers = db.Select<AliasedCustomer>(q => q
                .Join<AliasedCustomerAddress>()
                .Join<AliasedCustomerAddress, Country>((ca, c) => ca.Country == c.CountryName));

            Assert.That(dbCustomers.Count, Is.EqualTo(2));

            //Left Join
            dbCustomers = db.Select<AliasedCustomer>(q => q
                .Join<AliasedCustomerAddress>()
                .LeftJoin<AliasedCustomerAddress, Country>((ca, c) => ca.Country == c.CountryName));

            Assert.That(dbCustomers.Count, Is.EqualTo(3));

            //Warning: Right and Full Joins are not implemented by Sqlite3. Avoid if possible.
            var dbCountries = db.Select<Country>(q => q
                .LeftJoin<AliasedCustomerAddress>((c, ca) => ca.Country == c.CountryName)
                .LeftJoin<AliasedCustomerAddress, AliasedCustomer>());

            Assert.That(dbCountries.Count, Is.EqualTo(4));

            var dbAddresses = db.Select<AliasedCustomerAddress>(q => q
                .LeftJoin<Country>((ca, c) => ca.Country == c.CountryName)
                .LeftJoin<AliasedCustomerAddress, AliasedCustomer>());

            Assert.That(dbAddresses.Count, Is.EqualTo(3));
        }
    }
}