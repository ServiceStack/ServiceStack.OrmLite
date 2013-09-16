using System;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Common;
using ServiceStack.Common.Tests.Models;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
	[TestFixture]
	public class OrmLiteSelectTests
		: OrmLiteTestBase
	{

		[Test]
		public void Can_GetById_int_from_ModelWithFieldsOfDifferentTypes_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithFieldsOfDifferentTypes>(true);

				var rowIds = new List<int>(new[] { 1, 2, 3 });

				rowIds.ForEach(x => db.Insert(ModelWithFieldsOfDifferentTypes.Create(x)));

				var row = db.GetById<ModelWithFieldsOfDifferentTypes>(1);

				Assert.That(row.Id, Is.EqualTo(1));
			}
		}

		[Test]
		public void Can_GetById_string_from_ModelWithOnlyStringFields_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

				var row = db.GetById<ModelWithOnlyStringFields>("id-1");

				Assert.That(row.Id, Is.EqualTo("id-1"));
			}
		}

		[Test]
		public void Can_GetByIds_int_from_ModelWithFieldsOfDifferentTypes_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithFieldsOfDifferentTypes>(true);

				var rowIds = new List<int>(new[] { 1, 2, 3 });

				rowIds.ForEach(x => db.Insert(ModelWithFieldsOfDifferentTypes.Create(x)));

				var rows = db.GetByIds<ModelWithFieldsOfDifferentTypes>(rowIds);
				var dbRowIds = rows.ConvertAll(x => x.Id);

				Assert.That(dbRowIds, Is.EquivalentTo(rowIds));
			}
		}

		[Test]
		public void Can_GetByIds_string_from_ModelWithOnlyStringFields_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

				var rows = db.GetByIds<ModelWithOnlyStringFields>(rowIds);
				var dbRowIds = rows.ConvertAll(x => x.Id);

				Assert.That(dbRowIds, Is.EquivalentTo(rowIds));
			}
		}

		[Test]
		public void Can_select_with_filter_from_ModelWithOnlyStringFields_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

				var filterRow = ModelWithOnlyStringFields.Create("id-4");
				filterRow.AlbumName = "FilteredName";

				db.Insert(filterRow);

				var rows = db.Select<ModelWithOnlyStringFields>("\"AlbumName\" = {0}", filterRow.AlbumName);
				var dbRowIds = rows.ConvertAll(x => x.Id);

				Assert.That(dbRowIds, Has.Count.EqualTo(1));
				Assert.That(dbRowIds[0], Is.EqualTo(filterRow.Id));
			}
		}

		[Test]
		public void Can_select_scalar_value()
		{
			const int n = 5;

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);

				n.Times(x => db.Insert(ModelWithIdAndName.Create(x)));

				var count = db.GetScalar<long>("SELECT COUNT(*) FROM \"ModelWithIdAndName\"");

				Assert.That(count, Is.EqualTo(n));
			}
		}

		[Test]
		public void Can_loop_each_string_from_ModelWithOnlyStringFields_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

				var dbRowIds = new List<string>();
				foreach (var row in db.Each<ModelWithOnlyStringFields>())
				{
					dbRowIds.Add(row.Id);
				}

				Assert.That(dbRowIds, Is.EquivalentTo(rowIds));
			}
		}

		[Test]
		public void Can_loop_each_with_filter_from_ModelWithOnlyStringFields_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithOnlyStringFields>(true);

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

				var filterRow = ModelWithOnlyStringFields.Create("id-4");
				filterRow.AlbumName = "FilteredName";

				db.Insert(filterRow);

				var dbRowIds = new List<string>();
				var rows = db.Each<ModelWithOnlyStringFields>("\"AlbumName\" = {0}", filterRow.AlbumName);
				foreach (var row in rows)
				{
					dbRowIds.Add(row.Id);
				}

				Assert.That(dbRowIds, Has.Count.EqualTo(1));
				Assert.That(dbRowIds[0], Is.EqualTo(filterRow.Id));
			}
		}

		[Test]
		public void Can_GetFirstColumn()
		{
			const int n = 5;

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);

				n.Times(x => db.Insert(ModelWithIdAndName.Create(x)));

				var ids = db.GetFirstColumn<int>("SELECT \"Id\" FROM \"ModelWithIdAndName\"");

				Assert.That(ids.Count, Is.EqualTo(n));
			}
		}

		[Test]
		public void Can_GetFirstColumnDistinct()
		{
			const int n = 5;

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);

				n.Times(x => db.Insert(ModelWithIdAndName.Create(x)));

				var ids = db.GetFirstColumnDistinct<int>("SELECT \"Id\" FROM \"ModelWithIdAndName\"");

				Assert.That(ids.Count, Is.EqualTo(n));
			}
		}

		[Test]
		public void Can_GetLookup()
		{
			const int n = 5;

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);

				n.Times(x => {
					var row = ModelWithIdAndName.Create(x);
					row.Name = x % 2 == 0 ? "OddGroup" : "EvenGroup";
					db.Insert(row);
				});

				var lookup = db.GetLookup<string, int>("SELECT \"Name\", \"Id\" FROM \"ModelWithIdAndName\"");

				Assert.That(lookup, Has.Count.EqualTo(2));
				Assert.That(lookup["OddGroup"], Has.Count.EqualTo(3));
				Assert.That(lookup["EvenGroup"], Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void Can_GetDictionary()
		{
			const int n = 5;

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);

				n.Times(x => db.Insert(ModelWithIdAndName.Create(x)));

				var dictionary = db.GetDictionary<int, string>("SELECT \"Id\", \"Name\" FROM \"ModelWithIdAndName\"");

				Assert.That(dictionary, Has.Count.EqualTo(5));

				//Console.Write(dictionary.Dump());
			}
		}

		[Test]
		public void Can_Select_subset_ModelWithIdAndName_from_ModelWithFieldsOfDifferentTypes_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithFieldsOfDifferentTypes>(true);

				var rowIds = new List<int>(new[] { 1, 2, 3 });

				rowIds.ForEach(x => db.Insert(ModelWithFieldsOfDifferentTypes.Create(x)));

				var rows = db.Select<ModelWithIdAndName>("SELECT \"Id\", \"Name\" FROM \"ModelWithFieldsOfDifferentTypes\"");
				var dbRowIds = rows.ConvertAll(x => x.Id);

				Assert.That(dbRowIds, Is.EquivalentTo(rowIds));
			}
		}

		[Test]
		public void Can_Query_ModelWithFieldsOfDifferentTypes_with_dictionary_parameters()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithFieldsOfDifferentTypes>(true);

				var rowIds = new List<int>(new[] { 1, 2, 3 });

				rowIds.ForEach(x => db.Insert(ModelWithFieldsOfDifferentTypes.Create(x)));

				var rows = db.Query<ModelWithFieldsOfDifferentTypes>("SELECT * FROM \"ModelWithFieldsOfDifferentTypes\" where \"Id\" = :Id ",
					new Dictionary<string, object> { 
					{"Id", 3}
					});
				 
				Assert.AreEqual(rows.Count, 1);
				Assert.AreEqual(rows[0].Id, 3);
			}
		}

		[Test]
		public void Can_Select_Into_ModelWithIdAndName_from_ModelWithFieldsOfDifferentTypes_table()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithFieldsOfDifferentTypes>(true);

				var rowIds = new List<int>(new[] { 1, 2, 3 });

				rowIds.ForEach(x => db.Insert(ModelWithFieldsOfDifferentTypes.Create(x)));

				var rows = db.Select<ModelWithIdAndName>(typeof(ModelWithFieldsOfDifferentTypes));
				var dbRowIds = rows.ConvertAll(x => x.Id);

				Assert.That(dbRowIds, Is.EquivalentTo(rowIds));
			}
		}


		[Test]
		public void Can_Select_In_for_string_value()
		{
			const int n = 5;

			using (var db = OpenDbConnection())
			{
				db.CreateTable<ModelWithIdAndName>(true);

				n.Times(x => db.Insert(ModelWithIdAndName.Create(x)));

				var selectInNames = new[] {"Name1", "Name2"};
				var rows = db.Select<ModelWithIdAndName>("\"Name\" IN ({0})", selectInNames.SqlInValues());

				Assert.That(rows.Count, Is.EqualTo(selectInNames.Length));
			}
		}

	}
}