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
            db.CreateTable<ParamByteBO>(true);
        }

        [Test]
        public void ORA_ParamTestInsert()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);
                var dateTimeNow =new DateTime( DateTime.Now.Year,  DateTime.Now.Month,  DateTime.Now.Day);

                db.InsertParam(new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = null, DateTime = dateTimeNow });
                db.InsertParam(new ParamTestBO() { Id = 2, Double = 0.002, Int = 200, Info = "Two", NullableBool = true, DateTime = dateTimeNow });
                db.InsertParam(new ParamTestBO() { Id = 3, Double = 0.003, Int = 300, Info = "Three", NullableBool = false, DateTime = dateTimeNow.AddDays(23) });
                db.InsertParam(new ParamTestBO() { Id = 4, Double = 0.004, Int = 400, Info = "Four", NullableBool = null });

                var bo1 = db.SelectParam<ParamTestBO>(q => q.Id == 1).Single();
                var bo2 = db.SelectParam<ParamTestBO>(q => q.Id == 2).Single();
                var bo3 = db.SelectParam<ParamTestBO>(q => q.Id == 3).Single();
                var bo4 = db.SelectParam<ParamTestBO>(q => q.Id == 4).Single();

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
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var bo1 = new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = true };
                var bo2 = new ParamTestBO() { Id = 2, Double = 0.002, Int = 200, Info = "Two", NullableBool = true, DateTime = DateTime.Now };
                db.InsertParam(bo1);
                db.InsertParam(bo2);

                bo1.Double = 0.01;
                bo1.Int = 10000;
                bo1.Info = "OneUpdated";
                bo1.NullableBool = null;
                bo1.DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                db.UpdateParam(bo1);

                var bo1Check = db.GetById<ParamTestBO>(1);

                Assert.AreEqual(bo1.Double, bo1Check.Double);
                Assert.AreEqual(bo1.Int, bo1Check.Int);
                Assert.AreEqual(bo1.Info, bo1Check.Info);
                Assert.AreEqual(bo1.DateTime, bo1Check.DateTime);


                Assert.GreaterOrEqual(DateTime.Now, bo2.DateTime);

                bo2.Info = "TwoUpdated";
                bo2.Int = 9923;
                bo2.NullableBool = false;
                bo2.DateTime = DateTime.Now.AddDays(10);

                db.UpdateParam(bo2);

                var bo2Check = db.GetById<ParamTestBO>(2);

                Assert.Less(DateTime.Now, bo2.DateTime);
                Assert.AreEqual("TwoUpdated", bo2Check.Info);
                Assert.AreEqual(9923, bo2Check.Int);
                Assert.AreEqual(false, bo2Check.NullableBool);
            }
        }

        [Test]
        public void ORA_ParamTestDelete()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                db.Insert(new ParamTestBO() { Id = 1 });
                db.Insert(new ParamTestBO() { Id = 2 });
                db.Insert(new ParamTestBO() { Id = 3 });

                Assert.IsNotNull(db.Select<ParamTestBO>(q => q.Id == 1).FirstOrDefault());
                Assert.IsNotNull(db.Select<ParamTestBO>(q => q.Id == 2).FirstOrDefault());
                Assert.IsNotNull(db.Select<ParamTestBO>(q => q.Id == 3).FirstOrDefault());

                db.DeleteByIdParam<ParamTestBO>(1);
                db.DeleteByIdParam<ParamTestBO>(2);
                db.DeleteByIdParam<ParamTestBO>(3);

                Assert.IsNull(db.Select<ParamTestBO>(q => q.Id == 1).FirstOrDefault());
                Assert.IsNull(db.Select<ParamTestBO>(q => q.Id == 2).FirstOrDefault());
                Assert.IsNull(db.Select<ParamTestBO>(q => q.Id == 3).FirstOrDefault());
            }
        }

        [Test]
        public void ORA_ParamTestGetById()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                db.Insert(new ParamTestBO() { Id = 1, Info = "Item1" });
                db.Insert(new ParamTestBO() { Id = 2, Info = "Item2" });
                db.Insert(new ParamTestBO() { Id = 3, Info = "Item3" });

                Assert.AreEqual("Item1", db.GetByIdParam<ParamTestBO>(1).Info);
                Assert.AreEqual("Item2", db.GetByIdParam<ParamTestBO>(2).Info);
                Assert.AreEqual("Item3", db.GetByIdParam<ParamTestBO>(3).Info);
            }
        }

        [Test]
        public void ORA_ParamTestSelectLambda()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                db.InsertParam(new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = null });
                db.InsertParam(new ParamTestBO() { Id = 2, Double = 0.002, Int = 200, Info = "Two", NullableBool = true });
                db.InsertParam(new ParamTestBO() { Id = 3, Double = 0.003, Int = 300, Info = "Three", NullableBool = false });
                db.InsertParam(new ParamTestBO() { Id = 4, Double = 0.004, Int = 400, Info = "Four", NullableBool = null });

                //select multiple items
                Assert.AreEqual(2, db.Select<ParamTestBO>(q => q.NullableBool == null).Count);
                Assert.AreEqual(2, db.SelectParam<ParamTestBO>(q => q.NullableBool == null).Count);
                Assert.AreEqual(1, db.SelectParam<ParamTestBO>(q => q.NullableBool == true).Count);
                Assert.AreEqual(1, db.SelectParam<ParamTestBO>(q => q.NullableBool == false).Count);

                Assert.AreEqual(1, db.SelectParam<ParamTestBO>(q => q.Info == "Two").Count);
                Assert.AreEqual(1, db.SelectParam<ParamTestBO>(q => q.Int == 300).Count);
                Assert.AreEqual(1, db.SelectParam<ParamTestBO>(q => q.Double == 0.003).Count);
            }
        }

        [Test]
        public void ORA_ParamTestSelectLambda2()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                db.InsertParam(new ParamTestBO() { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = null });
                db.InsertParam(new ParamTestBO() { Id = 2, Double = 0.002, Int = 200, Info = "Two", NullableBool = true });
                db.InsertParam(new ParamTestBO() { Id = 3, Double = 0.003, Int = 300, Info = "Three", NullableBool = false });
                db.InsertParam(new ParamTestBO() { Id = 4, Double = 0.004, Int = 400, Info = "Four", NullableBool = null });

                db.InsertParam(new ParamRelBO() { PTId = 1, Info = "T1" });
                db.InsertParam(new ParamRelBO() { PTId = 1, Info = "T1" });
                db.InsertParam(new ParamRelBO() { PTId = 1, Info = "T1" });
                db.InsertParam(new ParamRelBO() { PTId = 1, Info = "T1" });
                db.InsertParam(new ParamRelBO() { PTId = 2, Info = "T1" });
                db.InsertParam(new ParamRelBO() { PTId = 2, Info = "T1" });
                db.InsertParam(new ParamRelBO() { PTId = 3, Info = "T1" });
                db.InsertParam(new ParamRelBO() { PTId = 4, Info = "T1" });
                db.InsertParam(new ParamRelBO() { PTId = 3, Info = "T2" });
                db.InsertParam(new ParamRelBO() { PTId = 4, Info = "T2" });

                Assert.AreEqual(8, db.SelectParam<ParamRelBO>(q => q.Info == "T1").Count);
                Assert.AreEqual(2, db.SelectParam<ParamRelBO>(q => q.Info == "T2").Count);
               
                Assert.AreEqual(3, db.SelectParam<ParamRelBO>(q => q.Info == "T1" && (q.PTId == 2 || q.PTId == 3) ).Count);
            }
        }

        [Test]
        public void ORA_ParamTestSelectLambdaComplex()
        {
            using (var db = OpenDbConnection())
            {
                //various special cases that still need to be addressed

                //Assert.AreEqual(10, db.SelectParameterized<ParamRelBO>(q => Sql.In(q.PTId, "T1", "T2")));
                //Assert.AreEqual(10, db.SelectParameterized<ParamRelBO>(q => q.Info.StartsWith("T")));
                //Assert.AreEqual(10, db.SelectParameterized<ParamRelBO>(q => q.Info.EndsWith("1")));
                //Assert.AreEqual(10, db.SelectParameterized<ParamRelBO>(q => q.Info.Contains("T")));
            }
        }

        [Test]
        public void ORA_ParamByteTest()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                db.DeleteAll<ParamByteBO>();
                var bo1 = new ParamByteBO() { Id = 1, Data = new byte[] { 1, 25, 43, 3, 1, 66, 82, 23, 11, 44, 66, 22, 52, 62, 76, 19, 30, 91, 4 } };

                db.InsertParam(bo1);
                var bo1Check = db.SelectParam<ParamByteBO>(s => s.Id == bo1.Id).Single();

                Assert.AreEqual(bo1.Id, bo1Check.Id);
                Assert.AreEqual(bo1.Data, bo1Check.Data);

                db.DeleteAll<ParamByteBO>();
            }
        }



        public class ParamTestBO
        {
            public int Id { get; set; }
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

        public class ParamByteBO
        {
            public int Id { get; set; }
            public byte[] Data { get; set; }
        }
    }
}
