using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class CustomerFetch
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }

        [Reference]
        public CustomerAddress PrimaryAddress { get; set; }

        [Reference]
        public List<OrderFetch> Orders { get; set; }
    }

    public class CustomerAddress
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int CustomerFetchId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class OrderFetch
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int CustomerFetchId { get; set; }
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
            db.DropAndCreateTable<CustomerFetch>();
            db.DropAndCreateTable<CustomerAddress>();
            db.DropAndCreateTable<OrderFetch>();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            db.Dispose();
        }

        [Test]
        public void Does_not_include_complex_reference_type_in_sql()
        {
            db.Select<CustomerFetch>();
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"Id\", \"Name\" FROM \"CustomerFetch\""));
        }

        [Test]
        public void Can_Save_and_Load_References()
        {
            var customer = new CustomerFetch
                {
                    Name = "Customer 1",
                    PrimaryAddress = new CustomerAddress {
                        AddressLine1 = "1 Humpty Street",
                        City = "Humpty Doo",
                        State = "Northern Territory",
                        Country = "Australia"
                    },
                    Orders = new[] { 
                        new OrderFetch { LineItem = "Line 1", Qty = 1, Cost = 1.99m },
                        new OrderFetch { LineItem = "Line 2", Qty = 2, Cost = 2.99m },
                    }.ToList(),
                };

            db.Save(customer);

            Assert.That(customer.Id, Is.GreaterThan(0));
            Assert.That(customer.PrimaryAddress.CustomerFetchId, Is.EqualTo(0));

            db.SaveReference(customer, customer.PrimaryAddress);
            Assert.That(customer.PrimaryAddress.CustomerFetchId, Is.EqualTo(customer.Id));

            db.SaveReference(customer, customer.Orders);
            Assert.That(customer.Orders.All(x => x.CustomerFetchId == customer.Id));

            var dbCustomer = db.SingleById<CustomerFetch>(customer.Id);

            db.LoadReferences(dbCustomer);

            dbCustomer.PrintDump();
            
            Assert.That(dbCustomer.PrimaryAddress, Is.Not.Null);
            Assert.That(dbCustomer.Orders.Count, Is.EqualTo(2));
        }
    }
}