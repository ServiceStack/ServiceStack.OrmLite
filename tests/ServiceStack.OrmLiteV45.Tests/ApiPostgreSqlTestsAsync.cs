﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.Shared;

namespace ServiceStack.OrmLite.Tests
{
    public class ApiPostgreSqlTestsAsync
        : OrmLiteTestBase
    {
        private IDbConnection db;

        [SetUp]
        public void SetUp()
        {
            SuppressIfOracle("SQL Server tests");
            db = CreatePostgreSqlDbFactory().OpenDbConnection();
            db.DropAndCreateTable<Person>();
            db.DropAndCreateTable<PersonWithAutoId>();
        }

        [TearDown]
        public void TearDown()
        {
            db.Dispose();
        }

        [Test]
        public async Task API_PostgreSql_Parameterized_Examples_Async()
        {
            var hold = OrmLiteConfig.UseParameterizeSqlExpressions;
            OrmLiteConfig.UseParameterizeSqlExpressions = true;

            await db.InsertAsync(Person.Rockstars);

            await db.SelectAsync<Person>(x => x.Age > 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" \nFROM \"person\"\nWHERE (\"age\" > :0)"));

            await db.SelectAsync<Person>(q => q.Where(x => x.Age > 40).OrderBy(x => x.Id));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" \nFROM \"person\"\nWHERE (\"age\" > :0)\nORDER BY \"id\""));

            await db.SelectAsync<Person>(q => q.Where(x => x.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" \nFROM \"person\"\nWHERE (\"age\" > :0)"));

            await db.SelectAsync(db.From<Person>().Where(x => x.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" \nFROM \"person\"\nWHERE (\"age\" > :0)"));

            await db.SingleAsync<Person>(x => x.Age == 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" \nFROM \"person\"\nWHERE (\"age\" = :0)\nLIMIT 1"));

            await db.SingleAsync<Person>(q => q.Where(x => x.Age == 42));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" \nFROM \"person\"\nWHERE (\"age\" = :0)\nLIMIT 1"));

            await db.SingleAsync(db.From<Person>().Where(x => x.Age == 42));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" \nFROM \"person\"\nWHERE (\"age\" = :0)\nLIMIT 1"));

            await db.ScalarAsync<Person, int>(x => Sql.Max(x.Age));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Max(\"age\") \nFROM \"person\""));

            await db.ScalarAsync<Person, int>(x => Sql.Max(x.Age), x => x.Age < 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Max(\"age\") \nFROM \"person\"\nWHERE (\"age\" < :0)"));

            await db.CountAsync<Person>(x => x.Age < 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"person\"\nWHERE (\"age\" < :0)"));

            await db.CountAsync(db.From<Person>().Where(x => x.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"person\"\nWHERE (\"age\" < :0)"));


            await db.SelectAsync<Person>("age > 40");
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE age > 40"));

            await db.SelectAsync<Person>("SELECT * FROM person WHERE age > 40");
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM person WHERE age > 40"));

            await db.SelectAsync<Person>("age > :Age", new { age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE age > :Age"));

            await db.SelectAsync<Person>("SELECT * FROM person WHERE age > :Age", new { age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM person WHERE age > :Age"));

            await db.SelectAsync<Person>("SELECT * FROM person WHERE age > :Age", new[] { db.CreateParam("age", 40) });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM person WHERE age > :Age"));

            await db.SelectAsync<Person>("age > :Age", new Dictionary<string, object> { { "age", 40 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE age > :Age"));

            await db.SelectAsync<Person>("SELECT * FROM person WHERE age > :Age", new Dictionary<string, object> { { "age", 40 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM person WHERE age > :Age"));

            await db.SelectFmtAsync<Person>("age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE age > 40"));

            await db.SelectFmtAsync<Person>("SELECT * FROM person WHERE age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM person WHERE age > 40"));

            await db.SelectAsync<EntityWithId>(typeof(Person));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\" FROM \"person\""));

            await db.SelectFmtAsync<EntityWithId>(typeof(Person), "age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\" FROM \"person\" WHERE age > 40"));

            await db.WhereAsync<Person>("Age", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE \"age\" = :Age"));

            await db.WhereAsync<Person>(new { Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE \"age\" = :Age"));

            await db.SelectByIdsAsync<Person>(new[] { 1, 2, 3 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE \"id\" IN (1,2,3)"));

            await db.SelectNonDefaultsAsync(new Person { Id = 1 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE \"id\" = :Id"));

            await db.SelectNonDefaultsAsync("age > :Age", new Person { Age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE age > :Age"));

            //await db.SelectLazy<Person>().ToList();
            //Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\""));

            //db.SelectLazy<Person>("age > :Age", new { age = 40 }).ToList();
            //Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE age > :Age"));

            //db.SelectLazyFmt<Person>("age > {0}", 40).ToList();
            //Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE age > 40"));

            //db.WhereLazy<Person>(new { Age = 27 }).ToList();
            //Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE \"age\" = :Age"));

            await db.SingleByIdAsync<Person>(1);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE \"id\" = :Id"));

            await db.SingleAsync<Person>(new { Age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE \"age\" = :Age"));

            await db.SingleAsync<Person>("age = :Age", new { age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE age = :Age"));

            await db.SingleAsync<Person>("age = :Age", new[] { db.CreateParam("age", 42) });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE age = :Age"));

            await db.SingleFmtAsync<Person>("age = {0}", 42); 
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE age = 42"));

            await db.SingleByIdAsync<Person>(1);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE \"id\" = :Id"));

            await db.SingleWhereAsync<Person>("Age", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE \"age\" = :Age"));

            await db.ScalarAsync<int>(db.From<Person>().Select(Sql.Count("*")).Where(q => q.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"person\"\nWHERE (\"age\" > :0)"));
            await db.ScalarAsync<int>(db.From<Person>().Select(x => Sql.Count("*")).Where(q => q.Age > 40));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT Count(*) \nFROM \"person\"\nWHERE (\"age\" > :0)"));

            await db.ScalarAsync<int>("SELECT COUNT(*) FROM person WHERE age > :Age", new { age = 40 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM person WHERE age > :Age"));

            await db.ScalarAsync<int>("SELECT COUNT(*) FROM person WHERE age > :Age", new[] { db.CreateParam("age", 40) });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM person WHERE age > :Age"));

            await db.ScalarFmtAsync<int>("SELECT COUNT(*) FROM person WHERE age > {0}", 40);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM person WHERE age > 40"));

            await db.ColumnAsync<string>(db.From<Person>().Select(x => x.LastName).Where(q => q.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"last_name\" \nFROM \"person\"\nWHERE (\"age\" = :0)"));

            await db.ColumnAsync<string>("SELECT last_name FROM person WHERE age = :Age", new { age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT last_name FROM person WHERE age = :Age"));

            await db.ColumnAsync<string>("SELECT last_name FROM person WHERE age = :Age", new[] { db.CreateParam("age", 27) });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT last_name FROM person WHERE age = :Age"));

            await db.ColumnFmtAsync<string>("SELECT last_name FROM person WHERE age = {0}", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT last_name FROM person WHERE age = 27"));

            await db.ColumnDistinctAsync<int>(db.From<Person>().Select(x => x.Age).Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"age\" \nFROM \"person\"\nWHERE (\"age\" < :0)"));

            await db.ColumnDistinctAsync<int>("SELECT age FROM person WHERE age < :Age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT age FROM person WHERE age < :Age"));

            await db.ColumnDistinctAsync<int>("SELECT age FROM person WHERE age < :Age", new[] { db.CreateParam("age", 50) });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT age FROM person WHERE age < :Age"));

            await db.ColumnDistinctFmtAsync<int>("SELECT age FROM person WHERE age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT age FROM person WHERE age < 50"));

            await db.LookupAsync<int, string>(db.From<Person>().Select(x => new { x.Age, x.LastName }).Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"age\",\"last_name\" \nFROM \"person\"\nWHERE (\"age\" < :0)"));

            await db.LookupAsync<int, string>("SELECT age, last_name FROM person WHERE age < :Age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT age, last_name FROM person WHERE age < :Age"));

            await db.LookupAsync<int, string>("SELECT age, last_name FROM person WHERE age < :Age", new[] { db.CreateParam("age", 50) });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT age, last_name FROM person WHERE age < :Age"));

            await db.LookupFmtAsync<int, string>("SELECT age, last_name FROM person WHERE age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT age, last_name FROM person WHERE age < 50"));

            await db.DictionaryAsync<int, string>(db.From<Person>().Select(x => new { x.Id, x.LastName }).Where(x => x.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\",\"last_name\" \nFROM \"person\"\nWHERE (\"age\" < :0)"));

            await db.DictionaryAsync<int, string>("SELECT id, last_name FROM person WHERE age < :Age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT id, last_name FROM person WHERE age < :Age"));

            await db.DictionaryAsync<int, string>("SELECT id, last_name FROM person WHERE age < :Age", new[] { db.CreateParam("age", 50) });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT id, last_name FROM person WHERE age < :Age"));

            await db.DictionaryFmtAsync<int, string>("SELECT id, last_name FROM person WHERE age < {0}", 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT id, last_name FROM person WHERE age < 50"));

            await db.ExistsAsync<Person>(x => x.Age < 50);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT 'exists' \nFROM \"person\"\nWHERE (\"age\" < :0)\nLIMIT 1"));

            await db.ExistsAsync(db.From<Person>().Where(x => x.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT 'exists' \nFROM \"person\"\nWHERE (\"age\" < :0)\nLIMIT 1"));

            await db.ExistsAsync<Person>(new { Age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE \"age\" = :Age"));

            await db.ExistsAsync<Person>("age = :Age", new { age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE age = :Age"));
            await db.ExistsAsync<Person>("SELECT * FROM person WHERE age = :Age", new { age = 42 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM person WHERE age = :Age"));

            await db.ExistsFmtAsync<Person>("age = {0}", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"id\", \"first_name\", \"last_name\", \"age\" FROM \"person\" WHERE age = 42"));
            await db.ExistsFmtAsync<Person>("SELECT * FROM person WHERE age = {0}", 42);
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM person WHERE age = 42"));

            await db.SqlListAsync<Person>(db.From<Person>().Select("*").Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * \nFROM \"person\"\nWHERE (\"age\" < :0)"));

            await db.SqlListAsync<Person>("SELECT * FROM person WHERE age < :Age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM person WHERE age < :Age"));

            await db.SqlListAsync<Person>("SELECT * FROM person WHERE age < :Age", new[] { db.CreateParam("age", 50) });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM person WHERE age < :Age"));

            await db.SqlListAsync<Person>("SELECT * FROM person WHERE age < :Age", new Dictionary<string, object> { { "age", 50 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT * FROM person WHERE age < :Age"));

            await db.SqlColumnAsync<string>(db.From<Person>().Select(x => x.LastName).Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT \"last_name\" \nFROM \"person\"\nWHERE (\"age\" < :0)"));

            await db.SqlColumnAsync<string>("SELECT last_name FROM person WHERE age < :Age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT last_name FROM person WHERE age < :Age"));

            await db.SqlColumnAsync<string>("SELECT last_name FROM person WHERE age < :Age", new[] { db.CreateParam("age", 50) });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT last_name FROM person WHERE age < :Age"));

            await db.SqlColumnAsync<string>("SELECT last_name FROM person WHERE age < :Age", new Dictionary<string, object> { { "age", 50 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT last_name FROM person WHERE age < :Age"));

            await db.SqlScalarAsync<int>(db.From<Person>().Select(Sql.Count("*")).Where(q => q.Age < 50));
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) \nFROM \"person\"\nWHERE (\"age\" < :0)"));

            await db.SqlScalarAsync<int>("SELECT COUNT(*) FROM person WHERE age < :Age", new { age = 50 });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM person WHERE age < :Age"));

            await db.SqlScalarAsync<int>("SELECT COUNT(*) FROM person WHERE age < :Age", new[] { db.CreateParam("age", 50) });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM person WHERE age < :Age"));

            await db.SqlScalarAsync<int>("SELECT COUNT(*) FROM person WHERE age < :Age", new Dictionary<string, object> { { "age", 50 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("SELECT COUNT(*) FROM person WHERE age < :Age"));

            var rowsAffected = await db.ExecuteNonQueryAsync("UPDATE Person SET last_name={0} WHERE id={1}".SqlFmt("WaterHouse", 7));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE Person SET last_name='WaterHouse' WHERE id=7"));

            rowsAffected = await db.ExecuteNonQueryAsync("UPDATE Person SET last_name=@name WHERE id=:Id", new { name = "WaterHouse", id = 7 });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE Person SET last_name=@name WHERE id=:Id"));


            await db.InsertAsync(new Person { Id = 7, FirstName = "Amy", LastName = "Winehouse", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"person\" (\"id\",\"first_name\",\"last_name\",\"age\") VALUES (:Id,:FirstName,:LastName,:Age)"));

            await db.InsertAsync(new Person { Id = 8, FirstName = "Tupac", LastName = "Shakur", Age = 25 },
                      new Person { Id = 9, FirstName = "Tupac", LastName = "Shakur2", Age = 26 });

            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"person\" (\"id\",\"first_name\",\"last_name\",\"age\") VALUES (:Id,:FirstName,:LastName,:Age)"));


            await db.InsertAllAsync(new[] { new Person { Id = 10, FirstName = "Biggie", LastName = "Smalls", Age = 24 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"person\" (\"id\",\"first_name\",\"last_name\",\"age\") VALUES (:Id,:FirstName,:LastName,:Age)"));

            await db.InsertOnlyAsync(new PersonWithAutoId { FirstName = "Amy", Age = 27 }, q => q.Insert(p => new { p.FirstName, p.Age }));
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"person_with_auto_id\" (\"first_name\",\"age\") VALUES ('Amy',27)"));

            await db.InsertOnlyAsync(new PersonWithAutoId { FirstName = "Amy", Age = 27 }, q => db.From<PersonWithAutoId>().Insert(p => new { p.FirstName, p.Age }));
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"person_with_auto_id\" (\"first_name\",\"age\") VALUES ('Amy',27)"));

            await db.UpdateAsync(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET \"first_name\"=:FirstName, \"last_name\"=:LastName, \"age\"=:Age WHERE \"id\"=:Id"));

            await db.UpdateAsync(new Person { Id = 8, FirstName = "Tupac", LastName = "Shakur3", Age = 27 },
                      new Person { Id = 9, FirstName = "Tupac", LastName = "Shakur4", Age = 28 });

            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET \"first_name\"=:FirstName, \"last_name\"=:LastName, \"age\"=:Age WHERE \"id\"=:Id"));

            await db.UpdateAsync(new[] { new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET \"first_name\"=:FirstName, \"last_name\"=:LastName, \"age\"=:Age WHERE \"id\"=:Id"));

            await db.UpdateAllAsync(new[] { new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 } });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET \"first_name\"=:FirstName, \"last_name\"=:LastName, \"age\"=:Age WHERE \"id\"=:Id"));

            await db.UpdateAsync(new Person { Id = 1, FirstName = "JJ", Age = 27 }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET \"id\"=:1, \"first_name\"=:2, \"last_name\"=:3, \"age\"=:4 WHERE (\"last_name\" = :0)"));

            await db.UpdateAsync<Person>(new { FirstName = "JJ" }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET \"first_name\"=:1 WHERE (\"last_name\" = :0)"));

            await db.UpdateNonDefaultsAsync(new Person { FirstName = "JJ" }, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET \"first_name\"=:1 WHERE (\"last_name\" = :0)"));

            await db.UpdateOnlyAsync(new Person { FirstName = "JJ" }, p => p.FirstName);
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET \"first_name\"=:0"));

            await db.UpdateOnlyAsync(new Person { FirstName = "JJ" }, p => p.FirstName, p => p.LastName == "Hendrix");
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET \"first_name\"=:1 WHERE (\"last_name\" = :0)"));

            await db.UpdateOnlyAsync(new Person { FirstName = "JJ", LastName = "Hendo" }, q => q.Update(p => p.FirstName));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET \"first_name\"=:0"));

            await db.UpdateOnlyAsync(new Person { FirstName = "JJ" }, q => q.Update(p => p.FirstName).Where(x => x.FirstName == "Jimi"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET \"first_name\"=:1 WHERE (\"first_name\" = :0)"));

            await db.UpdateFmtAsync<Person>(set: "first_name = {0}".SqlFmt("JJ"), where: "last_name = {0}".SqlFmt("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET first_name = 'JJ' WHERE last_name = 'Hendrix'"));

            await db.UpdateFmtAsync(table: "person", set: "first_name = {0}".SqlFmt("JJ"), where: "last_name = {0}".SqlFmt("Hendrix"));
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET first_name = 'JJ' WHERE last_name = 'Hendrix'"));

            await db.DeleteAsync<Person>(new { FirstName = "Jimi", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"person\" WHERE \"first_name\"=:FirstName AND \"age\"=:Age"));

            await db.DeleteAsync(new Person { Id = 1, FirstName = "Jimi", LastName = "Hendrix", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"person\" WHERE \"id\"=:Id AND \"first_name\"=:FirstName AND \"last_name\"=:LastName AND \"age\"=:Age"));

            await db.DeleteNonDefaultsAsync(new Person { FirstName = "Jimi", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"person\" WHERE \"first_name\"=:FirstName AND \"age\"=:Age"));

            await db.DeleteNonDefaultsAsync(new Person { FirstName = "Jimi", Age = 27 },
                                 new Person { FirstName = "Janis", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"person\" WHERE \"first_name\"=:FirstName AND \"age\"=:Age"));

            await db.DeleteByIdAsync<Person>(1);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"person\" WHERE \"id\" = :0"));

            await db.DeleteByIdsAsync<Person>(new[] { 1, 2, 3 });
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"person\" WHERE \"id\" IN (1,2,3)"));

            await db.DeleteFmtAsync<Person>("age = {0}", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"person\" WHERE age = 27"));

            await db.DeleteFmtAsync(typeof(Person), "age = {0}", 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"person\" WHERE age = 27"));

            await db.DeleteAsync<Person>(p => p.Age == 27);
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"person\" WHERE (\"age\" = :0)"));

            await db.DeleteAsync<Person>(q => q.Where(p => p.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"person\" WHERE (\"age\" = :0)"));

            await db.DeleteAsync(db.From<Person>().Where(p => p.Age == 27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"person\" WHERE (\"age\" = :0)"));

            await db.DeleteFmtAsync<Person>(where: "age = {0}".SqlFmt(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"person\" WHERE age = 27"));

            await db.DeleteFmtAsync(table: "Person", where: "age = {0}".SqlFmt(27));
            Assert.That(db.GetLastSql(), Is.EqualTo("DELETE FROM \"person\" WHERE age = 27"));

            await db.SaveAsync(new Person { Id = 11, FirstName = "Amy", LastName = "Winehouse", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("INSERT INTO \"person\" (\"id\",\"first_name\",\"last_name\",\"age\") VALUES (:Id,:FirstName,:LastName,:Age)"));
            await db.SaveAsync(new Person { Id = 11, FirstName = "Amy", LastName = "Winehouse", Age = 27 });
            Assert.That(db.GetLastSql(), Is.EqualTo("UPDATE \"person\" SET \"first_name\"=:FirstName, \"last_name\"=:LastName, \"age\"=:Age WHERE \"id\"=:Id"));

            await db.SaveAsync(new Person { Id = 12, FirstName = "Amy", LastName = "Winehouse", Age = 27 },
                    new Person { Id = 13, FirstName = "Amy", LastName = "Winehouse", Age = 27 });

            await db.SaveAllAsync(new[]{ new Person { Id = 14, FirstName = "Amy", LastName = "Winehouse", Age = 27 },
                              new Person { Id = 15, FirstName = "Amy", LastName = "Winehouse", Age = 27 } });

            OrmLiteConfig.UseParameterizeSqlExpressions = hold;
        }

    }
}