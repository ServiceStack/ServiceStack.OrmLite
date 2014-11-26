using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Issues
{
    public class Goal
    {
        [PrimaryKey]
        public long Id { get; set; }

        [Reference]
        public List<Item> Items { get; set; }
    }

    public class Item
    {
        [PrimaryKey]
        public long Id { get; set; }

        [ForeignKey(typeof(Goal))]
        public long AnyGoalId { get; set; }

        [Alias("CorrectGoalId")]
        [ForeignKey(typeof(Goal))]
        public long GoalId { get; set; }
    }

    [TestFixture]
    public class MultiFieldReferenceTests
        : OrmLiteTestBase
    {
        [Test]
        public void Does_select_correct_reference_field()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<Item>();
                db.DropTable<Goal>();
                db.CreateTable<Goal>();
                db.CreateTable<Item>();

                var goal = new Goal { Id = 1 };
                db.Save(goal, references: true);

                var goalWithItems = new Goal
                {
                    Id = 2,
                    Items = new List<Item>
                    {
                        new Item { Id = 10, AnyGoalId = 1 },
                        new Item { Id = 11, AnyGoalId = 1 },
                    }
                };

                db.Save(goalWithItems, references: true);

                Assert.That(goalWithItems.Items[0].GoalId, Is.EqualTo(goalWithItems.Id));
                Assert.That(goalWithItems.Items[1].GoalId, Is.EqualTo(goalWithItems.Id));

                var dbGoals = db.LoadSelect<Goal>(x => x.Id == goalWithItems.Id).First();
                db.GetLastSql().Print();

                Assert.That(dbGoals.Items[0].GoalId, Is.EqualTo(goalWithItems.Id));
                Assert.That(dbGoals.Items[1].GoalId, Is.EqualTo(goalWithItems.Id));
            }
        }
    }
}