using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class ModelWithCustomFields
    {
        public int Id { get; set; }

        [DecimalLength(12,3)]
        public decimal? Decimal { get; set; }
    }

    [TestFixture]
    public class CustomFieldTests : OrmLiteTestBase
    {
        [Test]
        public void Can_create_custom_Decimal_field()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithCustomFields>();

                var sql = db.GetLastSql();

                sql.Print();

                Assert.That(sql, Does.Contain("DECIMAL(12,3)"));
            }
        }
    }
}