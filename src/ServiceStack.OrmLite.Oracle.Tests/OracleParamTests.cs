using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Oracle.Tests
{
    [TestFixture]
    public class OracleParamTests : OracleTestBase
    {
        private void DropAndCreateTables(IDbConnection db)
        {
            if (db.TableExists("ParamRelBO"))
                db.DropTable<ParamRelBO>();

            db.CreateTable<ParamTestBO>(true);
            db.CreateTable<ParamRelBO>(true);
        }

        [Test]
        public void ORA_ParamTestInsert()
        {
            using (var db = ConnectionString.OpenDbConnection())
            {
                DropAndCreateTables(db);
                var dateTimeNow =new DateTime( DateTime.Now.Year,  DateTime.Now.Month,  DateTime.Now.Day);

                db.InsertParameterized(new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = null, DateTime = dateTimeNow });
                db.InsertParameterized(new ParamTestBO() { Id = 2, Double = 0.002, Int = 200, Info = "Two", NullableBool = true, DateTime = dateTimeNow });
                db.InsertParameterized(new ParamTestBO() { Id = 3, Double = 0.003, Int = 300, Info = "Three", NullableBool = false, DateTime = dateTimeNow.AddDays(23) });
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

                Assert.AreEqual(dateTimeNow, bo1.DateTime);
                Assert.AreEqual(dateTimeNow, bo2.DateTime);
                Assert.AreEqual(dateTimeNow.AddDays(23), bo3.DateTime);
                Assert.AreEqual(null, bo4.DateTime);
            }
        }

        [Test]
        public void ORA_ParamTestUpdate()
        {
            using (var db = ConnectionString.OpenDbConnection())
            {
                DropAndCreateTables(db);

                var bo1 = new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = true };
                db.InsertParameterized(bo1);

                bo1.Double = 0.01;
                bo1.Int = 10000;
                bo1.Info = "OneUpdated";
                bo1.NullableBool = null;
                bo1.DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                db.UpdateParameterized(bo1);

                var bo1Check = db.GetById<ParamTestBO>(1);

                Assert.AreEqual(bo1.Double, bo1Check.Double);
                Assert.AreEqual(bo1.Int, bo1Check.Int);
                Assert.AreEqual(bo1.Info, bo1Check.Info);
                Assert.AreEqual(bo1.DateTime, bo1Check.DateTime);
            }
        }

        [Test]
        public void ORA_ParamTestDelete()
        {
            using (var db = ConnectionString.OpenDbConnection())
            {
                DropAndCreateTables(db);

                db.Insert(new ParamTestBO() { Id = 1 });
                db.Insert(new ParamTestBO() { Id = 2 });
                db.Insert(new ParamTestBO() { Id = 3 });

                Assert.IsNotNull(db.Select<ParamTestBO>(q => q.Id == 1).FirstOrDefault());
                Assert.IsNotNull(db.Select<ParamTestBO>(q => q.Id == 2).FirstOrDefault());
                Assert.IsNotNull(db.Select<ParamTestBO>(q => q.Id == 3).FirstOrDefault());

                db.DeleteByIdParametized<ParamTestBO>(1);
                db.DeleteByIdParametized<ParamTestBO>(2);
                db.DeleteByIdParametized<ParamTestBO>(3);

                Assert.IsNull(db.Select<ParamTestBO>(q => q.Id == 1).FirstOrDefault());
                Assert.IsNull(db.Select<ParamTestBO>(q => q.Id == 2).FirstOrDefault());
                Assert.IsNull(db.Select<ParamTestBO>(q => q.Id == 3).FirstOrDefault());
            }
        }

        [Test]
        public void ORA_ParamTestSelectLambda()
        {
            using (var db = ConnectionString.OpenDbConnection())
            {
                DropAndCreateTables(db);

                db.InsertParameterized(new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = null });
                db.InsertParameterized(new ParamTestBO() { Id = 2, Double = 0.002, Int = 200, Info = "Two", NullableBool = true });
                db.InsertParameterized(new ParamTestBO() { Id = 3, Double = 0.003, Int = 300, Info = "Three", NullableBool = false });
                db.InsertParameterized(new ParamTestBO() { Id = 4, Double = 0.004, Int = 400, Info = "Four", NullableBool = null });

                //select multiple items
                Assert.AreEqual(2, db.Select<ParamTestBO>(q => q.NullableBool == null).Count);
                Assert.AreEqual(2, db.SelectParameterized<ParamTestBO>(q => q.NullableBool == null).Count);
                Assert.AreEqual(1, db.SelectParameterized<ParamTestBO>(q => q.NullableBool == true).Count);
                Assert.AreEqual(1, db.SelectParameterized<ParamTestBO>(q => q.NullableBool == false).Count);

                Assert.AreEqual(1, db.SelectParameterized<ParamTestBO>(q => q.Info == "Two").Count);
                Assert.AreEqual(1, db.SelectParameterized<ParamTestBO>(q => q.Int == 300).Count);
                Assert.AreEqual(1, db.SelectParameterized<ParamTestBO>(q => q.Double == 0.003).Count);
            }
        }

        [Test]
        public void ORA_ParamTestSelectLambda2()
        {
            using (var db = ConnectionString.OpenDbConnection())
            {
                DropAndCreateTables(db);

                db.InsertParameterized(new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = null });
                db.InsertParameterized(new ParamTestBO() { Id = 2, Double = 0.002, Int = 200, Info = "Two", NullableBool = true });
                db.InsertParameterized(new ParamTestBO() { Id = 3, Double = 0.003, Int = 300, Info = "Three", NullableBool = false });
                db.InsertParameterized(new ParamTestBO() { Id = 4, Double = 0.004, Int = 400, Info = "Four", NullableBool = null });

                db.InsertParameterized(new ParamRelBO() { PTId = 1, Info = "T1" });
                db.InsertParameterized(new ParamRelBO() { PTId = 1, Info = "T1" });
                db.InsertParameterized(new ParamRelBO() { PTId = 1, Info = "T1" });
                db.InsertParameterized(new ParamRelBO() { PTId = 1, Info = "T1" });
                db.InsertParameterized(new ParamRelBO() { PTId = 2, Info = "T1" });
                db.InsertParameterized(new ParamRelBO() { PTId = 2, Info = "T1" });
                db.InsertParameterized(new ParamRelBO() { PTId = 3, Info = "T1" });
                db.InsertParameterized(new ParamRelBO() { PTId = 4, Info = "T1" });
                db.InsertParameterized(new ParamRelBO() { PTId = 3, Info = "T2" });
                db.InsertParameterized(new ParamRelBO() { PTId = 4, Info = "T2" });

                Assert.AreEqual(8, db.SelectParameterized<ParamRelBO>(q => q.Info == "T1").Count);
                Assert.AreEqual(2, db.SelectParameterized<ParamRelBO>(q => q.Info == "T2").Count);

                Assert.AreEqual(3, db.SelectParameterized<ParamRelBO>(q => q.Info == "T1" && (q.PTId == 2 || q.PTId == 3) ).Count);
            }
        }

        [Test]
        public void ORA_ParamTestSelectLambdaComplex()
        {
            using (var db = ConnectionString.OpenDbConnection())
            {
                //various special cases that still need to be addressed

                //Assert.AreEqual(10, db.SelectParameterized<ParamRelBO>(q => Sql.In(q.PTId, "T1", "T2")));
                //Assert.AreEqual(10, db.SelectParameterized<ParamRelBO>(q => q.Info.StartsWith("T")));
                //Assert.AreEqual(10, db.SelectParameterized<ParamRelBO>(q => q.Info.EndsWith("1")));
                //Assert.AreEqual(10, db.SelectParameterized<ParamRelBO>(q => q.Info.Contains("T")));
            }
        }



        public class ParamTestBO
        {
            public int Id { get; set; }
            [StringLength(400)]
            public string Info { get; set; }
            public int Int { get; set; }
            public double Double { get; set; }
            public bool? NullableBool { get; set; }
            public DateTime? DateTime { get; set; }
        }


        public class ParamRelBO
        {
            [Sequence("SEQ_PARAMTESTREL_ID")]
            [PrimaryKey]
            [Alias("ParamRel_Id")]
            public int Id { get; set; }
            [ForeignKey(typeof(ParamTestBO))]
            public int PTId { get; set; }

            [Alias("InfoStr")]
            public string Info { get; set; }
        }
    }
}
