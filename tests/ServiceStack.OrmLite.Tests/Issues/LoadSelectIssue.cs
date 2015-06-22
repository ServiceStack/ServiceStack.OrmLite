using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Issues
{
    [TestFixture]
    public class LoadSelectIssue : OrmLiteTestBase
    {
        [Test]
        public void Can_LoadSelect_PlayerEquipment()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<PlayerEquipment>();
                db.DropTable<ItemData>();

                db.CreateTable<ItemData>();
                db.CreateTable<PlayerEquipment>();

                var item1 = new ItemData { Data = "ITEM1" };
                db.Save(item1);

                db.Save(new PlayerEquipment
                {
                    PlayerId = 1,
                    ItemId = item1.Id,
                    Quantity = 1,
                    IsEquipped = true,
                });

                var item2 = new ItemData { Data = "ITEM2" };
                db.Save(item2);

                db.Save(new PlayerEquipment
                {
                    PlayerId = 1,
                    ItemId = item2.Id,
                    Quantity = 1,
                    IsEquipped = false,
                });

                var playerId = 1;
                var results = db.LoadSelect<PlayerEquipment>(q => q.PlayerId == playerId);

                results.PrintDump();
            }
        }
    }

    public class PlayerEquipment
    {
        public string Id
        {
            get { return PlayerId + "/" + ItemId; }
        }

        public int PlayerId { get; set; }

        [References(typeof(ItemData))]
        public int ItemId { get; set; }

        public int Quantity { get; set; }

        public bool IsEquipped { get; set; }

        [Reference]
        public ItemData ItemData { get; set; }
    }

    public class ItemData
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Data { get; set; }
    }
}