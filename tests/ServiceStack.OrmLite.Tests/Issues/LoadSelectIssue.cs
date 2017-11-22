using System;
using NUnit.Framework;
using ServiceStack.Data;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;
using ServiceStack.OrmLite.SqlServer;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Issues
{
    [TestFixture]
    public class LoadSelectIssue : OrmLiteTestBase
    {
        public class PlayerEquipment
        {
            public string Id => PlayerId + "/" + ItemId;

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

        [Alias("EventCategory")]
        public class EventCategoryTbl : IHasSoftDelete, IHasTimeStamp
        {
            [PrimaryKey]
            public Guid EventCategoryId { get; set; }


            [System.ComponentModel.DataAnnotations.Required]
            public string Name { get; set; }

            /// <summary>
            /// Link to the file record that contains any image related to this category
            /// </summary>
            [References(typeof(FileTbl))]
            public Guid LinkedImageId { get; set; }

            [Reference]
            public FileTbl LinkedImage { get; set; }

            public bool IsDeleted { get; set; }

            [RowVersion]
            public ulong RowVersion { get; set; }
        }

        [Alias("File")]
        public class FileTbl : IHasSoftDelete, IHasTimeStamp
        {
            [PrimaryKey]
            public Guid FileId { get; set; }

            public string Name { get; set; }

            public string Extension { get; set; }

            public long FileSizeBytes { get; set; }

            public bool IsDeleted { get; set; }

            [RowVersion]
            public ulong RowVersion { get; set; }
        }

        public interface IHasTimeStamp
        {
            [RowVersion]
            ulong RowVersion { get; set; }
        }

        public interface IHasSoftDelete
        {
            bool IsDeleted { get; set; }
        }

        [Test]
        public void Can_execute_LoadSelect_when_child_references_implement_IHasSoftDelete()
        {
            LogManager.LogFactory = new ConsoleLogFactory(debugEnabled:true);
            // Automatically filter out all soft deleted records, for ALL table types.
            OrmLiteConfig.SqlExpressionSelectFilter = q =>
            {
                if (q.ModelDef.ModelType.HasInterface(typeof(IHasSoftDelete)))
                {
                    q.Where<IHasSoftDelete>(x => x.IsDeleted != true);
                }
            };

            using (var db = OpenDbConnection())
            {
                db.DropTable<EventCategoryTbl>();
                db.DropTable<FileTbl>();

                db.CreateTable<FileTbl>();
                db.CreateTable<EventCategoryTbl>();

                var results = db.LoadSelect<EventCategoryTbl>();
            }

            OrmLiteConfig.SqlExpressionSelectFilter = null;
        }

    }
}