using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceStack.OrmLite;

namespace ServiceStack.OrmLite.Tests
{
    public class OrmLiteUtilExtensionsTests : OrmLiteTestBase
    {
        [Test]
        public void CanCreateStringInStatement()
        {
            var list = new string[] { "A", "B", "C" };

            var sql = "IN ({0})".Params(list.SqlInValues());

            Assert.AreEqual("IN ('A','B','C')", sql);
        }

        [Test]
        public void CanCreateIntInStatement()
        {
            var list = new int[] { 1, 2, 3 };

            var sql = "IN ({0})".Params(list.SqlInValues());

            Assert.AreEqual("IN (1,2,3)", sql);
        }

        [Test]
        public void CanCreateNullInStatementFromEmptyList()
        {
            var list = new string[] {};

            var sql = "IN ({0})".Params(list.SqlInValues());

            Assert.AreEqual("IN (NULL)", sql);
        }
    }
}
