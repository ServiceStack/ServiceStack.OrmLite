using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using ServiceStack.OrmLite.Legacy;
using ServiceStack.OrmLite.MySql.Converters;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.MySql.Tests
{
    [TestFixture]
    public class DateTimeColumnTest
        : OrmLiteTestBase
    {
        [Test]
        public void Can_create_table_containing_DateTime_column()
        {
            using var db = OpenDbConnection();
            db.CreateTable<Analyze>(true);
        }

        [Test]
        public void Can_store_DateTime_Value()
        {
            using var db = OpenDbConnection();
            db.CreateTable<Analyze>(true);

            var obj = new Analyze {
                Id = 1,
                Date = DateTime.Now,
                Url = "http://www.google.com"
            };

            db.Save(obj);
        }

        [Test]
        public void Can_store_and_retrieve_DateTime_Value()
        {
            using var db = OpenDbConnection();
            db.CreateTable<Analyze>(true);

            var obj = new Analyze {
                Id = 1,
                Date = DateTime.Now,
                Url = "http://www.google.com"
            };

            db.Save(obj);

            var id = (int)db.LastInsertId();
            var target = db.SingleById<Analyze>(id);

            Assert.That(target, Is.Not.Null);
            Assert.That(target.Id, Is.EqualTo(id));
            Assert.That(target.Date, Is.EqualTo(obj.Date).Within(TimeSpan.FromSeconds(1)));
            Assert.That(target.Url, Is.EqualTo(obj.Url));
        }

        [Test]
        public void Can_change_DateTime_precision()
        {
            using var db = OpenDbConnection();
            if (MySqlDialect.Provider.GetConverter(typeof(DateTime)) is MySqlDateTimeConverterBase dateConverter)
            {
                dateConverter.Precision = 3;
            }

            OrmLiteUtils.PrintSql();
            db.CreateTable<Analyze>(true);

            var obj = new Analyze {
                Id = 1,
                Date = DateTime.Now,
                Url = "http://www.google.com"
            };

            db.Save(obj);

            var id = (int)db.LastInsertId();
            var target = db.SingleById<Analyze>(id);
            target.PrintDump();

            var q = db.From<Analyze>()
                .Where(x => x.Date >= DateTime.Now.Date);
            var results = db.Select(q);
            Assert.That(results.Count, Is.EqualTo(1));
            results.PrintDump();
            
            results = db.SelectFmt<Analyze>("AnalyzeDate >= {0}", DateTime.Now.AddDays(-1));
            results.PrintDump();
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
