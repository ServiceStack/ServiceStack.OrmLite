using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.DesignPatterns.Model;

namespace ServiceStack.OrmLite.MySql.Tests
{
    [TestFixture]
    public class DateTimeColumnTest
        : OrmLiteTestBase
    {
        [Test]
        public void Can_create_table_containing_DateTime_column()
        {
            using (var dbConn = ConnectionString.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<Analyze>(true);
            }
        }

        [Test]
        public void Can_store_DateTime_Value()
        {
            using (var dbConn = ConnectionString.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<Analyze>(true);

                var obj = new Analyze
                              {
                                  Id = 1,
                                  Date = DateTime.Now,
                                  Url = "http://www.google.com"
                              };

                dbCmd.Save(obj);
            }
        }

        [Test]
        public void Can_store_and_retrieve_DateTime_Value()
        {
            using (var dbConn = ConnectionString.OpenDbConnection())
            using (var dbCmd = dbConn.CreateCommand())
            {
                dbCmd.CreateTable<Analyze>(true);

                var obj = new Analyze
                {
                    Id = 1,
                    Date = DateTime.Now,
                    Url = "http://www.google.com"
                };

                dbCmd.Save(obj);

                var id = (int) dbCmd.GetLastInsertId();
                var target = dbCmd.QueryById<Analyze>(id);

                Assert.IsNotNull(target);
                Assert.AreEqual(id, target.Id);
                Assert.AreEqual(obj.Date.ToString("yyyy-MM-dd HH:mm:ss"), target.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                Assert.AreEqual(obj.Url, target.Url);
            }
        }

        /// <summary>
        /// Provided by RyogoNA in issue #38 https://github.com/ServiceStack/ServiceStack.OrmLite/issues/38#issuecomment-4625178
        /// </summary>
        [Alias("Analyzes")]
        public class Analyze : IHasId<int>
        {
            [AutoIncrement]
            [PrimaryKey]
            public int Id
            {
                get;
                set;
            }
            [Alias("AnalyzeDate")]
            public DateTime Date
            {
                get;
                set;
            }
            public string Url
            {
                get;
                set;
            }
        }
    }
}
