using System.ComponentModel.DataAnnotations;
using System.Data;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServerTests.UseCase
{
    [TestFixture]
    public class NestedComplexTypeUseCase : OrmLiteTestBase
    {
        public class Location
        {
            [AutoIncrement]
            public int Id { get; set; }

            [StringLength(50)]
            public string Description { get; set; }

            public GeoLocation GeoLocation { get; set; }
        }

        public class GeoLocation
        {
            [StringLength(50)]
            public string Latitude { get; set; }
            [StringLength(50)]
            public string Longitude { get; set; }
        }

        [Test]
        public void Handles_NULL_correctly_on_InsertParam_entity_with_nested_complex_type_where_nested_property_is_null()
        {
            using (IDbConnection db = OpenDbConnection())
            {
                db.CreateTable<Location>(true);

                var location = new Location
                {
                    Description = "HQ",
                    GeoLocation = null
                };

                location.Id = (int)db.InsertParam<Location>(location, true);

                var newLocation = db.GetByIdOrDefault<Location>(location.Id);

                Assert.That(newLocation, Is.Not.Null);
                Assert.That(newLocation.Id, Is.EqualTo(location.Id));
                Assert.That(newLocation.GeoLocation, Is.Null);
            }
        }

        [Test]
        public void Handles_NULL_correctly_on_Insert_entity_with_nested_complex_type_where_nested_property_is_null()
        {
            using (IDbConnection db = OpenDbConnection())
            {
                db.CreateTable<Location>(true);

                var location = new Location
                {
                    Description = "HQ",
                    GeoLocation = null
                };

                db.Insert<Location>(location);
                location.Id = (int)db.GetLastInsertId();

                var newLocation = db.GetByIdOrDefault<Location>(location.Id);

                Assert.That(newLocation, Is.Not.Null);
                Assert.That(newLocation.GeoLocation, Is.Null);
            }
        }
    }
}
