using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite.Tests;

namespace ServiceStack.OrmLite.PostgreSQL.Tests
{
    [TestFixture]
    public class CreatePostgreSQLTablesTests : OrmLiteTestBase
    {

        [Test]
        public void can_create_tables_after_UseUnicode_or_DefaultStringLenght_changed()
        {
            //first one passes
            _reCreateTheTable();

            //all of these pass now:
            OrmLiteConfig.DialectProvider.UseUnicode = true;
            _reCreateTheTable();

            OrmLiteConfig.DialectProvider.UseUnicode = false;
            _reCreateTheTable();

            OrmLiteConfig.DialectProvider.DefaultStringLength = 98765;

            _reCreateTheTable();
        }

        private void _reCreateTheTable()
        {
            using(var db = ConnectionString.OpenDbConnection()) {
                db.CreateTable<CreatePostgreSQLTablesTests_dummy_table>(true);
            }
        }

        private class CreatePostgreSQLTablesTests_dummy_table
        {
            [AutoIncrement]
            public int Id { get; set; }

            public String StringNoExplicitLength { get; set; }

            [StringLength(100)]
            public String String100Characters { get; set; }
        }
    }
}
