using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Oracle.Tests
{
    [TestFixture]
    public class OracleParamTests : OracleTestBase
    {
        [Test]
        public void ORA_ParamTestInsert()
        {
            using (var db = ConnectionString.OpenDbConnection())
            {
                db.CreateTable<ParamTestBO>(true);

                db.InsertParametized(new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One" });
                db.InsertParametized(new ParamTestBO() { Id = 2, Double = 0.002, Int = 200, Info = "Two" });
                db.InsertParametized(new ParamTestBO() { Id = 3, Double = 0.003, Int = 300, Info = "Three" });
                db.InsertParametized(new ParamTestBO() { Id = 4, Double = 0.004, Int = 400, Info = "Four" });

                Assert.AreEqual(100, db.GetById<ParamTestBO>(1).Int);
                Assert.AreEqual(200, db.GetById<ParamTestBO>(2).Int);
                Assert.AreEqual(300, db.GetById<ParamTestBO>(3).Int);
                Assert.AreEqual(400, db.GetById<ParamTestBO>(4).Int);
            }
        }

        [Test]
        public void ORA_ParamTestUpdate()
        {
            using (var db = ConnectionString.OpenDbConnection())
            {
                db.CreateTable<ParamTestBO>(true);
                var bo1 = new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One" };
                db.InsertParametized(bo1);

                bo1.Double = 0.01;
                bo1.Int = 10000;
                bo1.Info = "OneUpdated";

                db.UpdateParametized(bo1);

                var bo1Check = db.GetById<ParamTestBO>(1);

                Assert.AreEqual(bo1.Double, bo1Check.Double);
                Assert.AreEqual(bo1.Int, bo1Check.Int);
                Assert.AreEqual(bo1.Info, bo1Check.Info);
            }
        }

        [Test]
        public void ORA_ParamTestSelect()
        {
            using (var db = ConnectionString.OpenDbConnection())
            {
                db.CreateTable<ParamTestBO>(true);

                db.InsertParametized(new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One" });
                db.InsertParametized(new ParamTestBO() { Id = 2, Double = 0.002, Int = 200, Info = "Two" });
                db.InsertParametized(new ParamTestBO() { Id = 3, Double = 0.003, Int = 300, Info = "Three" });
                db.InsertParametized(new ParamTestBO() { Id = 4, Double = 0.004, Int = 400, Info = "Four" });

                var bo1 = db.SelectParametized<ParamTestBO>(q => q.Id == 1).Single();
                var bo2 = db.SelectParametized<ParamTestBO>(q => q.Id == 2).Single();
                var bo3 = db.SelectParametized<ParamTestBO>(q => q.Id == 3).Single();
                var bo4 = db.SelectParametized<ParamTestBO>(q => q.Id == 4).Single();

                Assert.AreEqual(1, bo1.Id);
                Assert.AreEqual(2, bo2.Id);
                Assert.AreEqual(3, bo3.Id);
                Assert.AreEqual(4, bo4.Id);

                Assert.AreEqual(0.001, bo1.Double);
                Assert.AreEqual(0.002, bo2.Double);
                Assert.AreEqual(0.003, bo3.Double);
                Assert.AreEqual(0.004, bo4.Double);

                Assert.AreEqual(100, bo1.Int);
                Assert.AreEqual(200, bo2.Int);
                Assert.AreEqual(300, bo3.Int);
                Assert.AreEqual(400, bo4.Int);

                Assert.AreEqual("One", bo1.Info);
                Assert.AreEqual("Two", bo2.Info);
                Assert.AreEqual("Three", bo3.Info);
                Assert.AreEqual("Four", bo4.Info);
            }
        }

        public class ParamTestBO
        {
            public int Id { get; set; }
            public string Info { get; set; }
            public int Int { get; set; }
            public double Double { get; set; }
        }
    }
}
