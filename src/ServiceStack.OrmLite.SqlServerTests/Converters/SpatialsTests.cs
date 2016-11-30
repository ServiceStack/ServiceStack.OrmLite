using System;
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
            Db.DropAndCreateTable<GeoTest>();

            // Statue of Liberty
            var geo = SqlGeography.Point(40.6898329, -74.0452177, 4326);

            Db.Insert(new GeoTest {Id = 1, Location = geo, NullLocation = SqlGeography.Null});

            var result = Db.SingleById<GeoTest>(1);

            Assert.AreEqual(geo.Lat, result.Location.Lat);
            Assert.AreEqual(geo.Long, result.Location.Long);
            Assert.AreEqual(geo.STSrid, result.Location.STSrid);

            // Converter always resolves to null even when Null property inserted into database
            Assert.AreEqual(null, result.NullLocation);

            result.PrintDump();
        }

        [Test]
        public void Can_insert_and_retrieve_SqlGeometry()
        {
            Db.DropAndCreateTable<GeoTest>();

            // A simple line from (0,0) to (4,4)  Length = SQRT(2 * 4^2)
            var wkt = new System.Data.SqlTypes.SqlChars("LINESTRING(0 0, 4 4)".ToCharArray());
            var shape = SqlGeometry.STLineFromText(wkt, 0);

            Db.Insert(new GeoTest { Id = 1, Shape = shape});

            var result = Db.SingleById<GeoTest>(1).Shape;

            var lengths = Db.Column<double>("select Shape.STLength() AS Length from GeoTest");

            Assert.AreEqual((double) result.STLength(), lengths.First());

            Assert.AreEqual(shape.STStartPoint().STX, result.STStartPoint().STX);
            Assert.AreEqual(shape.STStartPoint().STY, result.STStartPoint().STY);

            Assert.AreEqual(shape.STEndPoint().STX, result.STEndPoint().STX);
            Assert.AreEqual(shape.STEndPoint().STY, result.STEndPoint().STY);

            Assert.AreEqual(2, (int) result.STNumPoints());

            result.PrintDump();
        }

        [Test]
        public void Can_insert_SqlGeography_and_SqlGeometry()
        {
            Db.DropAndCreateTable<GeoTest>();

            // Statue of Liberty
            var geo = SqlGeography.Point(40.6898329, -74.0452177, 4326);

            // A simple line from (0,0) to (4,4)  Length = SQRT(2 * 4^2)
            var wkt = new System.Data.SqlTypes.SqlChars("LINESTRING(0 0, 4 4)".ToCharArray());
            var shape = SqlGeometry.STLineFromText(wkt, 0);

            Db.Insert(new GeoTest { Id = 1, Location = geo, Shape = shape });

            shape = Db.SingleById<GeoTest>(1).Shape;

            Assert.That(shape, Is.Not.Null);

            new { shape.STEndPoint().STX, shape.STEndPoint().STY }.PrintDump();
        }

        [Test]
        public void Can_insert_and_update_SqlGeography()
        {
            Db.DropAndCreateTable<ModelWithSqlGeography>();

            var wkt = "POINT(38.028495788574205 55.895460650576936)";
            var geo = SqlGeography.STGeomFromText(new System.Data.SqlTypes.SqlChars(wkt), 4326);

            var obj = new ModelWithSqlGeography { Name = "Test", Created = DateTime.UtcNow, Geo = geo };

            var id = (int)Db.Insert(obj, selectIdentity: true);
            obj.ID = id;

            try
            {
                // Update of POCO with SqlGeography proprety should work
                obj.Name = "Test - modified";
                obj.Edited = DateTime.UtcNow;
                Db.Update(obj);                    
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
            finally
            {
                // GetLastSql shouldn't return null after exception
                var lastSql = Db.GetLastSql();
                Assert.IsNotNull(lastSql);
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

    public class ModelWithSqlGeography
    {
        [AutoIncrement]
        public int ID { get; set; }
        [StringLength(255)]
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Edited { get; set; }
        public SqlGeography Geo { get; set; }
    }
}
