using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.Shared;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class CaptureSqlFilterTests
        : OrmLiteTestBase
    {
        [Test]
        public void Can_capture_each_type_of_API()
        {
            using (var captured = new CaptureSqlFilter())
            using (var db = OpenDbConnection())
            {
                db.CreateTable<Person>();
                db.Select<Person>(x => x.Age > 40);
                db.Single<Person>(x => x.Age == 42);
                db.Count<Person>(x => x.Age < 50);
                db.Insert(new Person { Id = 7, FirstName = "Amy", LastName = "Winehouse" });
                db.Update(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix" });
                db.Delete<Person>(new { FirstName = "Jimi", Age = 27 });
                db.SqlColumn<string>("SELECT LastName FROM Person WHERE Age < @age", 
                    new { age = 50 });
                db.SqlList<Person>("exec sp_name @firstName, @age", 
                    new { firstName = "aName", age = 1 });
                db.ExecuteNonQuery("UPDATE Person SET LastName={0} WHERE Id={1}"
                    .SqlFmt("WaterHouse", 7));

                var sql = string.Join(";\n\n", captured.SqlStatements.ToArray());
                sql.Print();
            }
        }

        [Test]
        public void Can_capture_CreateTable_APIs()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<Person>();
            }

            using (var captured = new CaptureSqlFilter())
            using (var db = OpenDbConnection())
            {
                int i = 0;
                i++; db.CreateTable<Person>();

                Assert.That(captured.SqlStatements.Last().NormalizeSql(),
                            Is.StringStarting("create table person"));

                Assert.That(captured.SqlStatements.Count, Is.EqualTo(i));

                var sql = string.Join(";\n", captured.SqlStatements.ToArray());
                sql.Print();
            }
        }

        [Test]
        public void Can_capture_Select_APIs()
        {
            using (var captured = new CaptureSqlFilter())
            using (var db = OpenDbConnection())
            {
                int i = 0;
                i++; db.Select<Person>(x => x.Age > 40);

                Assert.That(captured.SqlStatements.Last().NormalizeSql(),
                    Is.EqualTo("select id, firstname, lastname, age  from person where (age > 40)"));

                i++; db.Select<Person>(q => q.Where(x => x.Age > 40));
                i++; db.Select(db.From<Person>().Where(x => x.Age > 40));
                i++; db.Select<Person>("Age > 40");
                i++; db.Select<Person>("SELECT * FROM Person WHERE Age > 40");
                i++; db.Select<Person>("Age > @age", new { age = 40 });
                i++; db.Select<Person>("SELECT * FROM Person WHERE Age > @age", new { age = 40 });
                i++; db.Select<Person>("Age > @age", new Dictionary<string, object> { { "age", 40 } });
                i++; db.SelectFmt<Person>("Age > {0}", 40);
                i++; db.SelectFmt<Person>("SELECT * FROM Person WHERE Age > {0}", 40);
                i++; db.Where<Person>("Age", 27);
                i++; db.Where<Person>(new { Age = 27 });
                i++; db.SelectByIds<Person>(new[] { 1, 2, 3 });
                i++; db.SelectByIds<Person>(new[] { 1, 2, 3 });
                i++; db.SelectNonDefaults(new Person { Id = 1 });
                i++; db.SelectNonDefaults("Age > @Age", new Person { Age = 40 });
                i++; db.SelectLazy<Person>().ToList();
                i++; db.WhereLazy<Person>(new { Age = 27 }).ToList();
                i++; db.Select<Person>();
                i++; db.Single<Person>(x => x.Age == 42);
                i++; db.Single(db.From<Person>().Where(x => x.Age == 42));
                i++; db.Single<Person>(new { Age = 42 });
                i++; db.Single<Person>("Age = @age", new { age = 42 });
                i++; db.SingleById<Person>(1);
                i++; db.SingleWhere<Person>("Age", 42);
                i++; db.Exists<Person>(new { Age = 42 });
                i++; db.Exists<Person>("SELECT * FROM Person WHERE Age = @age", new { age = 42 });
                i++; db.ExistsFmt<Person>("Age = {0}", 42);
                i++; db.ExistsFmt<Person>("SELECT * FROM Person WHERE Age = {0}", 42);

                Assert.That(captured.SqlStatements.Count, Is.EqualTo(i));

                var sql = string.Join(";\n", captured.SqlStatements.ToArray());
                sql.Print();
            }
        }

        [Test]
        public void Can_capture_all_Single_Apis()
        {
            using (var captured = new CaptureSqlFilter())
            using (var db = OpenDbConnection())
            {
                int i = 0;
                i++; db.Single<Person>(x => x.Age == 42);

                Assert.That(captured.SqlStatements.Last().NormalizeSql(),
                    Is.EqualTo("select id, firstname, lastname, age  from person where (age = 42) limit 1").
                    Or.EqualTo("select top 1 id, firstname, lastname, age  from person where (age = 42)").
                    Or.EqualTo("select id, firstname, lastname, age  from person where (age = 42) order by 1 offset 0 rows fetch next 1 rows only"). //VistaDB
                    Or.EqualTo("select * from (\r select ssormlite1.*, rownum rnum from (\r select id, firstname, lastname, age  from person where (age = 42) order by person.id) ssormlite1\r where rownum <= 0 + 1) ssormlite2 where ssormlite2.rnum > 0")); //Oracle

                i++; db.ExistsFmt<Person>("Age = {0}", 42);
                i++; db.Single(db.From<Person>().Where(x => x.Age == 42));
                i++; db.Single<Person>(new { Age = 42 });
                i++; db.Single<Person>("Age = @age", new { age = 42 });
                i++; db.SingleFmt<Person>("Age = {0}", 42);
                i++; db.SingleById<Person>(1);
                i++; db.ExistsFmt<Person>("Age = {0}", 42);
                i++; db.SingleWhere<Person>("Age", 42);

                Assert.That(captured.SqlStatements.Count, Is.EqualTo(i));

                var sql = string.Join(";\n", captured.SqlStatements.ToArray());
                sql.Print();
            }
        }

        [Test]
        public void Can_capture_all_Scalar_Apis()
        {
            using (var captured = new CaptureSqlFilter())
            using (var db = OpenDbConnection())
            {
                int i = 0;
                i++; db.Scalar<Person, int>(x => Sql.Max(x.Age));

                Assert.That(captured.SqlStatements.Last().NormalizeSql(),
                    Is.EqualTo("select max(age)  from person"));

                i++; db.Scalar<Person, int>(x => Sql.Max(x.Age));
                i++; db.Scalar<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
                i++; db.Count<Person>(x => x.Age < 50);
                i++; db.Count(db.From<Person>().Where(x => x.Age < 50));
                i++; db.Scalar<int>("SELECT COUNT(*) FROM Person WHERE Age > @age", new { age = 40 });
                i++; db.ScalarFmt<int>("SELECT COUNT(*) FROM Person WHERE Age > {0}", 40);

                i++; db.SqlScalar<int>("SELECT COUNT(*) FROM Person WHERE Age < @age", new { age = 50 });
                i++; db.SqlScalar<int>("SELECT COUNT(*) FROM Person WHERE Age < @age", new Dictionary<string, object> { { "age", 50 } });

                Assert.That(captured.SqlStatements.Count, Is.EqualTo(i));

                var sql = string.Join(";\n", captured.SqlStatements.ToArray());
                sql.Print();
            }
        }


        [Test]
        public void Can_capture_Update_Apis()
        {
            using (var captured = new CaptureSqlFilter())
            using (var db = OpenDbConnection())
            {
                int i = 0;
                i++; db.Update(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });

                Assert.That(captured.SqlStatements.Last().NormalizeSql(),
                    Is.StringStarting("update person set firstname=@firstname, lastname=@lastname"));

                i++; db.Update(new[] { new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 } });
                i++; db.UpdateAll(new[] { new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 } });
                i++; db.Update(new Person { Id = 1, FirstName = "JJ", Age = 27 }, p => p.LastName == "Hendrix");
                i++; db.Update<Person>(new { FirstName = "JJ" }, p => p.LastName == "Hendrix");
                i++; db.UpdateNonDefaults(new Person { FirstName = "JJ" }, p => p.LastName == "Hendrix");
                i++; db.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName);
                i++; db.UpdateOnly(new Person { FirstName = "JJ" }, p => p.FirstName, p => p.LastName == "Hendrix");
                i++; db.UpdateOnly(new Person { FirstName = "JJ", LastName = "Hendo" }, ev => ev.Update(p => p.FirstName));
                i++; db.UpdateOnly(new Person { FirstName = "JJ" }, ev => ev.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
                i++; db.UpdateFmt<Person>(set: "FirstName = {0}".SqlFmt("JJ"), where: "LastName = {0}".SqlFmt("Hendrix"));
                i++; db.UpdateFmt(table: "Person", set: "FirstName = {0}".SqlFmt("JJ"), where: "LastName = {0}".SqlFmt("Hendrix"));

                Assert.That(captured.SqlStatements.Count, Is.EqualTo(i));

                var sql = string.Join(";\n", captured.SqlStatements.ToArray());
                sql.Print();
            }
        }

        [Test]
        public void Can_capture_Delete_Apis()
        {
            using (var captured = new CaptureSqlFilter())
            using (var db = OpenDbConnection())
            {
                int i = 0;
                i++; db.Delete<Person>(new { FirstName = "Jimi", Age = 27 });

                Assert.That(captured.SqlStatements.Last().NormalizeSql(),
                    Is.EqualTo("delete from person where firstname=@firstname and age=@age"));

                i++; db.Delete<Person>(new { FirstName = "Jimi", Age = 27 });
                i++; db.Delete(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });
                i++; db.DeleteNonDefaults(new Person { FirstName = "Jimi", Age = 27 });
                i++; db.DeleteById<Person>(1);
                i++; db.DeleteByIds<Person>(new[] { 1, 2, 3 });
                i++; db.DeleteFmt<Person>("Age = {0}", 27);
                i++; db.DeleteFmt(typeof(Person), "Age = {0}", 27);
                i++; db.Delete<Person>(p => p.Age == 27);
                i++; db.Delete<Person>(ev => ev.Where(p => p.Age == 27));
                i++; db.Delete(db.From<Person>().Where(p => p.Age == 27));
                i++; db.DeleteFmt<Person>(where: "Age = {0}".SqlFmt(27));
                i++; db.DeleteFmt(table: "Person", where: "Age = {0}".SqlFmt(27));

                Assert.That(captured.SqlStatements.Count, Is.EqualTo(i));

                var sql = string.Join(";\n", captured.SqlStatements.ToArray());
                sql.Print();
            }
        }

        [Test]
        public void Can_capture_CustomSql_Apis()
        {
            using (var captured = new CaptureSqlFilter())
            using (var db = OpenDbConnection())
            {
                int i = 0;
                i++; db.SqlColumn<string>("SELECT LastName FROM Person WHERE Age < @age", new { age = 50 });

                Assert.That(captured.SqlStatements.Last().NormalizeSql(),
                    Is.EqualTo("select lastname from person where age < @age"));

                i++; db.SqlColumn<string>("SELECT LastName FROM Person WHERE Age < @age", new { age = 50 });
                i++; db.SqlColumn<string>("SELECT LastName FROM Person WHERE Age < @age", new Dictionary<string, object> { { "age", 50 } });
                i++; db.SqlScalar<int>("SELECT COUNT(*) FROM Person WHERE Age < @age", new { age = 50 });
                i++; db.SqlScalar<int>("SELECT COUNT(*) FROM Person WHERE Age < @age", new Dictionary<string, object> { { "age", 50 } });

                i++; db.ExecuteNonQuery("UPDATE Person SET LastName={0} WHERE Id={1}".SqlFmt("WaterHouse", 7));
                i++; db.ExecuteNonQuery("UPDATE Person SET LastName=@name WHERE Id=@id", new { name = "WaterHouse", id = 7 });

                i++; db.SqlList<Person>("exec sp_name @firstName, @age", new { firstName = "aName", age = 1 });
                i++; db.SqlScalar<Person>("exec sp_name @firstName, @age", new { firstName = "aName", age = 1 });

                Assert.That(captured.SqlStatements.Count, Is.EqualTo(i));

                var sql = string.Join(";\n", captured.SqlStatements.ToArray());
                sql.Print();
            }
        }

        [Test]
        public void Can_capture_Insert_Apis()
        {
            using (var captured = new CaptureSqlFilter())
            using (var db = OpenDbConnection())
            {
                int i = 0;
                i++; db.Insert(new Person { Id = 7, FirstName = "Amy", LastName = "Winehouse", Age = 27 });

                Assert.That(captured.SqlStatements.Last().NormalizeSql(),
                    Is.StringStarting("insert into person (id,firstname,lastname,age) values"));

                i++; db.Insert(new Person { Id = 7, FirstName = "Amy", LastName = "Winehouse", Age = 27 });
                i++; db.InsertAll(new[] { new Person { Id = 10, FirstName = "Biggie", LastName = "Smalls", Age = 24 } });
                i++; db.InsertOnly(new PersonWithAutoId { FirstName = "Amy", Age = 27 }, ev => ev.Insert(p => new { p.FirstName, p.Age }));
                i++; db.InsertOnly(new PersonWithAutoId { FirstName = "Amy", Age = 27 }, ev => db.From<PersonWithAutoId>().Insert(p => new { p.FirstName, p.Age }));

                Assert.That(captured.SqlStatements.Count, Is.EqualTo(i));

                var sql = string.Join(";\n", captured.SqlStatements.ToArray());
                sql.Print();
            }
        }

    }
}