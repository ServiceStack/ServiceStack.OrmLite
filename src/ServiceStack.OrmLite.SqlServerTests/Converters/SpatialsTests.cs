using System.Linq;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.SqlServerTests.Converters
{
    [TestFixture]
    public class SpatialTests : SqlServerConvertersOrmLiteTestBase
    {
        [Test]
        public void Can_insert_and_retrieve_SqlGeography()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<GeoTest>();

                // Statue of Liberty
                var geo = SqlGeography.Point(40.6898329, -74.0452177, 4326);

                db.Insert(new GeoTest {Id = 1, Location = geo, NullLocation = SqlGeography.Null});

                var result = db.SingleById<GeoTest>(1);

                Assert.AreEqual(geo.Lat, result.Location.Lat);
                Assert.AreEqual(geo.Long, result.Location.Long);
                Assert.AreEqual(geo.STSrid, result.Location.STSrid);

                // Converter always resolves to null even when Null property inserted into database
                Assert.AreEqual(null, result.NullLocation);

                result.PrintDump();
            }
        }

        [Test]
        public void Can_insert_and_retrieve_SqlGeometry()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<GeoTest>();

                // A simple line from (0,0) to (4,4)  Length = SQRT(2 * 4^2)
                var wkt = new System.Data.SqlTypes.SqlChars("LINESTRING(0 0, 4 4)".ToCharArray());
                var shape = SqlGeometry.STLineFromText(wkt, 0);

                db.Insert(new GeoTest { Id = 1, Shape = shape});

                var result = db.SingleById<GeoTest>(1).Shape;

                var lengths = db.Column<double>("select Shape.STLength() AS Length from GeoTest");

                Assert.AreEqual((double) result.STLength(), lengths.First());

                Assert.AreEqual(shape.STStartPoint().STX, result.STStartPoint().STX);
                Assert.AreEqual(shape.STStartPoint().STY, result.STStartPoint().STY);

                Assert.AreEqual(shape.STEndPoint().STX, result.STEndPoint().STX);
                Assert.AreEqual(shape.STEndPoint().STY, result.STEndPoint().STY);

                Assert.AreEqual(2, (int) result.STNumPoints());

                result.PrintDump();
            }
        }

        [Test]
        public void Can_insert_SqlGeography_and_SqlGeometry()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<GeoTest>();

                // Statue of Liberty
                var geo = SqlGeography.Point(40.6898329, -74.0452177, 4326);

                // A simple line from (0,0) to (4,4)  Length = SQRT(2 * 4^2)
                var wkt = new System.Data.SqlTypes.SqlChars("LINESTRING(0 0, 4 4)".ToCharArray());
                var shape = SqlGeometry.STLineFromText(wkt, 0);

                db.Insert(new GeoTest { Id = 1, Location = geo, Shape = shape });

                var result = db.SingleById<GeoTest>(1);

                Assert.That(result, Is.Not.Null);

                result.PrintDump();
            }
        }
    }

    public class GeoTest
    {
        public long Id { get; set; }

        public SqlGeography Location { get; set; }

        public SqlGeography NullLocation { get; set; }

        public SqlGeometry Shape { get; set; }
    }
}
