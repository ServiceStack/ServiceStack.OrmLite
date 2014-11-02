using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.UseCase;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class SqlExpressionTests
        : OrmLiteTestBase
    {
        [Test]
        public async Task Can_select_limit_on_Table_with_References_Async()
        {
            using (var db = OpenDbConnection())
            {
                CustomerOrdersUseCase.DropTables(db); //Has conflicting 'Order' table
                db.DropAndCreateTable<Order>();
                db.DropAndCreateTable<Customer>();
                db.DropAndCreateTable<CustomerAddress>();

                var customer1 = LoadReferencesTests.GetCustomerWithOrders("1");
                db.Save(customer1, references: true);

                var customer2 = LoadReferencesTests.GetCustomerWithOrders("2");
                db.Save(customer2, references: true);

                var results = await db.LoadSelectAsync<Customer>(q => q
                    .OrderBy(x => x.Id)
                    .Limit(1, 1));

                //db.GetLastSql().Print();

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Name, Is.EqualTo("Customer 2"));
                Assert.That(results[0].PrimaryAddress.AddressLine1, Is.EqualTo("2 Humpty Street"));
                Assert.That(results[0].Orders.Count, Is.EqualTo(2));

                results = await db.LoadSelectAsync<Customer>(q => q
                    .Join<CustomerAddress>()
                    .OrderBy(x => x.Id)
                    .Limit(1, 1));

                db.GetLastSql().Print();

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Name, Is.EqualTo("Customer 2"));
                Assert.That(results[0].PrimaryAddress.AddressLine1, Is.EqualTo("2 Humpty Street"));
                Assert.That(results[0].Orders.Count, Is.EqualTo(2));
            }
        }
         
    }
}