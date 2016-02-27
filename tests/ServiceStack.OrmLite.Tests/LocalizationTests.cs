using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
	[TestFixture]
	public class LocalizationTests : OrmLiteTestBase
	{
		private readonly CultureInfo CurrentCulture = Thread.CurrentThread.CurrentCulture;
		private readonly CultureInfo CurrentUICulture = Thread.CurrentThread.CurrentUICulture;

		[SetUp]
		public void TestSetUp()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("vi-VN");
		}

		[TearDown]
		public void TestFixtureTearDown()
		{
			Thread.CurrentThread.CurrentCulture = CurrentCulture;
			Thread.CurrentThread.CurrentUICulture = CurrentUICulture;
		}

		public class Point
		{
			[AutoIncrement]
			public int Id { get; set; }
			public short Width { get; set; }
			public float Height { get; set; }
			public double Top { get; set; }
			public decimal Left { get; set; }
		}

		[Test]
		public void Can_query_using_float_in_alernate_culuture()
		{
			using (var db = OpenDbConnection())
			{
				db.CreateTable<Point>(true);

				db.Insert(new Point { Width = 4, Height = 1.123f, Top = 3.456d, Left = 2.345m});
                db.GetLastSql().Print();

                var sql = Dialect == Dialect.PostgreSql
                    ? "round(cast(Height as numeric),3)={0}"
                    : "round(Height,3)={0}";
                var points = db.SelectFmt<Point>(sql, 1.123);
			    db.GetLastSql().Print();

				Assert.That(points[0].Width, Is.EqualTo(4));
				Assert.That(points[0].Height, Is.EqualTo(1.123f));
				Assert.That(points[0].Top, Is.EqualTo(3.456d).Within(1d));
				Assert.That(points[0].Left, Is.EqualTo(2.345m).Within(1m));
			}
		}

	}
}