using System;
using NUnit.Framework;
using ServiceStack.OrmLite.Sqlite;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class Organization
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
    }

    public class OrganizationMembership
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public bool HasA { get; set; }
        public bool HasB { get; set; }
        public bool HasC { get; set; }
    }

    public class MergingNestedSqlExpressionIssue : OrmLiteTestBase
    {
        [Test]
        public void Does_merge_subselect_params_correctly()
        {
            OrmLiteConfig.DialectProvider = new SqliteOrmLiteDialectProvider();

            // select a group of ids
            var ids = OrmLiteConfig.DialectProvider.SqlExpression<OrganizationMembership>();
            ids.Where(x => x.HasA == true && x.HasB == true && x.HasC == true);
            ids.SelectDistinct(x => x.OrganizationId);

            // select organizations
            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Organization>();
            // that are active
            expression.Where(x => x.IsActive == true);
            // and belong to the same group
            expression.Where(x => Sql.In(x.Id, ids));

            Assert.That(expression.WhereExpression, Is.EqualTo(
                "WHERE (\"IsActive\" = @0) AND \"Id\" IN (SELECT DISTINCT \"OrganizationId\" \nFROM \"OrganizationMembership\"\nWHERE (((\"HasA\" = @1) AND (\"HasB\" = @2)) AND (\"HasC\" = @3)))"));
        }
    }
}