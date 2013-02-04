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

                db.InsertParameterized(new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = null });
                db.InsertParameterized(new ParamTestBO() { Id = 2, Double = 0.002, Int = 200, Info = "Two", NullableBool = true });
                db.InsertParameterized(new ParamTestBO() { Id = 3, Double = 0.003, Int = 300, Info = "Three", NullableBool = false });
                db.InsertParameterized(new ParamTestBO() { Id = 4, Double = 0.004, Int = 400, Info = "Four", NullableBool = null });

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
                var bo1 = new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = true };
                db.InsertParameterized(bo1);

                bo1.Double = 0.01;
                bo1.Int = 10000;
                bo1.Info = "OneUpdated";
                bo1.NullableBool = null;

                db.UpdateParameterized(bo1);

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

                db.InsertParameterized(new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = null });
                db.InsertParameterized(new ParamTestBO() { Id = 2, Double = 0.002, Int = 200, Info = "Two", NullableBool = true });
                db.InsertParameterized(new ParamTestBO() { Id = 3, Double = 0.003, Int = 300, Info = "Three", NullableBool = false });
                db.InsertParameterized(new ParamTestBO() { Id = 4, Double = 0.004, Int = 400, Info = "Four", NullableBool = null });

                var bo1 = db.SelectParameterized<ParamTestBO>(q => q.Id == 1).Single();
                var bo2 = db.SelectParameterized<ParamTestBO>(q => q.Id == 2).Single();
                var bo3 = db.SelectParameterized<ParamTestBO>(q => q.Id == 3).Single();
                var bo4 = db.SelectParameterized<ParamTestBO>(q => q.Id == 4).Single();

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

                Assert.AreEqual(null, bo1.NullableBool);
                Assert.AreEqual(true, bo2.NullableBool);
                Assert.AreEqual(false, bo3.NullableBool);
                Assert.AreEqual(null, bo4.NullableBool);

                //select multiple items
                //Assert.AreEqual(2, db.Select<ParamTestBO>(q => q.NullableBool == null).Count);
                //Assert.AreEqual(2, db.SelectParameterized<ParamTestBO>(q => q.NullableBool == null).Count);
                //Assert.AreEqual(1, db.SelectParameterized<ParamTestBO>(q => q.NullableBool == true).Count);
                //Assert.AreEqual(1, db.SelectParameterized<ParamTestBO>(q => q.NullableBool == false).Count);

                Assert.AreEqual(1, db.SelectParameterized<ParamTestBO>(q => q.Info == "Two").Count);
                Assert.AreEqual(1, db.SelectParameterized<ParamTestBO>(q => q.Int == 300).Count);
                Assert.AreEqual(1, db.SelectParameterized<ParamTestBO>(q => q.Double == 0.003).Count);
            }
        }

        public class ParamTestBO
        {
            public int Id { get; set; }
            public string Info { get; set; }
            public int Int { get; set; }
            public double Double { get; set; }
            public bool? NullableBool { get; set; }
        }

        public class ParamTestRelatedBO
        {
            public int Id { get; set; }
            [ForeignKey(typeof(ParamTestBO))]
            public int ParamTestBOId { get; set; }
            public string Padding { get; set; }
        }
    }
}
