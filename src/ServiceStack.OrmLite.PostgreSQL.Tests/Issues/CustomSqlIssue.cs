using System;
using System.Data;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Tests;
using System.Collections.Generic;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.PostgreSQL.Tests.Issues
{
    [Alias("color")]
    public class ColorModel
    {
        public string Color { get; set; }
        public string Value { get; set; }
    }

    public class ColorJsonModel
    {
        public int Id { get; set; }
        public string ColorJson { get; set; }
    }

    public class OrmLiteModelArrayTests : OrmLiteTestBase
    {
        public OrmLiteModelArrayTests() : base(Dialect.PostgreSql) {}

        [Test]
        public void test_model_with_array_to_json()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ColorModel>();

                db.Insert(new ColorModel { Color = "red", Value = "#f00" });
                db.Insert(new ColorModel { Color = "green", Value = "#0f0" });
                db.Insert(new ColorModel { Color = "blue", Value = "#00f" });
                db.Insert(new ColorModel { Color = "cyan", Value = "#0ff" });
                db.Insert(new ColorModel { Color = "magenta", Value = "#f0f" });
                db.Insert(new ColorModel { Color = "yellow", Value = "#ff0" });
                db.Insert(new ColorModel { Color = "black", Value = "#000" });

                const string sql = @"SELECT 1::integer AS id
                                        , json_agg(color.*) AS color_json
                                    FROM color;";

                var results = db.Select<ColorJsonModel>(sql);

                //results.PrintDump();

                Assert.That(results.Count, Is.EqualTo(1));

                foreach (var result in results)
                {
                    Assert.That(result.Id, Is.EqualTo(1));
                    Assert.That(result.ColorJson, Is.Not.Null);
                }

            }
        }

        [Test]
        public void test_model_with_array_and_json()
        {
            //OrmLiteConfig.DeoptimizeReader = true;

            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ColorModel>();

                db.Insert(new ColorModel { Color = "red", Value = "#f00" });
                db.Insert(new ColorModel { Color = "green", Value = "#0f0" });
                db.Insert(new ColorModel { Color = "blue", Value = "#00f" });
                db.Insert(new ColorModel { Color = "cyan", Value = "#0ff" });
                db.Insert(new ColorModel { Color = "magenta", Value = "#f0f" });
                db.Insert(new ColorModel { Color = "yellow", Value = "#ff0" });
                db.Insert(new ColorModel { Color = "black", Value = "#000" });

                // SQL contains array and json aggs.
                // We usually have ARRAY fields defined in the db, but when
                // retrieved we json-ize them. In otherwords the array exists in the tables/views.
                // We use SELECT.* which would contain the ARRAY field.
                // Array fields are not used in any of our models and should not cause the other
                // fields in the model to not be populated.
                const string sql = @"SELECT 1::integer AS id
                                            , json_agg(color.*) AS color_json
                                            , array_agg(color.*) AS color_array
                                    FROM color;";

                var results = db.Select<ColorJsonModel>(sql);

                Assert.That(results.Count, Is.EqualTo(1));

                foreach (var result in results)
                {
                    result.ColorJson.Print();
                    Assert.That(result.Id, Is.EqualTo(1));
                    Assert.That(result.ColorJson, Is.Not.Null);
                }
            }
        }
    }

}