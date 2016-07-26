using NUnit.Framework;
using ServiceStack.OrmLite.Tests.Support;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.UseCase
{
    [TestFixture]
    public class ArtistTrackSqlExpressions : ArtistTrackTestBase
    {
        [Test]
        public void Can_OrderBy_Column_Index()
        {
            using (var db = CreateArtistAndTrackTablesWithData(OpenDbConnection()))
            {
                var q = db.From<Track>()
                    .Where(x => x.Year > 1991)
                    .And(x => x.Name.Contains("A"))
                    .GroupBy(x => x.Year)
                    .OrderByDescending(2)
                    .ThenBy(x => x.Year)
                    .Take(1)
                    .Select(x => new { x.Year, Count = Sql.Count("*") });

                var result = db.Dictionary<int, int>(q);
                Assert.That(result[1993], Is.EqualTo(2));
            }
        }
    }
}