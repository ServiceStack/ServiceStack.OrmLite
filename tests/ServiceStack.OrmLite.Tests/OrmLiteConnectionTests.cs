using System;
using System.IO;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.OrmLite.Tests
{
	[TestFixture]
	public class OrmLiteConnectionTests 
		: OrmLiteTestBase
	{
		[Test]
		public void Can_create_connection()
		{
			using (var db = ConnectionString.OpenDbConnection())
			{
			}
		}

		[Test]
		public void Can_create_ReadOnly_connection()
		{
			using (var db = ConnectionString.OpenReadOnlyDbConnection()) 
			{
			}
		}

		[Test]
		public void Can_create_table_with_ReadOnly_connection()
		{
			using (var db = ConnectionString.OpenReadOnlyDbConnection())
			{
				try
				{
					db.CreateTable<ModelWithIdAndName>(true);
					db.Insert(new ModelWithIdAndName(1));
				}
				catch (Exception ex)
				{
					Log(ex.Message);
					return;
				}
				Assert.Fail("Should not be able to create a table with a readonly connection");
			}
		}

		[Test]
		public void Can_open_two_ReadOnlyConnections_to_same_database()
		{
            var db = "test.sqlite".OpenDbConnection();
            db.CreateTable<ModelWithIdAndName>(true);
            db.Insert(new ModelWithIdAndName(1));

			var dbReadOnly = "test.sqlite".OpenDbConnection();
            dbReadOnly.Insert(new ModelWithIdAndName(2));
            var rows = dbReadOnly.Select<ModelWithIdAndName>();
            Assert.That(rows, Has.Count.EqualTo(2));

			dbReadOnly.Dispose();
			db.Dispose();
		}

	}
}