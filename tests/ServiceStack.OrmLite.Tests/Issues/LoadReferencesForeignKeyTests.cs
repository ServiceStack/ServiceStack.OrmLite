using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class Root
    {
        [PrimaryKey]
        public int RootId { get; set; }

        [Reference]
        public List<RootItem> Items { get; set; }
    }

    public class RootItem
    {
        [PrimaryKey]
        public int RootItemId { get; set; }

        public int RootId { get; set; } //`{Parent}Id` convention to refer to Client

        public string MyValue { get; set; }
    }

    public class LoadReferencesForeignKeyTests : OrmLiteTestBase
    {
        [Test]
        public void Does_populate_Ref_Ids_of_non_convention_PrimaryKey_Tables()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Root>();
                db.DropAndCreateTable<RootItem>();

                var root = new Root {
                    RootId = 1,
                    Items = new List<RootItem> {
                        new RootItem { RootItemId = 2, MyValue = "x" }
                    }
                };

                db.Save(root, references: true);

                Assert.That(root.Items[0].RootId, Is.EqualTo(root.RootId));
            }
        }
    }
}