using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

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
			using (var dbCmd = db.CreateCommand())
			{
				dbCmd.CreateTable<ModelWithFieldsOfDifferentTypes>(true);

				var rowIds = new List<int>(new[] { 1, 2, 3 });

				rowIds.ForEach(x => dbCmd.Insert(ModelWithFieldsOfDifferentTypes.Create(x)));

				var row = dbCmd.QueryById<ModelWithFieldsOfDifferentTypes>(1);

				Assert.That(row.Id, Is.EqualTo(1));
			}
		}

		[Test]
		public void Can_GetById_string_from_ModelWithOnlyStringFields_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbCmd = db.CreateCommand())
			{
				dbCmd.CreateTable<ModelWithOnlyStringFields>(true);

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => dbCmd.Insert(ModelWithOnlyStringFields.Create(x)));

				var row = dbCmd.QueryById<ModelWithOnlyStringFields>("id-1");

				Assert.That(row.Id, Is.EqualTo("id-1"));
			}
		}
		
		[Test]
		public void Can_select_with_filter_from_ModelWithOnlyStringFields_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbCmd = db.CreateCommand())
			{
				dbCmd.CreateTable<ModelWithOnlyStringFields>(true);

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => dbCmd.Insert(ModelWithOnlyStringFields.Create(x)));

				var filterRow = ModelWithOnlyStringFields.Create("id-4");
				filterRow.AlbumName = "FilteredName";

				dbCmd.Insert(filterRow);

				var rows = dbCmd.Query<ModelWithOnlyStringFields>(new { filterRow.AlbumName });
				var dbRowIds = rows.ConvertAll(x => x.Id);
				Assert.That(dbRowIds, Has.Count.EqualTo(1));
				Assert.That(dbRowIds[0], Is.EqualTo(filterRow.Id));

				rows = dbCmd.Query<ModelWithOnlyStringFields>(new { AlbumName = filterRow.AlbumName });
				dbRowIds = rows.ConvertAll(x => x.Id);
				Assert.That(dbRowIds, Has.Count.EqualTo(1));
				Assert.That(dbRowIds[0], Is.EqualTo(filterRow.Id));

				var queryByExample = new ModelWithOnlyStringFields { AlbumName = filterRow.AlbumName };
				rows = dbCmd.QueryByExample<ModelWithOnlyStringFields>(queryByExample);
				dbRowIds = rows.ConvertAll(x => x.Id);
				Assert.That(dbRowIds, Has.Count.EqualTo(1));
				Assert.That(dbRowIds[0], Is.EqualTo(filterRow.Id));
			}
		}

		[Test]
		public void Can_loop_each_with_filter_from_ModelWithOnlyStringFields_table()
		{
			using (var db = ConnectionString.OpenDbConnection())
			using (var dbCmd = db.CreateCommand())
			{
				dbCmd.CreateTable<ModelWithOnlyStringFields>(true);

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => dbCmd.Insert(ModelWithOnlyStringFields.Create(x)));

				var filterRow = ModelWithOnlyStringFields.Create("id-4");
				filterRow.AlbumName = "FilteredName";

				dbCmd.Insert(filterRow);

				var dbRowIds = new List<string>();
				var rows = dbCmd.QueryEach<ModelWithOnlyStringFields>(new { filterRow.AlbumName });
				foreach (var row in rows)
				{
					dbRowIds.Add(row.Id);
				}

				Assert.That(dbRowIds, Has.Count.EqualTo(1));
				Assert.That(dbRowIds[0], Is.EqualTo(filterRow.Id));
			}
		}

	}
}