using NUnit.Framework;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class NullReferenceIssues : OrmLiteTestBase
    {
        public class Foo
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Key { get; set; }
        }

        [Test]
        public void Can_AlterColumn()
        {
            if (Dialect == Dialect.Sqlite)
                return; // Not supported

            LogManager.LogFactory = new ConsoleLogFactory(debugEnabled:true);

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Foo>();

                db.AlterColumn(typeof(Foo), new FieldDefinition
                {
                    Name = "Name",
                    FieldType = typeof(string),
                    IsNullable = true,
                    DefaultValue = null
                });
            }
        }

    }
}