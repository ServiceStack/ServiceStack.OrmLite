using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MonoTouch.CoreFoundation;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;

namespace SqliteExpressionsTest.iOS
{
    [Register("UniversalView")]
    public class UniversalView : UIView
    {
        private UIButton runTests;

        public UniversalView()
        {
            Initialize();
        }

        public UniversalView(RectangleF bounds)
            : base(bounds)
        {
            Initialize();
        }

        void Initialize()
        {
            //BackgroundColor = UIColor.Red;
            this.runTests = UIButton.FromType(UIButtonType.RoundedRect);
            this.runTests.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleMargins;
            this.runTests.Bounds = new RectangleF(50, 50, 125, 40);
            this.runTests.SetTitle("Run tests", UIControlState.Normal);
            this.runTests.SetTitle("Tests running...", UIControlState.Disabled);

            this.AddSubview(this.runTests);

            this.runTests.TouchUpInside += async (sender, args) =>
                {
                    this.runTests.Enabled = false;
                    await this.RunAsync();
                    this.runTests.Enabled = true;
                };
        }

        /// <summary>
        /// Run tests asyncronously.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task RunAsync()
        {
            await Task.Factory.StartNew(RunAuthorTests);
        }

        /// <summary>
        /// The run author tests.
        /// </summary>
        private static void RunAuthorTests()
        {
            //Console.WriteLine("Hello World!");

            //Console.WriteLine("Join Test");
            //JoinTest.Test();

            //Console.WriteLine("Ignored Field Select Test");
            //IgnoredFieldSelectTest.Test();

            //Console.WriteLine("Count Test");
            //CountTest.Test();

            OrmLiteConfig.DialectProvider = SqliteOrmLiteDialectProvider.Instance;
            SqlExpressionVisitor<Author> ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<Author>();

            using (IDbConnection db = GetFileConnectionString().OpenDbConnection())
            {
                db.DropTable<Author>();
                db.CreateTable<Author>();
                db.DeleteAll<Author>();

                var authors = new List<Author>();
                authors.Add(new Author() { Name = "Demis Bellot", Birthday = DateTime.Today.AddYears(-20), Active = true, Earnings = 99.9m, Comments = "CSharp books", Rate = 10, City = "London" });
                authors.Add(new Author() { Name = "Angel Colmenares", Birthday = DateTime.Today.AddYears(-25), Active = true, Earnings = 50.0m, Comments = "CSharp books", Rate = 5, City = "Bogota" });
                authors.Add(new Author() { Name = "Adam Witco", Birthday = DateTime.Today.AddYears(-20), Active = true, Earnings = 80.0m, Comments = "Math Books", Rate = 9, City = "London" });
                authors.Add(new Author() { Name = "Claudia Espinel", Birthday = DateTime.Today.AddYears(-23), Active = true, Earnings = 60.0m, Comments = "Cooking books", Rate = 10, City = "Bogota" });
                authors.Add(new Author() { Name = "Libardo Pajaro", Birthday = DateTime.Today.AddYears(-25), Active = true, Earnings = 80.0m, Comments = "CSharp books", Rate = 9, City = "Bogota" });
                authors.Add(new Author() { Name = "Jorge Garzon", Birthday = DateTime.Today.AddYears(-28), Active = true, Earnings = 70.0m, Comments = "CSharp books", Rate = 9, City = "Bogota" });
                authors.Add(new Author() { Name = "Alejandro Isaza", Birthday = DateTime.Today.AddYears(-20), Active = true, Earnings = 70.0m, Comments = "Java books", Rate = 0, City = "Bogota" });
                authors.Add(new Author() { Name = "Wilmer Agamez", Birthday = DateTime.Today.AddYears(-20), Active = true, Earnings = 30.0m, Comments = "Java books", Rate = 0, City = "Cartagena" });
                authors.Add(new Author() { Name = "Rodger Contreras", Birthday = DateTime.Today.AddYears(-25), Active = true, Earnings = 90.0m, Comments = "CSharp books", Rate = 8, City = "Cartagena" });
                authors.Add(new Author() { Name = "Chuck Benedict", Birthday = DateTime.Today.AddYears(-22), Active = true, Earnings = 85.5m, Comments = "CSharp books", Rate = 8, City = "London" });
                authors.Add(new Author() { Name = "James Benedict II", Birthday = DateTime.Today.AddYears(-22), Active = true, Earnings = 85.5m, Comments = "Java books", Rate = 5, City = "Berlin" });
                authors.Add(new Author() { Name = "Ethan Brown", Birthday = DateTime.Today.AddYears(-20), Active = true, Earnings = 45.0m, Comments = "CSharp books", Rate = 5, City = "Madrid" });
                authors.Add(new Author() { Name = "Xavi Garzon", Birthday = DateTime.Today.AddYears(-22), Active = true, Earnings = 75.0m, Comments = "CSharp books", Rate = 9, City = "Madrid" });
                authors.Add(new Author() { Name = "Luis garzon", Birthday = DateTime.Today.AddYears(-22), Active = true, Earnings = 85.0m, Comments = "CSharp books", Rate = 10, City = "Mexico" });

                db.InsertAll(authors);


                // lets start !

                // select authors born 20 year ago
                int year = DateTime.Today.AddYears(-20).Year;
                int expected = 5;

                ev.Where(rn => rn.Birthday >= new DateTime(year, 1, 1) && rn.Birthday <= new DateTime(year, 12, 31));
                List<Author> result = db.Select(ev);
                Console.WriteLine(ev.WhereExpression);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
                result = db.Select<Author>(qry => qry.Where(rn => rn.Birthday >= new DateTime(year, 1, 1) && rn.Birthday <= new DateTime(year, 12, 31)));
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
                result = db.Select<Author>(rn => rn.Birthday >= new DateTime(year, 1, 1) && rn.Birthday <= new DateTime(year, 12, 31));
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);

                // select authors from London, Berlin and Madrid : 6
                expected = 6;
                ev.Where(rn => Sql.In(rn.City, new object[] { "London", "Madrid", "Berlin" }));
                result = db.Select(ev);
                Console.WriteLine(ev.WhereExpression);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
                result = db.Select<Author>(rn => Sql.In(rn.City, new[] { "London", "Madrid", "Berlin" }));
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);

                // select authors from Bogota and Cartagena : 7
                expected = 7;
                ev.Where(rn => Sql.In(rn.City, new object[] { "Bogota", "Cartagena" }));
                result = db.Select(ev);
                Console.WriteLine(ev.WhereExpression);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
                result = db.Select<Author>(rn => Sql.In(rn.City, "Bogota", "Cartagena"));
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);


                // select authors which name starts with A
                expected = 3;
                ev.Where(rn => rn.Name.StartsWith("A"));
                result = db.Select(ev);
                Console.WriteLine(ev.WhereExpression);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
                result = db.Select<Author>(rn => rn.Name.StartsWith("A"));
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);

                // select authors which name ends with Garzon o GARZON o garzon ( no case sensitive )
                expected = 3;
                ev.Where(rn => rn.Name.ToUpper().EndsWith("GARZON"));
                result = db.Select(ev);
                Console.WriteLine(ev.WhereExpression);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
                result = db.Select<Author>(rn => rn.Name.ToUpper().EndsWith("GARZON"));
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);

                // select authors which name ends with garzon
                //A percent symbol ("%") in the LIKE pattern matches any sequence of zero or more characters 
                //in the string. 
                //An underscore ("_") in the LIKE pattern matches any single character in the string. 
                //Any other character matches itself or its lower/upper case equivalent (i.e. case-insensitive matching).
                expected = 3;
                ev.Where(rn => rn.Name.EndsWith("garzon"));
                result = db.Select(ev);
                Console.WriteLine(ev.WhereExpression);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
                result = db.Select<Author>(rn => rn.Name.EndsWith("garzon"));
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);


                // select authors which name contains  Benedict 
                expected = 2;
                ev.Where(rn => rn.Name.Contains("Benedict"));
                result = db.Select(ev);
                Console.WriteLine(ev.WhereExpression);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
                result = db.Select<Author>(rn => rn.Name.Contains("Benedict"));
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);


                // select authors with Earnings <= 50 
                expected = 3;
                ev.Where(rn => rn.Earnings <= 50);
                result = db.Select(ev);
                Console.WriteLine(ev.WhereExpression);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
                result = db.Select<Author>(rn => rn.Earnings <= 50);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);

                // select authors with Rate = 10 and city=Mexio 
                expected = 1;
                ev.Where(rn => rn.Rate == 10 && rn.City == "Mexico");
                result = db.Select(ev);
                Console.WriteLine(ev.WhereExpression);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
                result = db.Select<Author>(rn => rn.Rate == 10 && rn.City == "Mexico");
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);

                //  enough selecting, lets update;
                // set Active=false where rate =0
                expected = 2;
                ev.Where(rn => rn.Rate == 0).Update(rn => rn.Active);
                var rows = db.UpdateOnly(new Author() { Active = false }, ev);
                Console.WriteLine(ev.WhereExpression);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, rows, expected == rows);

                // insert values  only in Id, Name, Birthday, Rate and Active fields 
                expected = 4;
				//ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<Author>();

                //ev.Insert(rn => new { rn.Id, rn.Name, rn.Birthday, rn.Active, rn.Rate });
                db.InsertOnly(new Author() { Active = false, Rate = 0, Name = "Victor Grozny", Birthday = DateTime.Today.AddYears(-18) }, ev);
                db.InsertOnly(new Author() { Active = false, Rate = 0, Name = "Ivan Chorny", Birthday = DateTime.Today.AddYears(-19) }, ev);
                ev.Where(rn => !rn.Active);
                result = db.Select(ev);
                Console.WriteLine(ev.WhereExpression);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);

                //update comment for City == null 
                expected = 2;
                ev.Where(rn => rn.City == null).Update(rn => rn.Comments);
                rows = db.UpdateOnly(new Author() { Comments = "No comments" }, ev);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, rows, expected == rows);

                // delete where City is null 
                expected = 2;
                rows = db.Delete(ev);
                Console.WriteLine(ev.WhereExpression);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, rows, expected == rows);



                //   lets select  all records ordered by Rate Descending and Name Ascending
                //expected = 14;
                //ev.Where().OrderBy(rn => new { at = Sql.Desc(rn.Rate), rn.Name }); // clear where condition
                //result = db.Select(ev);
                //Console.WriteLine(ev.WhereExpression);
                //Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);
                //Console.WriteLine(ev.OrderByExpression);
                //var author = result.FirstOrDefault();
                //Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Claudia Espinel", author.Name, "Claudia Espinel" == author.Name);

                // select  only first 5 rows ....

				ev = OrmLiteConfig.DialectProvider.ExpressionVisitor<Author>();
                expected = 5;
                ev.Limit(5); // note: order is the same as in the last sentence
                result = db.Select(ev);
                Console.WriteLine(ev.WhereExpression);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);


                // and finally lets select only Name and City (name will be "UPPERCASED" )

                //ev.Select(rn => new { at = Sql.As(rn.Name.ToUpper(), "Name"), rn.City });
                //Console.WriteLine(ev.SelectExpression);
                //result = db.Select(ev);
                //var author = result.FirstOrDefault();
                //Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Claudia Espinel".ToUpper(), author.Name, "Claudia Espinel".ToUpper() == author.Name);

                //paging :
                ev.Limit(0, 4);// first page, page size=4;
                result = db.Select(ev);
                var author = result.FirstOrDefault();
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Claudia Espinel".ToUpper(), author.Name, "Claudia Espinel".ToUpper() == author.Name);

                ev.Limit(4, 4);// second page
                result = db.Select(ev);
                author = result.FirstOrDefault();
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Jorge Garzon".ToUpper(), author.Name, "Jorge Garzon".ToUpper() == author.Name);

                ev.Limit(8, 4);// third page
                result = db.Select(ev);
                author = result.FirstOrDefault();
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Rodger Contreras".ToUpper(), author.Name, "Rodger Contreras".ToUpper() == author.Name);

                // select distinct..
                ev.Limit(); // clear limit
                ev.SelectDistinct(r => r.City);
                expected = 6;
                result = db.Select(ev);
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", expected, result.Count, expected == result.Count);


                Console.WriteLine();
                // Tests for predicate overloads that make use of the expression visitor
                Console.WriteLine("First author by name (exists)");
                author = db.First<Author>(a => a.Name == "Jorge Garzon");
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Jorge Garzon", author.Name, "Jorge Garzon" == author.Name);

                try
                {
                    Console.WriteLine("First author by name (does not exist)");
                    author = db.First<Author>(a => a.Name == "Does not exist");

                    Console.WriteLine("Expected exception thrown, OK? False");
                }
                catch
                {
                    Console.WriteLine("Expected exception thrown, OK? True");
                }

                Console.WriteLine("First author or default (does not exist)");
                author = db.FirstOrDefault<Author>(a => a.Name == "Does not exist");
                Console.WriteLine("Expected:null ; OK? {0}", author == null);

                Console.WriteLine("First author or default by city (multiple matches)");
                author = db.FirstOrDefault<Author>(a => a.City == "Bogota");
                Console.WriteLine("Expected:{0} ; Selected:{1}, OK? {2}", "Angel Colmenares", author.Name, "Angel Colmenares" == author.Name);


                Console.ReadLine();
                Console.WriteLine("Press Enter to continue");

            }

            Console.WriteLine("This is The End my friend!");
        }

        /// <summary>
        /// The get file connection string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string GetFileConnectionString()
        {
            var connectionString = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "db.sqlite");

            //var connectionString = "~/db.sqlite".MapAbsolutePath();
            if (File.Exists(connectionString))
            {
                File.Delete(connectionString);
            }

            return connectionString;
        }
    }

    [Register("MainViewController")]
    public class MainViewController : UIViewController
    {
        public MainViewController()
        {
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            View = new UniversalView();

            base.ViewDidLoad();

        }
    }
}