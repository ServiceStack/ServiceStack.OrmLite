using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.OrmLite;
using System.IO;
using System.Data;
using ServiceStack.DataAnnotations;
using ServiceStack.Common.Utils;
using ServiceStack.OrmLite.Sqlite;

namespace SqliteExpressionsTest
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
        private static string GetFileConnectionString()
        {
            var connectionString = "~/db.sqlite".MapAbsolutePath();
            if (File.Exists(connectionString))
                File.Delete(connectionString);

            return connectionString;
        }


        public static void Test()
        {
            OrmLiteConfig.DialectProvider = SqliteOrmLiteDialectProvider.Instance;

            var path = GetFileConnectionString();
            if (File.Exists(path))
                File.Delete(path);
            //using (IDbConnection db = ":memory:".OpenDbConnection())
            using (IDbConnection db = path.OpenDbConnection())
            using (IDbCommand dbCmd = db.CreateCommand())
            {
                dbCmd.CreateTable<User>(true);
                dbCmd.CreateTable<UserData>(true);
                dbCmd.CreateTable<UserService>(true);

                dbCmd.Insert(new UserData { Id = 5, UserDataValue = "Value-5" });
                dbCmd.Insert(new UserData { Id = 6, UserDataValue = "Value-6" });

                dbCmd.Insert(new UserService { Id = 8, ServiceName = "Value-8" });
                dbCmd.Insert(new UserService { Id = 9, ServiceName = "Value-9" });

                dbCmd.Insert(new User { Id = 1, Name = "A", CreatedDate = DateTime.Now, UserDataId = 5, UserServiceId = 8 });
                dbCmd.Insert(new User { Id = 2, Name = "B", CreatedDate = DateTime.Now, UserDataId = 5, UserServiceId = 9 });
                dbCmd.Insert(new User { Id = 3, Name = "B", CreatedDate = DateTime.Now });


                var rowsB = dbCmd.Select<User>("Name = {0}", "B");
                var rowsB1 = dbCmd.Select<User>(user => user.Name == "B");

                var jn = new JoinSqlBuilder<UserEx, User>();
                jn = jn.InnerJoin<User, UserData>(x => x.UserDataId, x => x.Id, x => new { x.Name, x.Id }, x => new { x.UserDataValue })
                       .LeftOuterJoin<User, UserService>(x => x.UserServiceId, x => x.Id, null, x => new { x.ServiceName })
                       .OrderByDescending<User>(x=>x.Name)
                       .OrderBy<User>(x=>x.Id)
                       .Select<User>(x=>x.Id)
                       .Where<User>(x=> x.Id == 0);

                var sql = jn.ToSql();
                var items = db.Query<UserEx>(sql);
                
                jn.Clear();
                jn = jn.InnerJoin<User, UserData>(x => x.UserDataId, x => x.Id)
                       .LeftOuterJoin<User, UserService>(x => x.UserServiceId, x => x.Id)
                       .OrderByDescending<User>(x => x.Name)
                       .OrderBy<User>(x => x.Id)
                       .OrderByDescending<UserService>(x => x.ServiceName)
                       .Where<User>(x => x.Id > 0)
                       .Or<User>(x => x.Id < 10)
                       .And<User>(x => x.Name != "" || x.Name != null);

                var sql2 = jn.ToSql();
                var item = db.QuerySingle<UserEx>(sql2);

            }

            File.Delete(path);
        }
    }
}
