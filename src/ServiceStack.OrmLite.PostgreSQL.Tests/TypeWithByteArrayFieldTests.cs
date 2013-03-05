using NUnit.Framework;
using ServiceStack.OrmLite.Tests;

namespace ServiceStack.OrmLite.PostgreSQL.Tests
{
    public class TypeWithByteArrayFieldTests : OrmLiteTestBase
    {        
        [Test]
        public void CanInsertAndSelectByteArray()
        {
            var orig = new TypeWithByteArrayField { Id = 1, Content = new byte[] { 0, 17, 0, 17, 0, 7 } };

            using (var db = ConnectionString.OpenDbConnection())
            {
                db.CreateTable<TypeWithByteArrayField>(true);

                db.Save(orig);

                var target = db.GetById<TypeWithByteArrayField>(orig.Id);

                Assert.AreEqual(orig.Id, target.Id);
                Assert.AreEqual(orig.Content, target.Content);
            }
        }

        [Test]
        public void CanInsertAndSelectByteArray__manual_insert__manual_select()
        {
            var orig = new TypeWithByteArrayField { Id = 1, Content = new byte[] { 0, 17, 0, 17, 0, 7 } };

            using(var db = ConnectionString.OpenDbConnection()) {
                //insert and select manually - ok
                db.CreateTable<TypeWithByteArrayField>(true);
                _insertManually(orig, db);

                _selectAndVerifyManually(orig, db);
            }
        }

        [Test]
        public void CanInsertAndSelectByteArray__InsertParam_insert__manual_select()
        {
            var orig = new TypeWithByteArrayField { Id = 1, Content = new byte[] { 0, 17, 0, 17, 0, 7 } };

            using(var db = ConnectionString.OpenDbConnection()) {
                //insert using InsertParam, and select manually - ok
                db.CreateTable<TypeWithByteArrayField>(true);
                db.InsertParam(orig);

                _selectAndVerifyManually(orig, db);
            }
        }

        [Test]
        public void CanInsertAndSelectByteArray__InsertParam_insert__GetById_select()
        {
            var orig = new TypeWithByteArrayField { Id = 1, Content = new byte[] { 0, 17, 0, 17, 0, 7 } };

            using(var db = ConnectionString.OpenDbConnection()) {
                //InsertParam + GetByID - fails
                db.CreateTable<TypeWithByteArrayField>(true);
                db.InsertParam(orig);

                var target = db.GetById<TypeWithByteArrayField>(orig.Id);

                Assert.AreEqual(orig.Id, target.Id);
                Assert.AreEqual(orig.Content, target.Content);
            }
        }

        [Test]
        public void CanInsertAndSelectByteArray__Insert_insert__GetById_select()
        {
            var orig = new TypeWithByteArrayField { Id = 1, Content = new byte[] { 0, 17, 0, 17, 0, 7 } };

            using(var db = ConnectionString.OpenDbConnection()) {
                //InsertParam + GetByID - fails
                db.CreateTable<TypeWithByteArrayField>(true);
                db.Insert(orig);

                var target = db.GetById<TypeWithByteArrayField>(orig.Id);

                Assert.AreEqual(orig.Id, target.Id);
                Assert.AreEqual(orig.Content, target.Content);
            }
        }

        [Test]
        public void CanInsertAndSelectByteArray__Insert_insert__manual_select()
        {
            var orig = new TypeWithByteArrayField { Id = 1, Content = new byte[] { 0, 17, 0, 17, 0, 7 } };

            using(var db = ConnectionString.OpenDbConnection()) {
                //InsertParam + GetByID - fails
                db.CreateTable<TypeWithByteArrayField>(true);
                db.Insert(orig);

                _selectAndVerifyManually(orig, db);
            }
        }

        private static void _selectAndVerifyManually(TypeWithByteArrayField orig, System.Data.IDbConnection db)
        {
            using(var cmd = db.CreateCommand()) {
                cmd.CommandText = @"select ""Content"" from ""TypeWithByteArrayField"" where ""Id"" = 1 --manual select";
                using(var reader = cmd.ExecuteReader()) {
                    reader.Read();
                    var ba = reader["Content"] as byte[];
                    Assert.AreEqual(orig.Content.Length, ba.Length);
                    Assert.AreEqual(orig.Content, ba);
                }
            }
        }

        private static void _insertManually(TypeWithByteArrayField orig, System.Data.IDbConnection db)
        {
            using(var cmd = db.CreateCommand()) {
                cmd.CommandText = @"INSERT INTO ""TypeWithByteArrayField"" (""Id"",""Content"") VALUES (@Id, @Content) --manual insert";

                var p_id = cmd.CreateParameter();
                p_id.ParameterName = "@Id";
                p_id.Value = orig.Id;

                cmd.Parameters.Add(p_id);

                var p_content = cmd.CreateParameter();
                p_content.ParameterName = "@Content";
                p_content.Value = orig.Content;

                cmd.Parameters.Add(p_content);

                cmd.ExecuteNonQuery();
            }
        }
    }

    class TypeWithByteArrayField
    {
        public int Id { get; set; }
        public byte[] Content { get; set; }
    }
}