using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Tests.UseCase
{
    [TestFixture]
    public class AliasedFieldUseCase
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup() {
            OrmLiteConfig.DialectProvider = SqliteDialect.Provider;
        }

        public class Foo
        {
            [Alias("SOME_COLUMN_NAME")]
            public string Bar { get; set; }
        }

        [Test]
        public void CanResolveAliasedFieldNameInAnonymousType()
        {
            using (IDbConnection db = ":memory:".OpenDbConnection())
            {
                db.CreateTable<Foo>(false);

                db.Insert(new Foo { Bar = "some_value" });
                db.Insert(new Foo { Bar = "a totally different value" });
                db.Insert(new Foo { Bar = "whatever" });

                // the original classes property name is used to create the anonymous type
                List<Foo> foos = db.Where<Foo>(new { Bar = "some_value" });

                Assert.That(foos, Has.Count.EqualTo(1));

                // the aliased column name is used to create the anonymous type
                foos = db.Where<Foo>(new { SOME_COLUMN_NAME = "some_value" });

                Assert.That(foos, Has.Count.EqualTo(1));
            }
        }

    }
}
