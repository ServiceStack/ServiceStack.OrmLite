using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;
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
            public string CustomerName { get; set; }
            public int CustomerAddressId { get; set; }
            public string AddressLine1 { get; set; }
            public string City { get; set; }
            public int OrderId { get; set; }
            public string LineItem { get; set; }
            public decimal Cost { get; set; }
            public decimal OrderCost { get; set; }
            public string CountryCode { get; set; }
        }

        public class MixedCustomerInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string AliasedCustomerName { get; set; }
            public string aliasedcustomername { get; set; }
            public int Q_CustomerId { get; set; }
            public int Q_CustomerAddressQ_CustomerId { get; set; }
            public string CountryName { get; set; }
            public int CountryId { get; set; }
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
            var customers = AddCustomersWithOrders();

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
            var orderIds = results.ConvertAll(x => x.OrderId);
            var expectedOrderIds = new[] { customers[0].Orders[0].Id, customers[0].Orders[2].Id, customers[0].Orders[4].Id };
            Assert.That(orderIds, Is.EquivalentTo(expectedOrderIds));

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

            costs = countryResults.ConvertAll(x => x.Cost);
            Assert.That(costs, Is.EquivalentTo(new[] { 20m }));
            Assert.That(countryResults.ConvertAll(x => x.CountryCode), Is.EquivalentTo(new[] { "US" }));
        }

        private Customer[] AddCustomersWithOrders()
        {
            var customers = new[]
            {
                new Customer
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
                        new Order {LineItem = "Line 1", Qty = 2, Cost = 3.98m},
                        new Order {LineItem = "Line 2", Qty = 1, Cost = 1.49m},
                        new Order {LineItem = "Line 2", Qty = 2, Cost = 2.98m},
                        new Order {LineItem = "Australia Flag", Qty = 1, Cost = 9.99m},
                    }.ToList(),
                },
                new Customer
                {
                    Name = "Customer 2",
                    PrimaryAddress = new CustomerAddress
                    {
                        AddressLine1 = "2 Prospect Park",
                        Country = "USA"
                    },
                    Orders = new[]
                    {
                        new Order {LineItem = "USA", Qty = 1, Cost = 20m},
                    }.ToList(),
                },
            };

            customers.Each(c =>
                db.Save(c, references: true));

            return customers;
        }

        [Test]
        public void Can_do_LeftJoins_using_SqlExpression()
        {
            AddCustomers();

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

        private void AddCustomers()
        {
            var customers = new[]
            {
                new Customer
                {
                    Name = "Customer 1",
                    PrimaryAddress = new CustomerAddress
                    {
                        AddressLine1 = "1 Australia Street",
                        Country = "Australia"
                    },
                },
                new Customer
                {
                    Name = "Customer 2",
                    PrimaryAddress = new CustomerAddress
                    {
                        AddressLine1 = "2 America Street",
                        Country = "USA"
                    },
                },
                new Customer
                {
                    Name = "Customer 3",
                    PrimaryAddress = new CustomerAddress
                    {
                        AddressLine1 = "3 Canada Street",
                        Country = "Canada"
                    },
                },
            };

            customers.Each(c =>
                db.Save(c, references: true));
        }

        [Test]
        public void Can_Join_on_matching_Alias_convention()
        {
            Country[] countries;
            AddAliasedCustomers(out countries);

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

        private AliasedCustomer[] AddAliasedCustomers(out Country[] countries)
        {
            db.DropAndCreateTable<AliasedCustomer>();
            db.DropAndCreateTable<AliasedCustomerAddress>();
            db.DropAndCreateTable<Country>();

            var customers = new[]
            {
                new AliasedCustomer
                {
                    Name = "Customer 1",
                    PrimaryAddress = new AliasedCustomerAddress
                    {
                        AddressLine1 = "1 Australia Street",
                        Country = "Australia"
                    },
                },
                new AliasedCustomer
                {
                    Name = "Customer 2",
                    PrimaryAddress = new AliasedCustomerAddress
                    {
                        AddressLine1 = "2 America Street",
                        Country = "USA"
                    },
                },
                new AliasedCustomer
                {
                    Name = "Customer 3",
                    PrimaryAddress = new AliasedCustomerAddress
                    {
                        AddressLine1 = "3 Canada Street",
                        Country = "Canada"
                    },
                },
            };

            customers.Each(c =>
                           db.Save(c, references: true));

            countries = new[]
            {
                new Country {CountryName = "Australia", CountryCode = "AU"},
                new Country {CountryName = "USA", CountryCode = "US"},
                new Country {CountryName = "Italy", CountryCode = "IT"},
                new Country {CountryName = "Spain", CountryCode = "ED"}
            };
            db.Save(countries);

            return customers;
        }

        [Test]
        public void Does_populate_custom_columns_based_on_property_convention()
        {
            // Reset auto ids
            db.DropAndCreateTable<Order>();
            db.DropAndCreateTable<CustomerAddress>();
            db.DropAndCreateTable<Customer>();

            var customer = AddCustomerWithOrders();

            var results = db.Select<FullCustomerInfo, Customer>(q => q
                .Join<Customer, CustomerAddress>()
                .Join<Customer, Order>());

            var addressIds = results.ConvertAll(x => x.CustomerAddressId);
            var expectedAddressIds = new[] { customer.PrimaryAddress.Id, customer.PrimaryAddress.Id };
            Assert.That(addressIds, Is.EquivalentTo(expectedAddressIds));

            var orderIds = results.ConvertAll(x => x.OrderId);
            var expectedOrderIds = new[] { customer.Orders[0].Id, customer.Orders[1].Id };
            Assert.That(orderIds, Is.EquivalentTo(expectedOrderIds));

            var customerNames = results.ConvertAll(x => x.CustomerName);
            Assert.That(customerNames, Is.EquivalentTo(new[] { "Customer 1", "Customer 1" }));

            var orderCosts = results.ConvertAll(x => x.OrderCost);
            Assert.That(orderCosts, Is.EquivalentTo(new[] { 1.99m, 2.99m }));

            var expr = db.From<Customer>()
                .Join<Customer, CustomerAddress>()
                .Join<Customer, Order>()
                .Where<Order>(o => o.Cost > 2);

            results = db.Select<FullCustomerInfo>(expr);

            addressIds = results.ConvertAll(x => x.CustomerAddressId);
            Assert.That(addressIds, Is.EquivalentTo(new[] { customer.PrimaryAddress.Id }));

            orderIds = results.ConvertAll(x => x.OrderId);
            Assert.That(orderIds, Is.EquivalentTo(new[] { customer.Orders[1].Id }));

            customerNames = results.ConvertAll(x => x.CustomerName);
            Assert.That(customerNames, Is.EquivalentTo(new[] { "Customer 1" }));

            orderCosts = results.ConvertAll(x => x.OrderCost);
            Assert.That(orderCosts, Is.EquivalentTo(new[] { 2.99m }));
        }

        [Test]
        public void Does_populate_custom_mixed_columns()
        {
            Country[] countries;
            var customers = AddAliasedCustomers(out countries);

            //Normal Join
            var results = db.Select<MixedCustomerInfo, AliasedCustomer>(q => q
                .Join<AliasedCustomerAddress>()
                .Join<AliasedCustomerAddress, Country>((ca, c) => ca.Country == c.CountryName));

            var customerNames = results.Map(x => x.Name);
            Assert.That(customerNames, Is.EquivalentTo(new[] { "Customer 1", "Customer 2" }));

            customerNames = results.Map(x => x.AliasedCustomerName);
            Assert.That(customerNames, Is.EquivalentTo(new[] { "Customer 1", "Customer 2" }));

            customerNames = results.Map(x => x.aliasedcustomername);
            Assert.That(customerNames, Is.EquivalentTo(new[] { "Customer 1", "Customer 2" }));

            var customerIds = results.Map(x => x.Q_CustomerId);
            Assert.That(customerIds, Is.EquivalentTo(new[] { customers[0].Id, customers[1].Id }));

            customerIds = results.Map(x => x.Q_CustomerAddressQ_CustomerId);
            Assert.That(customerIds, Is.EquivalentTo(new[] { customers[0].Id, customers[1].Id }));

            var countryNames = results.Map(x => x.CountryName);
            Assert.That(countryNames, Is.EquivalentTo(new[] { "Australia", "USA" }));

            var countryIds = results.Map(x => x.CountryId);
            Assert.That(countryIds, Is.EquivalentTo(new[] { countries[0].Id, countries[1].Id }));
        }

        [Test]
        public void Can_LeftJoin_and_select_empty_relation()
        {
            AddCustomerWithOrders();

            var customer = new Customer
            {
                Name = "Customer 2",
                PrimaryAddress = new CustomerAddress
                {
                    AddressLine1 = "2 America Street",
                    Country = "USA"
                },
            };

            db.Save(customer, references: true);

            var q = db.From<Customer>();
            q.LeftJoin<Order>()
             .Where<Order>(o => o.Id == null);

            var customers = db.Select(q);

            Assert.That(customers.Count, Is.EqualTo(1));
            Assert.That(customers[0].Name, Is.EqualTo("Customer 2"));
        }

        [Test]
        public void Can_load_list_of_references()
        {
            AddCustomersWithOrders();

            var results = db.LoadSelect<Customer>();
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.All(x => x.PrimaryAddress != null));
            Assert.That(results.All(x => x.Orders.Count > 0));

            var customer1 = results.First(x => x.Name == "Customer 1");
            Assert.That(customer1.PrimaryAddress.Country, Is.EqualTo("Australia"));
            Assert.That(customer1.Orders.Select(x => x.Cost),
                Is.EquivalentTo(new[] { 1.99m, 3.98m, 1.49m, 2.98m, 9.99m }));

            var customer2 = results.First(x => x.Name == "Customer 2");
            Assert.That(customer2.PrimaryAddress.Country, Is.EqualTo("USA"));
            Assert.That(customer2.Orders[0].LineItem, Is.EqualTo("USA"));

            results = db.LoadSelect<Customer>(q => q.Name == "Customer 1");
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].PrimaryAddress.Country, Is.EqualTo("Australia"));
            Assert.That(results[0].Orders.Select(x => x.Cost),
                Is.EquivalentTo(new[] { 1.99m, 3.98m, 1.49m, 2.98m, 9.99m }));
        }

        [Test]
        public void Can_join_on_references_attribute()
        {
            // Drop tables in order that FK allows
            db.DropTable<TABLE_3>();
            db.DropTable<TABLE_2>();
            db.DropTable<TABLE_1>();
            db.CreateTable<TABLE_1>();
            db.CreateTable<TABLE_2>();
            db.CreateTable<TABLE_3>();

            var id1 = db.Insert(new TABLE_1 { One = "A" }, selectIdentity: true);
            var id2 = db.Insert(new TABLE_1 { One = "B" }, selectIdentity: true);

            db.Insert(new TABLE_2 { Three = "C", TableOneKey = (int) id1 });

            var q = db.From<TABLE_1>()
                      .Join<TABLE_2>();
            var results = db.Select(q);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].One, Is.EqualTo("A"));

            var row3 = new TABLE_3 {
                Three = "3a",
                TableTwo = new TABLE_2 
                {
                    Three = "3b",
                    TableOneKey = (int)id1,
                }
            };
            db.Save(row3, references:true);

            Assert.That(row3.TableTwoKey, Is.EqualTo(row3.TableTwo.Id));

            row3 = db.LoadSingleById<TABLE_3>(row3.Id);
            Assert.That(row3.TableTwoKey, Is.EqualTo(row3.TableTwo.Id));
        }

        [Test]
        public void Can_load_references_with_OrderBy()
        {
            AddCustomersWithOrders();

            var customers = db.LoadSelect<Customer>(q => q.OrderBy(x => x.Name));
            var addresses = customers.Select(x => x.PrimaryAddress).ToList();
            var orders = customers.SelectMany(x => x.Orders).ToList();

            Assert.That(customers.Count, Is.EqualTo(2));
            Assert.That(addresses.Count, Is.EqualTo(2));
            Assert.That(orders.Count, Is.EqualTo(6));
        }

        [Test]
        public void Can_load_references_with_OrderBy_and_Paging()
        {
            //This version of MariaDB doesn't yet support 'LIMIT & IN/ALL/ANY/SOME subquery'
            if (Dialect == Dialect.MySql) return;

            db.DropTable<Parent>();
            db.DropTable<Child>();
            db.CreateTable<Child>();
            db.CreateTable<Parent>();

            db.Save(new Child { Id = 1, Value = "Lolz" });
            db.Insert(new Parent { Id = 1, ChildId = null });
            db.Insert(new Parent { Id = 2, ChildId = 1 });

            // Select the Parent.Id == 2.  LoadSelect should populate the child, but doesn't.
            var q = db.From<Parent>()
                .Take(1)
                .OrderByDescending<Parent>(p => p.Id);

            var results = db.LoadSelect(q);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Child, Is.Not.Null);
            Assert.That(results[0].Child.Value, Is.EqualTo("Lolz"));

            q = db.From<Parent>()
                .Skip(1)
                .OrderBy<Parent>(p => p.Id);

            results = db.LoadSelect(q);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Child, Is.Not.Null);
            Assert.That(results[0].Child.Value, Is.EqualTo("Lolz"));
            results.PrintDump();
        }
    }

    public class Parent
    {
        [PrimaryKey]
        public int Id { get; set; }

        [References(typeof(Child))]
        public int? ChildId { get; set; }

        [Reference]
        public Child Child { get; set; }
    }

    public class Child
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Value { get; set; }
    }

    [Alias("Table1")]
    public class TABLE_1 : IHasId<int>
    {
        [AutoIncrement]
        [Alias("Key")]
        public int Id { get; set; }

        [Alias("Ena")]
        public string One { get; set; }
    }

    [Alias("Table2")]
    public class TABLE_2 : IHasId<int>
    {
        [AutoIncrement]
        [Alias("Key")]
        public int Id { get; set; }

        [Alias("Tri")]
        public string Three { get; set; }

        [References(typeof(TABLE_1))]
        [Alias("Table1")]
        public int TableOneKey { get; set; }
    }

    [Alias("Table3")]
    public class TABLE_3 : IHasId<int>
    {
        [AutoIncrement]
        [Alias("Key")]
        public int Id { get; set; }

        [Alias("Tri")]
        public string Three { get; set; }

        [References(typeof(TABLE_2))]
        public int? TableTwoKey { get; set; }

        [Reference]
        public TABLE_2 TableTwo { get; set; }
    }
}