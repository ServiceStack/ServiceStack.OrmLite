using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class AddressAudit
    {
        [PrimaryKey, AutoIncrement]
        [Alias("AddressId")]
        public int Id { get; set; }

        [Alias("AddressLinkId")]
        public int AddressId { get; set; }
    }

    [TestFixture]
    public class SelectAliasIssue : OrmLiteTestBase
    {
        [Test]
        public void Does_populate_table_with_Aliases_having_same_name_as_alternative_field()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<AddressAudit>();

                db.Insert(new AddressAudit { AddressId = 11 });
                db.Insert(new AddressAudit { AddressId = 12 });
                db.Insert(new AddressAudit { AddressId = 13 });

                var rows = db.Select<AddressAudit>();
                Assert.That(rows.All(x => x.Id > 0));

                var debtor = db.SingleById<AddressAudit>(2);
                var row = db.Single<AddressAudit>(audit => audit.AddressId == debtor.AddressId);

                row.PrintDump();
            }
        }
    }
}