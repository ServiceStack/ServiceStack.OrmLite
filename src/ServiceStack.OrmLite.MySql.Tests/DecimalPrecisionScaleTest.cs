using System;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.DesignPatterns.Model;

namespace ServiceStack.OrmLite.MySql.Tests
{
	[TestFixture]
	public class DecimalPrecisionScaleTest
		: OrmLiteTestBase
	{
		[Test]
		public void CanCreateDecimalWithPrecisionAndScale()
		{
			var fielDef = ModelDefinition<DecimalTest>.Definition.GetFieldDefinition<DecimalTest> (f => f.SomeDecimalWithPrecisionAndScale);
			Assert.AreEqual("ALTER TABLE `DecimalTest` ADD COLUMN `SomeDecimalWithPrecisionAndScale` DECIMAL (15,2) NOT NULL;",
			                OrmLiteConfig.DialectProvider.ToAddColumnStatement (typeof(DecimalTest), fielDef));
		}
						
		[Test]
		public void CanCreateDecimal()
		{
			var fielDef = ModelDefinition<DecimalTest>.Definition.GetFieldDefinition<DecimalTest> (f => f.SomeDecimal);
			Assert.AreEqual("ALTER TABLE `DecimalTest` ADD COLUMN `SomeDecimal` DECIMAL (38,6) NOT NULL;",
			                OrmLiteConfig.DialectProvider.ToAddColumnStatement (typeof(DecimalTest), fielDef));
		}

		public class DecimalTest 
		{

			public int Id
			{
				get;
				set;
			}

			public decimal SomeDecimal
			{
				get;
				set;
			}

			[DecimalLength(15,2)]
			public decimal SomeDecimalWithPrecisionAndScale
			{
				get;
				set;
			}

		}
	}
}
