using System;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.SqlServerTests
{
	public class DateTimeOffsetTests : OrmLiteTestBase
	{
		[Test]
		public void CanCreateTable()
		{
			ConnectionString.OpenDbConnection().CreateTable<TypeWithDateTimeOffset>();
		}

		[Test]
		public void CanStoreDateTimeOffset()
		{
			using (var con = ConnectionString.OpenDbConnection())
			{
				con.CreateTable<TypeWithDateTimeOffset>(true);
				con.Save(new TypeWithDateTimeOffset() { DateTimeOffset = DateTimeOffset.UtcNow });
			}
		}

		[Test]
		public void CanGetDateTimeOffset()
		{
			using (var con = ConnectionString.OpenDbConnection())
			{
				con.CreateTable<TypeWithDateTimeOffset>(true);
				var obj = new TypeWithDateTimeOffset { DateTimeOffset = DateTimeOffset.UtcNow };
				con.Save(obj);
				obj.Id = (int)con.GetLastInsertId();
				var target = con.GetById<TypeWithDateTimeOffset>(obj.Id);
				Assert.AreEqual(obj.Id, target.Id);
				var dateTimeOffsetFormatString = "yyyy-MM-dd HH:mm:ss.ffffff zzz";
				Assert.AreEqual(obj.DateTimeOffset.ToString(dateTimeOffsetFormatString), target.DateTimeOffset.ToString(dateTimeOffsetFormatString));
			}
		}

		[Test]
		public void CanQueryByDateTimeOffset_using_select_with_expression()
		{
			using (var con = ConnectionString.OpenDbConnection())
			{
				con.CreateTable<TypeWithDateTimeOffset>(true);
				var now = DateTimeOffset.Now;
				var tenDaysAgo = DateTimeOffset.Now.Subtract(new TimeSpan(10, 0, 0, 0));
				con.Save(new TypeWithDateTimeOffset { DateTimeOffset = tenDaysAgo });
				con.Save(new TypeWithDateTimeOffset { DateTimeOffset = now });
				con.Save(new TypeWithDateTimeOffset { DateTimeOffset = now });

				var target = con.Select<TypeWithDateTimeOffset>(q => q.DateTimeOffset == now);

				Assert.AreEqual(2, target.Count());
			}
		}

		[Test]
		public void CanQueryByDateTimeOffset_using_select_with_string()
		{
			using (var con = ConnectionString.OpenDbConnection())
			{
				con.CreateTable<TypeWithDateTimeOffset>(true);
				var now = DateTimeOffset.Now;
				var fortyTwoDaysAgo = DateTimeOffset.Now.Subtract(new TimeSpan(42, 0, 0, 0));
				con.Save(new TypeWithDateTimeOffset { DateTimeOffset = now });
				con.Save(new TypeWithDateTimeOffset { DateTimeOffset = now });
				con.Save(new TypeWithDateTimeOffset { DateTimeOffset = fortyTwoDaysAgo });

				var target = con.Select<TypeWithDateTimeOffset>("DateTimeOffset = {0}", now);

				Assert.AreEqual(2, target.Count());
			}
		}

		[Test]
		public void CanQueryByDateTimeOffset_using_where_with_AnonType()
		{
			using (var con = ConnectionString.OpenDbConnection())
			{
				con.CreateTable<TypeWithDateTimeOffset>(true);
				var fortyTwoDaysAgo = DateTimeOffset.Now.Subtract(new TimeSpan(42, 0, 0, 0));
				var utcNow = DateTimeOffset.UtcNow;
				con.Save(new TypeWithDateTimeOffset { DateTimeOffset = utcNow });
				con.Save(new TypeWithDateTimeOffset { DateTimeOffset = utcNow });
				con.Save(new TypeWithDateTimeOffset { DateTimeOffset = fortyTwoDaysAgo });

				var target = con.Where<TypeWithDateTimeOffset>(new { DateTimeOffset = fortyTwoDaysAgo });

				Assert.AreEqual(1, target.Count());
			}
		}

		public class TypeWithDateTimeOffset
		{
			[AutoIncrement, PrimaryKey]
			public int Id { get; set; }
			public DateTimeOffset DateTimeOffset { get; set; }
		}
	}
}