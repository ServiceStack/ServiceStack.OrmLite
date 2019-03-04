using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.OrmLite.Tests.Shared;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class MetaDataTests : OrmLiteTestBase
    {
        [Test]
        public void Can_get_TableNames()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Table1>();
                db.DropAndCreateTable<Table2>();

                3.Times(i => db.Insert(new Table1 {Id = i + 1, Field1 = $"Field{i+1}"}) );
                1.Times(i => db.Insert(new Table2 {Id = i + 1, Field2 = $"Field{i+1}"}) );
                
                var tableNames = db.GetTableNames();
                tableNames.TextDump().Print();
                Assert.That(tableNames.Count, Is.GreaterThan(0));
                Assert.That(tableNames.Any(x => x.EqualsIgnoreCase(nameof(Table1))));
                Assert.That(tableNames.Any(x => x.EqualsIgnoreCase(nameof(Table2))));
            }
        }

        [Test]
        public async Task Can_get_TableNames_Async()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Table1>();
                db.DropAndCreateTable<Table2>();

                3.Times(i => db.Insert(new Table1 {Id = i + 1, Field1 = $"Field{i+1}"}) );
                1.Times(i => db.Insert(new Table2 {Id = i + 1, Field2 = $"Field{i+1}"}) );
                
                var tableNames = await db.GetTableNamesAsync();
                tableNames.TextDump().Print();
                Assert.That(tableNames.Count, Is.GreaterThan(0));
                Assert.That(tableNames.Any(x => x.EqualsIgnoreCase(nameof(Table1))));
                Assert.That(tableNames.Any(x => x.EqualsIgnoreCase(nameof(Table2))));
            }
        }

        [Test]
        public void Can_get_TableNames_in_Schema()
        {
            var schema = "Schema";
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Schematable1>();
                db.DropAndCreateTable<Schematable2>();

                3.Times(i => db.Insert(new Schematable1 {Id = i + 1, Field1 = $"Field{i+1}"}) );
                1.Times(i => db.Insert(new Schematable2 {Id = i + 1, Field2 = $"Field{i+1}"}) );

                var tableNames = db.GetTableNames(schema);
                tableNames.TextDump().Print();
                Assert.That(tableNames.Count, Is.GreaterThan(0));
                Assert.That(tableNames.Any(x => x.IndexOf(nameof(Schematable1), StringComparison.OrdinalIgnoreCase) >= 0));
                Assert.That(tableNames.Any(x => x.IndexOf(nameof(Schematable2), StringComparison.OrdinalIgnoreCase) >= 0));
            }
        }

        int IndexOf(List<KeyValuePair<string, long>> tableResults, Func<KeyValuePair<string, long>, bool> fn)
        {
            for (int i = 0; i < tableResults.Count; i++)
            {
                if (fn(tableResults[i]))
                    return i;
            }
            return -1;
        }

        [Test]
        public void Can_get_GetTableNamesWithRowCounts()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Table1>();
                db.DropAndCreateTable<Table2>();

                3.Times(i => db.Insert(new Table1 {Id = i + 1, Field1 = $"Field{i+1}"}) );
                1.Times(i => db.Insert(new Table2 {Id = i + 1, Field2 = $"Field{i+1}"}) );
                
                var tableNames = db.GetTableNamesWithRowCounts();
                tableNames.TextDump().Print();
                Assert.That(tableNames.Count, Is.GreaterThan(0));

                var table1Pos = IndexOf(tableNames, x => x.Key.EqualsIgnoreCase(nameof(Table1)) && x.Value == 3);
                Assert.That(table1Pos, Is.GreaterThanOrEqualTo(0));

                var table2Pos = IndexOf(tableNames, x => x.Key.EqualsIgnoreCase(nameof(Table2)) && x.Value == 1);
                Assert.That(table2Pos, Is.GreaterThanOrEqualTo(0));
                
                Assert.That(table1Pos < table2Pos); //is sorted desc
            }
        }

        [Test]
        public async Task Can_get_GetTableNamesWithRowCounts_Async()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Table1>();
                db.DropAndCreateTable<Table2>();

                3.Times(i => db.Insert(new Table1 {Id = i + 1, Field1 = $"Field{i+1}"}) );
                1.Times(i => db.Insert(new Table2 {Id = i + 1, Field2 = $"Field{i+1}"}) );
                
                var tableNames = await db.GetTableNamesWithRowCountsAsync();
                tableNames.TextDump().Print();
                Assert.That(tableNames.Count, Is.GreaterThan(0));

                var table1Pos = IndexOf(tableNames, x => x.Key.EqualsIgnoreCase(nameof(Table1)) && x.Value == 3);
                Assert.That(table1Pos, Is.GreaterThanOrEqualTo(0));

                var table2Pos = IndexOf(tableNames, x => x.Key.EqualsIgnoreCase(nameof(Table2)) && x.Value == 1);
                Assert.That(table2Pos, Is.GreaterThanOrEqualTo(0));
                
                Assert.That(table1Pos < table2Pos); //is sorted desc
            }
        }

        [Test]
        public void Can_get_GetTableNamesWithRowCounts_in_Schema()
        {
            var schema = "Schema";
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Schematable1>();
                db.DropAndCreateTable<Schematable2>();

                3.Times(i => db.Insert(new Schematable1 {Id = i + 1, Field1 = $"Field{i+1}"}) );
                1.Times(i => db.Insert(new Schematable2 {Id = i + 1, Field2 = $"Field{i+1}"}) );
                
                var tableNames = db.GetTableNamesWithRowCounts(schema);
                tableNames.TextDump().Print();
                Assert.That(tableNames.Count, Is.GreaterThan(0));

                var table1Pos = IndexOf(tableNames, x => x.Key.IndexOf(nameof(Schematable1), StringComparison.OrdinalIgnoreCase) >=0 && x.Value == 3);
                Assert.That(table1Pos, Is.GreaterThanOrEqualTo(0));

                var table2Pos = IndexOf(tableNames, x => x.Key.IndexOf(nameof(Schematable2), StringComparison.OrdinalIgnoreCase) >=0 && x.Value == 1);
                Assert.That(table2Pos, Is.GreaterThanOrEqualTo(0));
                
                Assert.That(table1Pos < table2Pos); //is sorted desc
            }
        }
    }
}