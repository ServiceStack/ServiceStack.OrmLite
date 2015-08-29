using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.OrmLite.Dapper;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Issues
{
    [TestFixture]
    public class CsvTests : OrmLiteTestBase
    {
        [Test]
        public void Can_serialize_Dapper_results_to_CSV()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Poco>();

                db.Insert(new Poco { Id = 1, Name = "Foo" });
                db.Insert(new Poco { Id = 2, Name = "Bar" });

                var results = db.Query("select * from Poco");

                var json = JsonSerializer.SerializeToString(results);
                Assert.That(json, Is.EqualTo("[{\"Id\":1,\"Name\":\"Foo\"},{\"Id\":2,\"Name\":\"Bar\"}]"));

                var csv = CsvSerializer.SerializeToCsv(results);
                Assert.That(csv.NormalizeNewLines(), Is.EqualTo("Id,Name\n1,Foo\n2,Bar\n"));
            }
        }

        [Test]
        public void Can_serialize_OrmLite_results_to_CSV()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Poco>();

                db.Insert(new Poco { Id = 1, Name = "Foo" });
                db.Insert(new Poco { Id = 2, Name = "Bar" });

                var results = db.Select<Poco>();

                var json = JsonSerializer.SerializeToString(results);
                Assert.That(json, Is.EqualTo("[{\"Id\":1,\"Name\":\"Foo\"},{\"Id\":2,\"Name\":\"Bar\"}]"));

                var csv = results.ToCsv();
                Assert.That(csv.NormalizeNewLines(), Is.EqualTo("Id,Name\n1,Foo\n2,Bar\n"));

                var rows = db.Select<Dictionary<string,object>>("select * from Poco");
                csv = rows.ToCsv();
                Assert.That(csv.NormalizeNewLines(), Is.EqualTo("Id,Name\n1,Foo\n2,Bar\n"));
            }
        }
    }
}
