﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.UseCase;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class SqlExpressionTests
        : OrmLiteTestBase
    {
        public static void InitLetters(IDbConnection db)
        {
            db.DropAndCreateTable<LetterFrequency>();

            db.Insert(new LetterFrequency { Letter = "A" });
            db.Insert(new LetterFrequency { Letter = "B" });
            db.Insert(new LetterFrequency { Letter = "B" });
            db.Insert(new LetterFrequency { Letter = "C" });
            db.Insert(new LetterFrequency { Letter = "C" });
            db.Insert(new LetterFrequency { Letter = "C" });
            db.Insert(new LetterFrequency { Letter = "D" });
            db.Insert(new LetterFrequency { Letter = "D" });
            db.Insert(new LetterFrequency { Letter = "D" });
            db.Insert(new LetterFrequency { Letter = "D" });
        }

        [Test]
        public async Task Can_Select_as_List_Object_Async()
        {
            using (var db = OpenDbConnection())
            {
                InitLetters(db);

                var query = db.From<LetterFrequency>()
                  .Select("COUNT(*), MAX(Id), MIN(Id), Sum(Id)");

                query.ToSelectStatement().Print();

                var results = await db.SelectAsync<List<object>>(query);

                Assert.That(results.Count, Is.EqualTo(1));

                var result = results[0];
                Assert.That(result[0], Is.EqualTo(10));
                Assert.That(result[1], Is.EqualTo(10));
                Assert.That(result[2], Is.EqualTo(1));
                Assert.That(result[3], Is.EqualTo(55));

                results.PrintDump();
            }
        }

        [Test]
        public async Task Can_Select_as_Dictionary_Object_Async()
        {
            using (var db = OpenDbConnection())
            {
                InitLetters(db);

                var query = db.From<LetterFrequency>()
                  .Select("COUNT(*) count, MAX(Id) max, MIN(Id) min, Sum(Id) sum");

                query.ToSelectStatement().Print();

                var results = await db.SelectAsync<Dictionary<string, object>>(query);

                Assert.That(results.Count, Is.EqualTo(1));
                //results.PrintDump();

                var result = results[0];
                Assert.That(result["count"], Is.EqualTo(10));
                Assert.That(result["max"], Is.EqualTo(10));
                Assert.That(result["min"], Is.EqualTo(1));
                Assert.That(result["sum"], Is.EqualTo(55));
            }
        }
        [Test]
        public async Task Can_select_limit_on_Table_with_References_Async()
        {
            using (var db = OpenDbConnection())
            {
                CustomerOrdersUseCase.DropTables(db); //Has conflicting 'Order' table
                db.DropAndCreateTable<Order>();
                db.DropAndCreateTable<Customer>();
                db.DropAndCreateTable<CustomerAddress>();

                var customer1 = LoadReferencesTests.GetCustomerWithOrders("1");
                db.Save(customer1, references: true);

                var customer2 = LoadReferencesTests.GetCustomerWithOrders("2");
                db.Save(customer2, references: true);

                var results = await db.LoadSelectAsync<Customer>(q => q
                    .OrderBy(x => x.Id)
                    .Limit(1, 1));

                //db.GetLastSql().Print();

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Name, Is.EqualTo("Customer 2"));
                Assert.That(results[0].PrimaryAddress.AddressLine1, Is.EqualTo("2 Humpty Street"));
                Assert.That(results[0].Orders.Count, Is.EqualTo(2));

                results = await db.LoadSelectAsync<Customer>(q => q
                    .Join<CustomerAddress>()
                    .OrderBy(x => x.Id)
                    .Limit(1, 1));

                db.GetLastSql().Print();

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Name, Is.EqualTo("Customer 2"));
                Assert.That(results[0].PrimaryAddress.AddressLine1, Is.EqualTo("2 Humpty Street"));
                Assert.That(results[0].Orders.Count, Is.EqualTo(2));
            }
        }
    }
}