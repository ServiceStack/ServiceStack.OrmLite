using System;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Expression
{
    public class Task
    {
        [AutoIncrement]
        public long Id { get; set; }

        public long? ParentId { get; set; }

        public DateTime Created { get; set; }
    }

    [TestFixture]
    public class JoinAliasTests : OrmLiteTestBase
    {
        [Test]
        public void Can_use_JoinAlias_in_condition()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Task>();

                var parentId = db.Insert(new Task { Created = new DateTime(2000, 01, 01) }, selectIdentity: true);
                var childId = db.Insert(new Task { ParentId = parentId, Created = new DateTime(2001, 01, 01) }, selectIdentity:true);

                var fromDateTime = new DateTime(2000, 02, 02);

                var q = db.From<Task>()
                    .LeftJoin<Task>((parent, history) => parent.Id == history.ParentId, db.JoinAlias("history"))
                    .Where("history.\"Created\" >= {0} OR Task.\"Created\" >= {0}", fromDateTime);

                //TODO: JOIN Alias doesn't support self-joins

                var results = db.Select(q);

                db.GetLastSql().Print();

                results.PrintDump();
            }
        }
    }
}
