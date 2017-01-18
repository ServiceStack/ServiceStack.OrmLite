using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Dapper;

namespace ServiceStack.OrmLite.SqlServerTests
{
    public class InsertParam_GetLastInsertId : OrmLiteTestBase
    {
        [Test]
        public void Can_GetLastInsertedId_using_InsertParam()
        {
            var testObject = new SimpleType { Name = "test" };

            //verify that "normal" Insert works as expected
            using (var con = OpenDbConnection())
            {
                con.CreateTable<SimpleType>(true);

                con.Save(testObject);
                Assert.That(testObject.Id, Is.GreaterThan(0), "normal Insert");
            }

            //test with InsertParam
            using (var con = OpenDbConnection())
            {
                con.CreateTable<SimpleType>(true);

                var lastInsertId = con.Insert(testObject, selectIdentity: true);
                Assert.That(lastInsertId, Is.GreaterThan(0), "with InsertParam");
            }
        }

        public class ServerGuid
        {
            [Default(typeof(Guid), "newid()")]
            public Guid Id { get; set; }

            public string Name { get; set; }
        }

        [Test]
        public void Can_retrieve_ServerGuid()
        {
            using (var db = OpenDbConnection())
            using (var cmd = db.CreateCommand())
            {
                db.DropAndCreateTable<ServerGuid>();

                var obj = new ServerGuid { Name = "foo" };

                cmd.GetDialectProvider().PrepareParameterizedInsertStatement<ServerGuid>(cmd,
                    insertFields: OrmLiteUtils.GetNonDefaultValueInsertFields(obj));

                cmd.CommandText = cmd.CommandText.Replace("VALUES", "OUTPUT inserted.Id VALUES");

                cmd.GetDialectProvider().SetParameterValues<ServerGuid>(cmd, obj);

                var id = (Guid)cmd.ExecuteScalar();

                Assert.That(id, Is.Not.EqualTo(default(Guid)));

                var insertedRow = db.SingleById<ServerGuid>(id);

                Assert.That(insertedRow.Name, Is.EqualTo("foo"));
            }
        }
    }
}
