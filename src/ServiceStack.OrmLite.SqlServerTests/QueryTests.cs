using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;

namespace ServiceStack.OrmLite.SqlServerTests
{
	public class QueryTests : OrmLiteTestBase
	{
        [Test]
        public void Can_GetSingle_with_multiple_sql_statements()
        {
            using (var db = ConnectionString.OpenDbConnection())
            {
                db.CreateTable<ModelWithOnlyStringFields>(true);

                var rowIds = new List<string>(new[] { "id-1", "id-2", "id-3" });

                rowIds.ForEach(x => db.Insert(ModelWithOnlyStringFields.Create(x)));

                var filterRow = ModelWithOnlyStringFields.Create("id-4");
                filterRow.AlbumName = "FilteredName";

                db.Insert(filterRow);

				var sql = "update [ModelWithOnlyStringFields] set AlbumName = @AlbumName + 'test' from [ModelWithOnlyStringFields] where AlbumName=@AlbumName;select * from [ModelWithOnlyStringFields] where AlbumName=@AlbumName + 'test'";

                var row = db.QuerySingle<ModelWithOnlyStringFields>(sql, new { filterRow.AlbumName });
                Assert.That(row.Id, Is.EqualTo(filterRow.Id));
            }
        }
	}
}
