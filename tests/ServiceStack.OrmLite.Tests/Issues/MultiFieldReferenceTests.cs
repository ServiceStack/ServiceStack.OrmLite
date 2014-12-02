using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class Goal
    {
        [PrimaryKey]
        public long Id { get; set; }

        [Reference]
        public List<Item> Items { get; set; }
    }

    public class Item
    {
        [PrimaryKey]
        public long Id { get; set; }

        [ForeignKey(typeof(Goal))]
        public long AnyGoalId { get; set; }

        [Alias("CorrectGoalId")]
        [ForeignKey(typeof(Goal))]
        public long GoalId { get; set; }
    }

    public class AliasedCustomer
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }

        [Alias("_id_home_address")]
        [ForeignKey(typeof(AliasedCustomerAddress))]
        public int? HomeAddressId { get; set; }

        [Alias("_id_work_address")]
        [ForeignKey(typeof(AliasedCustomerAddress))]
        public int? WorkAddressId { get; set; }

        [Reference]
        public AliasedCustomerAddress HomeAddress { get; set; }

        [Reference]
        public AliasedCustomerAddress WorkAddress { get; set; }
    }

    public class AliasedCustomerAddress
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int AliasedCustomerId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    
    [TestFixture]
    public class MultiFieldReferenceTests
        : OrmLiteTestBase
    {
        [Test]
        public void Does_select_correct_reference_field()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<Item>();
                db.DropTable<Goal>();
                db.CreateTable<Goal>();
                db.CreateTable<Item>();

                var goal = new Goal { Id = 1 };
                db.Save(goal, references: true);

                var goalWithItems = new Goal
                {
                    Id = 2,
                    Items = new List<Item>
                    {
                        new Item { Id = 10, AnyGoalId = 1 },
                        new Item { Id = 11, AnyGoalId = 1 },
                    }
                };

                db.Save(goalWithItems, references: true);

                Assert.That(goalWithItems.Items[0].GoalId, Is.EqualTo(goalWithItems.Id));
                Assert.That(goalWithItems.Items[1].GoalId, Is.EqualTo(goalWithItems.Id));

                var dbGoals = db.LoadSelect<Goal>(x => x.Id == goalWithItems.Id).First();
                db.GetLastSql().Print();

                Assert.That(dbGoals.Items[0].GoalId, Is.EqualTo(goalWithItems.Id));
                Assert.That(dbGoals.Items[1].GoalId, Is.EqualTo(goalWithItems.Id));
            }
        }

        [Test]
        public void Does_fallback_to_reference_convention_when_alias_is_used()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<AliasedCustomer>();
                db.DropTable<AliasedCustomerAddress>();
                db.CreateTable<AliasedCustomerAddress>();
                db.CreateTable<AliasedCustomer>();

                var customer = new AliasedCustomer
                {
                    Name = "Name",
                    WorkAddress = new AliasedCustomerAddress
                    {
                        AddressLine1 = "1 Work Road",
                        Country = "UK",
                    },
                    HomeAddress = new AliasedCustomerAddress
                    {
                        AddressLine1 = "2 Home Street",
                        Country = "US",
                    }
                };

                db.Save(customer, references:true);

                db.Select<AliasedCustomer>().PrintDump();
                db.Select<AliasedCustomerAddress>().PrintDump();

                var dbCustomer = db.LoadSelect<AliasedCustomer>()[0];
                dbCustomer.PrintDump();

                Assert.That(dbCustomer.Name, Is.EqualTo("Name"));
                Assert.That(dbCustomer.WorkAddress, Is.Not.Null);
                Assert.That(dbCustomer.WorkAddress.Country, Is.EqualTo("UK"));
                Assert.That(dbCustomer.HomeAddress, Is.Not.Null);
                Assert.That(dbCustomer.HomeAddress.Country, Is.EqualTo("US"));
            }
        }

    }
}