using System.IO;
using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests
{
    public class TypeWithByteArrayFieldTests : OrmLiteTestBase
    {
        [Test]
        public void CanInsertAndSelectByteArray()
        {
            var orig = new TypeWithByteArrayField { Id = 1, Content = new byte[] { 0, 17, 0, 17, 0, 7 } };

            using (var db = OpenDbConnection())
            {
                db.CreateTable<TypeWithByteArrayField>(true);

                db.Save(orig);

                var target = db.SingleById<TypeWithByteArrayField>(orig.Id);

                Assert.AreEqual(orig.Id, target.Id);
                Assert.AreEqual(orig.Content, target.Content);
            }
        }

        [Test, Explicit]
        public void Can_add_attachment()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Attachment>();

                var bytes = "https://www.google.com/images/srpr/logo11w.png".GetBytesFromUrl();

                var file = new Attachment {
                    Data = bytes,
                    Description = "Google Logo",
                    Type = "png",
                    FileName = "logo11w.png"
                };

                db.Insert(file);

                var fromDb = db.Single<Attachment>(q => q.FileName == "logo11w.png");

                Assert.AreEqual(file.Data, fromDb.Data);

                File.WriteAllBytes(fromDb.FileName, fromDb.Data);
            }
        }
    }

    class TypeWithByteArrayField
    {
        public int Id { get; set; }
        public byte[] Content { get; set; }
    }

    public class Attachment
    {
        public string Description { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public byte[] Data { get; set; }
    }
}