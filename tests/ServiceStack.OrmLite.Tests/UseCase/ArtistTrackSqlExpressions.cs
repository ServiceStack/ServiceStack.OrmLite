using System.Linq;
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
            var hold = OrmLiteConfig.StripUpperInLike;
            OrmLiteConfig.StripUpperInLike = false;
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
            OrmLiteConfig.StripUpperInLike = hold;
        }

        [Test]
        public void Can_Order_by_Property_Alias()
        {
            var hold = OrmLiteConfig.StripUpperInLike;
            OrmLiteConfig.StripUpperInLike = false;
            using (var db = CreateArtistAndTrackTablesWithData(OpenDbConnection()))
            {
                var q = db.From<Track>()
                    .Where(x => x.Year > 1991)
                    .And(x => x.Name.Contains("A"))
                    .GroupBy(x => x.Year)
                    .OrderByDescending("Count")
                    .ThenBy(x => x.Year)
                    .Take(1)
                    .Select(x => new { x.Year, Count = Sql.Count("*") });

                var result = db.Dictionary<int, int>(q);
                Assert.That(result[1993], Is.EqualTo(2));
            }
            OrmLiteConfig.StripUpperInLike = hold;
        }

        [Test]
        public void Can_Select_joined_table_with_Alias()
        {
            using (var db = CreateArtistAndTrackTablesWithData(OpenDbConnection()))
            {
                var tracksByYear = db.Dictionary<string, int>(db.From<Track>()
                    .Join<Artist>()
                    .GroupBy<Artist>(x => x.Name)
                    .Select<Artist>(x => new { x.Name, Count = Sql.Count("*") }));

                Assert.That(tracksByYear.Count, Is.EqualTo(4));
                Assert.That(tracksByYear.Map(x => x.Value).Sum(), Is.EqualTo(8));
            }
        }

        [Test]
        public void Can_Count_Distinct()
        {
            using (var db = CreateArtistAndTrackTablesWithData(OpenDbConnection()))
            {
                var differentArtistsCount = db.Scalar<int>(db.From<Track>()
                    .Select(x => Sql.CountDistinct(x.ArtistId)));

                Assert.That(differentArtistsCount, Is.EqualTo(4));
            }
        }
    }
}