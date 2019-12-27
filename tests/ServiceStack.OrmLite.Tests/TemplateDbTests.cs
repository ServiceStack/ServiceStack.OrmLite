using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Templates;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixtureOrmLite]
    public class TemplateDbTests : OrmLiteProvidersTestBase
    {
        public TemplateDbTests(DialectContext context) : base(context) {}

        [Test]
        public async Task Can_retrieve_single_record_with_param()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Rockstar>();
                db.InsertAll(AutoQueryTests.SeedRockstars);

                var args = new Dictionary<string, object> {{"id", 3}};
                var tableName = "Rockstar".SqlTable(DialectProvider);
                var result = db.Single<Rockstar>($"SELECT * FROM {tableName} WHERE Id = @id", args);
                Assert.That(result.FirstName, Is.EqualTo("Kurt"));

                result = await db.SingleAsync<Rockstar>($"SELECT * FROM {tableName} WHERE Id = @id", args);
                Assert.That(result.FirstName, Is.EqualTo("Kurt"));

                result = await db.SingleAsync<Rockstar>($"SELECT * FROM {tableName} WHERE Id = @id", new { id = 3 });
                Assert.That(result.FirstName, Is.EqualTo("Kurt"));
            }
        }

        [Test]
        public async Task Can_call_dbSingle_with_param()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Rockstar>();
                db.InsertAll(AutoQueryTests.SeedRockstars);

                var firstName = "FirstName".SqlColumn(DialectProvider).StripDbQuotes();

                var args = new Dictionary<string, object> { { "id", 3 }};

                var filter = new TemplateDbFilters { DbFactory = base.DbFactory };
                
                var result = filter.dbSingle(default(TemplateScopeContext), $"SELECT * FROM Rockstar WHERE Id = @id", args);
                
                var objDictionary = (Dictionary<string, object>)result;
                Assert.That(objDictionary[firstName], Is.EqualTo("Kurt"));

                var asyncFilter = new TemplateDbFiltersAsync { DbFactory = base.DbFactory };
                result = await asyncFilter.dbSingle(default(TemplateScopeContext), $"SELECT * FROM Rockstar WHERE Id = @id", args);

                objDictionary = (Dictionary<string, object>)result;
                Assert.That(objDictionary[firstName], Is.EqualTo("Kurt"));
            }
        }

    }
}