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
    public class User_2
    {
        public long Id { get; set; }

        [Index]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }

        public long? UserDataId { get; set; }

        public long UserServiceId { get; set; }

        [Ignore]
        [Alias("DataValue")]
        public string UserDataValue { get; set; }

        [Ignore]
        [Alias("ServiceName")]
        public string UserServiceName { get; set; }
    }

    [Alias("UserData_2")]
    public class UserData_2
    {
        public long Id { get; set; }
        public string DataValue { get; set; }
    }

    [Alias("UserService_2")]
    public class UserService_2
    {
        public long Id { get; set; }
        public string ServiceName { get; set; }
    }


    public class IgnoredFieldSelectTest
    {
        public static void Test(string connectionString)
        {
            //using (IDbConnection db = ":memory:".OpenDbConnection())
            using (IDbConnection db = connectionString.OpenDbConnection())
            {
                db.CreateTable<User_2>(true);
                db.CreateTable<UserData_2>(true);
                db.CreateTable<UserService_2>(true);

                //Insert Test
                db.Insert(new UserData_2 { Id = 5, DataValue = "Value-5" });
                db.Insert(new UserData_2 { Id = 6, DataValue = "Value-6" });

                db.Insert(new UserService_2 { Id = 8, ServiceName = "Value-8" });
                db.Insert(new UserService_2 { Id = 9, ServiceName = "Value-9" });

                var user2 = new User_2 { Id = 1, Name = "A", CreatedDate = DateTime.Now, UserDataId = 5, UserServiceId = 8 };
                db.Insert(user2);
                db.Insert(new User_2 { Id = 2, Name = "B", CreatedDate = DateTime.Now, UserDataId = 5, UserServiceId = 9 });
                db.Insert(new User_2 { Id = 3, Name = "B", CreatedDate = DateTime.Now });
                
                //Update Test
                user2.CreatedDate = DateTime.Now;
                db.Update<User_2>(user2,x=>x.Id == 1);

                //Select Test

                var rowsB = db.Select<User_2>("Name = {0}", "B");
                var rowsB1 = db.Select<User_2>(user => user.Name == "B");

                var rowsUData = db.Select<UserData_2>();
                var rowsUServ = db.Select<UserService_2>();

                var jn2 = new JoinSqlBuilder<User_2, User_2>();
                jn2 = jn2.Join<User_2, UserData_2>(x => x.UserDataId, x => x.Id, x => new { x.Name, x.Id }, x => new { x.DataValue})
                       .Join<User_2, UserService_2>(x => x.UserServiceId, x => x.Id, null, x => new { x.ServiceName })
                       .OrderByDescending<User_2>(x => x.Name)
                       .OrderBy<User_2>(x => x.Id)
                       .Select<User_2>(x => x.Id);

                var sql2 = jn2.ToSql();
                var items2 = db.Query<User_2>(sql2);
                Console.WriteLine("Ignored Field Selected Items - {0}",items2.Count());

                var item = db.FirstOrDefault<User_2>(sql2);
                
            }
        }
    }
}
