using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Tests;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.PostgreSQL.Tests.Issues
{
    [TestFixture]
    public class JsonDataTypeTests : OrmLiteTestBase
    {
        [Test]
        public void Can_save_and_restore_JSON_property()
        {
            OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();

            var item = new LicenseCheckTemp();
            item.Body = new CheckHistory();
            item.Body.List.Add(new ItemHistory { AddedOn = DateTime.MaxValue, Note = "Test" });

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<LicenseCheckTemp>();
                db.GetLastSql().Print();
                db.Save(item);
            }

            using (var db = OpenDbConnection())
            {
                var items = db.Select<LicenseCheckTemp>();
                items.PrintDump();

                foreach (var licenseCheck in items.OrderBy(x => x.Id))
                {
                    if (licenseCheck.Body != null && licenseCheck.Body.List.Any())
                    {
                        foreach (var itemHistory in licenseCheck.Body.List)
                        {
                            Console.WriteLine($"{itemHistory.AddedOn} :  Note {itemHistory.Note}");
                        }
                    }
                }
            }
        }
    }

    public class LicenseCheckTemp
    {
        [AutoIncrement]
        public int Id { get; set; }

        [CustomField("json")]
        public CheckHistory Body { get; set; }
    }

    public class CheckHistory
    {
        public List<ItemHistory> List { get; set; } = new List<ItemHistory>();
    }

    public class ItemHistory
    {
        public string Note { get; set; }

        public DateTime AddedOn { get; set; }

    }

}