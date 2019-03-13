﻿using System;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Logging;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class CheckConstraintTest
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Required]
        [CheckConstraint("Age > 1")]
        public int Age { get; set; }

        [CheckConstraint("Name IS NOT NULL")]
        public string Name { get; set; }
    }

    [TestFixtureSource(typeof(ProvidersFixtureData), ProvidersFixtureData.SupportedAll)]
    [NonParallelizable]
    public class CheckConstraintTests : OrmLiteProvidersTestBase
    {
        public CheckConstraintTests(Dialect dialect) : base(dialect)
        {
        }
        
        [Test]
        public void Does_create_table_with_CheckConstraints()
        {
            if (Dialect == Dialect.MySql5_5 || Dialect == Dialect.MySql10_1) {
                //parsed but not supported http://stackoverflow.com/a/2115641/85785
                Assert.Ignore("Check constraints supported from MariaDb 10.2.1 onwards"); 
            }
            
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<CheckConstraintTest>();
                
                db.GetLastSql().Print();

                try
                {
                    db.Insert(new CheckConstraintTest { Age = 1 });
                    Assert.Fail("Should fail");
                }
                catch (Exception ex)
                {
                    Assert.That(ex.Message.ToLower(), Does.Contain("age"));
                    Assert.That(ex.Message.ToLower(), Does.Contain("constraint"));
                }

                try
                {
                    db.Insert(new CheckConstraintTest { Age = 2 });
                    Assert.Fail("Should fail");
                }
                catch (Exception ex)
                {
                    ex.Message.Print();
                    Assert.That(ex.Message.ToLower(), Does.Contain("name"));
                    Assert.That(ex.Message.ToLower(), Does.Contain("constraint"));
                }
            }
        }

        [Test]
        public void Can_insert_record_passing_check_constraints()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<CheckConstraintTest>();

                db.Insert(new CheckConstraintTest { Age = 2, Name = "foo" });
            }
        }
    }
}