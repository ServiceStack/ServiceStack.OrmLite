using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests
{
    public class OrmLiteUtilExtensionsTests : OrmLiteTestBase
    {
        [Test]
        public void CanCreateStringInStatement()
        {
            var list = new string[] { "A", "B", "C" };

            var sql = "IN ({0})".SqlFmt(list.SqlInValues());

            Assert.AreEqual("IN ('A','B','C')", sql);
        }

        [Test]
        public void CanCreateIntInStatement()
        {
            var list = new int[] { 1, 2, 3 };

            var sql = "IN ({0})".SqlFmt(list.SqlInValues());

            Assert.AreEqual("IN (1,2,3)", sql);
        }

        [Test]
        public void CanCreateNullInStatementFromEmptyList()
        {
            var list = new string[] {};

            var sql = "IN ({0})".SqlFmt(list.SqlInValues());

            Assert.AreEqual("IN (NULL)", sql);
        }

        [Test]
        public void Can_parse_field_Tokens()
        {
            Assert.That("FirstName".ParseTokens(), Is.EquivalentTo(new[] { "FirstName" }));
            Assert.That("FirstName, LastName".ParseTokens(), Is.EquivalentTo(new[] { "FirstName", "LastName" }));
            Assert.That("\"FirstName\"".ParseTokens(), Is.EquivalentTo(new[] { "\"FirstName\"" }));
            Assert.That("\"FirstName\",\"LastName\"".ParseTokens(), Is.EquivalentTo(new[] { "\"FirstName\"", "\"LastName\"" }));
            Assert.That("COALESCE(\"Time\", '2015-10-05')".ParseTokens(), Is.EquivalentTo(new[] { "COALESCE(\"Time\", '2015-10-05')" }));
            Assert.That("\"FirstName\",COALESCE(\"Time\", '2015-10-05'),\"LastName\"".ParseTokens(), Is.EquivalentTo(
                new[] { "\"FirstName\"", "COALESCE(\"Time\", '2015-10-05')", "\"LastName\"" }));
            Assert.That(" \"FirstName\" , COALESCE(\"Time\", '2015-10-05') , \"LastName\" ".ParseTokens(), Is.EquivalentTo(
                new[] { "\"FirstName\"", "COALESCE(\"Time\", '2015-10-05')", "\"LastName\"" }));
        }
    }
}
