using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using ServiceStack.DataAnnotations;
using ServiceStack.Data;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;
using NUnit.Framework;

namespace ReturnAttributeTests
{
    public class User
    {
        [Return]
        [PrimaryKey]
        [AutoIncrement]
        [Sequence("Gen_User_Id")]
        public int Id { get; set; }

        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }

    public class User2
    {
        [Return]
        [PrimaryKey]
        [AutoIncrement]
        [Sequence("Gen_User_Id")]
        public int Id { get; set; }

        public string Name { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        [Return]
        [Sequence("Gen_Counter_Id")]
        public int Counter { get; set; }
    }

    public class TestsBase
    {
        private OrmLiteConnectionFactory dbFactory;

        protected string ConnectionString { get; set; }
        protected IOrmLiteDialectProvider DialectProvider { get; set; }
        protected OrmLiteConnectionFactory DbFactory => (dbFactory == null) ? dbFactory = new OrmLiteConnectionFactory(ConnectionString, DialectProvider) : dbFactory;

        public TestsBase()
        {
        }

        protected void Init()
        {
        }
    }

    public class ReturnAttributeTests: TestsBase
    {
        public ReturnAttributeTests(): base()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["testDb"].ConnectionString;
            DialectProvider = SqlServer2012Dialect.Provider;
        }

        [Test]
        public void TestIdOnInsert()
        {
            Init();
            using (var db = DbFactory.Open())
            {
                db.CreateTable<User>(true);

                var user = new User { Name = "me", Email = "me@mydomain.com" };
                user.UserName = user.Email;

                db.Insert(user);               
                Assert.That(user.Id, Is.GreaterThan(0), "normal Insert");
            }
        }

        [Test]
        public void TestIdOnSave()
        {
            Init();
            using (var db = DbFactory.Open())
            {
                db.CreateTable<User>(true);

                var user = new User { Name = "me", Email = "me@mydomain.com" };
                user.UserName = user.Email;

                db.Save(user);
                Assert.That(user.Id, Is.GreaterThan(0), "normal Insert");
            }
        }

        [Test]
        public void TestTwoSequencesOnInsert()
        {
            Init();
            using (var db = DbFactory.Open())
            {
                db.CreateTable<User2>(true);

                var user = new User2 { Name = "me", Email = "me@mydomain.com" };
                user.UserName = user.Email;

                db.Insert(user);
                Assert.That(user.Id, Is.GreaterThan(0), "normal Insert");
                Assert.That(user.Counter, Is.GreaterThan(0), "counter sequence ok");
            }
        }

        [Test]
        public void TestSqlOnInsert()
        {
            Init();
            using (var db = DbFactory.Open())
            {
                db.CreateTable<User>(true);

                var user = new User { Name = "me", Email = "me@mydomain.com" };
                user.UserName = user.Email;

                var id = db.Insert(user);
                var sql = db.GetLastSql();
                Assert.That(sql, Is.EqualTo("INSERT INTO \"User\" (\"Id\",\"Name\",\"UserName\",\"Email\") OUTPUT INSERTED.\"Id\" VALUES (NEXT VALUE FOR \"Gen_User_Id\",@Name,@UserName,@Email)"), "normal Insert");
            }
        }
    }
}
