using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ServiceStack.OrmLite.SqlServerTests
{
	public class InsertParam_GetLastInsertId : OrmLiteTestBase
	{
		[Test]
		public void Can_GetLastInsertedId_using_InsertParam()
		{
			var testObject = new SimpleType { Name = "test" };

			//verify that "normal" Insert works as expected
			using(var con = ConnectionString.OpenDbConnection()) {
				con.CreateTable<SimpleType>(true);

				con.Insert(testObject);
				var normalLastInsertedId = con.GetLastInsertId();
				Assert.Greater(normalLastInsertedId, 0, "normal Insert");
			}

			//test with InsertParam
			using(var con = ConnectionFactory.OpenDbConnection()) {
				con.CreateTable<SimpleType>(true);

				con.InsertParam(testObject);
				var insertParamLastInsertedId = con.GetLastInsertId();
				Assert.Greater(insertParamLastInsertedId, 0, "with InsertParam");
			}
		}
	}
}
