﻿using System;
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
            if (Dialect == Dialect.PostgreSql || Dialect == Dialect.MySql)
                return; //Invalid Custom SQL for pgsql naming convention 

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Task>();

                var parentId = db.Insert(new Task { Created = new DateTime(2000, 01, 01) }, selectIdentity: true);
                var childId = db.Insert(new Task { ParentId = parentId, Created = new DateTime(2001, 01, 01) }, selectIdentity:true);

                var fromDateTime = new DateTime(2000, 02, 02);

                var q = db.From<Task>()
                    .CustomJoin("LEFT JOIN Task history ON (Task.Id = history.ParentId)")
                    .Where("history.\"Created\" >= {0} OR Task.\"Created\" >= {0}", fromDateTime);

                //doesn't work with Self Joins
                //var q = db.From<Task>()
                //    .LeftJoin<Task, Task>((parent, history) => (parent.Id == history.ParentId)
                //            && (history.CreatedAt >= fromDateTime || parent.CreatedAt >= fromDateTime)
                //        ,db.JoinAlias("history"));

                var results = db.Select(q);

                db.GetLastSql().Print();

                results.PrintDump();
            }
        }

        [Test]
        public void Can_use_Column_to_resolve_properties()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Task>();

                var parentId = db.Insert(new Task { Created = new DateTime(2000, 01, 01) }, selectIdentity: true);
                var childId = db.Insert(new Task { ParentId = parentId, Created = new DateTime(2001, 01, 01) }, selectIdentity: true);

                var q = db.From<Task>();

                var leftJoin = 
                    $"LEFT JOIN Task history ON ({q.Column<Task>(t => t.Id, prefixTable:true)} = history.{q.Column<Task>(t => t.ParentId)})";
                Assert.That(leftJoin, Is.EqualTo(
                    $"LEFT JOIN Task history ON ({q.Table<Task>()}.{q.Column<Task>(t => t.Id)} = history.{q.Column<Task>(t => t.ParentId)})"));
                Assert.That(leftJoin, Is.EqualTo(
                    $"LEFT JOIN Task history ON ({q.Table<Task>()}.{q.Column<Task>(nameof(Task.Id))} = history.{q.Column<Task>(nameof(Task.ParentId))})"));

                q.CustomJoin(leftJoin);

                var results = db.Select(q);

                db.GetLastSql().Print();

                Assert.That(results.Count, Is.EqualTo(2));
            }
        }
    }
}
