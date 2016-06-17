using System;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class AnyObjectClass
    {
        [Alias("_id")]
        public Guid Id { get; set; }

        [Alias("_identity")]
        public Guid? Identity { get; set; }

        [Alias("_name")]
        [StringLength(250)]
        public string Name { get; set; }
    }

    public class SqlExpressionNullConstantIssue : OrmLiteTestBase
    {
        [Test]
        public void Can_compare_null_constant_in_subquery()
        {
            LogManager.LogFactory = new ConsoleLogFactory();

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<AnyObjectClass>();

                var inQ = db.From<AnyObjectClass>()
                    .Where(y => y.Identity != null)
                    .Select(y => y.Identity.Value);

                var q = db.From<AnyObjectClass>().Where(x => Sql.In(x.Identity, inQ));

                var results = db.Select(q);

                results.PrintDump();
            }
        }

        [Test]
        public void Can_compare_null_constant_in_subquery_nested_in_SqlExpression()
        {
            LogManager.LogFactory = new ConsoleLogFactory();

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<AnyObjectClass>();

                var q = db.From<AnyObjectClass>().Where(x => Sql.In(x.Identity, 
                    db.From<AnyObjectClass>()
                        .Where(y => y.Identity != null)
                        .Select(y => y.Identity.Value)));

                var results = db.Select(q);

                results.PrintDump();
            }
        }
    }
}