using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.OrmLite.SqlServer;
using ServiceStack.OrmLite.Tests.App_Data;
using ServiceStack.OrmLite.Tests.UseCase;

namespace ServiceStack.OrmLite.Tests
{
    [TestFixture]
    public class OrmLiteInsertImageBlobTests
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=BpnCache_Spike;Integrated Security=SSPI;Connect Timeout=120;MultipleActiveResultSets=True";

        [Test]
        public void test_if_ormlite_sql_text_insert_works_for_image_blobs()
        {
            var dbFactory = new OrmLiteConnectionFactory(ConnectionString, SqlServerOrmLiteDialectProvider.Instance);

            using (var db = dbFactory.OpenDbConnection())
            using (var dbConn = db.CreateCommand())
            {
                dbConn.CreateTable<ImageBlobDto>(true);
                
                var dto = new ImageBlobDto()
                {
                    Image1 = ImageToBytes(ImageBlobResource.Bild),
                    Image2 = ImageToBytes(ImageBlobResource.Bild2),
                    Image3 = ImageToBytes(ImageBlobResource.Bild3)
                };

                dbConn.Insert(dto);
                Assert.Greater(0, dbConn.GetLastInsertId());
            }
        }

        [Test]
        public void test_if_ormlite_sql_prepare_insert_works_for_image_blobs()
        {
            var dbFactory = new OrmLiteConnectionFactory(ConnectionString, SqlServerOrmLiteDialectProvider.Instance);

            using (var db = dbFactory.OpenDbConnection())
            using (var dbConn = db.CreateCommand())
            {
                dbConn.CreateTable<ImageBlobDto>(true);

                var dto = new ImageBlobDto()
                {
                    Image1 = ImageToBytes(ImageBlobResource.Bild),
                    Image2 = ImageToBytes(ImageBlobResource.Bild2),
                    Image3 = ImageToBytes(ImageBlobResource.Bild3),
                    Complex = new object[] { "Foo", "Bar", "Baz", 1 }
                };

                var provider = OrmLiteConfig.DialectProvider as OrmLiteDialectProviderBase;
                Assert.NotNull(provider);

                using (var insertCmd = db.CreateInsertStatement(dto))
                {
                    var nInserted = insertCmd.ExecuteNonQuery();
                    Assert.Greater(nInserted, 0);

                    for (int i = 2; i < 101; i++)
                    {
                        dto.Complex = new object[] { "Foo", "Bar", "Baz", i };
                        insertCmd.ReparameterizeInsert(dto);
                        Assert.Greater(insertCmd.ExecuteNonQuery(), 0);
                    }
                }
            }
        }

        [Test]
        public void test_if_ormlite_sql_prepare_update_works_for_image_blobs()
        {
            var dbFactory = new OrmLiteConnectionFactory(ConnectionString, SqlServerOrmLiteDialectProvider.Instance);

            using (var db = dbFactory.OpenDbConnection())
            using (var dbConn = db.CreateCommand())
            {
                dbConn.CreateTable<ImageBlobDto>(true);

                var dto = new ImageBlobDto()
                {
                    Image1 = ImageToBytes(ImageBlobResource.Bild),
                    Image2 = ImageToBytes(ImageBlobResource.Bild2),
                    Image3 = ImageToBytes(ImageBlobResource.Bild3),
                    Complex = new object[] { "Foo", "Bar", "Baz", 1 }
                };

                var provider = OrmLiteConfig.DialectProvider as OrmLiteDialectProviderBase;
                Assert.NotNull(provider);

                using (var insertCmd = db.CreateInsertStatement(dto))
                {
                    var nInserted = insertCmd.ExecuteNonQuery();
                    Assert.Greater(nInserted, 0);   
                }

                dto.Id = 1;
                dto.Complex = new object[] { "Baz", "Bar", "Foo", 2 };
                using (var updateCmd = db.CreateUpdateStatement(dto))
                {
                    var nUpdated = updateCmd.ExecuteNonQuery();
                    Assert.Greater(nUpdated, 0);
                }
            }
        }

        protected byte[] ImageToBytes(Image i)
        {
            using (var s = new MemoryStream())
            {
                i.Save(s, ImageFormat.Png);
                return s.GetBuffer();
            }
        }
    }
}
