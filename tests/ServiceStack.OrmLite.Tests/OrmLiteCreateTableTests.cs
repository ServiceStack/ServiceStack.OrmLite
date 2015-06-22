using System;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Common.Tests.Models;
using ServiceStack.OrmLite.Tests.Shared;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    using ServiceStack.DataAnnotations;

    [TestFixture]
    public class OrmLiteCreateTableTests
        : OrmLiteTestBase
    {
        [Test]
        public void Does_table_Exists()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<ModelWithIdOnly>();

                Assert.That(
                    db.TableExists(typeof(ModelWithIdOnly).Name.SqlTableRaw()),
                    Is.False);

                db.CreateTable<ModelWithIdOnly>(true);

                Assert.That(
                    db.TableExists(typeof(ModelWithIdOnly).Name.SqlTableRaw()),
                    Is.True);
            }
        }

        [Test]
        public void Can_create_ModelWithIdOnly_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithIdOnly>(true);
            }
        }

        [Test]
        public void Can_create_ModelWithOnlyStringFields_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithOnlyStringFields>(true);
            }
        }

        [Test]
        public void Can_create_ModelWithLongIdAndStringFields_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithLongIdAndStringFields>(true);
            }
        }

        [Test]
        public void Can_create_ModelWithFieldsOfDifferentTypes_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithFieldsOfDifferentTypes>(true);
            }
        }

        [Test]
        public void Can_preserve_ModelWithIdOnly_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithIdOnly>(true);

                db.Insert(new ModelWithIdOnly(1));
                db.Insert(new ModelWithIdOnly(2));

                db.CreateTable<ModelWithIdOnly>(false);

                var rows = db.Select<ModelWithIdOnly>();

                Assert.That(rows, Has.Count.EqualTo(2));
            }
        }

        [Test]
        public void Can_preserve_ModelWithIdAndName_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithIdAndName>(true);

                db.Insert(new ModelWithIdAndName(1));
                db.Insert(new ModelWithIdAndName(2));

                db.CreateTable<ModelWithIdAndName>(false);

                var rows = db.Select<ModelWithIdAndName>();

                Assert.That(rows, Has.Count.EqualTo(2));
            }
        }

        [Test]
        public void Can_overwrite_ModelWithIdOnly_table()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTable<ModelWithIdOnly>(true);

                db.Insert(new ModelWithIdOnly(1));
                db.Insert(new ModelWithIdOnly(2));

                db.CreateTable<ModelWithIdOnly>(true);

                var rows = db.Select<ModelWithIdOnly>();

                Assert.That(rows, Has.Count.EqualTo(0));
            }
        }

        [Test]
        public void Can_create_multiple_tables()
        {
            using (var db = OpenDbConnection())
            {
                db.CreateTables(true, typeof(ModelWithIdOnly), typeof(ModelWithIdAndName));

                db.Insert(new ModelWithIdOnly(1));
                db.Insert(new ModelWithIdOnly(2));

                db.Insert(new ModelWithIdAndName(1));
                db.Insert(new ModelWithIdAndName(2));

                var rows1 = db.Select<ModelWithIdOnly>();
                var rows2 = db.Select<ModelWithIdOnly>();

                Assert.That(rows1, Has.Count.EqualTo(2));
                Assert.That(rows2, Has.Count.EqualTo(2));
            }
        }

        [Test]
        public void Can_create_ModelWithIdAndName_table_with_specified_DefaultStringLength()
        {
            OrmLiteConfig.DialectProvider.DefaultStringLength = 255;
            var createTableSql = OrmLiteConfig.DialectProvider.ToCreateTableStatement(typeof(ModelWithIdAndName));

            Console.WriteLine("createTableSql: " + createTableSql);
            if (Dialect != Dialect.PostgreSql)
            {
                Assert.That(createTableSql, Is.StringContaining("VARCHAR(255)").Or.StringContaining("VARCHAR2(255)"));
            }
            else
            {
                Assert.That(createTableSql, Is.StringContaining("text"));
            }
        }

        public class ModelWithGuid
        {
            public long Id { get; set; }
            public Guid Guid { get; set; }
        }

        [Test]
        public void Can_handle_table_with_Guid()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithGuid>();

                db.GetLastSql().Print();

                var models = new[] {
                    new ModelWithGuid { Id = 1, Guid = Guid.NewGuid() }, 
                    new ModelWithGuid { Id = 2, Guid = Guid.NewGuid() }
                };

                db.SaveAll(models);

                var newModel = db.SingleById<ModelWithGuid>(models[0].Id);

                Assert.That(newModel.Guid, Is.EqualTo(models[0].Guid));

                newModel = db.Single<ModelWithGuid>(q => q.Guid == models[0].Guid);

                Assert.That(newModel.Guid, Is.EqualTo(models[0].Guid));

                var newGuid = Guid.NewGuid();
                db.Update(new ModelWithGuid {Id = models[0].Id, Guid = newGuid});
                db.GetLastSql().Print();
                newModel = db.Single<ModelWithGuid>(q => q.Id == models[0].Id);
                Assert.That(newModel.Guid, Is.EqualTo(newGuid));
            }
        }

        public class ModelWithOddIds
        {
            [Index(false)]
            public long Id { get; set; }

            [PrimaryKey]
            public Guid Guid { get; set; }
        }

        [Test]
        public void Can_handle_table_with_non_conventional_id()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithOddIds>();

                db.GetLastSql().Print();

                var guid1 = Guid.NewGuid();
                db.Insert(new ModelWithOddIds { Id = 1, Guid = guid1 });
                db.Insert(new ModelWithOddIds { Id = 1, Guid = Guid.NewGuid() });

                var rows = db.Select<ModelWithOddIds>(q => q.Id == 1);

                Assert.That(rows.Count, Is.EqualTo(2));

                rows = db.Select<ModelWithOddIds>(q => q.Guid == guid1);
                Assert.That(rows, Has.Count.EqualTo(1));
            }
        }

        [Alias("Model Alias Space")]
        public class ModelAliasWithSpace
        {
            public int Id { get; set; }

            [Alias("The Name")]
            public string Field { get; set; }
        }

        [Test]
        public void Can_create_table_containing_Alias_with_space()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelAliasWithSpace>();

                db.GetLastSql().Print();

                db.Insert(new ModelAliasWithSpace { Id = 1, Field = "The Value" });

                var row = db.Single<ModelAliasWithSpace>(q => q.Field == "The Value");

                Assert.That(row.Field, Is.EqualTo("The Value"));
            }
        }

        [Test]
        public void Can_create_table_with_all_number_types()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithNumerics>();
                db.GetLastSql().Print();

                var defaultValues = new ModelWithNumerics {
                    Id = 1, Byte = 0, Short = 0, UShort = 0, 
                    Int = 0, UInt = 0, Long = 0, ULong = 0, 
                    Float = 0, Double = 0, Decimal = 0,
                };
                db.Insert(defaultValues);

                var fromDb = db.SingleById<ModelWithNumerics>(defaultValues.Id);
                Assert.That(ModelWithNumerics.ModelWithNumericsComparer.Equals(fromDb, defaultValues));
            }
        }

        public class ModelWithIndexer
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public ModelWithIndexer()
            {
                Attributes = new Dictionary<string, object>();
            }

            public Object this[string attributeName]
            {
                get
                {
                    return Attributes[attributeName];
                }
                set
                {
                    Attributes[attributeName] = value;
                }
            }

            Dictionary<string, object> Attributes { get; set; } 
        }

        [Test]
        public void Can_create_table_ModelWithIndexer()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithIndexer>();

                db.Insert(new ModelWithIndexer { Id = 1, Name = "foo" });

                var row = db.SingleById<ModelWithIndexer>(1);

                Assert.That(row.Name, Is.EqualTo("foo"));
            }
        }

        public interface IBaseEntity
        {
            long Id { get; set; }
            DateTime Created { get; set; }
            DateTime Updated { get; set; }
            DateTime? Deleted { get; set; }
            bool IsDeleted { get; set; }
        }

        public class BaseEntity : IBaseEntity
        {
            [AutoIncrement]
            [PrimaryKey]
            public long Id { get; set; }

            public DateTime Created { get; set; }

            public DateTime Updated { get; set; }

            public DateTime? Deleted { get; set; }

            public bool IsDeleted { get; set; }
        }

        public class UserEntity : BaseEntity
        {
        }

        public class AnswerEntity : BaseEntity
        {
            public long UserId { get; set; }
        }

        [Test]
        public void Can_create_and_join_on_Tables_with_Base_classes()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<UserEntity>();
                db.DropAndCreateTable<AnswerEntity>();

                var userId = db.Insert(new UserEntity 
                { 
                    Id = 1, 
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                }, 
                selectIdentity: true);

                db.Insert(new AnswerEntity
                {
                    UserId = userId, 
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow,
                });

                var q = db.From<AnswerEntity>();
                q.Join<AnswerEntity, UserEntity>((l, r) => l.UserId == r.Id);
                q.Where<AnswerEntity>(x => x.IsDeleted == false);

                var results = db.Select(q);
                results.PrintDump();

                Assert.That(results.Count, Is.EqualTo(1));
            }
        }
    }
}