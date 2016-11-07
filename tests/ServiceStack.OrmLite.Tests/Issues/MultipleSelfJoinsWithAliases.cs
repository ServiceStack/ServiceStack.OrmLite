using System;
using System.Data;
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
        public string BuyerInitials { get; set; }
        public string SellerFirstName { get; set; }
        public string SellerLastName { get; set; }
        public string SellerInitials { get; set; }
        public int AmountCents { get; set; }
    }

    public class MultipleSelfJoinsWithAliases : OrmLiteTestBase
    {
        private static Sale PopulateData(IDbConnection db, Guid tenantId)
        {
            db.DropTable<Sale>();
            db.DropTable<ContactIssue>();

            db.CreateTable<ContactIssue>();
            db.CreateTable<Sale>();

            var buyer = new ContactIssue
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                FirstName = "BuyerFirst",
                LastName = "LastBuyer"
            };

            var seller = new ContactIssue
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                FirstName = "SellerFirst",
                LastName = "LastSeller"
            };

            db.Insert(buyer, seller);

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                BuyerId = buyer.Id,
                SellerId = seller.Id,
                AmountCents = 100,
            };

            db.Insert(sale);
            return sale;
        }

        [Test]
        public void Can_use_cusom_SqlExpression_to_add_multiple_self_Left_Joins()
        {
            using (var db = OpenDbConnection())
            {
                var tenantId = Guid.NewGuid();
                var sale = PopulateData(db, tenantId);

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
                    .LeftJoin<ContactIssue>((s, c) => s.SellerId == c.Id, db.JoinAlias("seller"))
                    .LeftJoin<ContactIssue>((s, c) => s.BuyerId == c.Id, db.JoinAlias("buyer"))
                    .Select<Sale, ContactIssue>((s, c) => new
                    {
                        s,
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
                Assert.That(salesView.AmountCents, Is.EqualTo(sale.AmountCents));
                Assert.That(salesView.BuyerFirstName, Is.EqualTo("BuyerFirst"));
                Assert.That(salesView.BuyerLastName, Is.EqualTo("LastBuyer"));
                Assert.That(salesView.SellerFirstName, Is.EqualTo("SellerFirst"));
                Assert.That(salesView.SellerLastName, Is.EqualTo("LastSeller"));

                q.Select("seller.*, 0 EOT, buyer.*");

                var multi = db.Select<Tuple<ContactIssue, ContactIssue>>(q);
                multi.PrintDump();

                Assert.That(multi[0].Item1.FirstName, Is.EqualTo("SellerFirst"));
                Assert.That(multi[0].Item2.FirstName, Is.EqualTo("BuyerFirst"));
            }
        }

        [Test]
        public void Can_use_CustomSql()
        {
            var customFmt = "";
            if (Dialect == Dialect.SqlServer || Dialect == Dialect.SqlServer2012)
                customFmt = "CONCAT(LEFT({0}, 1),LEFT({1},1))";
            else if (Dialect == Dialect.Sqlite)
                customFmt = "substr({0}, 1, 1) || substr({1}, 1, 1)";

            if (string.IsNullOrEmpty(customFmt))
                return;

            using (var db = OpenDbConnection())
            {
                var tenantId = Guid.NewGuid();
                var sale = PopulateData(db, tenantId);

                var q = db.From<Sale>()
                    .LeftJoin<ContactIssue>((s, c) => s.SellerId == c.Id, db.JoinAlias("seller"))
                    .LeftJoin<ContactIssue>((s, c) => s.BuyerId == c.Id, db.JoinAlias("buyer"))
                    .Select<Sale, ContactIssue>((s, c) => new
                    {
                        s,
                        BuyerFirstName = Sql.JoinAlias(c.FirstName, "buyer"),
                        BuyerLastName = Sql.JoinAlias(c.LastName, "buyer"),
                        BuyerInitials = Sql.Custom(customFmt.Fmt("buyer.FirstName", "buyer.LastName")),
                        SellerFirstName = Sql.JoinAlias(c.FirstName, "seller"),
                        SellerLastName = Sql.JoinAlias(c.LastName, "seller"),
                        SellerInitials = Sql.Custom(customFmt.Fmt("seller.FirstName", "seller.LastName")),
                    });

                var sales = db.Select<SaleView>(q);
                var salesView = sales[0];

                Assert.That(salesView.BuyerFirstName, Is.EqualTo("BuyerFirst"));
                Assert.That(salesView.BuyerLastName, Is.EqualTo("LastBuyer"));
                Assert.That(salesView.BuyerInitials, Is.EqualTo("BL"));
                Assert.That(salesView.SellerFirstName, Is.EqualTo("SellerFirst"));
                Assert.That(salesView.SellerLastName, Is.EqualTo("LastSeller"));
                Assert.That(salesView.SellerInitials, Is.EqualTo("SL"));
            }
        }

    }
}