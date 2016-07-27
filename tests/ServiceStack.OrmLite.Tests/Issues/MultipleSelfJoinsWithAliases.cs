using System;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public interface IHaveTenantId
    {
        Guid TenantId { get; }
    }

    public class ContactIssue : IHaveTenantId
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    public class Sale : IHaveTenantId
    {
        public Guid Id { get; set; }

        public Guid TenantId { get; set; }

        [ForeignKey(typeof(ContactIssue), OnDelete = "NO ACTION")]
        public Guid BuyerId { get; set; }

        [ForeignKey(typeof(ContactIssue), OnDelete = "NO ACTION")]
        public Guid SellerId { get; set; }

        public int AmountCents { get; set; }
    }

    public class SaleView
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string BuyerFirstName { get; set; }
        public string BuyerLastName { get; set; }
        public string SellerFirstName { get; set; }
        public string SellerLastName { get; set; }
        public int AmountCents { get; set; }
    }

    public class MultipleSelfJoinsWithAliases : OrmLiteTestBase
    {
        [Test]
        public void Can_use_cusom_SqlExpression_to_add_multiple_self_Left_Joins()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<Sale>();
                db.DropTable<ContactIssue>();

                db.CreateTable<ContactIssue>();
                db.CreateTable<Sale>();

                var tenantId = Guid.NewGuid();

                var buyer = new ContactIssue {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    FirstName = "Buyer",
                    LastName = "LastBuyer"
                };

                var seller = new ContactIssue {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    FirstName = "Seller",
                    LastName = "LastSeller"
                };

                db.Insert(buyer, seller);

                var sale = new Sale {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    BuyerId = buyer.Id,
                    SellerId = seller.Id,
                    AmountCents = 100,
                };

                db.Insert(sale);

                var q = db.From<Sale>()
                    .CustomJoin("LEFT JOIN {0} seller on (Sale.{1} = seller.Id)"
                        .Fmt("ContactIssue".SqlTable(), "SellerId".SqlColumn()))
                    .CustomJoin("LEFT JOIN {0} buyer on (Sale.{1} = buyer.Id)"
                        .Fmt("ContactIssue".SqlTable(), "BuyerId".SqlColumn()))
                    .Select(@"Sale.*
                        , buyer.{0} AS BuyerFirstName
                        , buyer.{1} AS BuyerLastName
                        , seller.{0} AS SellerFirstName
                        , seller.{1} AS SellerLastName"
                    .Fmt("FirstName".SqlColumn(), "LastName".SqlColumn()));

                q.Where(x => x.TenantId == tenantId);

                var sales = db.Select<SaleView>(q);
                Assert.That(sales.Count, Is.EqualTo(1));

                //Alternative
                q = db.From<Sale>()
                    .LeftJoin<ContactIssue>((s,c) => s.SellerId == c.Id, db.JoinAlias<ContactIssue>("seller"))
                    .LeftJoin<ContactIssue>((s,c) => s.BuyerId == c.Id, db.JoinAlias<ContactIssue>("buyer"))
                    .Select<Sale, ContactIssue>((s,c) => new {
                        sale = Sql.AllFields(s),
                        BuyerFirstName = Sql.JoinAlias(c.FirstName, "buyer"),
                        BuyerLastName = Sql.JoinAlias(c.LastName, "buyer"),
                        SellerFirstName = Sql.JoinAlias(c.FirstName, "seller"),
                        SellerLastName = Sql.JoinAlias(c.LastName, "seller"),
                    });

                q.Where(x => x.TenantId == tenantId);

                sales = db.Select<SaleView>(q);
                Assert.That(sales.Count, Is.EqualTo(1));


                var salesView = sales[0];
                
                //salesView.PrintDump();

                Assert.That(salesView.Id, Is.EqualTo(sale.Id));
                Assert.That(salesView.TenantId, Is.EqualTo(sale.TenantId));
                Assert.That(salesView.BuyerFirstName, Is.EqualTo(buyer.FirstName));
                Assert.That(salesView.BuyerLastName, Is.EqualTo(buyer.LastName));
                Assert.That(salesView.SellerFirstName, Is.EqualTo(seller.FirstName));
                Assert.That(salesView.SellerLastName, Is.EqualTo(seller.LastName));
                Assert.That(salesView.AmountCents, Is.EqualTo(sale.AmountCents));


                q.Select("seller.*, 0 EOT, buyer.*");

                var multi = db.Select<Tuple<ContactIssue, ContactIssue>>(q);
                multi.PrintDump();

                Assert.That(multi[0].Item1.FirstName, Is.EqualTo("Seller"));
                Assert.That(multi[0].Item2.FirstName, Is.EqualTo("Buyer"));
            }
        }
    }
}