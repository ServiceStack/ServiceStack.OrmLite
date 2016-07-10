using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack.DataAnnotations;
using ServiceStack.Text;

namespace ServiceStack.OrmLite.Tests
{
    public class Rockstar
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Age { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime? DateDied { get; set; }
        public LivingStatus LivingStatus { get; set; }
    }

    public class RockstarAlbum
    {
        //[AutoIncrement]
        public int Id { get; set; }
        public int RockstarId { get; set; }
        public string Name { get; set; }
    }

    public class RockstarWithAlbum
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public int RockstarId { get; set; }
        public string RockstarAlbumName { get; set; }
        public int RockstarAlbumId { get; set; }
    }

    public class RockstarAlt
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Age { get; set; }
    }

    public enum LivingStatus
    {
        Alive,
        Dead
    }

    public class DeptEmployee
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [References(typeof(Department2))]
        public int DepartmentId { get; set; }
        [Alias("Primary_Addres")]
        public string PrimaryAddress { get; set; }

        [Reference]
        public Department2 Department { get; set; }
    }

    public class Department2
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
    }


    public class AutoQueryTests : OrmLiteTestBase
    {
        public static Rockstar[] SeedRockstars = new[] {
            new Rockstar { Id = 1, FirstName = "Jimi", LastName = "Hendrix", LivingStatus = LivingStatus.Dead, Age = 27, DateOfBirth = new DateTime(1942, 11, 27), DateDied = new DateTime(1970, 09, 18), },
            new Rockstar { Id = 2, FirstName = "Jim", LastName = "Morrison", Age = 27, LivingStatus = LivingStatus.Dead, DateOfBirth = new DateTime(1943, 12, 08), DateDied = new DateTime(1971, 07, 03),  },
            new Rockstar { Id = 3, FirstName = "Kurt", LastName = "Cobain", Age = 27, LivingStatus = LivingStatus.Dead, DateOfBirth = new DateTime(1967, 02, 20), DateDied = new DateTime(1994, 04, 05), },
            new Rockstar { Id = 4, FirstName = "Elvis", LastName = "Presley", Age = 42, LivingStatus = LivingStatus.Dead, DateOfBirth = new DateTime(1935, 01, 08), DateDied = new DateTime(1977, 08, 16), },
            new Rockstar { Id = 5, FirstName = "David", LastName = "Grohl", Age = 44, LivingStatus = LivingStatus.Alive, DateOfBirth = new DateTime(1969, 01, 14), },
            new Rockstar { Id = 6, FirstName = "Eddie", LastName = "Vedder", Age = 48, LivingStatus = LivingStatus.Alive, DateOfBirth = new DateTime(1964, 12, 23), },
            new Rockstar { Id = 7, FirstName = "Michael", LastName = "Jackson", Age = 50, LivingStatus = LivingStatus.Dead, DateOfBirth = new DateTime(1958, 08, 29), DateDied = new DateTime(2009, 06, 05), },
        };

        public static RockstarAlbum[] SeedAlbums = new[] {
            new RockstarAlbum { Id = 10, RockstarId = 1, Name = "Electric Ladyland" },
            new RockstarAlbum { Id = 11, RockstarId = 4, Name = "Nevermind" },
            new RockstarAlbum { Id = 12, RockstarId = 6, Name = "Thriller" },
        };

        private static readonly Department2[] SeedDepartments = new[]
        {
            new Department2 { Id = 10, Name = "Dept 1" },
            new Department2 { Id = 20, Name = "Dept 2" },
            new Department2 { Id = 30, Name = "Dept 3" },
        };

        public static DeptEmployee[] SeedEmployees = new[]
        {
            new DeptEmployee { Id = 1, DepartmentId = 10, FirstName = "First 1", LastName = "Last 1" },
            new DeptEmployee { Id = 2, DepartmentId = 20, FirstName = "First 2", LastName = "Last 2" },
            new DeptEmployee { Id = 3, DepartmentId = 30, FirstName = "First 3", LastName = "Last 3" },
        };

        [Test]
        public void Can_query_Rockstars()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Rockstar>();
                db.InsertAll(SeedRockstars);

                var q = db.From<Rockstar>()
                    .Where("Id < {0} AND Age = {1}", 3, 27);

                var results = db.Select(q);
                db.GetLastSql().Print();
                Assert.That(results.Count, Is.EqualTo(2));
                Assert.That(q.Params.Count, Is.EqualTo(2));

                q = db.From<Rockstar>()
                    .Where("Id < {0}", 3)
                    .Or("Age = {0}", 27);
                results = db.Select(q);
                Assert.That(results.Count, Is.EqualTo(3));
                Assert.That(q.Params.Count, Is.EqualTo(2));

                q = db.From<Rockstar>().Where("FirstName".SqlColumn() + " = {0}", "Kurt");
                results = db.Select(q);
                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(q.Params.Count, Is.EqualTo(1));
                Assert.That(results[0].LastName, Is.EqualTo("Cobain"));
            }
        }

        [Test]
        public void Can_query_Rockstars_with_ValueFormat()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Rockstar>();
                db.InsertAll(SeedRockstars);

                var q = db.From<Rockstar>()
                    .Where("FirstName".SqlColumn() + " LIKE {0}", "Jim%");

                var results = db.Select(q);
                db.GetLastSql().Print();
                Assert.That(results.Count, Is.EqualTo(2));
                Assert.That(q.Params.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_query_Rockstars_with_IN_Query()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Rockstar>();
                db.InsertAll(SeedRockstars);

                var q = db.From<Rockstar>()
                    .Where("FirstName".SqlColumn() + " IN ({0})", new SqlInValues(new[] { "Jimi", "Kurt", "Jim" }));

                var results = db.Select(q);
                db.GetLastSql().Print();
                Assert.That(results.Count, Is.EqualTo(3));
                Assert.That(q.Params.Count, Is.EqualTo(3));
            }
        }

        [Test]
        public void Does_query_Rockstars_Single_with_anon_SelectInto()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<Rockstar>();
                db.InsertAll(SeedRockstars);

                var q = db.From<Rockstar>()
                    .Where(x => x.FirstName == "Kurt")
                    .Select(x => new { x.Id, x.LastName });

                var result = db.Single<RockstarAlt>(q);
                Assert.That(result.LastName, Is.EqualTo("Cobain"));
                Assert.That(q.Params.Count, Is.EqualTo(1));

                var results = db.Select<RockstarAlt>(q);
                Assert.That(results[0].LastName, Is.EqualTo("Cobain"));
                Assert.That(q.Params.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void Can_use_dynamic_apis_on_Custom_Select()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<DeptEmployee>();
                db.DropTable<Department2>();
                db.CreateTable<Department2>();
                db.CreateTable<DeptEmployee>();

                db.InsertAll(SeedDepartments);
                db.InsertAll(SeedEmployees);

                var q = db.From<DeptEmployee>().Select(new[] {"FirstName"}).Take(2);

                var resultsMap = db.Select<Dictionary<string, object>>(q);
                Assert.That(resultsMap.Count, Is.EqualTo(2));
                var row = new Dictionary<string,object>(resultsMap[0], StringComparer.OrdinalIgnoreCase);
                Assert.That(row.ContainsKey("FirstName"));
                Assert.That(!row.ContainsKey("Id"));

                var resultsList = db.Select<List<object>>(q);
                Assert.That(resultsList.Count, Is.EqualTo(2));
                Assert.That(resultsList[0].Count, Is.EqualTo(1));

                var resultsDynamic = db.Select<dynamic>(q);
                Assert.That(resultsDynamic.Count, Is.EqualTo(2));
                var map = (IDictionary<string, object>) resultsDynamic[0];
                // something must be wrong in firebird dialect provider ( missing quotes)?
                if (Dialect != Dialect.Firebird)
                {
                    Assert.That(map.ContainsKey("FirstName"));
                    Assert.That(!map.ContainsKey("Id"));
                }
                else
                {
                    Assert.That(map.ContainsKey("FIRSTNAME"));
                    Assert.That(!map.ContainsKey("ID"));
                }
            }
        }
        
        [Test]
        public void Can_use_dynamic_apis_on_Custom_Select_with_alias_and_lambdas()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<DeptEmployee>();
                db.DropTable<Department2>();
                db.CreateTable<Department2>();
                db.CreateTable<DeptEmployee>();
                db.InsertAll(SeedDepartments);
                db.InsertAll(SeedEmployees);
                var expectedPropertyName = ((Func<Type, string>)(de =>
                {
                    var instance = de.CreateInstance<DeptEmployee>();
                    return nameof(instance.PrimaryAddress);
                    
                }))(typeof(DeptEmployee));
                var q = db.From<DeptEmployee>().Select(f=>f.PrimaryAddress).Take(2);
                var resultsMap = db.Select<Dictionary<string, object>>(q);
                var row = new Dictionary<string, object>(resultsMap[0], StringComparer.OrdinalIgnoreCase);
                Assert.That(row.ContainsKey(expectedPropertyName));
                var resultsDynamic = db.Select<dynamic>(q);
                var map = (IDictionary<string, object>)resultsDynamic[0];
                Assert.That(map.ContainsKey(expectedPropertyName));
                q = db.From<DeptEmployee>().Select(f => Sql.As(f.PrimaryAddress, f.PrimaryAddress)).Take(2);
                
                resultsMap = db.Select<Dictionary<string, object>>(q);
                row = new Dictionary<string, object>(resultsMap[0], StringComparer.OrdinalIgnoreCase);
                Assert.That(row.ContainsKey(expectedPropertyName));
                resultsDynamic = db.Select<dynamic>(q);
                map = (IDictionary<string, object>)resultsDynamic[0];
                Assert.That(map.ContainsKey(expectedPropertyName));
                
            }
            
        }


        [Test]
        public void Does_only_populate_Select_fields()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<DeptEmployee>();
                db.DropTable<Department2>();
                db.CreateTable<Department2>();
                db.CreateTable<DeptEmployee>();

                db.InsertAll(SeedDepartments);
                db.InsertAll(SeedEmployees);

                var q = db.From<DeptEmployee>()
                    .Join<Department2>()
                    .Select(new[] { "departmentid" });

                var results = db.Select(q);

                Assert.That(results.All(x => x.Id == 0));
                Assert.That(results.All(x => x.DepartmentId >= 10));

                q = db.From<DeptEmployee>()
                    .Join<Department2>()
                    .Select(new[] { "id", "departmentid" });

                results = db.Select(q);

                results.PrintDump();

                Assert.That(results.All(x => x.Id > 0 && x.Id < 10));
                Assert.That(results.All(x => x.DepartmentId >= 10));
            }
        }

        [Test]
        public void Does_only_populate_Select_fields_wildcard()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<DeptEmployee>();
                db.DropTable<Department2>();
                db.CreateTable<Department2>();
                db.CreateTable<DeptEmployee>();

                db.InsertAll(SeedDepartments);
                db.InsertAll(SeedEmployees);

                var q = db.From<DeptEmployee>()
                    .Join<Department2>()
                    .Select(new[] { "departmentid", "deptemployee.*" });

                Assert.That(q.OnlyFields, Is.EquivalentTo(new[] {
                    "departmentid", "Id", "FirstName", "LastName"
                }));

                var results = db.Select(q);
                Assert.That(results.All(x => x.Id >= 0));
                Assert.That(results.All(x => x.DepartmentId >= 10));
                Assert.That(results.All(x => x.FirstName != null));
                Assert.That(results.All(x => x.LastName != null));

                q = db.From<DeptEmployee>()
                    .Join<Department2>()
                    .Select(new[] { "departmentid", "department", "department2.*" });

                Assert.That(q.OnlyFields, Is.EquivalentTo(new[] {
                    "departmentid", "department", "Id", "Name"
                }));

                results = db.LoadSelect(q, include: q.OnlyFields);

                Assert.That(results.All(x => x.Id > 0 && x.Id < 10));
                Assert.That(results.All(x => x.DepartmentId >= 10));
                Assert.That(results.All(x => x.Department.Id >= 10));
                Assert.That(results.All(x => x.Department.Name != null));
            }
        }

        [Test]
        public void Does_only_populate_LoadSelect_fields()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<DeptEmployee>();
                db.DropTable<Department2>();
                db.CreateTable<Department2>();
                db.CreateTable<DeptEmployee>();

                db.InsertAll(SeedDepartments);
                db.InsertAll(SeedEmployees);

                var q = db.From<DeptEmployee>()
                    .Join<Department2>()
                    .Select(new[] { "departmentid" });

                var results = db.LoadSelect(q);
                results.PrintDump();

                Assert.That(results.All(x => x.Id == 0));
                Assert.That(results.All(x => x.DepartmentId >= 10));

                q = db.From<DeptEmployee>()
                    .Join<Department2>()
                    .Select(new[] { "id", "departmentid" });

                results = db.LoadSelect(q);
                results.PrintDump();

                Assert.That(results.All(x => x.Id > 0 && x.Id < 10));
                Assert.That(results.All(x => x.DepartmentId >= 10));
            }
        }

        [Test]
        public void Can_Select_custom_fields_using_dynamic()
        {
            using (var db = OpenDbConnection())
            {
                db.DropTable<DeptEmployee>();
                db.DropTable<Department2>();
                db.CreateTable<Department2>();
                db.CreateTable<DeptEmployee>();

                db.InsertAll(SeedDepartments);
                db.InsertAll(SeedEmployees);

                var q = db.From<DeptEmployee>()
                    .Join<Department2>()
                    .Select<DeptEmployee, Department2>(
                        (de, d2) => new { de.FirstName, de.LastName, d2.Name });
                var results = db.Select<dynamic>(q);
                db.GetLastSql().Print();

                var sb = new StringBuilder();
                foreach (var result in results)
                {
                    if (Dialect == Dialect.PostgreSql) // something wrong in PostreSql dialect?
                        sb.AppendLine(result.first_name + "," + result.last_name + "," + result.name);
                    else if (Dialect == Dialect.Firebird) // something wrong in PostreSql dialect (miissing quotes)? 
                        sb.AppendLine(result.FIRSTNAME + "," + result.LASTNAME + "," + result.NAME);
                    else
                        sb.AppendLine(result.FirstName + "," + result.LastName + "," + result.Name);
                }

                Assert.That(sb.ToString().NormalizeNewLines(), Is.EqualTo(
                    "First 1,Last 1,Dept 1\nFirst 2,Last 2,Dept 2\nFirst 3,Last 3,Dept 3\n"));

                q = db.From<DeptEmployee>()
                    .Join<Department2>()
                    .Select<Department2>(d2 => new { d2.Name });

                results = db.Select<dynamic>(q);

                sb.Length = 0;
                foreach (var result in results)
                {
                    if (Dialect == Dialect.PostgreSql)
                        sb.AppendLine(result.name);
                    else if (Dialect == Dialect.Firebird)
                        sb.AppendLine(result.NAME);
                    else
                        sb.AppendLine(result.Name);
                }

                Assert.That(sb.ToString().NormalizeNewLines(), Is.EqualTo("Dept 1\nDept 2\nDept 3\n"));
            }
        }
    }

}
