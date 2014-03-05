using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Tests.UseCase;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    /// <summary>
    /// Example of adding reference types to a POCO that:
    ///   - Doesn't persist as complex type blob in OrmLite.
    ///   - Doesn't impact other queries using the POCO.
    ///   - Can save and load references independently from itself.
    ///   - Loaded references are serialized in Text serializers.
    /// </summary>
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

    public class LoadReferencesTests 
        : OrmLiteTestBase
    {
        private IDbConnection db;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            db = base.OpenDbConnection();
            db.DropTable<OrderDetail>();
            db.DropAndCreateTable<Order>();
            db.DropAndCreateTable<Customer>();
            db.DropAndCreateTable<CustomerAddress>();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            db.Dispose();
        }

        [Test]
        public void Does_not_include_complex_reference_type_in_sql()
        {
            db.Select<Customer>();
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"Name\" FROM \"Customer\""));
        }

        [Test]
        public void Can_Save_and_Load_References()
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

            db.Save(customer);

            Assert.That(customer.Id, Is.GreaterThan(0));
            Assert.That(customer.PrimaryAddress.CustomerId, Is.EqualTo(0));

            db.SaveReferences(customer, customer.PrimaryAddress);
            Assert.That(customer.PrimaryAddress.CustomerId, Is.EqualTo(customer.Id));

            db.SaveReferences(customer, customer.Orders);
            Assert.That(customer.Orders.All(x => x.CustomerId == customer.Id));

            var dbCustomer = db.LoadSingleById<Customer>(customer.Id);

            dbCustomer.PrintDump();

            Assert.That(dbCustomer.PrimaryAddress, Is.Not.Null);
            Assert.That(dbCustomer.Orders.Count, Is.EqualTo(2));
        }

        [Test] 
        public void Can_SaveAllReferences_then_Load_them()
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

            db.Save(customer, references:true);

            Assert.That(customer.Id, Is.GreaterThan(0));
            Assert.That(customer.PrimaryAddress.CustomerId, Is.EqualTo(customer.Id));
            Assert.That(customer.Orders.All(x => x.CustomerId == customer.Id));

            var dbCustomer = db.LoadSingleById<Customer>(customer.Id);

            dbCustomer.PrintDump();

            Assert.That(dbCustomer.PrimaryAddress, Is.Not.Null);
            Assert.That(dbCustomer.Orders.Count, Is.EqualTo(2));
        }
    }
}