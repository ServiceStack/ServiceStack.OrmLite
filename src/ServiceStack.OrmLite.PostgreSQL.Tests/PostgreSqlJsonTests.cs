using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Tests;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.PostgreSQL.Tests
{
    public class ModelWithJsonType
    {
        public int Id { get; set; }

        [CustomField("json")]
        public ComplexType ComplexTypeJson { get; set; }
    }

    public class ComplexType
    {
        public int Id { get; set; }
        public SubType SubType { get; set; }
    }

    public class SubType
    {
        public string Name { get; set; }
    }


    [TestFixture]
    public class PostgreSqlJsonTests : OrmLiteTestBase
    {
        public PostgreSqlJsonTests() : base(Dialect.PostgreSql) {}

        [Test]
        public void Can_save_complex_types_as_JSON()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithJsonType>();

                db.GetLastSql().Print();

                var row = new ModelWithJsonType
                {
                    Id = 1,
                    ComplexTypeJson = new ComplexType
                    {
                        Id = 2, SubType = new SubType { Name = "SubType2" }
                    },
                    //ComplexTypeJsonb = new ComplexType
                    //{
                    //    Id = 3, SubType = new SubType { Name = "SubType3" }
                    //},
                };

                db.Insert(row);

                var result = db.Select<ModelWithJsonType>(
                    "complex_type_json->'SubType'->>'Name' = 'SubType2'");

                db.GetLastSql().Print();

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].Id, Is.EqualTo(1));
                Assert.That(result[0].ComplexTypeJson.Id, Is.EqualTo(2));
                Assert.That(result[0].ComplexTypeJson.SubType.Name, Is.EqualTo("SubType2"));
            }
        }
    }
}