using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                db.DropAndCreateTable<ModelWithFieldsOfDifferentTypes>();

				var rowIds = new List<int>(new[] { 1, 2, 3 });

                for (var i = 0; i < rowIds.Count; i++)
                    rowIds[i] = (int)db.Insert(ModelWithFieldsOfDifferentTypes.Create(rowIds[i]), selectIdentity: true);

                var row = db.SingleById<ModelWithFieldsOfDifferentTypes>(rowIds[1]);

                Assert.That(row.Id, Is.EqualTo(rowIds[1]));
			}
		}

		[Test]
		public void Can_GetById_string_from_ModelWithOnlyStringFields_table()
		{
			using (var db = OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithOnlyStringFields>();

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

                var row = db.SingleById<ModelWithOnlyStringFields>("id-1");

				Assert.That(row.Id, Is.EqualTo("id-1"));
			}
		}

		[Test]
		public void Can_GetByIds_int_from_ModelWithFieldsOfDifferentTypes_table()
		{
			using (var db = OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithFieldsOfDifferentTypes>();

				var rowIds = new List<int>(new[] { 1, 2, 3 });

                for (var i = 0; i < rowIds.Count; i++)
                    rowIds[i] = (int)db.Insert(ModelWithFieldsOfDifferentTypes.Create(rowIds[i]), selectIdentity: true);

				var rows = db.SelectByIds<ModelWithFieldsOfDifferentTypes>(rowIds);
				var dbRowIds = rows.ConvertAll(x => x.Id);

                Assert.That(dbRowIds, Is.EquivalentTo(rowIds));
			}
		}

		[Test]
		public void Can_GetByIds_string_from_ModelWithOnlyStringFields_table()
		{
			using (var db = OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithOnlyStringFields>();

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

				var rows = db.SelectByIds<ModelWithOnlyStringFields>(rowIds);
				var dbRowIds = rows.ConvertAll(x => x.Id);

				Assert.That(dbRowIds, Is.EquivalentTo(rowIds));
			}
		}

		[Test]
		public void Can_select_with_filter_from_ModelWithOnlyStringFields_table()
		{
			using (var db = OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithOnlyStringFields>();

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

				var filterRow = ModelWithOnlyStringFields.Create("id-4");
				filterRow.AlbumName = "FilteredName";

				db.Insert(filterRow);

				var rows = db.SelectFmt<ModelWithOnlyStringFields>("AlbumName".SqlColumn() + " = {0}", filterRow.AlbumName);
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
                db.DropAndCreateTable<ModelWithIdAndName>();

				n.Times(x => db.Insert(ModelWithIdAndName.Create(x)));

                var count = db.ScalarFmt<int>("SELECT COUNT(*) FROM {0}".Fmt("ModelWithIdAndName".SqlTable()));

				Assert.That(count, Is.EqualTo(n));
			}
		}

		[Test]
		public void Can_loop_each_string_from_ModelWithOnlyStringFields_table()
		{
			using (var db = OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithOnlyStringFields>();

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

				var dbRowIds = new List<string>();
				foreach (var row in db.SelectLazy<ModelWithOnlyStringFields>())
				{
					dbRowIds.Add(row.Id);
				}

				Assert.That(dbRowIds, Is.EquivalentTo(rowIds));
			}
		}

        [Test]
        public void Can_loop_each_string_from_ModelWithOnlyStringFields_table_Column()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithOnlyStringFields>();

                var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

                rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

                var dbRowIds = new List<string>();
                foreach (var rowId in db.ColumnLazy<string>(
                    db.From<ModelWithOnlyStringFields>().Select(x => x.Id)))
                {
                    dbRowIds.Add(rowId);
                }

                Assert.That(dbRowIds, Is.EquivalentTo(rowIds));
            }
        }

		[Test]
		public void Can_loop_each_with_filter_from_ModelWithOnlyStringFields_table()
		{
			using (var db = OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithOnlyStringFields>();

				var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

				rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

				var filterRow = ModelWithOnlyStringFields.Create("id-4");
				filterRow.AlbumName = "FilteredName";

				db.Insert(filterRow);

				var dbRowIds = new List<string>();
				var rows = db.SelectLazyFmt<ModelWithOnlyStringFields>("AlbumName".SqlColumn() + " = {0}", filterRow.AlbumName);
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
                db.DropAndCreateTable<ModelWithIdAndName>();

				n.Times(x => db.Insert(ModelWithIdAndName.Create(x)));

                var ids = db.ColumnFmt<int>("SELECT Id FROM {0}".Fmt("ModelWithIdAndName".SqlTable()));

				Assert.That(ids.Count, Is.EqualTo(n));
			}
		}

		[Test]
		public void Can_GetFirstColumnDistinct()
		{
			const int n = 5;

			using (var db = OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithIdAndName>();

				n.Times(x => db.Insert(ModelWithIdAndName.Create(x)));

                var ids = db.ColumnDistinctFmt<int>("SELECT Id FROM {0}".Fmt("ModelWithIdAndName".SqlTable()));

				Assert.That(ids.Count, Is.EqualTo(n));
			}
		}

		[Test]
		public void Can_GetLookup()
		{
			const int n = 5;

			using (var db = OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithIdAndName>();

				n.Times(x => {
					var row = ModelWithIdAndName.Create(x);
					row.Name = x % 2 == 0 ? "OddGroup" : "EvenGroup";
					db.Insert(row);
				});

                var lookup = db.LookupFmt<string, int>("SELECT Name, Id FROM {0}".Fmt("ModelWithIdAndName".SqlTable()));

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
				db.DropAndCreateTable<ModelWithIdAndName>();

				n.Times(x => db.Insert(ModelWithIdAndName.Create(x)));

                var dictionary = db.Dictionary<int, string>("SELECT Id, Name FROM {0}".Fmt("ModelWithIdAndName".SqlTable()));

				Assert.That(dictionary, Has.Count.EqualTo(5));

				//Console.Write(dictionary.Dump());
			}
		}

		[Test]
		public void Can_Select_subset_ModelWithIdAndName_from_ModelWithFieldsOfDifferentTypes_table()
		{
			using (var db = OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithFieldsOfDifferentTypes>();

				var rowIds = new List<int>(new[] { 1, 2, 3 });

                for (var i = 0; i < rowIds.Count; i++)
                    rowIds[i] = (int)db.Insert(ModelWithFieldsOfDifferentTypes.Create(rowIds[i]), selectIdentity: true);

                SuppressIfOracle("Oracle provider doesn't modify user supplied SQL to conform to name length restrictions");

                var rows = db.SelectFmt<ModelWithIdAndName>("SELECT Id, Name FROM {0}".Fmt("ModelWithFieldsOfDifferentTypes".SqlTable()));
				var dbRowIds = rows.ConvertAll(x => x.Id);

				Assert.That(dbRowIds, Is.EquivalentTo(rowIds));
			}
		}

		[Test]
		public void Can_Select_Into_ModelWithIdAndName_from_ModelWithFieldsOfDifferentTypes_table()
		{
            using (var db = OpenDbConnection())
			{
                db.DropAndCreateTable<ModelWithFieldsOfDifferentTypes>();

				var rowIds = new List<int>(new[] { 1, 2, 3 });

                for (var i = 0; i < rowIds.Count; i++)
                    rowIds[i] = (int)db.Insert(ModelWithFieldsOfDifferentTypes.Create(rowIds[i]), selectIdentity: true);
                
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
                db.DropAndCreateTable<ModelWithIdAndName>();

				n.Times(x => db.Insert(ModelWithIdAndName.Create(x)));

				var selectInNames = new[] {"Name1", "Name2"};
				var rows = db.SelectFmt<ModelWithIdAndName>("Name IN ({0})", selectInNames.SqlInValues());

				Assert.That(rows.Count, Is.EqualTo(selectInNames.Length));
			}
		}

		public class PocoFlag
		{
			public string Name { get; set; }
			public bool Flag { get; set; }
		}

		[Test]
		public void Can_populate_PocoFlag()
		{
			using (var db = OpenDbConnection())
			{
				var rows = db.SelectFmt<PocoFlag>("SELECT 1 as Flag");
				Assert.That(rows[0].Flag);
			}
		}

		public class PocoFlagWithId
		{
			public int Id { get; set; }
			public bool Flag { get; set; }
		}

		[Test]
		public void Can_populate_PocoFlagWithId()
		{
			using (var db = OpenDbConnection())
			{
				var rows = db.SelectFmt<PocoFlagWithId>("SELECT 1 as Id, 1 as Flag");
				Assert.That(rows[0].Id, Is.EqualTo(1));
				Assert.That(rows[0].Flag);
			}
		}

        public class TypeWithTimeSpan
        {
            public int Id { get; set; }
            public TimeSpan TimeSpan { get; set; }
        }

        [Test]
        public void Can_handle_TimeSpans()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TypeWithTimeSpan>();

                var timeSpan = new TimeSpan(1, 1, 1, 1);
                db.Insert(new TypeWithTimeSpan { Id = 1, TimeSpan = timeSpan });

                var model = db.SingleById<TypeWithTimeSpan>(1);

                Assert.That(model.TimeSpan, Is.EqualTo(timeSpan));
            }
        }

	    [Test]
	    public void Does_return_correct_numeric_values()
	    {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithDifferentNumTypes>();

                var row = ModelWithDifferentNumTypes.Create(1);

                db.Insert(row);

                var fromDb = db.Select<ModelWithDifferentNumTypes>().First();

                Assert.That(row.Short, Is.EqualTo(fromDb.Short));
                Assert.That(row.Int, Is.EqualTo(fromDb.Int));
                Assert.That(row.Long, Is.EqualTo(fromDb.Long));
                Assert.That(row.Float, Is.EqualTo(fromDb.Float));
                Assert.That(row.Double, Is.EqualTo(fromDb.Double));
                Assert.That(row.Decimal, Is.EqualTo(fromDb.Decimal));
            }
        }

	    [TestCase(1E125)]
	    [TestCase(-1E125)]
	    public void Does_return_large_double_values(double value)
	    {
	        using (var db = OpenDbConnection())
	        {
	            db.DropAndCreateTable<ModelWithDifferentNumTypes>();
	            var expected = new ModelWithDifferentNumTypes {Double = value};

	            var id = db.Insert(expected, true);
	            var actual = db.SingleById<ModelWithDifferentNumTypes>(id);

	            Assert.That(expected.Double, Is.EqualTo(actual.Double));
	        }
	    }
	}
}
