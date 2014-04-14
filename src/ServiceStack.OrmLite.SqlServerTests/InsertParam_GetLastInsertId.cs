using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

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

                var lastInsertId = con.Insert(testObject, selectIdentity:true);
                Assert.That(lastInsertId, Is.GreaterThan(0), "with InsertParam");
            }
        }
    }
}
