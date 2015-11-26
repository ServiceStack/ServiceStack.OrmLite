using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ServiceStack.OrmLite.Dapper;

namespace ServiceStack.OrmLite.Tests.UseCase
{
    class Post
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastChangeDate { get; set; }
        public int? Counter1 { get; set; }
        public int? Counter2 { get; set; }
        public int? Counter3 { get; set; }
        public int? Counter4 { get; set; }
        public int? Counter5 { get; set; }
        public int? Counter6 { get; set; }
        public int? Counter7 { get; set; }
        public int? Counter8 { get; set; }
        public int? Counter9 { get; set; }

    }

    [Ignore, Explicit("Integration Test")]
    public class PostPerfTests : OrmLiteTestBase
    {
        public PostPerfTests()
        {
            Dialect = Dialect.SqlServer2012;
        }

        private void EnsureDBSetup()
        {
            using (var cnn = OpenDbConnection().ToDbConnection())
            {
                var cmd = cnn.CreateCommand();
                cmd.CommandText = @"
if (OBJECT_ID('Post') is null)
begin
	create table Post
	(
		Id int identity primary key, 
		[Text] varchar(max) not null, 
		CreationDate datetime not null, 
		LastChangeDate datetime not null,
		Counter1 int,
		Counter2 int,
		Counter3 int,
		Counter4 int,
		Counter5 int,
		Counter6 int,
		Counter7 int,
		Counter8 int,
		Counter9 int
	)
	   
	set nocount on 

	declare @i int
	declare @c int

	declare @id int

	set @i = 0

	while @i < 5000
	begin 
		
		insert Post ([Text],CreationDate, LastChangeDate) values (replicate('x', 2000), GETDATE(), GETDATE())
		set @id = @@IDENTITY
		
		set @i = @i + 1
	end
end
";
                cmd.Connection = cnn;
                cmd.ExecuteNonQuery();
            }
        }

        class Test
        {
            public static Test Create(Action<int> iteration, string name)
            {
                return new Test { Iteration = iteration, Name = name };
            }

            public Action<int> Iteration { get; set; }
            public string Name { get; set; }
            public Stopwatch Watch { get; set; }
        }

        class Tester : List<Test>
        {
            public void Add(Action<int> iteration, string name)
            {
                Add(Test.Create(iteration, name));
            }

            public void Run(int iterations)
            {
                // warmup 
                foreach (var test in this)
                {
                    test.Iteration(iterations + 1);
                    test.Watch = new Stopwatch();
                    test.Watch.Reset();
                }

                var rand = new Random();
                for (int i = 1; i <= iterations; i++)
                {
                    foreach (var test in this.OrderBy(ignore => rand.Next()))
                    {
                        test.Watch.Start();
                        test.Iteration(i);
                        test.Watch.Stop();
                    }
                }

                foreach (var test in this.OrderBy(t => t.Watch.ElapsedMilliseconds))
                {
                    Console.WriteLine(test.Name + " took " + test.Watch.ElapsedMilliseconds + "ms");
                }
            }
        }

        [Test]
        public void Run_single_select_Dapper()
        {
            var tester = new Tester();

            var db = OpenDbConnection();
            tester.Add(id => db.Query<Post>("select * from Post where Id = @Id", new { Id = id }).ToList(), "Mapper Query");

            tester.Run(500);
        }

        [Test]
        public void Run_single_select_OrmLite()
        {
            var tester = new Tester();

            var db = OpenDbConnection();
            tester.Add(id => db.SelectFmt<Post>("select * from Post where Id = {0}", id), "OrmLite Query");

            tester.Run(500);
        }

        [Test]
        public void Run_multi_select_Dapper()
        {
            var tester = new Tester();

            var db = OpenDbConnection();
            tester.Add(id => db.Query<Post>("select top 1000 * from Post").ToList(), "Mapper Query");

            tester.Run(50);
        }

        [Test]
        public void Run_multi_select_OrmLite()
        {
            var tester = new Tester();

            var db = OpenDbConnection();
            tester.Add(id => db.SelectFmt<Post>("select top 1000 * from Post"), "OrmLite Query");

            tester.Run(50);
        }

        [Test]
        public void Run_multi_select_OrmLite_SqlExpression()
        {
            var tester = new Tester();

            var db = OpenDbConnection();
            tester.Add(id => db.Select<Post>(q => q.Limit(1000)), "OrmLite Query Expression");

            tester.Run(50);
        }
    }
}