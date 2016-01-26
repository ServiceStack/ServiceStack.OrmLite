using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.UseCase
{
    public class Parent
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        // the item currently published, modelled like an EF navigation property
        [Reference]
        public Child ActiveChild { get; set; }

        public int? ActiveChildId { get; set; }

        // all items mapped to this Parent
        [Reference]
        public List<Child> AllChildren { get; set; }
    }

    public class Child
    {
        [AutoIncrement]
        public int Id { get; set; }

        public int ParentId { get; set; }

        public string Description { get; set; }
    }

    public class ParentChildCyclicalExample : OrmLiteTestBase
    {
        [Test]
        public void Can_create_Parent_Child_Tables()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Parent>();
                db.DropAndCreateTable<Child>();

                var parent = new Parent
                {
                    Name = "Parent",
                    ActiveChild = new Child {  Description = "Active" },
                    AllChildren = new List<Child>
                    {
                        new Child { Description = "Child 1" },
                        new Child { Description = "Child 2" },
                    }
                };

                db.Save(parent, references:true);

                var dbParent = db.LoadSelect<Parent>()[0];
                dbParent.PrintDump();

                Assert.That(dbParent.ActiveChild, Is.Not.Null);
                Assert.That(dbParent.AllChildren.Count, Is.EqualTo(3));
            }
        }
    }
}