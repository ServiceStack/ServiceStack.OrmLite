﻿using NUnit.Framework;

namespace ServiceStack.OrmLite.Tests.Expressions
{
    public class ConditionalExpressionTest : ExpressionsTestBase
    {
        [Test]
        public void Can_select_conditional_and_expression()
        {
            var expected = new TestType()
            {
                IntColumn = 3,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);

            var actual = ConnectionString.OpenDbConnection().Select<TestType>(q => q.IntColumn > 2 && q.IntColumn < 4);

            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Count);
            CollectionAssert.Contains(actual, expected);
        }

        [Test]
        public void Can_select_conditional_or_expression()
        {
            var expected = new TestType()
            {
                IntColumn = 3,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);

            var actual = ConnectionString.OpenDbConnection().Select<TestType>(q => q.IntColumn == 3 || q.IntColumn < 0);

            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Count);
            CollectionAssert.Contains(actual, expected);
        }

        [Test]
        public void Can_select_evaluated_conditional_and_expression()
        {
            // ReSharper disable ConvertToConstant.Local
            var a = 10;
            var b = 5;
            // ReSharper restore ConvertToConstant.Local

            var expected = new TestType()
            {
                IntColumn = 3,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);

            var actual = ConnectionString.OpenDbConnection().Select<TestType>(q => q.BoolColumn == (a >= b && a > 0));

            Assert.IsNotNull(actual);
            Assert.Greater(actual.Count, 0);
            CollectionAssert.Contains(actual, expected);
        }

        [Test]
        public void Can_select_evaluated_conditional_or_expression()
        {
            // ReSharper disable ConvertToConstant.Local
            var a = 10;
            var b = 5;
            // ReSharper restore ConvertToConstant.Local

            var expected = new TestType()
            {
                IntColumn = 3,
                BoolColumn = true,
                StringColumn = "4"
            };

            EstablishContext(10, expected);

            var actual = ConnectionString.OpenDbConnection().Select<TestType>(q => q.IntColumn == 3 || a > b);

            Assert.IsNotNull(actual);
            Assert.AreEqual(11, actual.Count);
            CollectionAssert.Contains(actual, expected);
        }
    }
}