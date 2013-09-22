using System;
using System.Linq;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

using ServiceStack.Utils;
using ServiceStack.DataAnnotations;
using ServiceStack.Common;
using ServiceStack.Model;

using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Firebird;
using Database.Records;

namespace Database.Records
{

    public partial class Company
    {

        [Ignore]
        public string UpperName
        {
            get { return Name.ToUpper(); }
        }
    }
}

namespace TestLiteFirebird2
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //Set one before use (i.e. in a static constructor).
            OrmLiteConfig.DialectProvider = new FirebirdOrmLiteDialectProvider();

            using (IDbConnection db =
			       "User=SYSDBA;Password=masterkey;Database=employee.fdb;DataSource=localhost;Dialect=3;charset=ISO8859_1;".OpenDbConnection())
            {
                try
                {

                    Console.WriteLine(db.HasChildren<Company>(new Company() { Id = 1000 }));

                    Console.WriteLine(db.HasChildren<Company>(new Company() { Id = 5 }));

                    Console.WriteLine(db.Exists<Company>(Company.Me.Id + "={0}", 5));

                    Console.WriteLine(db.Exists<Company>(Company.Me.Id + "={0}", 1000));


                    var rows = db.Select<Company>();
                    Console.WriteLine("Company: rows before insert :{0}", rows.Count);

                    Company cp = new Company {
                        Name = "One More Company",
                        Employees = 10,
                        Started = DateTime.Today,
                        Turnover = 12525,
                        CreatedDate = DateTime.Now
                    };


                    db.Insert(cp);

                    rows = db.Select<Company>();
                    Console.WriteLine("Company: rows after  insert :{0}", rows.Count);
                    foreach (Company u in rows)
                    {
                        Console.WriteLine("{0} -- {1}", u.Id, u.Name);
                    }
                    Console.WriteLine("----------------------");

                    rows = db.Select<Company>(Company.Me.Id + ">={0} order by " + Company.Me.Id + " descending rows 5",
                                                  10);
                    Console.WriteLine(rows.Count);
                    foreach (Company u in rows)
                    {
                        Console.WriteLine("{0} -- {1} -- {2} -- {3} -- {4} -- {5} --{6}", u.Id, u.Name,
                                          (u.Employees.HasValue) ? u.Employees.Value.ToString() : "",
                                           u.Started.HasValue ? u.Started.Value.ToString() : "",
                                          u.Turnover.HasValue ? u.Turnover.Value.ToString() : "",
                                          u.CreatedDate.HasValue ? u.CreatedDate.Value.ToString() : "",
                                          u.UpperName
                                          );
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

    }

    //s.Substring(7, s.IndexOf("FROM")-8)  --> columns
    //s.Substring( s.IndexOf("FROM")+5)
}
