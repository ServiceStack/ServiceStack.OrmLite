using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Oracle;

using Database.Records;

namespace TestLiteOracle04
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.WriteLine ("Hello World!");

            OrmLiteConfig.DialectProvider = new OracleOrmLiteDialectProvider();
			
			ServiceStack.OrmLite.SqlExpression<Company> sql=
                new OracleSqlExpression<Company>();
			
			List<Object> names = new List<Object>();
			names.Add("SOME COMPANY");
			names.Add("XYZ");
			

			List<Object> ids = new List<Object>();
			ids.Add(1);
			ids.Add(2);
			
			
			using (IDbConnection db =
                   "Data Source=x;User Id=x;Password=x;".OpenDbConnection())
            {
                db.DropTable<Company>();
                db.CreateTable<Company>();

                Company company = new Company() { Id = 1, Name = "XYZ" };
                Console.WriteLine(company.Id.In(ids));
                Console.WriteLine(company.Name.In(names));

                db.Insert<Company>(company);

                sql.Where(cp => cp.Name == "On more Company");
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => cp.Name != "On more Company");
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);


                sql.Where(cp => cp.Name == null);
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => cp.Name != null);
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);


                sql.Where(cp => cp.SomeBoolean);   // TODO : fix 
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => !cp.SomeBoolean && 1 == 1); //TODO : fix
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => cp.SomeBoolean && 1 == 1); //TODO : fix
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => 1 == 1);  // TODO : fix ?
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => "1" == "1"); // TODO : fix  ?
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => "1" == "0"); // TODO : fix  ?
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => 1 != 1);    //ok 
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => cp.SomeBoolean == true); //OK    
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => cp.SomeBoolean == false);   //OK
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => !cp.SomeBoolean);    // OK
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (cp.Name == cp.Name));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (cp.Name == "On more Company" || cp.Id > 30));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (cp.CreatedDate == DateTime.Today));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (cp.CreatedDate == DateTime.Today && (cp.Name == "On more Company" || cp.Id > 30)));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (cp.Name.ToUpper() == "ONE MORE COMPANY"));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (cp.Name.ToLower() == "ONE MORE COMPANY".ToLower()));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (cp.Name.ToLower().StartsWith("one")));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (cp.Name.ToUpper().EndsWith("COMPANY")));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (cp.Name.ToUpper().Contains("MORE")));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (cp.Name.Substring(0) == "ONE MORE COMPANY"));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (cp.Name.ToUpper().Substring(0, 7) == "ONE MOR"));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);


                sql.Where(cp => (cp.CreatedDate >= new DateTime(2000, 1, 1)));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (cp.Employees / 2 > 10.0));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (cp.Employees * 2 > 10.0 / 5));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => ((cp.Employees + 3) > (10.0 + 5)));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => ((cp.Employees - 3) > (10.0 + 5)));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => ((cp.Employees % 3) > (10.0 + 5)));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);


                sql.Where(cp => (Math.Round(cp.SomeDouble) > (10.0 + 5)));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (Math.Round(cp.SomeDouble, 3) > (10.0 + 5)));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (Math.Floor(cp.SomeDouble) > (10.0 + 5)));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (Math.Ceiling(cp.SomeDouble) > (10.0 + 5)));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);


                sql.Where(cp => (string.Concat(cp.SomeDouble, "XYZ") == "SOME COMPANY XYZ"));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (string.Concat(cp.SomeDouble, "X", "Y", "Z") == "SOME COMPANY XYZ"));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (string.Concat(cp.Name, "X", "Y", "Z") == "SOME COMPANY XYZ"));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (string.Concat(cp.SomeDouble.ToString(), "X", "Y", "Z") == "SOME COMPANY XYZ"));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => ((cp.CreatedDate ?? DateTime.Today) == DateTime.Today));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => ((cp.Turnover ?? 0) > 15));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (Math.Abs(cp.Turnover ?? 0) > 15));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (Sql.In(cp.Name, names)));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (Sql.In(cp.Id, ids)));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);


                sql.OrderBy(cp => cp.Name);
                Console.WriteLine("{0}", sql.OrderByExpression);
                db.Select<Company>(sql);

                sql.OrderBy(cp => new { cp.Name, cp.Id });
                Console.WriteLine("{0}", sql.OrderByExpression);
                db.Select<Company>(sql);

                sql.OrderBy(cp => new { cp.Name, Id = cp.Id * -1 });
                Console.WriteLine("{0}", sql.OrderByExpression);
                db.Select<Company>(sql);

                sql.OrderByDescending(cp => cp.Name);
                Console.WriteLine("{0}", sql.OrderByExpression);
                db.Select<Company>(sql);

                sql.OrderBy(cp => new { cp.Name, X = cp.Id.Desc() });
                Console.WriteLine("{0}", sql.OrderByExpression);
                db.Select<Company>(sql);

                sql.Limit(1, 5);
                Console.WriteLine(sql.LimitExpression);
                db.Select<Company>(sql);

                sql.Limit(1);
                Console.WriteLine(sql.LimitExpression);
                db.Select<Company>(sql);

                sql.Where(cp => (string.Concat(cp.Name, "_", cp.Employees) == "SOME COMPANY XYZ_2"));
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);

                sql.Where(cp => cp.Id != 1);
                Console.WriteLine(sql.WhereExpression);
                db.Select<Company>(sql);


                sql.Select(cp => new { cp.Employees, cp.Name });
                Console.WriteLine("To Select:'{0}' ", sql.SelectExpression);
                db.Select<Company>(sql);

                sql.Select(cp => new { cp.Employees, cp.Name, Some = (cp.Id * 4).As("SomeExpression") });
                Console.WriteLine("To Select:'{0}' ", sql.SelectExpression);
                db.Select<Company>(sql);

                sql.Select(cp => new { cp.Employees, cp.Name, Some = cp.Turnover.Sum().As("SomeExpression") });
                Console.WriteLine("To Select:'{0}' ", sql.SelectExpression);
                db.Select<Company>(sql);

                sql.Select(cp => new { cp.Employees, cp.Name, Some = DbMethods.Sum(cp.Turnover ?? 0).As("SomeExpression") });
                Console.WriteLine("To Select:'{0}' ", sql.SelectExpression);
                db.Select<Company>(sql);


                sql.Update(cp => new { cp.Employees, cp.Name });
                Console.WriteLine("To Update:'{0}' ", string.Join(",", sql.UpdateFields.ToArray()));
                db.Select<Company>(sql);

                sql.Insert(cp => new { cp.Id, cp.Employees, cp.Name });
                Console.WriteLine("To Insert:'{0}' ", string.Join(",", sql.InsertFields.ToArray()));
                db.Select<Company>(sql);
            }
			
			Console.WriteLine("This is The End my friend!");
		}
	}
}

