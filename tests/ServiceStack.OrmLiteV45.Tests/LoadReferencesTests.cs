using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Tests.UseCase;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class Customer
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }

        [Reference]
        public CustomerAddress PrimaryAddress { get; set; }

        [Reference]
        public List<Order> Orders { get; set; }
    }

    public class CustomerAddress
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int CustomerId { get; set; }
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
        public int CustomerId { get; set; }
        public string LineItem { get; set; }
        public int Qty { get; set; }
        public decimal Cost { get; set; }
    }

    public class Country
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
    }

    public class LoadReferencesTests
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
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            db.Dispose();
        }

        public static Customer GetCustomerWithOrders(string id = "1")
        {
            var customer = new Customer
            {
                Name = "Customer " + id,
                PrimaryAddress = new CustomerAddress
                {
                    AddressLine1 = id + " Humpty Street",
                    City = "Humpty Doo",
                    State = "Northern Territory",
                    Country = "Australia"
                },
                Orders = new[]
                    {
                        new Order {LineItem = "Line 1", Qty = 1, Cost = 1.99m},
                        new Order {LineItem = "Line 2", Qty = 2, Cost = 2.99m},
                    }.ToList(),
            };
            return customer;
        }

        [Test]
        public async Task Can_Save_and_Load_References_Async()
        {
            var customer = new Customer
            {
                Name = "Customer 1",
                PrimaryAddress = new CustomerAddress
                {
                    AddressLine1 = "1 Humpty Street",
                    City = "Humpty Doo",
                    State = "Northern Territory",
                    Country = "Australia"
                },
                Orders = new[] { 
                    new Order { LineItem = "Line 1", Qty = 1, Cost = 1.99m },
                    new Order { LineItem = "Line 2", Qty = 2, Cost = 2.99m },
                }.ToList(),
            };

            await db.SaveAsync(customer);

            Assert.That(customer.Id, Is.GreaterThan(0));
            Assert.That(customer.PrimaryAddress.CustomerId, Is.EqualTo(0));

            await db.SaveReferencesAsync(customer, customer.PrimaryAddress);
            Assert.That(customer.PrimaryAddress.CustomerId, Is.EqualTo(customer.Id));

            await db.SaveReferencesAsync(customer, customer.Orders);
            Assert.That(customer.Orders.All(x => x.CustomerId == customer.Id));

            var dbCustomer = await db.LoadSingleByIdAsync<Customer>(customer.Id);

            dbCustomer.PrintDump();

            Assert.That(dbCustomer.PrimaryAddress, Is.Not.Null);
            Assert.That(dbCustomer.Orders.Count, Is.EqualTo(2));
        }
    }
}