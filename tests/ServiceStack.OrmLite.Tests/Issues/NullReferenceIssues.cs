using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class NullReferenceIssues : OrmLiteTestBase
    {
        //public NullReferenceIssues() : base(Dialect.MySql) {}

        public class Foo
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Key { get; set; }

            public int Int { get; set; }
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
                    Name = nameof(Foo.Name),
                    FieldType = typeof(string),
                    IsNullable = true,
                    DefaultValue = null
                });

                db.AlterColumn(typeof(Foo), new FieldDefinition
                {
                    Name = nameof(Foo.Int),
                    FieldType = typeof(int),
                    IsNullable = true,
                    DefaultValue = null
                });

                db.AddColumn(typeof(Foo), new FieldDefinition
                {
                    Name = "Bool",
                    FieldType = typeof(bool),
                });
            }
        }

    }
}