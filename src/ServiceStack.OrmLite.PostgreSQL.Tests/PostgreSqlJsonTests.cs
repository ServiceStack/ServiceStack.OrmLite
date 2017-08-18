using System;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Tests;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.PostgreSQL.Tests
{
    public class ModelWithJsonType
    {
        public int Id { get; set; }

        [PgSqlJson]
        public ComplexType ComplexTypeJson { get; set; }

        [PgSqlJsonB]
        public ComplexType ComplexTypeJsonb { get; set; }
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

    public class PgsqlData
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        [PgSqlIntArray]
        public int[] Ints { get; set; }

        [PgSqlTextArray]
        public string[] Strings { get; set; }
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
                    ComplexTypeJsonb = new ComplexType
                    {
                        Id = 3, SubType = new SubType { Name = "SubType3" }
                    },
                };

                db.Insert(row);

                var result = db.Select<ModelWithJsonType>(
                    "complex_type_json->'SubType'->>'Name' = 'SubType2'");

                db.GetLastSql().Print();

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].Id, Is.EqualTo(1));
                Assert.That(result[0].ComplexTypeJson.Id, Is.EqualTo(2));
                Assert.That(result[0].ComplexTypeJson.SubType.Name, Is.EqualTo("SubType2"));

                result = db.Select<ModelWithJsonType>(
                    "complex_type_jsonb->'SubType'->>'Name' = 'SubType3'");

                Assert.That(result[0].ComplexTypeJsonb.Id, Is.EqualTo(3));
                Assert.That(result[0].ComplexTypeJsonb.SubType.Name, Is.EqualTo("SubType3"));
            }
        }

        [Test]
        public void Does_save_PgSqlData()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<PgsqlData>();

                var data = new PgsqlData
                {
                    Id = Guid.NewGuid(),
                    Ints = new[] { 2, 4, 1 },
                    Strings = new[] { "test string 1", "test string 2" }
                };

                db.Save(data);

                var row = db.Select<PgsqlData>()[0];
                Assert.That(row.Ints.EquivalentTo(data.Ints));
                Assert.That(row.Strings.EquivalentTo(data.Strings));
            }
        }
    }
}