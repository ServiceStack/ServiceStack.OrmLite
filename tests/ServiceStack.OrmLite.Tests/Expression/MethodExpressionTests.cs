// Copyright (c) Service Stack LLC. All Rights Reserved.
// License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt


using System.Linq;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests.Expression
{
    public class MethodExpressionTests : ExpressionsTestBase
    {
        [Test]
        public void Can_select_ints_using_array_contains()
        {
            var ints = new[] { 1, 2, 3 };

            using (var db = OpenDbConnection())
            {
                db.Select<TestType>(x => ints.Contains(x.Id));

                Assert.That(db.GetLastSql(), Is.StringContaining("(1,2,3)"));
            }
        }

        [Test]
        public void Can_select_ints_using_list_contains()
        {
            var ints = new[] { 1, 2, 3 }.ToList();

            using (var db = OpenDbConnection())
            {
                db.Select<TestType>(x => ints.Contains(x.Id));

                Assert.That(db.GetLastSql(), Is.StringContaining("(1,2,3)"));
            }
        }

        [Test]
        public void Can_select_ints_using_empty_array_contains()
        {
            var ints = new int[] {};

            using (var db = OpenDbConnection())
            {
                db.Select<TestType>(x => ints.Contains(x.Id));
                
                Assert.That(db.GetLastSql(), Is.StringContaining("(NULL)"));
            }
        }

    }
}