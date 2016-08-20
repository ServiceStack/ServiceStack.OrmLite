using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Model;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests.Async.Issues
{
    //[Schema("dbo")]
    [Alias("Project")]
    public class Project : IHasId<int>
    {
        [Alias("ProjectId")]
        [Index(Unique = true)]
        [AutoIncrement]
        public int Id { get; set; }

        [Required]
        [References(typeof(Department))]
        public int DepartmentId { get; set; }
        [Reference]
        public Department Department { get; set; }

        [Required]
        public string ProjectName { get; set; }
        [Required]
        public bool IsArchived { get; set; }
        [Required]
        public DateTime CreatedOn { get; set; }
    }

    public class Department
    {
        [Alias("DepartmentId")]
        [Index(Unique = true)]
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [Alias("ProjectTask")]
    public class ProjectTask : IHasId<int>
    {
        [Alias("ProjectTaskId")]
        [Index(Unique = true)]
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Project))]
        public int ProjectId { get; set; }
        [Reference]
        public Project Project { get; set; }

        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime CreatedOn { get; set; }
        public DateTime? FinishedOn { get; set; }
        [Required]
        public int EstimatedHours { get; set; }

        //[References(typeof(Employee))]
        //public int? AssignedToId { get; set; }
        //[Reference]
        //public Employee AssignedTo { get; set; }

        //[References(typeof(Employee))]
        //public int? RequestedById { get; set; }
        //[Reference]
        //public Employee RequestedBy { get; set; }

        [References(typeof(ProjectTaskStatus))]
        public int? ProjectTaskStatusId { get; set; }
        [Reference]
        public ProjectTaskStatus ProjectTaskStatus { get; set; }

        [Required]
        public int Priority { get; set; }
        [Required]
        public int Order { get; set; }
    }

    //[Schema("dbo")]
    [Alias("ProjectTaskStatus")]
    public class ProjectTaskStatus : IHasId<int>
    {
        [Alias("ProjectTaskStatusId")]
        [Index(Unique = true)]
        [AutoIncrement]
        public int Id { get; set; }
        [Required]
        public string Description { get; set; }
    }
    
    [TestFixture]
    public class LoadSelectAmbiguousColumnIssue : OrmLiteTestBase
    {
        public class DeptEmployee //Ref of External Table
        {
            [PrimaryKey]
            public int Id { get; set; }
        }


        [Test]
        public async Task Can_select_columns_with_LoadSelectAsync()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<DeptEmployee>();

                db.DropTable<ProjectTask>();
                db.DropTable<Project>();
                db.DropTable<ProjectTaskStatus>();
                db.DropTable<Department>();

                db.CreateTable<Department>();
                db.CreateTable<ProjectTaskStatus>();
                db.CreateTable<Project>();
                db.CreateTable<ProjectTask>();

                int departmentId = 1;
                int statusId = 1;

                var q = db.From<ProjectTask>()
                          .Join<ProjectTask, Project>((pt, p) => pt.ProjectId == p.Id)
                          .Where<Project>(p => p.DepartmentId == departmentId || departmentId == 0)
                          .And<ProjectTask>(pt => pt.ProjectTaskStatusId == statusId || statusId == 0);

                var tasks = await db.LoadSelectAsync(q);

                tasks.PrintDump();
            }
        }
    }
}