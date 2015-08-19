using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Microsoft.SqlServer.Types;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.SqlServer;

namespace ServiceStack.OrmLite.SqlServerTests.Spatials
{
    [TestFixture]
    public class SqlServerSpatialTests : SqlServerSpatialsOrmLiteTestBase
    {
        [SetUp]
        public void Setup()
        {
            OpenDbConnection().CreateTable<GeoTestTable>(true);
        }

        // Avoid painful refactor to change all tests to use a using pattern
        private IDbConnection db;

        public override IDbConnection OpenDbConnection(string connString = null)
        {
            if (db != null && db.State != ConnectionState.Open)
                db = null;

            return db ?? (db = base.OpenDbConnection(connString));
        }

        [TearDown]
        public void TearDown()
        {
            if (db == null)
                return;
            db.Dispose();
            db = null;
        }


        [Test]
        public void Can_insert_and_retrieve_SqlGeography()
        {
            using (var db = OpenDbConnection())
            {
                // Statue of Liberty
                var geo = SqlGeography.Point(40.6898329,-74.0452177, 4326);

                db.Insert(new GeoTestTable() { Location = geo });

                var result = db.Select(db.From<GeoTestTable>()).First().Location;

                Assert.AreEqual(geo.Lat, result.Lat);
                Assert.AreEqual(geo.Long, result.Long);
                Assert.AreEqual(geo.STSrid, result.STSrid);
            }
        }

        [Test]
        public void Can_insert_and_retrieve_SqlGeometry()
        {
            using (var db = OpenDbConnection())
            {
                // A simple line from (0,0) to (4,4)  Length = SQRT(2 * 4^2)
                var wkt = new System.Data.SqlTypes.SqlChars("LINESTRING(0 0, 4 4)".ToCharArray());
                var shape = SqlGeometry.STLineFromText(wkt, 0);

                db.Insert(new GeoTestTable() { Shape = shape });

                var result = db.Select(db.From<GeoTestTable>()).First().Shape;

                var lengths = db.Column<double>("select Shape.STLength() AS Length from GeoTestTable");

                Assert.AreEqual((double)result.STLength(), lengths.First());

                Assert.AreEqual(shape.STStartPoint().STX, result.STStartPoint().STX);
                Assert.AreEqual(shape.STStartPoint().STY, result.STStartPoint().STY);

                Assert.AreEqual(shape.STEndPoint().STX, result.STEndPoint().STX);
                Assert.AreEqual(shape.STEndPoint().STY, result.STEndPoint().STY);

                Assert.AreEqual(2, (int)result.STNumPoints());
            }
        }
    }

    public class GeoTestTable
    {
        [AutoIncrement()]
        public long ID { get; set; }

        public SqlGeography Location { get; set; }

        public SqlGeometry Shape { get; set; }
    }
}
