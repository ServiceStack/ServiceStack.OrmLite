using System;
using System.IO;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.OrmLite.Tests
{
	[TestFixture]
	public class SqliteOrmLiteConnectionTests 
		: OrmLiteTestBase
	{
        [SetUp]
        public void SetUp()
        {
            OrmLiteConfig.DialectProvider = SqliteDialect.Provider;
            ConnectionString = "test.sqlite";
        }

		[Test]
		public void Can_create_connection()
		{
			using (var db = OpenDbConnection())
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
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithIdAndName>();
                db.Insert(new ModelWithIdAndName(1));

                using (var dbReadOnly = OpenDbConnection())
                {
                    dbReadOnly.Insert(new ModelWithIdAndName(2));
                    var rows = dbReadOnly.Select<ModelWithIdAndName>();
                    Assert.That(rows, Has.Count.EqualTo(2));
                }
            }
		}

	}
}