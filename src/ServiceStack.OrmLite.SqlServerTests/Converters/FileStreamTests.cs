using System;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.SqlServer.Converters;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.SqlServerTests.Converters
{
    [TestFixture]
    public class FileStreamTests : SqlServer2012ConvertersOrmLiteTestBase
    {
        [Explicit("Requires FileGroups enabled in DB")]
        [Test]
        public void Can_select_from_FileStream()
        {

            ConnectionString = "Data Source=localhost;Initial Catalog=test2;User Id=test;Password=test;Connect Timeout=120;MultipleActiveResultSets=True;Type System Version=SQL Server 2012;";
            var dialectProvider = SqlServerConverters.Configure(SqlServer2012Dialect.Provider);
            var dbFactory = new OrmLiteConnectionFactory(ConnectionString, dialectProvider);

            using (var db = dbFactory.OpenDbConnection())
            {
                db.DropTable<TestFile>();
                db.CreateTable<TestFile>();

                db.Insert(new TestFile { Contents = "contents".ToUtf8Bytes() });

                db.Select<TestFile>().PrintDump();

                //db.DropTable<FileStream>();
                //db.CreateTable<FileStream>();

                //db.Insert(new FileStream
                //{
                //    Name = "file.txt",
                //    Path = SqlHierarchyId.Parse("/1/2/3/"),
                //    ParentPath = SqlHierarchyId.Parse("/1/2/"),
                //    FileContent = "contents".ToUtf8Bytes(),
                //    FileType = MimeTypes.PlainText,
                //});

                //var q = db.From<FileStream>();
                //db.Select(q);
            }
        }
    }

    public class TestFile
    {
        [PrimaryKey]
        [CustomField("uniqueidentifier ROWGUIDCOL NOT NULL")]
        public Guid Id { get; set; }

        [CustomField("varbinary(max) FILESTREAM")]
        public byte[] Contents { get; set; }

        public bool IsDirectory { get; set; }

        [CustomSelect("Contents.GetFileNamespacePath() + (CASE WHEN is_directory = 1 THEN '\' ELSE '' END)")]
        public string FullPath { get; set; }
    }

    public class FileStream
    {
        [PrimaryKey]
        [CustomField("uniqueidentifier ROWGUIDCOL NOT NULL")]
        public Guid Id { get; set; }

        [CustomField("varbinary(max) FILESTREAM")]
        [Alias("file_stream")]
        //[DataAnnotations.Ignore]
        public byte[] FileContent { get; set; }

        [Alias("name")]
        [StringLength(255)]
        public string Name { get; set; }

        [Alias("path_locator")]
        public SqlHierarchyId Path { get; set; }

        //[ForeignKey(typeof(FileStream))]
        [Alias("parent_path_locator")]
        public SqlHierarchyId? ParentPath { get; set; }

        [Alias("file_type")]
        [Compute]
        [StringLength(255)]
        public string FileType { get; set; }

        [Alias("cached_file_size")]
        [Compute]
        public long? FileSize { get; set; }

        [Alias("creation_time")]
        public DateTimeOffset CreationDateTime { get; set; }

        [Alias("last_write_time")]
        public DateTimeOffset LastWriteDateTime { get; set; }

        [Alias("last_access_time")]
        public DateTimeOffset? LastAccessDateTime { get; set; }

        [Alias("is_directory")]
        public bool IsDirectory { get; set; }

        [Alias("is_offline")]
        public bool IsOffline { get; set; }

        [Alias("is_hidden")]
        public bool IsHidden { get; set; }

        [Alias("is_readonly")]
        public bool IsReadOnly { get; set; }

        [Alias("is_archive")]
        public bool IsArchive { get; set; }

        [Alias("is_system")]
        public bool IsSystem { get; set; }

        [Alias("is_temporary")]
        public bool IsTemporary { get; set; }

        [CustomSelect("file_stream.GetFileNamespacePath() + (CASE WHEN is_directory = 1 THEN '\' ELSE '' END)")]
        public string FullPath { get; set; }
    }
}