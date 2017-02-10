using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace ServiceStack.OrmLite.Tests.UseCase
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }

    public class ModelWithSoftDelete : ISoftDelete
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
    }

    public static class SqlExpressionExtensions
    {
        public static SqlExpression<T> OnlyActive<T>(this SqlExpression<T> q)
            where T : ISoftDelete
        {
            return q.Where(x => x.IsDeleted != true);
        }
    }

    public class SoftDeleteUseCase : OrmLiteTestBase
    {
        [Test]
        public void Can_add_generic_soft_delete_filter_to_SqlExpression()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<ModelWithSoftDelete>();

                db.Insert(new ModelWithSoftDelete { Name = "foo" });
                db.Insert(new ModelWithSoftDelete { Name = "bar", IsDeleted = true });

                var results = db.Select(db.From<ModelWithSoftDelete>().OnlyActive());

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Name, Is.EqualTo("foo"));

                var result = db.Single(db.From<ModelWithSoftDelete>().Where(x => x.Name == "foo").OnlyActive());
                Assert.That(result.Name, Is.EqualTo("foo"));
                result = db.Single(db.From<ModelWithSoftDelete>().Where(x => x.Name == "bar").OnlyActive());
                Assert.That(result, Is.Null);
            }
        }
    }
}