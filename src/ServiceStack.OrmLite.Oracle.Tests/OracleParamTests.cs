using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class OracleParamTests : OrmLiteTestBase
    {
        private void DropAndCreateTables(IDbConnection db)
        {
            if (db.TableExists("ParamRelBO"))
                db.DropTable<ParamRelBo>();

            db.CreateTable<ParamTestBo>(true);
            db.CreateTable<ParamRelBo>(true);
            db.CreateTable<ParamByteBo>(true);
            db.CreateTable<ParamComment>(true);
            db.CreateTable<ParamOrder>(true);
            db.CreateTable<ParamLeft>(true);
            db.CreateTable<ParamUser>(true);
            db.CreateTable<ParamPassword>(true);
            db.CreateTable<ParamActive>(true);
            db.CreateTable<ParamDouble>(true);
            db.CreateTable<ParamFloat>(true);
            db.CreateTable<ParamDecimal>(true);
            db.CreateTable<ParamString>(true);
            db.CreateTable<ParamDate>(true);
            db.CreateTable<ParamDateTime>(true);
            db.CreateTable<ParamType>(true);
            db.CreateTable<ParamTimestamp>(true);
        }

        [Test]
        public void ORA_ParamTestInsert()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);
                var dateTimeNow =new DateTime( DateTime.Now.Year,  DateTime.Now.Month,  DateTime.Now.Day);

                db.Insert(new ParamTestBo { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = null, DateTime = dateTimeNow });
                db.Insert(new ParamTestBo { Id = 2, Double = 0.002, Int = 200, Info = "Two", NullableBool = true, DateTime = dateTimeNow });
                db.Insert(new ParamTestBo { Id = 3, Double = 0.003, Int = 300, Info = "Three", NullableBool = false, DateTime = dateTimeNow.AddDays(23) });
                db.Insert(new ParamTestBo { Id = 4, Double = 0.004, Int = 400, Info = "Four", NullableBool = null });

                var bo1 = db.Select<ParamTestBo>(q => q.Id == 1).Single();
                var bo2 = db.Select<ParamTestBo>(q => q.Id == 2).Single();
                var bo3 = db.Select<ParamTestBo>(q => q.Id == 3).Single();
                var bo4 = db.Select<ParamTestBo>(q => q.Id == 4).Single();

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

                var bo1 = new ParamTestBo { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = true };
                var bo2 = new ParamTestBo { Id = 2, Double = 0.002, Int = 200, Info = "Two", NullableBool = true, DateTime = DateTime.Now };
                db.Insert(bo1);
                db.Insert(bo2);

                bo1.Double = 0.01;
                bo1.Int = 10000;
                bo1.Info = "OneUpdated";
                bo1.NullableBool = null;
                bo1.DateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

                db.Update(bo1);

                var bo1Check = db.SingleById<ParamTestBo>(1);

                Assert.AreEqual(bo1.Double, bo1Check.Double);
                Assert.AreEqual(bo1.Int, bo1Check.Int);
                Assert.AreEqual(bo1.Info, bo1Check.Info);
                Assert.AreEqual(bo1.DateTime, bo1Check.DateTime);


                Assert.GreaterOrEqual(DateTime.Now, bo2.DateTime);

                bo2.Info = "TwoUpdated";
                bo2.Int = 9923;
                bo2.NullableBool = false;
                bo2.DateTime = DateTime.Now.AddDays(10);

                db.Update(bo2);

                var bo2Check = db.SingleById<ParamTestBo>(2);

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

                db.Insert(new ParamTestBo { Id = 1 });
                db.Insert(new ParamTestBo { Id = 2 });
                db.Insert(new ParamTestBo { Id = 3 });

                Assert.IsNotNull(db.Select<ParamTestBo>(q => q.Id == 1).FirstOrDefault());
                Assert.IsNotNull(db.Select<ParamTestBo>(q => q.Id == 2).FirstOrDefault());
                Assert.IsNotNull(db.Select<ParamTestBo>(q => q.Id == 3).FirstOrDefault());

                db.DeleteById<ParamTestBo>(1);
                db.DeleteById<ParamTestBo>(2);
                db.DeleteById<ParamTestBo>(3);

                Assert.IsNull(db.Select<ParamTestBo>(q => q.Id == 1).FirstOrDefault());
                Assert.IsNull(db.Select<ParamTestBo>(q => q.Id == 2).FirstOrDefault());
                Assert.IsNull(db.Select<ParamTestBo>(q => q.Id == 3).FirstOrDefault());
            }
        }

        [Test]
        public void ORA_ParamTestGetById()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                db.Insert(new ParamTestBo { Id = 1, Info = "Item1" });
                db.Insert(new ParamTestBo { Id = 2, Info = "Item2" });
                db.Insert(new ParamTestBo { Id = 3, Info = "Item3" });

                Assert.AreEqual("Item1", db.SingleById<ParamTestBo>(1).Info);
                Assert.AreEqual("Item2", db.SingleById<ParamTestBo>(2).Info);
                Assert.AreEqual("Item3", db.SingleById<ParamTestBo>(3).Info);
            }
        }

        [Test]
        public void ORA_ParamTestSelectLambda()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                LoadParamTestBo(db);

                //select multiple items
                Assert.AreEqual(2, db.Select<ParamTestBo>(q => q.NullableBool == null).Count);
                Assert.AreEqual(2, db.Select<ParamTestBo>(q => q.NullableBool == null).Count);
                Assert.AreEqual(1, db.Select<ParamTestBo>(q => q.NullableBool == true).Count);
                Assert.AreEqual(1, db.Select<ParamTestBo>(q => q.NullableBool == false).Count);

                Assert.AreEqual(1, db.Select<ParamTestBo>(q => q.Info == "Two").Count);
                Assert.AreEqual(1, db.Select<ParamTestBo>(q => q.Int == 300).Count);
                Assert.AreEqual(1, db.Select<ParamTestBo>(q => q.Double == 0.003).Count);
            }
        }

        private void LoadParamTestBo(IDbConnection db)
        {
            db.Insert(new ParamTestBo { Id = 1, Double = 0.001, Int = 100, Info = "One", NullableBool = null });
            db.Insert(new ParamTestBo { Id = 2, Double = 0.002, Int = 200, Info = "Two", NullableBool = true });
            db.Insert(new ParamTestBo { Id = 3, Double = 0.003, Int = 300, Info = "Three", NullableBool = false });
            db.Insert(new ParamTestBo { Id = 4, Double = 0.004, Int = 400, Info = "Four", NullableBool = null });
        }

        [Test]
        public void ORA_ParamTestSelectLambda2()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                LoadParamTestBo(db);
                LoadParamRelBo(db);

                Assert.AreEqual(8, db.Select<ParamRelBo>(q => q.Info == "T1").Count);
                Assert.AreEqual(2, db.Select<ParamRelBo>(q => q.Info == "T2").Count);
               
                Assert.AreEqual(3, db.Select<ParamRelBo>(q => q.Info == "T1" && (q.PtId == 2 || q.PtId == 3) ).Count);
            }
        }

        private void LoadParamRelBo(IDbConnection db)
        {
            db.Insert(new ParamRelBo { PtId = 1, Info = "T1" });
            db.Insert(new ParamRelBo { PtId = 1, Info = "T1" });
            db.Insert(new ParamRelBo { PtId = 1, Info = "T1" });
            db.Insert(new ParamRelBo { PtId = 1, Info = "T1" });
            db.Insert(new ParamRelBo { PtId = 2, Info = "T1" });
            db.Insert(new ParamRelBo { PtId = 2, Info = "T1" });
            db.Insert(new ParamRelBo { PtId = 3, Info = "T1" });
            db.Insert(new ParamRelBo { PtId = 4, Info = "T1" });
            db.Insert(new ParamRelBo { PtId = 3, Info = "T2" });
            db.Insert(new ParamRelBo { PtId = 4, Info = "T2" });
        }

        [Test]
        public void ORA_ParamTestSelectLambdaComplex()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                LoadParamTestBo(db);
                LoadParamRelBo(db);

                Assert.AreEqual(10, db.Select<ParamRelBo>(q => Sql.In(q.Info, "T1", "T2")).Count);
                Assert.AreEqual(10, db.Select<ParamRelBo>(q => q.Info.StartsWith("T")).Count);
                Assert.AreEqual(8, db.Select<ParamRelBo>(q => q.Info.EndsWith("1")).Count);
                Assert.AreEqual(10, db.Select<ParamRelBo>(q => q.Info.Contains("T")).Count);
            }
        }

        [Test]
        public void ORA_ParamByteTest()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                db.DeleteAll<ParamByteBo>();
                var bo1 = new ParamByteBo { Id = 1, Data = new byte[] { 1, 25, 43, 3, 1, 66, 82, 23, 11, 44, 66, 22, 52, 62, 76, 19, 30, 91, 4 } };

                db.Insert(bo1);
                var bo1Check = db.Select<ParamByteBo>(s => s.Id == bo1.Id).Single();

                Assert.AreEqual(bo1.Id, bo1Check.Id);
                Assert.AreEqual(bo1.Data, bo1Check.Data);

                db.DeleteAll<ParamByteBo>();
            }
        }

        [Test]
        public void ORA_ReservedNameComment_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamComment { Comment = 1, Id = 2 };
                db.Insert(row);

                row.Comment = 454;
                db.Update(row);
            }
        }


        [Test]
        public void ORA_ReservedNameLeft_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamLeft { Id = 2, Left = 3 };
                db.Insert(row);

                row.Left = 454;
                db.Update(row);
            }
        }

        [Test]
        public void ORA_ReservedNameOrder_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamOrder { Id = 2, Order = 4 };
                db.Insert(row);

                row.Order = 67;
                db.Update(row);
            }
        }

        [Test]
        public void ORA_ReservedNameUser_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamUser { Id = 2, User = 5 };
                db.Insert(row);

                row.User = 35;
                db.Update(row);
            }
        }

        [Test]
        public void ORA_ReservedNamePassword_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamPassword { Id = 2, Password = 6 };
                db.Insert(row);

                row.Password = 335;
                db.Update(row);
            }
        }

        [Test]
        public void ORA_ReservedNameActive_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamActive { Id = 2, Active = 7 };
                db.Insert(row);

                row.Active = 454;
                db.Update(row);
            }
        }

        [Test]
        public void ORA_ReservedNameDouble_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamDouble { Id = 2, Double = 8 };
                db.Insert(row);

                row.Double = 4876554;
                db.Update(row);
            }
        }

        [Test]
        public void ORA_ReservedNameFloat_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamFloat { Id = 2, Float = 9 };
                db.Insert(row);

                row.Float = 798;
                db.Update(row);
            }
        }

        [Test]
        public void ORA_ReservedNameDecimal_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamDecimal { Id = 2, Decimal = 10 };
                db.Insert(row);

                row.Decimal = 454;
                db.Update(row);
            }
        }

        [Test]
        public void ORA_ReservedNameString_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamString { Id = 2, String = 11 };
                db.Insert(row);

                row.String = 234234;
                db.Update(row);
            }
        }

        [Test]
        public void ORA_ReservedNameDate_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamDate { Id = 2, Date = 12 };
                db.Insert(row);

                row.Date = 826;
                db.Update(row);
            }
        }

        [Test]
        public void ORA_ReservedNameDateTime_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamDateTime { Id = 2, DateTime = 13 };
                db.Insert(row);

                row.DateTime = 327895;
                db.Update(row);
            }
        }

        [Test]
        public void ORA_ReservedNameType_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamType { Id = 2, Type = 5 };
                db.Insert(row);

                row.Type = 454;
                db.Update(row);
            }
        }

        [Test]
        public void ORA_ReservedNameTimestamp_Test()
        {
            using (var db = OpenDbConnection())
            {
                DropAndCreateTables(db);

                var row = new ParamTimestamp { Id = 2, Timestamp = 27 };
                db.Insert(row);

                row.Timestamp = 28454;
                db.Update(row);
            }
        }

        public class ParamTestBo
        {
            public int Id { get; set; }
            public string Info { get; set; }
            public int Int { get; set; }
            public double Double { get; set; }
            public bool? NullableBool { get; set; }
            public DateTime? DateTime { get; set; }
        }


        public class ParamRelBo
        {
            [Sequence("SEQ_PARAMTESTREL_ID")]
            [PrimaryKey]
            [Alias("ParamRel_Id")]
            public int Id { get; set; }
            [ForeignKey(typeof(ParamTestBo))]
            public int PtId { get; set; }

            [Alias("InfoStr")]
            public string Info { get; set; }
        }

        public class ParamByteBo
        {
            public int Id { get; set; }
            public byte[] Data { get; set; }
        }

        public class ParamComment
        {
            public int Id { get; set; }
            public int Comment { get; set; }
        }

        public class ParamOrder
        {
            public int Id { get; set; }
            public int Order { get; set; }
        }

        public class ParamLeft
        {
            public int Id { get; set; }
            public int Left { get; set; }
        }

        public class ParamUser
        {
            public int Id { get; set; }
            public int User { get; set; }
        }

        public class ParamPassword
        {
            public int Id { get; set; }
            public int Password { get; set; }
        }

        public class ParamActive
        {
            public int Id { get; set; }
            public int Active { get; set; }
        }

        public class ParamDouble
        {
            public int Id { get; set; }
            public int Double { get; set; }
        }

        public class ParamFloat
        {
            public int Id { get; set; }
            public int Float { get; set; }
        }

        public class ParamDecimal
        {
            public int Id { get; set; }
            public int Decimal { get; set; }
        }

        public class ParamString
        {
            public int Id { get; set; }
            public int String { get; set; }
        }

        public class ParamDate
        {
            public int Id { get; set; }
            public int Date { get; set; }
        }

        public class ParamDateTime
        {
            public int Id { get; set; }
            public int DateTime { get; set; }
        }

        public class ParamType
        {
            public int Id { get; set; }
            public int Type { get; set; }
        }

        public class ParamTimestamp
        {
            public int Id { get; set; }
            public int Timestamp { get; set; }
        }
    }
}
