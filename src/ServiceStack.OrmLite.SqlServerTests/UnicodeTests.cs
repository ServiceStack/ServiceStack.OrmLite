using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServerTests
{
    /// <summary>
    /// test for issue #69
    /// </summary>
    class UnicodeTests : OrmLiteTestBase
    {
        [Test]
        public void can_insert_and_retrieve_unicode_values()
        {
            OrmLiteConfig.DialectProvider.UseUnicode = true;

            var testData = new[]{
                "árvíztűrő tükörfúrógép",
                "ÁRVÍZTŰRŐ TÜKÖRFÚRÓGÉP", //these are the Hungarian "special" characters, they work fine out of the box
                "♪♪♫"                     //this one comes back as 'ddd'
            };

            using(var con = ConnectionString.OpenDbConnection()) {
                con.ExecuteSql(table_re_creation_script);

                foreach(var item in testData) { con.Insert(new Unicode_poco { Text = item }); }

                var fromDb = con.Select<Unicode_poco>().Select(x => x.Text).ToArray();

                CollectionAssert.AreEquivalent(testData, fromDb);
            }
        }


        /* *
--if you run this in SSMS, it produces 'ddd'
INSERT INTO [Unicode_poco] ([Text]) VALUES ('hai ♪♪♫')

--if you run this in SSMS, it works fine
INSERT INTO [Unicode_poco] ([Text]) VALUES (N'hai ♪♪♫')
           
select * from Unicode_poco
         * */


        private class Unicode_poco
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string Text { get; set; }
        }

        /// <summary>
        /// because OrmLite does not create nvarchar columns
        /// </summary>
        private string table_re_creation_script = @"
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Unicode_poco]') AND type in (N'U'))
DROP TABLE [dbo].[Unicode_poco];


CREATE TABLE [dbo].[Unicode_poco](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Text] [nvarchar](4000) NULL,
 CONSTRAINT [PK_Unicode_poco] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]";
    }
}
