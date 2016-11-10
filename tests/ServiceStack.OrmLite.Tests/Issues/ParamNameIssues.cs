using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class LegacyRow
    {
        [AutoIncrement]
        public int ID { get; set; }

        public string Name { get; set; }

        [Alias("Age-In-Years")]
        public int Age { get; set; }
    }


    [TestFixture]
    public class ParamNameIssues : OrmLiteTestBase
    {
        [Test]
        public void Does_use_ParamName_filter()
        {
            LogManager.LogFactory = new ConsoleLogFactory();

            OrmLiteConfig.ParamNameFilter = name => name.Replace("-", "");

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<LegacyRow>();

                db.InsertAll(new []
                {
                    new LegacyRow { Name = "Row1", Age = 1 }, 
                    new LegacyRow { Name = "Row2", Age = 2 }, 
                });

                var rows = db.Select<LegacyRow>();
                Assert.That(rows.Count, Is.EqualTo(2));
            }

            OrmLiteConfig.ParamNameFilter = null;
        }
    }
}