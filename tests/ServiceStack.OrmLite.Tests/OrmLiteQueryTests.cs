using System;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Tests
{
	[TestFixture]
	public class OrmLiteQueryTests
		: OrmLiteTestBase
	{

		[Test]
		public void Can_GetById_int_from_ModelWithFieldsOfDifferentTypes_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithFieldsOfDifferentTypes>();

				var rowIds = new List<int>(new[] { 1, 2, 3 });

				rowIds.ForEach(x => db.Insert(ModelWithFieldsOfDifferentTypes.Create(x)));

				var row = db.QueryById<ModelWithFieldsOfDifferentTypes>(1);

				Assert.That(row.Id, Is.EqualTo(1));
			}
		}

		[Test]
		public void Can_GetById_string_from_ModelWithOnlyStringFields_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithOnlyStringFields>();

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

				var row = db.QueryById<ModelWithOnlyStringFields>("id-1");

				Assert.That(row.Id, Is.EqualTo("id-1"));
			}
		}
		
		[Test]
		public void Can_select_with_filter_from_ModelWithOnlyStringFields_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithOnlyStringFields>();

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

				var filterRow = ModelWithOnlyStringFields.Create("id-4");
				filterRow.AlbumName = "FilteredName";

				db.Insert(filterRow);

				var rows = db.Where<ModelWithOnlyStringFields>(new { filterRow.AlbumName });
				var dbRowIds = rows.ConvertAll(x => x.Id);
				Assert.That(dbRowIds, Has.Count.EqualTo(1));
				Assert.That(dbRowIds[0], Is.EqualTo(filterRow.Id));

				rows = db.Where<ModelWithOnlyStringFields>(new { filterRow.AlbumName });
				dbRowIds = rows.ConvertAll(x => x.Id);
				Assert.That(dbRowIds, Has.Count.EqualTo(1));
				Assert.That(dbRowIds[0], Is.EqualTo(filterRow.Id));

				var queryByExample = new ModelWithOnlyStringFields { AlbumName = filterRow.AlbumName };
				rows = db.ByExampleWhere<ModelWithOnlyStringFields>(queryByExample);
				dbRowIds = rows.ConvertAll(x => x.Id);
				Assert.That(dbRowIds, Has.Count.EqualTo(1));
				Assert.That(dbRowIds[0], Is.EqualTo(filterRow.Id));

				rows = db.Query<ModelWithOnlyStringFields>(
					"SELECT * FROM ModelWithOnlyStringFields WHERE AlbumName = @AlbumName", new { filterRow.AlbumName });
				dbRowIds = rows.ConvertAll(x => x.Id);
				Assert.That(dbRowIds, Has.Count.EqualTo(1));
				Assert.That(dbRowIds[0], Is.EqualTo(filterRow.Id));
			}
		}

		[Test]
		public void Can_loop_each_with_filter_from_ModelWithOnlyStringFields_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithOnlyStringFields>();

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

				var filterRow = ModelWithOnlyStringFields.Create("id-4");
				filterRow.AlbumName = "FilteredName";

				db.Insert(filterRow);

				var dbRowIds = new List<string>();
				var rows = db.EachWhere<ModelWithOnlyStringFields>(new { filterRow.AlbumName });
				foreach (var row in rows)
				{
					dbRowIds.Add(row.Id);
				}

				Assert.That(dbRowIds, Has.Count.EqualTo(1));
				Assert.That(dbRowIds[0], Is.EqualTo(filterRow.Id));
			}
		}

        [Test]
        public void Can_GetSingle_with_filter_from_ModelWithOnlyStringFields_table()
        {
            using (var db = ConnectionString.OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithOnlyStringFields>();

                var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

                rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

                var filterRow = ModelWithOnlyStringFields.Create("id-4");
                filterRow.AlbumName = "FilteredName";

                db.Insert(filterRow);

                var row = db.QuerySingle<ModelWithOnlyStringFields>(new { filterRow.AlbumName });
                Assert.That(row.Id, Is.EqualTo(filterRow.Id));

                row = db.QuerySingle<ModelWithOnlyStringFields>(new { filterRow.AlbumName, Id = (object)null });
                Assert.That(row.AlbumName, Is.EqualTo(filterRow.AlbumName));

                row = db.QuerySingle<ModelWithOnlyStringFields>(new { AlbumName = "Junk", Id = (object)null });
                Assert.That(row, Is.Null);
            }
        }

        class Note
        {
            [AutoIncrement] // Creates Auto primary key
            public int Id { get; set; }

            public string SchemaUri { get; set; }
            public string NoteText { get; set; }
            public DateTime? LastUpdated { get; set; }
            public string UpdatedBy { get; set; }
        }

        [Test]
        public void Can_query_where_and_select_Notes()
        {
            using (var db = ConnectionString.OpenDbConnection())
            {
                db.DropAndCreateTable<Note>();

                db.Insert(new Note
                    {
                        SchemaUri = "tcm:0-0-0",
                        NoteText = "Hello world 5",
                        LastUpdated = new DateTime(2013, 1, 5),
                        UpdatedBy = "RC"
                    });

                var notes = db.Where<Note>(new { SchemaUri = "tcm:0-0-0" });
                Assert.That(notes[0].Id, Is.EqualTo(1));
                Assert.That(notes[0].NoteText, Is.EqualTo("Hello world 5"));

                notes = db.Select<Note>("SchemaUri={0}", "tcm:0-0-0");
                Assert.That(notes[0].Id, Is.EqualTo(1));
                Assert.That(notes[0].NoteText, Is.EqualTo("Hello world 5"));

                notes = db.Query<Note>("SELECT * FROM Note WHERE SchemaUri=@schemaUri", new { schemaUri = "tcm:0-0-0" });
                Assert.That(notes[0].Id, Is.EqualTo(1));
                Assert.That(notes[0].NoteText, Is.EqualTo("Hello world 5"));

                notes = db.Query<Note>("SchemaUri=@schemaUri", new { schemaUri = "tcm:0-0-0" });
                Assert.That(notes[0].Id, Is.EqualTo(1));
                Assert.That(notes[0].NoteText, Is.EqualTo("Hello world 5"));
            }            
        }

        class NoteDto
        {
            public int Id { get; set; }
            public string SchemaUri { get; set; }
            public string NoteText { get; set; }
        }

        [Test]
        public void Can_select_NotesDto_with_pretty_sql()
        {
            using (var db = ConnectionString.OpenDbConnection())
            {
                db.DropAndCreateTable<Note>();

                db.Insert(new Note
                {
                    SchemaUri = "tcm:0-0-0",
                    NoteText = "Hello world 5",
                    LastUpdated = new DateTime(2013, 1, 5),
                    UpdatedBy = "RC"
                });

                var sql = @"
SELECT
Id, SchemaUri, NoteText
FROM Note
WHERE SchemaUri=@schemaUri
";

                var notes = db.Query<NoteDto>(sql, new { schemaUri = "tcm:0-0-0" });
                Assert.That(notes[0].Id, Is.EqualTo(1));
                Assert.That(notes[0].NoteText, Is.EqualTo("Hello world 5"));
            }
        }
	}
}