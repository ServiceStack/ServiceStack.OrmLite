using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class OrmLiteDropTableTests
        : OrmLiteTestBase
    {
        [Test]
        public void Can_drop_existing_table()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable(typeof(ModelWithIdOnly));
                db.DropAndCreateTable<ModelWithIdAndName>();

                Assert.That(
                    db.TableExists(typeof(ModelWithIdOnly).Name),
                    Is.True);
                Assert.That(
                    db.TableExists(typeof(ModelWithIdAndName).Name),
                    Is.True);

                db.DropTable<ModelWithIdOnly>();
                db.DropTable(typeof(ModelWithIdAndName));

                Assert.That(
                    db.TableExists(typeof(ModelWithIdOnly).Name),
                    Is.False);
                Assert.That(
                    db.TableExists(typeof(ModelWithIdAndName).Name),
                    Is.False);
            }
        }

        [Test]
        public void Can_drop_multiple_tables()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTables(typeof(ModelWithIdOnly), typeof(ModelWithIdAndName));

                Assert.That(
                    db.TableExists(typeof(ModelWithIdOnly).Name),
                    Is.True);
                Assert.That(
                    db.TableExists(typeof(ModelWithIdAndName).Name),
                    Is.True);

                db.DropTables(typeof(ModelWithIdOnly), typeof(ModelWithIdAndName));

                Assert.That(
                    db.TableExists(typeof(ModelWithIdOnly).Name),
                    Is.False);
                Assert.That(
                    db.TableExists(typeof(ModelWithIdAndName).Name),
                    Is.False);
            }
        }
    }
}
