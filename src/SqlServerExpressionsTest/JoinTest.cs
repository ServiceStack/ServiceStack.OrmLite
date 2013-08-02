using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.OrmLite;
using System.IO;
using System.Data;
using ServiceStack.DataAnnotations;
using ServiceStack.Common.Utils;
using ServiceStack.OrmLite.SqlServer;

namespace SqlServerExpressionsTest
{
    public class User
    {
        public long Id { get; set; }

        [Index]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }

        public long? UserDataId { get; set; }

        public long UserServiceId { get; set; }

    }

    public class UserEx
    {
        [BelongTo(typeof(User))]
        public long Id { get; set; }
        [BelongTo(typeof(User))]
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        [BelongTo(typeof(UserData))]
        public string UserDataValue { get; set; }
        [BelongTo(typeof(UserService))]
        [Alias("ServiceName")]
        public string UserServiceName { get; set; }
    }


    [Alias("UserData")]
    public class UserData
    {
        [AutoIncrement]
        public long Id { get; set; }
        public string UserDataValue { get; set; }
    }

    [Alias("UserService")]
    public class UserService
    {
        [AutoIncrement]
        public long Id { get; set; }
        public string ServiceName { get; set; }
    }

    public class JoinTest
    {
        public static void Test(string connectionString)
        {
            //using (IDbConnection db = ":memory:".OpenDbConnection())
            using (IDbConnection db = connectionString.OpenDbConnection())
            {
                db.CreateTable<User>(true);
                db.CreateTable<UserData>(true);
                db.CreateTable<UserService>(true);

                db.Insert(new UserData { Id = 5, UserDataValue = "Value-5" });
                db.Insert(new UserData { Id = 6, UserDataValue = "Value-6" });

                db.Insert(new UserService { Id = 8, ServiceName = "Value-8" });
                db.Insert(new UserService { Id = 9, ServiceName = "Value-9" });

                db.Insert(new User { Id = 1, Name = "A", CreatedDate = DateTime.Now, UserDataId = 5, UserServiceId = 8 });
                db.Insert(new User { Id = 2, Name = "B", CreatedDate = DateTime.Now, UserDataId = 5, UserServiceId = 9 });
                db.Insert(new User { Id = 3, Name = "B", CreatedDate = DateTime.Now });


                var rowsB = db.Select<User>("Name = {0}", "B");
                var rowsB1 = db.Select<User>(user => user.Name == "B");

                var jn = new JoinSqlBuilder<UserEx, User>();
                jn = jn.Join<User, UserData>(x => x.UserDataId, x => x.Id, x => new { x.Name, x.Id }, x => new { x.UserDataValue })
                       .LeftJoin<User, UserService>(x => x.UserServiceId, x => x.Id, null, x => new { x.ServiceName })
                       .OrderByDescending<User>(x=>x.Name)
                       .OrderBy<User>(x=>x.Id)
                       .Select<User>(x=>x.Id)
                       .Where<User>(x=> x.Id == 0);

                var sql = jn.ToSql();
                var items = db.Query<UserEx>(sql);
                
                jn.Clear();
                jn = jn.Join<User, UserData>(x => x.UserDataId, x => x.Id)
                       .LeftJoin<User, UserService>(x => x.UserServiceId, x => x.Id)
                       .OrderByDescending<User>(x => x.Name)
                       .OrderBy<User>(x => x.Id)
                       .OrderByDescending<UserService>(x => x.ServiceName)
                       .Where<User>(x => x.Id > 0)
                       .Or<User>(x => x.Id < 10)
                       .And<User>(x => x.Name != "" || x.Name != null);

                var sql2 = jn.ToSql();
                var item = db.QuerySingle<UserEx>(sql2);

                jn.Clear();
                jn = new JoinSqlBuilder<UserEx, User>();
                jn = jn.Join<User, UserData>(x => x.UserDataId, x => x.Id)
                       .LeftJoin<User, UserService>(x => x.UserServiceId, x => x.Id)
                       .OrderByDescending<User>(x=>x.Name)
                       .OrderBy<User>(x=>x.Id)
                       .SelectAll<UserData>()
                       .Where<User>(x=> x.Id == 0);

                var sql3 = jn.ToSql();
                var items3 = db.Query<UserEx>(sql3);

                jn.Clear();
                jn = new JoinSqlBuilder<UserEx, User>();
                jn = jn.Join<User, UserData>(x => x.UserDataId, x => x.Id, x => new { x.Name, x.Id }, x => new { x.UserDataValue })
                       .LeftJoin<User, UserService>(x => x.UserServiceId, x => x.Id, null, x => new { x.ServiceName })
                       .OrderByDescending<User>(x=>x.Name)
                       .OrderBy<User>(x=>x.Id)
                       .SelectDistinct()
                       .SelectAll<UserData>()
                       .Where<User>(x=> x.Id == 0);

                var sql4 = jn.ToSql();
                var items4 = db.Query<UserEx>(sql4);

                jn.Clear();
                jn = new JoinSqlBuilder<UserEx, User>();
                jn = jn.Join<User, UserData>(x => x.UserDataId, x => x.Id)
                       .LeftJoin<User, UserService>(x => x.UserServiceId, x => x.Id)
                       .OrderByDescending<User>(x=>x.Name)
                       .OrderBy<User>(x=>x.Id)
                       .SelectCount<User>(x=>x.Id)
                       .Where<User>(x=> x.Id == 0);

                var sql5 = jn.ToSql();
                var items5 = db.GetScalar<long>(sql5);

                jn.Clear();
                jn = new JoinSqlBuilder<UserEx, User>();
                jn = jn.Join<User, UserData>(x => x.UserDataId, x => x.Id)
                       .LeftJoin<User, UserService>(x => x.UserServiceId, x => x.Id)
                       .OrderByDescending<User>(x => x.Name)
                       .OrderBy<User>(x=>x.Id)
                       .SelectMax<User>(x=>x.Id)
                       .Where<User>(x=> x.Id == 0);

                var sql6 = jn.ToSql();
                var items6 = db.GetScalar<long>(sql6);

                jn.Clear();
                jn = new JoinSqlBuilder<UserEx, User>();
                jn = jn.Join<User, UserData>(x => x.UserDataId, x => x.Id)
                       .LeftJoin<User, UserService>(x => x.UserServiceId, x => x.Id)
                       .OrderByDescending<User>(x => x.Name)
                       .OrderBy<User>(x=>x.Id)
                       .SelectMin<User>(x=>x.Id)
                       .Where<User>(x=> x.Id == 0);

                var sql7 = jn.ToSql();
                var items7 = db.GetScalar<long>(sql7);

                jn.Clear();
                jn = new JoinSqlBuilder<UserEx, User>();
                jn = jn.Join<User, UserData>(x => x.UserDataId, x => x.Id)
                       .LeftJoin<User, UserService>(x => x.UserServiceId, x => x.Id)
                       .OrderByDescending<User>(x => x.Name)
                       .OrderBy<User>(x=>x.Id)
                       .SelectAverage<User>(x=>x.Id)
                       .Where<User>(x=> x.Id == 0);

                var sql8 = jn.ToSql();
                var items8 = db.GetScalar<long>(sql8);

                jn.Clear();
                jn = new JoinSqlBuilder<UserEx, User>();
                jn = jn.Join<User, UserData>(x => x.UserDataId, x => x.Id)
                       .LeftJoin<User, UserService>(x => x.UserServiceId, x => x.Id)
                       .OrderByDescending<User>(x => x.Name)
                       .OrderBy<User>(x=>x.Id)
                       .SelectSum<User>(x=>x.Id)
                       .Where<User>(x=> x.Id == 0);

                var sql9 = jn.ToSql();
                var items9 = db.GetScalar<long>(sql9);

            }
        }
    }
}
