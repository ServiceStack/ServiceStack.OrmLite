using System.Data;
using NUnit.Framework;
using ServiceStack.Serialization;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class ModelWithComplexType
    {
        public int Id { get; set; }
        public ComplexType ComplexType { get; set; }
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
    public class StringSerializerTests
        : OrmLiteTestBase
    {
        private static void InsertModelWithComplexType(IDbConnection db)
        {
            db.DropAndCreateTable<ModelWithComplexType>();

            db.Insert(new ModelWithComplexType {
                Id = 1,
                ComplexType = new ComplexType { Id = 2, SubType = new SubType { Name = "Sub" } }
            });
        }

        public string TestSql
        {
            get { return "SELECT {0} from {1}".Fmt("ComplexType".SqlColumn(), "ModelWithComplexType".SqlTable()); }
        }

        [Test]
        public void Serializes_complex_types_with_JSV_by_default_except_uses_JSON_for_PostgreSQL()
        {
            using (var db = OpenDbConnection())
            {
                InsertModelWithComplexType(db);

                var str = db.SqlScalar<string>(TestSql);

                Assert.That(str, 
                    Is.EqualTo("{\"Id\":2,\"SubType\":{\"Name\":\"Sub\"}}"). // PostgreSqlDialect
                    Or.EqualTo("{Id:2,SubType:{Name:Sub}}"));

                var data = db.SingleById<ModelWithComplexType>(1);
                Assert.That(data.ComplexType.SubType.Name, Is.EqualTo("Sub"));
            }
        }

        [Test]
        public void Can_use_JSV_StringSerializer()
        {
            using (var db = OpenDbConnection())
            {
                var hold = db.GetDialectProvider().StringSerializer;
                db.GetDialectProvider().StringSerializer = new JsvStringSerializer();

                InsertModelWithComplexType(db);

                var str = db.SqlScalar<string>(TestSql);
                Assert.That(str, Is.EqualTo("{Id:2,SubType:{Name:Sub}}"));

                var data = db.SingleById<ModelWithComplexType>(1);
                Assert.That(data.ComplexType.SubType.Name, Is.EqualTo("Sub"));

                db.GetDialectProvider().StringSerializer = hold;
            }
        }

        [Test]
        public void Can_use_JSON_StringSerializer()
        {
            using (var db = OpenDbConnection())
            {
                var hold = db.GetDialectProvider().StringSerializer;
                db.GetDialectProvider().StringSerializer = new JsonStringSerializer();

                InsertModelWithComplexType(db);

                var str = db.SqlScalar<string>(TestSql);
                Assert.That(str, Is.EqualTo("{\"Id\":2,\"SubType\":{\"Name\":\"Sub\"}}"));

                var data = db.SingleById<ModelWithComplexType>(1);
                Assert.That(data.ComplexType.SubType.Name, Is.EqualTo("Sub"));

                db.GetDialectProvider().StringSerializer = hold;
            }
        }

        [Test]
        public void Can_use_JSON_DataContract_StringSerializer()
        {
            using (var db = OpenDbConnection())
            {
                var hold = db.GetDialectProvider().StringSerializer;
                db.GetDialectProvider().StringSerializer = new JsonDataContractSerializer();

                InsertModelWithComplexType(db);

                var str = db.SqlScalar<string>(TestSql);
                Assert.That(str, Is.EqualTo("{\"Id\":2,\"SubType\":{\"Name\":\"Sub\"}}"));

                var data = db.SingleById<ModelWithComplexType>(1);
                Assert.That(data.ComplexType.SubType.Name, Is.EqualTo("Sub"));

                db.GetDialectProvider().StringSerializer = hold;
            }
        }

        [Test]
        public void Can_use_Xml_DataContract_StringSerializer()
        {
            using (var db = OpenDbConnection())
            {
                var hold = db.GetDialectProvider().StringSerializer;
                db.GetDialectProvider().StringSerializer = new DataContractSerializer();

                InsertModelWithComplexType(db);

                var str = db.SqlScalar<string>(TestSql);
                Assert.That(str, Is.EqualTo("<ComplexType xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://schemas.datacontract.org/2004/07/ServiceStack.OrmLite.Tests\"><Id>2</Id><SubType><Name>Sub</Name></SubType></ComplexType>"));

                var data = db.SingleById<ModelWithComplexType>(1);
                Assert.That(data.ComplexType.SubType.Name, Is.EqualTo("Sub"));

                db.GetDialectProvider().StringSerializer = hold;
            }
        }

        [Test]
        public void Can_use_XmlSerializer_StringSerializer()
        {
            using (var db = OpenDbConnection())
            {
                var hold = db.GetDialectProvider().StringSerializer;
                db.GetDialectProvider().StringSerializer = new XmlSerializableSerializer();

                InsertModelWithComplexType(db);

                var str = db.SqlScalar<string>(TestSql);
                Assert.That(str, Is.EqualTo("<?xml version=\"1.0\" encoding=\"utf-8\"?><ComplexType xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><Id>2</Id><SubType><Name>Sub</Name></SubType></ComplexType>"));

                var data = db.SingleById<ModelWithComplexType>(1);
                Assert.That(data.ComplexType.SubType.Name, Is.EqualTo("Sub"));

                db.GetDialectProvider().StringSerializer = hold;
            }
        }
    }
}