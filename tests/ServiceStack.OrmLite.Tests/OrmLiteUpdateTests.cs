using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Tests.Shared;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class OrmLiteUpdateTests
        : OrmLiteTestBase
    {
        private ModelWithFieldsOfDifferentTypes CreateModelWithFieldsOfDifferentTypes(IDbConnection db)
        {
            db.DropAndCreateTable<ModelWithFieldsOfDifferentTypes>();

            var row = ModelWithFieldsOfDifferentTypes.Create(1);
            return row;
        }

        [Test]
        public void Can_update_ModelWithFieldsOfDifferentTypes_table()
        {
            using (var db = OpenDbConnection())
            {
                var row = CreateModelWithFieldsOfDifferentTypes(db);

                row.Id = (int)db.Insert(row, selectIdentity: true);

                row.Name = "UpdatedName";

                db.Update(row);

                var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);

                ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
            }
        }

        [Test]
        public void Can_update_ModelWithFieldsOfDifferentTypes_table_with_filter()
        {
            using (var db = OpenDbConnection())
            {
                var row = CreateModelWithFieldsOfDifferentTypes(db);

                row.Id = (int)db.Insert(row, selectIdentity: true);

                row.Name = "UpdatedName";

                db.Update(row, x => x.LongId <= row.LongId);

                var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);

                ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
            }
        }

        [Test]
        public void Can_update_with_anonymousType_and_expr_filter()
        {
            using (var db = OpenDbConnection())
            {
                var row = CreateModelWithFieldsOfDifferentTypes(db);

                row.Id = (int) db.Insert(row, selectIdentity: true);
                row.DateTime = DateTime.Now;
                row.Name = "UpdatedName";

                db.Update<ModelWithFieldsOfDifferentTypes>(new {row.Name, row.DateTime},
                    x => x.LongId >= row.LongId && x.LongId <= row.LongId);

                var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);
                Console.WriteLine(dbRow.Dump());
                ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
            }
        }

        [Test]
        public void Can_update_with_optional_string_params()
        {
            using (var db = OpenDbConnection())
            {
                var row = CreateModelWithFieldsOfDifferentTypes(db);

                row.Id = (int)db.Insert(row, selectIdentity: true);
                row.Name = "UpdatedName";

                db.UpdateFmt<ModelWithFieldsOfDifferentTypes>(set: "NAME = {0}".SqlFmt(row.Name), where: "LongId".SqlColumn() + " <= {0}".SqlFmt(row.LongId));

                var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);
                Console.WriteLine(dbRow.Dump());
                ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
            }
        }

        [Test]
        public void Can_update_with_tableName_and_optional_string_params()
        {
            using (var db = OpenDbConnection())
            {
                var row = CreateModelWithFieldsOfDifferentTypes(db);

                row.Id = (int)db.Insert(row, selectIdentity: true);
                row.Name = "UpdatedName";

                db.UpdateFmt(table: "ModelWithFieldsOfDifferentTypes".SqlTableRaw(),
                    set: "NAME = {0}".SqlFmt(row.Name), where: "LongId".SqlColumn() + " <= {0}".SqlFmt(row.LongId));

                var dbRow = db.SingleById<ModelWithFieldsOfDifferentTypes>(row.Id);
                Console.WriteLine(dbRow.Dump());
                ModelWithFieldsOfDifferentTypes.AssertIsEqual(dbRow, row);
            }
        }

        [Test]
        public void Can_Update_Into_Table_With_Id_Only()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithIdOnly>(true);
                var row1 = new ModelWithIdOnly(1);
                db.Insert(row1);

                db.Update(row1);
            }
        }

        [Test]
        public void Can_Update_Many_Into_Table_With_Id_Only()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithIdOnly>(true);
                var row1 = new ModelWithIdOnly(1);
                var row2 = new ModelWithIdOnly(2);
                db.Insert(row1, row2);

                db.Update(row1, row2);

                var list = new List<ModelWithIdOnly> { row1, row2 };
                db.UpdateAll(list);
            }
        }

        [Test]
        public void Can_UpdateOnly_multiple_columns()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Person>();

                db.Insert(new Person { FirstName = "FirstName", Age = 100 });

                var existingPerson = db.Select<Person>().First();

                existingPerson.FirstName = "JJ";
                existingPerson.Age = 12;

                db.UpdateOnly(existingPerson,
                    onlyFields: p => new { p.FirstName, p.Age });

                var person = db.Select<Person>().First();

                Assert.That(person.FirstName, Is.EqualTo("JJ"));
                Assert.That(person.Age, Is.EqualTo(12));
            }
        }

        [Test]
        public void Can_Update_Only_Blobs()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<SomeBlobs>();

                db.Insert(new SomeBlobs { FirstName = "Bro", LastName = "Last" });
                db.Insert(new SomeBlobs { FirstName = "Sis", LastName = "Last" });

                var existing = db.Select<SomeBlobs>(p => p.FirstName == "Bro").First();

                const string blob1String = "This is going into Blob1";
                var blob1Array = blob1String.ToArray();
                var blob1Bytes = blob1Array.Length * 2;
                existing.Blob1 = new byte[blob1Bytes];
                Buffer.BlockCopy(blob1Array, 0, existing.Blob1, 0, blob1Bytes);

                const string blob2String = "And this is going into Blob2";
                var blob2Array = blob2String.ToArray();
                var blob2Bytes = blob2Array.Length * 2;
                existing.Blob2 = new byte[blob2Bytes];
                Buffer.BlockCopy(blob2Array, 0, existing.Blob2, 0, blob2Bytes);

                db.UpdateOnly(existing, p => new { p.Blob1, p.Blob2, p.FirstName }, r => r.LastName == "Last" && r.FirstName == "Bro");

                var verify = db.Select<SomeBlobs>(p => p.FirstName == "Bro").First();

                var verifyBlob1 = new char[verify.Blob1.Length / 2];
                Buffer.BlockCopy(verify.Blob1, 0, verifyBlob1, 0, verify.Blob1.Length);

                Assert.That(existing.Blob1, Is.EquivalentTo(verify.Blob1));
                Assert.That(existing.Blob2, Is.EquivalentTo(verify.Blob2));
            }
        }

        public class PocoWithBool
        {
            public int Id { get; set; }
            public bool Bool { get; set; }
        }

        public class PocoWithNullableBool
        {
            public int Id { get; set; }
            public bool? Bool { get; set; }
        }

        [Test]
        public void Can_UpdateOnly_bool_columns()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<PocoWithBool>();

                db.Insert(new PocoWithBool { Id = 1, Bool = false });
                var row = db.SingleById<PocoWithBool>(1);
                Assert.That(row.Bool, Is.False);

                db.UpdateNonDefaults(new PocoWithBool { Bool = true }, x => x.Id == 1);
                row = db.SingleById<PocoWithBool>(1);
                Assert.That(row.Bool, Is.True);

                Assert.Throws<ArgumentException>(() =>
                    db.UpdateNonDefaults(new PocoWithBool { Bool = false }, x => x.Id == 1));

                db.UpdateOnly(new PocoWithBool { Bool = false },
                    onlyFields: x => x.Bool,
                    where: x => x.Id == 1);
                row = db.SingleById<PocoWithBool>(1);
                Assert.That(row.Bool, Is.False);
            }
        }

        [Test]
        public void Can_UpdateOnly_nullable_bool_columns()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<PocoWithNullableBool>();

                db.Insert(new PocoWithNullableBool { Id = 1, Bool = true });
                var row = db.SingleById<PocoWithNullableBool>(1);
                Assert.That(row.Bool, Is.True);

                db.UpdateNonDefaults(new PocoWithNullableBool { Bool = false }, x => x.Id == 1);
                row = db.SingleById<PocoWithNullableBool>(1);
                Assert.That(row.Bool, Is.False);
            }
        }

        public class PocoWithNullableInt
        {
            public int Id { get; set; }
            public int? Int { get; set; }
        }

        [Test]
        public void Can_UpdateOnly_nullable_int_columns()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<PocoWithNullableInt>();

                db.Insert(new PocoWithNullableInt { Id = 1, Int = 1 });
                var row = db.SingleById<PocoWithNullableInt>(1);
                Assert.That(row.Int, Is.EqualTo(1));

                db.UpdateNonDefaults(new PocoWithNullableInt { Int = 0 }, x => x.Id == 1);
                row = db.SingleById<PocoWithNullableInt>(1);
                Assert.That(row.Int, Is.EqualTo(0));
            }
        }

        [Test]
        public void Does_Save_nullable_bool()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Shutdown>();

                db.Insert(new Shutdown { IsShutdownGraceful = null });
                var rows = db.Select<Shutdown>();

                Assert.That(rows.Count, Is.EqualTo(1));
                Assert.That(rows[0].IsShutdownGraceful, Is.Null);
            }
        }

        [Test]
        public void Can_updated_with_ExecuteSql_and_db_params()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Poco>();

                db.Insert(new Poco { Id = 1, Name = "A" });
                db.Insert(new Poco { Id = 2, Name = "B" });

                var sql = "UPDATE poco SET name = {0}name WHERE id = {0}id".Fmt(OrmLiteConfig.DialectProvider.ParamString);
                var result = db.ExecuteSql(sql, new { id = 2, name = "UPDATED" });
                Assert.That(result, Is.EqualTo(1));

                var row = db.SingleById<Poco>(2);
                Assert.That(row.Name, Is.EqualTo("UPDATED"));
            }
        }

        [Test]
        public void Does_Update_using_db_params()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Person>();
                db.InsertAll(Person.Rockstars);

                db.Update(new Person { Id = 1, FirstName = "JJ", Age = 27 }, p => p.LastName == "Hendrix");

                var sql = db.GetLastSql().NormalizeSql();
                Assert.That(sql, Is.StringContaining("where (lastname = @0)"));
                Assert.That(sql, Is.StringContaining("id=@1"));
                Assert.That(sql, Is.StringContaining("firstname=@2"));

                var row = db.SingleById<Person>(1);
                Assert.That(row.FirstName, Is.EqualTo("JJ"));
            }
        }

        [Test]
        public void Does_Update_anonymous_using_db_params()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Person>();
                db.InsertAll(Person.Rockstars);

                db.Update<Person>(new { FirstName = "JJ" }, p => p.LastName == "Hendrix");

                var sql = db.GetLastSql().NormalizeSql();
                Assert.That(sql, Is.StringContaining("where (lastname = @0)"));
                Assert.That(sql, Is.StringContaining("firstname=@1"));

                var row = db.SingleById<Person>(1);
                Assert.That(row.FirstName, Is.EqualTo("JJ"));
            }
        }

        [Test]
        public void Does_UpdateNonDefaults_using_db_params()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Person>();
                db.InsertAll(Person.Rockstars);

                db.UpdateNonDefaults(new Person { FirstName = "JJ" }, p => p.LastName == "Hendrix");

                var sql = db.GetLastSql().NormalizeSql();
                Assert.That(sql, Is.StringContaining("where (lastname = @0)"));
                Assert.That(sql, Is.StringContaining("firstname=@1"));

                var row = db.SingleById<Person>(1);
                Assert.That(row.FirstName, Is.EqualTo("JJ"));
            }
        }

        [Test]
        public void Does_UpdateOnly_using_db_params()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Person>();
                db.InsertAll(Person.Rockstars);

                db.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName, p => p.LastName == "Hendrix");

                var sql = db.GetLastSql().NormalizeSql();
                Assert.That(sql, Is.StringContaining("where (lastname = @0)"));
                Assert.That(sql, Is.StringContaining("firstname=@1"));

                var row = db.SingleById<Person>(1);
                Assert.That(row.FirstName, Is.EqualTo("JJ"));
            }
        }

        [Test]
        public void Does_UpdateOnly_with_SqlExpression_using_db_params()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Person>();
                db.InsertAll(Person.Rockstars);

                db.UpdateOnly(new Person { FirstName = "JJ" }, q => q.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));

                var sql = db.GetLastSql().NormalizeSql();
                Assert.That(sql, Is.StringContaining("where (firstname = @0)"));
                Assert.That(sql, Is.StringContaining("firstname=@1"));

                var row = db.SingleById<Person>(1);
                Assert.That(row.FirstName, Is.EqualTo("JJ"));
            }
        }

    }

    [CompositeIndex("FirstName", "LastName")]
    public class SomeBlobs
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[] Blob1 { get; set; }
        public byte[] Blob2 { get; set; }
    }

    public class Shutdown
    {
        public int Id { get; set; }
        public bool? IsShutdownGraceful { get; set; }
    }
}
